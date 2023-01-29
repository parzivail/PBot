using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DiscordPBot.Event;
using DSharpPlus.Entities;
using MinecraftCurseForge.NET;

namespace Sandbox
{
	public class Histogram<T> : Dictionary<T, int>
	{
		public new int this[T key]
		{
			get => ContainsKey(key) ? base[key] : 0;
			set => base[key] = value;
		}
	}

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

		private static async Task AsyncMain()
		{
			var api = new CurseForgeApi("");

			var mod = await api.GetMod(496522);
			var modDesc = await api.GetModDescription(496522);
			var mods = await api.GetMods(496522);
			var files = await api.GetModFiles(496522);
			var file = await api.GetModFile(496522, 3655802);
			var fileChangelog = await api.GetModFileChangelog(496522, 3655802);
			var fileDownload = await api.GetModFileDownloadUrl(496522, 3655802);
		}

		public static void Main(string[] args)
		{
			// AsyncMain().ConfigureAwait(false).GetAwaiter().GetResult();
			ParseLogMain(args);
		}

		public static void ParseLogMain(string[] args)
		{
			var emojiSurrogateToFilenameTable = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("emoji_surrogate_to_filename_table.json"));

			var discordNameLookipField = typeof(DiscordEmoji).GetProperty("DiscordNameLookup", BindingFlags.Static | BindingFlags.NonPublic);
			var discordNameLookup = (IReadOnlyDictionary<string, string>)discordNameLookipField.GetValue(null);

			var emojiHashToSurrogateTable = discordNameLookup.Keys.ToDictionary(EventEmojiUtil.HashEmojiSurrogates);

			var messageCount = 0;

			var diamondCount = 0;
			var reactionCount = 0;
			var reAdd = 0;
			var reRem = 0;

			var memberCount = 0;
			var joins = 0;
			var leaves = 0;

			var data = EventLogger.Read(@"E:\colby\Desktop\temp\log.bin");

			var sb = new StringBuilder();

			var firstEvent = data.Min(e => e.Timestamp);

			var messageMemberMap = new Dictionary<ulong, ulong>();

			var welcomes = new Histogram<ulong>();
			var welcomeable = new HashSet<ulong>();
			var welcomed = new HashSet<ulong>();
			
			var monthJoins = 0;
			var monthLeaves = 0;
			var oneMonthAgo = DateTime.UtcNow - TimeSpan.FromDays(30);

			var diamondWelcomeValue = 5;

			var timeStep = TimeSpan.FromDays(1);
			var cursor = firstEvent;
			int stepJoins = 0;
			int stepLeaves = 0;

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

				if (payload is IMentionEvent mention)
					sb.Append($" Mention=[{mention.MentionType}: {mention.MentionId}]");

				if (payload is IEmojiEvent emojiEvent)
				{
					var (builtIn, emojiId) = EventEmojiUtil.UnpackEmoji(emojiEvent.EmojiId);

					if (builtIn)
					{
						if (emojiHashToSurrogateTable.TryGetValue(emojiId, out var surrogate))
						{
							if (emojiSurrogateToFilenameTable.TryGetValue(StringToEntityList(surrogate), out var filename))
								sb.Append($" Emoji=https://discordapp.com/assets/{filename}");
							else
								sb.Append($" Emoji=[Unknown builtin surrogate: {emojiId} - {surrogate}]");
						}
						else
							sb.Append($" Emoji=[Unknown builtin hash: {emojiId}]");
					}
					else
						sb.Append($" Emoji=https://cdn.discordapp.com/emojis/{emojiId}.webp");
				}

				switch (id)
				{
					case EventId.MemberAdded:
						memberCount++;
						joins++;
						stepJoins++;

						if (timestamp > oneMonthAgo)
						{
							monthJoins++;
							welcomeable.Add(((IMemberEvent)payload).MemberId);
						}

						break;
					case EventId.MemberRemoved:
						memberCount--;
						leaves++;
						stepLeaves++;

						if (timestamp > oneMonthAgo)
						{
							monthLeaves++;
							welcomeable.Remove(((IMemberEvent)payload).MemberId);
						}

						break;
					case EventId.SyncMemberCount when payload is IIntegerEvent iie:
						if (iie.Data != memberCount)
							Console.WriteLine($"Member sync inconsistent! Sync: {iie.Data}, Tally: {memberCount}");
						memberCount = iie.Data;
						break;
					case EventId.MemberSpoke:
						messageCount++;
						messageMemberMap[((IMessageEvent)payload).MessageId] = ((IMemberEvent)payload).MemberId;
						break;
					case EventId.ReactionAdded when payload is IMessageEvent ime and IEmojiEvent iee:
						reactionCount++;
						reAdd++;

						if (timestamp > oneMonthAgo && iee.EmojiId == 873401784289341491) // diamond
						{
							diamondCount++;
							if (messageMemberMap.ContainsKey(ime.MessageId))
								welcomes[messageMemberMap[ime.MessageId]] += diamondWelcomeValue;
						}

						break;
					case EventId.ReactionRemoved:
						reactionCount--;
						reRem++;
						break;
					case EventId.MemberMentioned:
					{
						var mentionedId = ((IMentionEvent)payload).MentionId;
						var senderId = ((IMemberEvent)payload).MemberId;
						
						if (welcomeable.Contains(mentionedId) && !welcomed.Contains(mentionedId))
						{
							welcomes[senderId]++;
							welcomeable.Remove(mentionedId);
							welcomed.Add(mentionedId);
						}
						
						break;
					}
				}

				// Log member count
				// if (timestamp > cursor + timeStep)
				// {
				// 	Console.WriteLine($"{cursor},{stepJoins},{stepLeaves},{memberCount}");
				// 	stepJoins = 0;
				// 	stepLeaves = 0;
				// 	cursor += timeStep;
				// }
			
				// Print each entry to the console
				// Console.WriteLine(sb.ToString());
			}

			Console.WriteLine($"Total members: {memberCount} (+{joins}, -{leaves})");
			Console.WriteLine($"Total reactions: {reactionCount} (+{reAdd}, -{reRem})");
			Console.WriteLine($"Total diamonds: {diamondCount}");
			Console.WriteLine($"Total messages: {messageCount}");

			Console.WriteLine();

			Console.WriteLine($"Welcome Leaderboard (+{monthJoins}/-{monthLeaves})");
			var welcomers = welcomes.OrderByDescending(pair => pair.Value).ToArray();
			var aboveMedian = welcomers[..(welcomers.Length / 2)];
			var belowMedian = welcomers[(welcomers.Length / 2)..];
			
			Console.WriteLine($"Acolyte: {string.Join(", ", aboveMedian.Select(pair => $"<@!{pair.Key}>").OrderBy(arg => arg))}");
			Console.WriteLine($"Thanks: {string.Join(", ", belowMedian.Select(pair => $"<@!{pair.Key}>").OrderBy(arg => arg))}");
		}
	}
}