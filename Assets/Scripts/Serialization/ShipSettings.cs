using System;
using Newtonsoft.Json;

namespace Kiraio.Azure.Serialization
{
    [Serializable]
    public class ShipSettings
    {
        [JsonProperty("voice")] public Voice Voice;

        [JsonProperty("bgm")] public Bgm Bgm;
    }

    [Serializable]
    public class Voice
    {
        public float Volume = 1f;
        public bool Mute;
        public string Directory;
    }

    [Serializable]
    public class Bgm
    {
        public float Volume = 0.7f;
        public bool Mute;
        public string File;
    }
}
