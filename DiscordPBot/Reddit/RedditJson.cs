using Newtonsoft.Json;

namespace DiscordPBot.Reddit
{
    public class RedditJson
    {
        [JsonProperty("data")] public RedditPayload Subreddit { get; set; }
    }
}