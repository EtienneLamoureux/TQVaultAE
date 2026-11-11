using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

/// <summary>
/// Resolves items linked to a mod with that mod's database (mod data first, then the base game's).
/// </summary>
public class ItemModService(IDatabaseMaps maps, IDatabase database, IModLinkService modLinks) : IItemModService
{
	private readonly IDatabaseMaps Maps = maps;
	private readonly IDatabase Database = database;
	private readonly IModLinkService ModLinks = modLinks;

	/// <inheritdoc/>
	public IReadOnlyList<InstalledMod> InstalledMods => this.ModLinks.GetInstalledMods();

	/// <inheritdoc/>
	public InstalledMod? Find(string? modName)
		=> string.IsNullOrWhiteSpace(modName)
			? null
			: this.InstalledMods.FirstOrDefault(m => m.Name.Equals(modName, StringComparison.OrdinalIgnoreCase));

	/// <inheritdoc/>
	public IDisposable Scope(Item? item)
		// The item's map, and only it: a mod not installed reads the base game data. An item without a map yet (being
		// read, or a placeholder) uses the data in use at that moment.
		=> item is null || item.ModOrigin == ModOrigin.None
			? NoScope.Instance
			: this.Maps.Use(this.Find(item.Mod)?.Folder ?? string.Empty);

	/// <inheritdoc/>
	public ModSuggestion Suggest(IEnumerable<Item> items)
		=> this.ModLinks.SuggestFor(items.SelectMany(RecordsOf).Select(id => id.Raw));

	private static IEnumerable<RecordId> RecordsOf(Item item)
		=> new[] { item.BaseItemId, item.prefixID, item.suffixID, item.relicID, item.RelicBonusId, item.relic2ID, item.RelicBonus2Id }
			.Where(id => !RecordId.IsNullOrEmpty(id))!;
}
