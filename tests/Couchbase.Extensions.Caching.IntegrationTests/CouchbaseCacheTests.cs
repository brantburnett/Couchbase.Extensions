using System;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.Core.IO.Transcoders;
using Couchbase.KeyValue;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Couchbase.Extensions.Caching.IntegrationTests
{
    public class CouchbaseCacheTests : IClassFixture<ClusterFixture>
    {
        private readonly ClusterFixture _fixture;
        private readonly ITypeTranscoder _transcoder = new LegacyTranscoder();

        public CouchbaseCacheTests(ClusterFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Test_Set()
        {
            var cache = GetCache();

            var collection = await _fixture.GetDefaultCollectionAsync();
            var poco = new Poco {Name = "poco1", Age = 12};
            const string key = "CouchbaseCacheTests.Test_Set";

            try
            {
                await collection.RemoveAsync(key);
            }
            catch (DocumentNotFoundException)
            {
                // Ignore
            }

            cache.Set(key, GetBytes(poco), null);

            var result = await collection.GetAsync(key, new GetOptions().Transcoder(_transcoder));
            var actual = GetObject<Poco>(result.ContentAs<byte[]>());

            Assert.Equal(actual.ToString(), poco.ToString());
        }

        [Fact]
        public async Task Test_SetAsync()
        {
            var cache = GetCache();

            var poco = new Poco { Name = "poco1", Age = 12 };
            const string key = "CouchbaseCacheTests.Test_SetAsync";
            var collection = await _fixture.GetDefaultCollectionAsync();

            try
            {
                await collection.RemoveAsync(key);
            }
            catch (DocumentNotFoundException)
            {
                // Ignore
            }

            await cache.SetAsync(key, GetBytes(poco), null);

            var result = await collection.GetAsync(key, new GetOptions().Transcoder(_transcoder));
            var actual = GetObject<Poco>(result.ContentAs<byte[]>());

            Assert.Equal(actual.ToString(), poco.ToString());
        }

        [Fact]
        public async Task Test_Get()
        {
            var cache = GetCache();

            var poco = new Poco { Name = "poco1", Age = 12 };
            const string key = "CouchbaseCacheTests.Test_Get";
            var collection = await _fixture.GetDefaultCollectionAsync();

            try
            {
                await collection.RemoveAsync(key);
            }
            catch (DocumentNotFoundException)
            {
                // Ignore
            }

            await collection.InsertAsync(key, GetBytes(poco), new InsertOptions().Transcoder(_transcoder));

            var bytes = cache.Get(key);
            var actual = GetObject<Poco>(bytes);

            Assert.Equal(actual.ToString(), poco.ToString());
        }

        [Fact]
        public async Task Test_GetAsync()
        {
            var cache = GetCache();

            var poco = new Poco { Name = "poco1", Age = 12 };
            const string key = "CouchbaseCacheTests.Test_GetAsync";
            var collection = await _fixture.GetDefaultCollectionAsync();

            try
            {
                await collection.RemoveAsync(key);
            }
            catch (DocumentNotFoundException)
            {
                // Ignore
            }

            await collection.InsertAsync(key, GetBytes(poco), new InsertOptions().Transcoder(_transcoder));

            var bytes = await cache.GetAsync(key);
            var actual = GetObject<Poco>(bytes);

            Assert.Equal(actual.ToString(), poco.ToString());
        }

        [Fact]
        public async Task Test_Remove()
        {
            var cache = GetCache();

            var poco = new Poco { Name = "poco1", Age = 12 };
            const string key = "CouchbaseCacheTests.Test_Remove";
            var collection = await _fixture.GetDefaultCollectionAsync();

            try
            {
                await collection.RemoveAsync(key);
            }
            catch (DocumentNotFoundException)
            {
                // Ignore
            }

            await collection.InsertAsync(key, GetBytes(poco), new InsertOptions().Transcoder(_transcoder));

            cache.Remove(key);

            await Assert.ThrowsAsync<DocumentNotFoundException>(() => collection.GetAsync(key));
        }

        [Fact]
        public async Task Test_RemoveAsync()
        {
            var cache = GetCache();

            var poco = new Poco { Name = "poco1", Age = 12 };
            const string key = "CouchbaseCacheTests.Test_RemoveAsync";
            var collection = await _fixture.GetDefaultCollectionAsync();

            try
            {
                await collection.RemoveAsync(key);
            }
            catch (DocumentNotFoundException)
            {
                // Ignore
            }

            await collection.InsertAsync(key, GetBytes(poco), new InsertOptions().Transcoder(_transcoder));

            await cache.RemoveAsync(key);

            await Assert.ThrowsAsync<DocumentNotFoundException>(() => collection.GetAsync(key));
        }

        [Fact]
        public async Task Test_Refresh()
        {
            var cache = GetCache();

            var poco = new Poco { Name = "poco1", Age = 12 };
            const string key = "CouchbaseCacheTests.Test_Refresh";
            var collection = await _fixture.GetDefaultCollectionAsync();

            try
            {
                await collection.RemoveAsync(key);
            }
            catch (DocumentNotFoundException)
            {
                // Ignore
            }

            await collection.InsertAsync(key, GetBytes(poco),
                new InsertOptions().Transcoder(_transcoder).Expiry(new TimeSpan(0, 0, 0, 2)));

            cache.Refresh(key);

            await Task.Delay(2000);
            await collection.GetAsync(key);
        }

        [Fact]
        public async Task Test_RefreshAsync()
        {
            var cache = GetCache();

            var poco = new Poco { Name = "poco1", Age = 12 };
            const string key = "CouchbaseCacheTests.Test_RefreshAsync";
            var collection = await _fixture.GetDefaultCollectionAsync();

            try
            {
                await collection.RemoveAsync(key);
            }
            catch (DocumentNotFoundException)
            {
                // Ignore
            }

            await collection.InsertAsync(key, GetBytes(poco),
                new InsertOptions().Transcoder(_transcoder).Expiry(new TimeSpan(0, 0, 0, 2)));

            await cache.RefreshAsync(key);

            await Task.Delay(2000);
            await collection.GetAsync(key);
        }

        [Fact]
        public async Task Test_Refresh_WithTimeSpan()
        {
            var cache = GetCache(new TimeSpan(0,0,0,4));

            var poco = new Poco { Name = "poco1", Age = 12 };
            const string key = "CouchbaseCacheTests.Test_Refresh";
            var collection = await _fixture.GetDefaultCollectionAsync();

            try
            {
                await collection.RemoveAsync(key);
            }
            catch (DocumentNotFoundException)
            {
                // Ignore
            }

            await collection.InsertAsync(key, GetBytes(poco),
                new InsertOptions().Transcoder(_transcoder).Expiry(new TimeSpan(0, 0, 0, 2)));

            cache.Refresh(key);

            await Task.Delay(2000);
            await collection.GetAsync(key);
        }

        [Fact]
        public async Task Test_RefreshAsync_WithTimeSpan()
        {
            var cache = GetCache(new TimeSpan(0, 0, 0, 4));

            var poco = new Poco { Name = "poco1", Age = 12 };
            const string key = "CouchbaseCacheTests.Test_RefreshAsync_WithTimeSpan";
            var collection = await _fixture.GetDefaultCollectionAsync();

            try
            {
                await collection.RemoveAsync(key);
            }
            catch (DocumentNotFoundException)
            {
                // Ignore
            }

            var v = await collection.InsertAsync(key, GetBytes(poco),
                new InsertOptions().Transcoder(_transcoder).Expiry(new TimeSpan(0, 0, 0, 2)));

            await cache.RefreshAsync(key);

            await Task.Delay(2000);
            await collection.GetAsync(key, new GetOptions().Transcoder(_transcoder));
        }

        [Fact]
        public async Task Test_SetAsync_With_Absolute_Expires_After_2Seconds()
        {
            var cache = GetCache();
            var docKey = "Test_SetAsync_With_Absolute_Expires_After_2Seconds";
            await cache.SetAsync(docKey, "some cache value", new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2)
                });
            await Task.Delay(3000);

            var collection = await _fixture.GetDefaultCollectionAsync();
            await Assert.ThrowsAsync<DocumentNotFoundException>(() => collection.GetAsync(docKey, new GetOptions().Transcoder(_transcoder)));
        }

        [Fact]
        public async Task Test_SetAsync_With_Absolute()
        {
            var cache = GetCache();
            var docKey = "Test_SetAsync_With_Absolute";
            await cache.SetAsync(docKey, "some cache value", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2)
            });

            var collection = await _fixture.GetDefaultCollectionAsync();
            await collection.GetAsync(docKey, new GetOptions().Transcoder(_transcoder));
        }

        static byte[] GetBytes(Poco poco)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(poco));
        }

        static T GetObject<T>(byte[] bytes)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));
        }

        private CouchbaseCache GetCache(TimeSpan? timeSpan = null)
        {
            var options = new CouchbaseCacheOptions
            {
                LifeSpan = timeSpan
            };

            var provider = new Mock<ICouchbaseCacheCollectionProvider>();
            provider
                .Setup(x => x.GetCollectionAsync())
                .Returns(() => new ValueTask<ICouchbaseCollection>(_fixture.GetDefaultCollectionAsync()));

            return new CouchbaseCache(provider.Object, options);
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
