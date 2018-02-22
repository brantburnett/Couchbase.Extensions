using System.Security.Cryptography;
using System.Text;
using Couchbase.Extensions.Encryption.Providers;
using Couchbase.Extensions.Encryption.Stores;
using Xunit;

namespace Couchbase.Extensions.Encryption.UnitTests.Providers
{
    public class AesCryptoProviderTests
    {
        private IKeystoreProvider _keystore;

        public AesCryptoProviderTests()
        {
            using (var aes = Aes.Create())
            {
                aes.GenerateKey();
                var key = Encoding.Unicode.GetString(aes.Key);

                _keystore = new InsecureKeyStore("mypublickey", key);
            }
        }

        [Fact]
        public void Test_Encrypt()
        {
            var aesCryptoProvider = new AesCryptoProvider
            {
                KeyStore = _keystore,
                KeyName = "mypublickey"
            };

            var someText = "The old grey goose jumped over the wrickety vase.";

            var encryptedTest = aesCryptoProvider.Encrypt(someText);
            Assert.NotEqual(encryptedTest, someText);
        }

        [Fact]
        public void Test_Decrypt()
        {
            var aesCryptoProvider = new AesCryptoProvider
            {
                KeyStore = _keystore,
                KeyName = "mypublickey"
            };

            var someText = "The old grey goose jumped over the wrickety vase.";

            var encryptedTest = aesCryptoProvider.Encrypt(someText);
            Assert.NotEqual(encryptedTest, someText);

            var decryptedText = aesCryptoProvider.Decrypt(encryptedTest, "mypublickey");
            Assert.Equal(decryptedText, someText);
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
