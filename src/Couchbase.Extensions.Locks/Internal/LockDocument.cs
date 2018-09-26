using System;
using Couchbase.Core.Serialization;
using Newtonsoft.Json;

namespace Couchbase.Extensions.Locks.Internal
{
    internal class LockDocument
    {
        private const string LockPrefix = "__lock_";

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "holder")]
        public string Holder { get; set; }

        [JsonConverter(typeof(UnixMillisecondsConverter))]
        [JsonProperty(PropertyName = "requestedDateTime")]
        public DateTime RequestedDateTime { get; set; }

        public static string GetKey(string name)
        {
            return LockPrefix + name;
        }
    }
}
