using TQVaultAE.Application.DTOs;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Outcome of the checks made before saving a search query.
/// </summary>
public enum SearchQuerySaveCheck
{
	/// <summary>The query can be saved as a new one.</summary>
	Valid,
	/// <summary>The query has no name.</summary>
	NameMissing,
	/// <summary>The query filters nothing (<see cref="SearchQuery.HasFilters"/>).</summary>
	NoFilter,
	/// <summary>A query of that name, ignoring case, exists: saving replaces it.</summary>
	NameExists,
}

/// <summary>
/// The search queries saved by the user, kept in <c>SearchQueries.json</c> in the TQVault configuration folder.
/// </summary>
public interface ISearchQueryService
{
	/// <summary>The saved queries, in display order. Read from the file on first use.</summary>
	IReadOnlyList<SearchQuery> Queries { get; }

	/// <summary>
	/// Checks <paramref name="draft"/> before it is saved under <paramref name="name"/>: the name is required, the
	/// query must filter something, and a name already used (ignoring case) must be confirmed.
	/// </summary>
	SearchQuerySaveCheck CheckSave(string name, SearchQuery draft);

	/// <summary>
	/// Saves <paramref name="draft"/> under <paramref name="name"/> and writes the file. A query of the same name
	/// (ignoring case) is replaced at its place; a new one is added and the list sorted by name.
	/// </summary>
	/// <returns><paramref name="draft"/>, now in <see cref="Queries"/>; null when the file could not be read, left as it
	/// is (saving would replace every query it holds).</returns>
	SearchQuery? Save(string name, SearchQuery draft);

	/// <summary>Removes <paramref name="query"/> and writes the file.</summary>
	void Delete(SearchQuery query);

	/// <summary>
	/// Finds the live filters of <paramref name="query"/>, in its order: each saved filter is matched by list, category
	/// and value; the search term is built again. Filters whose value no longer exists (the items carrying it are gone)
	/// are left out.
	/// </summary>
	/// <param name="query">The saved query.</param>
	/// <param name="liveFilters">The values of the categories, by filter.</param>
	/// <param name="searchTermCategory">The <see cref="SearchFilter.CategoryName"/> of the search term.</param>
	/// <param name="makeSearchTerm">Builds the search term filter from its text.</param>
	IReadOnlyList<T> AttachFilters<T>(SearchQuery query, IReadOnlyDictionary<SearchFilter, T> liveFilters, string searchTermCategory, Func<string, T> makeSearchTerm);
}
