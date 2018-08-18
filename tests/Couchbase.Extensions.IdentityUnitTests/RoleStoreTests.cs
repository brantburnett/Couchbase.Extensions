using System;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Extensions.Identity;
using Couchbase.Linq;
using Moq;
using Xunit;

namespace Couchbase.Extensions.IdentityUnitTests
{
    public class RoleStoreTests
    {
        [Fact]
        public async Task Test_Null_Parameters()
        {
            var mockBucket = new Mock<IBucket>();
            //var context = new BucketContext(mockBucket.Object);
            var mockBucketProvider = new Mock<ICouchbaseIdentityBucketProvider>();
            mockBucketProvider.Setup(x => x.GetBucket()).Returns(mockBucket.Object);

#pragma warning disable Await1 // Method is not configured to be awaited
            Assert.Throws<ArgumentNullException>("provider", () => new UserStore((ICouchbaseIdentityBucketProvider)null));
            var store = new RoleStore(mockBucketProvider.Object);

            await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await store.SetNormalizedRoleNameAsync(null, null, CancellationToken.None));
            await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await store.SetRoleNameAsync(null, null, CancellationToken.None));
            await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await store.CreateAsync(null, CancellationToken.None));
            await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await store.DeleteAsync(null, CancellationToken.None));
            await Assert.ThrowsAsync<ArgumentNullException>("roleId", async () => await store.FindByIdAsync(null, CancellationToken.None));
            await Assert.ThrowsAsync<ArgumentNullException>("normalizedRoleName", async () => await store.FindByNameAsync(null, CancellationToken.None));
            await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await store.GetNormalizedRoleNameAsync(null, CancellationToken.None));
            await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await store.GetRoleIdAsync(null, CancellationToken.None));
            await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await store.GetRoleNameAsync(null, CancellationToken.None));

#pragma warning restore Await1 // Method is not configured to be awaited
        }
    }
}
