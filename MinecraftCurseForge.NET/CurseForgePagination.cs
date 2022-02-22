using System.Text.Json.Serialization;

namespace MinecraftCurseForge.NET
{
	public class CurseForgePagination
	{
		[JsonPropertyName("index")]
		public int Index { get; set; }

		[JsonPropertyName("pageSize")]
		public int PageSize { get; set; }

		[JsonPropertyName("resultCount")]
		public int ResultCount { get; set; }

		[JsonPropertyName("totalCount")]
		public long? TotalCount { get; set; }
	}
}