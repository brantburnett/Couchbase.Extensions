using Couchbase.KeyValue;

namespace Couchbase.Extensions.MultiOp
{
    /// <summary>
    /// The result of an operation with a return value.
    /// </summary>
    /// <typeparam name="T">The type of return value.</typeparam>
    public sealed class MultiOpResult<T> : MultiOpResult
        where T : IResult
    {
        /// <summary>
        /// The returned value, if successful.
        /// </summary>
        public T? Result { get; }

        /// <summary>
        /// Creates a new MultiOpResult.
        /// </summary>
        /// <param name="key">The document key.</param>
        /// <param name="result">The returned value.</param>
        public MultiOpResult(string key, T result) : base(key)
        {
            Result = result;
        }

        /// <summary>
        /// Creates a new MultiOpResult.
        /// </summary>
        /// <param name="key">The document key.</param>
        /// <param name="exception">The exception.</param>
        public MultiOpResult(string key, CouchbaseException exception) : base(key, exception)
        {
        }
    }
}
