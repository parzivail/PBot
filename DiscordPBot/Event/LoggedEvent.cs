using System;

namespace DiscordPBot.Event
{
	public record LoggedEvent(EventId Id, DateTime Timestamp, object Data);
}