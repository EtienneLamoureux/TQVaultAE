namespace TQVaultAE.Domain.Entities;

/// <summary>
/// Extra data scopes when getting item translations
/// </summary>
[Flags]
public enum FriendlyNamesExtraScopes
{
	BaseAttributes = 1 << 1,
	PrefixAttributes = 1 << 2,
	SuffixAttributes = 1 << 3,
	RelicAttributes = 1 << 4,
	Relic2Attributes = 1 << 5,
	Requirements = 1 << 6,
	ItemSet = 1 << 7,
	/// <summary>
	/// The skills granted by the item, its affixes and its relics, with their details, for a display that shows them apart:
	/// the attributes then name a granted skill without repeating its description and effects.
	/// </summary>
	GrantedSkills = 1 << 8,
	ItemExtraAttributes = PrefixAttributes | SuffixAttributes | RelicAttributes | Relic2Attributes,
	ItemFullDisplay = BaseAttributes | ItemExtraAttributes | ItemSet | Requirements,
}