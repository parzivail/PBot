using System.Runtime.InteropServices;

namespace DiscordPBot.Event
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct MemberEvent : IMemberEvent
	{
		/// <inheritdoc />
		public ulong MemberId { get; }

		public MemberEvent(ulong memberId)
		{
			MemberId = memberId;
		}
	}
}