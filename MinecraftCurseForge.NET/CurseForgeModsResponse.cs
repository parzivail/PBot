using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MinecraftCurseForge.NET
{
	public class CurseForgeModsResponse
	{
		[JsonPropertyName("data")] public List<CurseForgeMod> Data { get; set; }
	}
}