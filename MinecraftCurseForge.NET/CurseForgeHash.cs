using System.Text.Json.Serialization;

namespace MinecraftCurseForge.NET
{
	public class CurseForgeHash
	{
		[JsonPropertyName("value")] public string Value { get; set; }

		[JsonPropertyName("algo")] public CurseForgeHashAlgorithm Algo { get; set; }
	}
}