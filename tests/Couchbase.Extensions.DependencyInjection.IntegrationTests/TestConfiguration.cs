using Microsoft.Extensions.Configuration;

namespace Couchbase.Extensions.DependencyInjection.IntegrationTests
{
    public static class TestConfiguration
    {
        public static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("configuration.json");

            return builder.Build().GetSection("Couchbase");
        }
    }
}
