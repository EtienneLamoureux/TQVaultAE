using System.Net.Http;
using System.Threading.Tasks;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;

namespace TQVaultAE.Services;

public class PasteBinService(HttpClient httpClient, UserSettings userSettings) : IPasteBinService
{
	private readonly HttpClient _httpClient = httpClient;
	private readonly UserSettings _userSettings = userSettings;

	private const string PasteBinApiUrl = "https://pastebin.com/api/api_post.php";
	private const string PasteBinRawUrl = "https://pastebin.com/raw/{0}";

	public async Task<string> UploadAsync(string text, string? pasteName = null)
	{
		var formData = new Dictionary<string, string>
		{
			["api_dev_key"] = _userSettings.PasteBinApiKey ?? string.Empty,
			["api_option"] = "paste",
			["api_paste_code"] = text,
			["api_paste_private"] = "1",
			["api_paste_expire_date"] = _userSettings.PasteBinExpiration ?? UserSettings.DefaultPasteBinExpiration
		};

		if (!string.IsNullOrWhiteSpace(pasteName))
			formData["api_paste_name"] = pasteName;

		using var content = new FormUrlEncodedContent(formData);

		using var response = await _httpClient.PostAsync(PasteBinApiUrl, content);
		var result = await response.Content.ReadAsStringAsync();

		if (!response.IsSuccessStatusCode)
			throw new InvalidOperationException($"PasteBin API returned {(int)response.StatusCode}: {result.Trim()}");

		if (result.StartsWith("https://pastebin.com/", System.StringComparison.OrdinalIgnoreCase))
			return result.Trim();

		throw new InvalidOperationException($"PasteBin API error: {result}");
	}

	public async Task<string> FetchPasteAsync(string pasteUrl)
	{
		var pasteId = GetPasteIdFromUrl(pasteUrl);
		var rawUrl = string.Format(PasteBinRawUrl, pasteId);

		using var response = await _httpClient.GetAsync(rawUrl);
		var result = await response.Content.ReadAsStringAsync();

		if (!response.IsSuccessStatusCode)
			throw new InvalidOperationException($"PasteBin API returned {(int)response.StatusCode}: {result.Trim()}");

		return result;
	}

	private static string GetPasteIdFromUrl(string url)
	{
		var uri = new Uri(url);
		return uri.AbsolutePath.Trim('/');
	}
}