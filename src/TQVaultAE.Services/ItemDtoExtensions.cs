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
			baseName = item.BaseItemId?.Raw,
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
			height = item.Height
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
			endBlockCrap2 = 0,
			endBlockCrap1 = 0
		};

		if (!string.IsNullOrWhiteSpace(dto.relicName2))
		{
			item.atlantis = true;
			item.relic2ID = dto.relicName2;
			item.RelicBonus2Id = dto.relicBonus2;
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