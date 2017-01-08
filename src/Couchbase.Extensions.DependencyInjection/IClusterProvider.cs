using System;
using Couchbase.Core;

namespace Couchbase.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides access to a Couchbase cluster.
    /// </summary>
    public interface IClusterProvider : IDisposable
    {
        /// <summary>
        /// Returns the Couchbase cluster.
        /// </summary>
        ICluster GetCluster();
    }
}