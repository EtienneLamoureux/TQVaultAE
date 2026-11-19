using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

/// <summary>
/// Resolves items linked to a mod with that mod's database (mod data first, then the base game's).
/// </summary>
public class ItemModService(IDatabaseMaps maps, IModLinkService modLinks) : IItemModService
{
	private readonly IDatabaseMaps Maps = maps;
	private readonly IModLinkService ModLinks = modLinks;

	/// <inheritdoc/>
	public IReadOnlyList<InstalledMod> InstalledMods => this.ModLinks.GetInstalledMods();

	/// <inheritdoc/>
	public InstalledMod? Find(string? modName) => this.ModLinks.FindMod(modName);

	/// <inheritdoc/>
	public IDisposable Scope(Item? item)
		// The map of the item's container, and only it: a mod not installed reads the base game data. An item without a
		// container yet (being read, or a placeholder) uses the data in use at that moment.
		=> item is null || !item.HasContainerMod
			? NoScope.Instance
			: this.Maps.Use(this.Find(item.ContainerMod)?.Folder ?? string.Empty);

	/// <inheritdoc/>
	public Task LoadDataAsync(string? modName)
	{
		string folder = this.Find(modName)?.Folder ?? string.Empty;
		if (this.Maps.LoadedMapFolders.Any(loaded => GameMaps.Same(loaded, folder)))
			return Task.CompletedTask;

		return Task.Run(() =>
		{
			try
			{
				this.Maps.Preload(folder);
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
			{
				// Not cached: the first use loads it again and reports the error as it always did.
			}
		});
	}

	/// <inheritdoc/>
	public ModSuggestion Suggest(IEnumerable<Item> items)
		=> this.ModLinks.SuggestFor(items.SelectMany(RecordsOf).Select(id => id.Raw));

	private static IEnumerable<RecordId> RecordsOf(Item item)
		=> item.RecordIds();
}
