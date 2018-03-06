using Couchbase.Configuration.Client;

namespace Couchbase.Extensions.DnsDiscovery
{
    public interface ICouchbaseDnsLookup
    {
        /// <summary>
        /// Attempts to resolve DNS SRV records from the domain listed in
        /// "couchbase://domain.name" entries in the <see cref="CouchbaseClientDefinition.Servers"/>
        /// collection.
        /// </summary>
        /// <param name="clientDefinition">The client definition to update.</param>
        void Apply(CouchbaseClientDefinition clientDefinition);

        /// <summary>
        /// Attempts to resolve DNS SRV records from the provided domain name.
        /// </summary>
        /// <param name="clientDefinition">The client definition to update.</param>
        /// <param name="recordName">Domain name to resolve.</param>
        void Apply(CouchbaseClientDefinition clientDefinition, string recordName);
    }
}
