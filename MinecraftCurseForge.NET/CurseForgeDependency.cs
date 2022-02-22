using System.Text.Json.Serialization;

namespace MinecraftCurseForge.NET
{
	public class CurseForgeDependency
	{
		[JsonPropertyName("modId")] public int ModId { get; set; }

		[JsonPropertyName("fileId")] public int FileId { get; set; }

		[JsonPropertyName("relationType")] public CurseForgeFileRelationType RelationType { get; set; }
	}
}