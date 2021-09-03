using LiteDB;

namespace DiscordPBot.Model
{
	public class BotUser
	{
		[BsonId(false)] public ulong Id { get; set; }
		public int MessageCount { get; set; }
	}
}