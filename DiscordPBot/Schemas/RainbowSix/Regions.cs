using Newtonsoft.Json;

namespace DiscordPBot.Schemas.RainbowSix
{
    public class Regions
    {
        [JsonProperty("ncsa")]
        public Ncsa[] Ncsa { get; set; }
        [JsonProperty("emea")]
        public Emea[] Emea { get; set; }
        [JsonProperty("apac")]
        public Apac[] Apac { get; set; }
    }
}