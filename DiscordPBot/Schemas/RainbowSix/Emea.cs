using System;
using Newtonsoft.Json;

namespace DiscordPBot.Schemas.RainbowSix
{
    public class Emea
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("season_id")]
        public int SeasonId { get; set; }
        [JsonProperty("region")]
        public string Region { get; set; }
        [JsonProperty("abandons")]
        public int Abandons { get; set; }
        [JsonProperty("losses")]
        public int Losses { get; set; }
        [JsonProperty("max_mmr")]
        public int MaxMmr { get; set; }
        [JsonProperty("max_rank")]
        public int MaxRank { get; set; }
        [JsonProperty("mmr")]
        public int Mmr { get; set; }
        [JsonProperty("next_rank_mmr")]
        public int NextRankMmr { get; set; }
        [JsonProperty("prev_rank_mmr")]
        public int PrevRankMmr { get; set; }
        [JsonProperty("rank")]
        public int Rank { get; set; }
        [JsonProperty("skill_mean")]
        public int SkillMean { get; set; }
        [JsonProperty("skill_standard_deviation")]
        public float SkillStandardDeviation { get; set; }
        [JsonProperty("created_for_date")]
        public DateTime CreatedForDate { get; set; }
        [JsonProperty("wins")]
        public int Wins { get; set; }
    }
}