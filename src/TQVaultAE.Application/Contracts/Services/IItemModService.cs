using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// The map of an item (<see cref="Item.Mod"/>): its records resolve in that mod's database, mod data first and then the
/// base game's, whatever the active map is.
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
	/// Makes the database of an item's map answer for the current thread until disposed, so that reading, drawing and
	/// describing the item use its map's data: its mod's, or the base game's for none or a mod that is not installed.
	/// An item without a map yet (<see cref="ModOrigin.None"/>) gets nothing: it uses the data in use at that moment.
	/// </summary>
	IDisposable Scope(Item? item);

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
