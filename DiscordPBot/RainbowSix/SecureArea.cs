using Newtonsoft.Json;

namespace DiscordPBot.RainbowSix
{
    public class SecureArea
    {
        [JsonProperty("best_score")] public int BestScore { get; set; }

        [JsonProperty("games_played")] public int GamesPlayed { get; set; }

        [JsonProperty("kills_as_attacker_in_objective")]
        public int KillsAsAttackerInObjective { get; set; }

        [JsonProperty("kills_as_defender_in_objective")]
        public int KillsAsDefenderInObjective { get; set; }

        [JsonProperty("losses")] public int Losses { get; set; }

        [JsonProperty("playtime")] public int Playtime { get; set; }

        [JsonProperty("times_objective_secured")]
        public int TimesObjectiveSecured { get; set; }

        [JsonProperty("wins")] public int Wins { get; set; }

        [JsonProperty("wl")] public float Wl { get; set; }
    }
}