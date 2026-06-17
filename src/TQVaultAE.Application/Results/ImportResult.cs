using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Results;

public class ImportResult
{
	public bool Success { get; set; }
	public ExportScope Scope { get; set; }
	public Item Item { get; set; }
	public IReadOnlyList<Item> Items { get; set; }
	public int SackNumber { get; set; }
	public string VaultName { get; set; }
	public Dictionary<int, List<Item>> SackItems { get; set; }
	public int ImportedCount { get; set; }
	public int TotalCount { get; set; }
	public string ErrorMessage { get; set; }
	public BagButtonIconInfo TabIconInfo { get; set; }
	public Dictionary<int, BagButtonIconInfo> SackIconInfo { get; set; }

	public static ImportResult Succeeded(Item item)
		=> new()
		{
			Success = true,
			Scope = ExportScope.Item,
			Item = item,
			ImportedCount = 1,
			TotalCount = 1
		};

	public static ImportResult SucceededTab(IReadOnlyList<Item> items, int sackNumber, BagButtonIconInfo iconInfo = null)
		=> new()
		{
			Success = true,
			Scope = ExportScope.Tab,
			Items = items,
			SackNumber = sackNumber,
			TabIconInfo = iconInfo,
			ImportedCount = items.Count,
			TotalCount = items.Count
		};

	public static ImportResult SucceededVault(string vaultName, Dictionary<int, List<Item>> sackItems, Dictionary<int, BagButtonIconInfo> sackIconInfo = null)
	{
		var total = 0;
		foreach (var kv in sackItems)
			total += kv.Value.Count;

		return new()
		{
			Success = true,
			Scope = ExportScope.Vault,
			VaultName = vaultName,
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