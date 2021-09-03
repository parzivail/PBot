using System;
using System.Collections.Generic;
using LiteDB;
using Newtonsoft.Json;

namespace DiscordPBot.Model
{
	public class CurseForgeFiles
    {
        [JsonProperty("id"), BsonId(false)]
        public int Id { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("fileDate")]
        public DateTime FileDate { get; set; }
    }
}