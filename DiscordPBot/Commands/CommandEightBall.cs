using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DiscordPBot.Commands
{
    internal partial class PCommands
    {
        private static readonly List<EightBallReply> Replies = new List<EightBallReply>
        {
            new EightBallReply {Text = "It is certain", Score = 0},
            new EightBallReply {Text = "It is decidedly so.", Score = 0},
            new EightBallReply {Text = "Without a doubt.", Score = 0},
            new EightBallReply {Text = "Yes, definitely.", Score = 0},
            new EightBallReply {Text = "You may rely on it.", Score = 0},
            new EightBallReply {Text = "As I see it, yes.", Score = 0},
            new EightBallReply {Text = "Most likely.", Score = 0},
            new EightBallReply {Text = "Outlook good.", Score = 0},
            new EightBallReply {Text = "Yes.", Score = 0},
            new EightBallReply {Text = "Signs point to yes.", Score = 0},
            new EightBallReply {Text = "Reply hazy, try again.", Score = 1},
            new EightBallReply {Text = "Ask again later.", Score = 1},
            new EightBallReply {Text = "Better not tell you now.", Score = 1},
            new EightBallReply {Text = "Cannot predict now.", Score = 1},
            new EightBallReply {Text = "Concentrate and ask again.", Score = 1},
            new EightBallReply {Text = "Don't count on it.", Score = 2},
            new EightBallReply {Text = "My reply is no.", Score = 2},
            new EightBallReply {Text = "My sources say no.", Score = 2},
            new EightBallReply {Text = "Outlook not so good.", Score = 2},
            new EightBallReply {Text = "Very doubtful.", Score = 2}
        };

        [Command("8ball")]
        [Description("Ask the Magic 8 Ball® a question")]
        public async Task EightBall(CommandContext ctx, string question)
        {
            await ctx.TriggerTypingAsync();

            var r = Replies[Math.Abs(question.GetHashCode()) % Replies.Count];
            var emoji = "";

            switch (r.Score)
            {
                case 0:
                    emoji = ":white_check_mark:";
                    break;
                case 1:
                    emoji = ":warning:";
                    break;
                case 2:
                    emoji = ":x:";
                    break;
            }

            await ctx.RespondAsync($"{emoji} {r.Text}");
        }

        private class EightBallReply
        {
            public string Text { get; set; }
            public int Score { get; set; }
        }
    }
}