using TQVaultAE.Application.Search;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// The control holding the item panels that runs the item menu's <i>Search</i> (the main content): a panel finds it
/// among its parents.
/// </summary>
internal interface IItemCopySearchHost
{
	/// <summary>Shows the items matching <paramref name="item"/> for the search <paramref name="kind"/>.</summary>
	void ShowItemCopies(Item item, CopySearchKind kind);
}
