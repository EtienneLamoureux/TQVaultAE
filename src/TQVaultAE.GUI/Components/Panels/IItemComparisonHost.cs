using TQVaultAE.Domain.Entities;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// The control holding the item panels that opens the item comparisons (the main content): a panel finds it among its
/// parents.
/// </summary>
internal interface IItemComparisonHost
{
	/// <summary>
	/// Opens a new comparison window of <paramref name="first"/> and <paramref name="second"/>, beside the ones already
	/// open; it shows the items as they are now.
	/// </summary>
	void ShowComparison(Item first, Item second);
}
