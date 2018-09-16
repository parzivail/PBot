using Newtonsoft.Json;

namespace DiscordPBot.RainbowSix
{
    public class OperatorStats
    {
        [JsonProperty("kills")] public int Kills { get; set; }

        [JsonProperty("deaths")] public int Deaths { get; set; }

        [JsonProperty("kd")] public float Kd { get; set; }

        [JsonProperty("wins")] public int Wins { get; set; }

        [JsonProperty("losses")] public int Losses { get; set; }

        [JsonProperty("wl")] public float? Wl { get; set; }

        [JsonProperty("headshots")] public int Headshots { get; set; }

        [JsonProperty("dbnos")] public int Dbnos { get; set; }

        [JsonProperty("melee_kills")] public int MeleeKills { get; set; }

        [JsonProperty("experience")] public int Experience { get; set; }

        [JsonProperty("playtime")] public int Playtime { get; set; }

        [JsonProperty("abilities")] public Ability[] Abilities { get; set; }

        [JsonProperty("operator")] public Operator Operator { get; set; }
    }
}