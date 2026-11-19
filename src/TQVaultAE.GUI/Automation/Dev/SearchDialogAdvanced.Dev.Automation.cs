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
}
