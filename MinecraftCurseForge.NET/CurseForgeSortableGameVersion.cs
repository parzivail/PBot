using System;
using System.Text.Json.Serialization;

namespace MinecraftCurseForge.NET
{
	public class CurseForgeSortableGameVersion
	{
		[JsonPropertyName("gameVersionName")] public string GameVersionName { get; set; }

		[JsonPropertyName("gameVersionPadded")]
		public string GameVersionPadded { get; set; }

		[JsonPropertyName("gameVersion")] public string GameVersion { get; set; }

		[JsonPropertyName("gameVersionReleaseDate")]
		public DateTime GameVersionReleaseDate { get; set; }

		[JsonPropertyName("gameVersionTypeId")]
		public int? GameVersionTypeId { get; set; }
	}
}