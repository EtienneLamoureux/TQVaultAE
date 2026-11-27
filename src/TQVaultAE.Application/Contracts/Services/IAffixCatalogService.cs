using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Builds the catalogs offered by the item context menu: set members, completion bonuses and affixes.
/// </summary>
/// <remarks>
/// Returns plain data sorted for display; the GUI only turns it into menu items.
/// </remarks>
public interface IAffixCatalogService
{
	/// <summary>
	/// Gets the other members of the item's set, sorted by name.
	/// </summary>
	/// <param name="item">A set item.</param>
	/// <returns>The set members except the item itself; empty when the item is not part of a set.</returns>
	IReadOnlyList<SetMemberChoice> GetSetMemberChoices(Item item);

	/// <summary>
	/// Gets the completion bonuses of a completed relic or charm, or of an artifact, sorted by label.
	/// </summary>
	/// <param name="item">The item.</param>
	/// <returns>The bonuses; empty when the item takes no completion bonus.</returns>
	IReadOnlyList<BonusChoice> GetCompletionBonusChoices(Item item);

	/// <summary>
	/// Gets the completion bonuses of the relics or charms socketed in the item, sorted by label.
	/// </summary>
	/// <param name="item">The item.</param>
	/// <returns>The bonuses of the first and second socket; empty lists when the socket has none.</returns>
	(IReadOnlyList<BonusChoice> Relic1, IReadOnlyList<BonusChoice> Relic2) GetSocketedBonusChoices(Item item);

	/// <summary>
	/// Gets the affixes the item can take, grouped by affix name.
	/// </summary>
	/// <param name="item">The item.</param>
	/// <param name="enableEpicLegendaryAffixes">
	/// Offer every affix of the gear type to epic and legendary items, not only those of the item's own tables.
	/// </param>
	/// <returns>The catalog, or null when the item takes no affix.</returns>
	AffixCatalog? GetAffixCatalog(Item item, bool enableEpicLegendaryAffixes);

	/// <summary>
	/// Regroups affix groups by effect (taken from the affix file name) instead of affix name.
	/// </summary>
	/// <param name="groups">Groups by affix name, as returned by <see cref="GetAffixCatalog"/>.</param>
	/// <returns>Groups keyed by effect, the entries sorted by effect then number.</returns>
	IReadOnlyList<AffixGroup> GroupByEffect(IEnumerable<AffixGroup> groups);

	/// <summary>
	/// Gets the label of an affix shown in the sub menu of a group with several variants.
	/// </summary>
	/// <param name="entry">The affix.</param>
	/// <param name="byEffect">The groups are keyed by effect (the label then shows the affix name).</param>
	/// <returns>The label: number, affix name or effect, weight and DLC.</returns>
	string FormatVariantLabel(AffixEntry entry, bool byEffect);
}
