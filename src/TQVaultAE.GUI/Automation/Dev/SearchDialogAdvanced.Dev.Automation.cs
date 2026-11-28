using System.Security.Cryptography;
using System.Text;
using TQVaultAE.GUI.Automation;
using TQVaultAE.GUI.Models.Search;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>Debug access to the Advanced Search tool tips.</summary>
public partial class SearchDialogAdvanced
{
	/// <summary>
	/// Shows a tool tip of the dialog as the pointer resting on its label does, through the label's own handler:
	/// <c>filters</c> for the selected filters, <c>results</c> for the results found; <c>none</c> hides both.
	/// </summary>
	/// <returns>False when the handler showed nothing (no filter selected, no result).</returns>
	internal bool AutomationShowTooltip(string kind)
	{
		switch (kind.ToLowerInvariant())
		{
			case "filters":
				this.scalingLabelFiltersSelected_MouseEnter(this.scalingLabelFiltersSelected, EventArgs.Empty);
				return System.Windows.Forms.Application.OpenForms.OfType<Tooltips.SearchFiltersTooltip>().Any();
			case "results":
				this.scalingLabelProgress_MouseEnter(this.scalingLabelProgress, EventArgs.Empty);
				return System.Windows.Forms.Application.OpenForms.OfType<Tooltips.FoundResultsTooltip>().Any();
			case "none":
				this.scalingLabelFiltersSelected_MouseLeave(this.scalingLabelFiltersSelected, EventArgs.Empty);
				this.scalingLabelProgress_MouseLeave(this.scalingLabelProgress, EventArgs.Empty);
				return true;
			default:
				throw new AutomationRefusedException($"Unknown tool tip '{kind}': filters, results or none.");
		}
	}

	/// <summary>
	/// Rests the pointer on entry <paramref name="index"/> of a category list: its own MouseMove handler sets the
	/// matching items tool tip, which is then shown at once (the user sees it after the hover delay).
	/// </summary>
	/// <returns>The tool tip text (empty when the entry has none).</returns>
	internal string AutomationShowCountTooltip(Components.Layout.VirtualCheckedListBox list, int index)
	{
		var point = Point.Empty;
		var found = false;
		for (int y = 1; y < list.ClientSize.Height && !found; y += 2)
			for (int x = 4; x < list.ClientSize.Width && !found; x += 8)
				if (list.IndexFromPoint(x, y) == index)
					(point, found) = (new Point(x + 20, y), true);
		if (!found)
			throw new AutomationRefusedException($"Entry {index} of '{list.Name}' is not shown.");

		this.scalingCheckedListBox_MouseMove(list, new MouseEventArgs(MouseButtons.None, 0, point.X, point.Y, 0));
		var text = this.toolTip.GetToolTip(list) ?? string.Empty;
		if (!string.IsNullOrEmpty(text))
			this.toolTip.Show(text, list, point.X + 4, point.Y + 20);
		return text;
	}
}
