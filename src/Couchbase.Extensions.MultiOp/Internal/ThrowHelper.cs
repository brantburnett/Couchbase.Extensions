using System;
using System.Diagnostics.CodeAnalysis;

namespace Couchbase.Extensions.MultiOp.Internal
{
    internal static class ThrowHelper
    {
        [DoesNotReturn]
        public static void ThrowArgumentNullException(string paramName) =>
            throw new ArgumentNullException(paramName);

        [DoesNotReturn]
        public static void ThrowArgumentOutOfRange(string paramName) =>
            throw new ArgumentOutOfRangeException(paramName);

        [DoesNotReturn]
        public static void ThrowInvalidOperation(string message) =>
            throw new InvalidOperationException(message);

        [DoesNotReturn]
        public static void ThrowObjectDisposed(string objectName) =>
            throw new ObjectDisposedException(objectName);
    }
}
