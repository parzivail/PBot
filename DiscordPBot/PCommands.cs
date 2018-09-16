using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

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

        [Command("doimg"), Description("DEBUG COMMAND")]
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
            else
            {
                var reqUrl = $"https://www.reddit.com/r/{match.Groups[0].Value}/top.json?sort=top&t=year";
            }
        }
    }
}
