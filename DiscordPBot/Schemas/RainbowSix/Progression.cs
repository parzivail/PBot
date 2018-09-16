using Newtonsoft.Json;

namespace DiscordPBot.Schemas.RainbowSix
{
    public class Progression
    {
        [JsonProperty("level")] public int Level { get; set; }

        [JsonProperty("lootbox_probability")] public int LootboxProbability { get; set; }

        [JsonProperty("total_xp")] public int TotalXp { get; set; }
    }
}