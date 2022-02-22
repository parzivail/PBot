using System.Runtime.InteropServices;

namespace DiscordPBot.Event
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct MemberChannelMessageMentionEvent : IMemberEvent, IChannelEvent, IMessageEvent, IMentionEvent
	{
		/// <inheritdoc />
		public ulong MemberId { get; }

		/// <inheritdoc />
		public ulong ChannelId { get; }

		/// <inheritdoc />
		public ulong MessageId { get; }

		/// <inheritdoc />
		public MentionEventType MentionType { get; }

		/// <inheritdoc />
		public ulong MentionId { get; }

		public MemberChannelMessageMentionEvent(ulong memberId, ulong channelId, ulong messageId, MentionEventType mentionType, ulong mentionId)
		{
			MemberId = memberId;
			ChannelId = channelId;
			MessageId = messageId;
			MentionType = mentionType;
			MentionId = mentionId;
		}
	}
}