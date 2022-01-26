using System.Runtime.InteropServices;

namespace DiscordPBot.Event
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct MemberEvent
	{
		public readonly ulong MemberId;

		public MemberEvent(ulong memberId)
		{
			MemberId = memberId;
		}
	}
}