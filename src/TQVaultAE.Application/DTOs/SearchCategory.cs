using TQVaultAE.Application.Results;

namespace TQVaultAE.Application.DTOs;

/// <summary>
/// The categories of the advanced search dialog, each one a list of values to filter the items with.
/// </summary>
public enum SearchCategory
{
	/// <summary>Characters holding the item (inventory or equipment).</summary>
	Characters,
	/// <summary>Item class, e.g. "Sword".</summary>
	ItemType,
	/// <summary>Rarity text, for equipment only.</summary>
	Rarity,
	/// <summary>Style text of the base item.</summary>
	Style,
	/// <summary>Quality text of the base item.</summary>
	Quality,
	/// <summary>Vaults holding the item.</summary>
	Vaults,
	/// <summary>Charms socketed in the item.</summary>
	WithCharm,
	/// <summary>Relics socketed in the item.</summary>
	WithRelic,
	/// <summary>Game or expansion the item comes from.</summary>
	Origin,
	/// <summary>Set the item belongs to.</summary>
	SetItems,
	/// <summary>Skills granted by the item.</summary>
	Skills,
	/// <summary>Every attribute line of the item.</summary>
	ItemAttributes,
	/// <summary>Attribute lines of the base item.</summary>
	BaseAttributes,
	/// <summary>Name of the prefix.</summary>
	PrefixName,
	/// <summary>Attribute lines of the prefix.</summary>
	PrefixAttributes,
	/// <summary>Name of the suffix.</summary>
	SuffixName,
	/// <summary>Attribute lines of the suffix.</summary>
	SuffixAttributes,
	/// <summary>Mod the item comes from (<c>Item.Mod</c>).</summary>
	Mod,
}

/// <summary>
/// One value of a search category and the items having it.
/// </summary>
/// <param name="DisplayValue">The value as shown, without TQ color tags.</param>
/// <param name="Results">The items having this value, in database order. An item having the value twice is listed twice.</param>
public sealed record SearchCategoryEntry(string DisplayValue, IReadOnlyList<SearchResult> Results);
