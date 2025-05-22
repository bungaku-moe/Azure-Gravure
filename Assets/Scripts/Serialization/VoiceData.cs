using System;
using Newtonsoft.Json;

namespace Kiraio.Azure.Serialization
{
    [Serializable]
    public class VoiceData
    {
        [JsonProperty("link")] public string Link { get; set; }

        [JsonProperty("line")] public string Line { get; set; }
    }
}
