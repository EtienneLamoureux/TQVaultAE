using TQVaultAE.Data.Dto;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

public static class ItemDtoExtensions
{
	public static ItemDto FromItem(Item item)
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
			mod = item.Mod,
			noMod = item.NoMod ? true : null,
		};

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
			Mod = dto.noMod == true || string.IsNullOrWhiteSpace(dto.mod) ? null : dto.mod,
			NoMod = dto.noMod == true,
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