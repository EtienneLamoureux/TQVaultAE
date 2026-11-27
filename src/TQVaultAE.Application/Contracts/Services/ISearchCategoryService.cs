using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Builds the value lists of the advanced search categories from the searchable items.
/// </summary>
public interface ISearchCategoryService
{
	/// <summary>
	/// Builds the values of <paramref name="category"/>: the value of each item is taken, cleaned of its TQ tags and
	/// trimmed; empty values are dropped; the items are grouped by value and the values sorted (current culture).
	/// </summary>
	/// <param name="category">The category to build.</param>
	/// <param name="items">The searchable items, already loaded (<see cref="SearchResult.LazyLoad"/>).</param>
	/// <returns>The values in display order, each one with its items.</returns>
	IReadOnlyList<SearchCategoryEntry> BuildCategory(SearchCategory category, IEnumerable<SearchResult> items);
}
