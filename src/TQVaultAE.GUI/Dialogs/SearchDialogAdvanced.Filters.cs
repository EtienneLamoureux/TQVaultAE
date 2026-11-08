/// <summary>
/// Class for the Search Dialog box.
/// </summary>

using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Models.Search;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Dialogs;

public partial class SearchDialogAdvanced
{
	private void CleanAllCheckBoxes()
	{
		this.flowLayoutPanelMain.SuspendLayout();
		try
		{
			foreach (var lb in this.CachedCheckedListBoxes)
			{
				lb.BeginUpdate();
				lb.Items.Clear();
				lb.EndUpdate();
			}
		}
		finally
		{
			this.flowLayoutPanelMain.ResumeLayout(true);
		}
	}

	private void scalingCheckedListBox_MouseMove(object? sender, MouseEventArgs e)
	{
		var ctr = (Control)sender!;
		var (lstBox, tag) = ctr.GetBoxTag();

		var focusedIdx = lstBox.IndexFromPoint(e.Location);

		if (tag.LastTooltipIndex != focusedIdx)
		{
			tag.LastTooltipIndex = focusedIdx;
			if (tag.LastTooltipIndex > -1)
			{
				var item = lstBox.Items[focusedIdx] as BoxItem;
				if (item is not null && item.MatchingResults is not null)
					toolTip.SetToolTip(lstBox, string.Format(Resources.SearchMatchingItemsTT, item.MatchingResults.Count()));
				else
					toolTip.SetToolTip(lstBox, string.Empty);
			}
		}

		lstBox.Tag = tag;
	}

	private void SyncNaveButton()
	{
		/// Push invisible categories at the end of <see cref="flowLayoutPanelMain"/> so nav buttons define categories order in flowpanel
		var trail = NavMap.Where(map => !map.Panel.Visible).Select(map => map.Panel).ToList();
		// Remove
		trail.ForEach(c => this.flowLayoutPanelMain.Controls.Remove(c));
		// Then Put it back at the end
		this.flowLayoutPanelMain.Controls.AddRange(trail.ToArray());

		SyncNavButtonImage();
	}

	private void SyncNavButtonImage()
	{
		foreach (var map in NavMap)
		{
			// Invert Up & Down each time you click to make it behave like a toggle
			if (map.Panel.Visible)
			{
				map.Button.UpBitmap = this.ButtonImageDown;
				map.Button.OverBitmap = this.ButtonImageUp;
				map.Button.Image = map.Button.DownBitmap;
			}
			else
			{
				map.Button.UpBitmap = this.ButtonImageUp;
				map.Button.OverBitmap = this.ButtonImageDown;
				map.Button.Image = map.Button.UpBitmap;
			}
		}
	}

	private void Sync_SelectedFilters()
	{
		this.SelectedFilters.Clear();
		this.SelectedFiltersSet.Clear();

		// Add the SearchTerm on top 
		var (_, searchTermBoxItem) = this.scalingTextBoxSearchTerm.GetBoxItem(false);
		// if i get something to filter with
		if (!string.IsNullOrWhiteSpace(searchTermBoxItem.DisplayValue))
		{
			this.SelectedFilters.Add(searchTermBoxItem);
			this.SelectedFiltersSet.Add(searchTermBoxItem);
		}

		// Use cached list for performance
		// Use OfType instead of Cast to handle any non-BoxItem objects gracefully
		foreach (var lb in this.CachedCheckedListBoxes)
			this.SelectedFilters.AddRange(lb.CheckedItems.OfType<BoxItem>());

		// Populate HashSet for O(1) lookups
		foreach (var filter in this.SelectedFilters)
			this.SelectedFiltersSet.Add(filter);

		this.Apply_SelectedFilters();
	}

	private void scalingLabelCategory_MouseClick(object? sender, MouseEventArgs e)
	{
		var label = (ScalingLabel)sender!;
		if (label.Parent is not { } flowpanel)
			return;

		if (e.Button == MouseButtons.Left)
		{
			// Individual Expand/Collapse
			flowpanel.ProcessAllControls(c =>
			{
				if (c is VirtualCheckedListBox lb)
					lb.Visible = !lb.Visible;
			});
		}
		// Uncheck
		if (e.Button == MouseButtons.Right)
		{
			UncheckCategories(flowpanel);
			Sync_SelectedFilters();
		}
	}

