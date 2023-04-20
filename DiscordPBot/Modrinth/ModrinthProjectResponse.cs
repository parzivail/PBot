using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace DiscordPBot.Modrinth;

public record ModrinthProjectResponse(
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("slug")] string Slug,
	[property: JsonPropertyName("title")] string Title,
	[property: JsonPropertyName("downloads")] int Downloads,
	[property: JsonPropertyName("followers")] int Followers,
	[property: JsonPropertyName("icon_url")] string IconUrl
);