using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Services;

/// <summary>
/// Service for managing the global searchable item database.
/// </summary>
public class ItemDatabaseService(
    ILogger<ItemDatabaseService> log,
    IItemTextFormatter itemText) : IItemDatabaseService
{
    private readonly ILogger<ItemDatabaseService> _log = log;
    private readonly IItemTextFormatter _itemText = itemText;

    /// <summary>
    /// The searchable items and, by reference, the items already in it. Replaced as a whole, so both always match.
    /// </summary>
    /// <remarks>
    /// Enumerating a <see cref="ConcurrentBag{T}"/> locks it and copies it: checking for duplicates that way made
    /// loading the saves quadratic, with every loading thread waiting on that lock.
    /// </remarks>
    private sealed record Database(ConcurrentBag<SearchResult> Items, ConcurrentDictionary<Item, byte> Indexed)
    {
        public static Database Of(IEnumerable<SearchResult> results)
        {
            var list = results.ToList();
            var indexed = new ConcurrentDictionary<Item, byte>(ReferenceEqualityComparer.Instance);
            foreach (var r in list)
                indexed.TryAdd(r.Item, 0);
            return new Database(new ConcurrentBag<SearchResult>(list), indexed);
        }
    }

    private Database _database = Database.Of([]);

    public ConcurrentBag<SearchResult> ItemDatabase => this._database.Items;

    /// <inheritdoc/>
    public bool TryAddItemToDatabase(Item item)
    {
        if (item is null)
            return false;

        // Already in the database (by reference)
        var database = this._database;
        if (!database.Indexed.TryAdd(item, 0))
            return false;

        database.Items.Add(this.NewResult(item));
        return true;
    }

    /// <summary>
    /// A search entry of an item: its names are read the first time they are needed, with the item's map.
    /// </summary>
    private SearchResult NewResult(Item item)
        => new(
            item,
            new Lazy<ToFriendlyNameResult>(
                () => this._itemText.GetFriendlyNames(item, FriendlyNamesExtraScopes.ItemFullDisplay),
                LazyThreadSafetyMode.ExecutionAndPublication
            )
        );

    /// <inheritdoc/>
    public void RefreshItems(IReadOnlyCollection<Item> items)
    {
        var refreshed = new HashSet<Item>(items, ReferenceEqualityComparer.Instance);
        var database = this.ItemDatabase;
        if (refreshed.Count == 0 || !database.Any(r => refreshed.Contains(r.Item)))
            return;

        // Indexed items only: a new entry each, whose names are read again.
        this._database = Database.Of(database.Select(r => refreshed.Contains(r.Item) ? this.NewResult(r.Item) : r));
    }

    /// <inheritdoc/>
    public void AddItemToDatabase(Item item, string containerPath, string containerName, int sackNumber, SackType sackType, StashType? stashType = null)
    {
        if (item is null)
            return;

        item.Place.Path = containerPath;
        item.Place.Name = containerName;
        item.Place.SackNumber = sackNumber;
        item.Place.SackType = sackType;
        item.Place.StashType = stashType;

#if DEBUG
        // Every item a container read has a map (MODLINK.md): one without is a path that forgot to give it one.
        if (item.ModOrigin == ModOrigin.None)
            this._log.LogWarning("Item without a map: {Item} in {Container}", item.BaseItemId, containerPath);
#endif

        this.TryAddItemToDatabase(item);
    }

    /// <inheritdoc/>
    public void RemoveItemFromDatabase(Item item)
    {
        if (item is null)
            return;

        this._database = Database.Of(this.ItemDatabase.Where(r => r.Item != item));
    }

    /// <inheritdoc/>
    public void RemoveContainerFromDatabase(string containerPath)
    {
        if (string.IsNullOrEmpty(containerPath))
            return;

        var items = this.ItemDatabase;
        if (items.Any(r => string.Equals(r.Item.Place.Path, containerPath, StringComparison.OrdinalIgnoreCase)))
            this._database = Database.Of(items.Where(r => !string.Equals(r.Item.Place.Path, containerPath, StringComparison.OrdinalIgnoreCase)));
    }

    /// <inheritdoc/>
    public void AddLoadoutsToDatabase(LoadoutSet loadouts)
    {
        ArgumentNullException.ThrowIfNull(loadouts);

        // The loadouts file is their container: a character's reload (its player file) leaves them, and theirs leaves
        // the character's items.
        this.RemoveContainerFromDatabase(loadouts.FilePath);
        for (int index = 0; index < loadouts.Loadouts.Count; index++)
        {
            var loadout = loadouts.Loadouts[index];
            foreach (var item in loadout.Items)
            {
                item.Place.Loadout = loadout.Name;
                this.AddItemToDatabase(item, loadouts.FilePath, loadouts.CharacterName, index, SackType.Loadout);
            }
        }
    }

    /// <inheritdoc/>
    public void IndexVault(PlayerCollection vault, string containerPath, string containerName)
        => this.IndexSacks(vault, containerPath, containerName, SackType.Vault);

    /// <inheritdoc/>
    public void IndexPlayer(PlayerCollection player, string containerPath, string containerName)
    {
        this.IndexSacks(player, containerPath, containerName, SackType.Player);

        // The equipment is numbered by its stash panel tab, like the stashes (SearchResultNavigation).
        foreach (var item in player.EquipmentSack ?? [])
            this.AddItemToDatabase(item, containerPath, containerName, BagIdConstants.BAGID_EQUIPMENTPANEL, SackType.Equipment);
    }

    /// <inheritdoc/>
    public void IndexStash(Stash stash, string containerPath, string containerName, StashType stashType)
    {
        // A stash type is its stash panel tab (StashType values are the bag ids).
        foreach (var item in stash.Sack ?? [])
            this.AddItemToDatabase(item, containerPath, containerName, (int)stashType, SackType.Stash, stashType);
    }

    /// <summary>
    /// Adds the items of every sack of a vault or a character, each with its sack number (its index).
    /// </summary>
    private void IndexSacks(PlayerCollection container, string containerPath, string containerName, SackType sackType)
    {
        int sackNumber = -1;
        foreach (var sack in container)
        {
            sackNumber++;
            if (sack is null)
                continue;

            foreach (var item in sack)
                this.AddItemToDatabase(item, containerPath, containerName, sackNumber, sackType);
        }
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<RecordId, int> CountItemsByRecord(IEnumerable<RecordId> recordIds)
    {
        var counts = new Dictionary<RecordId, int>();
        foreach (var id in recordIds)
            counts.TryAdd(id, 0);

        if (counts.Count == 0)
            return counts;

        foreach (var result in this.ItemDatabase)
        {
            // A loadout holds copies of pieces a character wears or wore: they are not more items owned.
            if (result.SackType == SackType.Loadout)
                continue;

            if (result.Item.BaseItemId is { } id && counts.TryGetValue(id, out var count))
                counts[id] = count + 1;
        }

        return counts;
    }

    #region Search Methods

    /// <inheritdoc/>
    public IReadOnlyList<SearchResult> FullTextSearch(string searchText, bool isRegex = false)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return Array.Empty<SearchResult>();

        var itemDatabase = this.ItemDatabase;
        if (itemDatabase is null || itemDatabase.IsEmpty)
            return Array.Empty<SearchResult>();

        return this.ApplyTextSearch(itemDatabase, searchText, isRegex).ToList();
    }

    /// <inheritdoc/>
    public IReadOnlyList<SearchResult> FindCopies(Item source, CopySearchKind kind)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!ItemCopySearch.IsAvailable(source, kind))
            return Array.Empty<SearchResult>();

        var matches = ItemCopySearch.Matcher(source, kind);
        var results = this.ItemDatabase.ToArray();
        var sourceResult = results.FirstOrDefault(r => ReferenceEquals(r.Item, source)) ?? this.NewResult(source);

        var copies = results
            .Where(r => !ReferenceEquals(r.Item, source) && matches(r.Item))
            .Where(this.TryLoad)
            .OrderBy(r => r.ContainerName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(r => r.SackType)
            .ThenBy(r => r.SackNumber)
            .ThenBy(r => r.LoadoutName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(r => r.Item.PositionY)
            .ThenBy(r => r.Item.PositionX);

        // The source is listed even without a name, so the window always shows what was searched from.
        this.TryLoad(sourceResult);
        return [sourceResult, .. copies];
    }

    /// <summary>
    /// Loads the display data of a result; false for a broken one or one without a name (logged).
    /// </summary>
    private bool TryLoad(SearchResult result)
    {
        try
        {
            result.LazyLoad();
            return !string.IsNullOrWhiteSpace(result.ItemName);
        }
        catch (Exception ex)
        {
            this._log.LogWarning(ex, "Copy search: {Item} could not be loaded", result.Item.BaseItemId);
            return false;
        }
    }

    private void ResetAllItemDatabase(List<SearchResult> validItems)
    {
	    this._database = Database.Of(validItems);
	}

	private readonly object _warmUpLock = new();
	private Task? _warmUp;

	/// <inheritdoc/>
	public Task WarmUpAsync()
	{
		lock (this._warmUpLock)
		{
			if (this._warmUp is { IsCompleted: false })
				return this._warmUp;

			var items = this.ItemDatabase.ToArray();
			// LongRunning gives a dedicated thread, so its priority can be lowered without touching the thread pool.
			this._warmUp = Task.Factory.StartNew(() => this.WarmUp(items), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
			return this._warmUp;
		}
	}

	private void WarmUp(SearchResult[] items)
	{
		Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
		var watch = System.Diagnostics.Stopwatch.StartNew();
		long allocated = GC.GetTotalAllocatedBytes();
		int collections = GC.CollectionCount(0);

		// Half the cores, so the main window stays responsive while this runs.
		var options = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2) };
		int failed = this.LoadAll(items, options, "Search warm-up", null);

		// Allocations and collections cover the whole process while this runs, as other startup work goes on too.
		this._log.LogInformation("Search data warmed up in {Elapsed} ms ({Count} items, {Failed} failed, {Allocated} MB allocated, {Collections} gen0 collections)"
			, watch.ElapsedMilliseconds, items.Length, failed, (GC.GetTotalAllocatedBytes() - allocated) >> 20, GC.CollectionCount(0) - collections);
	}

	/// <inheritdoc/>
	public void PrepareForSearch(IProgress<int>? progress = null)
	{
		var items = this.ItemDatabase.ToArray();
		int processed = 0;

		// Report in batches: one UI message per item floods the message loop on large vaults.
		int batch = Math.Max(1, items.Length / 200);
		int failed = this.LoadAll(items, new ParallelOptions(), "Search data", () =>
		{
			if (Interlocked.Increment(ref processed) % batch == 0)
				progress?.Report(batch);
		});
		if (processed % batch != 0)
			progress?.Report(processed % batch);

		// Drop the items without a name (broken ones included): ConcurrentBag has no RemoveAll, so the database is rebuilt.
		var validItems = items.Where(r => !string.IsNullOrWhiteSpace(r.ItemName)).ToList();
		this.ResetAllItemDatabase(validItems);

		if (failed != 0)
			this._log.LogWarning("Search data: {Failed} of {Count} items could not be loaded and are left out", failed, items.Length);
	}

	/// <summary>
	/// Loads the display data of <paramref name="items"/> in parallel. A broken item does not stop the others: the first
	/// failure is logged and the failures counted.
	/// </summary>
	/// <param name="onItemDone">Called after each item, loaded or not.</param>
	/// <returns>The number of items that could not be loaded.</returns>
	private int LoadAll(SearchResult[] items, ParallelOptions options, string what, Action? onItemDone)
	{
		int failed = 0;
		Parallel.ForEach(items, options, item =>
		{
			try
			{
				item.LazyLoad();
			}
			catch (Exception ex)
			{
				// The search dialog drops items without a name; one broken item must not stop the others.
				if (Interlocked.Increment(ref failed) == 1)
					this._log.LogWarning(ex, "{What} failed for {Item}", what, item.Item?.BaseItemId);
			}
			onItemDone?.Invoke();
		});
		return failed;
	}

	/// <inheritdoc/>
	public IReadOnlyList<SearchResult> ExecuteAdvancedSearch(AdvancedSearchRequest request)
    {
        if (request.InitialResults is null)
            return Array.Empty<SearchResult>();

        var flags = new ItemFlagFilter(request.HasPrefix, request.HasSuffix, request.HasRelic, request.HasCharm, request.IsSetItem, request.HasSkill);
        var bounds = new RequirementBounds(
            request.MinLevel, request.MaxLevel,
            request.MinStrength, request.MaxStrength,
            request.MinDexterity, request.MaxDexterity,
            request.MinIntelligence, request.MaxIntelligence);

        // No filter reads no item's names.
        var results = flags.IsEmpty && bounds.IsEmpty
            ? request.InitialResults.ToList()
            : request.InitialResults
                .Where(r => flags.Matches(r.Item, r.FriendlyNames, this._itemText.HasGrantedSkill)
                    && bounds.Matches(r.FriendlyNames?.RequirementInfo))
                .ToList();

        this._log.LogDebug("Advanced search executed: {Count} results found", results.Count);
        return results;
    }

    /// <inheritdoc/>
    public IReadOnlyList<SearchResult> CombineFilterResults(IEnumerable<IEnumerable<SearchResult>> filterResults, SearchOperator logic)
    {
        if (logic == SearchOperator.Or)
            return filterResults.SelectMany(f => f).Distinct().ToList();

        // And: the item must be in every filter
        HashSet<SearchResult>? resultSet = null;
        foreach (var filter in filterResults)
        {
            if (resultSet is null)
                resultSet = new HashSet<SearchResult>(filter);
            else
                resultSet.IntersectWith(filter);
        }

        return resultSet is null ? Array.Empty<SearchResult>() : resultSet.ToList();
    }

    /// <inheritdoc/>
    public IReadOnlyList<T> KeepEntriesMatching<T>(IEnumerable<T> entries, Func<T, IEnumerable<SearchResult>> resultsOf, IEnumerable<SearchResult> results)
    {
        var resultSet = results as IReadOnlySet<SearchResult> ?? new HashSet<SearchResult>(results);
        if (resultSet.Count == 0)
            return Array.Empty<T>();

        return entries.Where(e => resultsOf(e).Any(resultSet.Contains)).ToList();
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Applies text search to results.
    /// </summary>
    private IEnumerable<SearchResult> ApplyTextSearch(IEnumerable<SearchResult> results, string searchText, bool isRegex)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return results;

        if (isRegex)
        {
            try
            {
                var regex = new Regex(searchText, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                return results.Where(r => r.FriendlyNames is not null &&
                    regex.IsMatch(r.FriendlyNames.FullNameClean ?? string.Empty));
            }
            catch (ArgumentException)
            {
                // Invalid regex, fall back to contains
                return results.Where(r => r.FriendlyNames is not null &&
                    (r.FriendlyNames.FullNameClean ?? string.Empty).Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }
        }
        else
        {
            return results.Where(r => r.FriendlyNames is not null &&
                r.FriendlyNames.FulltextIsMatchIndexOf(searchText));
        }
    }

    #endregion
}
