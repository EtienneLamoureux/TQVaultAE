using System.Collections.Generic;
using System.Drawing;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Service for managing item highlight search functionality.
/// </summary>
public interface IHighlightService
{
    /// <summary>
    /// Gets or sets the background color for item highlight.
    /// </summary>
    Color HighlightSearchItemColor { get; set; }

    /// <summary>
    /// Gets or sets the border color for item highlight.
    /// </summary>
    Color HighlightSearchItemBorderColor { get; set; }

    /// <summary>
    /// Gets or sets the highlight search string.
    /// </summary>
    string HighlightSearch { get; set; }

    /// <summary>
    /// Gets or sets the highlight search filters.
    /// </summary>
    HighlightFilterValues HighlightFilter { get; set; }

    /// <summary>
    /// Gets the list of items to highlight.
    /// </summary>
    IReadOnlyList<Item> HighlightedItems { get; }

    /// <summary>
    /// Finds items matching the highlight criteria.
    /// </summary>
    void FindHighlight();

    /// <summary>
    /// Resets the highlight search.
    /// </summary>
    void ResetHighlight();
}