	private void UncheckCategories(Control flowpanel)
	{
		// Optimize for main flowLayoutPanel - use cached list
		if (flowpanel == flowLayoutPanelMain)
		{
			this.flowLayoutPanelMain.SuspendLayout();
			try
			{
				foreach (var lb in this.CachedCheckedListBoxes)
				{
					lb.BeginUpdate();
					foreach (var idx in lb.CheckedIndices.Cast<int>().ToList()) lb.SetItemChecked(idx, false);
					lb.EndUpdate();
				}
			}
			finally
			{
				this.flowLayoutPanelMain.ResumeLayout(true);
			}
		}
		else
		{
			// Individual category panel - traverse
			flowpanel.ProcessAllControls(c =>
			{
				if (c is VirtualCheckedListBox lb)
				{
					lb.BeginUpdate();
					foreach (var idx in lb.CheckedIndices.Cast<int>().ToList()) lb.SetItemChecked(idx, false);
					lb.EndUpdate();
				}
			});
		}
	}

	private void Apply_SelectedFilters()
	{
		if (_Apply_SelectedFiltersDisabled)
			return;

		int MaxLvl = (int)numericUpDownMaxLvl.Value,
			MaxDex = (int)numericUpDownMaxDex.Value,
			MaxInt = (int)numericUpDownMaxInt.Value,
			MaxStr = (int)numericUpDownMaxStr.Value,
			MinLvl = (int)numericUpDownMinLvl.Value,
			MinDex = (int)numericUpDownMinDex.Value,
			MinInt = (int)numericUpDownMinInt.Value,
			MinStr = (int)numericUpDownMinStr.Value;
		bool MaxRequierement = scalingCheckBoxMaxReq.Checked, MinRequierement = scalingCheckBoxMinReq.Checked;

		this.scalingLabelFiltersSelected.Text = string.Format(this.scalingLabelFiltersSelected.Tag?.ToString() ?? "{0}", SelectedFilters.Count);

		// Combine selected filter results based on operator
		var initialResults = this.ItemDatabaseService.CombineFilterResults(
			this.SelectedFilters.Select(f => f.MatchingResults)
			, this.scalingComboBoxOperator.SelectedIndex == (int)SearchOperator.And ? SearchOperator.And : SearchOperator.Or);

		// Apply quick filters and requirements via service
		this.QueryResults = this.ItemDatabaseService.ExecuteAdvancedSearch(
			new AdvancedSearchRequest
			{
				InitialResults = initialResults,
				HasPrefix = this.scalingCheckBoxHavingPrefix.Checked,
				HasSuffix = this.scalingCheckBoxHavingSuffix.Checked,
				HasRelic = this.scalingCheckBoxHavingRelic.Checked,
				HasCharm = this.scalingCheckBoxHavingCharm.Checked,
				IsSetItem = this.scalingCheckBoxIsSetItem.Checked,
				HasSkill = this.scalingCheckBoxHavingSkill.Checked,
				MinLevel = MinRequierement ? MinLvl : 0,
				MaxLevel = MaxRequierement ? MaxLvl : 0,
				MinStrength = MinRequierement ? MinStr : 0,
				MaxStrength = MaxRequierement ? MaxStr : 0,
				MinDexterity = MinRequierement ? MinDex : 0,
				MaxDexterity = MaxRequierement ? MaxDex : 0,
				MinIntelligence = MinRequierement ? MinInt : 0,
				MaxIntelligence = MaxRequierement ? MaxInt : 0
			}
		).ToArray();

		// No filter (after Reset or unticking the last one): the count of every item, as when the dialog opened,
		// rather than 0.
		scalingLabelProgress.Text = this.SelectedFilters.Count == 0
			? string.Format(Resources.SearchItemCountIs, this.ItemDatabaseService.ItemDatabase.Count)
			: string.Format(Resources.SearchItemCountIs, QueryResults.Length);

		ApplyCategoriesReducer();
	}

	private void ApplyCategoriesReducer()
	{
		// Category Reduced Display
		if (scalingCheckBoxReduceDuringSelection.Checked)
		{
			ReduceCheckBoxesToQueryResult();
			return;
		}

		if (
			this.FilterCategories.HasChanged
			|| (
				// Category Full Display
				!scalingCheckBoxReduceDuringSelection.Checked
				/// But comes from <see cref="scalingCheckBoxReduceDuringSelection_CheckedChanged"/> meaning "from a Category Reduced Display"
				&& scalingCheckBoxReduceDuringSelection.Checked != scalingCheckBoxReduceDuringSelection_LastChecked
			)
		)
		{
			ResetCheckBoxesToFirstLoad();// Restore Full Display
		}
	}

