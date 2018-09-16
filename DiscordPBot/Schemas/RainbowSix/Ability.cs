using Newtonsoft.Json;

namespace DiscordPBot.Schemas.RainbowSix
{
    public class Ability
    {
        [JsonProperty("key")] public string Key { get; set; }

        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("value")] public int Value { get; set; }
    }
}