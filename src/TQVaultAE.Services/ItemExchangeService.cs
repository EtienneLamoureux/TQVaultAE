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

public class ItemExchangeService : IItemExchangeService
{
	private readonly JsonSerializerOptions _jsonOptions;
	private readonly IPasteBinService _pasteBinService;
	private readonly UserSettings _userSettings;
	private readonly IItemProvider _itemProvider;

	public ItemExchangeService(
		JsonSerializerOptions jsonOptions
		, IPasteBinService pasteBinService = null!
		, UserSettings userSettings = null!
		, IItemProvider itemProvider = null!
	)
	{
		_jsonOptions = jsonOptions;
		_pasteBinService = pasteBinService;
		_userSettings = userSettings;
		_itemProvider = itemProvider;
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
		var dtos = items.Select(ItemDtoExtensions.FromItem).ToList();
		var envelope = new ExportFormat
		{
			Scope = ExportScope.MultiSelect,
			Data = dtos
		};

		return JsonSerializer.Serialize(envelope, _jsonOptions);
	}

	public string SerializeSackCollection(SackCollection sack, int sackNumber)
	{
		var dto = TabExportDTO.FromSackCollection(sack, sackNumber);
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

			switch (scope)
			{
				case ExportScope.Item:
					{
						var dto = JsonSerializer.Deserialize<ItemDto>(dataElement.GetRawText(), _jsonOptions);

						if (dto == null)
							return ImportResult.Failed("Failed to deserialize item data.");

						var item = dto.ToItem();
						_itemProvider?.GetDBData(item);
						return ImportResult.Succeeded(item);
					}

				case ExportScope.Tab:
					{
						var dto = JsonSerializer.Deserialize<TabExportDTO>(dataElement.GetRawText(), _jsonOptions);

						if (dto == null)
							return ImportResult.Failed("Failed to deserialize tab data.");

						var items = new List<Item>();
						foreach (var itemDto in dto.Items)
						{
							var item = itemDto.ToItem();
							_itemProvider?.GetDBData(item);
							items.Add(item);
						}

						return ImportResult.SucceededTab(items, dto.SackNumber, dto.IconInfo);
					}

				case ExportScope.MultiSelect:
				{
					var dtos = JsonSerializer.Deserialize<List<ItemDto>>(dataElement.GetRawText(), _jsonOptions);

					if (dtos == null || dtos.Count == 0)
						return ImportResult.Failed("Failed to deserialize multi-select item data.");

					var items = dtos
						.Select(dto =>
						{
							var item = dto.ToItem();
							_itemProvider?.GetDBData(item);
							return item;
						})
						.ToList();

					return ImportResult.SucceededMultiSelect(items);
				}

			case ExportScope.Vault:
					{
						var dto = JsonSerializer.Deserialize<VaultExportDTO>(dataElement.GetRawText(), _jsonOptions);

						if (dto == null)
							return ImportResult.Failed("Failed to deserialize vault data.");

						var sackItems = new Dictionary<int, List<Item>>();
						var sackIconInfo = new Dictionary<int, BagButtonIconInfo>();
						foreach (var sackDto in dto.Sacks)
						{
							var itemList = new List<Item>();
							foreach (var itemDto in sackDto.Items)
							{
								var item = itemDto.ToItem();
								_itemProvider?.GetDBData(item);
								itemList.Add(item);
							}

							sackItems[sackDto.SackNumber] = itemList;
							if (sackDto.IconInfo != null)
								sackIconInfo[sackDto.SackNumber] = sackDto.IconInfo;
						}

						return ImportResult.SucceededVault(dto.Name, sackItems, sackIconInfo);
					}

				default:
					return ImportResult.Failed($"Unsupported scope: {scope}");
			}
		}
		catch (JsonException ex)
		{
			return ImportResult.Failed($"Invalid JSON format: {ex.Message}");
		}
	}

	public void ImportVaultInto(PlayerCollection vault, ImportResult importData)
	{
		if (vault == null || importData?.SackItems == null)
			return;

		// Clear all tabs
		for (int i = 0; i < vault.NumberOfSacks; i++)
		{
			var sack = vault.GetSack(i);
			sack?.items?.Clear();
			if (sack != null)
				sack.IsModified = true;
		}

		// Import items into the correct sacks
		foreach (var kvp in importData.SackItems)
		{
			int sackNumber = kvp.Key;
			if (sackNumber < 0 || sackNumber >= vault.NumberOfSacks)
				continue;

			var sack = vault.GetSack(sackNumber);
			if (sack == null)
				continue;

			foreach (var item in kvp.Value)
			{
				sack.AddItem(item);
			}
		}

		if (importData.SackIconInfo != null)
		{
			foreach (var kvp in importData.SackIconInfo)
			{
				int sackNumber = kvp.Key;
				if (sackNumber < 0 || sackNumber >= vault.NumberOfSacks)
					continue;

				var sack = vault.GetSack(sackNumber);
				if (sack == null)
					continue;

				sack.BagButtonIconInfo = kvp.Value;
			}
		}
	}

	public bool IsPasteBinUrl(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return false;

		return text.StartsWith("https://pastebin.com/", StringComparison.OrdinalIgnoreCase);
	}

	public bool HasPasteBinApiKey
		=> !string.IsNullOrWhiteSpace(_userSettings?.PasteBinApiKey);

	public async Task<string> ExportToPasteBinAsync(string json, string pasteName = null)
	{
		if (_pasteBinService == null)
			throw new InvalidOperationException("PasteBin service is not configured.");

		return await _pasteBinService.UploadAsync(json, pasteName);
	}

	public async Task<string> ImportFromPasteBinAsync(string pasteUrl)
	{
		if (_pasteBinService == null)
			throw new InvalidOperationException("PasteBin service is not configured.");

		var payload = await _pasteBinService.FetchPasteAsync(pasteUrl);
		return payload;
	}
}
