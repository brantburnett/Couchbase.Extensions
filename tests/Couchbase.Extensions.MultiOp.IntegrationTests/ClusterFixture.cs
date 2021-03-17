using System.Threading.Tasks;
using Couchbase.KeyValue;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Couchbase.Extensions.MultiOp.IntegrationTests
{
    public class ClusterFixture : IAsyncLifetime
    {
        private bool _bucketOpened;

        public ClusterOptions ClusterOptions { get; }

        public ICluster Cluster { get; private set; }

        public ClusterFixture()
        {
            ClusterOptions = GetClusterOptions();
        }

        public async ValueTask<ICluster> GetClusterAsync()
        {
            if (_bucketOpened)
            {
                return Cluster;
            }

            await GetDefaultBucketAsync().ConfigureAwait(false);
            return Cluster;
        }

        public async Task<IBucket> GetDefaultBucketAsync()
        {
            var bucket = await Cluster.BucketAsync("default").ConfigureAwait(false);

            _bucketOpened = true;

            return bucket;
        }

        public async Task<ICouchbaseCollection> GetDefaultCollectionAsync()
        {
            var bucket = await GetDefaultBucketAsync().ConfigureAwait(false);
            return bucket.DefaultCollection();
        }

        public static ClusterOptions GetClusterOptions()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .Build()
                .GetSection("couchbase")
                .Get<ClusterOptions>();
        }

        public async Task InitializeAsync()
        {
            Cluster = await Couchbase.Cluster.ConnectAsync(GetClusterOptions())
                .ConfigureAwait(false);
        }

        public Task DisposeAsync()
        {
            Cluster?.Dispose();

            return Task.CompletedTask;
        }
    }
}