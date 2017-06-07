using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Couchbase.Extensions.Caching
{
    public static class CouchbaseCacheServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a <see cref="CouchbaseCache"/> as a service using a <see cref="Action{CouchbaseCacheOptions}"/>
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the <see cref="CouchbaseCache"/> service to.</param>
        /// <param name="setupAction">The setup delegate that will be fired when the service is created.</param>
        /// <returns>The <see cref="IServiceCollection"/> that was updated with the <see cref="Action{CouchbaseCacheOptions}"/></returns>
        public static IServiceCollection AddDistributedCouchbaseCache(this IServiceCollection services, Action<CouchbaseCacheOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.AddOptions();
            services.Configure(setupAction);
            services.Add(ServiceDescriptor.Singleton<IDistributedCache, CouchbaseCache>());

            return services;
        }
    }
}
