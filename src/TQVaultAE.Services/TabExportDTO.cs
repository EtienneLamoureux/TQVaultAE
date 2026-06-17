using System.Collections.Generic;
using System.Text.Json.Serialization;
using TQVaultAE.Data.Dto;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

public class TabExportDTO
{
	[JsonPropertyName("sackNumber")]
	public int SackNumber { get; set; }

	[JsonPropertyName("iconInfo")]
	public BagButtonIconInfo IconInfo { get; set; }

	[JsonPropertyName("items")]
	public List<ItemDto> Items { get; set; }

	public static TabExportDTO FromSackCollection(SackCollection sack, int sackNumber)
	{
		var items = new List<ItemDto>();
		foreach (var item in sack)
			items.Add(ItemDtoExtensions.FromItem(item));

		return new TabExportDTO
		{
			SackNumber = sackNumber,
			IconInfo = sack.BagButtonIconInfo,
			Items = items
		};
	}
}