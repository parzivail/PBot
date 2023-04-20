using System;
using System.Collections.Generic;
using LiteDB;
using Newtonsoft.Json;

namespace DiscordPBot.Model
{
	public class ModrinthFileDatabaseEntry
    {
        [BsonId(false)]
        public string Id { get; set; }
        public string VersionNumber { get; set; }
        public string FileName { get; set; }
        public DateTime FileDate { get; set; }
    }
}