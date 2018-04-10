using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Couchbase.Extensions.Encryption.Providers
{
    public class AesCryptoProvider : CryptoProviderBase
    {
        public AesCryptoProvider(IKeystoreProvider keystore) : this()
        {
            KeyStore = keystore;
        }

        public AesCryptoProvider()
        {
            ProviderName = "AES-256-HMAC-SHA256";
        }

        public override byte[] Decrypt(byte[] encryptedBytes, byte[] iv, string keyName = null)
        {
            var key = KeyStore.GetKey(keyName ?? PublicKeyName);

            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;

                var decrypter = aes.CreateDecryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream(encryptedBytes))
                {
                    using (var cs = new CryptoStream(ms, decrypter, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        var value = sr.ReadToEnd();
                        return System.Text.Encoding.UTF8.GetBytes(value);
                    }
                }
            }
        }

        public override byte[] Encrypt(byte[] plainBytes, out byte[] iv)
        {
            var key = KeyStore.GetKey(PublicKeyName);

            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.GenerateIV();
                iv = aes.IV;

                aes.Mode = CipherMode.CBC;
                var encrypter = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encrypter, CryptoStreamMode.Write))
                    {
                        cs.Write(plainBytes, 0, plainBytes.Length);
                    }
                    return ms.ToArray();
                }
            }
        }

        public override bool RequiresAuthentication =>true;
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
