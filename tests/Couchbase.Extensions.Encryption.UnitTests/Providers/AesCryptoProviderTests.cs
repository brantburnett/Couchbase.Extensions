using System;
using System.Security.Cryptography;
using System.Text;
using Couchbase.Extensions.Encryption.Providers;
using Couchbase.Extensions.Encryption.Stores;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Couchbase.Extensions.Encryption.UnitTests.Providers
{
    public class AesCryptoProviderTests
    {
        private IKeystoreProvider _keystore;
        private readonly ITestOutputHelper _output;
        private byte[] _iv;


        public AesCryptoProviderTests(ITestOutputHelper output)
        {
            using (var aes = Aes.Create())
            {
                aes.GenerateKey();
                _iv = aes.IV;
                var key = Encoding.Unicode.GetString(aes.Key);

                _keystore = new InsecureKeyStore("mypublickey", "!mysecretkey#9^5usdk39d&dlf)03sL");
                _keystore.StoreKey("myauthsecret", "myauthpassword");
            }
            _output = output;
        }

        [Fact]
        public void Test_Encrypt()
        {
            var aesCryptoProvider = new AesCryptoProvider
            {
                KeyStore = _keystore,
                PublicKeyName = "mypublickey"
            };

            var someText = "The old grey goose jumped over the wrickety vase.";
            var textBytes = Encoding.UTF8.GetBytes(someText);
            var encryptedTest = aesCryptoProvider.Encrypt(textBytes, out _iv);

            Assert.NotEqual(encryptedTest, textBytes);
        }

        [Fact]
        public void Test_Decrypt()
        {
            var aesCryptoProvider = new AesCryptoProvider
            {
                KeyStore = _keystore,
                PublicKeyName = "mypublickey"
            };

            var someText = "The old grey goose jumped over the wrickety vase.";
            var textBytes = Encoding.UTF8.GetBytes(someText);
            var encryptedTest = aesCryptoProvider.Encrypt(textBytes, out _iv);

            Assert.NotEqual(encryptedTest, textBytes);

            var decryptedText = aesCryptoProvider.Decrypt(encryptedTest, _iv, "mypublickey");
            Assert.Equal(decryptedText, textBytes);
        }


        [Fact]
        public void Test_Authenticate()
        {
            var aesCryptoProvider = new AesCryptoProvider
            {
                KeyStore = _keystore,
                PublicKeyName = "mypublickey",
                SigningKeyName = "myauthsecret"
            };

            var someText = "The old grey goose jumped over the wrickety gate.";
            var textBytes = Encoding.UTF8.GetBytes(someText);
            var encryptedTest = aesCryptoProvider.Encrypt(textBytes, out _iv);

            Assert.NotEqual(encryptedTest, textBytes);

            var hash1 = aesCryptoProvider.GetSignature(encryptedTest);

            var decryptedText = aesCryptoProvider.Decrypt(encryptedTest, _iv, "mypublickey");
            Assert.Equal(decryptedText, textBytes);

            var hash2 = aesCryptoProvider.GetSignature(encryptedTest);

            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void Test_Encrypt2()
        {
            var aesCryptoProvider = new AesCryptoProvider
            {
                KeyStore = _keystore,
                PublicKeyName = "mypublickey"
            };

            var message = "The old grey goose jumped over the wrickety gate.";
            var utf8Message = System.Text.Encoding.UTF8.GetBytes(message);

            byte[] iv;
            var encryptedBytes = aesCryptoProvider.Encrypt(utf8Message, out iv);
            var base64message = Convert.ToBase64String(encryptedBytes);

        }

        [Fact]
        public void Test_Decrypt2()
        {
            var aesCryptoProvider = new AesCryptoProvider
            {
                KeyStore = _keystore,
                PublicKeyName = "mypublickey",
                SigningKeyName = "myauthsecret"
            };

            var message = "The old grey goose jumped over the wrickety gate.";
            var utf8Message = System.Text.Encoding.UTF8.GetBytes(message);

            byte[] iv;
            var encryptedBytes = aesCryptoProvider.Encrypt(utf8Message, out iv);
            var base64message = Convert.ToBase64String(encryptedBytes);

            _output.WriteLine(base64message);
            _output.WriteLine("iv: " + Convert.ToBase64String(iv));

            var base64MessageBytes = Convert.FromBase64String(base64message);
            var decrypted = aesCryptoProvider.Decrypt(base64MessageBytes, iv);

            Assert.Equal(message, System.Text.Encoding.UTF8.GetString(decrypted));

            _output.WriteLine("authkey: " + Convert.ToBase64String(Encoding.UTF8.GetBytes(_keystore.GetKey("myauthsecret"))));
            _output.WriteLine("sig: " + Convert.ToBase64String(aesCryptoProvider.GetSignature(encryptedBytes)));
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
