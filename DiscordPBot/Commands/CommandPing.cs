using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DiscordPBot.Commands
{
    internal class CommandPing
    {
        [Command("ping")]
        [Description("Check the status of the bot")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync($"Ping: {ctx.Client.Ping}ms");
        }
    }
}