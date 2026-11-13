namespace TQVaultAE.Domain.Entities;

/// <summary>
/// The value identity of an item: every field the game saves for it. Two items with the same key are the same piece
/// (a copy kept by TQVault and the original, for example), whatever their place, map or loaded data.
/// </summary>
/// <remarks>
/// Items have no key of their own and stay compared by reference everywhere else (<see cref="Item"/> does not override
/// <c>Equals</c>: it is mutable, and drags, tool tips and the search rely on reference identity). Build the key when
/// needed with <see cref="From"/>; never store its hash code, which changes from one run to the next.
/// Record ids are compared normalized (case and separators ignored), "no record" being the same whether null or empty;
/// the second relic fields are compared by value, so an item read without them equals one the game saved with them empty.
/// </remarks>
/// <param name="BaseItem">The base record.</param>
/// <param name="Prefix">The prefix record.</param>
/// <param name="Suffix">The suffix record.</param>
/// <param name="Relic1">The first relic or charm socketed.</param>
/// <param name="RelicBonus1">Its completion bonus.</param>
/// <param name="Relic2">The second relic or charm socketed.</param>
/// <param name="RelicBonus2">Its completion bonus.</param>
/// <param name="Seed">The seed.</param>
/// <param name="Var1">The first relic's piece count (or the relic's own, for a relic).</param>
/// <param name="Var2">The second relic's piece count.</param>
/// <param name="StackSize">The stack size.</param>
public readonly record struct ItemKey(
	string BaseItem,
	string Prefix,
	string Suffix,
	string Relic1,
	string RelicBonus1,
	string Relic2,
	string RelicBonus2,
	int Seed,
	int Var1,
	int Var2,
	int StackSize)
{
	/// <summary>True for the key of an empty slot's item (no base record).</summary>
	public bool IsEmpty => this.BaseItem.Length == 0;

	/// <summary>The key of an item, read from its saved fields now.</summary>
	/// <param name="item">The item.</param>
	public static ItemKey From(Item item)
	{
		ArgumentNullException.ThrowIfNull(item);
		return new(
			Of(item.BaseItemId),
			Of(item.prefixID),
			Of(item.suffixID),
			Of(item.relicID),
			Of(item.RelicBonusId),
			Of(item.relic2ID),
			Of(item.RelicBonus2Id),
			item.Seed,
			item.Var1,
			item.Var2,
			item.StackSize);
	}

	/// <summary>True for no item, or an empty slot's item (no base record).</summary>
	/// <param name="item">The item.</param>
	public static bool IsEmptyItem(Item? item) => item is null || RecordId.IsNullOrEmpty(item.BaseItemId);

	private static string Of(RecordId? id) => RecordId.IsNullOrEmpty(id) ? string.Empty : id!.Normalized;
}

/// <summary>
/// Compares items by value (<see cref="ItemKey"/>), for the places that need "the same piece" rather than "the same
/// object": a dictionary or a set of pieces, a match between a saved copy and the worn piece.
/// </summary>
public sealed class ItemIdentityComparer : IEqualityComparer<Item>
{
	/// <summary>The comparer.</summary>
	public static ItemIdentityComparer Instance { get; } = new();

	private ItemIdentityComparer()
	{
	}

	/// <inheritdoc/>
	public bool Equals(Item? x, Item? y)
	{
		if (ReferenceEquals(x, y))
			return true;
		if (x is null || y is null)
			return false;

		return ItemKey.From(x) == ItemKey.From(y);
	}

	/// <inheritdoc/>
	public int GetHashCode(Item obj) => ItemKey.From(obj).GetHashCode();
}
