using System.Collections.Specialized;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Linq;
using Xunit;

namespace Couchbase.Extensions.Caching.UnitTests
{
    public class CacheServiceExtensionTests
    {
        [Fact]
        public void AddDistributedCouchbaseCache_RegistersDistributedCacheAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddDistributedCouchbaseCache("bucketName", options => { });

            // Assert
            var distributedCache = services.FirstOrDefault(desc => desc.ServiceType == typeof(IDistributedCache));

            Assert.NotNull(distributedCache);
            Assert.Equal(ServiceLifetime.Singleton, distributedCache.Lifetime);
        }

        [Fact]
        public void AddDistributedCouchbaseCache_Allows_Chaining()
        {
            var services = new ServiceCollection();

            Assert.Same(services, services.AddDistributedCouchbaseCache("bucketName", _ => { }));
        }
    }
}
