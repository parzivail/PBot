﻿using System.Text.Json.Serialization;

namespace MinecraftCurseForge.NET
{
	public class CurseForgeModAuthor
	{
		[JsonPropertyName("id")] public int Id { get; set; }

		[JsonPropertyName("name")] public string Name { get; set; }

		[JsonPropertyName("url")] public string Url { get; set; }
	}
}