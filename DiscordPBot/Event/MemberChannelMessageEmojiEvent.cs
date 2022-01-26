using System.Runtime.InteropServices;

namespace DiscordPBot.Event;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MemberChannelMessageEmojiEvent
{
	public readonly ulong MemberId;
	public readonly ulong ChannelId;
	public readonly ulong MessageId;
	public readonly ulong EmojiId;

	public MemberChannelMessageEmojiEvent(ulong memberId, ulong channelId, ulong messageId, ulong emojiId)
	{
		MemberId = memberId;
		ChannelId = channelId;
		MessageId = messageId;
		EmojiId = emojiId;
	}
}