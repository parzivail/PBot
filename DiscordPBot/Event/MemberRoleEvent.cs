using System.Runtime.InteropServices;

namespace DiscordPBot.Event
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct MemberRoleEvent : IMemberEvent, IRoleEvent
	{
		/// <inheritdoc />
		public ulong MemberId { get; }

		/// <inheritdoc />
		public ulong RoleId { get; }

		public MemberRoleEvent(ulong memberId, ulong roleId)
		{
			MemberId = memberId;
			RoleId = roleId;
		}
	}
}