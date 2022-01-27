using DSharpPlus.Entities;

namespace DiscordPBot.Event
{
	public static class EventEmojiUtil
	{
		public static (bool BuiltIn, ulong Id) UnpackEmoji(ulong id)
		{
			return (id >> 56 == 0xFF, id);
		}

		public static ulong GetEmojiId(DiscordEmoji emoji)
		{
			return emoji.Id != 0 ? emoji.Id : HashEmojiSurrogates(emoji.Name);
		}

		public static ulong HashEmojiSurrogates(string name)
		{
			return 0xFF00000000000000 | (HashFnv1A(name, 7837703) << 32) | HashFnv1A(name, 16777619);
		}

		private static ulong HashFnv1A(string value, uint d)
		{
			var hash = 2166136261u;
			foreach (var t in value)
			{
				hash ^= t;
				hash *= d;
			}

			return hash;
		}
	}
}