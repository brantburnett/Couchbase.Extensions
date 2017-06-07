using System.Collections.Specialized;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System.Linq;
using Couchbase.Configuration.Client;
using Couchbase.Core;

namespace Couchbase.Extensions.Caching.UnitTests
{
    [TestFixture]
    public class CacheServiceExtensionTests
    {
        [Test]
        public void AddDistributedCouchbaseCache_RegistersDistributedCacheAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddDistributedCouchbaseCache(options => { });

            // Assert
            var distributedCache = services.FirstOrDefault(desc => desc.ServiceType == typeof(IDistributedCache));

            Assert.NotNull(distributedCache);
            Assert.AreEqual(ServiceLifetime.Singleton, distributedCache.Lifetime);
        }

        [Test]
        public void AddDistributedCouchbaseCache_ReplacesPreviouslyUserRegisteredServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped(typeof(IDistributedCache), sp => new Mock<IDistributedCache>());
            var bucket = new Mock<IBucket>();

            // Act
            services.AddDistributedCouchbaseCache(options => {
                options.BucketName = "default";
                options.Bucket = bucket.Object;
            });

            // Assert
            var serviceProvider = services.BuildServiceProvider();

            var distributedCache = services.FirstOrDefault(desc => desc.ServiceType == typeof(IDistributedCache));

            Assert.NotNull(distributedCache);
            Assert.AreEqual(ServiceLifetime.Scoped, distributedCache.Lifetime);
            Assert.IsInstanceOf<CouchbaseCache>(serviceProvider.GetRequiredService<IDistributedCache>());
        }

        [Test]
        public void AddDistributedCouchbaseCache_Allows_Chaining()
        {
            var services = new ServiceCollection();

            Assert.AreSame(services, services.AddDistributedCouchbaseCache(_ => { }));
        }
    }
}
