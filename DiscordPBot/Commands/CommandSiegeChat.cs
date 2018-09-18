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
        private static Font _scout;

        [Command("allchat")]
        [Description("Say something in chat")]
        public async Task SiegeChat(CommandContext ctx, string who, [RemainingText] string message)
        {
            await ctx.TriggerTypingAsync();

            // Font: Scout, 20pt Antialiased
            // Text top left: (5, 8)
            // Max lines: 560
            // Max text width: 290

            if (_scout == null)
            {
                var collection = new PrivateFontCollection();
                collection.AddFontFile("Resources/Allchat/Scout.ttf");
                var fontFamily = new FontFamily("Scout", collection);
                _scout = new Font(fontFamily, 20, GraphicsUnit.Point);
            }

            using (var bmp = new Bitmap("Resources/Allchat/chatentry.png"))
            using (var newBitmap = new Bitmap(bmp.Width, bmp.Height))
            {
                using (var g = Graphics.FromImage(newBitmap))
                {
                    g.DrawImage(bmp, 0, 0);

                    g.TextRenderingHint = TextRenderingHint.AntiAlias;
                    g.DrawString($"[ALL]  {who}: {message}", _scout, Brushes.White, new RectangleF(18, 18, 560, 35));
                }

                using (var ms = new MemoryStream(newBitmap.ToBytes()))
                {
                    await ctx.RespondWithFileAsync(ms, "image.png");
                }
            }
        }
    }
}