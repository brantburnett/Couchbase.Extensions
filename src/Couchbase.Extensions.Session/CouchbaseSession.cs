using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Core.IO.Transcoders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Couchbase.Extensions.Session
{
    public class CouchbaseSession :  ISession
    {
        private const int IdByteCount = 16;
        private const int KeyLengthLimit = ushort.MaxValue;

        private readonly IDistributedCache _cache;
        private readonly string _sessionKey;
        private readonly TimeSpan _idleTimeout;
        private readonly TimeSpan _ioTimeout;
        private readonly Func<bool> _tryEstablishSession;
        private readonly ILogger _logger;
        private bool _isModified;
        private bool _loaded;
        private bool _isAvailable;
        private bool _isNewSessionKey;
        private string _sessionId;
        private byte[] _sessionIdBytes;
        public ITypeTranscoder Transcoder { get; set; } = new LegacyTranscoder();

        public CouchbaseSession(IDistributedCache cache, string sessionKey, TimeSpan idleTimeout, TimeSpan ioTimeout,
            Func<bool> tryEstablishSession, ILoggerFactory loggerFactory, bool isNewSessionKey)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _sessionKey = string.IsNullOrEmpty(sessionKey) ? throw new ArgumentException(nameof(sessionKey)) : sessionKey;
            _tryEstablishSession = tryEstablishSession ?? throw new ArgumentNullException(nameof(tryEstablishSession));
            _logger = loggerFactory == null ? throw new ArgumentNullException(nameof(loggerFactory)) : loggerFactory.CreateLogger<CouchbaseSession>();
            Store = new Dictionary<string, byte[]>();
            _idleTimeout = idleTimeout;
            _ioTimeout = ioTimeout;
            _isNewSessionKey = isNewSessionKey;
        }

        /// <inheritdoc />
        public bool IsAvailable
        {
            get
            {
                Load();
                return _isAvailable;
            }
        }

        /// <inheritdoc />
        public string Id
        {
            get
            {
                Load();
                return _sessionId ??= new Guid(IdBytes).ToString();
            }
        }

        private byte[] IdBytes
        {
            get
            {
                if (IsAvailable && _sessionIdBytes == null)
                {
                    _sessionIdBytes = new byte[IdByteCount];
                    RandomNumberGenerator.Fill(_sessionIdBytes);
                }
                return _sessionIdBytes;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> Keys
        {
            get
            {
                Load();
                return Store.Keys;
            }
        }

        private IDictionary<string, byte[]> Store { get; set; }

        /// <inheritdoc />
        public bool TryGetValue(string key, out byte[] value)
        {
            Load();
            return Store.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public void Set(string key, byte[] value)
        {
            value = value ?? throw new ArgumentNullException(nameof(value));

            if (IsAvailable)
            {
                if (key.Length > KeyLengthLimit)
                {
                    throw new ArgumentOutOfRangeException(nameof(key), key.Length,
                        $"The key cannot be longer than '{KeyLengthLimit}' when encoded with UTF-8.");
                }

                if (!_tryEstablishSession())
                {
                   throw new InvalidOperationException("The session cannot be established after the response has started.");
                }
                _isModified = true;
                byte[] copy = new byte[value.Length];
                Buffer.BlockCopy(src: value, srcOffset: 0, dst: copy, dstOffset: 0, count: value.Length);
                Store[key] = copy;
            }
        }

        /// <inheritdoc />
        public void Remove(string key)
        {
            Load();
            _isModified |= Store.Remove(key);
        }

        /// <inheritdoc />
        public void Clear()
        {
            Load();
            _isModified |= Store.Count > 0;
            Store.Clear();
        }

        private void Load()
        {
            if (!_loaded)
            {
                try
                {
                    var data = _cache.Get(_sessionKey);
                    if (data != null)
                    {
                        Deserialize(data);
                    }
                    else if (!_isNewSessionKey)
                    {
                        _logger.LogInformation("Accessing expired session, Key:{sessionKey}", _sessionKey);
                    }
                    _isAvailable = true;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Session cache read exception, Key:{sessionKey}", _sessionKey);
                    _isAvailable = false;
                    _sessionId = string.Empty;
                    _sessionIdBytes = null;
                }
                finally
                {
                    _loaded = true;
                }
            }
        }

        /// <inheritdoc />
        public async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            // This will throw if called directly and a failure occurs. The user is expected to handle the failures.
            if (!_loaded)
            {
                using (var timeout = new CancellationTokenSource(_ioTimeout))
                {
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
                    try
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        var data = await _cache.GetAsync(_sessionKey, cts.Token);
                        if (data != null)
                        {
                            Deserialize(data);
                        }
                        else if (!_isNewSessionKey)
                        {
                            _logger.LogInformation("Accessing expired session, Key:{sessionKey}", _sessionKey);
                        }
                    }
                    catch (OperationCanceledException oex)
                    {
                        if (timeout.Token.IsCancellationRequested)
                        {
                            _logger.LogWarning("Loading the session timed out.");
                            throw new OperationCanceledException("Timed out loading the session.", oex, timeout.Token);
                        }
                        throw;
                    }
                }
                _isAvailable = true;
                _loaded = true;
            }
        }

        /// <inheritdoc />
        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (!IsAvailable)
            {
                _logger.LogInformation("Session cannot be committed since it is unavailable.");
                return;
            }

            using var timeout = new CancellationTokenSource(_ioTimeout);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
            if (_isModified)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    // This operation is only so we can log if the session already existed.
                    // Log and ignore failures.
                    try
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        var data = await _cache.GetAsync(_sessionKey, cts.Token);
                        if (data == null)
                        {
                            _logger.LogInformation("Session started; Key: { sessionKey}, Id: { sessionId}", _sessionKey, Id);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "Session cache read exception, Key:{sessionKey}", _sessionKey);
                    }
                }

                var bytes = Serialize(Store);

                try
                {
                    cts.Token.ThrowIfCancellationRequested();
                    await _cache.SetAsync(
                        _sessionKey,
                        bytes,
                        new DistributedCacheEntryOptions().SetSlidingExpiration(_idleTimeout),
                        cts.Token);
                    _isModified = false;

                    _logger.LogDebug("Session stored; Key:{sessionKey}, Id:{sessionId}, Count:{count}", _sessionKey, Id, Store.Count);
                }
                catch (OperationCanceledException oex)
                {
                    if (timeout.Token.IsCancellationRequested)
                    {
                        _logger.LogWarning("Committing the session timed out.");
                        throw new OperationCanceledException("Timed out committing the session.", oex, timeout.Token);
                    }
                    throw;
                }
            }
            else
            {
                try
                {
                    await _cache.RefreshAsync(_sessionKey, cts.Token);
                }
                catch (OperationCanceledException oex)
                {
                    if (timeout.Token.IsCancellationRequested)
                    {
                        _logger.LogWarning("Refreshing the session timed out.");
                        throw new OperationCanceledException("Timed out refreshing the session.", oex, timeout.Token);
                    }
                    throw;
                }
            }
        }

        private void Deserialize(byte[] bytes)
        {
            var jsonStr = Encoding.UTF8.GetString(bytes);
            if (!string.IsNullOrWhiteSpace(jsonStr))
            {
                Store = JsonConvert.DeserializeObject<IDictionary<string, byte[]>>(jsonStr);
            }
        }

        internal byte[] Serialize(object value)
        {
            //if already a byte array just return it
            if (value.GetType() == typeof(byte[]))
            {
                return (byte[])value;
            }

            //if not convert it to a JSON byte array
            using var ms = new MemoryStream();
            using (var sw = new StreamWriter(ms))
            {
                using var jr = new JsonTextWriter(sw);
                var serializer = JsonSerializer.Create();
                serializer.Serialize(jr, value);
            }
            return ms.ToArray();
        }
    }
}
