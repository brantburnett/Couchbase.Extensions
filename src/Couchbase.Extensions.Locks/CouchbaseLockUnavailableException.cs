using System;

namespace Couchbase.Extensions.Locks
{
    /// <summary>
    /// Indicates that a lock could not be obtained.
    /// </summary>
    public class CouchbaseLockUnavailableException : Exception
    {
        /// <summary>
        /// Name of the lock.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Requested lock holder.
        /// </summary>
        public string Holder { get; set; }

        /// <summary>
        /// Creates a new CouchbaseLockUnavailableException.
        /// </summary>
        /// <param name="name">Name of the lock.</param>
        public CouchbaseLockUnavailableException(string name)
            : base($"Lock '{name}' is currently unavailable.")
        {
        }
    }
}
