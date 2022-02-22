using System.Text.Json.Serialization;

namespace MinecraftCurseForge.NET
{
	public class CurseForgeModLinks
	{
		[JsonPropertyName("websiteUrl")] public string WebsiteUrl { get; set; }

		[JsonPropertyName("wikiUrl")] public string WikiUrl { get; set; }

		[JsonPropertyName("issuesUrl")] public string IssuesUrl { get; set; }

		[JsonPropertyName("sourceUrl")] public string SourceUrl { get; set; }
	}
}