using System.Linq;
using Couchbase.Extensions.Caching.IntegrationTests.Infrastructure;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Couchbase.Extensions.Caching.IntegrationTests
{
    public class CacheServiceExtensionTests
    {
        [Fact(Skip = "Temporarily skip.")]
        public void AddDistributedCouchbaseCache_ReplacesPreviouslyUserRegisteredServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped(typeof(IDistributedCache), sp => new Mock<IDistributedCache>());

            var config = TestConfiguration.GetConfiguration();
            services.AddCouchbase(config);

            // Act
            services.AddDistributedCouchbaseCache("default", options => { });

            // Assert
            var serviceProvider = services.BuildServiceProvider();

            var distributedCache = services.FirstOrDefault(desc => desc.ServiceType == typeof(IDistributedCache));

            Assert.NotNull(distributedCache);
            Assert.Equal(ServiceLifetime.Scoped, distributedCache.Lifetime);
            Assert.IsAssignableFrom<CouchbaseCache>(serviceProvider.GetRequiredService<IDistributedCache>());
        }
    }
}
