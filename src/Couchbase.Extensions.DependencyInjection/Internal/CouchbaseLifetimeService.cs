using System;
using Microsoft.Extensions.DependencyInjection;

namespace Couchbase.Extensions.DependencyInjection.Internal
{
    internal class CouchbaseLifetimeService :  ICouchbaseLifetimeService
    {
        private readonly IServiceProvider _serviceProvider;

        public CouchbaseLifetimeService(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            _serviceProvider = serviceProvider;
        }

        public void Close()
        {
            _serviceProvider.GetService<IBucketProvider>()?.Dispose();
            _serviceProvider.GetService<IClusterProvider>()?.Dispose();
        }
    }
}
