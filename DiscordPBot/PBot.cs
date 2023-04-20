using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using BitLog;
using DiscordPBot.Event;
using DiscordPBot.Model;
using DiscordPBot.Modrinth;
using DotNetEnv;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using LiteDB;
using Markdig;
using Microsoft.Extensions.Logging;
using MinecraftCurseForge.NET;
using Newtonsoft.Json;
using EventId = DiscordPBot.Event.EventId;

namespace DiscordPBot
{
	internal record FieldPrototype(string Header, string Value);

	internal static class PBot
	{
		private const bool Production = true;

		private static string _appName = "PBot";

		private static string _discordToken;

		private static ulong _ownerId;
		private static ulong _guild;

		private static ulong _managementChannel;
		private static ulong _downloadAnnouncementChannel;
		private static ulong _generalChannel;
		private static ulong _suggestionsChannel;
		private static ulong _bugsChannel;
		private static ulong _supportChannel;

		private static ulong _curseForgeEmoji;
		private static ulong _supportEmoji;
		private static ulong _modrinthEmoji;

		private static string _curseForgeApiKey;
		public static DateTime LastFileRefresh { get; private set; } = DateTime.UnixEpoch;
		public static int[] CurseForgeProjectIds { get; private set; }
		public static string[] CurseForgeProjectSlugs { get; private set; }
		public static string[] ModrinthProjectSlugs { get; private set; }
		public static ulong[] ModrinthProjectIndices { get; private set; }

		internal static readonly ManualResetEventSlim ExitHandle = new();

		private static Timer _taskScheduler24Hr;
		private static Timer _taskScheduler15Min;

		private static CurseForgeApi _curseForgeApi;
		private static ModrinthApi _modrinthApi = new();
		private static DiscordClient _discord;

		private static AtomicLogger _alog;

		private static LiteDatabase _db;

		public static ILiteCollection<CurseForgeFileDatabaseEntry> CfFileCollection { get; private set; }
		public static ILiteCollection<ModrinthFileDatabaseEntry> MrFileCollection { get; private set; }
		public static ulong OwnerId => _ownerId;

		public static DateTime StartTime;

		private static void LoadEnv()
		{
			Env.Load();
			_appName = Env.GetString("APP_NAME");

			_discordToken = Env.GetString("DISCORD_TOKEN");

			_ownerId = EnvGetUlong("OWNER_ID");
			_guild = EnvGetUlong("MANAGED_GUILD");

			_curseForgeApiKey = Env.GetString("CURSEFORGE_API_KEY");
			CurseForgeProjectIds = Env.GetString("CURSEFORGE_PROJECT_ID").Split(',').Select(int.Parse).ToArray();
			CurseForgeProjectSlugs = Env.GetString("CURSEFORGE_PROJECT_SLUG").Split(',');
			ModrinthProjectSlugs = Env.GetString("MODRINTH_PROJECT_SLUG").Split(',');
			ModrinthProjectIndices = Env.GetString("MODRINTH_PROJECT_INDEX").Split(',').Select(ulong.Parse).ToArray();

			_managementChannel = EnvGetUlong("MANAGEMENT_CHANNEL");
			_downloadAnnouncementChannel = EnvGetUlong("DOWNLOAD_ANNOUNCEMENT_CHANNEL");
			_generalChannel = EnvGetUlong("GENERAL_CHANNEL");
			_suggestionsChannel = EnvGetUlong("SUGGESTIONS_CHANNEL");
			_bugsChannel = EnvGetUlong("BUGS_CHANNEL");
			_supportChannel = EnvGetUlong("SUPPORT_CHANNEL");

			_curseForgeEmoji = EnvGetUlong("CURSEFORGE_EMOJI");
			_supportEmoji = EnvGetUlong("SUPPORT_EMOJI");
			_modrinthEmoji = EnvGetUlong("MODRINTH_EMOJI");

			StartTime = DateTime.Now;
		}

		private static ulong EnvGetUlong(string key, string fallback = null)
		{
			return ulong.Parse(Env.GetString(key, fallback));
		}

