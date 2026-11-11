using System.Collections.Concurrent;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Service for managing the global searchable item database.
/// </summary>
public interface IItemDatabaseService
{
    /// <summary>
    /// Gets the item database.
    /// </summary>
    ConcurrentBag<SearchResult> ItemDatabase { get; }

    /// <summary>
    /// Adds an item to the search database if it's not already present.
    /// </summary>
    bool TryAddItemToDatabase(Item item);

    /// <summary>
    /// Adds an item to the search database with its container information.
    /// </summary>
    void AddItemToDatabase(Item item, string containerPath, string containerName, int sackNumber, SackType sackType, StashType? stashType = null);

    /// <summary>
    /// Removes an item from the search database.
    /// </summary>
    void RemoveItemFromDatabase(Item item);

    /// <summary>
    /// Makes the search entries of items read again with another map: their names are read again when next needed.
    /// Items not in the search database are left out.
    /// </summary>
    void RefreshItems(IReadOnlyCollection<Item> items);

    /// <summary>
    /// Removes every item of a container file (a deleted or renamed vault) from the search database,
    /// so searches no longer find items in a container that is gone.
    /// </summary>
    /// <param name="containerPath">The container file, as given to <see cref="AddItemToDatabase"/>.</param>
    void RemoveContainerFromDatabase(string containerPath);

    /// <summary>
    /// Clears all items from the search database.
    /// </summary>
    void ClearItemDatabase();

    /// <summary>
    /// Populates the item database from all loaded containers.
    /// </summary>
    void RebuildItemDatabase();

    /// <summary>
    /// Counts the loaded items made from each of <paramref name="recordIds"/> (their base item),
    /// e.g. how many of each piece of a set are owned. Records with no loaded item count 0.
    /// </summary>
    IReadOnlyDictionary<RecordId, int> CountItemsByRecord(IEnumerable<RecordId> recordIds);

    #region Search Methods

    /// <summary>
    /// Executes advanced search filtering on initial results.
    /// </summary>
    /// <param name="request">The advanced search request with filter criteria.</param>
    /// <returns>Filtered search results.</returns>
    IReadOnlyList<SearchResult> ExecuteAdvancedSearch(AdvancedSearchRequest request);

    /// <summary>
    /// Combines the result sets of the selected search filters: with <see cref="SearchOperator.And"/> an item must be
    /// in every set, with <see cref="SearchOperator.Or"/> in at least one. Each item is returned once.
    /// </summary>
    /// <param name="filterResults">The results of each selected filter. No filter gives no result.</param>
    /// <param name="logic">How the sets are combined.</param>
    IReadOnlyList<SearchResult> CombineFilterResults(IEnumerable<IEnumerable<SearchResult>> filterResults, SearchOperator logic);

    /// <summary>
    /// Keeps the search category entries that match at least one of <paramref name="results"/>, in their order.
    /// Used to reduce the categories to what the current results still offer.
    /// </summary>
    /// <param name="entries">The category entries.</param>
    /// <param name="resultsOf">The items each entry matches.</param>
    /// <param name="results">The current search results.</param>
    IReadOnlyList<T> KeepEntriesMatching<T>(IEnumerable<T> entries, Func<T, IEnumerable<SearchResult>> resultsOf, IEnumerable<SearchResult> results);

    /// <summary>
    /// Performs a full-text search on items.
    /// </summary>
    /// <param name="searchText">The search text.</param>
    /// <param name="isRegex">Whether the search text is a regex pattern.</param>
    /// <returns>Matching results.</returns>
    IReadOnlyList<SearchResult> FullTextSearch(string searchText, bool isRegex = false);

    #endregion

    void ResetAllItemDatabase(List<SearchResult> validItems);

    /// <summary>
    /// Computes, in the background and at low priority, the display data of every item in the database
    /// (<see cref="SearchResult.LazyLoad"/>), the slow part of opening the search dialog, so it is ready when the
    /// dialog opens. Calling it again while it runs returns the running task.
    /// </summary>
    Task WarmUpAsync();

    /// <summary>
    /// Loads the display data of every item in the database (the part of <see cref="WarmUpAsync"/> not done yet), at
    /// full speed, then drops the items without a name, broken ones included. Run by the search dialog before it builds
    /// its categories; it blocks, so call it off the UI thread.
    /// </summary>
    /// <param name="progress">Receives the number of items loaded since the last report; the reports add up to the item count.</param>
    void PrepareForSearch(IProgress<int>? progress = null);
}
