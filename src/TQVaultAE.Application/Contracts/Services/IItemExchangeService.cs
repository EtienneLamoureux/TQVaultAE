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
	Task<string> ExportToPasteBinAsync(string json, string? pasteName = null);

	/// <summary>Exports an item to PasteBin, the paste named after the item.</summary>
	/// <returns>The URL of the paste.</returns>
	Task<string> ExportItemToPasteBinAsync(Item item);
	Task<string> ImportFromPasteBinAsync(string pasteUrl);

	/// <summary>
	/// Imports a text that is either a PasteBin URL, fetched first, or the export JSON itself.
	/// A PasteBin fetch error is returned as a failed result.
	/// </summary>
	Task<ImportResult> ImportFromTextAsync(string text);

	/// <summary>
	/// Reads an export JSON file and imports it.
	/// </summary>
	ImportResult ImportFromFile(string path);

	/// <summary>
	/// Writes the export JSON of a vault to a file.
	/// </summary>
	void ExportVaultToFile(PlayerCollection vault, string path);

	/// <summary>
	/// Serializes the items a user acts on: one item as an item export, several as a multi-select export.
	/// </summary>
	/// <returns>The export JSON, <see langword="null"/> when there is no item.</returns>
	string? SerializeSelection(IReadOnlyList<Item> items);

	/// <summary>
	/// Writes an export JSON to a file.
	/// </summary>
	void ExportToFile(string json, string path);

	/// <summary>
	/// Exports the items a user acts on to PasteBin: one item like <see cref="ExportItemToPasteBinAsync"/>,
	/// several as a multi-select export.
	/// </summary>
	/// <returns>The URL of the paste.</returns>
	Task<string> ExportSelectionToPasteBinAsync(IReadOnlyList<Item> items);

	/// <summary>
	/// Builds the PasteBin name of a tab export: the vault name, then the tab label or <c>Tab n</c> (1-based) when it has none.
	/// </summary>
	/// <param name="vaultName">Name of the vault or character the tab belongs to.</param>
	/// <param name="sack">The tab.</param>
	/// <param name="sackNumber">0-based number of the tab, as exported.</param>
	string BuildTabPasteName(string vaultName, SackCollection sack, int sackNumber);

	/// <summary>
	/// Exports a tab to PasteBin, the paste named by <see cref="BuildTabPasteName"/>.
	/// </summary>
	/// <param name="vaultName">Name of the vault or character the tab belongs to.</param>
	/// <param name="sack">The tab.</param>
	/// <param name="sackNumber">0-based number of the tab, as exported.</param>
	/// <returns>The URL of the paste.</returns>
	Task<string> ExportTabToPasteBinAsync(string vaultName, SackCollection sack, int sackNumber);

	/// <summary>
	/// Places the items of an item, multi-select or tab import into a sack.
	/// Each item keeps its exported position when those cells are free, else it takes the first open cells;
	/// an item that does not fit is skipped. A tab import also gives its icon to the sack.
	/// </summary>
	/// <returns>The number of items placed.</returns>
	int PlaceImportedItems(SackCollection sack, ImportResult importResult, int sackWidth, int sackHeight);

	/// <summary>
	/// Gets whether no sack of the vault holds an item.
	/// </summary>
	bool IsVaultEmpty(PlayerCollection vault);

	/// <summary>
	/// Builds the name of a vault created by an import: the exported name followed by a timestamp.
	/// </summary>
	string BuildImportedVaultName(string baseName, DateTime now);

	/// <summary>
	/// Creates a new vault named after the import and the time, fills it with the imported sacks and saves it.
	/// </summary>
	/// <returns>The name of the new vault.</returns>
	string ImportAsNewVault(ImportResult importResult, DateTime now);
}