using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace DiscordPBot.Modrinth;

public record ModrinthProjectVersionResponse(
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("project_id")] string ProjectId,
	[property: JsonPropertyName("author_id")] string AuthorId,
	[property: JsonPropertyName("featured")] bool Featured,
	[property: JsonPropertyName("name")] string Name,
	[property: JsonPropertyName("version_number")] string VersionNumber,
	[property: JsonPropertyName("changelog")] string Changelog,
	[property: JsonPropertyName("date_published")] DateTime DatePublished,
	[property: JsonPropertyName("downloads")] int Downloads,
	[property: JsonPropertyName("version_type")] string VersionType,
	[property: JsonPropertyName("status")] string Status,
	[property: JsonPropertyName("files")] ModrinthProjectFiles[] Files,
	[property: JsonPropertyName("game_versions")] string[] GameVersions,
	[property: JsonPropertyName("loaders")] string[] Loaders
);