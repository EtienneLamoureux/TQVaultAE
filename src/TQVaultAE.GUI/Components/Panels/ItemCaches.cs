using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Tooltips;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// The cached pictures and names of items changed in place, made again at their next use.
/// </summary>
internal static class ItemCaches
{
	/// <summary>
	/// Drops the item tool tips of <paramref name="items"/> and, unless <paramref name="bagTooltips"/> is false, the bag
	/// summary tool tips that show them; given <paramref name="names"/>, their friendly names too.
	/// </summary>
	public static void Invalidate(Item?[] items, IItemProvider? names, bool bagTooltips = true)
	{
		ItemTooltip.InvalidateCache(items);
		if (bagTooltips)
			BagButtonTooltip.InvalidateCache(items);
		names?.InvalidateFriendlyNamesCache(items.OfType<Item>().ToArray());
	}
}
