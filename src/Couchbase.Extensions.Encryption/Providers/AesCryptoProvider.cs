using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Couchbase.Extensions.Encryption.Providers
{
    public class AesCryptoProvider : ICryptoProvider
    {
        public AesCryptoProvider(IKeystoreProvider keystore) : this()
        {
            KeyStore = keystore;
        }

        public AesCryptoProvider()
        {
            Name = "AES-256";
        }

        public IKeystoreProvider KeyStore { get; set; }

        public object Decrypt(object value, string keyName = null)
        {
            var key = KeyStore.GetKey(keyName??KeyName);
            var encodedJsonBytes = Convert.FromBase64String(value.ToString());

            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.Unicode.GetBytes(key);

                byte[] iv = new byte[aes.BlockSize / 8];
                byte[] cipherBytes = new byte[encodedJsonBytes.Length - iv.Length];

                Array.Copy(encodedJsonBytes, iv, iv.Length);
                Array.Copy(encodedJsonBytes, iv.Length, cipherBytes, 0, cipherBytes.Length);

                aes.IV = iv;
                aes.Mode = CipherMode.CBC;

                var decrypter = aes.CreateDecryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream(cipherBytes))
                {
                    using (var cs = new CryptoStream(ms, decrypter, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        public object Encrypt(object value)
        {
            byte[] iv;
            byte[] encryptedField;

            var key = KeyStore.GetKey(KeyName);
            var valueToEncrypt = value.ToString();

            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.Unicode.GetBytes(key);
                aes.GenerateIV();
                iv = aes.IV;

                aes.Mode = CipherMode.CBC;
                var encrypter = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encrypter, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(valueToEncrypt);
                    }
                    encryptedField = ms.ToArray();
                }
            }

            var combined = new byte[iv.Length + encryptedField.Length];
            Array.Copy(iv, 0, combined, 0, iv.Length);
            Array.Copy(encryptedField, 0, combined, iv.Length, encryptedField.Length);

            return Convert.ToBase64String(combined);
        }

        public string KeyName { get; set; }

        public string Name { get; private set; }
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
