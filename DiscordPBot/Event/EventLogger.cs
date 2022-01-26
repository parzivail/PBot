using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BitLog;
using DSharpPlus;
using DSharpPlus.Entities;

namespace DiscordPBot.Event;

public record LoggedEvent(EventId Id, DateTime Timestamp, object Data);

public static class EventLogger
{
	public static void AttachEventLogger(AtomicLogger log, DiscordClient discord, ulong guildId)
	{
		discord.GuildMemberAdded += async (_, args) =>
		{
			if (args.Guild.Id == guildId && !args.Member.IsBot) await LogEvent(log, EventId.MemberAdded, new MemberEvent(args.Member.Id));
		};

		discord.GuildMemberRemoved += async (_, args) =>
		{
			if (args.Guild.Id == guildId && !args.Member.IsBot) await LogEvent(log, EventId.MemberRemoved, new MemberEvent(args.Member.Id));
		};

		discord.MessageCreated += async (_, args) =>
		{
			if (args.Guild.Id == guildId && !args.Author.IsBot) await LogEvent(log, EventId.MemberSpoke, new MemberEvent(args.Author.Id));
		};

		discord.MessageReactionAdded += async (_, args) =>
		{
			if (args.Guild.Id == guildId && !args.User.IsBot)
				await LogEvent(log, EventId.ReactionAdded, new MemberChannelMessageEmojiEvent(args.User.Id, args.Channel.Id, args.Message.Id, GetEmojiId(args.Emoji)));
		};

		discord.MessageReactionRemoved += async (_, args) =>
		{
			if (args.Guild.Id == guildId && !args.User.IsBot)
				await LogEvent(log, EventId.ReactionRemoved, new MemberChannelMessageEmojiEvent(args.User.Id, args.Channel.Id, args.Message.Id, GetEmojiId(args.Emoji)));
		};

		discord.VoiceStateUpdated += async (_, args) =>
		{
			if (args.Guild.Id == guildId && !args.User.IsBot)
			{
				if (args.Before?.Channel == null && args.After?.Channel != null)
					await LogEvent(log, EventId.MemberJoinVoice, new MemberChannelEvent(args.User.Id, args.After.Channel.Id));
				else if (args.Before?.Channel != null && args.After?.Channel == null)
					await LogEvent(log, EventId.MemberLeaveVoice, new MemberChannelEvent(args.User.Id, args.Before.Channel.Id));
				else if (args.Before?.Channel != null && args.After?.Channel != null && args.Before.Channel.Id != args.After.Channel.Id)
				{
					await LogEvent(log, EventId.MemberLeaveVoice, new MemberChannelEvent(args.User.Id, args.Before.Channel.Id));
					await LogEvent(log, EventId.MemberJoinVoice, new MemberChannelEvent(args.User.Id, args.After.Channel.Id));
				}
			}
		};

		discord.GuildBanAdded += async (_, args) =>
		{
			if (args.Guild.Id == guildId && !args.Member.IsBot) await LogEvent(log, EventId.MemberBanned, new MemberEvent(args.Member.Id));
		};

		discord.InviteCreated += async (_, args) =>
		{
			if (args.Guild.Id == guildId && !args.Invite.Inviter.IsBot) await LogEvent(log, EventId.InviteCreated, new MemberChannelEvent(args.Invite.Inviter.Id, args.Invite.Channel.Id));
		};

		discord.InviteDeleted += async (_, args) =>
		{
			if (args.Guild.Id == guildId && !args.Invite.Inviter.IsBot) await LogEvent(log, EventId.InviteDeleted, new MemberChannelEvent(args.Invite.Inviter.Id, args.Invite.Channel.Id));
		};
	}

	public static List<LoggedEvent> Read(string filename)
	{
		using var f = File.OpenRead(filename);
		var br = new BinaryReader(f);

		var events = new List<LoggedEvent>();
		while (f.Position < f.Length)
		{
			var eventId = (EventId)br.ReadUInt16();
			var timestamp = new DateTime(br.ReadInt64());

			switch (eventId)
			{
				case EventId.MemberAdded:
				case EventId.MemberRemoved:
				case EventId.MemberSpoke:
				case EventId.MemberBanned:
				{
					events.Add(new LoggedEvent(eventId, timestamp, ReadStruct<MemberEvent>(br)));
					break;
				}
				case EventId.ReactionAdded:
				case EventId.ReactionRemoved:
				{
					events.Add(new LoggedEvent(eventId, timestamp, ReadStruct<MemberChannelMessageEmojiEvent>(br)));
					break;
				}
				case EventId.MemberJoinVoice:
				case EventId.MemberLeaveVoice:
				case EventId.InviteCreated:
				case EventId.InviteDeleted:
				{
					events.Add(new LoggedEvent(eventId, timestamp, ReadStruct<MemberChannelEvent>(br)));
					break;
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(eventId), eventId, null);
			}
		}

		return events;
	}

	private static T ReadStruct<T>(BinaryReader br) where T : struct
	{
		var s = new T();
		var structSpan = GetStructSpan(ref s);
		var fileData = br.ReadBytes(structSpan.Length);
		fileData.CopyTo(structSpan);
		return s;
	}

	private static Span<byte> GetStructSpan<T>(ref T data) where T : struct
	{
		return MemoryMarshal.Cast<T, byte>(MemoryMarshal.CreateSpan(ref data, 1));
	}

	private static ulong GetEmojiId(DiscordEmoji emoji)
	{
		return emoji.Id != 0 ? emoji.Id : HashEmojiName(emoji.Name);
	}

	private static ulong HashEmojiName(string name)
	{
		return 0xFF00000000000000 | (HashFnv1A(name, 7837703) << 32) | HashFnv1A(name, 16777619);
	}

	private static ulong HashFnv1A(string value, uint d)
	{
		var hash = 2166136261u;
		foreach (var t in value)
		{
			hash ^= t;
			hash *= d;
		}

		return hash;
	}

	private static async Task LogEvent<T>(AtomicLogger log, EventId id, T e) where T : struct
	{
		var payloadData = GetEventBytes(DateTime.UtcNow, id, e);
		await log.WriteDataAsync(payloadData, CancellationToken.None);
	}

	private static byte[] GetEventBytes<T>(DateTime timestamp, EventId id, T e) where T : struct
	{
		const int idPos = 0;
		const int timestampPos = idPos + sizeof(ushort);
		const int payloadPos = timestampPos + sizeof(long);

		var eventSpan = GetStructSpan(ref e);

		var payloadSize = eventSpan.Length;
		var payloadData = new byte[payloadPos + payloadSize];
		var payloadDataSpan = payloadData.AsSpan();

		BitConverter.TryWriteBytes(payloadDataSpan[idPos..], (ushort)id);
		BitConverter.TryWriteBytes(payloadDataSpan[timestampPos..], timestamp.Ticks);
		eventSpan.CopyTo(payloadDataSpan[payloadPos..]);

		return payloadData;
	}
}