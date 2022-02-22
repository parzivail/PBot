using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace MinecraftCurseForge.NET
{
	public class CurseForgeApi
	{
		private const string BaseUrl = "https://api.curseforge.com";
		private readonly HttpClient _client = new();

		public CurseForgeApi(string apiKey)
		{
			_client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
			_client.DefaultRequestHeaders.Add("Accept", "application/json");
		}

		public async Task<CurseForgeMod> GetMod(int modId)
		{
			return JsonSerializer.Deserialize<DataResponse<CurseForgeMod>>(await _client.GetStringAsync(BaseUrl + $"/v1/mods/{modId}")).Data;
		}

		public async Task<List<CurseForgeMod>> GetMods(params int[] modIds)
		{
			var req = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "/v1/mods");

			req.Content = JsonContent.Create(new Dictionary<string, int[]>
			{
				["modIds"] = modIds
			});

			var res = await _client.SendAsync(req);
			var data = JsonSerializer.Deserialize<CurseForgeModsResponse>(await res.Content.ReadAsStringAsync());
			return data.Data;
		}

		public async Task<string> GetModDescription(int modId)
		{
			return JsonSerializer.Deserialize<DataResponse<string>>(await _client.GetStringAsync(BaseUrl + $"/v1/mods/{modId}/description")).Data;
		}

		public async Task<CurseForgeFilesResponse> GetModFiles(int modId, int? gameVersionTypeId = null, int? firstItemIndex = null, int? pageSize = null)
		{
			var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + $"/v1/mods/{modId}/files");

			var dict = new Dictionary<string, int>();

			if (gameVersionTypeId != null)
				dict["gameVersionTypeId"] = gameVersionTypeId.Value;
			if (firstItemIndex != null)
				dict["index"] = firstItemIndex.Value;
			if (pageSize != null)
				dict["pageSize"] = pageSize.Value;

			if (dict.Count > 0)
				req.Content = new StringContent(JsonSerializer.Serialize(dict));

			var res = await _client.SendAsync(req);
			return JsonSerializer.Deserialize<CurseForgeFilesResponse>(await res.Content.ReadAsStringAsync());
		}

		public async Task<CurseForgeModFile> GetModFile(int modId, int fileId)
		{
			return JsonSerializer.Deserialize<DataResponse<CurseForgeModFile>>(await _client.GetStringAsync(BaseUrl + $"/v1/mods/{modId}/files/{fileId}")).Data;
		}

		public async Task<string> GetModFileChangelog(int modId, int fileId)
		{
			return JsonSerializer.Deserialize<DataResponse<string>>(await _client.GetStringAsync(BaseUrl + $"/v1/mods/{modId}/files/{fileId}/changelog")).Data;
		}

		public async Task<string> GetModFileDownloadUrl(int modId, int fileId)
		{
			return JsonSerializer.Deserialize<DataResponse<string>>(await _client.GetStringAsync(BaseUrl + $"/v1/mods/{modId}/files/{fileId}/download-url")).Data;
		}
	}
}