using System.Collections.Generic;
using System.Text.Json.Serialization;
using TQVaultAE.Application;
using TQVaultAE.Data.Dto;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

public class VaultExportDTO
{
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("sacks")]
	public IReadOnlyList<SackExportDTO> Sacks { get; set; } = [];

	/// <summary>
	/// The mod the vault is linked to; absent for a vault of base game items.
	/// </summary>
	[JsonPropertyName("mod")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Mod { get; set; }

	public static VaultExportDTO FromPlayerCollection(PlayerCollection vault)
	{
		var sacks = new List<SackExportDTO>();
		for (int i = 0; i < vault.NumberOfSacks; i++)
		{
			var sack = vault.GetSack(i);
			if (sack is null)
				continue;

			if (sack.IsEmpty && (sack.BagButtonIconInfo is null || sack.BagButtonIconInfo.DisplayMode == BagButtonDisplayMode.Default))
				continue;

			var items = new List<ItemDto>();
			foreach (var item in sack)
			{
				// As in the vault file: an item following the vault's link has no link of its own to write.
				var dto = ItemDtoExtensions.FromItem(item);
				if (item.ModFromVault && string.Equals(item.Mod, vault.Mod, System.StringComparison.OrdinalIgnoreCase))
					dto.mod = null;

				items.Add(dto);
			}

			sacks.Add(new SackExportDTO
			{
				SackNumber = i,
				Items = items,
				IconInfo = sack.BagButtonIconInfo
			});
		}

		return new VaultExportDTO
		{
			Name = vault.PlayerName,
			Sacks = sacks,
			Mod = vault.Mod,
		};
	}
}

public class SackExportDTO
{
	[JsonPropertyName("sackNumber")]
	public int SackNumber { get; set; }

	[JsonPropertyName("items")]
	public IReadOnlyList<ItemDto> Items { get; set; } = [];

	[JsonPropertyName("iconInfo")]
	public BagButtonIconInfo? IconInfo { get; set; }
}