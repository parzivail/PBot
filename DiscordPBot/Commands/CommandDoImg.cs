using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using DiscordPBot.Util;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DiscordPBot.Commands
{
    partial class PCommands
    {
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
    }
}
