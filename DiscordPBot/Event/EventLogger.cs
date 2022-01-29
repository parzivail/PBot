using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BitLog;
using DSharpPlus;
using DSharpPlus.Entities;

namespace DiscordPBot.Event
{
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

			discord.GuildMemberUpdated += async (_, args) =>
			{
				if (args.Guild.Id != guildId || args.Member.IsBot)
					return;
				
				var time = DateTime.UtcNow;

				var srcRoles = args.RolesBefore.Select(role => role.Id).ToHashSet();
				var destRoles = args.RolesAfter.Select(role => role.Id).ToHashSet();

				var gainedRoles = new HashSet<ulong>(destRoles.Except(srcRoles));
				var lostRoles = new HashSet<ulong>(srcRoles.Except(destRoles));

				foreach (var gainedRole in gainedRoles)
					await LogEvent(log, EventId.MemberAddRole, new MemberRoleEvent(args.Member.Id, gainedRole), time);
				
				foreach (var lostRole in lostRoles)
					await LogEvent(log, EventId.MemberRemoveRole, new MemberRoleEvent(args.Member.Id, lostRole), time);
			};

			discord.MessageCreated += async (_, args) =>
			{
				if (args.Guild.Id == guildId && !args.Author.IsBot)
				{
					switch (args.Message.MessageType)
					{
						case MessageType.Default:
						case MessageType.Reply:
							await LogEvent(log, EventId.MemberSpoke, new MemberChannelEvent(args.Author.Id, args.Channel.Id));
							break;
						case MessageType.ChannelPinnedMessage:
							await LogEvent(log, EventId.MessagePinned, new MemberChannelEvent(args.Author.Id, args.Channel.Id));
							break;
						case MessageType.UserPremiumGuildSubscription:
							await LogEvent(log, EventId.NitroBoost, new MemberEvent(args.Author.Id));
							break;
						case MessageType.TierOneUserPremiumGuildSubscription:
							await LogEvent(log, EventId.NitroBoostObtainTier1, new MemberEvent(args.Author.Id));
							break;
						case MessageType.TierTwoUserPremiumGuildSubscription:
							await LogEvent(log, EventId.NitroBoostObtainTier2, new MemberEvent(args.Author.Id));
							break;
						case MessageType.TierThreeUserPremiumGuildSubscription:
							await LogEvent(log, EventId.NitroBoostObtainTier3, new MemberEvent(args.Author.Id));
							break;
						case MessageType.ApplicationCommand:
							await LogEvent(log, EventId.MemberUsedCommand, new MemberChannelEvent(args.Author.Id, args.Channel.Id));
							break;
					}
				}
			};

			discord.MessageReactionAdded += async (_, args) =>
			{
				if (args.Guild.Id == guildId && !args.User.IsBot)
					await LogEvent(log, EventId.ReactionAdded, new MemberChannelMessageEmojiEvent(args.User.Id, args.Channel.Id, args.Message.Id, EventEmojiUtil.GetEmojiId(args.Emoji)));
			};

			discord.MessageReactionRemoved += async (_, args) =>
			{
				if (args.Guild.Id == guildId && !args.User.IsBot)
					await LogEvent(log, EventId.ReactionRemoved, new MemberChannelMessageEmojiEvent(args.User.Id, args.Channel.Id, args.Message.Id, EventEmojiUtil.GetEmojiId(args.Emoji)));
			};

			discord.VoiceStateUpdated += async (_, args) =>
			{
				if (args.Guild.Id != guildId || args.User.IsBot)
					return;

				var time = DateTime.UtcNow;

				if (args.Before?.Channel == null && args.After?.Channel != null)
					await LogEvent(log, EventId.MemberJoinVoice, new MemberChannelEvent(args.User.Id, args.After.Channel.Id), time);
				else if (args.Before?.Channel != null && args.After?.Channel == null)
					await LogEvent(log, EventId.MemberLeaveVoice, new MemberChannelEvent(args.User.Id, args.Before.Channel.Id), time);
				else if (args.Before?.Channel != null && args.After?.Channel != null && args.Before.Channel.Id != args.After.Channel.Id)
				{
					await LogEvent(log, EventId.MemberLeaveVoice, new MemberChannelEvent(args.User.Id, args.Before.Channel.Id), time);
					await LogEvent(log, EventId.MemberJoinVoice, new MemberChannelEvent(args.User.Id, args.After.Channel.Id), time);
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
					case EventId.MemberBanned:
					case EventId.MemberPassedScreening:
					case EventId.NitroBoost:
					case EventId.NitroBoostObtainTier1:
					case EventId.NitroBoostObtainTier2:
					case EventId.NitroBoostObtainTier3:
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
					case EventId.MemberSpoke:
					case EventId.MemberJoinVoice:
					case EventId.MemberLeaveVoice:
					case EventId.InviteCreated:
					case EventId.InviteDeleted:
					case EventId.MessagePinned:
					case EventId.MemberUsedCommand:
					{
						events.Add(new LoggedEvent(eventId, timestamp, ReadStruct<MemberChannelEvent>(br)));
						break;
					}
					case EventId.MemberAddRole:
					case EventId.MemberRemoveRole:
					{
						events.Add(new LoggedEvent(eventId, timestamp, ReadStruct<MemberRoleEvent>(br)));
						break;
					}
					case EventId.SyncMemberCount:
					case EventId.SyncBoostCount:
					{
						events.Add(new LoggedEvent(eventId, timestamp, ReadStruct<IntegerEvent>(br)));
						break;
					}
					default:
						throw new ArgumentOutOfRangeException();
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

		public static async Task LogEvent<T>(AtomicLogger log, EventId id, T e, DateTime? time = null) where T : struct
		{
			var payloadData = GetEventBytes(time ?? DateTime.UtcNow, id, e);
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
}