using System;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection.Internal;
using Moq;
using Xunit;

namespace Couchbase.Extensions.DependencyInjection.UnitTests.Internal
{
    public class NamedBucketProviderTests
    {
        #region ctor

        [Fact]
        public void ctor_NullBucketProvider_Exception()
        {
            // Act/Assert

            var ex = Assert.Throws<ArgumentNullException>(() => new LocalNamedBucketProvider(null, "bucket", null));

            Assert.Equal("bucketProvider", ex.ParamName);
        }

        [Fact]
        public void ctor_NullBucketName_Exception()
        {
            // Arrange

            var bucketProvider = new  Mock<IBucketProvider>();

            // Act/Assert

            var ex =
                Assert.Throws<ArgumentNullException>(
                    () => new LocalNamedBucketProvider(bucketProvider.Object, null, null));

            Assert.Equal("bucketName", ex.ParamName);
        }

        #endregion

        #region GetBucket

        [Fact]
        public void GetBucket_UsesParametersToGetBucketFromProvider()
        {
            // Arrange

            var bucket = new Mock<IBucket>();

            var bucketProvider = new Mock<IBucketProvider>();
            bucketProvider
                .Setup(m => m.GetBucket("bucket", "password"))
                .Returns(bucket.Object);

            var namedBucketProvider = new LocalNamedBucketProvider(bucketProvider.Object, "bucket", "password");

            // Act

            var result = namedBucketProvider.GetBucket();

            // Assert

            Assert.Equal(bucket.Object, result);
        }

        #endregion

        #region Helpers

        private class LocalNamedBucketProvider : NamedBucketProvider
        {
            public LocalNamedBucketProvider(IBucketProvider bucketProvider, string bucketName, string password)
                : base(bucketProvider, bucketName, password)
            {
            }
        }

        #endregion
    }
}
