using System.Text;
using System.Text.Json;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Data.Dto;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

public class ItemExchangeService(
	JsonSerializerOptions jsonOptions
	, IPasteBinService pasteBinService = null!
	, UserSettings userSettings = null!
	, IItemProvider itemProvider = null!
	, IItemMovementService itemMovementService = null!
	, IFileIO fileIO = null!
	, IVaultService vaultService = null!
	, IItemModService? itemModService = null
) : IItemExchangeService
{
	private readonly JsonSerializerOptions _jsonOptions = jsonOptions;
	private readonly IPasteBinService _pasteBinService = pasteBinService;
	private readonly UserSettings _userSettings = userSettings;
	private readonly IItemProvider _itemProvider = itemProvider;
	private readonly IItemMovementService _itemMovementService = itemMovementService;
	private readonly IFileIO _fileIO = fileIO;
	private readonly IVaultService _vaultService = vaultService;
	private readonly IItemModService? _itemModService = itemModService;

	/// <summary>The file name offered when a vault tab is exported to a file.</summary>
	public const string TabExportFileName = "tab_export.json";

	/// <summary>The file name offered when items are exported to a file: one item or a selection.</summary>
	/// <param name="itemCount">The number of items exported.</param>
	public static string ItemsExportFileName(int itemCount)
		=> itemCount == 1 ? "item_export.json" : "items_export.json";

	/// <summary>
	/// A mod named by an export, when it is installed here; else null, and its name is added to <paramref name="dropped"/>:
	/// the link is dropped and the items fall back on their target container's map until the user links them.
	/// </summary>
	private string? Installed(string? mod, ICollection<string> dropped)
	{
		if (string.IsNullOrWhiteSpace(mod))
			return null;

		if (this._itemModService is null || this._itemModService.Find(mod) is not null)
			return mod;

		if (!dropped.Contains(mod, StringComparer.OrdinalIgnoreCase))
			dropped.Add(mod);

		return null;
	}

	/// <summary>
	/// An imported item linked to a mod not installed here loses that link: it takes its target container's map.
	/// </summary>
	private Item DropMissingMod(Item item, ICollection<string> dropped)
	{
		if (item.HasOwnMod && item.Mod is not null && this.Installed(item.Mod, dropped) is null)
		{
			item.Mod = null;
			item.ModOrigin = ModOrigin.None;
		}

		return item;
	}

	public string SerializeItem(Item item)
	{
		var dto = ItemDtoExtensions.FromItem(item);
		var envelope = new ExportFormat
		{
			Scope = ExportScope.Item,
			Data = dto
		};

		return JsonSerializer.Serialize(envelope, _jsonOptions);
	}

	public string SerializeItems(IEnumerable<Item> items)
	{
		var dtos = items.Select(i => ItemDtoExtensions.FromItem(i)).ToList();
		var envelope = new ExportFormat
		{
			Scope = ExportScope.MultiSelect,
			Data = dtos
		};

		return JsonSerializer.Serialize(envelope, _jsonOptions);
	}

	public string SerializeSackCollection(SackCollection sack, int sackNumber, string? containerMod = null)
	{
		var dto = TabExportDTO.FromSackCollection(sack, sackNumber, containerMod);
		var envelope = new ExportFormat
		{
			Scope = ExportScope.Tab,
			Data = dto
		};

		return JsonSerializer.Serialize(envelope, _jsonOptions);
	}

	public string SerializePlayerCollection(PlayerCollection vault)
	{
		var dto = VaultExportDTO.FromPlayerCollection(vault);
		var envelope = new ExportFormat
		{
			Scope = ExportScope.Vault,
			Data = dto
		};

		return JsonSerializer.Serialize(envelope, _jsonOptions);
	}


	public ImportResult ImportFromJson(string json)
	{
		try
		{
			using var doc = JsonDocument.Parse(json);
			var root = doc.RootElement;

			if (!root.TryGetProperty("formatVersion", out var fv) || fv.GetInt32() != 1)
				return ImportResult.Failed("Invalid or unsupported export format.");

			if (!root.TryGetProperty("scope", out var scopeElem))
				return ImportResult.Failed("Missing scope in export format.");

			var scopeText = scopeElem.GetString();
			if (!Enum.TryParse<ExportScope>(scopeText, ignoreCase: true, out var scope))
				return ImportResult.Failed($"Unsupported scope: {scopeText}");

			if (!root.TryGetProperty("data", out var dataElement))
				return ImportResult.Failed("Missing data in export format.");

			var dropped = new List<string>();
			var result = this.Import(scope, dataElement, dropped);
			result.DroppedMods = dropped;
			return result;
		}
		catch (JsonException ex)
		{
			return ImportResult.Failed($"Invalid JSON format: {ex.Message}");
		}
	}

	private ImportResult Import(ExportScope scope, JsonElement dataElement, List<string> dropped)
	{
		{
			switch (scope)
			{
				case ExportScope.Item:
					{
						var dto = JsonSerializer.Deserialize<ItemDto>(dataElement.GetRawText(), _jsonOptions);

						if (dto is null)
							return ImportResult.Failed("Failed to deserialize item data.");

						var item = this.DropMissingMod(dto.ToItem(), dropped);
						_itemProvider?.GetDBData(item);
						return ImportResult.Succeeded(item);
					}

				case ExportScope.Tab:
					{
						var dto = JsonSerializer.Deserialize<TabExportDTO>(dataElement.GetRawText(), _jsonOptions);

						if (dto is null)
							return ImportResult.Failed("Failed to deserialize tab data.");

						// The items without an own map inherited the tab's container's map (a mod, or the base game): they
						// keep it, placed by PlaceImportedItems (inherited in a vault of that map, else as their own). A mod
						// not installed here is dropped: they take their target's map.
						string? tabMod = this.Installed(dto.Mod, dropped);
						bool tabMapKnown = tabMod is not null || string.IsNullOrWhiteSpace(dto.Mod);
						var items = new List<Item>();
						foreach (var itemDto in dto.Items)
						{
							var item = this.DropMissingMod(itemDto.ToItem(), dropped);
							if (item.ModOrigin == ModOrigin.None && tabMapKnown && !itemDto.IsDroppedOwnMod(item))
							{
								item.Mod = tabMod;
								item.ModOrigin = ModOrigin.Inherited;
								item.InVault = true;
							}

							_itemProvider?.GetDBData(item);
							items.Add(item);
						}

						return ImportResult.SucceededTab(items, dto.SackNumber, dto.IconInfo, tabMod, tabMapKnown);
					}

				case ExportScope.MultiSelect:
				{
					var dtos = JsonSerializer.Deserialize<List<ItemDto>>(dataElement.GetRawText(), _jsonOptions);

					if (dtos is null || dtos.Count == 0)
						return ImportResult.Failed("Failed to deserialize multi-select item data.");

					var items = dtos
						.Select(dto =>
						{
							var item = this.DropMissingMod(dto.ToItem(), dropped);
							_itemProvider?.GetDBData(item);
							return item;
						})
						.ToList();

					return ImportResult.SucceededMultiSelect(items);
				}

			case ExportScope.Vault:
					{
						var dto = JsonSerializer.Deserialize<VaultExportDTO>(dataElement.GetRawText(), _jsonOptions);

						if (dto is null)
							return ImportResult.Failed("Failed to deserialize vault data.");

						string? vaultMod = this.Installed(dto.Mod, dropped);
						var sackItems = new Dictionary<int, List<Item>>();
						var sackIconInfo = new Dictionary<int, BagButtonIconInfo>();
						foreach (var sackDto in dto.Sacks)
						{
							var itemList = new List<Item>();
							foreach (var itemDto in sackDto.Items)
							{
								// An item without an own map inherits the vault's.
								var item = this.DropMissingMod(itemDto.ToItem(), dropped);
								item.InVault = true;
								item.InheritMod(vaultMod);
								_itemProvider?.GetDBData(item);
								itemList.Add(item);
							}

							sackItems[sackDto.SackNumber] = itemList;
							if (sackDto.IconInfo is not null)
								sackIconInfo[sackDto.SackNumber] = sackDto.IconInfo;
						}

						return ImportResult.SucceededVault(dto.Name, sackItems, sackIconInfo, vaultMod);
					}

				default:
					return ImportResult.Failed($"Unsupported scope: {scope}");
			}
		}
	}

	public void ImportVaultInto(PlayerCollection vault, ImportResult importData)
	{
		if (vault is null || importData?.SackItems is null)
			return;

		// The vault's link comes with an export that has one; else the vault keeps its own, which the items follow.
		if (importData.VaultMod is not null)
			vault.Mod = importData.VaultMod;

		// Clear all tabs
		for (int i = 0; i < vault.NumberOfSacks; i++)
		{
			var sack = vault.GetSack(i);
			sack?.items?.Clear();
			if (sack is not null)
				sack.IsModified = true;
		}

		// Import items into the correct sacks
		foreach (var kvp in importData.SackItems)
		{
			int sackNumber = kvp.Key;
			if (sackNumber < 0 || sackNumber >= vault.NumberOfSacks)
				continue;

			var sack = vault.GetSack(sackNumber);
			if (sack is null)
				continue;

			foreach (var item in kvp.Value)
			{
				// A vault item: without an own map, it inherits the vault's, which the export may not have set.
				item.InVault = true;
				if (!item.HasOwnMod && !string.Equals(item.Mod, vault.Mod, StringComparison.OrdinalIgnoreCase))
				{
					item.Mod = vault.Mod;
					item.ModOrigin = ModOrigin.Inherited;
					this._itemProvider?.GetDBData(item);
				}

				sack.AddItem(item);
			}
		}

		if (importData.SackIconInfo is not null)
		{
			foreach (var kvp in importData.SackIconInfo)
			{
				int sackNumber = kvp.Key;
				if (sackNumber < 0 || sackNumber >= vault.NumberOfSacks)
					continue;

				var sack = vault.GetSack(sackNumber);
				if (sack is null)
					continue;

				sack.BagButtonIconInfo = kvp.Value;
			}
		}

		// The search lists the vault's new content, not the items it replaced.
		this._vaultService?.ReindexVault(vault);
	}

	public bool IsPasteBinUrl(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return false;

		return text.StartsWith("https://pastebin.com/", StringComparison.OrdinalIgnoreCase);
	}

	public bool HasPasteBinApiKey
		=> !string.IsNullOrWhiteSpace(_userSettings?.PasteBinApiKey);

	public async Task<string> ExportToPasteBinAsync(string json, string? pasteName = null)
	{
		if (_pasteBinService is null)
			throw new InvalidOperationException("PasteBin service is not configured.");

		return await _pasteBinService.UploadAsync(json, pasteName);
	}

	/// <inheritdoc/>
	public Task<string> ExportItemToPasteBinAsync(Item item)
	{
		var json = this.SerializeItem(item);
		var friendlyName = this._itemProvider?.GetFriendlyNames(item).FullNameClean;
		var pasteName = string.IsNullOrWhiteSpace(friendlyName) ? null : friendlyName;
		return this.ExportToPasteBinAsync(json, pasteName);
	}

	public async Task<string> ImportFromPasteBinAsync(string pasteUrl)
	{
		if (_pasteBinService is null)
			throw new InvalidOperationException("PasteBin service is not configured.");

		var payload = await _pasteBinService.FetchPasteAsync(pasteUrl);
		return payload;
	}

	public async Task<ImportResult> ImportFromTextAsync(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return ImportResult.Failed("Nothing to import.");

		var trimmed = text.Trim();
		if (!this.IsPasteBinUrl(trimmed))
			return this.ImportFromJson(text);

		string json;
		try
		{
			json = await this.ImportFromPasteBinAsync(trimmed);
		}
		catch (Exception ex)
		{
			return ImportResult.Failed($"Failed to import from PasteBin: {ex.Message}");
		}

		return this.ImportFromJson(json);
	}

	public ImportResult ImportFromFile(string path)
		=> this.ImportFromJson(this._fileIO.ReadAllText(path));

	public void ExportVaultToFile(PlayerCollection vault, string path)
		=> this.ExportToFile(this.SerializePlayerCollection(vault), path);

	/// <inheritdoc/>
	public string? SerializeSelection(IReadOnlyList<Item> items)
		=> items.Count switch
		{
			0 => null,
			1 => this.SerializeItem(items[0]),
			_ => this.SerializeItems(items),
		};

	/// <inheritdoc/>
	public void ExportToFile(string json, string path)
		=> this._fileIO.WriteAllText(path, json);

	/// <inheritdoc/>
	public Task<string> ExportSelectionToPasteBinAsync(IReadOnlyList<Item> items)
		=> items.Count == 1
			? this.ExportItemToPasteBinAsync(items[0])
			: this.ExportToPasteBinAsync(this.SerializeItems(items), "Multi-item export");

	/// <inheritdoc/>
	public string BuildTabPasteName(string vaultName, SackCollection sack, int sackNumber)
	{
		var tabName = sack.BagButtonIconInfo?.Label;
		return string.IsNullOrWhiteSpace(tabName)
			? $"{vaultName}/Tab {sackNumber + 1}"
			: $"{vaultName}/{tabName}";
	}

	/// <inheritdoc/>
	public Task<string> ExportTabToPasteBinAsync(string vaultName, SackCollection sack, int sackNumber, string? containerMod = null)
		=> this.ExportToPasteBinAsync(
			this.SerializeSackCollection(sack, sackNumber, containerMod)
			, this.BuildTabPasteName(vaultName, sack, sackNumber));

	public int PlaceImportedItems(SackCollection sack, ImportResult importResult, int sackWidth, int sackHeight, string? targetMod = null)
	{
		IReadOnlyList<Item> items = importResult.Scope == ExportScope.Item
			? (importResult.Item is { } single ? [single] : [])
			: importResult.Items;

		// A tab's items that inherited its mod keep it: inherited in a container of that mod, else as their own.
		if (importResult.Scope == ExportScope.Tab && importResult.TabMapKnown
			&& !string.Equals(importResult.TabMod, targetMod, StringComparison.OrdinalIgnoreCase))
		{
			foreach (var item in items.Where(i => i.ModOrigin == ModOrigin.Inherited))
				item.ModOrigin = ModOrigin.Own;
		}

		var imported = 0;
		foreach (var item in items)
		{
			var x = Math.Max(0, item.PositionX);
			var y = Math.Max(0, item.PositionY);
			var w = item.Width;
			var h = item.Height;

			if (!this.AreCellsFree(sack, x, y, w, h, sackWidth, sackHeight))
			{
				var pos = this._itemMovementService.FindOpenCells(sack, w, h, sackWidth, sackHeight);
				if (pos.X < 0)
					continue;

				x = pos.X;
				y = pos.Y;
			}

			item.PositionX = x;
			item.PositionY = y;
			sack.AddItem(item);
			imported++;
		}

		if (importResult.Scope == ExportScope.Tab && importResult.TabIconInfo is not null)
			sack.BagButtonIconInfo = importResult.TabIconInfo;

		sack.IsModified = true;
		return imported;
	}

	private bool AreCellsFree(SackCollection sack, int x, int y, int width, int height, int sackWidth, int sackHeight)
	{
		if (x < 0 || y < 0 || width < 1 || height < 1)
			return false;

		if (x + width > sackWidth || y + height > sackHeight)
			return false;

		for (var cx = x; cx < x + width; cx++)
		{
			for (var cy = y; cy < y + height; cy++)
			{
				if (this._itemMovementService.FindItem(sack, new System.Drawing.Point(cx, cy)) is not null)
					return false;
			}
		}

		return true;
	}

	public bool IsVaultEmpty(PlayerCollection vault)
	{
		for (int i = 0; i < vault.NumberOfSacks; i++)
		{
			var sack = vault.GetSack(i);
			if (sack is not null && !sack.IsEmpty)
				return false;
		}

		return true;
	}

	/// <summary>
	/// The name of an imported vault as a file name: the name comes from the imported text, which may hold a path or
	/// characters a file name cannot have.
	/// </summary>
	internal static string SafeVaultName(string? name)
	{
		var invalid = Path.GetInvalidFileNameChars();
		var safe = new string([.. (name ?? string.Empty).Where(c => !invalid.Contains(c))]).Trim().Trim('.');
		return safe.Length == 0 ? "Imported" : safe;
	}

	public string BuildImportedVaultName(string baseName, DateTime now)
		=> $"{baseName}_{now.ToString("yyyyMMdd_HHmmss", System.Globalization.CultureInfo.InvariantCulture)}";

	public string ImportAsNewVault(ImportResult importResult, DateTime now)
	{
		var newName = this.BuildImportedVaultName(SafeVaultName(importResult.VaultName), now);

		var newVault = this._vaultService.CreateVault(newName);
		this.ImportVaultInto(newVault, importResult);
		foreach (var sack in newVault)
			sack.IsModified = true;

		this._vaultService.SaveVault(newVault, newName);
		return newName;
	}
}
