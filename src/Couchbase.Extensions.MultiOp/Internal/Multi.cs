using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.KeyValue;

namespace Couchbase.Extensions.MultiOp.Internal
{
    internal static class Multi
    {
        #region ObserveMulti

        /// <summary>
        /// Execute a series of operations, indexed by a key, returning success/failure as an observable.
        /// </summary>
        /// <param name="keys">Keys to operate upon.</param>
        /// <param name="operation">Asynchronous operation to perform.</param>
        /// <param name="multiOpOptions">Options controlling parallelization.</param>
        /// <returns>An observable with the results.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult> ObserveMulti(
            IEnumerable<string> keys,
            Func<string, Task> operation,
            MultiOpOptions? multiOpOptions = null)
        {
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (keys == null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(keys));
            }
            if (operation == null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(operation));
            }
            Contract.EndContractBlock();
            // ReSharper restore ConditionIsAlwaysTrueOrFalse

            return Observable.Create<MultiOpResult>((observer, cancellationToken) =>
                    ExecuteMultiAsync(observer, keys, operation, multiOpOptions, cancellationToken))
                .Synchronize();
        }

        /// <summary>
        /// Execute a series of operations, indexed by a key/value pair, returning success/failure as an observable.
        /// </summary>
        /// <param name="items">Keys and values to operate upon.</param>
        /// <param name="operation">Asynchronous operation to perform.</param>
        /// <param name="multiOpOptions">Options controlling parallelization.</param>
        /// <returns>An observable with the results.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult> ObserveMulti<TValue>(
            IEnumerable<KeyValuePair<string, TValue>> items,
            Func<KeyValuePair<string, TValue>, Task> operation,
            MultiOpOptions? multiOpOptions = null)
        {
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (items == null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(items));
            }
            if (operation == null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(operation));
            }
            Contract.EndContractBlock();
            // ReSharper restore ConditionIsAlwaysTrueOrFalse

            return Observable.Create<MultiOpResult>((observer, cancellationToken) =>
                    ExecuteMultiAsync(observer, items, operation, multiOpOptions, cancellationToken))
                .Synchronize();
        }

        /// <summary>
        /// Execute a series of operations, indexed by a key, returning results as an observable.
        /// </summary>
        /// <param name="keys">Keys to operate upon.</param>
        /// <param name="operation">Asynchronous operation to perform.</param>
        /// <param name="multiOpOptions">Options controlling parallelization.</param>
        /// <returns>An observable with the results.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<TResult>> ObserveMulti<TResult>(
            IEnumerable<string> keys,
            Func<string, Task<TResult>> operation,
            MultiOpOptions? multiOpOptions = null)
            where TResult : IResult
        {
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (keys == null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(keys));
            }
            if (operation == null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(operation));
            }
            Contract.EndContractBlock();
            // ReSharper restore ConditionIsAlwaysTrueOrFalse

            return Observable.Create<MultiOpResult<TResult>>((observer, cancellationToken) =>
                    ExecuteMultiAsync(observer, keys, operation, multiOpOptions, cancellationToken))
                .Synchronize();
        }

        /// <summary>
        /// Execute a series of operations, indexed by a key/value pair, returning results as an observable.
        /// </summary>
        /// <param name="items">Keys and values to operate upon.</param>
        /// <param name="operation">Asynchronous operation to perform.</param>
        /// <param name="multiOpOptions">Options controlling parallelization.</param>
        /// <returns>An observable with the results.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<TResult>> ObserveMulti<TValue, TResult>(
            IEnumerable<KeyValuePair<string, TValue>> items,
            Func<KeyValuePair<string, TValue>, Task<TResult>> operation,
            MultiOpOptions? multiOpOptions = null)
            where TResult : IResult
        {
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (items == null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(items));
            }
            if (operation == null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(operation));
            }
            Contract.EndContractBlock();
            // ReSharper restore ConditionIsAlwaysTrueOrFalse

            return Observable.Create<MultiOpResult<TResult>>((observer, cancellationToken) =>
                    ExecuteMultiAsync(observer, items, operation, multiOpOptions, cancellationToken))
                .Synchronize();
        }

        #endregion

        #region ExecuteMultiAsync

        /// <summary>
        /// Execute a series of operations, indexed by a key, sending success/failure to an observer.
        /// </summary>
        /// <param name="observer">Observer to receive results.</param>
        /// <param name="keys">Keys to operate upon</param>
        /// <param name="operation">Asynchronous operation to perform.</param>
        /// <param name="multiOpOptions">Options controlling parallelization.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task indicating the completion of all operations.</returns>
        /// <remarks>
        /// Framework exceptions are propagated to the task, stopping operations. Couchbase exceptions are
        /// trapped and returned in the results.
        /// </remarks>
        public static Task ExecuteMultiAsync(
            IObserver<MultiOpResult> observer,
            IEnumerable<string> keys,
            Func<string, Task> operation,
            MultiOpOptions? multiOpOptions,
            CancellationToken cancellationToken)
        {
            multiOpOptions ??= MultiOpOptions.Default;

            var distributor = new Distributor<string>(keys, multiOpOptions);

            var batchResults = distributor.GetBatches()
                .RunFairForEach(async state =>
                {
                    var batch = (IEnumerable<string>) state!;

                    foreach (var key in batch)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        MultiOpResult result;
                        try
                        {
                            await operation(key).ConfigureAwait(false);

                            result = new MultiOpResult(key);
                        }
                        catch (CouchbaseException ex)
                        {
                            result = new MultiOpResult(key, ex);
                        }

                        observer.OnNext(result);
                    }
                });

            return Task.WhenAll(batchResults);
        }

        /// <summary>
        /// Execute a series of operations, indexed by a key/value pair, sending success/failure to an observer.
        /// </summary>
        /// <param name="observer">Observer to receive results.</param>
        /// <param name="items">Keys and values to operate upon.</param>
        /// <param name="operation">Asynchronous operation to perform.</param>
        /// <param name="multiOpOptions">Options controlling parallelization.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task indicating the completion of all operations.</returns>
        /// <remarks>
        /// Framework exceptions are propagated to the task, stopping operations. Couchbase exceptions are
        /// trapped and returned in the results.
        /// </remarks>
        public static Task ExecuteMultiAsync<TValue>(
            IObserver<MultiOpResult> observer,
            IEnumerable<KeyValuePair<string, TValue>> items,
            Func<KeyValuePair<string, TValue>, Task> operation,
            MultiOpOptions? multiOpOptions,
            CancellationToken cancellationToken)
        {
            multiOpOptions ??= MultiOpOptions.Default;

            var distributor = new Distributor<KeyValuePair<string, TValue>>(items, multiOpOptions);

            return Task.WhenAll(distributor.GetBatches()
                .RunFairForEach(async state =>
                {
                    var batch = (IEnumerable<KeyValuePair<string, TValue>>) state!;

                    foreach (var item in batch)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        MultiOpResult result;
                        try
                        {
                            await operation(item).ConfigureAwait(false);

                            result = new MultiOpResult(item.Key);
                        }
                        catch (CouchbaseException ex)
                        {
                            result = new MultiOpResult(item.Key, ex);
                        }

                        observer.OnNext(result);
                    }
                }));
        }

        /// <summary>
        /// Execute a series of operations, indexed by a key, sending results to an observer.
        /// </summary>
        /// <param name="observer">Observer to receive results.</param>
        /// <param name="keys">Keys to operate upon</param>
        /// <param name="operation">Asynchronous operation to perform.</param>
        /// <param name="multiOpOptions">Options controlling parallelization.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task indicating the completion of all operations.</returns>
        /// <remarks>
        /// Framework exceptions are propagated to the task, stopping operations. Couchbase exceptions are
        /// trapped and returned in the results.
        /// </remarks>
        public static Task ExecuteMultiAsync<TResult>(
            IObserver<MultiOpResult<TResult>> observer,
            IEnumerable<string> keys,
            Func<string, Task<TResult>> operation,
            MultiOpOptions? multiOpOptions,
            CancellationToken cancellationToken)
            where TResult : IResult
        {
            multiOpOptions ??= MultiOpOptions.Default;

            var distributor = new Distributor<string>(keys, multiOpOptions);

            var batchResults = distributor.GetBatches()
                .RunFairForEach(async state =>
                {
                    var batch = (IEnumerable<string>) state!;

                    foreach (var key in batch)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        MultiOpResult<TResult> result;
                        try
                        {
                            result = new MultiOpResult<TResult>(key,
                                await operation(key).ConfigureAwait(false));
                        }
                        catch (CouchbaseException ex)
                        {
                            result = new MultiOpResult<TResult>(key, ex);
                        }

                        observer.OnNext(result);
                    }
                });

            return Task.WhenAll(batchResults);
        }

        /// <summary>
        /// Execute a series of operations, indexed by a key/value pair, sending results to an observer.
        /// </summary>
        /// <param name="observer">Observer to receive results.</param>
        /// <param name="items">Keys and values to operate upon.</param>
        /// <param name="operation">Asynchronous operation to perform.</param>
        /// <param name="multiOpOptions">Options controlling parallelization.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task indicating the completion of all operations.</returns>
        /// <remarks>
        /// Framework exceptions are propagated to the task, stopping operations. Couchbase exceptions are
        /// trapped and returned in the results.
        /// </remarks>
        public static Task ExecuteMultiAsync<TValue, TResult>(
            IObserver<MultiOpResult<TResult>> observer,
            IEnumerable<KeyValuePair<string, TValue>> items,
            Func<KeyValuePair<string, TValue>, Task<TResult>> operation,
            MultiOpOptions? multiOpOptions,
            CancellationToken cancellationToken)
            where TResult : IResult
        {
            multiOpOptions ??= MultiOpOptions.Default;

            var distributor = new Distributor<KeyValuePair<string, TValue>>(items, multiOpOptions);

            return Task.WhenAll(distributor.GetBatches()
                .RunFairForEach(async state =>
                {
                    var batch = (IEnumerable<KeyValuePair<string, TValue>>) state!;

                    foreach (var item in batch)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        MultiOpResult<TResult> result;
                        try
                        {
                            result = new MultiOpResult<TResult>(item.Key,
                                await operation(item).ConfigureAwait(false));
                        }
                        catch (CouchbaseException ex)
                        {
                            result = new MultiOpResult<TResult>(item.Key, ex);
                        }

                        observer.OnNext(result);
                    }
                }));
        }

        #endregion
    }
}
