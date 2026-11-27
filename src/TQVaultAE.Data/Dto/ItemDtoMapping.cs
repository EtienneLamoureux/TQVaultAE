using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Data.Dto;

/// <summary>The fields an <see cref="ItemDto"/> shares with its <see cref="Item"/>, for the vault files and the exports.</summary>
public static class ItemDtoMapping
{
	/// <summary>The item's records, seed, relic pieces, place and stack. Size and mod are left to the caller.</summary>
	public static ItemDto ToDto(this Item item)
	{
		ArgumentNullException.ThrowIfNull(item);
		return new()
		{
			stackSize = item.StackSize,
			seed = item.Seed,
			baseName = RecordId.RawOf(item.BaseItemId),
			prefixName = RecordId.RawOf(item.prefixID),
			suffixName = RecordId.RawOf(item.suffixID),
			relicName = RecordId.RawOf(item.relicID),
			relicBonus = RecordId.RawOf(item.RelicBonusId),
			var1 = item.Var1,
			relicName2 = RecordId.RawOf(item.relic2ID),
			relicBonus2 = RecordId.RawOf(item.RelicBonus2Id),
			var2 = item.Var2,
			pointX = item.PositionX,
			pointY = item.PositionY,
		};
	}

	/// <summary>Sets the item's records, seed, relic pieces, place and stack from <paramref name="dto"/>.</summary>
	public static void ApplyTo(this ItemDto dto, Item item)
	{
		ArgumentNullException.ThrowIfNull(dto);
		ArgumentNullException.ThrowIfNull(item);
		item.StackSize = dto.stackSize;
		item.Seed = dto.seed;
		item.BaseItemId = dto.baseName;
		item.prefixID = dto.prefixName;
		item.suffixID = dto.suffixName;
		item.relicID = dto.relicName;
		item.RelicBonusId = dto.relicBonus;
		item.Var1 = dto.var1;
		item.PositionX = dto.pointX;
		item.PositionY = dto.pointY;

		// The json has no atlantis flag: the game also writes the second relic fields empty, with a var2 to keep, so an
		// item has them whenever they hold anything but the defaults of an item without them.
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
	}
}
