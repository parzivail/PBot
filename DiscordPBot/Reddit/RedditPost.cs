using Newtonsoft.Json;

namespace DiscordPBot.Reddit
{
    public class RedditPost
    {
        [JsonProperty("data")] public RedditPostPayload PostData { get; set; }
    }
}