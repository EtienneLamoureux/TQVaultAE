using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Comparison;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Services;

/// <summary>
/// Compares two items stat by stat, from the values their tooltips display.
/// </summary>
public class ItemComparisonService(IItemProvider itemProvider) : IItemComparisonService
{
	private readonly IItemProvider _itemProvider = itemProvider;

	/// <inheritdoc/>
	public ItemComparison Compare(Item first, Item second)
	{
		// The stats are read with the granted skills apart, so that a skill's effects are compared with the same skill's
		// and never summed with the item attributes; the tooltips show the classic display, skills inline.
		var stats = ItemStatComparer.Compare(this.SheetOf(first), this.SheetOf(second));
		return new ItemComparison(
			this._itemProvider.GetFriendlyNames(first, FriendlyNamesExtraScopes.ItemFullDisplay)
			, this._itemProvider.GetFriendlyNames(second, FriendlyNamesExtraScopes.ItemFullDisplay)
			, stats);
	}

	private ItemStatSheet SheetOf(Item item)
		=> ItemStatSheet.From(this._itemProvider.GetFriendlyNames(item, FriendlyNamesExtraScopes.ItemFullDisplay | FriendlyNamesExtraScopes.GrantedSkills));
}
