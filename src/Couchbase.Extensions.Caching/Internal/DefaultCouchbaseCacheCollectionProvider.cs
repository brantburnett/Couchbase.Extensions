using System;
using System.Threading.Tasks;
using Couchbase.KeyValue;

namespace Couchbase.Extensions.Caching.Internal
{
    internal class DefaultCouchbaseCacheCollectionProvider : ICouchbaseCacheCollectionProvider
    {
        private readonly ICouchbaseCacheBucketProvider _bucketProvider;

        public DefaultCouchbaseCacheCollectionProvider(ICouchbaseCacheBucketProvider bucketProvider)
        {
            _bucketProvider = bucketProvider ?? throw new ArgumentNullException(nameof(bucketProvider));
        }

        public async ValueTask<ICouchbaseCollection> GetCollectionAsync()
        {
            var bucket = await _bucketProvider.GetBucketAsync().ConfigureAwait(false);

            return bucket.DefaultCollection();
        }
    }
}
