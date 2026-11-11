using System.Globalization;
using System.Text.RegularExpressions;

namespace TQVaultAE.Application;

/// <summary>A piece of game text with its colour code: <c>{^y}</c> yellow, <c>{^w}</c> white..., or none.</summary>
public readonly record struct GameTextRun(string Text, char? Color);

/// <summary>
/// How the game's character window shows its statistics (records <c>xpack\ui\character\tab1new..tab3new</c>): attribute
/// totals, play time, coloured texts.
/// </summary>
public static partial class CharacterPanelLayout
{
	/// <summary>Width of the statistics panel of the character window.</summary>
	public const int Width = 251;

	/// <summary>Height of the statistics panel.</summary>
	public const int Height = 390;

	/// <summary>The last difficulty of the game: Normal 0, Epic 1, Legendary 2.</summary>
	public const int Legendary = 2;

	/// <summary>
	/// The difficulty the statistics are computed for (resistance penalty, attack speed): the one picked in the editor,
	/// else the last one the character unlocked; always one of the game's three.
	/// </summary>
	/// <param name="unlocked">The difficulty unlocked, as saved.</param>
	/// <param name="picked">The difficulty picked in the editor, -1 for none.</param>
	public static int ShownDifficulty(int unlocked, int picked = -1)
		=> Math.Clamp(picked >= 0 ? picked : unlocked, 0, Legendary);

	/// <summary>Where the panel's gold frame starts, under the tab strip.</summary>
	public const int FrameTop = 20;

	/// <summary>
	/// An attribute as the game shows it: the base value plus the flat bonuses of gear and skills, times the percentage
	/// bonuses, truncated (58 + 13.5 strength shows 71 in the game).
	/// </summary>
	public static int AttributeTotal(int baseValue, double flatBonus, double percentBonus)
		=> (int)Math.Max(0, (baseValue + flatBonus) * (100 + percentBonus) / 100);

	/// <summary>
	/// The elapsed time line, <c>tagCTimeNumber</c> ("{%d0}:{%d1}:{%d2}"): days, hours and minutes of play, as
	/// the game splits its play time in minutes.
	/// </summary>
	public static string PlayTime(string format, int seconds)
	{
		var minutes = Math.Max(0, seconds) / 60;
		return FormatGameText(format, minutes / 60 / 24, minutes / 60 % 24, minutes % 60);
	}

	/// <summary>
	/// Fills a game text format: <c>{%d0}</c>, <c>{%t1}</c>, <c>{%s1}</c>... (any printf format, by index) with the
	/// values. For a whole number <c>%t</c> writes thousands separators and the <c>+</c> flag (<c>{%+.0f0}</c>) the sign of
	/// positive values; other values are written as they are. The one formatter of the game texts: the character panel,
	/// the skill pane and the editor's skill tool tips.
	/// </summary>
	public static string FormatGameText(string format, params object?[] values)
		=> GameFormatRegex().Replace(format, m =>
		{
			var index = m.Groups["index"].Value[0] - '0';
			if (index >= values.Length)
				return m.Value;

			if (values[index] is not (sbyte or byte or short or ushort or int or uint or long) || values[index] is not IConvertible whole)
				return Convert.ToString(values[index], CultureInfo.CurrentCulture) ?? string.Empty;

			var number = whole.ToInt64(CultureInfo.InvariantCulture);
			var text = m.Groups["type"].Value == "t"
				? number.ToString("N0", CultureInfo.CurrentCulture)
				: number.ToString(CultureInfo.CurrentCulture);
			return m.Value.StartsWith("{%+", StringComparison.Ordinal) && number >= 0 ? "+" + text : text;
		});

	/// <summary>Splits a game text at its colour codes (<c>{^y}</c>, <c>{^w}</c>...); other tags are removed.</summary>
	public static IReadOnlyList<GameTextRun> ColorRuns(string text)
	{
		var runs = new List<GameTextRun>();
		char? color = null;
		int start = 0;
		foreach (Match m in ColorCodeRegex().Matches(text))
		{
			if (m.Index > start)
				runs.Add(new GameTextRun(text[start..m.Index], color));

			color = char.ToLowerInvariant(m.Groups["code"].Value[0]);
			start = m.Index + m.Length;
		}

		if (start < text.Length)
			runs.Add(new GameTextRun(text[start..], color));

		return runs;
	}

	[GeneratedRegex("""\{%[^}]*?(?<type>[a-z])(?<index>[0-9])\}""")]
	private static partial Regex GameFormatRegex();

	[GeneratedRegex("""\{\^(?<code>[a-zA-Z])\}""")]
	private static partial Regex ColorCodeRegex();
}