		private static void Main(string[] args)
		{
			using (_db = new LiteDatabase("data.db"))
			using (_alog = new AtomicLogger("log.bin"))
			{
				// manually checkpoint
				_db.CheckpointSize = 0;
				_db.Checkpoint();

				CfFileCollection = _db.GetCollection<CurseForgeFileDatabaseEntry>("cf_files");
				CfFileCollection.EnsureIndex(file => file.Id);
				
				MrFileCollection = _db.GetCollection<ModrinthFileDatabaseEntry>("modrinth_files");
				MrFileCollection.EnsureIndex(file => file.Id);

				try
				{
					AsyncMain().ConfigureAwait(false).GetAwaiter().GetResult();
				}
				catch (Exception e)
				{
					Console.WriteLine("Failed due to exception");
					Console.WriteLine(e);
				}
			}

			Console.WriteLine("Clean exit, disposed managed resources.");
		}

		private static async Task AsyncMain()
		{
			LoadEnv();

			// Production Bot:
			// Client ID: 490629949565435927
			// Permissions integer: 51264
			// Invite link: https://discordapp.com/oauth2/authorize?client_id=490629949565435927&scope=bot&permissions=51264

			// Dev Bot:
			// Client ID: 491370448425058306
			// Permissions integer: 51264
			// Invite link: https://discordapp.com/oauth2/authorize?client_id=491370448425058306&scope=bot&permissions=51264

			_discord = new DiscordClient(new DiscordConfiguration
			{
				Token = _discordToken,
				TokenType = TokenType.Bot,
				Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers
			});

			await _discord.ConnectAsync();

			EventLogger.AttachEventLogger(_alog, _discord, _guild);

			_discord.ClientErrored += OnClientError;
			_discord.MessageCreated += OnMessageCreated;

			var commands = _discord.UseCommandsNext(new CommandsNextConfiguration
			{
				StringPrefixes = new[] { "!p ", $"<@{_discord.CurrentUser.Id}> ", $"<@!{_discord.CurrentUser.Id}> " },
			});
			commands.RegisterCommands<ExitModule>();
			commands.RegisterCommands<PingModule>();
			commands.RegisterCommands<CurseForgeModule>();
			commands.RegisterCommands<ModrinthModule>();

			_discord.Logger.Log(LogLevel.Information, $"{_appName} running");

			_curseForgeApi = new CurseForgeApi(_curseForgeApiKey);

			ScheduleTasks();

			ExitHandle.Wait();
		}

		private static List<FieldPrototype> CreateEmbed(XmlNodeList nodes)
		{
			string fieldTitle = null;
			var sbFieldValue = new StringBuilder();

			var initialList = new List<FieldPrototype>();

			foreach (var node in nodes)
			{
				if (node is not XmlElement xe)
					continue;

				switch (xe.Name)
				{
					case "h1" or "h2" or "h3" or "h4" or "h5" or "h6":
						fieldTitle = XmlToMarkdown(xe.ChildNodes);
						break;
					case "ul":
					{
						AppendList(xe.ChildNodes, sbFieldValue);

						initialList.Add(new FieldPrototype(fieldTitle ?? "Changelog", sbFieldValue.ToString()));
						sbFieldValue.Clear();
						break;
					}
				}
			}

			var segmentedList = new List<FieldPrototype>();

			foreach (var prototype in initialList)
			{
				if (prototype.Value.Length < 1024)
				{
					segmentedList.Add(prototype);
					continue;
				}

				var value = prototype.Value;
				var taken = TakeOrLess(value, 1024);
				segmentedList.Add(new FieldPrototype(prototype.Header, taken));
				value = value[taken.Length..];

				while (value.Length > 0)
				{
					taken = TakeOrLess(value, 1024);
					segmentedList.Add(new FieldPrototype($"{prototype.Header} (Cont'd.)", taken));
					value = value[taken.Length..];
				}
			}

			return segmentedList;
		}

		private static string TakeOrLess(string s, int length)
		{
			var worstCase = s[..Math.Min(length, s.Length)];

			var newlineIdx = worstCase.LastIndexOf('\n');
			return newlineIdx <= 0 ? worstCase : worstCase[..newlineIdx];
		}

