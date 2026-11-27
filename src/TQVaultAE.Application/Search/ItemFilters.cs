using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Application.Search;

/// <summary>
/// Requirement bounds shared by the advanced search and the highlight filter. A bound of 0 is no bound.
/// </summary>
public readonly record struct RequirementBounds(
	int MinLevel, int MaxLevel,
	int MinStrength, int MaxStrength,
	int MinDexterity, int MaxDexterity,
	int MinIntelligence, int MaxIntelligence)
{
	/// <summary>
	/// Gets whether no bound is set, so every item passes.
	/// </summary>
	public bool IsEmpty => this.MinLevel <= 0 && this.MaxLevel <= 0 && this.MinStrength <= 0 && this.MaxStrength <= 0
		&& this.MinDexterity <= 0 && this.MaxDexterity <= 0 && this.MinIntelligence <= 0 && this.MaxIntelligence <= 0;

	/// <summary>
	/// Gets whether an item's requirements are within the bounds. An item without requirement information passes, and
	/// so does a requirement the item does not have.
	/// </summary>
	public bool Matches(RequirementInfo? requirements)
		=> requirements is null
			|| (Within(requirements.Lvl, this.MinLevel, this.MaxLevel)
				&& Within(requirements.Str, this.MinStrength, this.MaxStrength)
				&& Within(requirements.Dex, this.MinDexterity, this.MaxDexterity)
				&& Within(requirements.Int, this.MinIntelligence, this.MaxIntelligence));

	private static bool Within(int? value, int min, int max)
		=> value is not { } v || ((min <= 0 || v >= min) && (max <= 0 || v <= max));
}

/// <summary>
/// The item kinds both the advanced search and the highlight filter can require: an item must have each one set.
/// <see cref="Modded"/> keeps the items from a mod (<see cref="ItemMods.IsModded"/>).
/// </summary>
public readonly record struct ItemFlagFilter(bool Prefix, bool Suffix, bool Relic, bool Charm, bool SetItem, bool Skill, bool Modded = false)
{
	/// <summary>
	/// Gets whether no flag is set, so every item passes.
	/// </summary>
	public bool IsEmpty => !(this.Prefix || this.Suffix || this.Relic || this.Charm || this.SetItem || this.Skill || this.Modded);

	/// <summary>
	/// Gets whether an item has every flag set.
	/// </summary>
	/// <param name="item">The item.</param>
	/// <param name="names">Its friendly names; an item without them has no affix, relic, charm or set.</param>
	/// <param name="hasGrantedSkill">Tells whether an item grants a skill; read last, as it is the slowest.</param>
	public bool Matches(Item item, ToFriendlyNameResult? names, Func<Item, bool> hasGrantedSkill)
	{
		var named = names?.Item;
		return (!this.Prefix || named?.HasPrefix == true)
			&& (!this.Suffix || named?.HasSuffix == true)
			&& (!this.Relic || named?.HasRelic == true)
			&& (!this.Charm || named?.HasCharm == true)
			&& (!this.SetItem || names?.ItemSet is not null)
			&& (!this.Modded || ItemMods.IsModded(item))
			&& (!this.Skill || hasGrantedSkill(item));
	}
}

/// <summary>
/// Which mod an item comes from, the rule the advanced search and the highlight filter share: its map
/// (<see cref="Item.Mod"/>, the mod's identity <c>name|source</c>, its own or the container's), null for the base game.
/// Mods are compared as the installed mods are found (<c>IModLinkService.FindMod</c>): ignoring case.
/// </summary>
public static class ItemMods
{
	/// <summary>True when the item comes from a mod, installed or not.</summary>
	public static bool IsModded(Item item) => !string.IsNullOrWhiteSpace(item.Mod);

	/// <summary>True when the item comes from the mod <paramref name="mod"/> (its identity); false for no mod.</summary>
	public static bool IsFrom(Item item, string? mod)
		=> !string.IsNullOrWhiteSpace(mod) && string.Equals(item.Mod, mod, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// True when the item comes from one of <paramref name="mods"/> (identities); every item passes an empty list, as
	/// every section of the filters does when nothing is ticked in it.
	/// </summary>
	public static bool IsFromAny(Item item, IReadOnlyCollection<string> mods)
		=> mods.Count == 0 || mods.Any(mod => IsFrom(item, mod));
}
