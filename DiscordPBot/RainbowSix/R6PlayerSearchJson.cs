using System;
using Newtonsoft.Json;

namespace DiscordPBot.RainbowSix
{
    public class R6PlayerSearchJson
    {
        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("username")] public string Username { get; set; }

        [JsonProperty("platform")] public string Platform { get; set; }

        [JsonProperty("ubisoft_id")] public string UbisoftId { get; set; }

        [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")] public DateTime UpdatedAt { get; set; }
    }
}