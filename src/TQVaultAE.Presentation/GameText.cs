using System.Globalization;
using System.Text.RegularExpressions;

namespace TQVaultAE.Presentation;

/// <summary>
/// Fills the game's text formats (<c>{%d0}</c>, <c>{%t1}</c>, <c>{%s1}</c>...) as the game writes them: the character panel,
/// the skill pane and the editor's skill tool tips. Item texts convert the formats for <see cref="string.Format(string, object[])"/>
/// instead (<c>ConvertFormat</c>).
/// </summary>
public static partial class GameText
{
	/// <summary>
	/// Fills a game text format with the values, by index (any printf format). For a whole number <c>%t</c> writes thousands
	/// separators and the <c>+</c> flag (<c>{%+.0f0}</c>, <c>{+%d0}</c>) the sign of positive values; other values are
	/// written as they are.
	/// </summary>
	public static string Format(string format, params object?[] values)
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
			bool signed = m.Value.StartsWith("{%+", StringComparison.Ordinal) || m.Value.StartsWith("{+%", StringComparison.Ordinal);
			return signed && number >= 0 ? "+" + text : text;
		});

	/// <summary>
	/// The elapsed time line, <c>tagCTimeNumber</c> ("{%d0}:{%d1}:{%d2}"): days, hours and minutes of play, as the game
	/// splits its play time in minutes.
	/// </summary>
	public static string PlayTime(string format, int seconds)
	{
		var minutes = Math.Max(0, seconds) / 60;
		return Format(format, minutes / 60 / 24, minutes / 60 % 24, minutes % 60);
	}

	// The sign flag also comes before the % in a few texts: "{+%d0} to all Skills".
	[GeneratedRegex("""\{\+?%[^}]*?(?<type>[a-z])(?<index>[0-9])\}""")]
	private static partial Regex GameFormatRegex();
}
