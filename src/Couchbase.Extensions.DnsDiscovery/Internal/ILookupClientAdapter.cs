using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DnsClient.Protocol;

namespace Couchbase.Extensions.DnsDiscovery.Internal
{
    /// <summary>
    /// Mockable adapter for DnsClient.LookupClient
    /// </summary>
    internal interface ILookupClientAdapter
    {
        Task<IEnumerable<SrvRecord>> QuerySrvAsync(string query);
    }
}
