using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MinecraftCurseForge.NET
{
	public class CurseForgeFilesResponse
	{
		[JsonPropertyName("data")]
		public List<CurseForgeModFile> Files { get; set; }

		[JsonPropertyName("pagination")]
		public CurseForgePagination Pagination { get; set; }
	}
}