using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

public interface IItemExchangeService
{
	string SerializeItem(Item item);
	string SerializeItems(IEnumerable<Item> items);
	string SerializeSackCollection(SackCollection sack, int sackNumber);
	string SerializePlayerCollection(PlayerCollection vault);
	ImportResult ImportFromJson(string json);
	void ImportVaultInto(PlayerCollection vault, ImportResult importData);
	bool IsPasteBinUrl(string text);
	bool HasPasteBinApiKey { get; }
	Task<string> ExportToPasteBinAsync(string json, string pasteName = null);
	Task<string> ImportFromPasteBinAsync(string pasteUrl);
}