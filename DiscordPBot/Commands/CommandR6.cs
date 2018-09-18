using System.Net;
using System.Threading.Tasks;
using DiscordPBot.Schemas.RainbowSix;
using DiscordPBot.Util;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Humanizer.Localisation;
using Newtonsoft.Json;

namespace DiscordPBot.Commands
{
    internal class CommandR6
    {
        [Command("r6")]
        [Description("Get stats about a player on PC.")]
        public async Task Rainbow6(CommandContext ctx, string username)
        {
            await ctx.TriggerTypingAsync();

            R6PlayerStatsJson playerStats;

            using (var wc = new WebClient())
            {
                var player = await SiegeUtils.GetPlayer(ctx, username, wc);

                if (player == null)
                {
                    await ctx.RespondAsync(":warning: No players found with that username.");
                    return;
                }

                try
                {
                    var reqUrl = $"https://www.r6stats.com/api/stats/{player.UbisoftId}";
                    var json = wc.DownloadString(reqUrl);
                    playerStats = JsonConvert.DeserializeObject<R6PlayerStatsJson>(json);
                }
                catch (WebException e)
                {
                    PBot.LogError($"r6 stats WebException: {e.Message}");
                    await ctx.RespondAsync(":interrobang: Could not fetch player stats.");
                    return;
                }
                catch (JsonSerializationException e)
                {
                    PBot.LogError($"r6 stats JsonSerializationException: {e.Message}");
                    await ctx.RespondAsync(":interrobang: Could not load player stats.");
                    return;
                }
                catch (JsonReaderException e)
                {
                    PBot.LogError($"r6 stats JsonSerializationException: {e.Message}");
                    await ctx.RespondAsync(":interrobang: Could not load player stats.");
                    return;
                }
            }

            var stats = playerStats.Stats[0];

            username = playerStats.Username;
            var ubisoftId = playerStats.UbisoftId;
            
            var winPercent = (int) (stats.General.Wins / (float) (stats.General.Wins + stats.General.Losses) * 100);
            var playtime = stats.General.Playtime.Seconds().Humanize(maxUnit: TimeUnit.Hour);
            var shotHitAccuracy = (int) (stats.General.BulletsHit / (float)stats.General.BulletsFired * 100);
            var bombWinPercent = (int) (stats.Gamemode.Bomb.Wins / (float) (stats.Gamemode.Bomb.Wins + stats.Gamemode.Bomb.Losses) * 100);
            var hostageWinPercent = (int) (stats.Gamemode.Hostage.Wins / (float) (stats.Gamemode.Hostage.Wins + stats.Gamemode.Hostage.Losses) * 100);
            var secureWinPercent = (int) (stats.Gamemode.SecureArea.Wins / (float) (stats.Gamemode.SecureArea.Wins + stats.Gamemode.SecureArea.Losses) * 100);
            var winRankedPercent = (int) (stats.Queue.Ranked.Wins / (float) (stats.Queue.Ranked.Wins + stats.Queue.Ranked.Losses) * 100);
            var rankedPlaytime = stats.Queue.Ranked.Playtime.Seconds().Humanize(maxUnit: TimeUnit.Hour);

            var embed = new DiscordEmbedBuilder()
                .WithColor(PDiscordColor.SiegeYellow)
                .WithThumbnailUrl($"https://ubisoft-avatars.akamaized.net/{ubisoftId}/default_146_146.png")
                .WithAuthor(
                    $"{username}'s Stats (Level {playerStats.Progression.Level}/{playerStats.Progression.TotalXp} XP)"
                )
                .AddField("Kill/Death",
                    $"**Kills:** {stats.General.Kills}\n" +
                    $"**Deaths:** {stats.General.Deaths}\n" +
                    $"**Assists:** {stats.General.Assists}\n" +
                    $"**K/D:** {stats.General.Kd}",
                    true)
                .AddField("Win/Loss",
                    $"**Won:** {stats.General.Wins} ({winPercent}%)\n" +
                    $"**Lost:** {stats.General.Losses} ({100 - winPercent}%)\n" +
                    $"**W/L:** {stats.General.Wl}\n" +
                    $"**Playtime:** {playtime}",
                    true)
                .AddField("Stats",
                    $"**DBNOs:** {stats.General.Dbnos}\n" +
                    $"**Headshots:** {stats.General.Headshots}\n" +
                    $"**Penetrations:** {stats.General.PenetrationKills}\n" +
                    $"**Melees:** {stats.General.MeleeKills}\n" +
                    $"**Revives:** {stats.General.Revives}\n" +
                    $"**Blind Kills:** {stats.General.BlindKills}",
                    true)
                .AddField("Shots",
                    $"**Fired:** {stats.General.BulletsFired}\n" +
                    $"**Hit:** {stats.General.BulletsHit}\n" +
                    $"**Accuracy:** {shotHitAccuracy}%",
                    true)
                .AddField("Extras",
                    $"**Suicides:** {stats.General.Suicides}\n" +
                    $"**Barricades:** {stats.General.BarricadesDeployed}\n" +
                    $"**Reinforcements:** {stats.General.ReinforcementsDeployed}\n" +
                    $"**Gadgets Destroyed:** {stats.General.GadgetsDestroyed}\n" +
                    $"**Rappel Breaches:** {stats.General.RappelBreaches}\n" +
                    $"**Disatance Travelled:** {stats.General.DistanceTravelled:n0}m",
                    true)
                .AddField("Quick Rank",
                    $"**Wins:** {stats.Queue.Ranked.Wins} ({winRankedPercent}%)\n" +
                    $"**Losses:** {stats.Queue.Ranked.Losses} ({100 - winRankedPercent}%)\n" +
                    $"**W/L:** {stats.Queue.Ranked.Wl}\n" +
                    $"**K/D:** {stats.Queue.Ranked.Kd}\n" +
                    $"**Playtime:** {rankedPlaytime}",
                    true)
                .AddField("Bomb",
                    $"**Wins:** {stats.Gamemode.Bomb.Wins} ({bombWinPercent}%)\n" +
                    $"**Losses:** {stats.Gamemode.Bomb.Losses} ({100 - bombWinPercent}%)\n" +
                    $"**Best Score:** {stats.Gamemode.Bomb.BestScore}",
                    true)
                .AddField("Hostage",
                    $"**Wins:** {stats.Gamemode.Hostage.Wins} ({hostageWinPercent}%)\n" +
                    $"**Losses:** {stats.Gamemode.Hostage.Losses} ({100 - hostageWinPercent}%)\n" +
                    $"**Best Score:** {stats.Gamemode.Hostage.BestScore}\n" +
                    $"**Extractions Denied:** {stats.Gamemode.Hostage.ExtractionsDenied}",
                    true)
                .AddField("Secure",
                    $"**Wins:** {stats.Gamemode.SecureArea.Wins} ({secureWinPercent}%)\n" +
                    $"**Losses:** {stats.Gamemode.SecureArea.Losses} ({100 - secureWinPercent}%)\n" +
                    $"**Best Score:** {stats.Gamemode.SecureArea.BestScore}\n" +
                    $"**Obj. Kills (ATK):** {stats.Gamemode.SecureArea.KillsAsAttackerInObjective}\n" +
                    $"**Obj. Kills (DEF):** {stats.Gamemode.SecureArea.KillsAsDefenderInObjective}\n" +
                    $"**Objectives Secured:** {stats.Gamemode.SecureArea.TimesObjectiveSecured}",
                    true);

            await ctx.RespondAsync(embed: embed);
        }
    }
}