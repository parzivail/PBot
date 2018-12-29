using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
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
        [Group("r6", CanInvokeWithoutSubcommand = true)]
        class GroupR6
        {
            private static readonly R6Rank[] Ranks =
            {
                new R6Rank
                {
                    Name = "Unranked",
                    ImageUrl = "unranked"
                },
                new R6Rank
                {
                    Name = "Copper IV",
                    ImageUrl = "copper-4"
                },
                new R6Rank
                {
                    Name = "Copper III",
                    ImageUrl = "copper-3"
                },
                new R6Rank
                {
                    Name = "Copper II",
                    ImageUrl = "copper-2"
                },
                new R6Rank
                {
                    Name = "Copper I",
                    ImageUrl = "copper-1"
                },
                new R6Rank
                {
                    Name = "Bronze IV",
                    ImageUrl = "bronze-4"
                },
                new R6Rank
                {
                    Name = "Bronze III",
                    ImageUrl = "bronze-3"
                },
                new R6Rank
                {
                    Name = "Bronze II",
                    ImageUrl = "bronze-2"
                },
                new R6Rank
                {
                    Name = "Bronze I",
                    ImageUrl = "bronze-1"
                },
                new R6Rank
                {
                    Name = "Silver IV",
                    ImageUrl = "silver-4"
                },
                new R6Rank
                {
                    Name = "Silver III",
                    ImageUrl = "silver-3"
                },
                new R6Rank
                {
                    Name = "Silver II",
                    ImageUrl = "silver-2"
                },
                new R6Rank
                {
                    Name = "Silver I",
                    ImageUrl = "silver-1"
                },
                new R6Rank
                {
                    Name = "Gold IV",
                    ImageUrl = "gold-4"
                },
                new R6Rank
                {
                    Name = "Gold III",
                    ImageUrl = "gold-3"
                },
                new R6Rank
                {
                    Name = "Gold II",
                    ImageUrl = "gold-2"
                },
                new R6Rank
                {
                    Name = "Gold I",
                    ImageUrl = "gold-1"
                },
                new R6Rank
                {
                    Name = "Platinum III",
                    ImageUrl = "platinum-3"
                },
                new R6Rank
                {
                    Name = "Platinum II",
                    ImageUrl = "platinum-2"
                },
                new R6Rank
                {
                    Name = "Platinum I",
                    ImageUrl = "platinum-1"
                },
                new R6Rank
                {
                    Name = "Diamond",
                    ImageUrl = "diamond"
                },
            };

            public async Task ExecuteGroupAsync(CommandContext ctx, string username)
            {
                await Rainbow6(ctx, username);
            }

            [Command("rank")]
            [Description("Get stats about a player on PC.")]
            public async Task Rainbow6Rank(CommandContext ctx, string username, [RemainingText] string seasonName)
            {
                var message = await ctx.RespondAsync(":clock10: Contacting player database...");

                if (seasonName == null)
                    seasonName = "";
                seasonName = seasonName.Trim().Replace(" ", "_").ToLower();

                R6PlayerSeasonStats playerStats;
                
                using (var wc = new WebClient())
                {
                    var player = await SiegeUtils.GetPlayer(ctx, username, wc);

                    if (player == null)
                    {
                        await message.ModifyAsync(":warning: No players found with that username.");
                        return;
                    }

                    try
                    {
                        var reqUrl = $"https://www.r6stats.com/api/stats/{player.UbisoftId}/seasonal";
                        var json = wc.DownloadString(reqUrl);
                        playerStats = JsonConvert.DeserializeObject<R6PlayerSeasonStats>(json);
                    }
                    catch (WebException e)
                    {
                        PBot.LogError($"r6 rank WebException: {e.Message}");
                        await message.ModifyAsync($":interrobang: Could not fetch player stats. ```WebException {e.Message}```");
                        return;
                    }
                    catch (JsonSerializationException e)
                    {
                        PBot.LogError($"r6 rank JsonSerializationException: {e.Message}");
                        await message.ModifyAsync($":interrobang: Could not load player stats. ```JsonSerializationException {e.Message}```");
                        return;
                    }
                    catch (JsonReaderException e)
                    {
                        PBot.LogError($"r6 rank JsonReaderException: {e.Message}");
                        await message.ModifyAsync($":interrobang: Could not load player stats. ```JsonReaderException {e.Message}```");
                        return;
                    }
                    catch (Exception e)
                    {
                        PBot.LogError($"r6 rank Exception: {e.Message}");
                        await message.ModifyAsync($":interrobang: Could not load player stats. ```Exception {e.Message}```");
                        return;
                    }
                }

                if (string.IsNullOrWhiteSpace(seasonName))
                    seasonName = playerStats.Seasons.Values.OrderByDescending(season1 => season1.StartDate).First().Key;

                if (!playerStats.Seasons.ContainsKey(seasonName))
                {
                    await message.ModifyAsync(":warning: No seasonName found with that name.");
                    return;
                }

                var season = playerStats.Seasons[seasonName];
                var stats = season.Regions.Ncsa[0];

                var totalGames = stats.Wins + stats.Losses + stats.Abandons;
                var winPercent = totalGames == 0 ? 0 : (int) (stats.Wins / (float) totalGames * 100);
                var lossPercent = totalGames == 0 ? 0 : (int) (stats.Losses / (float) totalGames * 100);
                var abandonPercent = totalGames == 0 ? 0 : (int) (stats.Abandons / (float) totalGames * 100);

                username = playerStats.Username;
                var ubisoftId = playerStats.UbisoftId;

                var embed = new DiscordEmbedBuilder()
                    .WithColor(PDiscordColor.SiegeYellow)
                    .WithThumbnailUrl(
                        $"https://raw.githubusercontent.com/parzivail/PBot/master/DiscordPBot/Resources/R6RankImages/{Ranks[stats.Rank].ImageUrl}.png")
                    .WithAuthor(
                        $"{username}'s Rank (Operation {season.Name})",
                        icon_url: $"https://ubisoft-avatars.akamaized.net/{ubisoftId}/default_146_146.png"
                    )
                    .AddField("Win/Loss",
                        $"**Wins**: {stats.Wins} ({winPercent}%)\n" +
                        $"**Losses**: {stats.Losses} ({lossPercent}%)\n" +
                        $"**Abandons**: {stats.Abandons} ({abandonPercent}%)",
                        true)
                    .AddField("Rank",
                        $"**Rank**: {Ranks[stats.Rank].Name}\n" +
                        $"**Max Rank**: {Ranks[stats.MaxRank].Name}",
                        true)
                    .AddField("Stats",
                        $"**MMR**: {stats.Mmr}\n" +
                        $"**Max MMR**: {stats.MaxMmr}\n" +
                        $"**Skill (mean)**: {stats.SkillMean}\n" +
                        $"**Skill (std. dev.)**: {stats.SkillStandardDeviation}",
                        true);
                
                await message.ModifyAsync("", embed);
            }

            [Command("stats")]
            [Description("Get stats about a player on PC.")]
            public async Task Rainbow6(CommandContext ctx, string username)
            {
                var message = await ctx.RespondAsync(":clock10: Contacting player database...");

                R6PlayerStatsJson playerStats;

                using (var wc = new WebClient())
                {
                    var player = await SiegeUtils.GetPlayer(ctx, username, wc);

                    if (player == null)
                    {
                        await message.ModifyAsync(":warning: No players found with that username.");
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
                        await message.ModifyAsync($":interrobang: Could not fetch player stats. ```WebException {e.Message}```");
                        return;
                    }
                    catch (JsonSerializationException e)
                    {
                        PBot.LogError($"r6 stats JsonSerializationException: {e.Message}");
                        await message.ModifyAsync($":interrobang: Could not load player stats. ```JsonSerializationException {e.Message}```");
                        return;
                    }
                    catch (JsonReaderException e)
                    {
                        PBot.LogError($"r6 stats JsonReaderException: {e.Message}");
                        await message.ModifyAsync($":interrobang: Could not load player stats. ```JsonReaderException {e.Message}```");
                        return;
                    }
                    catch (Exception e)
                    {
                        PBot.LogError($"r6 stats Exception: {e.Message}");
                        await message.ModifyAsync($":interrobang: Could not load player stats. ```Exception {e.Message}```");
                        return;
                    }
                }

                var stats = playerStats.Stats[0];

                username = playerStats.Username;
                var ubisoftId = playerStats.UbisoftId;

                var winPercent = (int) (stats.General.Wins / (float) (stats.General.Wins + stats.General.Losses) * 100);
                var playtime = stats.General.Playtime.Seconds().Humanize(maxUnit: TimeUnit.Hour);
                var shotHitAccuracy = (int) (stats.General.BulletsHit / (float) stats.General.BulletsFired * 100);
                var bombWinPercent = (int) (stats.Gamemode.Bomb.Wins /
                                            (float) (stats.Gamemode.Bomb.Wins + stats.Gamemode.Bomb.Losses) * 100);
                var hostageWinPercent = (int) (stats.Gamemode.Hostage.Wins /
                                               (float) (stats.Gamemode.Hostage.Wins + stats.Gamemode.Hostage.Losses) *
                                               100);
                var secureWinPercent = (int) (stats.Gamemode.SecureArea.Wins /
                                              (float) (stats.Gamemode.SecureArea.Wins +
                                                       stats.Gamemode.SecureArea.Losses) * 100);
                var winRankedPercent = (int) (stats.Queue.Ranked.Wins /
                                              (float) (stats.Queue.Ranked.Wins + stats.Queue.Ranked.Losses) * 100);
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

                await message.ModifyAsync("", embed);
            }

            [Command("ops")]
            [Description("Get operator stats about a player on PC.")]
            public async Task Rainbow6Op(CommandContext ctx, string username)
            {
                var message = await ctx.RespondAsync(":clock10: Contacting player database...");

                R6PlayerStatsJson playerStats;

                using (var wc = new WebClient())
                {
                    var player = await SiegeUtils.GetPlayer(ctx, username, wc);

                    if (player == null)
                    {
                        await message.ModifyAsync(":warning: No players found with that username.");
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
                        PBot.LogError($"r6 ops WebException: {e.Message}");
                        await message.ModifyAsync($":interrobang: Could not fetch player stats. ```WebException {e.Message}```");
                        return;
                    }
                    catch (JsonSerializationException e)
                    {
                        PBot.LogError($"r6 ops JsonSerializationException: {e.Message}");
                        await message.ModifyAsync($":interrobang: Could not load player stats. ```JsonSerializationException {e.Message}```");
                        return;
                    }
                    catch (JsonReaderException e)
                    {
                        PBot.LogError($"r6 ops JsonReaderException: {e.Message}");
                        await message.ModifyAsync($":interrobang: Could not load player stats. ```JsonReaderException {e.Message}```");
                        return;
                    }
                    catch (Exception e)
                    {
                        PBot.LogError($"r6 ops Exception: {e.Message}");
                        await message.ModifyAsync($":interrobang: Could not load player stats. ```Exception {e.Message}```");
                        return;
                    }
                }

                username = playerStats.Username;
                var ubisoftId = playerStats.UbisoftId;

                var embed = new DiscordEmbedBuilder()
                    .WithColor(PDiscordColor.SiegeYellow)
                    .WithThumbnailUrl($"https://ubisoft-avatars.akamaized.net/{ubisoftId}/default_146_146.png")
                    .WithAuthor(
                        $"{username}'s Favorite Ops"
                    );

                var ops = playerStats.Operators.OrderByDescending(stats => stats.Playtime).ToList();

                for (var i = 0; i < Math.Min(ops.Count, 5); i++)
                {
                    var op = ops[i];

                    var extras = new StringBuilder();

                    foreach (var ability in op.Abilities)
                        extras.Append($"\n**{ability.Title}:** {ability.Value}");

                    embed = embed.AddField($"{op.Operator.Name} ({op.Operator.Role})",
                        $"**Kills:** {op.Kills}\n**Deaths:** {op.Deaths}\n**K/D:** {op.Kd}\n**Playtime:** {op.Playtime.Seconds().Humanize(maxUnit: TimeUnit.Hour)}{extras}",
                        true);
                }

                await message.ModifyAsync("", embed);
            }
        }
    }
}