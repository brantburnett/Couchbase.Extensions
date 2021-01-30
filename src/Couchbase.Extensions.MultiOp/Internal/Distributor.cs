using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Couchbase.Extensions.MultiOp.Internal
{
    internal sealed class Distributor<T> : IDisposable
    {
        private const int MaxLocalQueueSize = 5;

        private readonly IEnumerator<T> _enumerator;
        private readonly MultiOpOptions _options;
        private int? _knownLength;
        private volatile bool _disposed;
        private bool _used;
        private bool _hasData;

        public Distributor(IEnumerable<T> items, MultiOpOptions options)
        {
            if (items is IList<T> list)
            {
                _knownLength = list.Count;
            }

            _enumerator = items.GetEnumerator();
            _options = options;
        }

        public IList<IEnumerable<T>> GetBatches()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Distributor<T>));
            }
            if (_used)
            {
                throw new InvalidOperationException();
            }

            // Track the fact we've started
            _used = true;

            // Move to the first element (if any)
            _hasData = _enumerator.MoveNext();
            if (!_hasData)
            {
                return Array.Empty<IEnumerable<T>>();
            }

            IEnumerable<T>[] result;

            if (_knownLength != null && _knownLength.GetValueOrDefault() <= _options.DegreeOfParallelism)
            {
                // Optimize for short data sets, just build simple enumerables with a single value

                result = new IEnumerable<T>[_knownLength.GetValueOrDefault()];

                var i = 0;
                while (i < result.Length)
                {
                    result[i] = new SingleEnumerable(_enumerator.Current);

                    _enumerator.MoveNext();
                    i++;
                }

                _enumerator.Dispose();
                _hasData = false;
            }
            else
            {
                // Make a BatchEnumerable, and return a copy for each degree of parallelism

                result = new IEnumerable<T>[_options.DegreeOfParallelism];

                // Make sure our local queue size isn't overly aggressive for medium size lists, we must be sure
                // all of our batches actually process data
                var localQueueSize = _knownLength != null
                    ? Math.Min(_knownLength.GetValueOrDefault() / _options.DegreeOfParallelism, MaxLocalQueueSize)
                    : MaxLocalQueueSize;

                var enumerable = new BatchEnumerable(this, localQueueSize);
                for (var i = 0; i < result.Length; i++)
                {
                    result[i] = enumerable;
                }
            }

            return result;
        }

        public void Dispose()
        {
            _disposed = true;
            _enumerator.Dispose();
        }

        private int GetMoreItems(T[] buffer)
        {
            lock (_enumerator)
            {
                if (_disposed)
                {
                    return 0;
                }

                int count = 0;
                while (_hasData && count < buffer.Length)
                {
                    buffer[count] = _enumerator.Current!;
                    count++;

                    _hasData = _enumerator.MoveNext();
                }

                if (!_hasData)
                {
                    // No more data so go ahead and dispose of the enumerator
                    _enumerator.Dispose();
                }

                return count;
            }
        }

        private class BatchEnumerable : IEnumerable<T>
        {
            private readonly Distributor<T> _distributor;
            private readonly int _localQueueSize;

            public BatchEnumerable(Distributor<T> distributor, int localQueueSize)
            {
                _distributor = distributor;
                _localQueueSize = localQueueSize;
            }

            public IEnumerator<T> GetEnumerator()
            {
                var queue = new T[_localQueueSize];

                while (true)
                {
                    var count = _distributor.GetMoreItems(queue);

                    if (count == 0)
                    {
                        yield break;
                    }

                    for (var i = 0; i < count; i++)
                    {
                        yield return queue[i];
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class SingleEnumerable : IEnumerable<T>, IEnumerator<T>
        {
            private enum State
            {
                Waiting = 0,
                BeforeValue,
                AtValue,
                AfterValue,
                Disposed
            }

            private State _currentState;

            public T Current { get; }

            public SingleEnumerable(T value)
            {
                Current = value;
            }

            public IEnumerator<T> GetEnumerator()
            {
                if (_currentState != State.Waiting)
                {
                    ThrowHelper.ThrowInvalidOperation("Collection may only be enumerated once.");
                }

                _currentState = State.BeforeValue;

                return this;
            }

            public bool MoveNext()
            {
                switch (_currentState)
                {
                    case State.BeforeValue:
                        _currentState = State.AtValue;
                        return true;

                    case State.Disposed:
                        ThrowHelper.ThrowObjectDisposed(nameof(SingleEnumerable));
                        break;

                    default:
                        _currentState = State.AfterValue;
                        break;
                }

                return false;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            object? IEnumerator.Current => Current;

            public void Reset()
            {
                if (_currentState == State.Disposed)
                {
                    ThrowHelper.ThrowObjectDisposed(nameof(SingleEnumerable));
                }

                _currentState = State.BeforeValue;
            }

            public void Dispose()
            {
                _currentState = State.Disposed;
            }
        }
    }
}