	private void ResetCheckBoxesToFirstLoad()
	{
		// Reset to FirstLoad
		CleanAllCheckBoxes();

		this.flowLayoutPanelMain.SuspendLayout();
		try
		{
			foreach (var lb in this.CachedCheckedListBoxes)
			{
				var (_, tag) = lb.GetBoxTag();
				lb.BeginUpdate();
				foreach (var item in tag.DataSource)
				{
					// Use HashSet for O(1) lookup instead of O(n) List.Contains
					var isChecked = this.SelectedFiltersSet.Contains(item);

					if (!isChecked // Do not hide already checked
						/// Should i filter out this check box based on <see cref="FilterCategories"/> ?
						&& this.FilterCategories.AvoidDisplayAttribute(item.DisplayValue)
					) continue;

					lb.Items.Add(item, isChecked);
				}
				lb.EndUpdate();
			}
		}
		finally
		{
			this.flowLayoutPanelMain.ResumeLayout(true);
		}
	}

	private void ReduceCheckBoxesToQueryResult()
	{
		// Nothing selected: there is nothing to reduce to, show every category entry.
		if (this.SelectedFilters.Count == 0)
		{
			ResetCheckBoxesToFirstLoad();
			return;
		}

		if (this.QueryResults.Length != 0)
		{
			CleanAllCheckBoxes();

			this.flowLayoutPanelMain.SuspendLayout();
			try
			{
				// Reset to QueryResult.
				var queryResults = new HashSet<SearchResult>(this.QueryResults);
				foreach (var lb in this.CachedCheckedListBoxes)
				{
					var (_, tag) = lb.GetBoxTag();

					// Entries matching the query results
					var matching = this.ItemDatabaseService.KeepEntriesMatching(tag.DataSource, b => b.MatchingResults, queryResults);

					lb.BeginUpdate();
					foreach (var item in matching)
					{
						// Use HashSet for O(1) lookup instead of O(n) List.Contains
						var isChecked = this.SelectedFiltersSet.Contains(item);

						if (!isChecked // Do not hide already checked
							/// Should i filter out this check box based on <see cref="FilterCategories"/> ?
							&& this.FilterCategories.AvoidDisplayAttribute(item.DisplayValue)
						) continue;

						lb.Items.Add(item, isChecked);
					}
					lb.EndUpdate();
				}
			}
			finally
			{
				this.flowLayoutPanelMain.ResumeLayout(true);
			}
		}
	}

	/// <summary>
	/// Clears the selection, the search term and the category filter, and shows every category entry again
	/// (also when the categories were reduced to the previous results).
	/// </summary>
	private void ResetSelectedFilters()
	{
		this._Apply_SelectedFiltersDisabled = true;
		try
		{
			this.scalingComboBoxQueryList.ResetText();
			ResetQuickFilters();
			ResetRequirements();
			UncheckCategories(flowLayoutPanelMain);
			TextBoxSearchTerm_UpdateText_Notrigger(string.Empty);
			ClearFilterCategories();
		}
		finally
		{
			this._Apply_SelectedFiltersDisabled = false;
		}

		// Recompute the (now empty) selection; with nothing selected the categories return to their full content.
		TextBoxSearchTerm_TextChanged_Logic();
		ResetCheckBoxesToFirstLoad();
	}

	internal void ResetRequirements()
	{
		scalingCheckBoxMaxReq.Checked =
		scalingCheckBoxMinReq.Checked = false;
		numericUpDownMaxDex.Value =
		numericUpDownMaxInt.Value =
		numericUpDownMaxLvl.Value =
		numericUpDownMaxStr.Value =
		numericUpDownMinDex.Value =
		numericUpDownMinInt.Value =
		numericUpDownMinLvl.Value =
		numericUpDownMinStr.Value = 0;
	}

	private void typeAssistantFilterCategories_Idled(object? sender, EventArgs e)
	{
		var f = FilterCategories;

		f.SearchRawPrevious = f.SearchRaw;
		f.SearchRaw = this.scalingTextBoxFilterCategories.Text;
		f.HasFilterCategory = !string.IsNullOrWhiteSpace(f.SearchRaw);

		if (f.HasFilterCategory)
			(f.IsRegex, f.Search, f.Regex, f.RegexIsValid)
				= StringHelper.IsTQVaultSearchRegEx(f.SearchRaw);

		/// Wrapped into an Invoke() because i'm currently in the thread of the
		/// <see cref="typeAssistantFilterCategories"> and i need  <see cref="ApplyCategoriesReducer"/> to be executed from the main thread to avoid concurrent access exception
		/// Check IsHandleCreated and !IsDisposed to avoid InvalidOperationException when the form is initializing or disposing
		if (this.IsHandleCreated && !this.IsDisposed)
			this.Invoke(ApplyCategoriesReducer);
	}

