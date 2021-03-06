using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    class WellKnownKey
    {
        [Preserve]
        public WellKnownKey() {}

        [JsonProperty("use")]
        public string Use;

        [JsonProperty("kty")]
        public string KeyType;

        [JsonProperty("kid")]
        public string KeyId;

        [JsonProperty("alg")]
        public string Algorithm;

        [JsonProperty("n")]
        public string N;

        [JsonProperty("e")]
        public string E;
    }
}
