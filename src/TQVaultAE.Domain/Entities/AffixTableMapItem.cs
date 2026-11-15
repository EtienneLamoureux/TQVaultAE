namespace TQVaultAE.Domain.Entities;

/// <summary>An affix table a loot item table can pick, with its weight among the tables of the same kind.</summary>
public record WeightedAffixTable(RecordId Table, float Weight);

/// <summary>
/// The affix tables one loot item table rolls for the items it drops, per kind, with their weights.
/// </summary>
/// <param name="LootTable">The loot item table.</param>
/// <param name="Broken">Its broken affix tables, the game writes the broken affix in the prefix slot.</param>
/// <param name="Prefix">Its prefix tables.</param>
/// <param name="Suffix">Its suffix tables.</param>
/// <param name="Unreachable">
/// The tables the loot item table names but the game never rolls for the item: a slot or table without weight, a kind
/// without chance or kind weight. Their weights mean nothing then.
/// </param>
public record AffixTableMapItem(
	RecordId LootTable
	, IReadOnlyList<WeightedAffixTable> Broken
	, IReadOnlyList<WeightedAffixTable> Prefix
	, IReadOnlyList<WeightedAffixTable> Suffix
	, bool Unreachable = false)
{
	public bool IsEmpty
		=> Broken.Count == 0
		&& Prefix.Count == 0
		&& Suffix.Count == 0;
}
