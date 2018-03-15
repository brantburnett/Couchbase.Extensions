using Couchbase.Configuration.Client;
using Microsoft.Extensions.Configuration;

namespace Couchbase.Extensions.Encryption.IntegrationTests
{
    public static class TestConfiguration
    {
        public static ICouchbaseClientDefinition GetConfiguration()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("configuration.json");

            var configurationSection = builder.Build().GetSection("Couchbase");
            var definition = new CouchbaseClientDefinition();
            configurationSection.Bind(definition);

            return definition;
        }
    }
}
