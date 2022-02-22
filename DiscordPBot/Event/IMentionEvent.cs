namespace DiscordPBot.Event;

public interface IMentionEvent
{
	MentionEventType MentionType { get; }
	ulong MentionId { get; }
}