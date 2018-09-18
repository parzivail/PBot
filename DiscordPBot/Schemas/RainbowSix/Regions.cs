using Newtonsoft.Json;

namespace DiscordPBot.Schemas.RainbowSix
{
    public class Regions
    {
        [JsonProperty("ncsa")]
        public Region[] Ncsa { get; set; }
        [JsonProperty("emea")]
        public Region[] Emea { get; set; }
        [JsonProperty("apac")]
        public Region[] Apac { get; set; }
    }
}