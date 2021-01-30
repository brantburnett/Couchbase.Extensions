using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Extensions.MultiOp.Internal;
using Couchbase.KeyValue;
using Microsoft.VisualBasic.CompilerServices;
using Moq;
using Xunit;

namespace Couchbase.Extensions.MultiOp.UnitTests.Internal
{
    public class MultiTests
    {
        #region ObserveMulti

        [Fact]
        public async Task ObserveMulti_Success_ReturnsSuccess()
        {
            // Arrange

            var keys = Enumerable.Range(0, 100)
                .Select(p => p.ToString());

            // Act

            var result = await Multi.ObserveMulti(keys, _ => Task.Delay(10)).ToList();

            // Assert

            Assert.Equal(100, result.Count);
            Assert.All(result, p => Assert.True(p.IsSuccessful));
        }

        [Fact]
        public async Task ObserveMulti_CouchbaseException_ReturnsErrors()
        {
            // Arrange

            var keys = Enumerable.Range(0, 100)
                .Select(p => p.ToString());

            // Act

            var result = await Multi.ObserveMulti(keys, _ => Task.FromException(new CouchbaseException())).ToList();

            // Assert

            Assert.Equal(100, result.Count);
            Assert.All(result, p =>
            {
                Assert.NotNull(p.Exception);
                Assert.IsAssignableFrom<CouchbaseException>(p.Exception);
            });
        }

