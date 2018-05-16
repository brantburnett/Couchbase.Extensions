using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Couchbase.Extensions.Encryption
{
    public class EncryptableFieldConverter : JsonConverter
    {
        public EncryptableFieldConverter(PropertyInfo targetProperty, Dictionary<string, ICryptoProvider> cryptoProviders, string providerName)
        {
            TargetProperty = targetProperty;
            CryptoProviders = cryptoProviders;
            ProviderName = providerName;
            SerializerSettings =
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
        }

        public PropertyInfo TargetProperty { get; }

        public Dictionary<string, ICryptoProvider> CryptoProviders { get; set; }

        public string ProviderName { get; set; }

        public JsonSerializerSettings SerializerSettings { get; set; }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var rawJson = JsonConvert.SerializeObject(value, SerializerSettings);
            var cryptoProvider = CryptoProviders[ProviderName];

            var rawBytes = Encoding.UTF8.GetBytes(rawJson);
            var cipherText = cryptoProvider.Encrypt(rawBytes, out var iv);
            var base64CipherText = Convert.ToBase64String(cipherText);

            string base64Iv = null;
            if (iv != null)
            {
                base64Iv = Convert.ToBase64String(iv);
            }

            byte[] signatureBytes = null;
            if (cryptoProvider.RequiresAuthentication)
            {
                //sig = HMAC256(BASE64(alg + iv + ciphertext))
                var algBytes = Encoding.UTF8.GetBytes(cryptoProvider.ProviderName);
                var buffer = new byte[algBytes.Length + iv.Length + cipherText.Length];

                Buffer.BlockCopy(algBytes, 0, buffer, 0, algBytes.Length);
                Buffer.BlockCopy(iv, 0, buffer, algBytes.Length, iv.Length);
                Buffer.BlockCopy(cipherText, 0, buffer, algBytes.Length + iv.Length, cipherText.Length);

                //sign the entire buffer
                signatureBytes = cryptoProvider.GetSignature(buffer);
            }

            var token = new JObject(
                new JProperty("alg", cryptoProvider.ProviderName),
                new JProperty("ciphertext", base64CipherText));

            if (signatureBytes != null)
            {
                var base64Sig = Convert.ToBase64String(signatureBytes);
                token.Add("sig", base64Sig);
            }
            if (iv != null && !string.IsNullOrWhiteSpace(base64Iv))
            {
                token.Add("iv", base64Iv);
            }

            token.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            var encryptedFields = (JObject)JToken.ReadFrom(reader);
            var alg = encryptedFields.SelectToken("alg");
            var cipherText = encryptedFields.SelectToken("ciphertext");
            var iv = encryptedFields.SelectToken("iv");
            var signature = encryptedFields.SelectToken("sig");

            var cryptoProvider = CryptoProviders[ProviderName];

            var cipherBytes = Convert.FromBase64String(cipherText.Value<string>());
            byte[] ivBytes = null;
            if (iv != null)
            {
                ivBytes = Convert.FromBase64String(iv.Value<string>());
            }

            if (signature != null && ivBytes != null)
            {
                //sig = BASE64(HMAC256(alg + BASE64(iv) + BASE64(ciphertext)))
                var algBytes = Encoding.UTF8.GetBytes(alg.Value<string>());

                var buffer = new byte[algBytes.Length + ivBytes.Length + cipherBytes.Length];
                Buffer.BlockCopy(algBytes, 0, buffer, 0, algBytes.Length);
                Buffer.BlockCopy(ivBytes, 0, buffer, algBytes.Length, ivBytes.Length);
                Buffer.BlockCopy(cipherBytes, 0, buffer, algBytes.Length + ivBytes.Length, cipherBytes.Length);

                var sig = cryptoProvider.GetSignature(buffer);
                if (signature.Value<string>() != Convert.ToBase64String(sig))
                {
                    throw new AuthenticationException("signatures do not match!");
                }
            }

            var decryptedPayload = cryptoProvider.Decrypt(cipherBytes, ivBytes);
            return ConvertToType(Encoding.UTF8.GetString(decryptedPayload));
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        private object ConvertToType(string decryptedValue)
        {
            var typeCode = Type.GetTypeCode(TargetProperty.PropertyType);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return JsonConvert.DeserializeObject<bool>(decryptedValue);
                case TypeCode.Byte:
                    return JsonConvert.DeserializeObject<byte>(decryptedValue);
                case TypeCode.Char:
                    return JsonConvert.DeserializeObject<char>(decryptedValue);
                case TypeCode.DateTime:
                    return JsonConvert.DeserializeObject<DateTime>(decryptedValue);
#if NET45
                case TypeCode.DBNull:
                    return null;
#endif
                case TypeCode.Decimal:
                    return JsonConvert.DeserializeObject<Decimal>(decryptedValue);
                case TypeCode.Double:
                    return JsonConvert.DeserializeObject<double>(decryptedValue);
                case TypeCode.Empty:
                    return null;
                case TypeCode.Int16:
                    return JsonConvert.DeserializeObject<short>(decryptedValue);
                case TypeCode.Int32:
                    return JsonConvert.DeserializeObject<int>(decryptedValue);
                case TypeCode.Int64:
                    return JsonConvert.DeserializeObject<long>(decryptedValue);
                case TypeCode.Object:
                    return JsonConvert.DeserializeObject(decryptedValue, TargetProperty.PropertyType);
                case TypeCode.SByte:
                    return JsonConvert.DeserializeObject<sbyte>(decryptedValue);
                case TypeCode.Single:
                    return JsonConvert.DeserializeObject<float>(decryptedValue);
                case TypeCode.String:
                    return JsonConvert.DeserializeObject<string>(decryptedValue);
                case TypeCode.UInt16:
                    return JsonConvert.DeserializeObject<ushort>(decryptedValue);
                case TypeCode.UInt32:
                    return JsonConvert.DeserializeObject<uint>(decryptedValue);
                case TypeCode.UInt64:
                    return JsonConvert.DeserializeObject<ulong>(decryptedValue);
            }
            return null;
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
