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
        /// Touch  multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="expiry">New expiry.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult> Touch(this ICouchbaseCollection collection,
            IEnumerable<string> keys, TimeSpan expiry, MultiOpOptions? multiOpOptions = null) =>
            collection.Touch(keys, expiry, (TouchOptions?) null, multiOpOptions);

        /// <summary>
        /// Touch multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="expiry">New expiry.</param>
        /// <param name="optionsBuilder">Builder which configures the options to apply to each operation.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult> Touch(this ICouchbaseCollection collection,
            IEnumerable<string> keys, TimeSpan expiry, Action<TouchOptions> optionsBuilder, MultiOpOptions? multiOpOptions = null)
        {
            var options = new TouchOptions();
            optionsBuilder(options);

            return collection.Touch(keys, expiry, options, multiOpOptions);
        }

        /// <summary>
        /// Touch multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="expiry">New expiry.</param>
        /// <param name="touchOptions">Options to apply to the operation.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult> Touch(this ICouchbaseCollection collection,
            IEnumerable<string> keys, TimeSpan expiry, TouchOptions? touchOptions, MultiOpOptions? multiOpOptions = null) =>
            Multi.ObserveMulti(keys,
                key => collection.TouchAsync(key, expiry, touchOptions),
                multiOpOptions);

        /// <summary>
        /// Touch multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="expiryFactory">A factory which returns the expiry to use for each key.</param>
        /// <param name="optionsFactory">A factory which returns the options to use for each key.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult> Touch(this ICouchbaseCollection collection,
            IEnumerable<string> keys, Func<string, TimeSpan> expiryFactory, Func<string, TouchOptions?> optionsFactory,
            MultiOpOptions? multiOpOptions = null) =>
            Multi.ObserveMulti(keys,
                key => collection.TouchAsync(key, expiryFactory(key), optionsFactory(key)),
                multiOpOptions);
    }
}
