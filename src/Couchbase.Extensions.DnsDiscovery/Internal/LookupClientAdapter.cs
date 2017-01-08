using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using Microsoft.Extensions.Logging;

namespace Couchbase.Extensions.DnsDiscovery.Internal
{
    /// <summary>
    /// Mockable adapter for DnsClient.LookupClient
    /// </summary>
    internal class LookupClientAdapter : ILookupClientAdapter
    {
        private readonly ILogger<LookupClientAdapter> _logger;
        private readonly LookupClient _lookupClient = new LookupClient();

        public LookupClientAdapter(ILogger<LookupClientAdapter> logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _logger = logger;
        }

        public async Task<IEnumerable<SrvRecord>> QuerySrvAsync(string query)
        {
            var response = await _lookupClient.QueryAsync(query, QueryType.SRV);

            if (response.HasError)
            {
                _logger.LogError("DNS query error '{0}'", response.ErrorMessage);

                return new SrvRecord[] {};
            }

            return response.Answers.SrvRecords();
        }
    }
}
