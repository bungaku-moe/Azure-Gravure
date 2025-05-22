using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kiraio.Azure.Serialization
{
    [Serializable]
    public class SkinData
    {
        [JsonProperty("gid")] public int? Gid { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("skins")] public List<Skin> Skins { get; } = new();
    }

    public class Skin
    {
        [JsonProperty("id")] public int? Id { get; set; }

        [JsonProperty("gid")] public int? Gid { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("desc")] public string Desc { get; set; }

        [JsonProperty("tag")] public List<string> Tag { get; } = new();

        [JsonProperty("illustrator")] public int? Illustrator { get; set; }

        [JsonProperty("illustrator2")] public int? Illustrator2 { get; set; }

        [JsonProperty("voice_actor")] public int? VoiceActor { get; set; }

        [JsonProperty("voice_actor2")] public int? VoiceActor2 { get; set; }

        [JsonProperty("bgm")] public object Bgm { get; set; }

        [JsonProperty("background")] public object Background { get; set; }

        [JsonProperty("background2")] public object Background2 { get; set; }

        [JsonProperty("painting")] public string Painting { get; set; }

        [JsonProperty("painting_n")] public object PaintingN { get; set; }

        [JsonProperty("banner")] public string Banner { get; set; }

        [JsonProperty("chibi")] public string Chibi { get; set; }

        [JsonProperty("icon")] public string Icon { get; set; }

        [JsonProperty("qicon")] public string Qicon { get; set; }

        [JsonProperty("shipyard")] public string Shipyard { get; set; }
    }
}
