using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MinecraftCurseForge.NET
{
	public class CurseForgeMod
	{
		[JsonPropertyName("id")] public int Id { get; set; }

		[JsonPropertyName("gameId")] public int GameId { get; set; }

		[JsonPropertyName("name")] public string Name { get; set; }

		[JsonPropertyName("slug")] public string Slug { get; set; }

		[JsonPropertyName("links")] public CurseForgeModLinks Links { get; set; }

		[JsonPropertyName("summary")] public string Summary { get; set; }

		[JsonPropertyName("status")] public CurseForgeModStatus Status { get; set; }

		[JsonPropertyName("downloadCount")] public double DownloadCount { get; set; }

		[JsonPropertyName("isFeatured")] public bool IsFeatured { get; set; }

		[JsonPropertyName("primaryCategoryId")]
		public int PrimaryCategoryId { get; set; }

		[JsonPropertyName("categories")] public List<CurseForgeModCategory> Categories { get; set; }

		[JsonPropertyName("classId")] public int? ClassId { get; set; }

		[JsonPropertyName("authors")] public List<CurseForgeModAuthor> Authors { get; set; }

		[JsonPropertyName("logo")] public CurseForgeModLogo Logo { get; set; }

		[JsonPropertyName("screenshots")] public List<CurseForgeModScreenshot> Screenshots { get; set; }

		[JsonPropertyName("mainFileId")] public int MainFileId { get; set; }

		[JsonPropertyName("latestFiles")] public List<CurseForgeModFile> LatestFiles { get; set; }

		[JsonPropertyName("latestFilesIndexes")]
		public List<CurseForgeModFileIndex> LatestFilesIndexes { get; set; }

		[JsonPropertyName("dateCreated")] public DateTime DateCreated { get; set; }

		[JsonPropertyName("dateModified")] public DateTime DateModified { get; set; }

		[JsonPropertyName("dateReleased")] public DateTime DateReleased { get; set; }

		[JsonPropertyName("allowModDistribution")]
		public bool? AllowModDistribution { get; set; }

		[JsonPropertyName("gamePopularityRank")]
		public int GamePopularityRank { get; set; }
	}
}