		private static void AppendList(XmlNodeList nodes, StringBuilder sbFieldValue, int indentLevel = 1)
		{
			const string indent = "\uFEFF\u2003";
			const char bullet = '•';

			var line = 0;
			foreach (var node in nodes)
			{
				if (line > 0)
					sbFieldValue.AppendLine();
				line++;

				if (node is not XmlElement { Name: "li" } li)
					continue;

				for (var i = 0; i < indentLevel; i++)
					sbFieldValue.Append(indent);
				sbFieldValue.Append(bullet).Append(' ').Append(XmlToMarkdown(li.ChildNodes, indentLevel));
			}
		}

		private static string XmlToMarkdown(XmlNodeList xml, int indentLevel = 0)
		{
			var sb = new StringBuilder();

			foreach (var node in xml)
			{
				switch (node)
				{
					case XmlText text:
						sb.Append(text.Value.Trim('\r', '\n'));
						break;
					case XmlElement { Name: "code" } tag:
						sb.Append('`').Append(XmlToMarkdown(tag.ChildNodes)).Append('`');
						break;
					case XmlElement { Name: "i" or "em" } tag:
						sb.Append('*').Append(XmlToMarkdown(tag.ChildNodes)).Append('*');
						break;
					case XmlElement { Name: "b" or "strong" } tag:
						sb.Append("**").Append(XmlToMarkdown(tag.ChildNodes)).Append("**");
						break;
					case XmlElement { Name: "u" or "ins" } tag:
						sb.Append("__").Append(XmlToMarkdown(tag.ChildNodes)).Append("__");
						break;
					case XmlElement { Name: "strike" or "del" } tag:
						sb.Append("~~").Append(XmlToMarkdown(tag.ChildNodes)).Append("~~");
						break;
					case XmlElement { Name: "pre" } tag:
						sb.Append("```").Append(XmlToMarkdown(tag.ChildNodes)).Append("```");
						break;
					case XmlElement { Name: "ul" } tag:
						sb.AppendLine();
						AppendList(tag.ChildNodes, sb, indentLevel + 1);
						break;
				}
			}

			return sb.ToString();
		}

		private static void ScheduleTasks()
		{
			_taskScheduler24Hr = new Timer(ScheduledTask24Hr, null, TimeSpan.FromSeconds(4), TimeSpan.FromHours(24));
			_taskScheduler15Min = new Timer(ScheduledTask15Min, null, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(15));
		}

		private static async void ScheduledTask24Hr(object state)
		{
			var guild = await GetManagedGuild();
			var time = DateTime.UtcNow;

			await EventLogger.LogEvent(_alog, EventId.SyncMemberCount, new IntegerEvent(guild.MemberCount), time);
			await EventLogger.LogEvent(_alog, EventId.SyncBoostCount, new IntegerEvent(guild.PremiumSubscriptionCount ?? 0), time);
		}

		private static async void ScheduledTask15Min(object state)
		{
			if (Production)
			{
				LastFileRefresh = DateTime.UtcNow;
				try
				{
					await RefreshCurseFiles();
				}
				catch (HttpRequestException e)
				{
					// CurseForge API probably has a problem, log and move on
					_discord.Logger.Log(LogLevel.Error, $"Couldn't refresh CurseForge files: {e.Message}");
				}
				try
				{
					await RefreshModrinthFiles();
				}
				catch (HttpRequestException e)
				{
					// Modrinth API probably has a problem, log and move on
					_discord.Logger.Log(LogLevel.Error, $"Couldn't refresh Modrinth files: {e.Message}");
				}
			}
		}

