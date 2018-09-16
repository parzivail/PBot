using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace DiscordPBot.Util
{
    internal static class E
    {
        public static byte[] ToBytes(this Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}