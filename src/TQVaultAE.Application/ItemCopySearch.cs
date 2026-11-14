using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application;

/// <summary>
/// What the item menu's <i>Search</i> looks for among the loaded items, from the item clicked.
/// </summary>
public enum CopySearchKind
{
	/// <summary>The same piece: same base, prefix, suffix and seed. Socketed relics, charms and their bonus may differ.</summary>
	SameSeed,

	/// <summary>Every copy: same base, prefix and suffix, any seed. Socketed relics, charms and their bonus may differ.</summary>
	AllCopies,

	/// <summary>The items made from the same base record, whatever their affixes.</summary>
	SameBase,

	/// <summary>The items with the same prefix, whatever their base. Needs an item with a prefix.</summary>
	SamePrefix,

	/// <summary>The items with the same suffix, whatever their base. Needs an item with a suffix.</summary>
	SameSuffix,
}

/// <summary>
/// The rules of the item menu's <i>Search</i>: which items match the item clicked for each <see cref="CopySearchKind"/>.
/// Record ids are compared as <see cref="ItemKey"/> does (case and separators ignored).
/// </summary>
public static class ItemCopySearch
{
	/// <summary>The searches, in the menu's order.</summary>
	public static IReadOnlyList<CopySearchKind> Kinds { get; } = Enum.GetValues<CopySearchKind>();

	/// <summary>
	/// True when the search makes sense for <paramref name="item"/>: a prefix or suffix search needs the item to have one.
	/// </summary>
	/// <param name="item">The item clicked.</param>
	/// <param name="kind">The search.</param>
	public static bool IsAvailable(Item item, CopySearchKind kind)
	{
		ArgumentNullException.ThrowIfNull(item);
		if (ItemKey.IsEmptyItem(item))
			return false;

		var key = ItemKey.From(item);
		return kind switch
		{
			CopySearchKind.SamePrefix => key.Prefix.Length != 0,
			CopySearchKind.SameSuffix => key.Suffix.Length != 0,
			_ => true,
		};
	}

	/// <summary>
	/// A matcher of the items found by the search for <paramref name="source"/>; the source matches itself. An
	/// unavailable search (<see cref="IsAvailable"/>) matches nothing.
	/// </summary>
	/// <param name="source">The item clicked.</param>
	/// <param name="kind">The search.</param>
	public static Func<Item, bool> Matcher(Item source, CopySearchKind kind)
	{
		if (!IsAvailable(source, kind))
			return _ => false;

		// The source's key is read once: the matcher runs over every loaded item.
		var wanted = ItemKey.From(source);
		return item =>
		{
			if (item is null || ItemKey.IsEmptyItem(item))
				return false;

			var key = ItemKey.From(item);
			return kind switch
			{
				CopySearchKind.SameSeed => SameAffixes(key, wanted) && key.Seed == wanted.Seed,
				CopySearchKind.AllCopies => SameAffixes(key, wanted),
				CopySearchKind.SameBase => key.BaseItem == wanted.BaseItem,
				CopySearchKind.SamePrefix => key.Prefix == wanted.Prefix,
				CopySearchKind.SameSuffix => key.Suffix == wanted.Suffix,
				_ => false,
			};
		};
	}

	private static bool SameAffixes(ItemKey key, ItemKey wanted)
		=> key.BaseItem == wanted.BaseItem && key.Prefix == wanted.Prefix && key.Suffix == wanted.Suffix;
}
