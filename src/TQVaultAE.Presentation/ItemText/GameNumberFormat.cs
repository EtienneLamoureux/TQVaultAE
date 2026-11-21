using System.Globalization;
using System.Text.RegularExpressions;

namespace TQVaultAE.Presentation.ItemText;

/// <summary>
/// Keeps a non-zero value visible in a converted signed game format (<c>{0:+#0;-#0}</c> from <c>{%+.0f0}</c>): a value
/// that rounds to 0 at the format's precision would read "+0", and a negative one "-+0" (.NET writes the first section
/// with a minus sign). Such a value is written with the decimals it has, up to 3 (-0.35 base attack speed of a Tartarus
/// effect); every other value, and the unsigned formats (an energy cost the game rounds), as the format says.
/// </summary>
public static partial class GameNumberFormat
{
	/// <summary>The most decimals added.</summary>
	private const int MaxDecimals = 3;

	[GeneratedRegex(@"\{(?<index>\d+):(?<spec>[^}]*)\}")]
	private static partial Regex PlaceholderRegex();

	/// <summary>The format with more decimals in the placeholders whose non-zero value would round to 0.</summary>
	public static string KeepNonZero(string format, params object?[] values)
	{
		ArgumentNullException.ThrowIfNull(format);
		ArgumentNullException.ThrowIfNull(values);
		if (!format.Contains(':', StringComparison.Ordinal))
			return format;

		return PlaceholderRegex().Replace(format, m =>
		{
			int index = int.Parse(m.Groups["index"].Value, CultureInfo.InvariantCulture);
			if (index >= values.Length || Number(values[index]) is not { } value || value == 0)
				return m.Value;

			var spec = m.Groups["spec"].Value;
			var sections = spec.Split(';');
			if (!sections[0].StartsWith('+') || sections.Any(s => !IsCustomNumber(s)))
				return m.Value;

			int decimals = Decimals(sections[0]);
			if (Math.Round(Math.Abs(value), decimals, MidpointRounding.AwayFromZero) != 0)
				return m.Value;

			var widened = string.Join(";", sections.Select(s => WithDecimals(s, MaxDecimals)));
			return "{" + m.Groups["index"].Value + ":" + widened + "}";
		});
	}

	private static double? Number(object? value) => value switch
	{
		double d => d,
		float f => f,
		decimal m => (double)m,
		_ => null,
	};

	/// <summary>A custom numeric section: digits placeholders with an optional sign and decimals ("+#0.0").</summary>
	private static bool IsCustomNumber(string section)
		=> section.Length != 0 && section.All(c => c is '#' or '0' or '.' or '+' or '-');

	private static int Decimals(string section)
		=> section.IndexOf('.', StringComparison.Ordinal) is var dot and >= 0 ? section.Length - dot - 1 : 0;

	private static string WithDecimals(string section, int decimals)
	{
		var dot = section.IndexOf('.', StringComparison.Ordinal);
		var whole = dot >= 0 ? section[..dot] : section;
		return decimals == 0 ? whole : whole + "." + new string('#', decimals);
	}
}
