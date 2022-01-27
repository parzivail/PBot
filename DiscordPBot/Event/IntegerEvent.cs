using System.Runtime.InteropServices;

namespace DiscordPBot.Event
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct IntegerEvent : IIntegerEvent
	{
		/// <inheritdoc />
		public int Data { get; }

		public IntegerEvent(int data)
		{
			Data = data;
		}
	}
}