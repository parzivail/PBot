using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MinecraftCurseForge.NET
{
	public class CurseForgeModFile
	{
		[JsonPropertyName("id")]
		public int Id { get; set; }

		[JsonPropertyName("gameId")]
		public int GameId { get; set; }

		[JsonPropertyName("modId")]
		public int ModId { get; set; }

		[JsonPropertyName("isAvailable")]
		public bool IsAvailable { get; set; }

		[JsonPropertyName("displayName")]
		public string DisplayName { get; set; }

		[JsonPropertyName("fileName")]
		public string FileName { get; set; }

		[JsonPropertyName("releaseType")]
		public CurseForgeFileReleaseType ReleaseType { get; set; }

		[JsonPropertyName("fileStatus")]
		public CurseForgeFileStatus FileStatus { get; set; }

		[JsonPropertyName("hashes")]
		public List<CurseForgeHash> Hashes { get; set; }

		[JsonPropertyName("fileDate")]
		public DateTime FileDate { get; set; }

		[JsonPropertyName("fileLength")]
		public long FileLength { get; set; }

		[JsonPropertyName("downloadCount")]
		public long DownloadCount { get; set; }

		[JsonPropertyName("downloadUrl")]
		public string DownloadUrl { get; set; }

		[JsonPropertyName("gameVersions")]
		public List<string> GameVersions { get; set; }

		[JsonPropertyName("sortableGameVersions")]
		public List<CurseForgeSortableGameVersion> SortableGameVersions { get; set; }

		[JsonPropertyName("dependencies")]
		public List<CurseForgeDependency> Dependencies { get; set; }

		[JsonPropertyName("exposeAsAlternative")]
		public bool? ExposeAsAlternative { get; set; }

		[JsonPropertyName("parentProjectFileId")]
		public int? ParentProjectFileId { get; set; }

		[JsonPropertyName("alternateFileId")]
		public int? AlternateFileId { get; set; }

		[JsonPropertyName("isServerPack")]
		public bool? IsServerPack { get; set; }

		[JsonPropertyName("serverPackFileId")]
		public int? ServerPackFileId { get; set; }

		[JsonPropertyName("fileFingerprint")]
		public long FileFingerprint { get; set; }

		[JsonPropertyName("modules")]
		public List<CurseForgeModule> Modules { get; set; }
	}
}