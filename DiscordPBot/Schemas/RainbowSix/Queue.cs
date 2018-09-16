using Newtonsoft.Json;

namespace DiscordPBot.Schemas.RainbowSix
{
    public class Queue
    {
        [JsonProperty("casual")] public Casual Casual { get; set; }

        [JsonProperty("ranked")] public Ranked Ranked { get; set; }
    }
}