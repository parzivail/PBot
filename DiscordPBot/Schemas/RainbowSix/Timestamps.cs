using System;
using Newtonsoft.Json;

namespace DiscordPBot.Schemas.RainbowSix
{
    public class Timestamps
    {
        [JsonProperty("created")] public DateTime Created { get; set; }

        [JsonProperty("last_updated")] public DateTime LastUpdated { get; set; }
    }
}