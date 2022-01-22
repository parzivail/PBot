using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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

namespace DiscordPBot
{
	internal static class PBot
	{
		private const string AppName = "PBot";

		private const ulong SelfId = 491370448425058306;
		private const ulong OwnerId = 135836100198531073;

		private const ulong Guild = 412945916476129280;
		private const ulong ManagementChannel = 844606217636675644;
		private const ulong DownloadAnnouncementChannel = 427921970605195264;
		private const ulong BugsChannel = 800139171687563275;

		private const int MessagesForRegular = 100;
		private const ulong RegularLoungeChannel = 827224067882221628;
		private const ulong RegularRole = 502106996871266314;

		private const ulong CurseForgeEmoji = 856963013809537065;

		private const int CurseForgeProjectId = 496522;
		private const string CurseForgeProjectSlug = "pswg";

		private static readonly ManualResetEventSlim ExitHandle = new();
		private static readonly WebClient Web = new();

		private static string _discordToken;
		private static Timer _taskScheduler;

		private static DiscordClient _discord;

		private static LiteDatabase _db;
		private static ILiteCollection<BotUser> _users;
		public static ILiteCollection<CurseForgeFiles> CfFileCollection { get; private set; }

		private static HashSet<ulong> _bannedRegularChannels = new()
		{
			413320228198547457, // help
			497574514511839242, // bot-spam
			715343964147810335, // mute-appeal
		};

		private static void LoadEnv()
		{
			Env.Load();
			_discordToken = Env.GetString("DISCORD_TOKEN");
		}

		private static void Main(string[] args)
		{
			using (_db = new LiteDatabase("data.db"))
			{
				_users = _db.GetCollection<BotUser>("bot_users");
				_users.EnsureIndex(user => user.Id);

				CfFileCollection = _db.GetCollection<CurseForgeFiles>("cf_files");
				CfFileCollection.EnsureIndex(file => file.Id);

				try
				{
					AsyncMain().ConfigureAwait(false).GetAwaiter().GetResult();
				}
				catch (Exception e)
				{
					Console.WriteLine($"Failed due to exception: {e.Message}");
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
				TokenType = TokenType.Bot
			});

			await _discord.ConnectAsync();

			_discord.ClientErrored += OnClientError;
			_discord.MessageCreated += OnMessageCreated;

			var commands = _discord.UseCommandsNext(new CommandsNextConfiguration()
			{
				StringPrefixes = new[] { "!p ", $"<@{SelfId}> " },
			});
			commands.RegisterCommands<PingModule>();
			commands.RegisterCommands<CurseForgeModule>();
			commands.RegisterCommands<RoleModule>();

			_discord.Logger.Log(LogLevel.Information, "PBot running");

			StartTaskScheduler();

			ExitHandle.Wait();
		}

		private static void StartTaskScheduler()
		{
			_taskScheduler = new Timer(ScheduledTask, null, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(15));
		}

		private static async void ScheduledTask(object state)
		{
			await RefreshCurseFiles();
		}

		public static async Task RefreshCurseFiles()
		{
			var response = await Web.DownloadStringTaskAsync($"https://addons-ecs.forgesvc.net/api/v2/addon/{CurseForgeProjectId}/files");
			var files = JsonConvert.DeserializeObject<CurseForgeFiles[]>(response);

			if (files == null)
			{
				SendToManagement(new DiscordMessageBuilder().WithContent(":x: Unable to deserialize CurseForge API response, `files` was null."));
				return;
			}

			var file = files.OrderByDescending(f => f.Id).First();

			if (CfFileCollection.Count() > 0 && CfFileCollection.Exists(f => f.Id == file.Id))
				return;
			
			CfFileCollection.Insert(file);

			var managedGuild = await GetManagedGuild();

			var curseEmoji = await managedGuild.GetEmojiAsync(CurseForgeEmoji);
			var downloadChannel = managedGuild.GetChannel(DownloadAnnouncementChannel);
			var bugsChannel = managedGuild.GetChannel(BugsChannel);

			var messageBuilder = new DiscordMessageBuilder()
				.WithContent($"@everyone\n" +
				             $"**{file.DisplayName}** has been released!\n" +
				             $"**Download+changelog:**\n" +
				             $"{curseEmoji} https://www.curseforge.com/minecraft/mc-mods/{CurseForgeProjectSlug}/files/{file.Id}\n" +
				             $"Bugs? {bugsChannel.Mention}!");

			var message = await messageBuilder.SendAsync(downloadChannel);
			await downloadChannel.CrosspostMessageAsync(message);
			
			SendToManagement(new DiscordMessageBuilder().WithContent($":white_check_mark: Found new CurseForge file **{file.DisplayName}** (`{file.Id}`), notified {downloadChannel.Mention}"));
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

		private static async Task<DiscordGuild> GetManagedGuild() => await _discord.GetGuildAsync(Guild);

		private static async Task<DiscordChannel> GetManagementChannel() => (await GetManagedGuild()).GetChannel(ManagementChannel);

		private static async Task<DiscordChannel> GetDownloadsChannel() => (await GetManagedGuild()).GetChannel(DownloadAnnouncementChannel);

		private static async Task OnClientError(DiscordClient sender, ClientErrorEventArgs e)
		{
			_discord.Logger.Log(LogLevel.Error, $"Error in event handler {e.EventName}: {e.Exception.Message}");
		}

		private static async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
		{
			if (e.Author.IsBot)
				return;

			if (e.Author.Id == OwnerId)
			{
				if (e.Message.Content == $"<@{SelfId}> exit")
				{
					await e.Message.CreateReactionAsync(DiscordEmoji.FromName(_discord, ":white_check_mark:"));
					ExitHandle.Set();
				}
			}

			await UpdateAndNotifyRegularRole(e);
		}

		private static async Task UpdateAndNotifyRegularRole(MessageCreateEventArgs e)
		{
			if (_bannedRegularChannels.Contains(e.Channel.Id))
				return;

			if (!_users.Exists(botUser => botUser.Id == e.Author.Id))
				_users.Insert(new BotUser
				{
					Id = e.Author.Id,
					MessageCount = 0
				});

			var user = _users.FindOne(botUser => botUser.Id == e.Author.Id);

			user.MessageCount++;

			_users.Update(user);

			if (user.MessageCount == MessagesForRegular)
			{
				var loungeChannel = e.Guild.GetChannel(RegularLoungeChannel);
				await e.Message.RespondAsync($"Thank you for being an active part of the community — you now have access to {loungeChannel.Mention}!");

				var regularRole = e.Guild.GetRole(RegularRole);
				var member = await e.Guild.GetMemberAsync(e.Author.Id);
				await member.GrantRoleAsync(regularRole);
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