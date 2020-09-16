using System;
using Microsoft.Extensions.Options;

namespace Couchbase.Extensions.Caching
{
    /// <inheritdoc />
    public class CouchbaseCacheOptions : IOptions<CouchbaseCacheOptions>
    {
        /// <summary>
        /// Twenty minute default for sliding expiration
        /// </summary>
        public CouchbaseCacheOptions() => LifeSpan = new TimeSpan(0, 0, 20, 0);

        /// <summary>
        /// The current <see cref="CouchbaseCacheOptions"/> instance.
        /// </summary>
        public CouchbaseCacheOptions Value => this;

        /// <summary>
        /// The global lifespan for cache items. Sliding expiration is only supported and the default is 20 minutes.
        /// </summary>
        public TimeSpan? LifeSpan { get; set; }
    }
}
