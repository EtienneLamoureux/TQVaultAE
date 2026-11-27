using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Forge;

/// <summary>Part of a material item used in God mode.</summary>
public enum ForgePart
{
	Base,
	Prefix,
	Suffix,
	Relic1,
	Relic2,
}

/// <summary>
/// Applies the Forge selection to the base item: its prefix, suffix and relics are taken from the material items.
/// </summary>
public static class ForgeMerge
{
	/// <summary>
	/// Merges <paramref name="selection"/> into <paramref name="target"/> (the base item or a preview copy of it).
	/// </summary>
	/// <param name="godPart">
	/// God mode only: part of the material item to use for a slot; <c>null</c> leaves that slot's value unchanged.
	/// </param>
	public static void Apply(Item target, ForgeSelection selection, bool godMode, Func<ForgeSlot, ForgePart?> godPart)
	{
		if (selection.Prefix is { } prefix)
		{
			target.prefixInfo = null;
			if (!godMode)
				target.prefixID = prefix.prefixID;
			else if (godPart(ForgeSlot.Prefix) is { } part)
				target.prefixID = AffixFrom(prefix, part);
		}

		if (selection.Suffix is { } suffix)
		{
			target.suffixInfo = null;
			if (!godMode)
				target.suffixID = suffix.suffixID;
			else if (godPart(ForgeSlot.Suffix) is { } part)
				target.suffixID = AffixFrom(suffix, part);
		}

		if (selection.Relic1 is { } relic1)
		{
			target.RelicInfo = target.RelicBonusInfo = null;
			if (RelicFor(relic1, ForgeSlot.Relic1) is { } r)
				(target.relicID, target.RelicBonusId, target.Var1) = r;
		}

		if (selection.Relic2 is { } relic2)
		{
			target.Relic2Info = target.RelicBonus2Info = null;
			if (RelicFor(relic2, ForgeSlot.Relic2) is { } r)
			{
				(target.relic2ID, target.RelicBonus2Id, target.Var2) = r;

				// The second relic is only written for an item that has its fields.
				target.atlantis = true;
			}
		}

		(RecordId Relic, RecordId Bonus, int Shards)? RelicFor(Item source, ForgeSlot slot)
		{
			if (!godMode)
				return RelicFrom(source);
			return godPart(slot) is { } part ? RelicFrom(source, part) : null;
		}
	}

	/// <summary>Record used as a prefix or suffix, God mode.</summary>
	private static RecordId AffixFrom(Item source, ForgePart part) => part switch
	{
		ForgePart.Prefix => source.prefixID,
		ForgePart.Suffix => source.suffixID,
		ForgePart.Relic1 => source.relicID,
		ForgePart.Relic2 => source.relic2ID,
		_ => source.BaseItemId,
	};

	/// <summary>Relic, bonus and shard count taken from a relic item or from the first relic of an item.</summary>
	private static (RecordId Relic, RecordId Bonus, int Shards) RelicFrom(Item source)
		=> source.IsRelicOrCharm
			? (source.BaseItemId, source.RelicBonusId, source.Var1)
			: (source.relicID, source.RelicBonusId, source.Var1);

	/// <summary>Relic, bonus and shard count, God mode: affixes become a relic with no bonus and one shard.</summary>
	private static (RecordId Relic, RecordId Bonus, int Shards) RelicFrom(Item source, ForgePart part) => part switch
	{
		ForgePart.Prefix => (source.prefixID, RecordId.Empty, 1),
		ForgePart.Suffix => (source.suffixID, RecordId.Empty, 1),
		ForgePart.Relic1 => (source.relicID, source.RelicBonusId, source.Var1),
		ForgePart.Relic2 => (source.relic2ID, source.RelicBonus2Id, source.Var2),
		_ => (source.BaseItemId, source.RelicBonusId, source.Var1),
	};
}
