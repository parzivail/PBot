using Newtonsoft.Json;

namespace DiscordPBot.RainbowSix
{
    public class Operator
    {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("internal_name")] public string InternalName { get; set; }

        [JsonProperty("role")] public string Role { get; set; }

        [JsonProperty("ctu")] public string Ctu { get; set; }

        [JsonProperty("images")] public Images Images { get; set; }
    }
}