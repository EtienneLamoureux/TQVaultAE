namespace TQVaultAE.Domain.Comparison;

/// <summary>
/// Where a stat line of an item comes from, in the order the comparison lists the sources of a stat. Two items' sources
/// are matched by role: the base item with the base item, the first relic slot with the first relic slot.
/// </summary>
public enum StatSource
{
	/// <summary>The base item record.</summary>
	Base,
	/// <summary>The prefix (magical affix) of the item.</summary>
	Prefix,
	/// <summary>The suffix (magical affix) of the item.</summary>
	Suffix,
	/// <summary>The relic or charm socketed in the first slot.</summary>
	Relic1,
	/// <summary>The completion bonus of the first relic or charm, or of the artifact, relic or charm itself.</summary>
	RelicBonus1,
	/// <summary>The relic or charm socketed in the second slot.</summary>
	Relic2,
	/// <summary>The completion bonus of the second relic or charm.</summary>
	RelicBonus2,
}

/// <summary>
/// The part of a summed stat that one source gives: the stat line as that source writes it (its lines of the stat summed).
/// </summary>
/// <param name="Source">Where the part comes from.</param>
/// <param name="Line">The source's own line of the stat, with its values.</param>
public sealed record StatPart(StatSource Source, StatLine Line);