	/// <param name="searchQuery">The saved query.</param>
	/// <param name="filters">Its live filters (<see cref="LoadPersonnalQueries"/>).</param>
	private void Make_SelectedFilters(SearchQuery searchQuery, IReadOnlyList<BoxItem> filters)
	{
		// Make _SelectedFilters from saved query
		this.SelectedFilters.Clear();
		this.SelectedFiltersSet.Clear();

		this.SelectedFilters.AddRange(filters);

		// Populate HashSet for O(1) lookups
		foreach (var filter in this.SelectedFilters)
			this.SelectedFiltersSet.Add(filter);

		ResetCheckBoxesToFirstLoad();

		// Restore SearchTerm ?
		var term = filters.FirstOrDefault(i => i.CheckedList is null);

		/// Avoid trigger of <see cref="scalingTextBoxSearchTerm_TextChanged"/>
		if (term is null)
			TextBoxSearchTerm_UpdateText_Notrigger(string.Empty);
		else
		{
			TextBoxSearchTerm_UpdateText_Notrigger(term.DisplayValue);
			this.scalingTextBoxSearchTerm.Tag = term;
		}

		MakeSearchTermBoxItem();

		_Apply_SelectedFiltersDisabled = true;// Prevent filter before ui is fully init

		(// Find category visibility differences and apply
			from map in this.NavMap
			join vis in searchQuery.Visible on map.Button.Name equals vis.Name
			where map.Panel.Visible != vis.Visible
			select map
		).ToList().ForEach(map => map.Button.PerformClick());

		// Requirements
		this.numericUpDownMaxStr.Value = searchQuery.MaxStr;
		this.numericUpDownMaxDex.Value = searchQuery.MaxDex;
		this.numericUpDownMaxInt.Value = searchQuery.MaxInt;
		this.numericUpDownMinStr.Value = searchQuery.MinStr;
		this.numericUpDownMinDex.Value = searchQuery.MinDex;
		this.numericUpDownMinInt.Value = searchQuery.MinInt;
		this.numericUpDownMaxLvl.Value = searchQuery.MaxLvl;
		this.numericUpDownMinLvl.Value = searchQuery.MinLvl;
		this.scalingCheckBoxMinReq.Checked = searchQuery.MinRequirement;
		this.scalingCheckBoxMaxReq.Checked = searchQuery.MaxRequirement;

		// Quick Filters
		this.scalingCheckBoxHavingPrefix.Checked = searchQuery.HavingPrefix;
		this.scalingCheckBoxHavingSuffix.Checked = searchQuery.HavingSuffix;
		this.scalingCheckBoxHavingRelic.Checked = searchQuery.HavingRelic;
		this.scalingCheckBoxHavingCharm.Checked = searchQuery.HavingCharm;
		this.scalingCheckBoxIsSetItem.Checked = searchQuery.IsSetItem;
		this.scalingCheckBoxHavingSkill.Checked = searchQuery.HavingSkill;

		// UI Behaviors
		this.numericUpDownMaxElement.Value = searchQuery.MaxElement;
		this.scalingComboBoxOperator.SelectedIndex = (int)searchQuery.Logic;
		this.scalingTextBoxFilterCategories.Text = searchQuery.Filter;
		this.scalingCheckBoxReduceDuringSelection.Checked = searchQuery.Reduce;

		_Apply_SelectedFiltersDisabled = false;

		this.Apply_SelectedFilters();
	}

	private void numericUpDownMinMax_ValueChanged(object? sender, EventArgs e)
	{
		var num = (NumericUpDown)sender!;

		// Avoid absurd range: the other bound follows the one changed
		foreach (var (min, max) in new[] {
			(this.numericUpDownMinLvl, this.numericUpDownMaxLvl),
			(this.numericUpDownMinStr, this.numericUpDownMaxStr),
			(this.numericUpDownMinDex, this.numericUpDownMaxDex),
			(this.numericUpDownMinInt, this.numericUpDownMaxInt),
		})
		{
			if (num != min && num != max)
				continue;

			var (low, high) = AdvancedSearchRequest.OrderRange((int)min.Value, (int)max.Value, num == min);
			if (min.Value != low)
				min.Value = low;
			if (max.Value != high)
				max.Value = high;
			break;
		}

		if (// No need to filter if corresponding checkbox is uncheck
			(num.Name.Contains("Min") && !this.scalingCheckBoxMinReq.Checked)
			|| (num.Name.Contains("Max") && !this.scalingCheckBoxMaxReq.Checked)
		) return;

		Apply_SelectedFilters();
	}
}