		public static async Task RefreshCurseFiles()
		{
			for (var i = 0; i < CurseForgeProjectIds.Length; i++)
			{
				var id = CurseForgeProjectIds[i];
				var slug = CurseForgeProjectSlugs[i];

				CurseForgeFilesResponse files;

				try
				{
					files = await _curseForgeApi.GetModFiles(id);
				}
				catch (Exception e)
				{
					SendToManagement(new DiscordMessageBuilder().WithContent($":x: Unable to deserialize CurseForge API response while loading {slug}: {e.Message}"));
					continue;
				}

				if (files == null)
				{
					SendToManagement(new DiscordMessageBuilder().WithContent($":x: Unable to deserialize CurseForge API response while loading {slug}, `files` was null."));
					continue;
				}

				if (files.Files.Count == 0)
					continue;

				var file = files.Files.OrderByDescending(f => f.Id).First();

				var curseChangelogResponse = await _curseForgeApi.GetModFileChangelog(id, file.Id);

				if (CfFileCollection.Count() > 0 && CfFileCollection.Exists(f => f.Id == file.Id))
					continue;

				CfFileCollection.Insert(new CurseForgeFileDatabaseEntry
				{
					Id = file.Id,
					DisplayName = file.DisplayName,
					FileDate = file.FileDate,
					FileName = file.FileName
				});
				_db.Checkpoint();

				if (file.FileDate < StartTime)
					continue;

				var details = await _curseForgeApi.GetMod(id);

				var managedGuild = await GetManagedGuild();

				var curseEmoji = await managedGuild.GetEmojiAsync(_curseForgeEmoji);
				var supportEmoji = await managedGuild.GetEmojiAsync(_supportEmoji);
				var downloadChannel = managedGuild.GetChannel(_downloadAnnouncementChannel);

				var doc = new XmlDocument();
				doc.LoadXml($"<changelog>{curseChangelogResponse}</changelog>");

				var changelogEmbed = new DiscordEmbedBuilder()
					.WithColor(new DiscordColor(0x0D0D0D));
				var fieldPrototypes = CreateEmbed(doc["changelog"].ChildNodes);

				foreach (var (header, value) in fieldPrototypes)
					changelogEmbed.AddField(HttpUtility.HtmlDecode(header ?? ""), HttpUtility.HtmlDecode(value));

				var message = await downloadChannel.SendMessageAsync(builder =>
				{
					builder
						.WithContent($"@everyone A new {details.Name} update has been released!")
						.WithAllowedMention(new EveryoneMention())
						.AddEmbed(new DiscordEmbedBuilder()
							.WithTitle(file.DisplayName)
							.WithThumbnail(details.Logo.ThumbnailUrl)
							.WithTimestamp(file.FileDate)
							.WithColor(new DiscordColor(0xFFD400))
							.AddField($"Download on {curseEmoji} CurseForge", $"https://www.curseforge.com/minecraft/{slug}/files/{file.Id}")
							.AddField($":speech_balloon: Feedback", MentionChannel(_generalChannel), true)
							.AddField($":beetle: Report Bugs", MentionChannel(_bugsChannel), true)
							.AddField($":bulb: Suggestions", MentionChannel(_suggestionsChannel), true)
							.AddField($"{supportEmoji} Support", $"Want to chip in a couple bucks? Check out the rewards in {MentionChannel(_supportChannel)}! All proceeds fund development costs.")
							.Build());
					if (changelogEmbed.Fields.Count > 0)
						builder.AddEmbed(changelogEmbed.Build());
				});

				await downloadChannel.CrosspostMessageAsync(message);

				SendToManagement(new DiscordMessageBuilder().WithContent(
					$":white_check_mark: Found new CurseForge file for {slug} **{file.DisplayName}** (`{file.Id}`), notified {downloadChannel.Mention}"));

				// Don't notify for more than one project per refresh cycle
				return;
			}
		}

