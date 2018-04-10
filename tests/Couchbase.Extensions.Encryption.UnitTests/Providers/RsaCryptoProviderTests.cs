using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;
using Couchbase.Extensions.Encryption.Providers;
using Couchbase.Extensions.Encryption.Stores;
using Xunit;

namespace Couchbase.Extensions.Encryption.UnitTests.Providers
{
    public class RsaCryptoProviderTests
    {
        private IKeystoreProvider _keystore;

        public RsaCryptoProviderTests()
        {
            using (var rsa = RSA.Create())
            {
                rsa.KeySize = 2048;
                var privateKey = rsa.ExportParameters(true);
                var publicKey = rsa.ExportParameters(false);

                _keystore = new InsecureKeyStore(
                    new KeyValuePair<string, string>("privateKey", GetKeyAsString(privateKey)),
                    new KeyValuePair<string, string>("publicKey", GetKeyAsString(publicKey)));
            }
        }

        [Fact]
        public void Test_Encrypt()
        {
            var rsaCryptoProvider = new RsaCryptoProvider
            {
                KeyStore = _keystore,
                PublicKeyName = "publicKey",
                PrivateKey = "privateKey"
            };

            var someText = "The old grey goose jumped over the wrickety vase.";

            var encryptedTest = rsaCryptoProvider.Encrypt(someText);
            Assert.NotEqual(encryptedTest, someText);
        }

        [Fact]
        public void Test_Decrypt()
        {
            var rsaCryptoProvider = new RsaCryptoProvider
            {
                KeyStore =_keystore,
                PublicKeyName = "publicKey",
                PrivateKey = "privateKey"
            };

            var someText = "The old grey goose jumped over the wrickety vase.";

            var encryptedTest = rsaCryptoProvider.Encrypt(someText);
            Assert.NotEqual(encryptedTest, someText);

            var decryptedText = rsaCryptoProvider.Decrypt(encryptedTest, "privateKey");
            Assert.Equal(decryptedText, someText);
        }

        private string GetKeyAsString(RSAParameters parameters)
        {
            using (var writer = new StringWriter())
            {
                var serializer = new XmlSerializer(typeof(RSAParameters));
                serializer.Serialize(writer, parameters);

                return writer.ToString();
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
