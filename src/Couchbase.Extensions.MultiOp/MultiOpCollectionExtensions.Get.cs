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
        /// Get multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IGetResult>> Get(this ICouchbaseCollection collection,
            IEnumerable<string> keys, MultiOpOptions? multiOpOptions = null) =>
            collection.Get(keys, (GetOptions?) null, multiOpOptions);

        /// <summary>
        /// Get multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="optionsBuilder">Builder which configures the options to apply to each operation.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IGetResult>> Get(this ICouchbaseCollection collection,
            IEnumerable<string> keys, Action<GetOptions> optionsBuilder, MultiOpOptions? multiOpOptions = null)
        {
            var options = new GetOptions();
            optionsBuilder(options);

            return collection.Get(keys, options, multiOpOptions);
        }

        /// <summary>
        /// Get multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="getOptions">Options to apply to the operation.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IGetResult>> Get(this ICouchbaseCollection collection,
            IEnumerable<string> keys, GetOptions? getOptions, MultiOpOptions? multiOpOptions = null) =>
            Multi.ObserveMulti(keys,
                key => collection.GetAsync(key, getOptions),
                multiOpOptions);

        /// <summary>
        /// Get multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="optionsFactory">A factory which returns the options to use for each key.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IGetResult>> Get(this ICouchbaseCollection collection,
            IEnumerable<string> keys, Func<string, GetOptions?> optionsFactory,
            MultiOpOptions? multiOpOptions = null) =>
            Multi.ObserveMulti(keys,
                key => collection.GetAsync(key, optionsFactory(key)),
                multiOpOptions);
    }
}
