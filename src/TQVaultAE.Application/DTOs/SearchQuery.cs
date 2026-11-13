namespace TQVaultAE.Application.DTOs;

/// <summary>
/// A filter of a saved search query: a value of a search category, or the search term.
/// </summary>
/// <param name="CheckedListName">Name of the list holding the value; empty for the search term.</param>
/// <param name="CategoryName">Name of the category label of the value.</param>
/// <param name="DisplayValue">The value, or the search term.</param>
public sealed record SearchFilter(string CheckedListName, string CategoryName, string DisplayValue);

/// <summary>
/// Whether a search category is shown.
/// </summary>
/// <param name="Name">Name of the category button.</param>
/// <param name="Visible"><see langword="true"/> when the category is shown.</param>
public sealed record SearchCategoryVisibility(string Name, bool Visible);

/// <summary>
/// A search query saved by the user in <c>SearchQueries.json</c>: the selected filters and the dialog settings.
/// </summary>
/// <remarks>
/// The property names and order are those of the file written by earlier versions: do not rename them.
/// </remarks>
public class SearchQuery
{
	/// <summary>The selected filters, the search term included.</summary>
	public List<SearchFilter> CheckedItems { get; set; } = [];
	public string QueryName { get; set; } = string.Empty;
	/// <summary>Text filtering the category values.</summary>
	public string Filter { get; set; } = string.Empty;
	/// <summary>Which categories are shown.</summary>
	public List<SearchCategoryVisibility> Visible { get; set; } = [];
	/// <summary>Values shown per category.</summary>
	public decimal MaxElement { get; set; }
	/// <summary>Reduce the categories to the values of the current results.</summary>
	public bool Reduce { get; set; }
	public SearchOperator Logic { get; set; }

	public int MaxStr { get; set; }
	public int MaxDex { get; set; }
	public int MaxInt { get; set; }
	public int MinStr { get; set; }
	public int MinDex { get; set; }
	public int MinInt { get; set; }
	public bool MinRequirement { get; set; }
	public bool MaxRequirement { get; set; }
	public int MaxLvl { get; set; }
	public int MinLvl { get; set; }
	public bool HavingPrefix { get; set; }
	public bool HavingSuffix { get; set; }
	public bool HavingRelic { get; set; }
	public bool HavingCharm { get; set; }
	public bool IsSetItem { get; set; }
	public bool HavingSkill { get; set; }

	/// <summary>
	/// The query filters something: a filter is selected, a quick filter ticked, or a requirement bound set and enabled.
	/// </summary>
	public bool HasFilters()
		=> this.CheckedItems.Count != 0
			|| (this.MinRequirement && (this.MinLvl != 0 || this.MinStr != 0 || this.MinDex != 0 || this.MinInt != 0))
			|| (this.MaxRequirement && (this.MaxLvl != 0 || this.MaxStr != 0 || this.MaxDex != 0 || this.MaxInt != 0))
			|| this.HavingCharm
			|| this.HavingRelic
			|| this.HavingPrefix
			|| this.HavingSuffix
			|| this.IsSetItem
			|| this.HavingSkill;

	/// <summary>
	/// A copy keeping only the dialog settings (shown categories, category filter, values per category, reduce, operator):
	/// no selected filter, search term, quick filter or requirement. The search dialog keeps this much when the items it
	/// was built from changed, since its filters referred to the previous items.
	/// </summary>
	public SearchQuery SettingsOnly()
		=> new()
		{
			Filter = this.Filter,
			Visible = [.. this.Visible],
			MaxElement = this.MaxElement,
			Reduce = this.Reduce,
			Logic = this.Logic,
		};

	public override string ToString()
		=> this.QueryName;
}
