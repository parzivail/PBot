using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DiscordPBot.Schemas.RainbowSix
{
    public class R6PlayerSeasonStats
    {
        [JsonProperty("username")]
		public string Username { get; set; }
        [JsonProperty("ubisoft_id")]
		public string UbisoftId { get; set; }
        [JsonProperty("last_updated")]
		public DateTime LastUpdated { get; set; }
        [JsonProperty("seasons")]
		public Dictionary<string, Season> Seasons { get; set; }
    }
}
