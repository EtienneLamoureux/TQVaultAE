using TQVaultAE.Data.Dto;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

public static class ItemDtoExtensions
{
	/// <summary>
	/// The export of an item.
	/// </summary>
	/// <param name="item">The item.</param>
	/// <param name="ownMapOnly">True in a vault or tab export, which writes the container's mod once: an item writes only
	/// its own map. False in an item export: each item writes the map it is read with, made explicit.</param>
	public static ItemDto FromItem(Item item, bool ownMapOnly = false)
		=> new()
		{
			stackSize = item.StackSize,
			seed = item.Seed,
			baseName = item.BaseItemId.Raw,
			prefixName = item.prefixID?.Raw,
			suffixName = item.suffixID?.Raw,
			relicName = item.relicID?.Raw,
			relicBonus = item.RelicBonusId?.Raw,
			var1 = item.Var1,
			relicName2 = item.relic2ID?.Raw,
			relicBonus2 = item.RelicBonus2Id?.Raw,
			var2 = item.Var2,
			pointX = item.PositionX,
			pointY = item.PositionY,
			width = item.Width,
			height = item.Height,
			mod = ownMapOnly && !item.HasOwnMod ? null : item.Mod,
			noMod = (!ownMapOnly || item.HasOwnMod) && item.Mod is null ? true : null,
		};

	/// <summary>
	/// True when the export gave the item an own mod that the import dropped (not installed): the item then takes its
	/// target container's map, not the map its export's container had.
	/// </summary>
	public static bool IsDroppedOwnMod(this ItemDto dto, Item item)
		=> !string.IsNullOrWhiteSpace(dto.mod) && dto.noMod != true && item.ModOrigin == ModOrigin.None;

	public static Item ToItem(this ItemDto dto)
	{
		var item = new Item
		{
			StackSize = dto.stackSize,
			Seed = dto.seed,
			BaseItemId = dto.baseName,
			prefixID = dto.prefixName,
			suffixID = dto.suffixName,
			relicID = dto.relicName,
			RelicBonusId = dto.relicBonus,
			Var1 = dto.var1,
			PositionX = dto.pointX,
			PositionY = dto.pointY,
			Width = dto.width,
			Height = dto.height,
			// "mod" or "noMod" is the item's own map; without them it takes its container's when placed.
			Mod = dto.noMod == true || string.IsNullOrWhiteSpace(dto.mod) ? null : dto.mod,
			ModOrigin = dto.noMod == true || !string.IsNullOrWhiteSpace(dto.mod) ? ModOrigin.Own : ModOrigin.None,
			endBlockCrap2 = 0,
			endBlockCrap1 = 0
		};

		// As the vault loader: the game also writes the second relic fields empty, with a var2 to keep, so an item
		// has them whenever they hold anything but the defaults of an item without them.
		if (!string.IsNullOrWhiteSpace(dto.relicName2)
			|| !string.IsNullOrWhiteSpace(dto.relicBonus2)
			|| dto.var2 != Item.var2Default)
		{
			item.atlantis = true;
			item.relic2ID = dto.relicName2 ?? string.Empty;
			item.RelicBonus2Id = dto.relicBonus2 ?? string.Empty;
			item.Var2 = dto.var2;
		}
		else
		{
			item.relic2ID = RecordId.Empty;
			item.RelicBonus2Id = RecordId.Empty;
			item.Var2 = Item.var2Default;
		}

		return item;
	}
}