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
        /// Get a portion of multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="specBuilder">Builder to define the lookups for each document.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<ILookupInResult>> LookupIn(this ICouchbaseCollection collection,
            IEnumerable<string> keys,
            Action<string, LookupInSpecBuilder> specBuilder,
            MultiOpOptions? multiOpOptions = null) =>
            collection.LookupIn(keys, specBuilder, (LookupInOptions?) null, multiOpOptions);

        /// <summary>
        /// Get a portion of multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="specBuilder">Builder to define the lookups for each document.</param>
        /// <param name="optionsBuilder">Builder which configures the options to apply to each operation.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<ILookupInResult>> LookupIn(this ICouchbaseCollection collection,
            IEnumerable<string> keys,
            Action<string, LookupInSpecBuilder> specBuilder,
            Action<LookupInOptions> optionsBuilder,
            MultiOpOptions? multiOpOptions = null)
        {
            var options = new LookupInOptions();
            optionsBuilder(options);

            return collection.LookupIn(keys, specBuilder, options, multiOpOptions);
        }

        /// <summary>
        /// Get a portion of multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="specBuilder">Builder to define the lookups for each document.</param>
        /// <param name="lookupInOptions">Options to apply to the operation.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<ILookupInResult>> LookupIn(this ICouchbaseCollection collection,
            IEnumerable<string> keys,
            Action<string, LookupInSpecBuilder> specBuilder,
            LookupInOptions? lookupInOptions,
            MultiOpOptions? multiOpOptions = null) =>
            Multi.ObserveMulti(keys,
                key => collection.LookupInAsync(key, builder => specBuilder(key, builder), lookupInOptions),
                multiOpOptions);

        /// <summary>
        /// Get a portion of multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="specBuilder">Builder to define the lookups for each document.</param>
        /// <param name="optionsFactory">A factory which returns the options to use for each key.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<ILookupInResult>> LookupIn(this ICouchbaseCollection collection,
            IEnumerable<string> keys,
            Action<string, LookupInSpecBuilder> specBuilder,
            Func<string, LookupInOptions?> optionsFactory,
            MultiOpOptions? multiOpOptions = null) =>
            Multi.ObserveMulti(keys,
                key => collection.LookupInAsync(key, builder => specBuilder(key, builder), optionsFactory(key)),
                multiOpOptions);
    }
}
