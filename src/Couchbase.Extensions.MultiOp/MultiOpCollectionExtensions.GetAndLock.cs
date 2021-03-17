using System;
using System.Collections.Generic;
using Couchbase.Extensions.MultiOp.Internal;
using Couchbase.KeyValue;

namespace Couchbase.Extensions.MultiOp
{
    /// <summary>
    /// Extensions to perform multiple operations in parallel on <see cref="CouchbaseCollection"/>.
    /// </summary>
    public static partial class MultiOpCollectionExtensions
    {
        /// <summary>
        /// Get and lock multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="expiry">Duration of the lock.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IGetResult>> GetAndLock(this ICouchbaseCollection collection,
            IEnumerable<string> keys, TimeSpan expiry, MultiOpOptions? multiOpOptions = null) =>
            collection.GetAndLock(keys, expiry, (GetAndLockOptions?) null, multiOpOptions);

        /// <summary>
        /// Get and lock multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="expiry">Duration of the lock.</param>
        /// <param name="optionsBuilder">Builder which configures the options to apply to each operation.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IGetResult>> GetAndLock(this ICouchbaseCollection collection,
            IEnumerable<string> keys, TimeSpan expiry, Action<GetAndLockOptions> optionsBuilder, MultiOpOptions? multiOpOptions = null)
        {
            var options = new GetAndLockOptions();
            optionsBuilder(options);

            return collection.GetAndLock(keys, expiry, options, multiOpOptions);
        }

        /// <summary>
        /// Get and lock multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="expiry">Duration of the lock.</param>
        /// <param name="getOptions">Options to apply to the operation.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IGetResult>> GetAndLock(this ICouchbaseCollection collection,
            IEnumerable<string> keys, TimeSpan expiry, GetAndLockOptions? getOptions, MultiOpOptions? multiOpOptions = null) =>
            Multi.ObserveMulti(keys,
                key => collection.GetAndLockAsync(key, expiry, getOptions),
                multiOpOptions);

        /// <summary>
        /// Get and lock multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="expiryFactory">A factory which returns the lock expiry to use for each key.</param>
        /// <param name="optionsFactory">A factory which returns the options to use for each key.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IGetResult>> GetAndLock(this ICouchbaseCollection collection,
            IEnumerable<string> keys, Func<string, TimeSpan> expiryFactory, Func<string, GetAndLockOptions?> optionsFactory,
            MultiOpOptions? multiOpOptions = null) =>
            Multi.ObserveMulti(keys,
                key => collection.GetAndLockAsync(key, expiryFactory(key), optionsFactory(key)),
                multiOpOptions);
    }
}
