namespace TQVaultAE.Domain.Comparison;

/// <summary>Which way a difference goes, from the numbers' signs.</summary>
public enum StatTrend
{
	/// <summary>Every number is higher (or equal), or the stat is only on this item.</summary>
	Higher,
	/// <summary>Every number is lower (or equal), or the stat is only on the other item.</summary>
	Lower,
	/// <summary>Some numbers are higher, others lower.</summary>
	Mixed,
}

/// <summary>Which items have a stat.</summary>
public enum StatPresence
{
	/// <summary>Both items: the line shows the difference.</summary>
	Both,
	/// <summary>Only this item: the line shows the stat fully plus.</summary>
	OnlyThis,
	/// <summary>Only the other item: the line shows the stat fully minus.</summary>
	OnlyOther,
}

/// <summary>
/// One line of a difference column.
/// </summary>
/// <param name="Text">The stat with the differences in place of its values ("+5 Strength").</param>
/// <param name="Trend">Which way the numbers go.</param>
/// <param name="Presence">Which items have the stat.</param>
public sealed record StatDifference(string Text, StatTrend Trend, StatPresence Presence);

/// <summary>
/// The differences of one group.
/// </summary>
/// <param name="Kind">What the group is about.</param>
/// <param name="Title">The title of the group, or null.</param>
/// <param name="LowerIsBetter">Lower values are better (requirements).</param>
/// <param name="Lines">The differences, the stats of this item first in its order, then those only the other item has.</param>
public sealed record StatDifferenceGroup(StatGroupKind Kind, string? Title, bool LowerIsBetter, IReadOnlyList<StatDifference> Lines);

/// <summary>
/// The two difference columns of a comparison.
/// </summary>
/// <param name="FirstMinusSecond">The first item's stats minus the second's.</param>
/// <param name="SecondMinusFirst">The second item's stats minus the first's.</param>
public sealed record ItemStatComparison(IReadOnlyList<StatDifferenceGroup> FirstMinusSecond, IReadOnlyList<StatDifferenceGroup> SecondMinusFirst);

/// <summary>
/// Compares the stats of two items stat by stat: the same stat on both gives the difference of its numbers, a stat on one
/// item only is shown fully plus on its side and fully minus on the other. Equal stats are left out.
/// </summary>
public static class ItemStatComparer
{
	/// <summary>Compares <paramref name="first"/> and <paramref name="second"/> both ways.</summary>
	public static ItemStatComparison Compare(ItemStatSheet first, ItemStatSheet second)
		=> new(Differences(first, second), Differences(second, first));

	/// <summary><paramref name="self"/> minus <paramref name="other"/>, group by group.</summary>
	public static IReadOnlyList<StatDifferenceGroup> Differences(ItemStatSheet self, ItemStatSheet other)
	{
		var result = new List<StatDifferenceGroup>();
		var otherGroups = other.Groups.ToList();

		foreach (var group in self.Groups)
		{
			// The first group of the other item with the same key, each used once.
			int index = otherGroups.FindIndex(g => g.Key == group.Key);
			StatGroup? match = null;
			if (index >= 0)
			{
				match = otherGroups[index];
				otherGroups.RemoveAt(index);
			}

			Add(result, group, Differences(group, match));
		}

		// Groups only the other item has, fully minus. Kept in the other item's order, before the requirements.
		foreach (var group in otherGroups)
		{
			var diff = new StatDifferenceGroup(group.Kind, group.Title, group.LowerIsBetter, [.. group.Stats.Select(OnlyOther)]);
			int at = result.FindIndex(g => g.Kind == StatGroupKind.Requirements);
			if (at >= 0 && group.Kind != StatGroupKind.Requirements)
				result.Insert(at, diff);
			else
				result.Add(diff);
		}

		return result;
	}

	private static void Add(List<StatDifferenceGroup> result, StatGroup group, List<StatDifference> lines)
	{
		if (lines.Count != 0)
			result.Add(new StatDifferenceGroup(group.Kind, group.Title, group.LowerIsBetter, lines));
	}

	/// <summary>The stats of <paramref name="self"/> against those of <paramref name="other"/> (null when it has none of the group).</summary>
	private static List<StatDifference> Differences(StatGroup self, StatGroup? other)
	{
		var lines = new List<StatDifference>();
		var otherByKey = other?.Stats.ToDictionary(s => s.Key, StringComparer.Ordinal) ?? [];

		foreach (var stat in self.Stats)
		{
			if (!otherByKey.Remove(stat.Key, out var otherStat))
			{
				lines.Add(OnlyThis(stat));
				continue;
			}

			var values = stat.Values.Zip(otherStat.Values, (a, b) => a - b).ToArray();
			if (values.All(v => v == 0))
				continue;

			var decimals = stat.Decimals.Zip(otherStat.Decimals, Math.Max).ToArray();
			var separator = stat.Decimals.Any(d => d > 0) ? stat.DecimalSeparator : otherStat.DecimalSeparator;
			lines.Add(new StatDifference((stat with { DecimalSeparator = separator }).FormatDifference(values, decimals), TrendOf(values), StatPresence.Both));
		}

		// What only the other item has, in its order.
		if (other is not null)
			lines.AddRange(other.Stats.Where(s => otherByKey.ContainsKey(s.Key)).Select(OnlyOther));

		return lines;
	}

	/// <summary>A stat only this item has: its values, fully plus.</summary>
	private static StatDifference OnlyThis(StatLine stat)
		=> new(stat.Format(stat.Values, stat.Decimals), StatTrend.Higher, StatPresence.OnlyThis);

	/// <summary>A stat only the other item has: its values negated, fully minus.</summary>
	private static StatDifference OnlyOther(StatLine stat)
		=> new(stat.Format([.. stat.Values.Select(v => -v)], stat.Decimals), StatTrend.Lower, StatPresence.OnlyOther);

	private static StatTrend TrendOf(decimal[] differences)
	{
		bool higher = differences.Any(v => v > 0);
		bool lower = differences.Any(v => v < 0);
		return higher && lower ? StatTrend.Mixed : higher ? StatTrend.Higher : StatTrend.Lower;
	}
}
