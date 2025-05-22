using System;
using Newtonsoft.Json;

namespace Kiraio.Azure.Serialization
{
    [Serializable]
    public class ShipData
    {
        [JsonProperty("Ship")] public string Ship { get; set; }

        [JsonProperty("Skin")] public string Skin { get; set; }

        [JsonProperty("Cubism")] public string Cubism { get; set; }
        [JsonProperty("Has Censored")] public bool HasCensored { get; set; }
    }
}
