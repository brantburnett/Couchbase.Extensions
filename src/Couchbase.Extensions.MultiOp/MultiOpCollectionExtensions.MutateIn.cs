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
        /// Mutate a portion of multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="specBuilder">Builder to define the mutations for each document.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IMutateInResult>> MutateIn(this ICouchbaseCollection collection,
            IEnumerable<string> keys,
            Action<string, MutateInSpecBuilder> specBuilder,
            MultiOpOptions? multiOpOptions = null) =>
            collection.MutateIn(keys, specBuilder, (MutateInOptions?) null, multiOpOptions);

        /// <summary>
        /// Mutate a portion of multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="specBuilder">Builder to define the mutations for each document.</param>
        /// <param name="optionsBuilder">Builder which configures the options to apply to each operation.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IMutateInResult>> MutateIn(this ICouchbaseCollection collection,
            IEnumerable<string> keys,
            Action<string, MutateInSpecBuilder> specBuilder,
            Action<MutateInOptions> optionsBuilder,
            MultiOpOptions? multiOpOptions = null)
        {
            var options = new MutateInOptions();
            optionsBuilder(options);

            return collection.MutateIn(keys, specBuilder, options, multiOpOptions);
        }

        /// <summary>
        /// Mutate a portion of multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="specBuilder">Builder to define the mutations for each document.</param>
        /// <param name="mutateInOptions">Options to apply to the operation.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IMutateInResult>> MutateIn(this ICouchbaseCollection collection,
            IEnumerable<string> keys,
            Action<string, MutateInSpecBuilder> specBuilder,
            MutateInOptions? mutateInOptions,
            MultiOpOptions? multiOpOptions = null) =>
            Multi.ObserveMulti(keys,
                key => collection.MutateInAsync(key, builder => specBuilder(key, builder), mutateInOptions),
                multiOpOptions);

        /// <summary>
        /// Mutate a portion of multiple documents.
        /// </summary>
        /// <param name="collection">Collection to use.</param>
        /// <param name="keys">Keys to get.</param>
        /// <param name="specBuilder">Builder to define the mutations for each document.</param>
        /// <param name="optionsFactory">A factory which returns the options to use for each key.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IMutateInResult>> MutateIn(this ICouchbaseCollection collection,
            IEnumerable<string> keys,
            Action<string, MutateInSpecBuilder> specBuilder,
            Func<string, MutateInOptions?> optionsFactory,
            MultiOpOptions? multiOpOptions = null) =>
            Multi.ObserveMulti(keys,
                key => collection.MutateInAsync(key, builder => specBuilder(key, builder), optionsFactory(key)),
                multiOpOptions);
    }
}
