using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Couchbase.Extensions.Encryption.Stores
{
    public class FileSystemKeyStore : IKeystoreProvider
    {
        public DataProtectionScope ProtectionScope { get; set; }

        public string StorePath { get; set; }

        public string GetKey(string keyname)
        {
            using (var stream = new FileStream(GetPath(keyname), FileMode.Open))
            {
                var encryptedBytes = new byte[stream.Length];
                stream.Read(encryptedBytes, 0, (int)stream.Length);

                var entropy = Encoding.ASCII.GetBytes(keyname);
                var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, entropy, ProtectionScope);
                return Encoding.ASCII.GetString(decryptedBytes);
            }
        }

        public void StoreKey(string keyname, string key)
        {
            using (var stream = new FileStream(GetPath(keyname), FileMode.OpenOrCreate))
            {
                var userData = Encoding.ASCII.GetBytes(key);
                var entropy = Encoding.ASCII.GetBytes(keyname);
                var encryptedBytes = ProtectedData.Protect(userData, entropy, ProtectionScope);

                if (stream.CanWrite && encryptedBytes != null)
                {
                    stream.Write(encryptedBytes, 0, encryptedBytes.Length);
                }
            }
        }

        internal string GetPath(string storeName)
        {
            if (!storeName.Contains(".dat"))
            {
                storeName = string.Concat(storeName, ".dat");
            }
            return StorePath == null ? storeName : Path.Combine(StorePath, storeName);
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
