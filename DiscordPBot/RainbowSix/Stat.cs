using Newtonsoft.Json;

namespace DiscordPBot.RainbowSix
{
    public class Stat
    {
        [JsonProperty("general")] public General General { get; set; }

        [JsonProperty("queue")] public Queue Queue { get; set; }

        [JsonProperty("gamemode")] public Gamemode Gamemode { get; set; }

        [JsonProperty("timestamps")] public Timestamps Timestamps { get; set; }
    }
}