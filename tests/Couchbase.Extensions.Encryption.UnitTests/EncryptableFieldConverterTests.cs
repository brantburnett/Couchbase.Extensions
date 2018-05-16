using System;
using System.Collections.Generic;
using System.Security.Authentication;
using Couchbase.Extensions.Encryption.Providers;
using Couchbase.Extensions.Encryption.Stores;
using Newtonsoft.Json;
using Xunit;

namespace Couchbase.Extensions.Encryption.UnitTests
{
    public class EncryptableFieldConverterTests
    {
        [Fact]
        public void Test_Serialize()
        {
            var serializer = GetFieldSerializer();

            var poco = new Poco
            {
                StringField = "Woot!"
            };
            var bytes = serializer.Serialize(poco);
            var poco2 = serializer.Deserialize<Poco>(bytes, 0, bytes.Length);

            Assert.Equal(poco.StringField, poco2.StringField);
        }

        [Fact]
        public void When_Cipher_Is_Modified_HMAC_Throws_AuthException()
        {
            var serializer = GetFieldSerializer();

            var poco = new Poco
            {
                StringField = "Woot!"
            };

            var bytes = serializer.Serialize(poco);
            bytes[120] = Convert.FromBase64String(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("s")))[0];

            Assert.Throws<AuthenticationException>(() => serializer.Deserialize<Poco>(bytes, 0, bytes.Length));
        }

        public class Poco
        {
            [EncryptedField(Provider = "MyProvider")]
            public string StringField { get; set; }
        }

        public EncryptedFieldSerializer GetFieldSerializer()
        {
            var providers = new Dictionary<string, ICryptoProvider>
            {
                {"MyProvider", new AesCryptoProvider(new InsecureKeyStore(
                    new KeyValuePair<string, string>("publickey", "!mysecretkey#9^5usdk39d&dlf)03sL"),
                    new KeyValuePair<string, string>("myauthsecret", "mysecret")))
                {
                    PublicKeyName = "publickey",
                    SigningKeyName = "myauthsecret"
                }}
            };
            return new EncryptedFieldSerializer(
                new JsonSerializerSettings { ContractResolver = new EncryptedFieldContractResolver(providers) },
                new JsonSerializerSettings { ContractResolver = new EncryptedFieldContractResolver(providers) });
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
