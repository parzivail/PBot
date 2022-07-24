using System.Runtime.InteropServices;

namespace DiscordPBot.Event
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct MemberChannelMessageEvent : IMemberEvent, IChannelEvent, IMessageEvent
	{
		/// <inheritdoc />
		public ulong MemberId { get; }

		/// <inheritdoc />
		public ulong ChannelId { get; }

		/// <inheritdoc />
		public ulong MessageId { get; }

		public MemberChannelMessageEvent(ulong memberId, ulong channelId, ulong messageId)
		{
			MemberId = memberId;
			ChannelId = channelId;
			MessageId = messageId;
		}
	}
}