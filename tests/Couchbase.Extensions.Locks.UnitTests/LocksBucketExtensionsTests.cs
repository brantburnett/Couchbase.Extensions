using System;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Extensions.Locks.Internal;
using Couchbase.KeyValue;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Couchbase.Extensions.Locks.UnitTests
{
    public class LocksBucketExtensionsTests
    {
        private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

        #region RequestMutexAsync

        [Fact]
        public async Task RequestMutexAsync_NullCollection_ArgumentNullException()
        {
            // Act/Assert

            await Assert.ThrowsAsync<ArgumentNullException>("collection",
                () => LocksCollectionExtensions.RequestMutexAsync(null, "name", "holder", OneSecond));
        }

        [Theory]
        [InlineData(null, "holder", "name")]
        [InlineData("", "holder", "name")]
        [InlineData("name", null, "holder")]
        [InlineData("name", "", "holder")]
        public async Task RequestMutexAsync_NullOrEmptyString_ArgumentException(string name, string holder, string paramName)
        {
            // Arrange

            var collection = BuildMockCollection();

            // Act/Assert

            await Assert.ThrowsAsync<ArgumentException>(paramName,
                () => collection.Object.RequestMutexAsync(name, holder, OneSecond));
        }

        [Theory]
        [InlineData("00:00:00")]
        [InlineData("-00:00:01")]
        [InlineData("-00:01:00")]
        public async Task RequestMutexAsync_NonPositiveExpiration_ArgumentOutOfRangeException(string expirationString)
        {
            // Arrange

            var expiration = TimeSpan.Parse(expirationString);
            var collection = BuildMockCollection();

            // Act/Assert

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>("expiration",
                () => collection.Object.RequestMutexAsync("name", "holder", expiration));
        }

        #endregion

        #region Helpers

        private static Mock<ICouchbaseCollection> BuildMockCollection()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(p => p.GetService(typeof(ILogger<CouchbaseMutex>)))
                .Returns(NullLogger<CouchbaseMutex>.Instance);

            var cluster = Mock.Of<ICluster>(p => p.ClusterServices == serviceProvider.Object);
            var bucket = Mock.Of<IBucket>(p => p.Cluster == cluster);
            var scope = Mock.Of<IScope>(p => p.Bucket == bucket);

            var collection = new Mock<ICouchbaseCollection>();
            collection
                .Setup(p => p.Scope)
                .Returns(scope);

            return collection;
        }

        #endregion
    }
}
