using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscordPBot.Reddit;
using DiscordPBot.Util;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DiscordPBot.Commands
{
    internal partial class PCommands
    {
        [Command("peekr")]
        [Description("Gets this year's top 3 posts of a particular subreddit.")]
        public async Task Peekr(CommandContext ctx, string subreddit)
        {
            await ctx.TriggerTypingAsync();

            var regexGetSubreddit = new Regex(@"(?:.+\/?r?\/|^)([A-Za-z0-9_]{3,21})$");
            var match = regexGetSubreddit.Match(subreddit);
            if (!match.Success)
            {
                await ctx.RespondAsync(":x: Invalid subreddit.");
                return;
            }

            subreddit = match.Groups[1].Value;

            var reqUrl = $"https://www.reddit.com/r/{subreddit}/top.json?sort=top&t=year&limit=3";
            using (var wc = new WebClient())
            {
                RedditJson posts;

                try
                {
                    var json = wc.DownloadString(reqUrl);
                    posts = JsonConvert.DeserializeObject<RedditJson>(json);
                }
                catch (WebException e)
                {
                    PBot.LogError($"peekr WebException: {e.Message}");
                    await ctx.RespondAsync(":interrobang: Could not fetch posts.");
                    return;
                }
                catch (JsonSerializationException e)
                {
                    PBot.LogError($"peekr JsonSerializationException: {e.Message}");
                    await ctx.RespondAsync(":interrobang: Could not load posts.");
                    return;
                }

                if (posts.Subreddit.Posts.Length == 0)
                {
                    await ctx.RespondAsync(":warning: No posts in subreddit.");
                    return;
                }

                var nsfw = posts.Subreddit.Posts[0].PostData.Nsfw;

                var desc =
                    $"**Check out [/r/{subreddit}](https://np.reddit.com/r/{subreddit})'s{(nsfw ? " [NSFW]" : " ")} [top posts](https://np.reddit.com/r/{subreddit}/top/?sort=top&t=year) this year:**\n";

                for (var i = 0; i < posts.Subreddit.Posts.Length; i++)
                {
                    var post = posts.Subreddit.Posts[i];
                    desc +=
                        $"\\#{i + 1}: [{post.PostData.Title}]({post.PostData.Link}) ([{post.PostData.NumComments} comment{(post.PostData.NumComments == 1 ? "" : "s")}](https://np.reddit.com{post.PostData.CommentsLink}))\n";
                }

                var embed = new DiscordEmbedBuilder()
                    .WithColor(PDiscordColor.RedditOrange)
                    .WithDescription(desc);

                await ctx.RespondAsync(embed: embed);
            }
        }
    }
}