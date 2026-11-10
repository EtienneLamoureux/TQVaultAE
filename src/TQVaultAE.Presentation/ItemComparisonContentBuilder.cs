using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Comparison;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Presentation;

/// <summary>
/// The four columns of the item comparison, left to right.
/// </summary>
/// <param name="FirstDifferences">The first item's stats minus the second's.</param>
/// <param name="First">The classic tooltip of the first item.</param>
/// <param name="Second">The classic tooltip of the second item.</param>
/// <param name="SecondDifferences">The second item's stats minus the first's.</param>
public sealed record ItemComparisonContent(
	IReadOnlyList<TooltipLine> FirstDifferences
	, IReadOnlyList<TooltipLine> First
	, IReadOnlyList<TooltipLine> Second
	, IReadOnlyList<TooltipLine> SecondDifferences);

/// <summary>
/// Decides what the item comparison shows: each item's classic tooltip, built by <see cref="ItemTooltipContentBuilder"/>,
/// and the difference columns in their colors. The GUI only draws the lines.
/// </summary>
public class ItemComparisonContentBuilder(ITranslationService translationService)
{
	/// <summary>Color of a difference that is better for the item of the column.</summary>
	public static readonly TQColor BetterColor = TQColor.Green;

	/// <summary>Color of a difference that is worse for the item of the column.</summary>
	public static readonly TQColor WorseColor = TQColor.Red;

	/// <summary>Color of a difference with numbers going both ways.</summary>
	public static readonly TQColor MixedColor = TQColor.Yellow;

	private readonly ItemTooltipContentBuilder _tooltip = new(translationService);

	/// <summary>Builds the four columns of <paramref name="comparison"/>.</summary>
	public ItemComparisonContent Build(ItemComparison comparison)
		=> new(
			DifferenceLines(comparison.Stats.FirstMinusSecond)
			, this._tooltip.Build(comparison.First, ItemTooltipView.Classic).Lines
			, this._tooltip.Build(comparison.Second, ItemTooltipView.Classic).Lines
			, DifferenceLines(comparison.Stats.SecondMinusFirst));

	/// <summary>
	/// The lines of a difference column: the groups separated by a delimiter, a granted skill under its name, each
	/// difference colored better or worse for the column's item; a single line when nothing differs.
	/// </summary>
	public static IReadOnlyList<TooltipLine> DifferenceLines(IReadOnlyList<StatDifferenceGroup> groups)
	{
		var lines = new List<TooltipLine>();
		foreach (var group in groups)
		{
			if (lines.Count != 0)
				lines.Add(TooltipLine.Delimiter);

			if (group.Title is not null)
				lines.Add(TooltipLine.Of(group.Title, ItemStyle.Epic.TQColor(), bold: true));

			lines.AddRange(group.Lines.Select(line => TooltipLine.Of(line.Text, ColorOf(line.Trend, group.LowerIsBetter))));
		}

		if (lines.Count == 0)
			lines.Add(TooltipLine.Of(Resources.ItemComparisonNoDifference, ItemStyle.Broken.TQColor()));

		return lines;
	}

	/// <summary>The color of a difference: higher is better, unless lower is better in its group.</summary>
	public static TQColor ColorOf(StatTrend trend, bool lowerIsBetter)
		=> trend switch
		{
			StatTrend.Mixed => MixedColor,
			StatTrend.Higher => lowerIsBetter ? WorseColor : BetterColor,
			_ => lowerIsBetter ? BetterColor : WorseColor,
		};
}
