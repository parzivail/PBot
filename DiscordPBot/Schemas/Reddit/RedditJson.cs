using Newtonsoft.Json;

namespace DiscordPBot.Schemas.Reddit
{
    public class RedditJson
    {
        [JsonProperty("data")] public RedditPayload Subreddit { get; set; }
    }
}