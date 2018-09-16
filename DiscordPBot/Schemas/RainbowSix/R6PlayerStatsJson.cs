using System;
using Newtonsoft.Json;

namespace DiscordPBot.Schemas.RainbowSix
{
    public class R6PlayerStatsJson
    {
        [JsonProperty("username")] public string Username { get; set; }

        [JsonProperty("platform")] public string Platform { get; set; }

        [JsonProperty("ubisoft_id")] public string UbisoftId { get; set; }

        [JsonProperty("last_updated")] public DateTime LastUpdated { get; set; }

        [JsonProperty("aliases")] public Alias[] Aliases { get; set; }

        [JsonProperty("progression")] public Progression Progression { get; set; }

        [JsonProperty("stats")] public Stat[] Stats { get; set; }

        [JsonProperty("operators")] public OperatorStats[] Operators { get; set; }
    }
}