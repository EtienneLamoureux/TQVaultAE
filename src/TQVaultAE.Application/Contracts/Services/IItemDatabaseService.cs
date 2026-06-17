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
    /// Clears all items from the search database.
    /// </summary>
    void ClearItemDatabase();

    /// <summary>
    /// Populates the item database from all loaded containers.
    /// </summary>
    void RebuildItemDatabase();

    #region Search Methods

    /// <summary>
    /// Executes advanced search filtering on initial results.
    /// </summary>
    /// <param name="request">The advanced search request with filter criteria.</param>
    /// <returns>Filtered search results.</returns>
    IReadOnlyList<SearchResult> ExecuteAdvancedSearch(AdvancedSearchRequest request);

    /// <summary>
    /// Performs a full-text search on items.
    /// </summary>
    /// <param name="searchText">The search text.</param>
    /// <param name="isRegex">Whether the search text is a regex pattern.</param>
    /// <returns>Matching results.</returns>
    IReadOnlyList<SearchResult> FullTextSearch(string searchText, bool isRegex = false);

    #endregion

    void ResetAllItemDatabase(List<SearchResult> validItems);
}
