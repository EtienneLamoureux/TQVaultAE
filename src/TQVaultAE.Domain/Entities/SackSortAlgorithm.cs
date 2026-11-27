namespace TQVaultAE.Domain.Entities;

/// <summary>
/// The order the sort button of a bag puts its items in. Each order keeps the next keys of <see cref="Normal"/> to
/// break ties, so items of the same kind stay together.
/// </summary>
public enum SackSortAlgorithm
{
	/// <summary>The largest items first, then by group and record: the historical sort, the default.</summary>
	Normal = 0,

	/// <summary>The lowest required level first.</summary>
	Level,

	/// <summary>By item type (helmets, rings, swords, relics...), then by required level.</summary>
	Type,

	/// <summary>The rarest gear first (legendary, epic, rare...), then by type and required level.</summary>
	Rarity,
}
