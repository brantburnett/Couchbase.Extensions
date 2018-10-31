using System;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.IO;
using Couchbase.Logging;

namespace Couchbase.Extensions.Locks.Internal
{
    /// <inheritdoc/>
    internal sealed class CouchbaseMutex : ICouchbaseMutex
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(CouchbaseMutex));

        private readonly IBucket _bucket;
        private ulong _cas;
        private TimeSpan _expirationInterval;
        private CancellationTokenSource _cancellationTokenSource;

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string Holder { get; }

        public CouchbaseMutex(IBucket bucket, string name, string holder)
        {
            _bucket = bucket ?? throw new ArgumentNullException(nameof(bucket));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Holder = holder ?? throw new ArgumentNullException(nameof(holder));
        }

        /// <inheritdoc/>
        public async Task Renew(TimeSpan expiration)
        {
            if (expiration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(expiration), "Value must be positive.");
            }

            var key = LockDocument.GetKey(Name);
            var document = new LockDocument
            {
                Holder = Holder,
                RequestedDateTime = DateTime.UtcNow
            };

            IOperationResult<LockDocument> result;
            if (_cas == 0)
            {
                // We're creating a new lock
                _log.Debug("Requesting lock '{0}' for holder '{1}' for {2}", Name, Holder, expiration);
                result = await _bucket.InsertAsync(key, document, expiration).ConfigureAwait(false);
            }
            else
            {
                _log.Debug("Renewing lock '{0}' for holder '{1}' for {2}", Name, Holder, expiration);
                result = await _bucket.UpsertAsync(key, document, _cas, expiration).ConfigureAwait(false);
            }

            if (result.Status == ResponseStatus.DocumentMutationDetected || result.Status == ResponseStatus.KeyExists)
            {
                _log.Debug("Lock '{0}' unavailable, getting lock info", Name);

                var getResult = await _bucket.GetDocumentAsync<LockDocument>(key).ConfigureAwait(false);
                if (getResult.Status == ResponseStatus.KeyNotFound)
                {
                    // Couldn't find the lock, must have expired between Insert and Get, try one more time
                    result = await _bucket.InsertAsync(key, document, expiration).ConfigureAwait(false);
                    if (result.Status == ResponseStatus.KeyExists)
                    {
                        throw new CouchbaseLockUnavailableException(Name);
                    }

                    _log.Debug("Lock '{0}' issued to holder '{1}'", Name, Holder);
                    result.EnsureSuccess();

                    _expirationInterval = expiration;
                    _cas = result.Cas;
                    return;
                }

                getResult.EnsureSuccess();

                _log.Debug("Unable to acquire lock '{0}' for holder '{1}'", Name, Holder);

                throw new CouchbaseLockUnavailableException(Name)
                {
                    Holder = getResult.Content.Holder
                };
            }

            _log.Debug("Lock '{0}' issued to holder '{1}'", Name, Holder);
            result.EnsureSuccess();

            _expirationInterval = expiration;
            _cas = result.Cas;
        }

        /// <inheritdoc/>
        public void AutoRenew(TimeSpan renewalInterval, TimeSpan maximumLifespan)
        {
            if (renewalInterval <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(renewalInterval), "Value must be positive.");
            }
            if (maximumLifespan <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumLifespan), "Value must be positive.");
            }

            // Abort previous AutoRenew
            _cancellationTokenSource?.Cancel();

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            var startTime = DateTime.Now;
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested && (maximumLifespan - (DateTime.Now - startTime)) > TimeSpan.Zero)
                {
                    await Task.Delay(renewalInterval, token).ConfigureAwait(false);

                    var lifespanRemaining = maximumLifespan - (DateTime.Now - startTime);
                    if (!token.IsCancellationRequested && lifespanRemaining > TimeSpan.Zero)
                    {
                        try
                        {
                            var lockDuration = TimeSpan.FromTicks(
                                Math.Min(_expirationInterval.Ticks, lifespanRemaining.Ticks));

                            await Renew(lockDuration).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            _log.Error($"Error auto-renewing lock '{Name}' for holder '{Holder}'", ex);
                        }
                    }
                }
            }, token);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel(false);
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            if (_cas == 0)
            {
                // Never locked
                return;
            }

            var key = LockDocument.GetKey(Name);

            _bucket.RemoveAsync(key, _cas)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        _log.Warn($"Error releasing lock '{Name}' for holder '{Holder}'", t.Exception);
                    }
                    else if (t.Result != null)
                    {
                        var result = t.Result;
                        if (result.Status == ResponseStatus.KeyNotFound)
                        {
                            _log.Debug("Did not release lock '{0}' for holder '{1}' because it was already released.",
                                Name,
                                Holder);
                        }
                        else if (result.Status == ResponseStatus.DocumentMutationDetected)
                        {
                            _log.Debug(
                                "Did not release lock '{0}' for holder '{1}' because it was already held by another.",
                                Name,
                                Holder);
                        }
                        else if (!result.Success)
                        {
                            _log.Warn("Error releasing lock '{0}' for holder '{1}': {2}", Name, Holder,
                                result.Exception?.Message ?? result.Message);
                        }
                    }
                });
        }
    }
}
