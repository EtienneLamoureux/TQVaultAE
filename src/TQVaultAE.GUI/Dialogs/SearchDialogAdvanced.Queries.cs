/// <summary>
/// Class for the Search Dialog box.
/// </summary>

using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Models.Search;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Dialogs;

public partial class SearchDialogAdvanced
{
	private void TextBoxSearchTerm_TextChanged_Logic()
	{
		MakeSearchTermBoxItem();

		/// Wrapped into an Invoke() because i'm currently in the thread of the
		/// <see cref="typeAssistantSearchBox"> and i need  <see cref="Sync_SelectedFilters"/> to be executed from the main thread to avoid concurrent access exception
		/// Check IsHandleCreated and !IsDisposed to avoid InvalidOperationException when the form is initializing or disposing
		if (this.IsHandleCreated && !this.IsDisposed)
			this.Invoke(Sync_SelectedFilters);
	}

	private BoxItem MakeSearchTermBoxItem(string? searchTerm = null)
	{
		// Init a special BoxItem for the search term
		var txt = searchTerm is null
			? scalingTextBoxSearchTerm.Text.Trim()
			: searchTerm.Trim();

		var (_, searchTermBoxItem) = scalingTextBoxSearchTerm.GetBoxItem(true);// true : i need to make a new intance here to make the SearchQuery remember search term (must not share same object reference)

		searchTermBoxItem.Category = scalingLabelSearchTerm;
		searchTermBoxItem.DisplayValue = txt;

		if (string.IsNullOrWhiteSpace(txt))
			searchTermBoxItem.MatchingResults = [];
		else
		{
			// Item fulltext search using service method
			var (isRegex, _, regex, regexIsValid) = StringHelper.IsTQVaultSearchRegEx(txt);

			searchTermBoxItem.MatchingResults = this.ItemDatabaseService.FullTextSearch(txt, isRegex && regexIsValid).ToList();
		}

		// When this method is used as a BoxItem factory, i don't want the textbox to keep the reference.
		if (searchTerm is not null)
			scalingTextBoxSearchTerm.Tag = null;

		return searchTermBoxItem;
	}

	private void LoadPersonnalQueries()
	{
		this.QueryFilters.Clear();
		var queries = this.SearchQueryService.Queries;
		if (queries.Count == 0)
			return;

		var liveFilters = this.LiveFilters();

		// Saved filters may not be found if you have lost the items that carry the corresponding properties.
		foreach (var query in queries)
			this.QueryFilters[query] = this.AttachFilters(query, liveFilters);

		SearchQueriesInit();
	}

	/// <summary>The live category values, by filter - use cached list for performance.</summary>
	private Dictionary<SearchFilter, BoxItem> LiveFilters()
	{
		var liveFilters = new Dictionary<SearchFilter, BoxItem>();
		foreach (var lb in this.CachedCheckedListBoxes)
		{
			var (_, tag) = lb.GetBoxTag();
			foreach (var boxItem in tag.DataSource)
				liveFilters.TryAdd(boxItem.ToFilter(), boxItem);
		}

		return liveFilters;
	}

	/// <summary>The live values of the filters of <paramref name="query"/>, the search term made again.</summary>
	private IReadOnlyList<BoxItem> AttachFilters(SearchQuery query, IReadOnlyDictionary<SearchFilter, BoxItem> liveFilters)
		=> this.SearchQueryService.AttachFilters(query, liveFilters, scalingLabelSearchTerm.Name, term => MakeSearchTermBoxItem(term));

	/// <summary>The query as set in the dialog.</summary>
	private SearchQuery CurrentQuery()
		=> new()
		{
			CheckedItems = this.SelectedFilters.Select(f => f.ToFilter()).ToList(),
			// Visible elements / category
			MaxElement = this.numericUpDownMaxElement.Value,
			// Reduce categories
			Reduce = this.scalingCheckBoxReduceDuringSelection.Checked,
			// Logical operator
			Logic = (SearchOperator)this.scalingComboBoxOperator.SelectedIndex,
			// Add Category filter
			Filter = this.scalingTextBoxFilterCategories.Text,
			// Add Display elements
			Visible = NavMap.Select(m => new SearchCategoryVisibility(m.Button.Name, m.Panel.Visible)).ToList(),
			// Requierements
			MinRequirement = scalingCheckBoxMinReq.Checked,
			MaxRequirement = scalingCheckBoxMaxReq.Checked,
			MaxLvl = (int)numericUpDownMaxLvl.Value,
			MaxStr = (int)numericUpDownMaxStr.Value,
			MaxDex = (int)numericUpDownMaxDex.Value,
			MaxInt = (int)numericUpDownMaxInt.Value,
			MinLvl = (int)numericUpDownMinLvl.Value,
			MinStr = (int)numericUpDownMinStr.Value,
			MinDex = (int)numericUpDownMinDex.Value,
			MinInt = (int)numericUpDownMinInt.Value,
			HavingPrefix = this.scalingCheckBoxHavingPrefix.Checked,
			HavingSuffix = this.scalingCheckBoxHavingSuffix.Checked,
			HavingRelic = this.scalingCheckBoxHavingRelic.Checked,
			HavingCharm = this.scalingCheckBoxHavingCharm.Checked,
			IsSetItem = this.scalingCheckBoxIsSetItem.Checked,
			HavingSkill = this.scalingCheckBoxHavingSkill.Checked,
		};

	private void scalingButtonQuerySave_Click(object? sender, EventArgs e)
	{
		var input = scalingComboBoxQueryList.Text.Trim();
		var draft = this.CurrentQuery();

		switch (this.SearchQueryService.CheckSave(input, draft))
		{
			case SearchQuerySaveCheck.NameMissing:
				this.Services.MessageBoxes.Inform(Resources.SearchQueryNameMustBeSet, Resources.GlobalInputWarning, MessageBoxIcon.Error);
				return;
			case SearchQuerySaveCheck.NoFilter:
				this.Services.MessageBoxes.Inform(Resources.SearchTermRequired, Resources.GlobalInputWarning, MessageBoxIcon.Error);
				return;
			case SearchQuerySaveCheck.NameExists:
				if (!this.Services.MessageBoxes.Confirm(Resources.SearchQueryNameAlreadyExist, Resources.GlobalInputWarning, MessageBoxIcon.Warning))
					return;
				break;
		}

		// Replaces the query of the same name, or adds it
		var replaced = this.SearchQueryService.Queries.FirstOrDefault(q => q.QueryName.Equals(input, StringComparison.OrdinalIgnoreCase));
		if (replaced is not null)
			this.QueryFilters.Remove(replaced);

		var saved = this.SearchQueryService.Save(input, draft);
		this.QueryFilters[saved] = this.SelectedFilters.ToArray();// i need a clone here so ToArray() do the job

		SearchQueriesInit();
	}

	private void scalingButtonQueryDelete_Click(object? sender, EventArgs e)
	{
		var idx = scalingComboBoxQueryList.SelectedIndex;
		if (idx == -1) return;

		var item = scalingComboBoxQueryList.SelectedItem;

		if (!this.Services.MessageBoxes.Confirm(string.Format(Resources.GlobalDeleteConfirm, item), Resources.GlobalInputWarning))
			return;


		scalingComboBoxQueryList.Items.RemoveAt(idx);
		if (item is SearchQuery query)
		{
			this.QueryFilters.Remove(query);
			this.SearchQueryService.Delete(query);
		}
	}
}
