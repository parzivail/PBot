using Newtonsoft.Json;

namespace DiscordPBot.Schemas.RainbowSix
{
    public class Gamemode
    {
        [JsonProperty("bomb")] public Bomb Bomb { get; set; }

        [JsonProperty("secure_area")] public SecureArea SecureArea { get; set; }

        [JsonProperty("hostage")] public Hostage Hostage { get; set; }
    }
}