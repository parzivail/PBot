using System.Runtime.InteropServices;

namespace DiscordPBot.Event
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct MemberChannelEvent : IMemberEvent, IChannelEvent
	{
		/// <inheritdoc />
		public ulong MemberId { get; }

		/// <inheritdoc />
		public ulong ChannelId { get; }

		public MemberChannelEvent(ulong memberId, ulong channelId)
		{
			MemberId = memberId;
			ChannelId = channelId;
		}
	}
}