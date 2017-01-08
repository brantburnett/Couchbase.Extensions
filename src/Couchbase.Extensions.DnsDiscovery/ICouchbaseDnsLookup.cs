using Couchbase.Configuration.Client;

namespace Couchbase.Extensions.DnsDiscovery
{
    public interface ICouchbaseDnsLookup
    {
        void Apply(CouchbaseClientDefinition clientDefinition, string recordName);
    }
}
