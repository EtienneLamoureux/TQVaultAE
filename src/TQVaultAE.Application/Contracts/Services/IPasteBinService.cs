using System.Threading.Tasks;

namespace TQVaultAE.Application.Contracts.Services;

public interface IPasteBinService
{
	Task<string> UploadAsync(string text, string pasteName = null);
	Task<string> FetchPasteAsync(string pasteUrl);
}