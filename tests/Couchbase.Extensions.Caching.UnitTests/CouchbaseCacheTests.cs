using System;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.KeyValue;
using Moq;
using Xunit;

namespace Couchbase.Extensions.Caching.UnitTests
{
    public class CouchbaseCacheTests
    {
        [Fact]
        public void Set_WhenKeyIsNull_ThrowArgumentNullException()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            Assert.Throws<ArgumentNullException>(() => cache.Set(null, new byte[0], null));
        }

        [Fact]
        public async Task SetAsync_WhenKeyIsNull_ThrowArgumentNullException()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await cache.SetAsync(null, new byte[0], null));
        }

        [Fact]
        public void Get_WhenKeyIsNull_ThrowArgumentNullException()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            Assert.Throws<ArgumentNullException>(() => cache.Get(null));
        }

        [Fact]
        public async Task GetAsync_WhenKeyIsNull_ThrowArgumentNullException()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await cache.GetAsync(null));
        }

        [Fact]
        public async Task GetAsync_DocumentNotFound_ReturnsNull()
        {
            var collection = new Mock<ICouchbaseCollection>();
            collection
                .Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<GetOptions>()))
                .ThrowsAsync(new DocumentNotFoundException());

            var provider = new Mock<ICouchbaseCacheCollectionProvider>();
            provider
                .Setup(m => m.GetCollectionAsync())
                .ReturnsAsync(collection.Object);

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            await cache.GetAsync("key");
        }

        [Fact]
        public void Refresh_WhenKeyIsNull_ThrowArgumentNullException()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            Assert.Throws<ArgumentNullException>(() => cache.Refresh(null));
        }

        [Fact]
        public async Task RefreshAsync_WhenKeyIsNull_ThrowArgumentNullException()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await cache.RefreshAsync(null));
        }

        [Fact]
        public void Remove_WhenKeyIsNull_ThrowArgumentNullException()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            Assert.Throws<ArgumentNullException>(() => cache.Remove(null));
        }

        [Fact]
        public async Task RemoveAsync_WhenKeyIsNull_ThrowArgumentNullException()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            await Assert.ThrowsAsync< ArgumentNullException>(async () => await cache.RemoveAsync(null));
        }

        [Fact]
        public async Task RemoveAsync_DocumentNotFound_ReturnsNull()
        {
            var collection = new Mock<ICouchbaseCollection>();
            collection
                .Setup(m => m.RemoveAsync(It.IsAny<string>(), It.IsAny<RemoveOptions>()))
                .ThrowsAsync(new DocumentNotFoundException());

            var provider = new Mock<ICouchbaseCacheCollectionProvider>();
            provider
                .Setup(m => m.GetCollectionAsync())
                .ReturnsAsync(collection.Object);

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            await cache.RemoveAsync("key");
        }

        [Fact]
        public void Set_WhenValueIsNull_ThrowArgumentNullException()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            Assert.Throws<ArgumentNullException>(() => cache.Set(null, new byte[0], null));
        }

        [Fact]
        public async Task SetAsync_WhenValueIsNull_ThrowArgumentNullException()
        {
            var provider = new Mock<ICouchbaseCacheCollectionProvider>();

            var cache = new CouchbaseCache(provider.Object, new CouchbaseCacheOptions());

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await cache.SetAsync(null, new byte[0], null));
        }
    }
}
