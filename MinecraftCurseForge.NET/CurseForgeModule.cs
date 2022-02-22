using System.Text.Json.Serialization;

namespace MinecraftCurseForge.NET
{
	public class CurseForgeModule
	{
		[JsonPropertyName("name")] public string Name { get; set; }

		[JsonPropertyName("fingerprint")] public float Fingerprint { get; set; }
	}
}