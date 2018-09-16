using Newtonsoft.Json;

namespace DiscordPBot.Reddit
{
    public class RedditPayload
    {
        [JsonProperty("children")]
        public RedditPost[] Posts { get; set; }
    }
}