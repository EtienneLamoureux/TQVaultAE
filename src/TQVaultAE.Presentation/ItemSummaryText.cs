using System.Globalization;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Presentation;

/// <summary>
/// One line of an item summary: the item name prefixed by its count, and the first item of that name.
/// </summary>
/// <param name="Text">"N x name", the count inserted after the leading color tag of the name.</param>
/// <param name="First">The first item of that name, used for the line color.</param>
public sealed record ItemSummaryLine(string Text, ToFriendlyNameResult First);

/// <summary>
/// Texts shown for search results and bag contents.
/// </summary>
public static class ItemSummaryText
{
	/// <summary>
	/// Gets the label of a container type, as shown in the search results.
	/// </summary>
	/// <param name="sackType">The container type.</param>
	/// <param name="stashType">The stash type, for stashes.</param>
	/// <returns>The localized label, or "Unknown".</returns>
	public static string ContainerTypeLabel(SackType sackType, StashType? stashType)
		=> sackType switch
		{
			SackType.Vault => Resources.ResultsContainerVault,
			SackType.Player => Resources.ResultsContainerPlayer,
			SackType.Equipment => Resources.ResultsContainerEquip,
			SackType.Stash => stashType switch
			{
				StashType.TransferStash => Resources.GlobalTransferStash,
				StashType.RelicVaultStash => Resources.GlobalRelicVaultStash,
				_ => Resources.ResultsContainerStash
			},
			_ => "Unknown",
		};

	/// <summary>
	/// Gets the bag label of a search result ("Bag 3"), as shown in the search results.
	/// </summary>
	/// <param name="bagNumber">The 1-based bag number, or null where no bag applies.</param>
	/// <returns>The localized label, or an empty string without a bag number.</returns>
	public static string BagLabel(int? bagNumber)
		=> bagNumber is int number
			? string.Format(CultureInfo.CurrentCulture, Resources.ResultsBag, number)
			: string.Empty;

	/// <summary>
	/// Groups items by their bag tooltip name and counts them ("N x name"), sorted by the name without color tags.
	/// </summary>
	/// <param name="items">Friendly names of the items.</param>
	/// <returns>One line per distinct name.</returns>
	public static IReadOnlyList<ItemSummaryLine> SummarizeByName(IEnumerable<ToFriendlyNameResult> items)
		=> items
			.OrderBy(d => d.FullNameBagTooltipClean)
			.GroupBy(d => d.FullNameBagTooltip)
			.Select(g => new ItemSummaryLine(g.Key.InsertAfterColorPrefix($"{g.Count()} x "), g.First()))
			.ToList();
}
