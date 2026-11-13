using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Domain.Comparison;

/// <summary>
/// A displayed stat line split into its text and its numbers: "+42 Strength" is the template "# Strength" with the value 42.
/// Two lines with the same <see cref="Key"/> describe the same stat, whatever their values.
/// </summary>
/// <param name="Template">The text without color tags, numbers replaced by <see cref="Slot"/>, white space collapsed.</param>
/// <param name="Values">The numbers, signed, in reading order.</param>
/// <param name="Decimals">The number of decimals each number was written with.</param>
/// <param name="DecimalSeparator">The decimal separator the line uses ('.' or ','), '.' without decimals.</param>
/// <param name="Indented">The line starts with white space: it belongs to the line above (a "Chance of" group, a pet bonus).</param>
/// <param name="Key">What identifies the stat: the template, after the template of its parent line when indented.</param>
public sealed partial record StatLine(string Template, decimal[] Values, int[] Decimals, char DecimalSeparator, bool Indented, string Key)
{
	/// <summary>Which numbers were written with an explicit plus sign ("+42 Strength"), to write them back the same way.</summary>
	public bool[] Plus { get; init; } = [];

	/// <summary>
	/// What each source gives to this line once lines of the same stat are summed in a <see cref="StatGroup"/>,
	/// in <see cref="StatSource"/> order; empty for a line read alone.
	/// </summary>
	public IReadOnlyList<StatPart> Sources { get; init; } = [];

	/// <summary>Stands for a number in <see cref="Template"/>.</summary>
	public const char Slot = '\u001F';

	/// <summary>
	/// A number, with its sign when the sign is not glued to a letter or a digit: "10-20" is 10 and 20, "+5" is 5, "-5" is -5.
	/// U+2212 (minus sign) counts as '-'.
	/// </summary>
	[GeneratedRegex(@"(?<![\w.,])(?<sign>[+\-−])?(?<int>\d+)(?:(?<sep>[.,])(?<frac>\d+))?")]
	private static partial Regex NumberRegex();

	[GeneratedRegex(@"\s+")]
	private static partial Regex WhiteSpaceRegex();

	/// <summary>
	/// Parses a displayed line; null for a line without text (empty, white space, color tags only).
	/// </summary>
	/// <param name="text">The line as displayed, with its color tags.</param>
	/// <param name="parentTemplate">The template of the last line that was not indented, for an indented line.</param>
	public static StatLine? Parse(string? text, string? parentTemplate = null)
	{
		if (string.IsNullOrWhiteSpace(text))
			return null;

		// Line breaks inside a line ("770 Armor{^N} ") are spaces; then drop the color tags.
		var plain = (text.Replace(StringHelper.TQNewLineTag, " ").Replace('\n', ' ').Replace('\r', ' ').RemoveAllTQTags() ?? string.Empty);
		if (string.IsNullOrWhiteSpace(plain))
			return null;

		bool indented = char.IsWhiteSpace(plain[0]);
		plain = WhiteSpaceRegex().Replace(plain.Trim(), " ");

		var values = new List<decimal>();
		var decimals = new List<int>();
		var plus = new List<bool>();
		char separator = '.';
		var template = new StringBuilder(plain.Length);
		int last = 0;
		foreach (Match m in NumberRegex().Matches(plain))
		{
			var integer = m.Groups["int"].Value;
			var fraction = m.Groups["frac"].Value;
			if (m.Groups["sep"].Success)
				separator = m.Groups["sep"].Value[0];

			var value = decimal.Parse(fraction.Length == 0 ? integer : $"{integer}.{fraction}", NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
			if (m.Groups["sign"].Value is "-" or "−")
				value = -value;

			values.Add(value);
			decimals.Add(fraction.Length);
			plus.Add(m.Groups["sign"].Value == "+");
			template.Append(plain, last, m.Index - last).Append(Slot);
			last = m.Index + m.Length;
		}

		template.Append(plain, last, plain.Length - last);
		var t = template.ToString();
		var key = indented && parentTemplate is not null ? $"{parentTemplate}\n{t}" : t;
		return new StatLine(t, [.. values], [.. decimals], separator, indented, key) { Plus = [.. plus] };
	}

	/// <summary>The indent written before an <see cref="Indented"/> line.</summary>
	public const string Indent = "    ";

	/// <summary>
	/// Writes <paramref name="values"/> in the template, each with an explicit sign ("+5", "-5", "±0"), with
	/// <paramref name="decimals"/> decimals; an indented line keeps an indent.
	/// </summary>
	public string Format(IReadOnlyList<decimal> values, IReadOnlyList<int> decimals)
		=> this.Format(values, decimals, null);

	private string Format(IReadOnlyList<decimal> values, IReadOnlyList<int> decimals, decimal[]? own)
	{
		var text = new StringBuilder(this.Template.Length + (values.Count * 4) + Indent.Length);
		if (this.Indented)
			text.Append(Indent);

		int index = 0;
		foreach (var c in this.Template)
		{
			if (c != Slot)
			{
				text.Append(c);
				continue;
			}

			text.Append(own is not null && values[index] == 0
				? FormatPlain(own[index], decimals[index], this.DecimalSeparator)
				: FormatSigned(values[index], decimals[index], this.DecimalSeparator));
			index++;
		}

		return text.ToString();
	}

	/// <summary>
	/// Writes the differences <paramref name="differences"/> in the template like <see cref="Format"/>, but a number that
	/// did not change shows this line's own value without a sign ("+5.0% Chance of +20 Damage over 3.0 Seconds").
	/// </summary>
	public string FormatDifference(IReadOnlyList<decimal> differences, IReadOnlyList<int> decimals)
		=> this.Format(differences, decimals, this.Values);

	/// <summary>
	/// Writes this line's own values as it was displayed: "+42 Strength" keeps its plus sign, "42% Pierce Resistance" has none.
	/// </summary>
	public string FormatOwn()
	{
		var text = new StringBuilder(this.Template.Length + (this.Values.Length * 4) + Indent.Length);
		if (this.Indented)
			text.Append(Indent);

		int index = 0;
		foreach (var c in this.Template)
		{
			if (c != Slot)
			{
				text.Append(c);
				continue;
			}

			bool plus = index < this.Plus.Length && this.Plus[index] && this.Values[index] > 0;
			text.Append(plus ? "+" : string.Empty).Append(FormatPlain(this.Values[index], this.Decimals[index], this.DecimalSeparator));
			index++;
		}

		return text.ToString();
	}

	/// <summary>A number without a plus sign: "5", "-2.5".</summary>
	public static string FormatPlain(decimal value, int decimals, char separator = '.')
	{
		var text = Math.Round(value, decimals, MidpointRounding.AwayFromZero).ToString($"F{decimals}", CultureInfo.InvariantCulture);
		return separator == '.' ? text : text.Replace('.', separator);
	}

	/// <summary>A number with an explicit sign: "+5", "-2.5", "±0".</summary>
	public static string FormatSigned(decimal value, int decimals, char separator = '.')
	{
		var magnitude = Math.Abs(Math.Round(value, decimals, MidpointRounding.AwayFromZero))
			.ToString($"F{decimals}", CultureInfo.InvariantCulture);
		if (separator != '.')
			magnitude = magnitude.Replace('.', separator);

		var sign = value > 0 ? "+" : value < 0 ? "-" : "±";
		return sign + magnitude;
	}
}
