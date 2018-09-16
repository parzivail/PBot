using Newtonsoft.Json;

namespace DiscordPBot.Schemas.Reddit
{
    public class RedditPost
    {
        [JsonProperty("data")] public RedditPostPayload PostData { get; set; }
    }
}