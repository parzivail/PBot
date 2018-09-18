using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DiscordPBot.Util;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DiscordPBot.Commands
{
    internal partial class PCommands
    {
        private static Font _tahoma;

        [Command("clippy")]
        [Description("Invoke the almighty power of Clippy")]
        public async Task Clippy(CommandContext ctx, [RemainingText] string message)
        {
            await ctx.TriggerTypingAsync();

            // Font: Tahoma, 10pt Aliased
            // Bubble BG: #FFFFCC
            // Text top left: (5, 8)
            // Max lines: 4
            // Max text width: 290

            message = message.Replace("|", "\n");

            if (_tahoma == null)
                _tahoma = new Font(new FontFamily("Tahoma"), 10, GraphicsUnit.Point);

            using (var bmp = new Bitmap("Resources/Clippy/clippy.png"))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                    g.DrawString(message, _tahoma, Brushes.Black, new RectangleF(5, 8, 290, 64));
                }

                using (var ms = new MemoryStream(bmp.ToBytes()))
                {
                    await ctx.RespondWithFileAsync(ms, "image.png");
                }
            }
        }
    }
}