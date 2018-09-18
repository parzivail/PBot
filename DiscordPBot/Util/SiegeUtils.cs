using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DiscordPBot.Schemas.RainbowSix;
using DSharpPlus.CommandsNext;
using Newtonsoft.Json;

namespace DiscordPBot.Util
{
    class SiegeUtils
    {
        public static async Task<R6PlayerSearchJson> GetPlayer(CommandContext ctx, string username, WebClient wc)
        {
            try
            {
                var json = wc.DownloadString($"https://www.r6stats.com/api/player-search/{username}/pc");
                var results = JsonConvert.DeserializeObject<R6PlayerSearchJson[]>(json);
                return results.Length == 0 ? null : results[0];
            }
            catch (WebException)
            {
                await ctx.RespondAsync(":warning: No players found with that username.");
                return null;
            }
            catch (JsonSerializationException e)
            {
                PBot.LogError($"r6 search JsonSerializationException: {e.Message}");
                await ctx.RespondAsync(":interrobang: Could not load players.");
                return null;
            }
            catch (JsonReaderException e)
            {
                PBot.LogError($"r6 search JsonSerializationException: {e.Message}");
                await ctx.RespondAsync(":interrobang: Could not load players.");
                return null;
            }
        }
    }
}
