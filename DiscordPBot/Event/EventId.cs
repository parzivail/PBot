namespace DiscordPBot.Event
{
	public enum EventId : ushort
	{
		MemberAdded = 1,
		MemberRemoved = 2,
		MemberSpoke = 3,
		ReactionAdded = 4,
		ReactionRemoved = 5,
		MemberJoinVoice = 6,
		MemberLeaveVoice = 7,
		MemberBanned = 8,
		InviteCreated = 9,
		InviteDeleted = 10,
		MemberAddRole = 11,
		MemberRemoveRole = 12,
		MemberPassedScreening = 13,
	}
}