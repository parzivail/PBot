using System;
using Newtonsoft.Json;

namespace DiscordPBot.RainbowSix
{
    public class Alias
    {
        [JsonProperty("username")] public string Username { get; set; }

        [JsonProperty("last_seen_at")] public DateTime LastSeenAt { get; set; }
    }
}