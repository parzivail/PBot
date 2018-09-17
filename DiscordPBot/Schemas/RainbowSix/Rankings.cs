using Newtonsoft.Json;

namespace DiscordPBot.Schemas.RainbowSix
{
    public class Rankings
    {
        [JsonProperty("ncsa")]
        public object Ncsa { get; set; }
        [JsonProperty("emea")]
        public object Emea { get; set; }
        [JsonProperty("apac")]
        public object Apac { get; set; }
        [JsonProperty("global")]
        public object Global { get; set; }
    }
}