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
        /// Remove multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to remove.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult> Remove(this ICouchbaseCollection collection,
            IEnumerable<string> keys, MultiOpOptions? multiOpOptions = null) =>
            collection.Remove(keys, (RemoveOptions?) null, multiOpOptions);

        /// <summary>
        /// Remove multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to remove.</param>
        /// <param name="optionsBuilder">Builder which configures the options to apply to each operation.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult> Remove(this ICouchbaseCollection collection,
            IEnumerable<string> keys, Action<RemoveOptions> optionsBuilder, MultiOpOptions? multiOpOptions = null)
        {
            var options = new RemoveOptions();
            optionsBuilder(options);

            return collection.Remove(keys, options, multiOpOptions);
        }

        /// <summary>
        /// Remove multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to remove.</param>
        /// <param name="removeOptions">Options to apply to the operation.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult> Remove(this ICouchbaseCollection collection,
            IEnumerable<string> keys, RemoveOptions? removeOptions, MultiOpOptions? multiOpOptions = null) =>
            Multi.ObserveMulti(keys,
                key => collection.RemoveAsync(key, removeOptions),
                multiOpOptions);

        /// <summary>
        /// Remove multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to remove.</param>
        /// <param name="optionsFactory">A factory which returns the options to use for each key.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult> Remove(this ICouchbaseCollection collection,
            IEnumerable<string> keys, Func<string, RemoveOptions?> optionsFactory,
            MultiOpOptions? multiOpOptions = null) =>
            Multi.ObserveMulti(keys,
                key => collection.RemoveAsync(key, optionsFactory(key)),
                multiOpOptions);
    }
}
