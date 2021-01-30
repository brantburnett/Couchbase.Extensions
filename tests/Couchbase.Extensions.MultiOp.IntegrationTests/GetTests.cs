using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Core.Exceptions.KeyValue;
using Xunit;

namespace Couchbase.Extensions.MultiOp.IntegrationTests
{
    public class GetTests : IClassFixture<ClusterFixture>
    {
        private static readonly MultiOpOptions HighParallelism = new MultiOpOptions
        {
            DegreeOfParallelism = 100
        };

        public GetTests(ClusterFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly ClusterFixture _fixture;

        [Theory]
        [InlineData(10)]
        [InlineData(100_000)]
        public async Task Get_NotFound(int size)
        {
            var collection = (await _fixture.GetDefaultBucketAsync()).DefaultCollection();

            var keys = Enumerable.Range(1, size)
                .Select(_ => Guid.NewGuid().ToString())
                .ToList();

            var result = await collection.Get(keys, HighParallelism).ToList();

            Assert.Equal(size, result.Count);
            Assert.All(result, p => Assert.IsAssignableFrom<DocumentNotFoundException>(p.Exception));
        }

        [Fact]
        public async Task Get_CanCancel()
        {
            // Observe on the Couchbase cluster that operations cease before reaching 100k

            var collection = (await _fixture.GetDefaultBucketAsync()).DefaultCollection();

            var keys = Enumerable.Range(1, 100_000)
                .Select(_ => Guid.NewGuid().ToString());

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                collection.Get(keys, HighParallelism)
                    .ToList().ToTask(cts.Token));
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100_000)]
        public async Task Get_Exists(int size)
        {
            var collection = (await _fixture.GetDefaultBucketAsync()).DefaultCollection();

            var keys = Enumerable.Range(1, size)
                .Select(_ => Guid.NewGuid().ToString())
                .ToList();

            try
            {
                await collection.Upsert(keys.Select(p => new KeyValuePair<string, TestDoc>(p, new TestDoc {Value = p})), HighParallelism)
                    .EnsureSuccessfulAsync();

                var result =
                    await collection.Get(keys, HighParallelism)
                        .ToList();

                Assert.Equal(size, result.Count);
                Assert.All(result, p =>
                {
                    Assert.NotNull(p.Result);
                    Assert.Equal(p.Key, p.Result.ContentAs<TestDoc>().Value);
                });
            }
            finally
            {
                await collection.Remove(keys, HighParallelism);
            }
        }

        private class TestDoc
        {
            public string Value { get; set; }
        }
    }
}
