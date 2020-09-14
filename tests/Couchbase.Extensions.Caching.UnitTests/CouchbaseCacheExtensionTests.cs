using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Couchbase.Extensions.Caching.UnitTests
{
    public class CouchbaseCacheExtensionTests
    {
        private readonly ITestOutputHelper output;

        public CouchbaseCacheExtensionTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Set_WhenKeyIsNull_ThrowArgumentNullException()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            Assert.Throws<ArgumentNullException>(() => cache.Set(null, new byte[0], new DistributedCacheEntryOptions()));
        }

        [Fact]
        public async Task SetAsync_WhenKeyIsNull_ThrowArgumentNullException()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await cache.SetAsync(null, new byte[0], new DistributedCacheEntryOptions()));
        }

        [Fact]
        public void Get_WhenKeyIsNull_ThrowArgumentNullException()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            Assert.Throws<ArgumentNullException>(() => cache.Get(null, new DistributedCacheEntryOptions()));
        }

        [Fact]
        public async Task GetAsync_WhenKeyIsNull_ThrowArgumentNullException()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await cache.GetAsync(null, new DistributedCacheEntryOptions()));
        }

        [Fact]
        public void Get_Generic_WhenKeyIsNull_ThrowArgumentNullException()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            Assert.Throws<ArgumentNullException>(() => cache.Get<Poco>(null, new DistributedCacheEntryOptions()));
        }

        [Fact]
        public async Task GetAsynct_Generic_WhenKeyIsNull_ThrowArgumentNullException()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await cache.GetAsync<Poco>(null, new DistributedCacheEntryOptions()));
        }

        [Fact]
        public void Test_GetLifeTime_AbsoluteExpiration()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            var now = DateTime.UtcNow;

            var timerIncrement = 1;

            var absoluteExpiration = now.AddDays(timerIncrement);

            var result = CouchbaseCacheExtensions.GetLifetime(cache, new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = new DateTimeOffset(absoluteExpiration)
            });

            Assert.Equal(TimeSpan.FromDays(timerIncrement).Ticks, (long)Math.Round((double)result.Ticks / 1000000000) * 1000000000);
        }

        [Fact]
        public void Test_GetLifeTime_AbsoluteExpirationRelativeToNow()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            var now = DateTime.UtcNow;

            var timerIncrement = 15;

            var absoluteExpirationRelativeToNow = TimeSpan.FromMinutes(timerIncrement);

            var result = CouchbaseCacheExtensions.GetLifetime(cache, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
            });
            
            Assert.Equal(absoluteExpirationRelativeToNow.Ticks, (long)Math.Round((double)result.Ticks/1000000000)*1000000000);
        }

        [Fact]
        public void Test_GetLifeTime_SlidingExpiration()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            var slidingExpiration = TimeSpan.FromSeconds(10);
            var result = CouchbaseCacheExtensions.GetLifetime(cache, new DistributedCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration
            });

            output.WriteLine(new DateTime(result.Ticks).ToString(CultureInfo.InvariantCulture));
            Assert.Equal(new DateTime(result.Ticks), new DateTime(slidingExpiration.Ticks));
        }

        [Fact]
        public void Test_GetLifeTime_When_Options_Null_Default_SlidingExpiration_Used()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            var result = CouchbaseCacheExtensions.GetLifetime(cache);

            Assert.Equal(result, TimeSpan.FromMinutes(20));
        }

        public class Poco
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public override string ToString()
            {
                return string.Concat(Name, "-", Age);
            }
        }
    }
}
