using System.Globalization;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
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
/// The two columns of one item in the aligned comparison (<see cref="ItemComparisonContentBuilder.AlignedColumns"/>).
/// </summary>
/// <param name="Item">The item's lines, with a row under a stat for each of its sources shown.</param>
/// <param name="Differences">The difference lines, one per row of <paramref name="Item"/>, then those without a row.</param>
public sealed record AlignedColumnPair(IReadOnlyList<TooltipLine> Item, IReadOnlyList<TooltipLine> Differences);

/// <summary>
/// The choices of the comparison dialog's toolbar.
/// </summary>
/// <param name="AlignRows">Puts each difference level with the item line of its stat.</param>
/// <param name="HideUnchanged">Leaves out the stats both items have with the same numbers.</param>
/// <param name="ShowSources">Shows under each stat summed from several sources what each source gives.</param>
public sealed record ItemComparisonOptions(bool AlignRows = true, bool HideUnchanged = false, bool ShowSources = false)
{
	/// <summary>The choices saved in the user settings.</summary>
	public static ItemComparisonOptions From(ItemCompareSettings? settings)
		=> settings is null ? new() : new(settings.AlignRows, settings.HideUnchanged, settings.ShowSources);

	/// <summary>
	/// Writes these choices into <paramref name="settings"/>.
	/// </summary>
	/// <returns>Whether a saved choice changed (the settings then need a save).</returns>
	public bool ApplyTo(UserSettings settings)
	{
		ArgumentNullException.ThrowIfNull(settings);
		settings.ItemCompare ??= new ItemCompareSettings();
		if (settings.ItemCompare.AlignRows == this.AlignRows && settings.ItemCompare.HideUnchanged == this.HideUnchanged
			&& settings.ItemCompare.ShowSources == this.ShowSources)
			return false;

		settings.ItemCompare.AlignRows = this.AlignRows;
		settings.ItemCompare.HideUnchanged = this.HideUnchanged;
		settings.ItemCompare.ShowSources = this.ShowSources;
		return true;
	}

