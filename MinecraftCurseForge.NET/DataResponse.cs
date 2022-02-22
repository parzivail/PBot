using System.Text.Json.Serialization;

namespace MinecraftCurseForge.NET
{
	public class DataResponse<T>
	{
		[JsonPropertyName("data")]
		public T Data { get; set; }
	}
}