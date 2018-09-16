using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DiscordPBot.RainbowSix;
using DiscordPBot.Util;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Humanizer.Localisation;
using Newtonsoft.Json;

namespace DiscordPBot.Commands
{
    partial class PCommands
    {
        [Command("r6op"), Description("Get operator stats about a player on PC.")]
        public async Task Rainbow6Op(CommandContext ctx, string username)
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
                catch (WebException)
                {
                    await ctx.RespondAsync(":warning: No players found with that username.");
                    return;
                }
                catch (JsonSerializationException e)
                {
                    PBot.LogError($"r6op search JsonSerializationException: {e.Message}");
                    await ctx.RespondAsync(":interrobang: Could not load players.");
                    return;
                }
                catch (JsonReaderException e)
                {
                    PBot.LogError($"r6op search JsonSerializationException: {e.Message}");
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

                embed = embed.AddField($"{op.Operator.Name} ({op.Operator.Role})", $"**Kills:** {op.Kills}\n**Deaths:** {op.Deaths}\n**K/D:** {op.Kd}\n**Playtime:** {op.Playtime.Seconds().Humanize(maxUnit: TimeUnit.Hour)}{extras}", true);
            }

            await ctx.RespondAsync(embed: embed);
        }
    }
}
