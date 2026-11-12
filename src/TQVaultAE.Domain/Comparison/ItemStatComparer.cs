namespace TQVaultAE.Domain.Comparison;

/// <summary>Which way a difference goes, from the numbers' signs.</summary>
public enum StatTrend
{
	/// <summary>Every number is higher (or equal), or the stat is only on this item with positive numbers.</summary>
	Higher,
	/// <summary>Every number is lower (or equal), or the stat is only on the other item with positive numbers.</summary>
	Lower,
	/// <summary>Some numbers are higher, others lower.</summary>
	Mixed,
	/// <summary>The stat is on both items with the same numbers.</summary>
	Unchanged,
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
/// <param name="Text">The stat with the differences in place of its values ("+5 Strength"); an unchanged stat shows its own values.</param>
/// <param name="Trend">Which way the numbers go.</param>
/// <param name="Presence">Which items have the stat.</param>
/// <param name="Key">The stat's <see cref="StatLine.Key"/>, to find the line of the item tooltip it belongs to.</param>
/// <param name="LowerIsBetter">Lower numbers are better for this stat (requirements, energy cost, recharge).</param>
public sealed record StatDifference(string Text, StatTrend Trend, StatPresence Presence, string Key = "", bool LowerIsBetter = false);

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
/// Compares the stats of two items stat by stat: the same stat on both gives the difference of its numbers
/// (<see cref="StatTrend.Unchanged"/> when they are equal), a stat on one item only is shown fully plus on its side and
/// fully minus on the other.
/// </summary>
public static class ItemStatComparer
{
	/// <summary>Compares <paramref name="first"/> and <paramref name="second"/> both ways.</summary>
	/// <param name="first">The first item's stats.</param>
	/// <param name="second">The second item's stats.</param>
	/// <param name="lowerIsBetter">
	/// The <see cref="LowerIsBetterStats.Signature"/> of the stats where lower is better, outside the requirements; null for none.
	/// </param>
	public static ItemStatComparison Compare(ItemStatSheet first, ItemStatSheet second, IReadOnlySet<string>? lowerIsBetter = null)
		=> new(Differences(first, second, lowerIsBetter), Differences(second, first, lowerIsBetter));

	/// <summary><paramref name="self"/> minus <paramref name="other"/>, group by group.</summary>
	public static IReadOnlyList<StatDifferenceGroup> Differences(ItemStatSheet self, ItemStatSheet other, IReadOnlySet<string>? lowerIsBetter = null)
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

			Add(result, group, Differences(group, match, lowerIsBetter));
		}

		// Groups only the other item has, fully minus. Kept in the other item's order, before the requirements.
		foreach (var group in otherGroups)
		{
			var diff = new StatDifferenceGroup(group.Kind, group.Title, group.LowerIsBetter
				, [.. group.Stats.Select(s => OnlyOther(s, IsLowerBetter(group, s, lowerIsBetter)))]);
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
	private static List<StatDifference> Differences(StatGroup self, StatGroup? other, IReadOnlySet<string>? lowerIsBetter)
	{
		var lines = new List<StatDifference>();
		var otherByKey = other?.Stats.ToDictionary(s => s.Key, StringComparer.Ordinal) ?? [];

		foreach (var stat in self.Stats)
		{
			bool lower = IsLowerBetter(self, stat, lowerIsBetter);
			if (!otherByKey.Remove(stat.Key, out var otherStat))
			{
				lines.Add(OnlyThis(stat, lower));
				continue;
			}

			var values = stat.Values.Zip(otherStat.Values, (a, b) => a - b).ToArray();
			var decimals = stat.Decimals.Zip(otherStat.Decimals, Math.Max).ToArray();
			var separator = stat.Decimals.Any(d => d > 0) ? stat.DecimalSeparator : otherStat.DecimalSeparator;
			var trend = TrendOf(values);

			// Unchanged: the stat as the item shows it.
			var text = trend == StatTrend.Unchanged
				? stat.FormatOwn()
				: (stat with { DecimalSeparator = separator }).FormatDifference(values, decimals);
			lines.Add(new StatDifference(text, trend, StatPresence.Both, stat.Key, lower));
		}

		// What only the other item has, in its order.
		if (other is not null)
			lines.AddRange(other.Stats.Where(s => otherByKey.ContainsKey(s.Key)).Select(s => OnlyOther(s, IsLowerBetter(other, s, lowerIsBetter))));

		return lines;
	}

	/// <summary>Lower is better for the whole group (requirements), or for this stat.</summary>
	private static bool IsLowerBetter(StatGroup group, StatLine stat, IReadOnlySet<string>? lowerIsBetter)
		=> group.LowerIsBetter || (lowerIsBetter is { Count: > 0 } && lowerIsBetter.Contains(LowerIsBetterStats.Signature(stat.Template)));

	/// <summary>A stat only this item has: its values, fully plus, compared with nothing.</summary>
	private static StatDifference OnlyThis(StatLine stat, bool lowerIsBetter)
		=> new(stat.Format(stat.Values, stat.Decimals), TrendOf(stat.Values), StatPresence.OnlyThis, stat.Key, lowerIsBetter);

	/// <summary>A stat only the other item has: its values negated, fully minus.</summary>
	private static StatDifference OnlyOther(StatLine stat, bool lowerIsBetter)
	{
		decimal[] negated = [.. stat.Values.Select(v => -v)];
		return new(stat.Format(negated, stat.Decimals), TrendOf(negated), StatPresence.OnlyOther, stat.Key, lowerIsBetter);
	}

	private static StatTrend TrendOf(decimal[] differences)
	{
		bool higher = differences.Any(v => v > 0);
		bool lower = differences.Any(v => v < 0);
		return higher && lower ? StatTrend.Mixed : higher ? StatTrend.Higher : lower ? StatTrend.Lower : StatTrend.Unchanged;
	}
}
