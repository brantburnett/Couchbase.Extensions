using System;
using System.Reflection;

namespace Couchbase.Extensions.Locks
{
    /// <summary>
    /// Global lock holder configuration.
    /// </summary>
    public static class LockHolder
    {
        private static string _default;

        /// <summary>
        /// Default lock holder if none is specified. Will be initialized
        /// with a random GUID for each application instance.
        /// </summary>
        public static string Default
        {
            get => _default;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Value cannot be null or empty.", nameof(value));
                }

                _default = value;
            }
        }

        static LockHolder()
        {
            _default = Guid.NewGuid().ToString();
        }
    }
}
