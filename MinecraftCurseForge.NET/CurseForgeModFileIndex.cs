using System.Text.Json.Serialization;

namespace MinecraftCurseForge.NET
{
	public class CurseForgeModFileIndex
	{
		[JsonPropertyName("gameVersion")] public string GameVersion { get; set; }

		[JsonPropertyName("fileId")] public int FileId { get; set; }

		[JsonPropertyName("filename")] public string Filename { get; set; }

		[JsonPropertyName("releaseType")] public int ReleaseType { get; set; }

		[JsonPropertyName("gameVersionTypeId")]
		public int? GameVersionTypeId { get; set; }

		[JsonPropertyName("modLoader")] public CurseForgeModLoaderType? ModLoader { get; set; }
	}
}