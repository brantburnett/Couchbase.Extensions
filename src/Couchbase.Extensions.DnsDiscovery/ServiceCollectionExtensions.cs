using System;
using Couchbase.Configuration.Client;
using Couchbase.Extensions.DnsDiscovery.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Couchbase.Extensions.DnsDiscovery
{
    /// <summary>
    /// Extensions to <see cref="IServiceCollection"/> for Couchbase DNS discovery.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Couchbase dependencies to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="recordName">Name of the DNS SRV record for lookup.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        /// <remarks>
        /// This method should be called after adding any other Couchbase configuration
        /// to the service provider.
        /// </remarks>
        public static IServiceCollection AddCouchbaseDnsDiscovery(this IServiceCollection services,
            string recordName)
        {
            if (recordName == null)
            {
                throw new ArgumentNullException(nameof(recordName));
            }

            // Register CouchbaseDnsLookup
            services.TryAddTransient<ILookupClientAdapter, LookupClientAdapter>();
            services.TryAddTransient<ICouchbaseDnsLookup, CouchbaseDnsLookup>();

            // Register the action to alter CouchbaseClientDefinition options
            return services.AddTransient<IConfigureOptions<CouchbaseClientDefinition>>(serviceProvider =>
            {
                // Get the ICouchbaseDnsLookup implementation
                var lookup = serviceProvider.GetRequiredService<ICouchbaseDnsLookup>();

                // Return action that calls Apply on the ICouchbaseDnsLookup implementation
                return new ConfigureOptions<CouchbaseClientDefinition>(clientDefinition =>
                {
                    lookup.Apply(clientDefinition, recordName);
                });
            });
        }
    }
}
