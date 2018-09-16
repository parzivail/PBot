using Newtonsoft.Json;

namespace DiscordPBot.Reddit
{
    public class RedditPostPayload
    {
        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("over_18")] public bool Nsfw { get; set; }

        [JsonProperty("permalink")] public string CommentsLink { get; set; }

        [JsonProperty("num_comments")] public int NumComments { get; set; }

        [JsonProperty("url")] public string Link { get; set; }
    }
}