using System;
using Newtonsoft.Json;

namespace DiscordPBot.Schemas.RainbowSix
{
    public class Season
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("start_date")]
        public DateTime? StartDate { get; set; }
        [JsonProperty("end_date")]
        public DateTime? EndDate { get; set; }
        [JsonProperty("rankings")]
        public Rankings Rankings { get; set; }
        [JsonProperty("regions")]
        public Regions Regions { get; set; }
    }
}