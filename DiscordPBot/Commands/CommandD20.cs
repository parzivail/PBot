using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DiscordPBot.Commands
{
    internal partial class PCommands
    {
        [Command("d")]
        [Description("Rolls an n sided die")]
        public async Task D(CommandContext ctx, int sides)
        {
            await ctx.TriggerTypingAsync();

            var roll = PBot.Rng.Next(0, sides) + 1;

            await ctx.RespondAsync($"You rolled {(roll == sides ? "a perfect " : "")}{roll}");
        }
    }
}