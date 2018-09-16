using Newtonsoft.Json;

namespace DiscordPBot.Schemas.Reddit
{
    public class RedditPayload
    {
        [JsonProperty("children")] public RedditPost[] Posts { get; set; }
    }
}