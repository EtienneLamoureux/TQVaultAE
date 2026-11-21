using System.Text.Json;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;

namespace TQVaultAE.Services;

/// <summary>
/// The search queries saved by the user, kept in <c>SearchQueries.json</c>.
/// </summary>
public class SearchQueryService(
	ILogger<SearchQueryService> log,
	IGamePathService gamePathService,
	IFileIO fileIO,
	IPathIO pathIO) : ISearchQueryService
{
	/// <summary>Name of the file in the TQVault configuration folder.</summary>
	public const string FileName = "SearchQueries.json";

	/// <summary>The format written by earlier versions: indented, PascalCase names, read ignoring case.</summary>
	private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };

	private readonly ILogger<SearchQueryService> _log = log;
	private readonly IGamePathService _gamePathService = gamePathService;
	private readonly IFileIO _fileIO = fileIO;
	private readonly IPathIO _pathIO = pathIO;
	private List<SearchQuery>? _queries;

	private string FilePath => this._pathIO.Combine(this._gamePathService.TQVaultConfigFolder, FileName);

	/// <inheritdoc/>
	public IReadOnlyList<SearchQuery> Queries => this.Loaded ?? [];

	// Null while the file cannot be read: nothing is kept, the next operation reads it again (a lock may be over), and
	// nothing is written meanwhile. Each operation reads it once.
	private List<SearchQuery>? Loaded => this._queries ?? this.Read();

	/// <inheritdoc/>
	public SearchQuerySaveCheck CheckSave(string name, SearchQuery draft)
	{
		if (string.IsNullOrWhiteSpace(name))
			return SearchQuerySaveCheck.NameMissing;

		if (!draft.HasFilters())
			return SearchQuerySaveCheck.NoFilter;

		return IndexOf(this.Loaded ?? [], name.Trim()) == -1 ? SearchQuerySaveCheck.Valid : SearchQuerySaveCheck.NameExists;
	}

	/// <inheritdoc/>
	public SearchQuery? Save(string name, SearchQuery draft)
	{
		if (this.Loaded is not { } queries)
		{
			this._log.LogError("The saved search queries file could not be read: not overwritten");
			return null;
		}

		name = name.Trim();
		draft.QueryName = name;
		int index = IndexOf(queries, name);
		if (index == -1)
		{
			queries.Add(draft);
			// Stable sort: queries named alike keep their order.
			var sorted = queries.OrderBy(q => q.QueryName).ToList();
			queries.Clear();
			queries.AddRange(sorted);
		}
		else
		{
			queries[index] = draft;
		}

		this.Write(queries);
		return draft;
	}

	/// <inheritdoc/>
	public void Delete(SearchQuery query)
	{
		if (this.Loaded is { } queries && queries.Remove(query))
			this.Write(queries);
	}

	/// <inheritdoc/>
	public IReadOnlyList<T> AttachFilters<T>(SearchQuery query, IReadOnlyDictionary<SearchFilter, T> liveFilters, string searchTermCategory, Func<string, T> makeSearchTerm)
	{
		var found = new List<T>(query.CheckedItems.Count);
		foreach (var filter in query.CheckedItems)
		{
			if (filter.CategoryName == searchTermCategory)
				found.Add(makeSearchTerm(filter.DisplayValue));
			else if (liveFilters.TryGetValue(filter, out var live))
				found.Add(live);
		}

		return found;
	}

	private static int IndexOf(List<SearchQuery> queries, string name)
		=> queries.FindIndex(q => q.QueryName.Equals(name, StringComparison.OrdinalIgnoreCase));

	private List<SearchQuery>? Read()
	{
		// A file that cannot be read is left as it is: saving now would replace every query it holds.
		var (queries, failed) = JsonFile.Read<List<SearchQuery>>(this._fileIO, this.FilePath, JsonOptions, this._log, "saved search queries");
		if (failed)
			return null;

		queries ??= [];

		// Missing members are left null by the deserializer.
		queries.RemoveAll(q => q is null);
		foreach (var query in queries)
		{
			query.QueryName ??= string.Empty;
			query.Filter ??= string.Empty;
			query.CheckedItems = query.CheckedItems?.Where(f => f is not null).ToList() ?? [];
			query.Visible = query.Visible?.Where(v => v is not null).ToList() ?? [];
		}
		return this._queries = queries;
	}

	private void Write(List<SearchQuery> queries)
		=> JsonFile.Write(this._fileIO, this.FilePath, queries, JsonOptions);
}
