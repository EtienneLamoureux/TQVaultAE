using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.DTOs;

/// <summary>
/// A set member offered to create the rest of a set.
/// </summary>
/// <param name="RecordId">Raw record id of the set member.</param>
/// <param name="Label">Translated name of the set member.</param>
public sealed record SetMemberChoice(string RecordId, string Label);

/// <summary>
/// A completion bonus offered by a loot table (relic, charm or artifact bonus).
/// </summary>
/// <param name="Id">Record id of the bonus.</param>
/// <param name="Label">Display text: translation, file name, weight and DLC.</param>
/// <param name="Unknown">The bonus is an orphan reference missing from the database.</param>
/// <param name="Untranslated">The bonus has no translation tag.</param>
/// <param name="IsCurrent">The bonus is the one the item has now.</param>
public sealed record BonusChoice(RecordId Id, string Label, bool Unknown, bool Untranslated, bool IsCurrent);

/// <summary>
/// The affixes an item can take, grouped by affix name and sorted by type then name.
/// </summary>
/// <param name="HasBroken">The item has broken affixes to offer.</param>
/// <param name="Broken">Broken affix groups.</param>
/// <param name="HasPrefix">The item has prefixes to offer.</param>
/// <param name="Prefix">Prefix groups.</param>
/// <param name="HasSuffix">The item has suffixes to offer.</param>
/// <param name="Suffix">Suffix groups.</param>
/// <param name="AllAffixesOfGearType">
/// The affixes are every affix of the item's gear type (the Epic/Legendary setting): each percentage is the affix's share
/// in its table. Otherwise they are the affixes the game can roll on the item: each percentage is the chance of the affix
/// once the item gets an affix of that kind, from the drop source that gives it best.
/// </param>
public sealed record AffixCatalog(
	bool HasBroken
	, IReadOnlyList<AffixGroup> Broken
	, bool HasPrefix
	, IReadOnlyList<AffixGroup> Prefix
	, bool HasSuffix
	, IReadOnlyList<AffixGroup> Suffix
	, bool AllAffixesOfGearType = false
);
