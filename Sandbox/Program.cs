using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using DiscordPBot.Event;
using DSharpPlus.Entities;

namespace Sandbox
{
	public class Program
	{
		private static string StringToEntityList(string srcString)
		{
			const int startHighSurrogate = 0xD800;
			const int endHighSurrogate = 0xDBFF;
			const char zeroWidthJoiner = '\u200D';
			const char varSelector16 = '\uFE0F';
			
			if (srcString.IndexOf(zeroWidthJoiner) < 0)
				srcString = srcString.Replace(varSelector16.ToString(), "");
			
			var entityList = new List<string>();
			var lastHighSurrogate = 0;
			
			foreach (var c in srcString)
			{
				int codepoint = c;
				if (lastHighSurrogate > 0)
				{
					entityList.Add((65536 + (lastHighSurrogate - startHighSurrogate << 10) + (codepoint - endHighSurrogate - 1)).ToString("x"));
					lastHighSurrogate = 0;
				}
				else if (codepoint is >= startHighSurrogate and <= endHighSurrogate)
					lastHighSurrogate = codepoint; 
				else
					entityList.Add(codepoint.ToString("x"));
			}

			return string.Join("-", entityList);
		}
		
		public static void Main(string[] args)
		{
			var emojiSurrogateToFilenameTable = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("emoji_surrogate_to_filename_table.json"));
			
			var discordNameLookipField = typeof(DiscordEmoji).GetProperty("DiscordNameLookup", BindingFlags.Static | BindingFlags.NonPublic);
			var discordNameLookup = (IReadOnlyDictionary<string, string>)discordNameLookipField.GetValue(null);
			
			var emojiHashToSurrogateTable = discordNameLookup.Keys.ToDictionary(EventEmojiUtil.HashEmojiSurrogates);
			
			var data = EventLogger.Read(@"E:\colby\Desktop\temp\log.bin");

			var sb = new StringBuilder();

			foreach (var (id, timestamp, payload) in data)
			{
				sb.Clear();

				sb.Append($"{timestamp.ToLocalTime():M/d/yyyy h:mm:ss.ffff tt}")
					.Append(' ')
					.Append(id);

				if (payload is IMemberEvent memberEvent)
					sb.Append($" Member={memberEvent.MemberId}");

				if (payload is IChannelEvent channelEvent)
					sb.Append($" Channel={channelEvent.ChannelId}");

				if (payload is IMessageEvent messageEvent)
					sb.Append($" Message={messageEvent.MessageId}");

				if (payload is IEmojiEvent emojiEvent)
				{
					var (builtIn, emojiId) = EventEmojiUtil.UnpackEmoji(emojiEvent.EmojiId);

					if (builtIn)
					{
						var surrogate = emojiHashToSurrogateTable[emojiId];
						var filename = emojiSurrogateToFilenameTable[StringToEntityList(surrogate)];
						sb.Append($" Emoji=https://discordapp.com/assets/{filename}");
					}
					else
						sb.Append($" Emoji=https://cdn.discordapp.com/emojis/{emojiId}.webp");
				}

				Console.WriteLine(sb.ToString());
			}
		}
	}
}