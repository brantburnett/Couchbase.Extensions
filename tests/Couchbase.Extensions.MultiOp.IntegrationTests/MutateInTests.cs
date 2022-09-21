using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Couchbase.Extensions.MultiOp.IntegrationTests
{
    public class MutateInTests : IClassFixture<ClusterFixture>
    {
        private static readonly MultiOpOptions HighParallelism = new MultiOpOptions
        {
            DegreeOfParallelism = 100
        };

        public MutateInTests(ClusterFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly ClusterFixture _fixture;

        [Theory]
        [InlineData(10)]
        [InlineData(100_000)]
        public async Task MutateIn_Exists(int size)
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
                    await collection.MutateIn(keys, (key, builder) => builder.Replace("value", key.Substring(10)), HighParallelism)
                        .ToList();

                Assert.Equal(size, result.Count);
                Assert.All(result, p =>
                {
                    Assert.True(p.IsSuccessful);
                    Assert.NotNull(p.Result);
                });

                var getResult = await collection.Get(keys, HighParallelism).ToList();

                Assert.All(getResult, p =>
                {
                    Assert.Equal(p.Key.Substring(10), p.Result!.ContentAs<TestDoc>().Value);
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
