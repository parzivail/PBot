using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscordPBot.RainbowSix;
using DiscordPBot.Reddit;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;

namespace DiscordPBot
{
    class PCommands
    {
        [Command("ping"), Description("Check the status of the bot")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            
            await ctx.RespondAsync($"Ping: {ctx.Client.Ping}ms");
        }

        [Command("doimg"), Description("DEBUG COMMAND"), Hidden]
        public async Task DoImg(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            using (var bmp = new Bitmap(256, 256))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);

                    g.DrawLine(Pens.Black, 5, 5, 251, 251);
                }

                using (var ms = new MemoryStream(bmp.ToBytes()))
                    await ctx.RespondWithFileAsync(ms, "image.png");
            }
        }

        [Command("peekr"), Description("Gets this year's top 3 posts of a particular subreddit.")]
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

                var embed = new DiscordEmbedBuilder
                {
                    Color = PDiscordColor.RedditOrange,
                    Description = desc
                }.Build();

                await ctx.RespondAsync(embed: embed);
            }
        }

        [Command("r6"), Description("Get stats about a player on PC.")]
        public async Task Rainbow6(CommandContext ctx, string username)
        {
            await ctx.TriggerTypingAsync();

            using (var wc = new WebClient())
            {
                R6PlayerSearchJson[] searchResults;

                try
                {
                    var reqUrl = $"https://www.r6stats.com/api/player-search/{username}/pc";
                    var json = wc.DownloadString(reqUrl);
                    searchResults = JsonConvert.DeserializeObject<R6PlayerSearchJson[]>(json);
                }
                catch (WebException e)
                {
                    PBot.LogError($"r6 search WebException: {e.Message}");
                    await ctx.RespondAsync(":interrobang: Could not fetch players.");
                    return;
                }
                catch (JsonSerializationException e)
                {
                    PBot.LogError($"r6 search JsonSerializationException: {e.Message}");
                    await ctx.RespondAsync(":interrobang: Could not load players.");
                    return;
                }

                if (searchResults.Length == 0)
                {
                    await ctx.RespondAsync(":warning: No players found with that username.");
                    return;
                }

                R6PlayerStatsJson playerStats;

                try
                {
                    var reqUrl = $"https://www.r6stats.com/api/stats/{searchResults[0].UbisoftId}";
                    var json = wc.DownloadString(reqUrl);
                    playerStats = JsonConvert.DeserializeObject<R6PlayerStatsJson>(json);
                }
                catch (WebException e)
                {
                    PBot.LogError($"r6 stats WebException: {e.Message}");
                    await ctx.RespondAsync(":interrobang: Could not fetch player stats.");
                    return;
                }
                catch (JsonSerializationException e)
                {
                    PBot.LogError($"r6 stats JsonSerializationException: {e.Message}");
                    await ctx.RespondAsync(":interrobang: Could not load player stats.");
                    return;
                }

                await ctx.RespondAsync($"{playerStats.Username}'s KD: {playerStats.Stats[0].General.Kd}");
            }
        }
    }
}
