using System.Runtime.InteropServices;

namespace DiscordPBot.Event
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct MemberChannelMessageEmojiEvent : IMemberEvent, IChannelEvent, IMessageEvent, IEmojiEvent
	{
		/// <inheritdoc />
		public ulong MemberId { get; }

		/// <inheritdoc />
		public ulong ChannelId { get; }

		/// <inheritdoc />
		public ulong MessageId { get; }

		/// <inheritdoc />
		public ulong EmojiId { get; }

		public MemberChannelMessageEmojiEvent(ulong memberId, ulong channelId, ulong messageId, ulong emojiId)
		{
			MemberId = memberId;
			ChannelId = channelId;
			MessageId = messageId;
			EmojiId = emojiId;
		}
	}
}