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
        /// Check for the existence of multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IExistsResult>> Exists(this ICouchbaseCollection collection,
            IEnumerable<string> keys, MultiOpOptions? multiOpOptions = null) =>
            collection.Exists(keys, (ExistsOptions?) null, multiOpOptions);

        /// <summary>
        /// Check for the existence of multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="optionsBuilder">Builder which configures the options to apply to each operation.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IExistsResult>> Exists(this ICouchbaseCollection collection,
            IEnumerable<string> keys, Action<ExistsOptions> optionsBuilder, MultiOpOptions? multiOpOptions = null)
        {
            var options = new ExistsOptions();
            optionsBuilder(options);

            return collection.Exists(keys, options, multiOpOptions);
        }

        /// <summary>
        /// Check for the existence of multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="existsOptions">Options to apply to the operation.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IExistsResult>> Exists(this ICouchbaseCollection collection,
            IEnumerable<string> keys, ExistsOptions? existsOptions, MultiOpOptions? multiOpOptions = null) =>
            Multi.ObserveMulti(keys,
                key => collection.ExistsAsync(key, existsOptions),
                multiOpOptions);

        /// <summary>
        /// Check for the existence of multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="optionsFactory">A factory which returns the options to use for each key.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IExistsResult>> Exists(this ICouchbaseCollection collection,
            IEnumerable<string> keys, Func<string, ExistsOptions?> optionsFactory,
            MultiOpOptions? multiOpOptions = null) =>
            Multi.ObserveMulti(keys,
                key => collection.ExistsAsync(key, optionsFactory(key)),
                multiOpOptions);
    }
}