		public static async Task RefreshModrinthFiles()
		{
			for (var i = 0; i < ModrinthProjectSlugs.Length; i++)
			{
				var slug = ModrinthProjectSlugs[i];
				var idx = ModrinthProjectIndices[i];
				
				var project = await _modrinthApi.GetProject(slug);
				await EventLogger.LogEvent(_alog, EventId.SyncProjectDownloads, new IntegerUInt64KeyEvent(project.Downloads, idx), DateTime.UtcNow);
				await EventLogger.LogEvent(_alog, EventId.SyncProjectFollowers, new IntegerUInt64KeyEvent(project.Followers, idx), DateTime.UtcNow);
				
				ModrinthProjectVersionResponse[] projectVersions;

				try
				{
					projectVersions = await _modrinthApi.GetProjectVersions(slug);
				}
				catch (Exception e)
				{
					SendToManagement(new DiscordMessageBuilder().WithContent($":x: Unable to deserialize Modrinth API response while loading {slug}: {e.Message}"));
					continue;
				}

				if (projectVersions == null || projectVersions.Length == 0)
				{
					SendToManagement(new DiscordMessageBuilder().WithContent($":x: Unable to deserialize Modrinth API response while loading {slug}, `files` was null or empty."));
					continue;
				}

				var file = projectVersions.OrderByDescending(f => f.DatePublished).First();

				if (MrFileCollection.Count() > 0 && MrFileCollection.Exists(f => f.Id == file.Id))
					continue;

				MrFileCollection.Insert(new ModrinthFileDatabaseEntry()
				{
					Id = file.Id,
					VersionNumber = file.VersionNumber,
					FileDate = file.DatePublished,
					FileName = file.Name
				});
				_db.Checkpoint();

				if (file.DatePublished < StartTime)
					continue;

				var managedGuild = await GetManagedGuild();

				var modrinthEmoji = await managedGuild.GetEmojiAsync(_modrinthEmoji);
				var supportEmoji = await managedGuild.GetEmojiAsync(_supportEmoji);
				var downloadChannel = managedGuild.GetChannel(_downloadAnnouncementChannel);

				var markdownHtml = Markdown.ToHtml(file.Changelog);

				var doc = new XmlDocument();
				doc.LoadXml($"<changelog>{markdownHtml}</changelog>");

				var changelogEmbed = new DiscordEmbedBuilder()
					.WithColor(new DiscordColor(0x0D0D0D));
				var fieldPrototypes = CreateEmbed(doc["changelog"].ChildNodes);

				foreach (var (header, value) in fieldPrototypes)
					changelogEmbed.AddField(HttpUtility.HtmlDecode(header ?? ""), HttpUtility.HtmlDecode(value));

				var slug1 = slug;
				var message = await downloadChannel.SendMessageAsync(builder =>
				{
					builder
						.WithContent($"@everyone A new {project.Title} update has been released!")
						.WithAllowedMention(new EveryoneMention())
						.AddEmbed(new DiscordEmbedBuilder()
							.WithTitle(file.Name)
							.WithThumbnail(project.IconUrl)
							.WithTimestamp(file.DatePublished)
							.WithColor(new DiscordColor(0xFFD400))
							.AddField($"Download on {modrinthEmoji} Modrinth", $"https://modrinth.com/mod/{slug1}/version/{file.VersionNumber}")
							.AddField($":speech_balloon: Feedback", MentionChannel(_generalChannel), true)
							.AddField($":beetle: Report Bugs", MentionChannel(_bugsChannel), true)
							.AddField($":bulb: Suggestions", MentionChannel(_suggestionsChannel), true)
							.AddField($"{supportEmoji} Support", $"Want to chip in a couple bucks? Check out the rewards in {MentionChannel(_supportChannel)}! All proceeds fund development costs.")
							.Build());
					if (changelogEmbed.Fields.Count > 0)
						builder.AddEmbed(changelogEmbed.Build());
				});

				await downloadChannel.CrosspostMessageAsync(message);

				SendToManagement(new DiscordMessageBuilder().WithContent(
					$":white_check_mark: Found new Modrinth file for {slug} **{file.Name}** (`{file.Id}`), notified {downloadChannel.Mention}"));

				// Don't notify for more than one project per refresh cycle
				return;
			}
		}

		private static string MentionChannel(ulong channelId)
		{
			return $"<#{channelId}>";
		}

		private static async void SendToManagement(DiscordMessageBuilder message)
		{
			var managementChannel = await GetManagementChannel();
			await message.SendAsync(managementChannel);
		}

		private static async void SendToDownloads(DiscordMessageBuilder message)
		{
			var managementChannel = await GetDownloadsChannel();
			await message.SendAsync(managementChannel);
		}

		private static async Task<DiscordGuild> GetManagedGuild() => await _discord.GetGuildAsync(_guild);

		private static async Task<DiscordChannel> GetManagementChannel() => (await GetManagedGuild()).GetChannel(_managementChannel);

