using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Couchbase.Extensions.Encryption.Providers;
using Couchbase.Extensions.Encryption.Stores;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Couchbase.Extensions.Encryption.UnitTests
{
    public class FieldEncryptionContractResolverTests
    {
        private readonly ITestOutputHelper output;

        public FieldEncryptionContractResolverTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void When_No_CryptoProviders_Configured_Throw_ArgumentException()
        {
            var cryptoProviders = new Dictionary<string, ICryptoProvider>
            {
                //uh oh
            };

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new EncryptedFieldContractResolver(cryptoProviders)
            };

            var pocoPlain = new Poco2 { Foo = new List<int> { 3, 4, 5 } };

            Assert.Throws<ArgumentException>(() =>
            {
                var result = JsonConvert.SerializeObject(pocoPlain, settings);
                return result;
            });
        }

        [Fact]
        public void When_CryptoProviders_Are_Null_Throw_ArgumentException()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new EncryptedFieldContractResolver(null)
            };

            var pocoPlain = new Poco2 { Foo = new List<int> { 3, 4, 5 } };

            Assert.Throws<ArgumentException>(() => JsonConvert.SerializeObject(pocoPlain, settings));
        }

        [Fact]
        public void Test_String_Encryption_RoundTrip()
        {
            var settings = GetSettings();
            var pocoPlain = new Poco {Bar = "bar", Foo = 2};

            var json = JsonConvert.SerializeObject(pocoPlain, settings);

            Assert.Contains("__crypt", json); ;

            var poco = JsonConvert.DeserializeObject<Poco>(json, settings);
            Assert.Equal("bar", poco.Bar);
        }

        [Fact]
        public void Test_Integer_Encryption_RoundTrip()
        {
            var settings = GetSettings();
            var pocoPlain = new Poco1 {Foo = 2};

            var json = JsonConvert.SerializeObject(pocoPlain, settings);

            Assert.Contains("__crypt", json); ;

            var poco = JsonConvert.DeserializeObject<Poco1>(json, settings);
            Assert.Equal(2, poco.Foo);
        }

        [Fact]
        public void Test_List_Encryption_RoundTrip()
        {
            var settings = GetSettings();
            var pocoPlain = new Poco2 { Foo = new List<int> {3, 4, 5} };

            var json = JsonConvert.SerializeObject(pocoPlain, settings);

            var poco = JsonConvert.DeserializeObject<Poco2>(json, settings);
            Assert.Equal(new List<int> { 3, 4, 5 }, poco.Foo);
        }

        [Fact]
        public void Test_Object_Encryption_RoundTrip()
        {
            var settings = GetSettings();
            var pocoPlain = new PocoWithChildObject() { Foo = new ChildObject
            {
                IntValue = 3,
                StringValue = "hello"
            } };

            var json = JsonConvert.SerializeObject(pocoPlain, settings);

            Assert.Contains("__crypt", json);

            var poco = JsonConvert.DeserializeObject<PocoWithChildObject>(json, settings);
            Assert.Equal(new ChildObject
            {
                IntValue = 3,
                StringValue = "hello"
            }, poco.Foo);
        }

        [Fact]
        public void Test_Null_Encryption_RoundTrip()
        {
            var settings = GetSettings();
            var pocoPlain = new Poco2 { Foo = null };

            var json = JsonConvert.SerializeObject(pocoPlain, settings);

            Assert.Contains("__crypt", json);

            var poco = JsonConvert.DeserializeObject<Poco2>(json, settings);
            Assert.Null(poco.Foo);
        }

        private JsonSerializerSettings GetSettings()
        {
            var cryptoProviders = new Dictionary<string, ICryptoProvider>
            {
                {
                    "AES-256-HMAC-SHA256", new AesCryptoProvider(new InsecureKeyStore(
                        new KeyValuePair<string, string>("publickey", "!mysecretkey#9^5usdk39d&dlf)03sL"),
                        new KeyValuePair<string, string>("myauthsecret", "mysecret")))
                    {
                        PublicKeyName = "publickey",
                        SigningKeyName = "myauthsecret"
                    }
                }
            };

            return new JsonSerializerSettings
            {
                ContractResolver = new EncryptedFieldContractResolver(cryptoProviders)
            };
        }

        public class Poco
        {
            [EncryptedField(Provider = "AES-256-HMAC-SHA256")]
            public string Bar { get; set; }
            public int Foo { get; set; }
        }

        public class Poco1
        {
            [EncryptedField(Provider = "AES-256-HMAC-SHA256")]
            public int Foo { get; set; }
        }

        public class Poco2
        {
            [EncryptedField]
            public List<int> Foo { get; set; }
        }

        public class PocoWithChildObject
        {
            [EncryptedField(Provider = "AES-256-HMAC-SHA256")]
            public ChildObject Foo { get; set; }
        }

        public class ChildObject
        {
            public string StringValue { get; set; }

            public int IntValue { get; set; }

            public override bool Equals(object obj)
            {
                var that = obj as ChildObject;
                if (obj == null) return false;
                return that != null && (IntValue == that.IntValue && StringValue == that.StringValue);
            }
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
