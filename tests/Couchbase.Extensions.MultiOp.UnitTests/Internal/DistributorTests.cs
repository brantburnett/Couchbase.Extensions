using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Extensions.MultiOp.Internal;
using Xunit;

namespace Couchbase.Extensions.MultiOp.UnitTests.Internal
{
    public class DistributorTests
    {
        private static readonly MultiOpOptions Dop10 = new MultiOpOptions {DegreeOfParallelism = 10};

        [Fact]
        public void GetBatches_EmptyList_EmptyResult()
        {
            // Arrange

            var distributor = new Distributor<int>(new int[] { }, MultiOpOptions.Default);

            // Act

            var batches = distributor.GetBatches();

            // Assert

            Assert.Empty(batches);
        }

        [Fact]
        public void GetBatches_EmptyEnumerable_EmptyResult()
        {
            // Arrange

            var distributor = new Distributor<int>(Enumerable.Range(0, 0), MultiOpOptions.Default);

            // Act

            var batches = distributor.GetBatches();

            // Assert

            Assert.Empty(batches);
        }

        [Fact]
        public void GetBatches_ListSmallerThanDOP_SingleEnumerables()
        {
            // Arrange

            var distributor = new Distributor<int>(Enumerable.Range(0, 5).ToArray(), Dop10);

            // Act

            var batches = distributor.GetBatches();

            // Assert

            Assert.Equal(5, batches.Count);

            var hashSet = new HashSet<int>();
            foreach (var batch in batches)
            {
                var item = Assert.Single(batch);

                // Be sure each item is unique
                Assert.True(hashSet.Add(item));
            }
        }

        [Fact]
        public void GetBatches_ListSameAsDOP_SingleEnumerables()
        {
            // Arrange

            var distributor = new Distributor<int>(Enumerable.Range(0, 10).ToArray(), Dop10);

            // Act

            var batches = distributor.GetBatches();

            // Assert

            Assert.Equal(10, batches.Count);

            var hashSet = new HashSet<int>();
            foreach (var batch in batches)
            {
                var item = Assert.Single(batch);

                // Be sure each item is unique
                Assert.True(hashSet.Add(item));
            }
        }

        [Fact]
        public void GetBatches_ListDoubleDOP_EvenDistribution()
        {
            // This test ensures medium sized lists of known size don't operate a local queue for batches that is
            // too large and reduces the degree of parallelism. With 20 items and DOP of 10, we should get a local
            // queue size within each batch of 20 / 10 == 2.
            // Note: Uneven distribution is still possible with medium sized lists of unknown size (IEnumerable that is not a list)

            // Arrange

            var distributor = new Distributor<int>(Enumerable.Range(0, 20).ToArray(), Dop10);

            // Act

            var batches = distributor.GetBatches();

            // Assert

            Assert.Equal(10, batches.Count);

            foreach (var batch in batches)
            {
                // If any batch were to take a local queue > 2, one of the batches would end up short.
                // Take is used to make sure we don't cause the local queue to fill more than once.
                Assert.Equal(2, batch.Take(2).Count());
            }
        }

        [Fact]
        public async Task GetBatches_LargeList_DistributesInParallel()
        {
            // Arrange

            var distributor = new Distributor<int>(Enumerable.Range(0, 2000).ToArray(), Dop10);

            var results = new ConcurrentBag<List<int>>();

            // Act

            var batches = distributor.GetBatches();

            // Assert

            Assert.Equal(10, batches.Count);

            await Task.WhenAll(batches.RunFairForEach(async state =>
            {
                var batch = (IEnumerable<int>) state;

                var result = new List<int>();
                foreach (var i in batch)
                {
                    result.Add(i);

                    await Task.Yield();
                }

                results.Add(result);
            }));

            Assert.All(results, Assert.NotEmpty);
        }

        [Fact]
        public async Task GetBatches_Enumerable_DistributesInParallel()
        {
            // Arrange

            var distributor = new Distributor<int>(Enumerable.Range(0, 2000), Dop10);

            var results = new ConcurrentBag<List<int>>();

            // Act

            var batches = distributor.GetBatches();

            // Assert

            Assert.Equal(10, batches.Count);

            await Task.WhenAll(batches.RunFairForEach(async state =>
            {
                var batch = (IEnumerable<int>) state;

                var result = new List<int>();
                foreach (var i in batch)
                {
                    result.Add(i);

                    await Task.Yield();
                }

                results.Add(result);
            }));

            Assert.All(results, Assert.NotEmpty);
        }
    }
}
