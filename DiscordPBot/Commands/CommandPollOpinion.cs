using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DiscordPBot.Commands
{
    internal class CommandPollOpinion
    {
        [Command("opinion")]
        [Description("Polls for opinions as reactions to your message")]
        public async Task D(CommandContext ctx, [RemainingText] string message)
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":+1:"));
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":-1:"));
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":shrug:"));
        }
    }
}