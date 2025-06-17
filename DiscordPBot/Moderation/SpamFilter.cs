using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConcurrentCollections;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DiscordPBot.Moderation;

public class SpamFilter
{
	private record TimedMessage(DateTime Timestamp, ulong User, ulong Channel, ulong Message, int MessageHashCode);

	private static readonly TimeSpan TimeoutTimespan = TimeSpan.FromMinutes(10);
	private static readonly TimeSpan ThresholdTimespan = TimeSpan.FromSeconds(5);
	private static readonly ConcurrentHashSet<TimedMessage> RecentMessages = [];

	public static async Task ProcessMessage(MessageCreateEventArgs e)
	{
		RemoveStaleMessages();

		var message = new TimedMessage(DateTime.UtcNow, e.Author.Id, e.Channel.Id, e.Message.Id, e.Message.Content.GetHashCode());
		RecentMessages.Add(message);

		var similar = GetSimilar(message);
		if (similar.Count > 1)
		{
			var member = await e.Guild.GetMemberAsync(e.Author.Id);
			try
			{
				await member.TimeoutAsync(DateTimeOffset.Now.Add(TimeoutTimespan));

				PBot.SendToManagement(
					new DiscordMessageBuilder()
						.WithContent($"(Spam filter) {e.Author.Mention} was timed out for posting same message in multiple channels")
				);
			}
			catch (Exception ex)
			{
				PBot.SendToManagement(
					new DiscordMessageBuilder()
						.WithContent($"(Spam filter) {e.Author.Mention} posted the same message in multiple channels, which were deleted, but they could not be timed out (`{ex.Message}`).")
				);
			}

			foreach (var otherMessage in similar)
			{
				var channel = e.Guild.GetChannel(otherMessage.Channel);
				var messageInstance = await channel.GetMessageAsync(otherMessage.Message);
				await channel.DeleteMessageAsync(messageInstance, "(Spam filter) Multiple channel cross-post");
			}
		}
	}

	/// <summary>
	/// Remove messages older than twice the moderation threshold
	/// </summary>
	private static void RemoveStaleMessages()
	{
		var now = DateTime.UtcNow;
		
		var staleMessages = RecentMessages
			.Where(message => Math.Abs((now - message.Timestamp).TotalSeconds) > 2 * ThresholdTimespan.TotalSeconds)
			.ToList();

		foreach (var message in staleMessages)
			RecentMessages.TryRemove(message);
	}

	/// <summary>
	/// Get all messages within the moderation threshold from the same user, regardless
	/// of channel, as long as the message has the same content
	/// </summary>
	/// <param name="message"></param>
	/// <returns></returns>
	private static List<TimedMessage> GetSimilar(TimedMessage message)
	{
		return RecentMessages
			.Where(timedMessage =>
				timedMessage.User == message.User
				&& timedMessage.MessageHashCode == message.MessageHashCode
				&& Math.Abs((timedMessage.Timestamp - message.Timestamp).TotalSeconds) < ThresholdTimespan.TotalSeconds
			)
			.ToList();
	}
}