        [Fact]
        public async Task ObserveMulti_FrameworkException_EmitsToObservable()
        {
            // Arrange

            var keys = Enumerable.Range(0, 100)
                .Select(p => p.ToString());

            // Act/Assert

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await Multi.ObserveMulti(keys, p => p == "20"
                    ? Task.FromException(new InvalidOperationException())
                    : Task.CompletedTask).ToList();
            });
        }

        [Fact]
        public async Task ObserveMulti_Disposed_StopsOperationsAndThrows()
        {
            // Arrange

            var keys = Enumerable.Range(0, 100)
                .Select(p => p.ToString());

            using var cts = new CancellationTokenSource();

            var count = 0;

            // Act/Assert

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await Multi.ObserveMulti(keys, p =>
                {
                    if (p == "20")
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        cts.Cancel();
                    }

                    count++;
                    return Task.CompletedTask;
                }).ToList().ToTask(cts.Token);
            });

            Assert.True(count < 100);
        }

        #endregion

        #region ObserveMultiTItem

        [Fact]
        public async Task ObserveMultiTItem_Success_ReturnsSuccess()
        {
            // Arrange

            var keys = Enumerable.Range(0, 100)
                .Select(p => new KeyValuePair<string, int>(p.ToString(), p));

            // Act

            var result = await Multi.ObserveMulti(keys, _ => Task.Delay(10)).ToList();

            // Assert

            Assert.Equal(100, result.Count);
            Assert.All(result, p => Assert.True(p.IsSuccessful));
        }

        [Fact]
        public async Task ObserveMultiTItem_CouchbaseException_ReturnsErrors()
        {
            // Arrange

            var keys = Enumerable.Range(0, 100)
                .Select(p => new KeyValuePair<string, int>(p.ToString(), p));

            // Act

            var result = await Multi.ObserveMulti(keys, _ => Task.FromException(new CouchbaseException())).ToList();

            // Assert

            Assert.Equal(100, result.Count);
            Assert.All(result, p =>
            {
                Assert.NotNull(p.Exception);
                Assert.IsAssignableFrom<CouchbaseException>(p.Exception);
            });
        }

        [Fact]
        public async Task ObserveMultiTItem_FrameworkException_EmitsToObservable()
        {
            // Arrange

            var keys = Enumerable.Range(0, 100)
                .Select(p => new KeyValuePair<string, int>(p.ToString(), p));

            // Act/Assert

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await Multi.ObserveMulti(keys, p => p.Value == 20
                    ? Task.FromException(new InvalidOperationException())
                    : Task.CompletedTask).ToList();
            });
        }

        [Fact]
        public async Task ObserveMultiTItem_Disposed_StopsOperationsAndThrows()
        {
            // Arrange

            var keys = Enumerable.Range(0, 100)
                .Select(p => new KeyValuePair<string, int>(p.ToString(), p));

            using var cts = new CancellationTokenSource();

            var count = 0;

            // Act/Assert

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await Multi.ObserveMulti(keys, p =>
                {
                    if (p.Value == 20)
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        cts.Cancel();
                    }

                    count++;
                    return Task.CompletedTask;
                }).ToList().ToTask(cts.Token);
            });

            Assert.True(count < 100);
        }

        #endregion

        #region ObserveMultiTResult

        [Fact]
        public async Task ObserveMultiTResult_Success_ReturnsSuccess()
        {
            // Arrange

            var keys = Enumerable.Range(0, 100)
                .Select(p => p.ToString());

            // Act

            var result = await Multi.ObserveMulti(keys, async p =>
            {
                await Task.Delay(10);

                return Mock.Of<IGetResult>();
            }).ToList();

            // Assert

            Assert.Equal(100, result.Count);
            Assert.All(result, p => Assert.True(p.IsSuccessful));
        }

        [Fact]
        public async Task ObserveMultiTResult_CouchbaseException_ReturnsErrors()
        {
            // Arrange

            var keys = Enumerable.Range(0, 100)
                .Select(p => p.ToString());

            // Act

            var result = await Multi.ObserveMulti(keys, _ => Task.FromException<IGetResult>(new CouchbaseException())).ToList();

            // Assert

            Assert.Equal(100, result.Count);
            Assert.All(result, p =>
            {
                Assert.NotNull(p.Exception);
                Assert.IsAssignableFrom<CouchbaseException>(p.Exception);
            });
        }

        [Fact]
        public async Task ObserveMultiTResult_FrameworkException_EmitsToObservable()
        {
            // Arrange

            var keys = Enumerable.Range(0, 100)
                .Select(p => p.ToString());

            // Act/Assert

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await Multi.ObserveMulti(keys, p => p == "20"
                    ? Task.FromException<IGetResult>(new InvalidOperationException())
                    : Task.FromResult(Mock.Of<IGetResult>())).ToList();
            });
        }

        [Fact]
        public async Task ObserveMultiTResult_Disposed_StopsOperationsAndThrows()
        {
            // Arrange

            var keys = Enumerable.Range(0, 100)
                .Select(p => p.ToString());

            using var cts = new CancellationTokenSource();

            var count = 0;

            // Act/Assert

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await Multi.ObserveMulti(keys, p =>
                {
                    if (p == "20")
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        cts.Cancel();
                    }

                    count++;
                    return Task.FromResult(Mock.Of<IGetResult>());
                }).ToList().ToTask(cts.Token);
            });

            Assert.True(count < 100);
        }

        #endregion

        #region ObserveMultiTItemTResult

        [Fact]
        public async Task ObserveMultiTItemTResult_Success_ReturnsSuccess()
        {
            // Arrange

            var keys = Enumerable.Range(0, 100)
                .Select(p => new KeyValuePair<string, int>(p.ToString(), p));

            // Act

            var result = await Multi.ObserveMulti(keys, async p =>
            {
                await Task.Delay(10);

                return Mock.Of<IGetResult>();
            }).ToList();

            // Assert

            Assert.Equal(100, result.Count);
            Assert.All(result, p => Assert.True(p.IsSuccessful));
        }

        [Fact]
        public async Task ObserveMultiTItemTResult_CouchbaseException_ReturnsErrors()
        {
            // Arrange

            var keys = Enumerable.Range(0, 100)
                .Select(p => new KeyValuePair<string, int>(p.ToString(), p));

            // Act

            var result = await Multi.ObserveMulti(keys, _ => Task.FromException<IGetResult>(new CouchbaseException())).ToList();

            // Assert

            Assert.Equal(100, result.Count);
            Assert.All(result, p =>
            {
                Assert.NotNull(p.Exception);
                Assert.IsAssignableFrom<CouchbaseException>(p.Exception);
            });
        }

        [Fact]
        public async Task ObserveMultiTItemTResult_FrameworkException_EmitsToObservable()
        {
            // Arrange

            var keys = Enumerable.Range(0, 100)
                .Select(p => new KeyValuePair<string, int>(p.ToString(), p));

            // Act/Assert

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await Multi.ObserveMulti(keys, p => p.Value == 20
                    ? Task.FromException<IGetResult>(new InvalidOperationException())
                    : Task.FromResult(Mock.Of<IGetResult>())).ToList();
            });
        }

        [Fact]
        public async Task ObserveMultiTItemTResult_Disposed_StopsOperationsAndThrows()
        {
            // Arrange

            var keys = Enumerable.Range(0, 100)
                .Select(p => new KeyValuePair<string, int>(p.ToString(), p));

            using var cts = new CancellationTokenSource();

            var count = 0;

            // Act/Assert

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await Multi.ObserveMulti(keys, p =>
                {
                    if (p.Value == 20)
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        cts.Cancel();
                    }

                    count++;
                    return Task.FromResult(Mock.Of<IGetResult>());
                }).ToList().ToTask(cts.Token);
            });

            Assert.True(count < 100);
        }

        #endregion
    }
}
