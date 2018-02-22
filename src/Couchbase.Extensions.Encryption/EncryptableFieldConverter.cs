using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Couchbase.Extensions.Encryption
{
    public class EncryptableFieldConverter : JsonConverter
    {
        public EncryptableFieldConverter(PropertyInfo targetProperty, Dictionary<string, ICryptoProvider> cryptoProviders, string providerName)
        {
            TargetProperty = targetProperty;
            CryptoProviders = cryptoProviders;
            ProviderName = providerName;
        }

        public PropertyInfo TargetProperty { get; }

        public Dictionary<string, ICryptoProvider> CryptoProviders { get; set; }

        public string ProviderName { get; set; }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var rawJson = SerializeAsJson(value);
            var cryptoProvider = CryptoProviders[ProviderName];

            var token = new JObject(
                new JProperty("alg", cryptoProvider.Name),
                new JProperty("kid", cryptoProvider.KeyName),
                new JProperty("payload", cryptoProvider.Encrypt(rawJson)));

            token.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var encryptedFields = (JObject)JToken.ReadFrom(reader);
            var alg = encryptedFields.SelectToken("alg");
            var kid = encryptedFields.SelectToken("kid");
            var payload = encryptedFields.SelectToken("payload");

            var cryptoProvider =  CryptoProviders[alg.Value<string>()];
            var decryptedPayload = cryptoProvider.Decrypt(payload, kid.Value<string>());
            return ConvertToType(decryptedPayload.ToString());
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        private string SerializeAsJson(object value)
        {
            var typeCode = Type.GetTypeCode(TargetProperty.PropertyType);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.DateTime:
#if NET45
                case TypeCode.DBNull:
#endif
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Empty:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Object:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return JsonConvert.SerializeObject(value);
                case TypeCode.String:
                    return value.ToString();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private object ConvertToType(string decryptedValue)
        {
            var typeCode = Type.GetTypeCode(TargetProperty.PropertyType);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return Convert.ToBoolean(decryptedValue);
                case TypeCode.Byte:
                    return Convert.ToByte(decryptedValue);
                case TypeCode.Char:
                    return Convert.ToChar(decryptedValue);
                case TypeCode.DateTime:
                    return Convert.ToDateTime(decryptedValue);
#if NET45
                case TypeCode.DBNull:
                    return null;
#endif
                case TypeCode.Decimal:
                    return Convert.ToDecimal(decryptedValue);
                case TypeCode.Double:
                    return Convert.ToDouble(decryptedValue);
                case TypeCode.Empty:
                    return null;
                case TypeCode.Int16:
                    return Convert.ToInt16(decryptedValue);
                case TypeCode.Int32:
                    return Convert.ToInt32(decryptedValue);
                case TypeCode.Int64:
                    return Convert.ToInt64(decryptedValue);
                case TypeCode.Object:
                    return JsonConvert.DeserializeObject(decryptedValue, TargetProperty.PropertyType);
                case TypeCode.SByte:
                    return Convert.ToSByte(decryptedValue);
                case TypeCode.Single:
                    return Convert.ToSingle(decryptedValue);
                case TypeCode.String:
                    return decryptedValue;
                case TypeCode.UInt16:
                    return Convert.ToUInt16(decryptedValue);
                case TypeCode.UInt32:
                    return Convert.ToUInt32(decryptedValue);
                case TypeCode.UInt64:
                    return Convert.ToUInt64(decryptedValue);
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
