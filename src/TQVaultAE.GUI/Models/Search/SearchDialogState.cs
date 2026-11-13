using TQVaultAE.Application.DTOs;

namespace TQVaultAE.GUI.Models.Search;

/// <summary>Where a category list was left: its focus entry and the first column shown.</summary>
/// <param name="FocusedIndex">Entry with the keyboard focus, or a negative value for none.</param>
/// <param name="FirstColumn">First column shown (horizontal scroll).</param>
public sealed record SearchListPosition(int FocusedIndex, int FirstColumn);

/// <summary>
/// Everything the user set in the search dialog when it closed. The dialog is kept between openings, but made again
/// after a zoom change: the new one is given this state and the user finds the dialog as they left it.
/// </summary>
/// <remarks>
/// The filters and settings are the <see cref="Query"/>, as a saved query keeps them, and are restored the same way;
/// the rest is what a saved query leaves out. Lengths are at zoom 1, so they follow the new zoom.
/// </remarks>
public sealed class SearchDialogState
{
	/// <summary>Selected filters (search term included) and dialog settings.</summary>
	public required SearchQuery Query { get; init; }

	/// <summary>Item database the dialog was built from, and its size then: the filters refer to these items.</summary>
	public required object Database { get; init; }
	public required int DatabaseCount { get; init; }

	/// <summary>Text of the search box, also when typed too recently to be applied yet.</summary>
	public required string SearchText { get; init; }

	/// <summary>Text of the saved query box, and the saved query selected in it, if any.</summary>
	public required string QueryText { get; init; }
	public string? SelectedQueryName { get; init; }

	/// <summary>Names of the category panels, in the order shown (hiding and showing categories moves them).</summary>
	public required IReadOnlyList<string> CategoryOrder { get; init; }

	/// <summary>Names of the category lists collapsed (category label click, Collapse All).</summary>
	public required IReadOnlySet<string> CollapsedLists { get; init; }

	/// <summary>Position of each category list, by name.</summary>
	public required IReadOnlyDictionary<string, SearchListPosition> Lists { get; init; }

	/// <summary>Scroll position of the categories area, at zoom 1.</summary>
	public PointF Scroll { get; init; }

	/// <summary>Size the user gave the dialog, at zoom 1; <see langword="null"/> when not resized by the user.</summary>
	public SizeF? UserSize { get; init; }

	/// <summary>The dialog was maximized.</summary>
	public bool Maximized { get; init; }
}
