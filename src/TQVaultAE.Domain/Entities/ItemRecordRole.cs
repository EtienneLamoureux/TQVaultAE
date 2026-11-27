namespace TQVaultAE.Domain.Entities;

/// <summary>
/// What a record named by an item is to it.
/// </summary>
public enum ItemRecordRole
{
	/// <summary>The base item.</summary>
	BaseItem,
	/// <summary>The prefix affix.</summary>
	Prefix,
	/// <summary>The suffix affix.</summary>
	Suffix,
	/// <summary>A relic or charm, socketed or the item itself.</summary>
	Relic,
	/// <summary>The completion bonus of a relic or charm.</summary>
	RelicBonus,
}
