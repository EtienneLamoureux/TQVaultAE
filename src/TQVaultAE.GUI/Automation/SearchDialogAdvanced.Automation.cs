using System.Security.Cryptography;
using System.Text;
using TQVaultAE.GUI.Models.Search;

namespace TQVaultAE.GUI.Dialogs;

public partial class SearchDialogAdvanced
{
	/// <summary>
	/// The user's state of the open dialog, read from its controls (not from <see cref="SearchDialogState"/>), and its
	/// results, to compare a dialog with the one made again at another zoom.
	/// </summary>
	internal object AutomationState()
	{
		var results = this.QueryResults
			.Select(r => $"{r.Container}|{r.SackType}|{r.SackNumber}|{r.Item.PositionX},{r.Item.PositionY}|{r.Item.BaseItemId.Raw}|{r.Item.Seed}")
			.Order(StringComparer.Ordinal)
			.ToList();
		var scroll = this.flowLayoutPanelMainAutoScroll.AutoScrollPosition;

		return new
		{
			zoom = this.ScaledZoom,
			size = new { width = this.Width, height = this.Height },
			layoutSize = new { width = this._layoutSize.Width, height = this._layoutSize.Height },
			windowState = this.WindowState.ToString(),
			statePending = this._pendingState is not null,
			searchText = this.scalingTextBoxSearchTerm.Text,
			appliedSearchText = (this.scalingTextBoxSearchTerm.Tag as BoxItem)?.DisplayValue,
			categoryFilter = this.scalingTextBoxFilterCategories.Text,
			queryText = this.scalingComboBoxQueryList.Text,
			selectedQuery = this.scalingComboBoxQueryList.SelectedItem?.ToString(),
			operatorIndex = this.scalingComboBoxOperator.SelectedIndex,
			reduce = this.scalingCheckBoxReduceDuringSelection.Checked,
			maxElement = this.numericUpDownMaxElement.Value,
			quickFilters = new
			{
				prefix = this.scalingCheckBoxHavingPrefix.Checked,
				suffix = this.scalingCheckBoxHavingSuffix.Checked,
				relic = this.scalingCheckBoxHavingRelic.Checked,
				charm = this.scalingCheckBoxHavingCharm.Checked,
				setItem = this.scalingCheckBoxIsSetItem.Checked,
				skill = this.scalingCheckBoxHavingSkill.Checked,
			},
			requirements = new
			{
				min = this.scalingCheckBoxMinReq.Checked,
				max = this.scalingCheckBoxMaxReq.Checked,
				minLvl = this.numericUpDownMinLvl.Value,
				maxLvl = this.numericUpDownMaxLvl.Value,
				minStr = this.numericUpDownMinStr.Value,
				maxStr = this.numericUpDownMaxStr.Value,
				minDex = this.numericUpDownMinDex.Value,
				maxDex = this.numericUpDownMaxDex.Value,
				minInt = this.numericUpDownMinInt.Value,
				maxInt = this.numericUpDownMaxInt.Value,
			},
			selectedFilters = this.SelectedFilters.Select(f => $"{f.CheckedListName}:{f.DisplayValue}").ToList(),
			categoryOrder = this.flowLayoutPanelMain.Controls.Cast<Control>().Select(c => c.Name).ToList(),
			hiddenCategories = this.NavMap.Where(m => !m.Panel.Visible).Select(m => m.Panel.Name).ToList(),
			lists = this.CachedCheckedListBoxes.Select(lb => new
			{
				name = lb.Name,
				collapsed = !lb.VisibleSelf,
				count = lb.Items.Count,
				@checked = lb.CheckedItems.Select(i => i.ToString()).ToList(),
				focusedIndex = lb.FocusedIndex,
				firstColumn = lb.FirstColumn,
			}).ToList(),
			scroll = new { x = -scroll.X, y = -scroll.Y },
			resultCount = this.QueryResults.Length,
			resultHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n", results))))[..16],
			resultsFirst = results.Take(5).ToList(),
			progress = this.scalingLabelProgress.Text,
			filtersSelectedLabel = this.scalingLabelFiltersSelected.Text,
		};
	}

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
				throw new ArgumentException($"Unknown tool tip '{kind}': filters, results or none.", nameof(kind));
		}
	}
}