		private static async Task<DiscordChannel> GetDownloadsChannel() => (await GetManagedGuild()).GetChannel(_downloadAnnouncementChannel);

		private static async Task OnClientError(DiscordClient sender, ClientErrorEventArgs e)
		{
			_discord.Logger.Log(LogLevel.Error, $"Error in event handler {e.EventName}: {e.Exception.Message}");
		}

		private static async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
		{
			if (e.Author.IsBot)
				return;
		}
	}

	public class ExitModule : BaseCommandModule
	{
		[Command("exit")]
		public async Task Exit(CommandContext ctx)
		{
			if (ctx.Member.Id == PBot.OwnerId)
			{
				await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
				PBot.ExitHandle.Set();
			}
		}
	}

	public class PingModule : BaseCommandModule
	{
		[Command("ping")]
		public async Task Ping(CommandContext ctx)
		{
			await ctx.Message.RespondAsync($":ping_pong: {ctx.Client.Ping} ms");
		}
	}

	public class CurseForgeModule : BaseCommandModule
	{
		[Command("cf_meta")]
		public async Task GetMeta(CommandContext ctx)
		{
			var sb = new StringBuilder();

			for (var i = 0; i < PBot.CurseForgeProjectIds.Length; i++)
				sb.AppendLine($"Tracking project: `{PBot.CurseForgeProjectIds[i]} {PBot.CurseForgeProjectSlugs[i]}`");

			sb.AppendLine($"Last check: <t:{((DateTimeOffset)PBot.LastFileRefresh).ToUnixTimeSeconds()}:R>");
			sb.AppendLine($"Next check: <t:{((DateTimeOffset)PBot.LastFileRefresh.AddMinutes(15)).ToUnixTimeSeconds()}:R>");

			await ctx.Message.RespondAsync(sb.ToString());
		}

		[Command("cf_latest")]
		public async Task GetLatest(CommandContext ctx)
		{
			var files = PBot.CfFileCollection.Query()
				.OrderByDescending(files => files.FileDate)
				.Limit(4)
				.ToArray();

			var sb = new StringBuilder();
			foreach (var file in files)
			{
				var timestamp = (DateTimeOffset)file.FileDate;
				sb.AppendLine($"**{file.DisplayName}** (`{file.Id} {file.FileName}`), released <t:{timestamp.ToUnixTimeSeconds()}:R>");
			}

			await ctx.Message.RespondAsync($"Most recent CurseForge files are:\n{sb}");
		}

		[Command("cf_refresh")]
		public async Task Refresh(CommandContext ctx)
		{
			await PBot.RefreshCurseFiles();
			await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
		}
	}

	public class ModrinthModule : BaseCommandModule
	{
		[Command("mr_meta")]
		public async Task GetMeta(CommandContext ctx)
		{
			var sb = new StringBuilder();

			for (var i = 0; i < PBot.ModrinthProjectSlugs.Length; i++)
				sb.AppendLine($"Tracking project: `{PBot.ModrinthProjectSlugs[i]}`");

			sb.AppendLine($"Last check: <t:{((DateTimeOffset)PBot.LastFileRefresh).ToUnixTimeSeconds()}:R>");
			sb.AppendLine($"Next check: <t:{((DateTimeOffset)PBot.LastFileRefresh.AddMinutes(15)).ToUnixTimeSeconds()}:R>");

			await ctx.Message.RespondAsync(sb.ToString());
		}

		[Command("mr_latest")]
		public async Task GetLatest(CommandContext ctx)
		{
			var files = PBot.MrFileCollection.Query()
				.OrderByDescending(files => files.FileDate)
				.Limit(4)
				.ToArray();

			var sb = new StringBuilder();
			foreach (var file in files)
			{
				var timestamp = (DateTimeOffset)file.FileDate;
				sb.AppendLine($"**{file.FileName}** (`{file.Id}`), released <t:{timestamp.ToUnixTimeSeconds()}:R>");
			}

			await ctx.Message.RespondAsync($"Most recent Modrinth files are:\n{sb}");
		}

		[Command("mr_refresh")]
		public async Task Refresh(CommandContext ctx)
		{
			await PBot.RefreshModrinthFiles();
			await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
		}
	}
}