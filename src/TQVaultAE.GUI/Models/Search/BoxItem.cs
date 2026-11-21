using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Components.Scaling;

namespace TQVaultAE.GUI.Models.Search;

/// <summary>
/// A filter shown in the search dialog: a category value in its list, or the search term (no list).
/// </summary>
/// <remarks>
/// Not saved: a saved query keeps its <see cref="SearchFilter"/> (<see cref="ToFilter"/>).
/// </remarks>
public class BoxItem
{
	/// <summary>List showing the value; <see langword="null"/> for the search term.</summary>
	public VirtualCheckedListBox? CheckedList { get; set; }

	/// <summary>Label of the category.</summary>
	public ScalingLabel? Category { get; set; }

	public IEnumerable<SearchResult> MatchingResults { get; set; } = [];

	public string CheckedListName => this.CheckedList?.Name ?? string.Empty;
	public string CategoryName => this.Category?.Name ?? string.Empty;
	public string DisplayValue { get; set; } = string.Empty;

	/// <summary>The filter as saved in a search query.</summary>
	public SearchFilter ToFilter()
		=> new(this.CheckedListName, this.CategoryName, this.DisplayValue);

	public override string ToString()
		=> this.DisplayValue;
}