	/// <summary>
	/// Whether the sources of <paramref name="line"/> show under it: a stat with sources follows <see cref="ShowSources"/>,
	/// unless its key is in <paramref name="toggled"/> (the stats clicked open or closed against it).
	/// </summary>
	public bool SourcesShown(StatDifference line, IReadOnlySet<string>? toggled = null)
	{
		ArgumentNullException.ThrowIfNull(line);
		return line.Sources.Count != 0 && this.ShowSources != (toggled?.Contains(line.Key) ?? false);
	}
}

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
	/// <param name="comparison">The two items and their differences.</param>
	/// <param name="options">The toolbar choices.</param>
	/// <param name="toggled">The keys of the stats whose sources were clicked open or closed against <see cref="ItemComparisonOptions.ShowSources"/>.</param>
	public ItemComparisonContent Build(ItemComparison comparison, ItemComparisonOptions? options = null, IReadOnlySet<string>? toggled = null)
	{
		ArgumentNullException.ThrowIfNull(comparison);
		options ??= new ItemComparisonOptions();
		Func<StatDifference, bool> shown = line => options.SourcesShown(line, toggled);

		// The granted skills' names stay in the item columns: their differences are aligned under them.
		var skills = comparison.Stats.FirstMinusSecond.Concat(comparison.Stats.SecondMinusFirst)
			.Where(g => g.Kind == StatGroupKind.GrantedSkill && g.Title is not null).Select(g => g.Title!).Distinct().ToList();
		var first = ItemColumnLines(this._tooltip.Build(comparison.First, ItemTooltipView.Classic).Lines, skills);
		var second = ItemColumnLines(this._tooltip.Build(comparison.Second, ItemTooltipView.Classic).Lines, skills);
		if (!options.AlignRows)
		{
			return new(
				DifferenceLines(comparison.Stats.FirstMinusSecond, options.HideUnchanged, shown)
				, first
				, second
				, DifferenceLines(comparison.Stats.SecondMinusFirst, options.HideUnchanged, shown));
		}

		var left = AlignedColumns(first, comparison.Stats.FirstMinusSecond, options.HideUnchanged, shown);
		var right = AlignedColumns(second, comparison.Stats.SecondMinusFirst, options.HideUnchanged, shown);
		return new(left.Differences, left.Item, right.Item, right.Differences, Aligned: true);
	}

	/// <summary>
	/// The lines of an item column: the item tooltip without the lines that carry no number (lore, set member lists,
	/// "Speed: Fast", "Grants Skill :"), as in the difference columns. The item name (the first line) and the warnings
	/// stay, and so does a text line heading indented lines with numbers ("Bonus to all Pets:") or naming one of
	/// <paramref name="skillNames"/> (the granted skills, whose differences are aligned under their name). Blank lines and
	/// delimiters left next to each other are merged into one (a delimiter wins), none at the start or the end.
	/// </summary>
	public static IReadOnlyList<TooltipLine> ItemColumnLines(IReadOnlyList<TooltipLine> lines, IEnumerable<string>? skillNames = null)
	{
		var parsed = lines.Select(l => l.Kind is TooltipLineKind.Text or TooltipLineKind.Range ? StatLine.Parse(l.Text) : null).ToArray();
		int name = Array.FindIndex(parsed, s => s is not null);
		var skills = (skillNames ?? []).Select(n => StatLine.Parse(n)?.Template).OfType<string>().Where(t => t.Length != 0).ToList();

		bool Kept(int i)
		{
			var line = lines[i];
			if (line.Kind is TooltipLineKind.Spacer or TooltipLineKind.Delimiter or TooltipLineKind.Warning || i == name)
				return true;

			if (parsed[i] is not { } stat)
				return false;

			if (stat.Values.Length != 0 || skills.Exists(s => stat.Template.Contains(s, StringComparison.Ordinal)))
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
	/// <paramref name="hideUnchanged"/>. "No difference" comes first when no stat differs. A stat whose sources are
	/// <paramref name="sourcesShown"/> has them under it, one line each.
	/// </summary>
	public static IReadOnlyList<TooltipLine> DifferenceLines(IReadOnlyList<StatDifferenceGroup> groups, bool hideUnchanged = false
		, Func<StatDifference, bool>? sourcesShown = null)
	{
		ArgumentNullException.ThrowIfNull(groups);
		sourcesShown ??= NoSources;
		var lines = new List<TooltipLine>();
		if (!HasDifference(groups))
			lines.Add(NoDifferenceLine);

		foreach (var group in groups)
		{
			var shown = group.Lines.Where(l => !Hidden(l, hideUnchanged, sourcesShown(l))).ToList();
			if (shown.Count == 0)
				continue;

			if (lines.Count != 0)
				lines.Add(TooltipLine.Delimiter);

			if (group.Title is not null)
				lines.Add(TitleLine(group.Title));

			foreach (var line in shown)
			{
				bool open = sourcesShown(line);
				lines.Add(LineOf(line, group, open));
				if (open)
					lines.AddRange(line.Sources.Select(source => SourceDifferenceLine(line, source, group)));
			}
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
		=> AlignedColumns(itemLines, groups, hideUnchanged).Differences;

	/// <summary>
	/// The item's column and its difference column level with it, as <see cref="AlignedDifferenceLines"/>. A stat whose
	/// sources are <paramref name="sourcesShown"/> has a row under it for each source, in both columns: the item column
	/// writes what the source gives on this item (a dash when it has no such source), the difference column the difference
	/// with the same source of the other item.
	/// </summary>
	public static AlignedColumnPair AlignedColumns(IReadOnlyList<TooltipLine> itemLines, IReadOnlyList<StatDifferenceGroup> groups
		, bool hideUnchanged = false, Func<StatDifference, bool>? sourcesShown = null)
	{
		ArgumentNullException.ThrowIfNull(itemLines);
		ArgumentNullException.ThrowIfNull(groups);
		sourcesShown ??= NoSources;
		var stats = ItemRowStats(itemLines);
		var rows = new (StatDifferenceGroup Group, StatDifference Line)?[itemLines.Count];
		var rest = new List<(StatDifferenceGroup Group, StatDifference Line)>();

		foreach (var group in groups)
		{
			// A granted skill's stats are only looked for under its name; without that row, they all follow the rows.
			int title = group.Title is null ? -1 : IndexOfTitle(stats, group.Title);
			int from = group.Title is null ? 0 : title < 0 ? stats.Length : title + 1;
			foreach (var line in group.Lines)
			{
				if (Hidden(line, hideUnchanged, sourcesShown(line)))
					continue;

				int row = line.Presence == StatPresence.OnlyOther ? -1 : IndexOfStat(stats, rows, from, line.Key);
				if (row < 0)
					rest.Add((group, line));
				else
					rows[row] = (group, line);
			}
		}

		bool noDifference = !HasDifference(groups) && rows.Length != 0;
		var item = new List<TooltipLine>(itemLines.Count);
		var lines = new List<TooltipLine>(rows.Length + rest.Count + 2);
		for (int i = 0; i < rows.Length; i++)
		{
			if (i == 0 && noDifference)
			{
				item.Add(itemLines[i]);
				lines.Add(NoDifferenceLine);
				continue;
			}

			if (rows[i] is not var (group, line))
			{
				item.Add(itemLines[i]);
				lines.Add(itemLines[i].Kind == TooltipLineKind.Delimiter ? TooltipLine.Delimiter : TooltipLine.Spacer);
				continue;
			}

			// The item's line opens or closes the sources too.
			bool open = sourcesShown(line);
			item.Add(line.Sources.Count == 0 ? itemLines[i] : itemLines[i] with { ToggleKey = line.Key });
			lines.Add(LineOf(line, group, open));
			if (!open)
				continue;

			foreach (var source in line.Sources)
			{
				item.Add(SourceOwnLine(line, source));
				lines.Add(SourceDifferenceLine(line, source, group));
			}
		}

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

			bool open = sourcesShown(line);
			lines.Add(LineOf(line, group, open));
			if (open)
				lines.AddRange(line.Sources.Select(source => SourceDifferenceLine(line, source, group)));
		}

		if (lines.Count == 0)
			lines.Add(NoDifferenceLine);

		return new AlignedColumnPair(item, lines);
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

	/// <summary>The marker after a stat whose sources are hidden: a click shows them.</summary>
	public const string SourcesClosedMarker = "[+]";

	/// <summary>The marker after a stat whose sources are shown: a click hides them.</summary>
	public const string SourcesOpenMarker = "[-]";

	/// <summary>Written in the item column for a source this item does not have.</summary>
	public const string NoSourceText = "—";

	/// <summary>
	/// The name of a source in the lines under a stat: "Base item", "Prefix", "Suffix", "Relic or charm 1", "Completion
	/// bonus 1"...
	/// </summary>
	public static string SourceLabel(StatSource source)
		=> source switch
		{
			StatSource.Base => Resources.ForgeCapSectionBase,
			StatSource.Prefix => Resources.GlobalPrefix,
			StatSource.Suffix => Resources.GlobalSuffix,
			StatSource.Relic1 => string.Format(CultureInfo.CurrentCulture, Resources.ItemComparisonSourceRelic, 1),
			StatSource.Relic2 => string.Format(CultureInfo.CurrentCulture, Resources.ItemComparisonSourceRelic, 2),
			StatSource.RelicBonus1 => string.Format(CultureInfo.CurrentCulture, Resources.ItemComparisonSourceBonus, 1),
			_ => string.Format(CultureInfo.CurrentCulture, Resources.ItemComparisonSourceBonus, 2),
		};

	private static bool NoSources(StatDifference line) => false;

	/// <summary>
	/// A stat left out by "Hide unchanged": same numbers on both items, unless its sources are shown and differ (the same
	/// total made of other parts).
	/// </summary>
	private static bool Hidden(StatDifference line, bool hideUnchanged, bool sourcesShown)
		=> hideUnchanged && line.Trend == StatTrend.Unchanged
		&& !(sourcesShown && line.Sources.Any(s => s.Trend != StatTrend.Unchanged));

	/// <summary>
	/// A difference line in its color; a stat with sources ends with a marker saying whether they are shown, and a click
	/// on it shows or hides them.
	/// </summary>
	private static TooltipLine LineOf(StatDifference line, StatDifferenceGroup group, bool sourcesShown = false)
	{
		var color = ColorOf(line.Trend, line.LowerIsBetter || group.LowerIsBetter);
		if (line.Sources.Count == 0)
			return TooltipLine.Of(line.Text, color);

		var marker = sourcesShown ? SourcesOpenMarker : SourcesClosedMarker;
		return TooltipLine.Of($"{line.Text}  {NeutralColor.ColorTag()}{marker}", color) with { ToggleKey = line.Key };
	}

	/// <summary>
	/// The line of a source under a difference: the source's name, then the difference with the same source of the other
	/// item, in the colors of the differences.
	/// </summary>
	private static TooltipLine SourceDifferenceLine(StatDifference line, StatSourceDifference source, StatDifferenceGroup group)
		=> TooltipLine.Of(SourceText(line, source.Source, source.Text), ColorOf(source.Trend, line.LowerIsBetter || group.LowerIsBetter))
			with { Detail = true };

	/// <summary>The line of a source under an item's stat: the source's name, then what it gives on this item, in gray.</summary>
	private static TooltipLine SourceOwnLine(StatDifference line, StatSourceDifference source)
		=> TooltipLine.Of(SourceText(line, source.Source, source.Own ?? NoSourceText), NeutralColor) with { Detail = true };

	/// <summary>A source's line, one indent deeper than the stat: "    Relic or charm 1: +15% Pierce Resistance".</summary>
	private static string SourceText(StatDifference line, StatSource source, string text)
	{
		var indent = line.Text[..(line.Text.Length - line.Text.TrimStart().Length)];
		return indent + StatLine.Indent + string.Format(CultureInfo.CurrentCulture, Resources.ItemComparisonSourceLine, SourceLabel(source), text.TrimStart());
	}

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
	private static int IndexOfStat(StatLine?[] stats, (StatDifferenceGroup, StatDifference)?[] rows, int from, string key)
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
