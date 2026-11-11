namespace TQVaultAE.Domain.Entities;

/// <summary>
/// Where an item's map (<see cref="Item.Mod"/>) comes from.
/// </summary>
public enum ModOrigin
{
	/// <summary>No map yet: the item was not put in a container. It is read with the data in use at that moment.</summary>
	None = 0,

	/// <summary>The item's own: a mod or the base game chosen for it, a link read from a vault file or an export, or
	/// the map it had when it left a character. Kept wherever the item goes.</summary>
	Own,

	/// <summary>The map of the container holding it (vault, character, game stash): it changes with the container's
	/// link, and with the container when the item moves.</summary>
	Inherited,
}
