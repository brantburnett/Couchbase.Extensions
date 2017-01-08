using System;
using System.Collections.Generic;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection.Internal;
using Moq;
using Xunit;

namespace Couchbase.Extensions.DependencyInjection.UnitTests.Internal
{
    public class BucketProviderTests
    {

        #region ctor

        [Fact]
        public void ctor_NoClusterProvider_Exception()
        {
            // Act/Assert

            var ex = Assert.Throws<ArgumentNullException>(() => new BucketProvider(null));

            Assert.Equal("clusterProvider", ex.ParamName);
        }

        #endregion

        #region GetBucket

        [Fact]
        public void GetBucket_Disposed_Exception()
        {
            // Arrange

            var clusterProvider = new Mock<IClusterProvider>();

            var provider = new BucketProvider(clusterProvider.Object);
            provider.Dispose();

            // Act/Assert

            var ex = Assert.Throws<ObjectDisposedException>(() => provider.GetBucket("bucket1"));

            Assert.Equal(nameof(BucketProvider), ex.ObjectName);
        }

        [Fact]
        public void GetBucket_NullBucketName_Exception()
        {
            // Arrange

            var clusterProvider = new Mock<IClusterProvider>();

            var provider = new BucketProvider(clusterProvider.Object);

            // Act/Assert

            var ex = Assert.Throws<ArgumentNullException>(() => provider.GetBucket(null));

            Assert.Equal("bucketName", ex.ParamName);
        }

        [Fact]
        public void GetBucket_FirstTime_OpensBucket()
        {
            // Arrange

            var bucket1 = new Mock<IBucket>();

            var cluster = new Mock<ICluster>();
            cluster
                .Setup(m => m.OpenBucket("bucket1"))
                .Returns(bucket1.Object);
            cluster
                .SetupGet(m => m.Configuration)
                .Returns(new ClientConfiguration()
                {
                    BucketConfigs = new Dictionary<string, BucketConfiguration>()
                });

            var clusterProvider = new Mock<IClusterProvider>();
            clusterProvider
                .Setup(m => m.GetCluster())
                .Returns(cluster.Object);

            var provider = new BucketProvider(clusterProvider.Object);

            // Act

            var result = provider.GetBucket("bucket1");

            // Assert

            Assert.Equal(bucket1.Object, result);
        }

        [Fact]
        public void GetBucket_MultipleTimes_ReturnsFirstOpenBucket()
        {
            // Arrange

            var bucket1 = new Mock<IBucket>();

            var cluster = new Mock<ICluster>();
            cluster
                .Setup(m => m.OpenBucket("bucket1"))
                .Returns(bucket1.Object);
            cluster
                .SetupGet(m => m.Configuration)
                .Returns(new ClientConfiguration()
                {
                    BucketConfigs = new Dictionary<string, BucketConfiguration>()
                });

            var clusterProvider = new Mock<IClusterProvider>();
            clusterProvider
                .Setup(m => m.GetCluster())
                .Returns(cluster.Object);

            var provider = new BucketProvider(clusterProvider.Object);

            // Act

            provider.GetBucket("bucket1");
            var result = provider.GetBucket("bucket1");

            // Assert

            Assert.Equal(bucket1.Object, result);
            cluster.Verify(m => m.OpenBucket("bucket1"), Times.Once);
        }

        [Fact]
        public void GetBucket_NoPassword_OpensBucketWithNoPassword()
        {
            // Arrange

            var bucket1 = new Mock<IBucket>();

            var cluster = new Mock<ICluster>();
            cluster
                .Setup(m => m.OpenBucket("bucket1"))
                .Returns(bucket1.Object);
            cluster
                .SetupGet(m => m.Configuration)
                .Returns(new ClientConfiguration()
                {
                    BucketConfigs = new Dictionary<string, BucketConfiguration>()
                });

            var clusterProvider = new Mock<IClusterProvider>();
            clusterProvider
                .Setup(m => m.GetCluster())
                .Returns(cluster.Object);

            var provider = new BucketProvider(clusterProvider.Object);

            // Act

            provider.GetBucket("bucket1");

            // Assert

            cluster.Verify(m => m.OpenBucket("bucket1"), Times.Once);
        }

