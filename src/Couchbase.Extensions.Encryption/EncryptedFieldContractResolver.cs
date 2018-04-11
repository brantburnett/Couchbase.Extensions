using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Couchbase.Extensions.Encryption.Utils;

namespace Couchbase.Extensions.Encryption
{
    public class EncryptedFieldContractResolver : CamelCasePropertyNamesContractResolver
    {
        public EncryptedFieldContractResolver(Dictionary<string, ICryptoProvider> cryptoProviders)
        {
            CryptoProviders = cryptoProviders;
            EncryptedFieldPrefix = "__crypt_";
        }

        public EncryptedFieldContractResolver(Dictionary<string, ICryptoProvider> cryptoProviders, string encryptedFieldPrefix)
            : this(cryptoProviders)
        {
            EncryptedFieldPrefix = encryptedFieldPrefix;
        }

        public Dictionary<string, ICryptoProvider> CryptoProviders { get; set; }

        public string EncryptedFieldPrefix { get; set; }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var result = base.CreateProperty(member, memberSerialization);

            if (member.GetEncryptedFieldAttribute(out var attribute))
            {
                if (attribute.Provider == null)
                {
                    if (CryptoProviders == null || CryptoProviders.Count == 0)
                    {
                        throw new ArgumentException("A CryptoProvider must be configured.");
                    }

                    //assign the default provider if none is supplied
                    attribute.Provider = CryptoProviders.First().Key;
                }

                var propertyInfo = member as PropertyInfo;
                result.PropertyName = EncryptedFieldPrefix + result.PropertyName;

                result.Converter = new EncryptableFieldConverter(propertyInfo, CryptoProviders, attribute.Provider);
                result.MemberConverter = new EncryptableFieldConverter(propertyInfo, CryptoProviders, attribute.Provider);
            }

            return result;
        }

        public static bool GetEncryptedFieldAttribute(MemberInfo methodInfo, out EncryptedFieldAttribute attribute)
        {
#if NETSTANDARD15
            attribute = methodInfo.GetCustomAttribute<EncryptedFieldAttribute>();
            return attribute != null;
#else
            if (Attribute.IsDefined(methodInfo, typeof(EncryptedFieldAttribute)))
            {
                attribute = (EncryptedFieldAttribute)Attribute.
                    GetCustomAttribute(methodInfo, typeof(EncryptedFieldAttribute));

                return true;
            }
            else
            {
                attribute = null;
                return false;
            }
#endif
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
