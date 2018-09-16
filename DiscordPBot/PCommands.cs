using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
                    await ctx.RespondAsync(":interrobang: Could not fetch posts.");
                    return;
                }
                catch (JsonSerializationException)
                {
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
                    $"**Here's a sneak peek of [/r/{subreddit}](https://np.reddit.com/r/{subreddit}){(nsfw ? " [NSFW]" : " ")} using the [top posts](https://np.reddit.com/r/{subreddit}/top/?sort=top&t=year) of the year!**\n";

                for (var i = 0; i < posts.Subreddit.Posts.Length; i++)
                {
                    var post = posts.Subreddit.Posts[i];
                    desc +=
                        $"\\#{i + 1}: [{post.PostData.Title}]({post.PostData.Link}) | [{post.PostData.NumComments} comment{(post.PostData.NumComments == 1 ? "" : "s")}](https://np.reddit.com{post.PostData.CommentsLink})\n";
                }

                var embed = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Orange,
                    Description = desc
                }.Build();

                await ctx.RespondAsync(embed: embed);
            }
        }
    }
}
