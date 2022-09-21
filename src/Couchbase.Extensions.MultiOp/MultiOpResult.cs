namespace Couchbase.Extensions.MultiOp
{
    /// <summary>
    /// The result of an operation.
    /// </summary>
    public class MultiOpResult
    {
        /// <summary>
        /// The document key.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// The exception, if any.
        /// </summary>
        /// <remarks>
        /// Non-Couchbase exceptions are returned as errors on observables.
        /// </remarks>
        public CouchbaseException? Exception { get; }

        /// <summary>
        /// True if the operation was successful.
        /// </summary>
        public bool IsSuccessful => Exception == null;

        /// <summary>
        /// Creates a new MultiOpResult.
        /// </summary>
        /// <param name="key">The document key.</param>
        public MultiOpResult(string key) : this(key, null)
        {
        }

        /// <summary>
        /// Creates a new MultiOpResult.
        /// </summary>
        /// <param name="key">The document key.</param>
        /// <param name="exception">The exception.</param>
        public MultiOpResult(string key, CouchbaseException? exception)
        {
            Key = key;
            Exception = exception;
        }
    }
}
