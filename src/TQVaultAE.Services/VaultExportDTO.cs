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

	/// <summary>
	/// Where the mod of <see cref="Mod"/> is installed from (<c>workshop:&lt;id&gt;</c> or <c>custommaps</c>): a Steam
	/// Workshop mod and a custom map can have the same folder name. Absent from older exports: the name alone is then
	/// looked up.
	/// </summary>
	[JsonPropertyName("modSource")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? ModSource { get; set; }

	/// <summary>The mod's identity (<see cref="ModIdentity"/>): <see cref="Mod"/> and <see cref="ModSource"/>.</summary>
	[JsonIgnore]
	public string? ModKey => ModIdentity.Compose(this.Mod, this.ModSource);

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
				// As in the vault file: only an item's own map is written, the others inherit the vault's mod.
				items.Add(ItemDtoExtensions.FromItem(item, ownMapOnly: true));
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
			Mod = ModIdentity.NameOf(vault.Mod),
			ModSource = ModIdentity.SourceOf(vault.Mod),
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