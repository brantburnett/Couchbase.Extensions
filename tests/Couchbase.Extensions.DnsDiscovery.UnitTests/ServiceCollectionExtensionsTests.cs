using System;
using System.Collections.Generic;
using Couchbase.Configuration.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Couchbase.Extensions.DnsDiscovery.UnitTests
{
    public class ServiceCollectionExtensionsTests
    {
        #region AddCouchbaseDnsDiscovery

        [Fact]
        public void AddCouchbaseDnsDiscovery_NullRecordName_Exception()
        {
            // Arrange

            var services = new ServiceCollection();

            //  Act/Assert

            var ex = Assert.Throws<ArgumentNullException>(() => services.AddCouchbaseDnsDiscovery(null));

            Assert.Equal("recordName", ex.ParamName);
        }

        [Fact]
        public void AddCouchbaseDnsDiscovery_WithRecordName_RegistersOption()
        {
            // Arrange

            const string recordName = "_couchbase._tcp.services.local";

            var serverList = new List<Uri>();

            var lookup = new Mock<ICouchbaseDnsLookup>();
            lookup
                .Setup(m => m.Apply(It.IsAny<CouchbaseClientDefinition>(), recordName))
                .Callback<CouchbaseClientDefinition, string>((clientDefinition, tempRecordName) =>
                {
                    clientDefinition.Servers = serverList;
                });

            var services = new ServiceCollection();
            services.AddOptions();
            services.AddSingleton(lookup.Object);
            services.AddCouchbaseDnsDiscovery(recordName);

            var serviceProvider = services.BuildServiceProvider();

            //  Act

            var result = serviceProvider.GetService<IOptions<CouchbaseClientDefinition>>();

            // Assert

            Assert.NotNull(result);
            Assert.Equal(serverList, result.Value.Servers);
        }

        #endregion
    }
}

