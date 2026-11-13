using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Actions;

/// <summary>
/// Whether two items are the same piece: every field the game saves for an item is equal.
/// </summary>
/// <remarks>
/// Items have no key of their own; a copy kept by TQVault (a loadout) is the worn piece when the base record, the
/// affixes, the socketed relics or charms with their completion bonus and piece counts, the seed and the stack size are
/// all equal. The second relic fields are compared by value: an item read from a file without them holds their defaults,
/// like an item the game saved with them empty.
/// </remarks>
public static class ItemIdentity
{
	/// <summary>True for no item, or an empty slot's item (no base record).</summary>
	/// <param name="item">The item.</param>
	public static bool IsEmpty(Item? item) => item is null || RecordId.IsNullOrEmpty(item.BaseItemId);

	/// <summary>
	/// True when both items hold the same saved values; two empty items are identical, an empty and a real one are not.
	/// </summary>
	/// <param name="first">An item.</param>
	/// <param name="second">The other item.</param>
	public static bool AreIdentical(Item? first, Item? second)
	{
		if (IsEmpty(first) || IsEmpty(second))
			return IsEmpty(first) && IsEmpty(second);

		return Same(first!.BaseItemId, second!.BaseItemId)
			&& Same(first.prefixID, second.prefixID)
			&& Same(first.suffixID, second.suffixID)
			&& Same(first.relicID, second.relicID)
			&& Same(first.RelicBonusId, second.RelicBonusId)
			&& Same(first.relic2ID, second.relic2ID)
			&& Same(first.RelicBonus2Id, second.RelicBonus2Id)
			&& first.Seed == second.Seed
			&& first.Var1 == second.Var1
			&& first.Var2 == second.Var2
			&& first.StackSize == second.StackSize;
	}

	/// <summary>Same record, "no record" being equal whether null or empty.</summary>
	private static bool Same(RecordId? first, RecordId? second)
		=> RecordId.IsNullOrEmpty(first) ? RecordId.IsNullOrEmpty(second) : first == second;
}
