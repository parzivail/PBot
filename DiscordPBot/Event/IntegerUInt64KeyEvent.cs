using System.Runtime.InteropServices;

namespace DiscordPBot.Event
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct IntegerUInt64KeyEvent : IIntegerEvent, IUInt64KeyEvent
	{
		/// <inheritdoc />
		public int Data { get; }
		
		/// <inheritdoc />
		public ulong Key { get; }

		public IntegerUInt64KeyEvent(int data, ulong key)
		{
			Data = data;
			Key = key;
		}
	}
}