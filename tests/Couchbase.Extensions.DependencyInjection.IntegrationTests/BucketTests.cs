using Couchbase.IO;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Couchbase.Extensions.DependencyInjection.IntegrationTests
{
    public class BucketTests
    {
        /******
         * Note: These tests require the travel-sample bucket
         ******/

        [Fact]
        public void ConnectAndGetBucket()
        {
            // Arrange

            var configuration = TestConfiguration.GetConfiguration();

            var services = new ServiceCollection();
            services
                .AddLogging()
                .AddCouchbase(configuration);

            var serviceProvider = services.BuildServiceProvider();

            try
            {
                // Act

                var bucketProvider = serviceProvider.GetRequiredService<IBucketProvider>();

                var bucket = bucketProvider.GetBucket("travel-sample");

                var result = bucket.Get<dynamic>("some-document");

                // Assert

                Assert.True(result.Success || result.Status == ResponseStatus.KeyNotFound);
            }
            finally
            {
                serviceProvider.GetRequiredService<ICouchbaseLifetimeService>().Close();
            }
        }

        [Fact]
        public void ConnectAndGetBucketUsingNamedBucketProvider()
        {
            // Arrange

            var configuration = TestConfiguration.GetConfiguration();

            var services = new ServiceCollection();
            services
                .AddLogging()
                .AddCouchbase(configuration)
                .AddCouchbaseBucket<ITestBucketProvider>("travel-sample");

            var serviceProvider = services.BuildServiceProvider();

            try
            {
                // Act

                var bucketProvider = serviceProvider.GetRequiredService<ITestBucketProvider>();

                var bucket = bucketProvider.GetBucket();

                var result = bucket.Get<dynamic>("some-document");

                // Assert

                Assert.True(result.Success || result.Status == ResponseStatus.KeyNotFound);
            }
            finally
            {
                serviceProvider.GetRequiredService<ICouchbaseLifetimeService>().Close();
            }
        }

        #region Helpers

        public interface ITestBucketProvider : INamedBucketProvider
        {
        }

        #endregion
    }
}
