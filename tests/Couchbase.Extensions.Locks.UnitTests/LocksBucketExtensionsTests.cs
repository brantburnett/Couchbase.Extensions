using System;
using System.Threading.Tasks;
using Couchbase.Core;
using Moq;
using Xunit;

namespace Couchbase.Extensions.Locks.UnitTests
{
    public class LocksBucketExtensionsTests
    {
        private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

        #region RequestMutexAsync

        [Fact]
        public async Task RequestMutexAsync_NullBucket_ArgumentNullException()
        {
            // Act/Assert

            await Assert.ThrowsAsync<ArgumentNullException>("bucket",
                () => LocksBucketExtensions.RequestMutexAsync(null, "name", "holder", OneSecond));
        }

        [Theory]
        [InlineData(null, "holder", "name")]
        [InlineData("", "holder", "name")]
        [InlineData("name", null, "holder")]
        [InlineData("name", "", "holder")]
        public async Task RequestMutexAsync_NullOrEmptyString_ArgumentException(string name, string holder, string paramName)
        {
            // Arrange

            var bucket = new Mock<IBucket>().Object;

            // Act/Assert

            await Assert.ThrowsAsync<ArgumentException>(paramName,
                () => bucket.RequestMutexAsync(name, holder, OneSecond));
        }

        [Theory]
        [InlineData("00:00:00")]
        [InlineData("-00:00:01")]
        [InlineData("-00:01:00")]
        public async Task RequestMutexAsync_NonPositiveExpiration_ArgumentOutOfRangeException(string expirationString)
        {
            // Arrange

            var expiration = TimeSpan.Parse(expirationString);
            var bucket = new Mock<IBucket>().Object;

            // Act/Assert

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>("expiration",
                () => bucket.RequestMutexAsync("name", "holder", expiration));
        }

        #endregion
    }
}
