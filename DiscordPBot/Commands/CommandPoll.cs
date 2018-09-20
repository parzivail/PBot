using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DiscordPBot.Commands
{
    internal class CommandPoll
    {
        [Command("poll")]
        [Description("Polls for opinions as reactions to PBot's embed message")]
        public async Task D(CommandContext ctx, [RemainingText] string message)
        {
            await ctx.TriggerTypingAsync();

            var rawOptions = message.Split("|").Select(s => s.Trim()).ToArray();

            var sb = new StringBuilder();
            for (var i = 0; i < Math.Min(26, rawOptions.Length); i++)
            {
                var indicator = (char)('a' + i);
                sb.Append($":regional_indicator_{indicator}: {rawOptions[i]}\n");
            }

            var embed = new DiscordEmbedBuilder()
                .AddField("Poll", sb.ToString());

            var msg = await ctx.RespondAsync(embed: embed);

            for (var i = 0; i < Math.Min(26, rawOptions.Length); i++)
            {
                var indicator = (char)('a' + i);
                await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, $":regional_indicator_{indicator}:"));
            }
        }
    }
}