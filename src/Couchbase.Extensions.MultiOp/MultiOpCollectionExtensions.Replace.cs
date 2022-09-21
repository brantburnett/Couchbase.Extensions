using System;
using System.Collections.Generic;
using Couchbase.Extensions.MultiOp.Internal;
using Couchbase.KeyValue;

namespace Couchbase.Extensions.MultiOp
{
    /// <summary>
    /// Extensions to mutate multiple documents in parallel on <see cref="CouchbaseCollection"/>.
    /// </summary>
    public static partial class MultiOpCollectionExtensions
    {
        /// <summary>
        /// Replace multiple documents.
        /// </summary>
        /// <typeparam name="T">Document model.</typeparam>
        /// <param name="collection">Collection to use.</param>
        /// <param name="items">Items to upsert.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IMutationResult>> Replace<T>(this ICouchbaseCollection collection,
            IEnumerable<KeyValuePair<string, T>> items, MultiOpOptions? multiOpOptions = null) =>
            collection.Replace(items, (ReplaceOptions?) null, multiOpOptions);

        /// <summary>
        /// Replace multiple documents.
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
        public static IObservable<MultiOpResult<IMutationResult>> Replace<T>(this ICouchbaseCollection collection,
            IEnumerable<KeyValuePair<string, T>> items, Action<ReplaceOptions> optionsBuilder, MultiOpOptions? multiOpOptions = null)
        {
            var options = new ReplaceOptions();
            optionsBuilder(options);

            return collection.Replace(items, options, multiOpOptions);
        }

        /// <summary>
        /// Replace multiple documents.
        /// </summary>
        /// <typeparam name="T">Document model.</typeparam>
        /// <param name="collection">Collection to use.</param>
        /// <param name="items">Items to upsert.</param>
        /// <param name="replaceOptions">Options to apply to the operation.</param>
        /// <param name="multiOpOptions">Options to control parallelization.</param>
        /// <returns>An observable which receives results as they arrive.</returns>
        /// <remarks>
        /// Operations are not executed until the observable is subscribed.
        /// </remarks>
        public static IObservable<MultiOpResult<IMutationResult>> Replace<T>(
            this ICouchbaseCollection collection,
            IEnumerable<KeyValuePair<string, T>> items, ReplaceOptions? replaceOptions, MultiOpOptions? multiOpOptions = null) =>
            Multi.ObserveMulti(items,
                item => collection.ReplaceAsync(item.Key, item.Value, replaceOptions),
                multiOpOptions);

        /// <summary>
        /// Replace multiple documents.
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
        public static IObservable<MultiOpResult<IMutationResult>> Replace<T>(this ICouchbaseCollection collection,
            IEnumerable<KeyValuePair<string, T>> items, Func<string, ReplaceOptions?> optionsFactory,
            MultiOpOptions? multiOpOptions = null) =>
            Multi.ObserveMulti(items,
                item => collection.ReplaceAsync(item.Key, item.Value, optionsFactory(item.Key)),
                multiOpOptions);
    }
}
