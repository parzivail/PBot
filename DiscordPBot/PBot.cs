using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using BitLog;
using DiscordPBot.Event;
using DiscordPBot.Model;
using DotNetEnv;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using LiteDB;
using Microsoft.Extensions.Logging;
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

		private static int _curseForgeProjectId;
		private static string _curseForgeProjectSlug;

		internal static readonly ManualResetEventSlim ExitHandle = new();
		private static readonly HttpClient Web = new();

		private static Timer _taskScheduler24Hr;
		private static Timer _taskScheduler15Min;

		private static DiscordClient _discord;

		private static AtomicLogger _alog;

		private static LiteDatabase _db;
		
		public static ILiteCollection<CurseForgeFiles> CfFileCollection { get; private set; }
		public static ulong OwnerId => _ownerId;

		private static void LoadEnv()
		{
			Env.Load();
			_appName = Env.GetString("APP_NAME");
			
			_discordToken = Env.GetString("DISCORD_TOKEN");
			
			_ownerId = EnvGetUlong("OWNER_ID");
			_guild = EnvGetUlong("MANAGED_GUILD");
			
			_curseForgeProjectId = Env.GetInt("CURSEFORGE_PROJECT_ID");
			_curseForgeProjectSlug = Env.GetString("CURSEFORGE_PROJECT_SLUG");
			
			_managementChannel = EnvGetUlong("MANAGEMENT_CHANNEL");
			_downloadAnnouncementChannel = EnvGetUlong("DOWNLOAD_ANNOUNCEMENT_CHANNEL");
			_generalChannel = EnvGetUlong("GENERAL_CHANNEL");
			_suggestionsChannel = EnvGetUlong("SUGGESTIONS_CHANNEL");
			_bugsChannel = EnvGetUlong("BUGS_CHANNEL");
			_supportChannel = EnvGetUlong("SUPPORT_CHANNEL");
			
			_curseForgeEmoji = EnvGetUlong("CURSEFORGE_EMOJI");
			_supportEmoji = EnvGetUlong("SUPPORT_EMOJI");
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

				CfFileCollection = _db.GetCollection<CurseForgeFiles>("cf_files");
				CfFileCollection.EnsureIndex(file => file.Id);

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
			commands.RegisterCommands<RoleModule>();

			_discord.Logger.Log(LogLevel.Information, $"{_appName} running");

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
				try
				{
					await RefreshCurseFiles();
				}
				catch (HttpRequestException e)
				{
					// CurseForge API probably has a problem, log and move on
					_discord.Logger.Log(LogLevel.Error, $"Couldn't refresh CurseForge files: {e.Message}");
				}
			}
		}

		public static async Task RefreshCurseFiles()
		{
			var curseFilesResponse = await Web.GetStringAsync($"https://addons-ecs.forgesvc.net/api/v2/addon/{_curseForgeProjectId}/files");
			var files = JsonConvert.DeserializeObject<CurseForgeFiles[]>(curseFilesResponse);

			if (files == null)
			{
				SendToManagement(new DiscordMessageBuilder().WithContent(":x: Unable to deserialize CurseForge API response, `files` was null."));
				return;
			}

			var file = files.OrderByDescending(f => f.Id).First();

			var curseChangelogResponse = await Web.GetStringAsync($"https://addons-ecs.forgesvc.net/api/v2/addon/{_curseForgeProjectId}/file/{file.Id}/changelog");

			if (CfFileCollection.Count() > 0 && CfFileCollection.Exists(f => f.Id == file.Id))
				return;

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
				changelogEmbed.AddField(header ?? "", value);

			var message = await downloadChannel.SendMessageAsync(builder => builder
				.WithContent("@everyone A new PSWG version has been released!")
				.AddEmbed(new DiscordEmbedBuilder()
					.WithTitle(file.DisplayName)
					.WithTimestamp(file.FileDate)
					.WithColor(new DiscordColor(0xFFD400))
					.AddField($"Download on {curseEmoji} CurseForge", $"https://www.curseforge.com/minecraft/mc-mods/{_curseForgeProjectSlug}/files/{file.Id}")
					.AddField($":speech_balloon: Feedback", MentionChannel(_generalChannel), true)
					.AddField($":beetle: Report Bugs", MentionChannel(_bugsChannel), true)
					.AddField($":bulb: Suggestions", MentionChannel(_suggestionsChannel), true)
					.AddField($"Support {supportEmoji}", $"If you'd like to show a token of your appreciation, consider checking out the rewards in {MentionChannel(_supportChannel)}!")
					.Build())
				.AddEmbed(changelogEmbed.Build())
			);
			
			CfFileCollection.Insert(file);
			_db.Checkpoint();

			await downloadChannel.CrosspostMessageAsync(message);

			SendToManagement(new DiscordMessageBuilder().WithContent($":white_check_mark: Found new CurseForge file **{file.DisplayName}** (`{file.Id}`), notified {downloadChannel.Mention}"));
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
		[Command("cf_latest")]
		public async Task GetLatest(CommandContext ctx)
		{
			var file = PBot.CfFileCollection.Query()
				.OrderByDescending(files => files.FileDate)
				.First();

			var timestamp = (DateTimeOffset)file.FileDate;
			await ctx.Message.RespondAsync($"Most recent CurseForge file is **{file.DisplayName}** (`{file.Id}`), released <t:{timestamp.ToUnixTimeSeconds()}:R>");
		}

		[Command("cf_refresh")]
		public async Task Refresh(CommandContext ctx)
		{
			await PBot.RefreshCurseFiles();
			await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
		}
	}

	public class RoleModule : BaseCommandModule
	{
		private static Dictionary<string, ulong> _roleNames = new()
		{
			{ "rebel", 541458430796234752 },
			{ "empire", 541458360445304854 },
			{ "jedi", 541456961582137356 },
			{ "sith", 541457017672433694 },
			{ "droid", 541458558915444746 },
			{ "event", 497574739041320960 },
		};

		[Command("join")]
		public async Task Join(CommandContext ctx, string role)
		{
			role = role.ToLower().Trim();
			if (_roleNames.TryGetValue(role, out var roleId))
			{
				var roleInstance = ctx.Guild.GetRole(roleId);
				var member = await ctx.Guild.GetMemberAsync(ctx.User.Id);
				await member.GrantRoleAsync(roleInstance);

				await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
			}
			else
			{
				await ctx.Message.RespondAsync($"No role named **{role}**!");
			}
		}

		[Command("leave")]
		public async Task Leave(CommandContext ctx, string role)
		{
			role = role.ToLower().Trim();
			if (_roleNames.TryGetValue(role, out var roleId))
			{
				var roleInstance = ctx.Guild.GetRole(roleId);
				var member = await ctx.Guild.GetMemberAsync(ctx.User.Id);
				await member.RevokeRoleAsync(roleInstance);

				await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
			}
			else
			{
				await ctx.Message.RespondAsync($"No role named **{role}**!");
			}
		}
	}
}