        [Fact]
        public void GetBucket_NoPassword_OpensBucketWithConfigPassword()
        {
            // Arrange

            var bucket1 = new Mock<IBucket>();

            var cluster = new Mock<ICluster>();
            cluster
                .Setup(m => m.OpenBucket("bucket1"))
                .Returns(bucket1.Object);
            cluster
                .SetupGet(m => m.Configuration)
                .Returns(new ClientConfiguration()
                {
                    BucketConfigs = new Dictionary<string, BucketConfiguration>
                    {
                        {
                            "bucket1",
                            new BucketConfiguration()
                            {
                                BucketName = "bucket1",
                                Password = "password"
                            }
                        }
                    }
                });

            var clusterProvider = new Mock<IClusterProvider>();
            clusterProvider
                .Setup(m => m.GetCluster())
                .Returns(cluster.Object);

            var provider = new BucketProvider(clusterProvider.Object);

            // Act

            provider.GetBucket("bucket1");

            // Assert

            cluster.Verify(m => m.OpenBucket("bucket1", "password"), Times.Once);
        }

        [Fact]
        public void GetBucket_WithPassword_OpensBucketWithPassword()
        {
            // Arrange

            var bucket1 = new Mock<IBucket>();

            var cluster = new Mock<ICluster>();
            cluster
                .Setup(m => m.OpenBucket("bucket1", "password"))
                .Returns(bucket1.Object);
            cluster
                .SetupGet(m => m.Configuration)
                .Returns(new ClientConfiguration()
                {
                    BucketConfigs = new Dictionary<string, BucketConfiguration>()
                });

            var clusterProvider = new Mock<IClusterProvider>();
            clusterProvider
                .Setup(m => m.GetCluster())
                .Returns(cluster.Object);

            var provider = new BucketProvider(clusterProvider.Object);

            // Act

            provider.GetBucket("bucket1", "password");

            // Assert

            cluster.Verify(m => m.OpenBucket("bucket1", "password"), Times.Once);
        }

        [Fact]
        public void GetBucket_WithPassword_OpensBucketWithPasswordNotConfig()
        {
            // Arrange

            var bucket1 = new Mock<IBucket>();

            var cluster = new Mock<ICluster>();
            cluster
                .Setup(m => m.OpenBucket("bucket1"))
                .Returns(bucket1.Object);
            cluster
                .SetupGet(m => m.Configuration)
                .Returns(new ClientConfiguration()
                {
                    BucketConfigs = new Dictionary<string, BucketConfiguration>
                    {
                        {
                            "bucket1",
                            new BucketConfiguration()
                            {
                                BucketName = "bucket1",
                                Password = "password2"
                            }
                        }
                    }
                });

            var clusterProvider = new Mock<IClusterProvider>();
            clusterProvider
                .Setup(m => m.GetCluster())
                .Returns(cluster.Object);

            var provider = new BucketProvider(clusterProvider.Object);

            // Act

            provider.GetBucket("bucket1", "password");

            // Assert

            cluster.Verify(m => m.OpenBucket("bucket1", "password"), Times.Once);
        }

        #endregion

        #region Dispose

        [Fact]
        public void Dispose_DisposesBuckets()
        {
            // Arrange

            var bucket1 = new Mock<IBucket>();
            bucket1.Setup(m => m.Dispose());

            var bucket2 = new Mock<IBucket>();
            bucket2.Setup(m => m.Dispose());

            var cluster = new Mock<ICluster>();
            cluster
                .Setup(m => m.OpenBucket("bucket1"))
                .Returns(bucket1.Object);
            cluster
                .Setup(m => m.OpenBucket("bucket2"))
                .Returns(bucket2.Object);
            cluster
                .SetupGet(m => m.Configuration)
                .Returns(new ClientConfiguration()
                {
                    BucketConfigs = new Dictionary<string, BucketConfiguration>()
                });

            var clusterProvider = new Mock<IClusterProvider>();
            clusterProvider
                .Setup(m => m.GetCluster())
                .Returns(cluster.Object);

            var provider = new BucketProvider(clusterProvider.Object);
            provider.GetBucket("bucket1");
            provider.GetBucket("bucket2");

            // Act

            provider.Dispose();

            // Assert

            bucket1.Verify(m => m.Dispose(), Times.AtLeastOnce);
            bucket2.Verify(m => m.Dispose(), Times.AtLeastOnce);
        }

        #endregion
    }
}
