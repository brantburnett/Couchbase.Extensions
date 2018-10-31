using System;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Extensions.Locks.Internal;

namespace Couchbase.Extensions.Locks
{
    public static class LocksBucketExtensions
    {
        /// <summary>
        /// Request a distributed Couchbase Mutex that expires after <paramref name="expiration"/>.
        /// </summary>
        /// <remarks>
        /// Uses the default holder from <see cref="LockHolder"/>.
        ///
        /// The <see cref="ICouchbaseMutex"/> should be disposed once the lock is no longer needed.
        /// </remarks>
        /// <param name="bucket">Couchbase bucket.</param>
        /// <param name="name">Name of the lock.</param>
        /// <param name="expiration">Time until mutex expires, if not renewed.</param>
        /// <returns>The <see cref="ICouchbaseMutex"/>.</returns>
        /// <exception cref="CouchbaseLockUnavailableException">Thrown if the lock could not be acquired.</exception>
        /// <exception cref="CouchbaseResponseException">Thrown on general Couchbase communication errors.</exception>
        /// <exception cref="ArgumentNullException">Thrown if bucket is null.</exception>
        /// <exception cref="ArgumentException">Thrown for invalid name.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown for non-positive expiration.</exception>
        public static Task<ICouchbaseMutex> RequestMutexAsync(this IBucket bucket, string name, TimeSpan expiration)
        {
            return bucket.RequestMutexAsync(name, LockHolder.Default, expiration);
        }

        /// <summary>
        /// Request a distributed Couchbase Mutex that expires after <paramref name="expiration"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="ICouchbaseMutex"/> should be disposed once the lock is no longer needed.
        /// </remarks>
        /// <param name="bucket">Couchbase bucket.</param>
        /// <param name="name">Name of the lock.</param>
        /// <param name="holder">Useful identifier for who is holding the lock.</param>
        /// <param name="expiration">Time until mutex expires, if not renewed.</param>
        /// <returns>The <see cref="ICouchbaseMutex"/>.</returns>
        /// <exception cref="CouchbaseLockUnavailableException">Thrown if the lock could not be acquired.</exception>
        /// <exception cref="CouchbaseResponseException">Thrown on general Couchbase communication errors.</exception>
        /// <exception cref="ArgumentNullException">Thrown if bucket is null.</exception>
        /// <exception cref="ArgumentException">Thrown for invalid name or holder.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown for non-positive expiration.</exception>
        public static async Task<ICouchbaseMutex> RequestMutexAsync(this IBucket bucket, string name, string holder, TimeSpan expiration)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException(nameof(bucket));
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            }
            if (string.IsNullOrEmpty(holder))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(holder));
            }
            if (expiration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(expiration), "Value must be positive.");
            }

            var lockObj = new CouchbaseMutex(bucket, name, holder);

            // This will throw if it fails to create the initial lock
            await lockObj.Renew(expiration);

            return lockObj;
        }
    }
}
