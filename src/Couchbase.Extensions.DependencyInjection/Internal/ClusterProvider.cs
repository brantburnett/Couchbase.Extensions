using System;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Microsoft.Extensions.Options;

namespace Couchbase.Extensions.DependencyInjection.Internal
{
    internal class ClusterProvider : IClusterProvider
    {
        private readonly IOptions<CouchbaseClientDefinition> _options;
        private ICluster _cluster;
        private bool _disposed = false;

        public ClusterProvider(IOptions<CouchbaseClientDefinition> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;
        }

        public virtual ICluster GetCluster()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ClusterProvider));
            }

            if (_cluster != null)
            {
                return _cluster;
            }

            _cluster = CreateCluster(_options.Value);

            return _cluster;
        }

        /// <summary>
        /// Seam for injecting mock
        /// </summary>
        protected virtual ICluster CreateCluster(CouchbaseClientDefinition clientDefinition)
        {
            return new Cluster(clientDefinition);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                if (_cluster != null)
                {
                    _cluster.Dispose();
                    _cluster = null;
                }
            }
        }
    }
}
