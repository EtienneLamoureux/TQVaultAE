using System.Globalization;
using TQVaultAE.Application.Search;

namespace TQVaultAE.Presentation;

/// <summary>
/// Texts of the item menu's <i>Search</i> (<see cref="ItemCopySearch"/>): its entries and the results window's title.
/// </summary>
public static class ItemCopySearchText
{
	/// <summary>The menu entry of a search.</summary>
	/// <param name="kind">The search.</param>
	public static string MenuLabel(CopySearchKind kind) => kind switch
	{
		CopySearchKind.SameSeed => Resources.SackPanelMenuSearchSameSeed,
		CopySearchKind.AllCopies => Resources.SackPanelMenuSearchAllCopies,
		CopySearchKind.SameBase => Resources.SackPanelMenuSearchSameBase,
		CopySearchKind.SamePrefix => Resources.SackPanelMenuSearchSamePrefix,
		CopySearchKind.SameSuffix => Resources.SackPanelMenuSearchSameSuffix,
		_ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
	};

	/// <summary>
	/// The results window's title: the search and the item searched from ("All copies of Ring (2 found)"), with the
	/// number of items found besides it.
	/// </summary>
	/// <param name="kind">The search.</param>
	/// <param name="itemName">The name of the item searched from.</param>
	/// <param name="found">The number of items found, the item searched from left out.</param>
	public static string Title(CopySearchKind kind, string itemName, int found)
	{
		string format = kind switch
		{
			CopySearchKind.SameSeed => Resources.SearchCopiesSameSeed,
			CopySearchKind.AllCopies => Resources.SearchCopiesAllCopies,
			CopySearchKind.SameBase => Resources.SearchCopiesSameBase,
			CopySearchKind.SamePrefix => Resources.SearchCopiesSamePrefix,
			CopySearchKind.SameSuffix => Resources.SearchCopiesSameSuffix,
			_ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
		};
		string search = string.Format(CultureInfo.CurrentCulture, format, itemName);
		return string.Format(CultureInfo.CurrentCulture, Resources.SearchCopiesCount, search, found);
	}
}
