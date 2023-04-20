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
		MessagePinned = 14,
		NitroBoost = 15,
		NitroBoostObtainTier1 = 16,
		NitroBoostObtainTier2 = 17,
		NitroBoostObtainTier3 = 18,
		MemberUsedCommand = 19,
		SyncMemberCount = 20,
		SyncBoostCount = 21,
		MemberMentioned = 22,
		ChannelMentioned = 23,
		RoleMentioned = 24,
		SyncProjectDownloads = 25,
		SyncProjectFollowers = 26
	}
}