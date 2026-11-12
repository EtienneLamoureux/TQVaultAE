using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Comparison;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Presentation;

/// <summary>
/// The four columns of the item comparison, left to right.
/// </summary>
/// <param name="FirstDifferences">The first item's stats minus the second's.</param>
/// <param name="First">The classic tooltip of the first item, without its text-only lines (<see cref="ItemColumnLines"/>).</param>
/// <param name="Second">The classic tooltip of the second item, without its text-only lines.</param>
/// <param name="SecondDifferences">The second item's stats minus the first's.</param>
/// <param name="Aligned">
/// Each difference column has a row for each row of its item's column, level with it (blank when the row has no
/// difference); the differences without a row of their own follow.
/// </param>
public sealed record ItemComparisonContent(
	IReadOnlyList<TooltipLine> FirstDifferences
	, IReadOnlyList<TooltipLine> First
	, IReadOnlyList<TooltipLine> Second
	, IReadOnlyList<TooltipLine> SecondDifferences
	, bool Aligned = false);

/// <summary>
/// The choices of the comparison dialog's toolbar.
/// </summary>
/// <param name="AlignRows">Puts each difference level with the item line of its stat.</param>
/// <param name="HideUnchanged">Leaves out the stats both items have with the same numbers.</param>
public sealed record ItemComparisonOptions(bool AlignRows = true, bool HideUnchanged = false);

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

	/// <summary>Color of a stat both items have with the same numbers: a muted gray on the dark tooltip background.</summary>
	public static readonly TQColor NeutralColor = TQColor.DarkGray;

	private readonly ItemTooltipContentBuilder _tooltip = new(translationService);

	/// <summary>Builds the four columns of <paramref name="comparison"/>.</summary>
	public ItemComparisonContent Build(ItemComparison comparison, ItemComparisonOptions? options = null)
	{
		options ??= new ItemComparisonOptions();
		var first = ItemColumnLines(this._tooltip.Build(comparison.First, ItemTooltipView.Classic).Lines);
		var second = ItemColumnLines(this._tooltip.Build(comparison.Second, ItemTooltipView.Classic).Lines);
		return options.AlignRows
			? new(
				AlignedDifferenceLines(first, comparison.Stats.FirstMinusSecond, options.HideUnchanged)
				, first
				, second
				, AlignedDifferenceLines(second, comparison.Stats.SecondMinusFirst, options.HideUnchanged)
				, Aligned: true)
			: new(
				DifferenceLines(comparison.Stats.FirstMinusSecond, options.HideUnchanged)
				, first
				, second
				, DifferenceLines(comparison.Stats.SecondMinusFirst, options.HideUnchanged));
	}

	/// <summary>
	/// The lines of an item column: the item tooltip without the lines that carry no number (lore, set member lists,
	/// "Speed: Fast", "Grants Skill :"), as in the difference columns. The item name (the first line) and the warnings
	/// stay, and so does a text line heading indented lines with numbers ("Bonus to all Pets:"). Blank lines and
	/// delimiters left next to each other are merged into one (a delimiter wins), none at the start or the end.
	/// </summary>
	public static IReadOnlyList<TooltipLine> ItemColumnLines(IReadOnlyList<TooltipLine> lines)
	{
		var parsed = lines.Select(l => l.Kind is TooltipLineKind.Text or TooltipLineKind.Range ? StatLine.Parse(l.Text) : null).ToArray();
		int name = Array.FindIndex(parsed, s => s is not null);

		bool Kept(int i)
		{
			var line = lines[i];
			if (line.Kind is TooltipLineKind.Spacer or TooltipLineKind.Delimiter or TooltipLineKind.Warning || i == name)
				return true;

			if (parsed[i] is not { } stat)
				return false;

			if (stat.Values.Length != 0)
				return true;

			// A heading: the next line with text is indented and has numbers.
			int next = Array.FindIndex(parsed, i + 1, s => s is not null);
			return next >= 0 && parsed[next] is { Indented: true, Values.Length: > 0 };
		}

		var result = new List<TooltipLine>(lines.Count);
		TooltipLine? gap = null;
		for (int i = 0; i < lines.Count; i++)
		{
			if (!Kept(i))
				continue;

			var line = lines[i];
			if (line.Kind is TooltipLineKind.Spacer or TooltipLineKind.Delimiter)
			{
				if (gap is null || line.Kind == TooltipLineKind.Delimiter)
					gap = line;
				continue;
			}

			if (gap is not null && result.Count != 0)
				result.Add(gap);
			gap = null;
			result.Add(line);
		}

		return result;
	}

	/// <summary>
	/// The lines of a difference column: the groups separated by a delimiter, a granted skill under its name, each
	/// difference colored better or worse for the column's item, unchanged stats in gray unless
	/// <paramref name="hideUnchanged"/>. "No difference" comes first when no stat differs.
	/// </summary>
	public static IReadOnlyList<TooltipLine> DifferenceLines(IReadOnlyList<StatDifferenceGroup> groups, bool hideUnchanged = false)
	{
		var lines = new List<TooltipLine>();
		if (!HasDifference(groups))
			lines.Add(NoDifferenceLine);

		foreach (var group in groups)
		{
			var shown = group.Lines.Where(l => !hideUnchanged || l.Trend != StatTrend.Unchanged).ToList();
			if (shown.Count == 0)
				continue;

			if (lines.Count != 0)
				lines.Add(TooltipLine.Delimiter);

			if (group.Title is not null)
				lines.Add(TitleLine(group.Title));

			lines.AddRange(shown.Select(line => LineOf(line, group)));
		}

		return lines;
	}

	/// <summary>
	/// The lines of a difference column level with its item's column (<paramref name="itemLines"/>): each difference on
	/// the row of the item line showing the same stat, a blank row (or a delimiter facing a delimiter) elsewhere. The
	/// differences without such a row (the stats only the other item has, and those the tooltip writes differently)
	/// follow after a delimiter, as <see cref="DifferenceLines"/> would show them. "No difference" takes the first row
	/// when no stat differs.
	/// </summary>
	public static IReadOnlyList<TooltipLine> AlignedDifferenceLines(IReadOnlyList<TooltipLine> itemLines, IReadOnlyList<StatDifferenceGroup> groups
		, bool hideUnchanged = false)
	{
		var stats = ItemRowStats(itemLines);
		var rows = new TooltipLine?[itemLines.Count];
		var rest = new List<(StatDifferenceGroup Group, StatDifference Line)>();

		foreach (var group in groups)
		{
			// A granted skill's stats are only looked for under its name; without that row, they all follow the rows.
			int title = group.Title is null ? -1 : IndexOfTitle(stats, group.Title);
			int from = group.Title is null ? 0 : title < 0 ? stats.Length : title + 1;
			foreach (var line in group.Lines)
			{
				if (hideUnchanged && line.Trend == StatTrend.Unchanged)
					continue;

				int row = line.Presence == StatPresence.OnlyOther ? -1 : IndexOfStat(stats, rows, from, line.Key);
				if (row < 0)
					rest.Add((group, line));
				else
					rows[row] = LineOf(line, group);
			}
		}

		if (!HasDifference(groups) && rows.Length != 0)
			rows[0] = NoDifferenceLine;

		var lines = new List<TooltipLine>(rows.Length + rest.Count + 2);
		for (int i = 0; i < rows.Length; i++)
			lines.Add(rows[i] ?? (itemLines[i].Kind == TooltipLineKind.Delimiter ? TooltipLine.Delimiter : TooltipLine.Spacer));

		StatDifferenceGroup? last = null;
		foreach (var (group, line) in rest)
		{
			if (!ReferenceEquals(group, last))
			{
				lines.Add(TooltipLine.Delimiter);
				if (group.Title is not null)
					lines.Add(TitleLine(group.Title));
				last = group;
			}

			lines.Add(LineOf(line, group));
		}

		if (lines.Count == 0)
			lines.Add(NoDifferenceLine);

		return lines;
	}

	/// <summary>The color of a difference: higher is better, unless lower is better for its stat; gray when unchanged.</summary>
	public static TQColor ColorOf(StatTrend trend, bool lowerIsBetter)
		=> trend switch
		{
			StatTrend.Unchanged => NeutralColor,
			StatTrend.Mixed => MixedColor,
			StatTrend.Higher => lowerIsBetter ? WorseColor : BetterColor,
			_ => lowerIsBetter ? BetterColor : WorseColor,
		};

	private static TooltipLine NoDifferenceLine => TooltipLine.Of(Resources.ItemComparisonNoDifference, ItemStyle.Broken.TQColor());

	private static TooltipLine TitleLine(string title) => TooltipLine.Of(title, ItemStyle.Epic.TQColor(), bold: true);

	private static TooltipLine LineOf(StatDifference line, StatDifferenceGroup group)
		=> TooltipLine.Of(line.Text, ColorOf(line.Trend, line.LowerIsBetter || group.LowerIsBetter));

	private static bool HasDifference(IReadOnlyList<StatDifferenceGroup> groups)
		=> groups.Any(g => g.Lines.Any(l => l.Trend != StatTrend.Unchanged));

	/// <summary>The stat each item row shows, parsed as the comparison parses the item's texts; null for a row without text.</summary>
	private static StatLine?[] ItemRowStats(IReadOnlyList<TooltipLine> itemLines)
	{
		var stats = new StatLine?[itemLines.Count];
		string? parent = null;
		for (int i = 0; i < itemLines.Count; i++)
		{
			var line = itemLines[i];
			if (line.Kind is TooltipLineKind.Spacer or TooltipLineKind.Delimiter)
				continue;

			var stat = StatLine.Parse(line.Text, parent);
			if (stat is null)
				continue;

			if (!stat.Indented)
				parent = stat.Template;
			stats[i] = stat;
		}

		return stats;
	}

	/// <summary>The row naming a granted skill: its text contains the skill's name; -1 when none does.</summary>
	private static int IndexOfTitle(StatLine?[] stats, string title)
	{
		var name = StatLine.Parse(title)?.Template;
		if (string.IsNullOrEmpty(name))
			return -1;

		return Array.FindIndex(stats, s => s is not null && s.Template.Contains(name, StringComparison.Ordinal));
	}

	/// <summary>
	/// The first free row from <paramref name="from"/> showing the stat <paramref name="key"/>; else the first free row with
	/// its own text (the item tooltip may head it differently); -1 when none does.
	/// </summary>
	private static int IndexOfStat(StatLine?[] stats, TooltipLine?[] rows, int from, string key)
	{
		if (key.Length == 0)
			return -1;

		for (int i = from; i < stats.Length; i++)
		{
			if (rows[i] is null && stats[i] is { Values.Length: > 0 } stat && string.Equals(stat.Key, key, StringComparison.Ordinal))
				return i;
		}

		var template = key[(key.LastIndexOf('\n') + 1)..];
		for (int i = from; i < stats.Length; i++)
		{
			if (rows[i] is null && stats[i] is { Values.Length: > 0 } stat && string.Equals(stat.Template, template, StringComparison.Ordinal))
				return i;
		}

		return -1;
	}
}
