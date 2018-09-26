using System;
using System.Threading.Tasks;

namespace Couchbase.Extensions.Locks
{
    /// <summary>
    /// Represents a distributed Mutex lock controlled by Couchbase. The lock will be released
    /// when it expires or this class is disposed. Renewals may be used to continue to hold the
    /// lock past the expiration.
    /// </summary>
    public interface ICouchbaseMutex : IDisposable
    {
        /// <summary>
        /// Lock name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Lock holder.
        /// </summary>
        string Holder { get; }

        /// <summary>
        /// Attempt to renew the lock.
        /// </summary>
        /// <remarks>
        /// If the lock already expired but is still available, this will create a new lock.
        /// If auto renewal is in progress, future renewals will use the new expiration duration.
        /// </remarks>
        /// <param name="expiration">Length of time to renew the lock.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="expiration"/> is not positive.</exception>
        /// <exception cref="CouchbaseLockUnavailableException">The lock already expired and was taken.</exception>
        Task Renew(TimeSpan expiration);

        /// <summary>
        /// Enables automatic lock renewal until the lock is disposed, using the previously set expiration.
        /// </summary>
        /// <remarks>
        /// The lock is not renewed when AutoRenew is called, it waits for the first <paramref name="renewInterval"/>.
        /// If renewInterval is less than the expiration of the lock, it may fail to renew. Best practice is
        /// to set renewInterval do some value significantly smaller than the expiration.
        /// </remarks>
        /// <param name="renewInterval">How often the lock should be renewed.</param>
        /// <param name="maximumLifespan">Maximum lifespan of the lock, after which automatic renewals cease.</param>
        void AutoRenew(TimeSpan renewInterval, TimeSpan maximumLifespan);
    }
}
