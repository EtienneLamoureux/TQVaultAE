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
	/// The installed mod an item, a vault or an export names (<see cref="IModLinkService.FindMod"/>).
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
	/// Loads the database of a mod's map on a background thread when it is not loaded yet (about a second for a large
	/// mod), so that a choice of that mod does not wait for it on the UI thread. A failed load is left to the first use,
	/// which tries again and reports it.
	/// </summary>
	/// <param name="modName">The mod (<see cref="InstalledMod.Name"/>); null, or a mod not installed, for the base game.</param>
	/// <returns>Completes once the database is loaded, or the load failed; at once when it is loaded already.</returns>
	Task LoadDataAsync(string? modName);

	/// <summary>
	/// Suggests the mod of items (one item, or a vault's items) from the records they name that the base game does not
	/// have, like the Suggest button of the mod link dialog.
	/// </summary>
	ModSuggestion Suggest(IEnumerable<Item> items);
}

/// <summary>
/// The scope of an item's map for code that may run without <see cref="IItemModService"/> (tests).
/// </summary>
public static class ItemModScopes
{
	/// <summary>
	/// <see cref="IItemModService.Scope"/> of the item, or a scope that does nothing without the service: every reading
	/// of an item's records (its attributes, affixes, relics, granted skills, set) goes through it.
	/// </summary>
	public static IDisposable ScopeOrNone(this IItemModService? itemMods, Item? item)
		=> itemMods?.Scope(item) ?? NoScope.Instance;
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
