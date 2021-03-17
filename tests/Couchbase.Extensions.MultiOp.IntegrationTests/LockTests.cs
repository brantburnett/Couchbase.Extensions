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
    public class LockTests : IClassFixture<ClusterFixture>
    {
        private static readonly MultiOpOptions HighParallelism = new MultiOpOptions
        {
            DegreeOfParallelism = 100
        };

        public LockTests(ClusterFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly ClusterFixture _fixture;

        [Theory]
        [InlineData(10)]
        [InlineData(100_000)]
        public async Task LockAndUnlock(int size)
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
                    await collection.GetAndLock(keys, TimeSpan.FromSeconds(30), HighParallelism)
                        .ToList();

                Assert.Equal(size, result.Count);
                Assert.All(result, p =>
                {
                    Assert.NotNull(p.Result);
                    Assert.Equal(p.Key, p.Result.ContentAs<TestDoc>().Value);
                });

                await collection.Unlock<TestDoc>(result.Select(p => new KeyValuePair<string, ulong>(p.Key, p.Result!.Cas)), HighParallelism)
                    .ToList();

                Assert.All(result, p =>
                {
                    Assert.True(p.IsSuccessful);
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
