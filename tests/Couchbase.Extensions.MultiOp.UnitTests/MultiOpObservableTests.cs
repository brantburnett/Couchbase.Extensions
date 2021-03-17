using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Couchbase.Extensions.MultiOp.UnitTests
{
    public class MultiOpObservableTests
    {
        #region EnsureSuccessfulAsync

        [Fact]
        public async Task EnsureSuccessfulAsync_NoError_ReturnsWhenComplete()
        {
            // Arrange

            var count = 0;

            var observable = Enumerable.Range(1, 10)
                .Select(p => new MultiOpResult(p.ToString()))
                .ToObservable()
                .Do(_ => count++);

            // Act

            await observable.EnsureSuccessfulAsync();

            // Assert

            Assert.Equal(10, count);
        }

        [Fact]
        public async Task EnsureSuccessfulAsync_CouchbaseError_ExceptionWhenComplete()
        {
            // Arrange

            var count = 0;

            var observable = Enumerable.Range(1, 10)
                .Select(p => p % 2 == 0
                    ? new MultiOpResult(p.ToString(), new CouchbaseException("test"))
                    : new MultiOpResult(p.ToString()))
                .ToObservable()
                .Do(_ => count++);

            // Act

            var task = observable.EnsureSuccessfulAsync();

            try
            {
                await task;
            }
            catch
            {
                // The AggregateException is unwrapped by await, we'll ignore that and validate Task.Exception
            }

            // Assert

            Assert.Equal(10, count);

            var aggregateException = Assert.IsAssignableFrom<AggregateException>(task.Exception);
            Assert.Equal(5, aggregateException.InnerExceptions.Count);
        }

        [Fact]
        public async Task EnsureSuccessfulAsync_FrameworkError_ThrowsException()
        {
            // Arrange

            var count = 0;

            var observable = Enumerable.Range(0, 10)
                .Select(p => p < 5
                    ? new MultiOpResult(p.ToString())
                    : throw new InvalidOperationException())
                .ToObservable()
                .Do(_ => count++);

            // Act/Assert

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await observable.EnsureSuccessfulAsync());

            Assert.Equal(5, count);
        }

        [Fact]
        public async Task EnsureSuccessfulAsync_Canceled_ThrowsOperationCanceledExceptionAndStops()
        {
            // Arrange

            var count = 0;

            using var cts = new CancellationTokenSource();

            var observable = Observable.Create<MultiOpResult>(async (observer, cancellationToken) =>
            {
                for (var i = 0; i < 100; i++)
                {
                    await Task.Delay(10, cancellationToken).ConfigureAwait(false);

                    if (i == 20)
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        cts.Cancel();
                    }

                    observer.OnNext(new MultiOpResult(i.ToString()));
                    count++;
                }
            }).Synchronize();

            // Act/Assert

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => await observable.EnsureSuccessfulAsync(cts.Token).ConfigureAwait(false));

            Assert.Equal(21, count);
        }

        #endregion
    }
}
