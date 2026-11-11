using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Results;

public class ImportResult
{
	public bool Success { get; set; }
	public ExportScope Scope { get; set; }
	public Item? Item { get; set; }
	public IReadOnlyList<Item> Items { get; set; } = [];
	public int SackNumber { get; set; }
	public string VaultName { get; set; } = string.Empty;

	/// <summary>The mod the imported vault is linked to; null for none or when not a vault.</summary>
	public string? VaultMod { get; set; }
	/// <summary>Items of each tab, for a vault import only.</summary>
	public Dictionary<int, List<Item>>? SackItems { get; set; }
	public int ImportedCount { get; set; }
	public int TotalCount { get; set; }
	public string? ErrorMessage { get; set; }
	public BagButtonIconInfo? TabIconInfo { get; set; }

	/// <summary>The map of the container a tab export comes from, which its items without an own map inherited; null for
	/// the base game. Only meaningful when <see cref="TabMapKnown"/>.</summary>
	public string? TabMod { get; set; }

	/// <summary>False when the tab's mod is not installed here: its items then take their target's map.</summary>
	public bool TabMapKnown { get; set; }

	/// <summary>The mods the export names that are not installed here: their links were dropped.</summary>
	public IReadOnlyList<string> DroppedMods { get; set; } = [];
	public Dictionary<int, BagButtonIconInfo>? SackIconInfo { get; set; }

	public static ImportResult Succeeded(Item item)
		=> new()
		{
			Success = true,
			Scope = ExportScope.Item,
			Item = item,
			ImportedCount = 1,
			TotalCount = 1
		};

	public static ImportResult SucceededTab(IReadOnlyList<Item> items, int sackNumber, BagButtonIconInfo? iconInfo = null
		, string? tabMod = null, bool tabMapKnown = false)
		=> new()
		{
			Success = true,
			Scope = ExportScope.Tab,
			Items = items,
			SackNumber = sackNumber,
			TabIconInfo = iconInfo,
			TabMod = tabMod,
			TabMapKnown = tabMapKnown,
			ImportedCount = items.Count,
			TotalCount = items.Count
		};

	public static ImportResult SucceededVault(string vaultName, Dictionary<int, List<Item>> sackItems, Dictionary<int, BagButtonIconInfo>? sackIconInfo = null
		, string? vaultMod = null)
	{
		var total = 0;
		foreach (var kv in sackItems)
			total += kv.Value.Count;

		return new()
		{
			Success = true,
			Scope = ExportScope.Vault,
			VaultName = vaultName,
			VaultMod = vaultMod,
			SackItems = sackItems,
			SackIconInfo = sackIconInfo,
			ImportedCount = total,
			TotalCount = total
		};
	}

	public static ImportResult SucceededMultiSelect(IReadOnlyList<Item> items)
		=> new()
		{
			Success = true,
			Scope = ExportScope.MultiSelect,
			Items = items,
			ImportedCount = items.Count,
			TotalCount = items.Count
		};

	public static ImportResult Failed(string errorMessage)
		=> new()
		{
			Success = false,
			ErrorMessage = errorMessage
		};
}