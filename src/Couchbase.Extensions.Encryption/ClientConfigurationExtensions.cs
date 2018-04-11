using System.Linq;
using Couchbase.Configuration.Client;
using Newtonsoft.Json;

namespace Couchbase.Extensions.Encryption
{
    public static class ClientConfigurationExtensions
    {
        public static void EnableFieldEncryption(this ClientConfiguration config, params ICryptoProvider[] cryptoProviders)
        {
            config.Serializer = () =>
            {
                var providers = cryptoProviders.ToDictionary(cryptoProvider => cryptoProvider.ProviderName);
                return new EncryptedFieldSerializer(
                    new JsonSerializerSettings {ContractResolver = new EncryptedFieldContractResolver(providers)},
                    new JsonSerializerSettings {ContractResolver = new EncryptedFieldContractResolver(providers)});
            };
        }
    }
}

#region [ License information          ]
/* ************************************************************
 *
 *    @author Couchbase <info@couchbase.com>
 *    @copyright 2017 Couchbase, Inc.
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *
 * ************************************************************/
#endregion
