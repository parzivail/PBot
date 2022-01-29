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
					entityList.Add((0xFFFF + (lastHighSurrogate - startHighSurrogate << 10) + (codepoint - endHighSurrogate)).ToString("x"));
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

			var messageCount = 0;
			var reactionCount = 0;
			var reAdd = 0;
			var reRem = 0;

			var memberCount = 0;
			var joins = 0;
			var leaves = 0;

			var data = EventLogger.Read(@"E:\colby\Desktop\temp\log.bin");

			var sb = new StringBuilder();

			var firstEvent = data.Min(e => e.Timestamp);

			foreach (var (id, timestamp, payload) in data)
			{
				sb.Clear();

				sb.Append($"{timestamp.ToLocalTime():MM/dd/yyyy hh:mm:ss.ffff tt}")
					.Append(' ')
					.Append($"+{(timestamp - firstEvent):G}")
					.Append(' ')
					.Append(id.ToString().PadRight(20));

				if (payload is IIntegerEvent intEvent)
					sb.Append($" Data={intEvent.Data}");

				if (payload is IMemberEvent memberEvent)
					sb.Append($" Member={memberEvent.MemberId}");

				if (payload is IChannelEvent channelEvent)
					sb.Append($" Channel={channelEvent.ChannelId}");

				if (payload is IMessageEvent messageEvent)
					sb.Append($" Message={messageEvent.MessageId}");

				if (payload is IRoleEvent roleEvent)
					sb.Append($" Role={roleEvent.RoleId}");

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

				switch (id)
				{
					case EventId.MemberAdded:
						memberCount++;
						joins++;
						break;
					case EventId.MemberRemoved:
						memberCount--;
						leaves++;
						break;
					case EventId.SyncMemberCount when payload is IIntegerEvent iie:
						memberCount = iie.Data;
						break;
					case EventId.MemberSpoke:
						messageCount++;
						break;
					case EventId.ReactionAdded:
						reactionCount++;
						reAdd++;
						break;
					case EventId.ReactionRemoved:
						reactionCount--;
						reRem++;
						break;
				}

				Console.WriteLine(sb.ToString());
			}

			Console.WriteLine($"Total members: {memberCount} (+{joins}, -{leaves})");
			Console.WriteLine($"Total reactions: {reactionCount} (+{reAdd}, -{reRem})");
			Console.WriteLine($"Total messages: {messageCount}");
		}
	}
}