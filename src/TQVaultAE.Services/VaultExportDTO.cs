using System.Collections.Generic;
using System.Text.Json.Serialization;
using TQVaultAE.Application;
using TQVaultAE.Data.Dto;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

public class VaultExportDTO
{
	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("sacks")]
	public IReadOnlyList<SackExportDTO> Sacks { get; set; }

	public static VaultExportDTO FromPlayerCollection(PlayerCollection vault)
	{
		var sacks = new List<SackExportDTO>();
		for (int i = 0; i < vault.NumberOfSacks; i++)
		{
			var sack = vault.GetSack(i);
			if (sack == null)
				continue;

			if (sack.IsEmpty && (sack.BagButtonIconInfo == null || sack.BagButtonIconInfo.DisplayMode == BagButtonDisplayMode.Default))
				continue;

			var items = new List<ItemDto>();
			foreach (var item in sack)
				items.Add(ItemDtoExtensions.FromItem(item));

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
			Sacks = sacks
		};
	}
}

public class SackExportDTO
{
	[JsonPropertyName("sackNumber")]
	public int SackNumber { get; set; }

	[JsonPropertyName("items")]
	public IReadOnlyList<ItemDto> Items { get; set; }

	[JsonPropertyName("iconInfo")]
	public BagButtonIconInfo IconInfo { get; set; }
}