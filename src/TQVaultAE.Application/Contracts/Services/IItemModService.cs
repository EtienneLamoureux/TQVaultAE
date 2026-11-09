using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// The mod an item is linked to (<see cref="Item.Mod"/>): its records resolve in that mod's database, mod data first and
/// then the base game's, whatever the active map is.
/// </summary>
public interface IItemModService
{
	/// <summary>
	/// The mods installed on this computer.
	/// </summary>
	IReadOnlyList<InstalledMod> InstalledMods { get; }

	/// <summary>
	/// The installed mod of a name (<see cref="InstalledMod.Name"/>).
	/// </summary>
	/// <returns>The mod; null for none or when it is not installed.</returns>
	InstalledMod? Find(string? modName);

	/// <summary>
	/// Makes the database of an item's mod answer for the current thread until disposed, so that reading, drawing and
	/// describing the item use its mod's data. Does nothing for an item without a mod or whose mod is not installed.
	/// </summary>
	IDisposable Scope(Item? item);

	/// <summary>
	/// True when an item names a record the base game does not have: it belongs to a mod.
	/// </summary>
	bool IsModOnly(Item item);

	/// <summary>
	/// Links to a mod the items that are not linked yet and that only that mod explains (records the base game does not
	/// have): the items of a character linked to that mod.
	/// </summary>
	/// <returns>Number of items linked.</returns>
	int AdoptMod(IEnumerable<Item> items, string modName);

	/// <summary>
	/// Suggests the mod of items (one item, or a vault's items) from the records they name that the base game does not
	/// have, like the Suggest button of the mod link dialog.
	/// </summary>
	ModSuggestion Suggest(IEnumerable<Item> items);
}

/// <summary>
/// A scope that does nothing.
/// </summary>
public sealed class NoScope : IDisposable
{
	public static readonly NoScope Instance = new();

	private NoScope()
	{
	}

	public void Dispose()
	{
	}
}
