using System.Runtime.InteropServices;

namespace DiscordPBot.Event
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct MemberChannelEvent
	{
		public readonly ulong MemberId;
		public readonly ulong ChannelId;

		public MemberChannelEvent(ulong memberId, ulong channelId)
		{
			MemberId = memberId;
			ChannelId = channelId;
		}
	}
}