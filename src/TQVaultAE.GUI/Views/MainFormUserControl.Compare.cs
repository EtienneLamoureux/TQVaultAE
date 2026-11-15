using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Dialogs;

namespace TQVaultAE.GUI.Views;

/// <summary>
/// The item comparisons of the main content: each one in its own window, floating over the main window like the search
/// results, as many as the user opens.
/// </summary>
public partial class MainFormUserControl : IItemComparisonHost
{
	/// <inheritdoc/>
	void IItemComparisonHost.ShowComparison(Item first, Item second) => this.OpenComparison(first, second);

	/// <summary>
	/// Opens a new comparison window of <paramref name="first"/> and <paramref name="second"/>, owned by the main window
	/// (closed with it) and placed clear of the comparisons already open. It compares copies of the items made now: the
	/// window stays right whatever happens to the items while it is open (moved, changed, deleted, or their vault or
	/// character closed). The window disposes itself when it closes.
	/// </summary>
	/// <returns>The window, shown.</returns>
	internal ItemComparisonDialog OpenComparison(Item first, Item second)
	{
		var dialog = this.Services.Forms.Create<ItemComparisonDialog>();
		dialog.First = first.Clone();
		dialog.Second = second.Clone();
		dialog.Show();
		return dialog;
	}

	/// <summary>The comparison windows open, oldest first.</summary>
	internal IReadOnlyList<ItemComparisonDialog> OpenComparisons
		=> this.FindForm()?.OwnedForms.OfType<ItemComparisonDialog>().Where(f => !f.IsDisposed).ToList() ?? [];
}
