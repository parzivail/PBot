using Newtonsoft.Json;

namespace DiscordPBot.RainbowSix
{
    public class Bomb
    {
        [JsonProperty("best_score")]
        public int BestScore { get; set; }
        [JsonProperty("games_played")]
        public int GamesPlayed { get; set; }
        [JsonProperty("losses")]
        public int Losses { get; set; }
        [JsonProperty("playtime")]
        public int Playtime { get; set; }
        [JsonProperty("wins")]
        public int Wins { get; set; }
        [JsonProperty("wl")]
        public float Wl { get; set; }
    }
}