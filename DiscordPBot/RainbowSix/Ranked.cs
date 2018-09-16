using Newtonsoft.Json;

namespace DiscordPBot.RainbowSix
{
    public class Ranked
    {
        [JsonProperty("deaths")] public int Deaths { get; set; }

        [JsonProperty("games_played")] public int GamesPlayed { get; set; }

        [JsonProperty("kd")] public float Kd { get; set; }

        [JsonProperty("kills")] public int Kills { get; set; }

        [JsonProperty("losses")] public int Losses { get; set; }

        [JsonProperty("playtime")] public int Playtime { get; set; }

        [JsonProperty("wins")] public int Wins { get; set; }

        [JsonProperty("wl")] public float Wl { get; set; }
    }
}