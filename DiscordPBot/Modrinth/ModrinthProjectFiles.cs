using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace DiscordPBot.Modrinth;

public record ModrinthProjectFiles(
	[property: JsonPropertyName("url")] string Url,
	[property: JsonPropertyName("filename")] string Filename,
	[property: JsonPropertyName("primary")] bool Primary,
	[property: JsonPropertyName("size")] int Size
);