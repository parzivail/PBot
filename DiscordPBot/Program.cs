using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetEnv;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Net.WebSocket;

namespace DiscordPBot
{
    class Program
    {
        private static string _discordToken;
        private static DiscordClient _discord;
        private static InteractivityModule _interactivity;
        private static CommandsNextModule _commands;

        private static void LoadEnv()
        {
            Env.Load();
            _discordToken = Env.GetString("DISCORD_TOKEN");
        }

        private static async Task Main(string[] args)
        {
            LoadEnv();

            // Client ID: 490629949565435927
            // Permissions integer: 51264
            // Invite link: https://discordapp.com/oauth2/authorize?client_id=490629949565435927&scope=bot&permissions=51264

            _discord = new DiscordClient(new DiscordConfiguration
            {
                Token = _discordToken,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Info
            });

            _interactivity = _discord.UseInteractivity(new InteractivityConfiguration());
            _commands = _discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = "~",
                EnableMentionPrefix = true,
                EnableDms = true
            });
            _commands.RegisterCommands<PCommands>();
            _commands.SetHelpFormatter<HelpFormatter>();

            _discord.Ready += DiscordOnReady;
            _discord.MessageCreated += DiscordOnMessageCreated;

            await _discord.ConnectAsync();

            await Task.Delay(-1);

            Console.WriteLine("End of Line.");
            Console.ReadKey();
        }

        private static Task DiscordOnReady(ReadyEventArgs args)
        {
            _discord.UpdateStatusAsync(new DiscordGame("github.com/parzivail/PBot"));

            return Task.CompletedTask;
        }

        private static Task DiscordOnMessageCreated(MessageCreateEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
