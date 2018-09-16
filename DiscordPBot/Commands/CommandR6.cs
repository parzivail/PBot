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
    internal partial class PCommands
    {
        [Command("r6")]
        [Description("Get stats about a player on PC.")]
        public async Task Rainbow6(CommandContext ctx, string username)
        {
            await ctx.TriggerTypingAsync();

            R6PlayerStatsJson playerStats;

            using (var wc = new WebClient())
            {
                R6PlayerSearchJson[] searchResults;

                try
                {
                    var reqUrl = $"https://www.r6stats.com/api/player-search/{username}/pc";
                    var json = wc.DownloadString(reqUrl);
                    searchResults = JsonConvert.DeserializeObject<R6PlayerSearchJson[]>(json);
                }
                catch (WebException e)
                {
                    await ctx.RespondAsync(":warning: No players found with that username.");
                    return;
                }
                catch (JsonSerializationException e)
                {
                    PBot.LogError($"r6 search JsonSerializationException: {e.Message}");
                    await ctx.RespondAsync(":interrobang: Could not load players.");
                    return;
                }
                catch (JsonReaderException e)
                {
                    PBot.LogError($"r6 search JsonSerializationException: {e.Message}");
                    await ctx.RespondAsync(":interrobang: Could not load players.");
                    return;
                }

                if (searchResults.Length == 0)
                {
                    await ctx.RespondAsync(":warning: No players found with that username.");
                    return;
                }

                try
                {
                    var reqUrl = $"https://www.r6stats.com/api/stats/{searchResults[0].UbisoftId}";
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

            username = playerStats.Username;

            var ubisoftId = playerStats.UbisoftId;

            var numKills = playerStats.Stats[0].General.Kills;
            var numDeaths = playerStats.Stats[0].General.Deaths;
            var numAssists = playerStats.Stats[0].General.Assists;
            var kd = playerStats.Stats[0].General.Kd;

            var numWon = playerStats.Stats[0].General.Wins;
            var numLost = playerStats.Stats[0].General.Losses;
            var winLoss = playerStats.Stats[0].General.Wl;
            var winPercent = (int) (numWon / (float) (numWon + numLost) * 100);
            var playtime = playerStats.Stats[0].General.Playtime.Seconds().Humanize(maxUnit: TimeUnit.Hour);

            var numDbnos = playerStats.Stats[0].General.Dbnos;
            var numHeadshots = playerStats.Stats[0].General.Headshots;
            var numPenetrations = playerStats.Stats[0].General.PenetrationKills;
            var numMelees = playerStats.Stats[0].General.MeleeKills;
            var numRevives = playerStats.Stats[0].General.Revives;
            var numBlindKills = playerStats.Stats[0].General.BlindKills;

            var numShots = playerStats.Stats[0].General.BulletsFired;
            var numHit = playerStats.Stats[0].General.BulletsHit;
            var shotHitAccuracy = (int) (numHit / (float) numShots * 100);

            var numSuicides = playerStats.Stats[0].General.Suicides;
            var numBarricades = playerStats.Stats[0].General.BarricadesDeployed;
            var numReinforcements = playerStats.Stats[0].General.ReinforcementsDeployed;
            var numGadgetDestroyed = playerStats.Stats[0].General.GadgetsDestroyed;
            var numRappelBreaches = playerStats.Stats[0].General.RappelBreaches;
            var distanceTravelled = $"{playerStats.Stats[0].General.DistanceTravelled / 1000:n0}m";

            var numBombWins = playerStats.Stats[0].Gamemode.Bomb.Wins;
            var numBombLosses = playerStats.Stats[0].Gamemode.Bomb.Losses;
            var bombWinPercent = (int) (numBombWins / (float) (numBombWins + numBombLosses) * 100);
            var numBombBestScore = playerStats.Stats[0].Gamemode.Bomb.BestScore;

            var numHostageWins = playerStats.Stats[0].Gamemode.Hostage.Wins;
            var numHostageLosses = playerStats.Stats[0].Gamemode.Hostage.Losses;
            var hostageWinPercent = (int) (numHostageWins / (float) (numHostageWins + numHostageLosses) * 100);
            var numHostageBestScore = playerStats.Stats[0].Gamemode.Hostage.BestScore;
            var numHostageExtractionsDenied = playerStats.Stats[0].Gamemode.Hostage.ExtractionsDenied;

            var numSecureWins = playerStats.Stats[0].Gamemode.SecureArea.Wins;
            var numSecureLosses = playerStats.Stats[0].Gamemode.SecureArea.Losses;
            var secureWinPercent = (int) (numSecureWins / (float) (numSecureWins + numSecureLosses) * 100);
            var numSecureBestScore = playerStats.Stats[0].Gamemode.SecureArea.BestScore;
            var numKillsAsAttackerInObjective = playerStats.Stats[0].Gamemode.SecureArea.KillsAsAttackerInObjective;
            var numKillsAsDefenderInObjective = playerStats.Stats[0].Gamemode.SecureArea.KillsAsDefenderInObjective;
            var numTimesObjectiveSecured = playerStats.Stats[0].Gamemode.SecureArea.TimesObjectiveSecured;

            var numRankedWon = playerStats.Stats[0].Queue.Ranked.Wins;
            var numRankedLost = playerStats.Stats[0].Queue.Ranked.Losses;
            var rankedWinLoss = playerStats.Stats[0].Queue.Ranked.Wl;
            var rankedKd = playerStats.Stats[0].Queue.Ranked.Kd;
            var winRankedPercent = (int) (numRankedWon / (float) (numRankedWon + numRankedLost) * 100);
            var rankedPlaytime = playerStats.Stats[0].Queue.Ranked.Playtime.Seconds().Humanize(maxUnit: TimeUnit.Hour);

            var embed = new DiscordEmbedBuilder()
                .WithColor(PDiscordColor.SiegeYellow)
                .WithThumbnailUrl($"https://ubisoft-avatars.akamaized.net/{ubisoftId}/default_146_146.png")
                .WithAuthor(
                    $"{username}'s Stats"
                )
                .AddField("Kill/Death",
                    $"**Kills:** {numKills}\n**Deaths:** {numDeaths}\n**Assists:** {numAssists}\n**K/D:** {kd}", true)
                .AddField("Win/Loss",
                    $"**Won:** {numWon} ({winPercent}%)\n**Lost:** {numLost} ({100 - winPercent}%)\n**W/L:** {winLoss}\n**Playtime:** {playtime}",
                    true)
                .AddField("Stats",
                    $"**DBNOs:** {numDbnos}\n**Headshots:** {numHeadshots}\n**Penetrations:** {numPenetrations}\n**Melees:** {numMelees}\n**Revives:** {numRevives}\n**Blind Kills:** {numBlindKills}",
                    true)
                .AddField("Shots", $"**Fired:** {numShots}\n**Hit:** {numHit}\n**Accuracy:** {shotHitAccuracy}%", true)
                .AddField("Extras",
                    $"**Suicides:** {numSuicides}\n**Barricades:** {numBarricades}\n**Reinforcements:** {numReinforcements}\n**Gadgets Destroyed:** {numGadgetDestroyed}\n**Rappel Breaches:** {numRappelBreaches}\n**Disatance Travelled:** {distanceTravelled}",
                    true)
                .AddField("Bomb",
                    $"**Wins:** {numBombWins} ({bombWinPercent}%)\n**Losses:** {numBombLosses} ({100 - bombWinPercent}%)\n**Best Score:** {numBombBestScore}",
                    true)
                .AddField("Hostage",
                    $"**Wins:** {numHostageWins} ({hostageWinPercent}%)\n**Losses:** {numHostageLosses} ({100 - hostageWinPercent}%)\n**Best Score:** {numHostageBestScore}\n**Extractions Denied:** {numHostageExtractionsDenied}",
                    true)
                .AddField("Secure",
                    $"**Wins:** {numSecureWins} ({secureWinPercent}%)\n**Losses:** {numSecureLosses} ({100 - secureWinPercent}%)\n**Best Score:** {numSecureBestScore}\n**Obj. Kills (ATK):** {numKillsAsAttackerInObjective}\n**Obj. Kills (DEF):** {numKillsAsDefenderInObjective}\n**Objectives Secured:** {numTimesObjectiveSecured}",
                    true)
                .AddField("Quick Rank",
                    $"**Wins:** {numRankedWon} ({winRankedPercent}%)\n**Losses:** {numRankedLost} ({100 - winRankedPercent}%)\n**W/L:** {rankedWinLoss}\n**K/D:** {rankedKd}\n**Playtime:** {rankedPlaytime}",
                    true);

            await ctx.RespondAsync(embed: embed);
        }
    }
}