using System;
using Newtonsoft.Json;

namespace Kiraio.Azure.Serialization
{
    [Serializable]
    public class MainConfig
    {
        [JsonProperty("shipData")] public string ShipDataFile;

        [JsonProperty("skinData")] public string ShipSkinFile;

        [JsonProperty("voicelinesData")] public string VoicelinesFile;

        [JsonProperty("ship")] public string ShipModelJson;
    }
}
