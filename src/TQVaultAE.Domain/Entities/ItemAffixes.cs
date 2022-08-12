using System.Collections.ObjectModel;

namespace TQVaultAE.Domain.Entities;

public record ItemAffixes(
	ReadOnlyDictionary<GameDlc, ReadOnlyCollection<LootTableCollection>> Broken
	, ReadOnlyDictionary<GameDlc, ReadOnlyCollection<LootTableCollection>> Prefix
	, ReadOnlyDictionary<GameDlc, ReadOnlyCollection<LootTableCollection>> Suffix
);
