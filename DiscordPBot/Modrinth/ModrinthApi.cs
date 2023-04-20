using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MinecraftCurseForge.NET;

namespace DiscordPBot.Modrinth;

public class ModrinthApi
{
	private const string BaseUrl = "https://api.modrinth.com/v2";
	private readonly HttpClient _client = new();

	public ModrinthApi()
	{
		_client.DefaultRequestHeaders.Add("User-Agent", "parzivail/pbot");
	}
	
	public async Task<ModrinthProjectResponse> GetProject(string slug)
	{
		return JsonSerializer.Deserialize<ModrinthProjectResponse>(await _client.GetStringAsync(BaseUrl + $"/project/{slug}"));
	}
	
	public async Task<ModrinthProjectVersionResponse[]> GetProjectVersions(string slug)
	{
		return JsonSerializer.Deserialize<ModrinthProjectVersionResponse[]>(await _client.GetStringAsync(BaseUrl + $"/project/{slug}/version"));
	}
}