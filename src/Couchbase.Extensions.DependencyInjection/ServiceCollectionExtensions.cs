using System;
using Couchbase.Configuration.Client;
using Couchbase.Extensions.DependencyInjection.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Couchbase.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions to <see cref="IServiceCollection"/> for Couchbase dependency injection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Couchbase dependencies to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configurationSection">Section from the configuration that can be bound to <see cref="CouchbaseClientDefinition"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddCouchbase(this IServiceCollection services,
            IConfiguration configurationSection)
        {
            if (configurationSection == null)
            {
                throw new ArgumentNullException(nameof(configurationSection));
            }

            return services.AddCouchbase(configurationSection.Bind);
        }

        /// <summary>
        /// Add Couchbase dependencies to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="options">Optional action to configure the <see cref="CouchbaseClientDefinition"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddCouchbase(this IServiceCollection services,
            Action<CouchbaseClientDefinition> options)
        {
            services.AddOptions();

            services.AddSingleton<NamedBucketProxyGenerator>();
            services.TryAddSingleton<ICouchbaseLifetimeService, CouchbaseLifetimeService>();
            services.TryAddSingleton<IClusterProvider, ClusterProvider>();
            services.TryAddSingleton<IBucketProvider, BucketProvider>();

            if (options != null)
            {
                services.Configure(options);
            }

            return services;
        }

        public static IServiceCollection AddCouchbaseBucket<T>(this IServiceCollection services, string bucketName)
            where T: class, INamedBucketProvider
        {
            return services.AddCouchbaseBucket<T>(bucketName, null);
        }

        public static IServiceCollection AddCouchbaseBucket<T>(this IServiceCollection services, string bucketName,
            string password)
            where T : class, INamedBucketProvider
        {
            if (bucketName == null)
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            services.AddSingleton(serviceProvider =>
            {
                var generator = serviceProvider.GetRequiredService<NamedBucketProxyGenerator>();

                return generator.GetProxy<T>(serviceProvider.GetRequiredService<IBucketProvider>(), bucketName, password);
            });

            return services;
        }
    }
}
