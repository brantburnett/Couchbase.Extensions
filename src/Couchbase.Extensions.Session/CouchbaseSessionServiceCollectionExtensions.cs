using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.DependencyInjection;

namespace Couchbase.Extensions.Session
{
    public static class CouchbaseSessionServiceCollectionExtensions
    {
        public static IServiceCollection AddCouchbaseSession(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<ISessionStore, CouchbaseDistributedSessionStore>();
            services.AddDataProtection();

            return services;
        }

        public static IServiceCollection AddCouchbaseSession(this IServiceCollection services,
            Action<SessionOptions> options)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            services.Configure(options);
            services.AddCouchbaseSession();

            return services;
        }
    }
}
