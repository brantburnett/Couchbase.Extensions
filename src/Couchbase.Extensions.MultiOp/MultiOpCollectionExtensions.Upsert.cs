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
        /// Upsert multiple documents.
        /// </summary>
        /// <typeparam name="T">Document model.</typeparam>
        /// <param name="collection">Collection to use.</param>
        /// <param name="items">Items to upsert.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IMutationResult>> Upsert<T>(this ICouchbaseCollection collection,
            IEnumerable<KeyValuePair<string, T>> items, MultiOpOptions? multiOpOptions = null) =>
            collection.Upsert(items, (UpsertOptions?) null, multiOpOptions);

        /// <summary>
        /// Upsert multiple documents.
        /// </summary>
        /// <typeparam name="T">Document model.</typeparam>
        /// <param name="collection">Collection to use.</param>
        /// <param name="items">Items to upsert.</param>
        /// <param name="optionsBuilder">Builder which configures the options to apply to each operation.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IMutationResult>> Upsert<T>(this ICouchbaseCollection collection,
            IEnumerable<KeyValuePair<string, T>> items, Action<UpsertOptions> optionsBuilder, MultiOpOptions? multiOpOptions = null)
        {
            var options = new UpsertOptions();
            optionsBuilder(options);

            return collection.Upsert(items, options, multiOpOptions);
        }

        /// <summary>
        /// Upsert multiple documents.
        /// </summary>
        /// <typeparam name="T">Document model.</typeparam>
        /// <param name="collection">Collection to use.</param>
        /// <param name="items">Items to upsert.</param>
        /// <param name="upsertOptions">Options to apply to the operation.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IMutationResult>> Upsert<T>(
            this ICouchbaseCollection collection,
            IEnumerable<KeyValuePair<string, T>> items, UpsertOptions? upsertOptions, MultiOpOptions? multiOpOptions = null) =>
            Multi.ObserveMulti(items,
                item => collection.UpsertAsync(item.Key, item.Value, upsertOptions),
                multiOpOptions);

        /// <summary>
        /// Upsert multiple documents.
        /// </summary>
        /// <typeparam name="T">Document model.</typeparam>
        /// <param name="collection">Collection to use.</param>
        /// <param name="items">Items to upsert.</param>
        /// <param name="optionsFactory">A factory which returns the options to use for each key.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IMutationResult>> Upsert<T>(this ICouchbaseCollection collection,
            IEnumerable<KeyValuePair<string, T>> items, Func<string, UpsertOptions?> optionsFactory,
            MultiOpOptions? multiOpOptions = null) =>
            Multi.ObserveMulti(items,
                item => collection.UpsertAsync(item.Key, item.Value, optionsFactory(item.Key)),
                multiOpOptions);
    }
}
