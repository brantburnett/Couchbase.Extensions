
namespace Couchbase.Extensions.Encryption
{
    /// <summary>
    /// Provides an interface for implementing a cryptographic algorithm for Field Level Encryption.
    /// </summary>
    public interface ICryptoProvider
    {
        /// <summary>
        /// The key store to retrieve the keys used for encryption and signing the encrypted data if required.
        /// </summary>
        IKeystoreProvider KeyStore { get; set; }

        /// <summary>
        /// Decrypts a byte array.
        /// </summary>
        /// <param name="encryptedBytes">A base64 encoded byte array.</param>
        /// <param name="iv">The initialization vector to use.</param>
        /// <param name="keyName">The key name that will be used to look up the encryption key from the <see cref="IKeystoreProvider"/>.</param>
        /// <returns>A decrypted, UTF8 encoded byte array.</returns>
        byte[] Decrypt(byte[] encryptedBytes, byte[] iv, string keyName = null);

        /// <summary>
        /// Encrypts a UTF8 byte array.
        /// </summary>
        /// <param name="plainBytes">A UTF8 encoded byte array.</param>
        /// <param name="iv">The initialization vector to use.</param>
        /// <returns>An encrypted UTF8 byte array.</returns>
        byte[] Encrypt(byte[] plainBytes, out byte[] iv);

        /// <summary>
        /// Generates a signature from a byte array.
        /// </summary>
        /// <param name="cipherBytes">The byte array used to generate the signature from.</param>
        /// <returns></returns>
        byte[] GetSignature(byte[] cipherBytes);

        /// <summary>
        /// The name of the configured <see cref="ICryptoProvider"/> - for example 'MyProvider'.
        /// </summary>
        string ProviderName { get; set; }

        /// <summary>
        /// The name of the encryption key.
        /// </summary>
        string PublicKeyName { get; set; }

        /// <summary>
        /// The name of the private if required for an asymmetric algorithm
        /// </summary>
        string PrivateKeyName { get; set; }

        /// <summary>
        /// The name of the password or key used for signing if required.
        /// </summary>
        string SigningKeyName { get; set; }

        /// <summary>
        /// True if the algorithm requires a signature to be generated and compared.
        /// </summary>
        bool RequiresAuthentication { get; }
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
