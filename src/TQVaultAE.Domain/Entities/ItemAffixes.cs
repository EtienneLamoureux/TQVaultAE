using System.Collections.ObjectModel;

namespace TQVaultAE.Domain.Entities;

/// <summary>The affix tables an item can take its affixes from, per kind.</summary>
/// <param name="Broken">The broken affixes the game can roll, one distribution per loot item table.</param>
/// <param name="Prefix">The prefixes the game can roll.</param>
/// <param name="Suffix">The suffixes the game can roll.</param>
/// <param name="Unreachable">
/// The affix tables the item's loot item tables name but the game never rolls for it, per kind (0 broken, 1 prefix,
/// 2 suffix); empty unless asked for.
/// </param>
public record ItemAffixes(
	ReadOnlyDictionary<GameDlc, ReadOnlyCollection<LootTableCollection>> Broken
	, ReadOnlyDictionary<GameDlc, ReadOnlyCollection<LootTableCollection>> Prefix
	, ReadOnlyDictionary<GameDlc, ReadOnlyCollection<LootTableCollection>> Suffix
	, IReadOnlyList<(int Kind, LootTableCollection Table)>? Unreachable = null
);
