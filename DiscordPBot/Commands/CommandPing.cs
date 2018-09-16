using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscordPBot.RainbowSix;
using DiscordPBot.Reddit;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Humanizer.Localisation;
using Newtonsoft.Json;

namespace DiscordPBot.Commands
{
    partial class PCommands
    {
        [Command("ping"), Description("Check the status of the bot")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync($"Ping: {ctx.Client.Ping}ms");
        }
    }
}
