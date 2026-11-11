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
	public BagButtonIconInfo? IconInfo { get; set; }

	[JsonPropertyName("items")]
	public List<ItemDto> Items { get; set; } = [];

	/// <summary>
	/// The mod of the container the tab comes from, which its items without an own map inherit; absent for the base
	/// game.
	/// </summary>
	[JsonPropertyName("mod")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Mod { get; set; }

	/// <param name="sack">The tab.</param>
	/// <param name="sackNumber">Its number.</param>
	/// <param name="containerMod">The mod of its container (vault or character); null for the base game.</param>
	public static TabExportDTO FromSackCollection(SackCollection sack, int sackNumber, string? containerMod = null)
	{
		// As in the vault file: only an item's own map is written, the others inherit the tab's mod.
		var items = new List<ItemDto>();
		foreach (var item in sack)
			items.Add(ItemDtoExtensions.FromItem(item, ownMapOnly: true));

		return new TabExportDTO
		{
			SackNumber = sackNumber,
			IconInfo = sack.BagButtonIconInfo,
			Items = items,
			Mod = string.IsNullOrWhiteSpace(containerMod) ? null : containerMod,
		};
	}
}