using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kiraio.Azure.Serialization
{
    [Serializable]
    public class AudioEventData
    {
        [JsonProperty("motion")] public string Motion;

        [JsonProperty("Events")] public List<AudioEvent> Events;
    }

    public class AudioEvent
    {
        [JsonProperty("audio")] public string Audio;

        [JsonProperty("time")] public float Time;
    }
}
