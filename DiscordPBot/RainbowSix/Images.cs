using Newtonsoft.Json;

namespace DiscordPBot.RainbowSix
{
    public class Images
    {
        [JsonProperty("badge")] public string Badge { get; set; }

        [JsonProperty("bust")] public string Bust { get; set; }

        [JsonProperty("figure")] public string Figure { get; set; }
    }
}