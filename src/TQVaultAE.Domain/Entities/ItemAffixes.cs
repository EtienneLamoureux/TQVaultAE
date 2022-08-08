using System.Collections.ObjectModel;

namespace TQVaultAE.Domain.Entities;

public record ItemAffixes(
	ReadOnlyDictionary<GameExtension, ReadOnlyCollection<LootTableCollection>> Broken
	, ReadOnlyDictionary<GameExtension, ReadOnlyCollection<LootTableCollection>> Prefix
	, ReadOnlyDictionary<GameExtension, ReadOnlyCollection<LootTableCollection>> Suffix
);
