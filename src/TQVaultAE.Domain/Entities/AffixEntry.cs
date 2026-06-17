using System.Collections;

namespace TQVaultAE.Domain.Entities;

/// <summary>
/// Represents a single affix entry within an affix group.
/// </summary>
public record AffixEntry(
	int TypeId,
	RecordId AffixId,
	GameDlc AffixIdDlc,
	string Translation,
	float WeightPercent,
	string FormattedText,
	LootRandomizerItem LootRandomizer
)
{
	/// <summary>
	/// Gets the type identifier (0=Broken, 1=Prefix, 2=Suffix).
	/// </summary>
	public int TypeId { get; init; } = TypeId;

	/// <summary>
	/// Gets the affix record identifier.
	/// </summary>
	public RecordId AffixId { get; init; } = AffixId;

	/// <summary>
	/// Gets the DLC of the affix.
	/// </summary>
	public GameDlc AffixIdDlc { get; init; } = AffixIdDlc;

	/// <summary>
	/// Gets the translation of the affix.
	/// </summary>
	public string Translation { get; init; } = Translation;

	/// <summary>
	/// Gets the weight percentage of the affix.
	/// </summary>
	public float WeightPercent { get; init; } = WeightPercent;

	/// <summary>
	/// Gets the formatted text for display.
	/// </summary>
	public string FormattedText { get; init; } = FormattedText;

	/// <summary>
	/// Gets the loot randomizer item associated with this affix.
	/// </summary>
	public LootRandomizerItem LootRandomizer { get; init; } = LootRandomizer;
}

/// <summary>
/// Represents a group of affixes sharing the same type and translation.
/// </summary>
public class AffixGroup : IEnumerable<AffixEntry>
{
	private readonly List<AffixEntry> _entries;

	/// <summary>
	/// Initializes a new instance of the <see cref="AffixGroup"/> class.
	/// </summary>
	/// <param name="key">The group's key.</param>
	/// <param name="entries">The entries in this group.</param>
	public AffixGroup(AffixGroupKey key, IEnumerable<AffixEntry> entries)
	{
		this.Key = key;
		this._entries = entries?.ToList() ?? new List<AffixEntry>();
	}

	/// <summary>
	/// Gets the group's key containing TypeId and Translation.
	/// </summary>
	public AffixGroupKey Key { get; }

	/// <summary>
	/// Gets an enumerator over the entries in this group.
	/// </summary>
	/// <returns>An enumerator of <see cref="AffixEntry"/>.</returns>
	public IEnumerator<AffixEntry> GetEnumerator() => this._entries.GetEnumerator();

	/// <summary>
	/// Gets a non-generic enumerator.
	/// </summary>
	/// <returns>An enumerator.</returns>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	/// Gets the number of entries in this group.
	/// </summary>
	public int Count => this._entries.Count;
}

/// <summary>
/// Represents the key for an <see cref="AffixGroup"/>.
/// </summary>
public record AffixGroupKey(int TypeId, string Translation)
{
	/// <summary>
	/// Gets the type identifier (0=Broken, 1=Prefix, 2=Suffix).
	/// </summary>
	public int TypeId { get; init; } = TypeId;

	/// <summary>
	/// Gets the translation/name of the affix group.
	/// </summary>
	public string Translation { get; init; } = Translation;
}
