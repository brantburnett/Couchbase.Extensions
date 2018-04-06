using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Couchbase.Configuration.Client;
using DnsClient;
using DnsClient.Protocol;
using Microsoft.Extensions.Logging;

namespace Couchbase.Extensions.DnsDiscovery.Internal
{
    internal class CouchbaseDnsLookup : ICouchbaseDnsLookup
    {
        private readonly ILookupClientAdapter _lookupClient;
        private readonly ILogger<CouchbaseDnsLookup> _logger;

        public CouchbaseDnsLookup(ILookupClientAdapter lookupClient, ILogger<CouchbaseDnsLookup> logger)
        {
            if (lookupClient == null)
            {
                throw new ArgumentNullException(nameof(lookupClient));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _lookupClient = lookupClient;
            _logger = logger;
        }

        public void Apply(CouchbaseClientDefinition clientDefinition)
        {
            if (clientDefinition == null)
            {
                throw new ArgumentNullException(nameof(clientDefinition));
            }

            if (!string.IsNullOrEmpty(clientDefinition.ConnectionString))
            {
                ApplyFromConnectionString(clientDefinition);
            }
            else
            {
                ApplyFromServers(clientDefinition);
            }
        }

        public void Apply(CouchbaseClientDefinition clientDefinition, string recordName)
        {
            if (clientDefinition == null)
            {
                throw new ArgumentNullException(nameof(clientDefinition));
            }
            if (recordName == null)
            {
                throw new ArgumentNullException(nameof(recordName));
            }

            // Ensure an empty collection of servers before resolving
            if (clientDefinition.Servers == null)
            {
                clientDefinition.Servers = new List<Uri>();
            }
            else
            {
                clientDefinition.Servers.Clear();
            }

            if (!recordName.StartsWith("_"))
            {
                var serversPrepended = Resolve("_couchbase._tcp." + recordName, false);
                // Automatically prepend _couchbase._tcp to the recordName
                if (serversPrepended != null)
                {
                    // We found an SRV record with _couchbase._tcp prepended, so stop
                    clientDefinition.Servers.AddRange(serversPrepended);
                    return;
                }
            }

            // Try the record name without prepending _couchbase._tcp (backwards compatibility)
            var servers = Resolve(recordName, false);
            if (servers != null)
            {
                clientDefinition.Servers.AddRange(servers);
            }
        }

        private void ApplyFromConnectionString(CouchbaseClientDefinition clientDefinition)
        {
            Uri uri;
            try
            {
                uri = new Uri(clientDefinition.ConnectionString);
            }
            catch (UriFormatException)
            {
                // We should only perform DNS SRV lookups on a connection string with a single server listed
                // Any URI with more than one server (comma delimited) will fail to parse as a URI
                // So just short circuit on UriFormatException
                return;
            }

            var recordName = TestForSrvRecordName(uri);
            if (recordName != null)
            {
                var resolvedServers = Resolve(recordName, true);
                if (resolvedServers != null)
                {
                    clientDefinition.ConnectionString = ReplaceConnectionStringServers(uri, resolvedServers);
                }
            }

            // Leaves servers in place if no DNS SRV records match
        }

        private void ApplyFromServers(CouchbaseClientDefinition clientDefinition)
        {
            if (clientDefinition.Servers.Count != 1)
            {
                // Must have a single host
                return;
            }

            var recordName = TestForSrvRecordName(clientDefinition.Servers[0]);
            if (recordName != null)
            {
                var resolvedServers = Resolve(recordName, true);
                if (resolvedServers != null)
                {
                    clientDefinition.Servers.Clear();
                    clientDefinition.Servers.AddRange(resolvedServers);
                }
            }

            // Leaves servers in place if no DNS SRV records match
        }

        private List<Uri> Resolve(string recordName, bool ignorePriority)
        {
            if (recordName == null)
            {
                throw new ArgumentNullException(nameof(recordName));
            }

            try
            {
                _logger.LogInformation("Looking up Couchbase servers using record '{0}'", recordName);

                List<SrvRecord> servers;
                var syncContextCache = SynchronizationContext.Current;
                try
                {
                    // Ensure that we're outside any sync context before waiting on an async result to prevent deadlocks
                    SynchronizationContext.SetSynchronizationContext(null);

                    servers = _lookupClient
                        .QuerySrvAsync(recordName).Result
                        .OrderBy(p => p.Priority)
                        .ToList();
                }
                finally
                {
                    if (syncContextCache != null)
                    {
                        SynchronizationContext.SetSynchronizationContext(syncContextCache);
                    }
                }

                if (!servers.Any())
                {
                    _logger.LogError("No SRV records returned for query '{0}'", recordName);
                    return null;
                }

                var serverQuery = servers.AsEnumerable();

                if (!ignorePriority)
                {
                    // The original version of this extension used the priority
                    // However, Couchbase spec says that it should not
                    // For bacwards compatibility, we continue to use it if the recordName is manually provided
                    // But ignore in the new (and recommended) pattern of auto discovery from the configuration

                    var firstPriority = servers.First().Priority;
                    serverQuery = serverQuery.Where(p => p.Priority == firstPriority);
                }

                var result = new List<Uri>();
                foreach (var server in serverQuery)
                {
                    var uri = new Uri($"http://{FormatTargetDns(server.Target)}:{server.Port}/pools");

                    _logger.LogInformation("Got Couchbase server '{0}'", uri);

                    result.Add(uri);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, "Exception getting Couchbase servers for '{0}'", recordName);
                return null;
            }
        }

        private string FormatTargetDns(string target)
        {
            if (target.EndsWith("."))
            {
                return target.Substring(0, target.Length - 1);
            }

            return target;
        }

        /// <summary>
        /// Tests to see if a URI should use SRV lookup
        /// </summary>
        /// <param name="uri"><see cref="Uri"/> to test.</param>
        /// <returns>SRV record name to lookup, or null if not applicable</returns>
        private static string TestForSrvRecordName(Uri uri)
        {
            // Based upon https://github.com/couchbaselabs/sdk-doctor/blob/master/connstr/connstr.go#L24

            if (uri.Scheme != "couchbase" && uri.Scheme != "couchbases")
            {
                // Must be couchbase or couchbases
                return null;
            }

            if (uri.Host.Contains(';'))
            {
                // Must have a single host
                return null;
            }

            if (!uri.IsDefaultPort)
            {
                // Must not include a port in the URI
                return null;
            }

            return $"_{uri.Scheme}._tcp.{uri.Host}";
        }

        private static string ReplaceConnectionStringServers(Uri connectionString, IEnumerable<Uri> servers)
        {
            var builder = new StringBuilder(256);
            builder.Append(connectionString.Scheme);
            builder.Append("://");

            var first = true;
            foreach (var server in servers)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(',');
                }

                builder.AppendFormat("{0}:{1}", server.Host, server.Port);
            }

            builder.Append(connectionString.GetComponents(UriComponents.PathAndQuery, UriFormat.SafeUnescaped));

            return builder.ToString();
        }
    }
}
