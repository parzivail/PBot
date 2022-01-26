using System;
using BitLog;
using DiscordPBot.Event;

namespace Sandbox
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var data = EventLogger.Read(@"C:\Users\Admin\RiderProjects\PBot\DiscordPBot\bin\Debug\net5.0\log.bin");
			foreach (var loggedEvent in data)
			{
				Console.WriteLine($"{loggedEvent.Timestamp.ToLocalTime()} {loggedEvent.Id}");
			}
		}
	}
}