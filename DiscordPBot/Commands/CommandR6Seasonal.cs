using System.Linq;
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
                ImageUrl = "platinum"
            },
        };

        [Command("r6rank")]
        [Description("Get stats about a player on PC.")]
        public async Task Rainbow6Rank(CommandContext ctx, string username, [RemainingText] string seasonName)
        {
            await ctx.TriggerTypingAsync();

            if (seasonName == null)
                seasonName = "";
            seasonName = seasonName.Trim().Replace(" ", "_").ToLower();

            R6PlayerSeasonStats playerStats;

            using (var wc = new WebClient())
            {
                var player = await SiegeUtils.GetPlayer(ctx, username);

                if (player == null)
                {
                    await ctx.RespondAsync(":warning: No players found with that username.");
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

            if (string.IsNullOrWhiteSpace(seasonName))
                seasonName = playerStats.Seasons.Values.OrderByDescending(season1 => season1.StartDate).First().Key;

            if (!playerStats.Seasons.ContainsKey(seasonName))
            {
                await ctx.RespondAsync(":warning: No seasonName found with that name.");
                return;
            }

            var season = playerStats.Seasons[seasonName];
            var stats = season.Regions.Ncsa[0];

            var totalGames = stats.Wins + stats.Losses + stats.Abandons;
            var winPercent = totalGames == 0 ? 0 : (int)(stats.Wins / (float)totalGames * 100);
            var lossPercent = totalGames == 0 ? 0 : (int)(stats.Losses / (float)totalGames * 100);
            var abandonPercent = totalGames == 0 ? 0 : (int)(stats.Abandons / (float)totalGames * 100);

            username = playerStats.Username;
            var ubisoftId = playerStats.UbisoftId;

            var embed = new DiscordEmbedBuilder()
                .WithColor(PDiscordColor.SiegeYellow)
                .WithThumbnailUrl($"https://raw.githubusercontent.com/parzivail/PBot/master/DiscordPBot/Resources/R6Ranks/{Ranks[stats.Rank].ImageUrl}.png")
                .WithAuthor(
                    $"{username}'s Rank (Operation {season.Name})"
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

            await ctx.RespondAsync(embed: embed);
        }
    }

    internal class R6Rank
    {
        public string ImageUrl { get; set; }
        public string Name { get; set; }
    }
}