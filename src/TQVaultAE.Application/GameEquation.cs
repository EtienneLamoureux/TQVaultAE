using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TQVaultAE.Application;

/// <summary>
/// Evaluates a designer equation of the game records (<c>combatequations.dbr</c>, <c>healthmanaregen.dbr</c>): its
/// variable names are replaced by their values, then the arithmetic is computed (+ - * / and parentheses).
/// </summary>
public static partial class GameEquation
{
	[GeneratedRegex(@"[A-Za-z_][A-Za-z0-9_]*")]
	private static partial Regex Identifier();

	/// <summary>
	/// The value of an equation; a variable not given counts 0.
	/// </summary>
	/// <exception cref="FormatException">The equation uses an operator the evaluator does not support (<c>^</c>).</exception>
	public static double Evaluate(string equation, IReadOnlyDictionary<string, double> variables)
	{
		ArgumentNullException.ThrowIfNull(equation);
		if (equation.Contains('^', StringComparison.Ordinal))
			throw new FormatException($"Unsupported power operator in \"{equation}\".");

		var lookup = new Dictionary<string, double>(variables, StringComparer.OrdinalIgnoreCase);
		var expression = Identifier().Replace(equation, m => Number(lookup.GetValueOrDefault(m.Value)));
		return Compute(expression);
	}

	/// <summary>
	/// The variables of a damage equation (<c>combatequations.dbr</c>): its damage variable and the character's
	/// attributes and level.
	/// </summary>
	/// <param name="damageVariable">The damage variable, such as <c>physicalDamageDV</c>.</param>
	public static Dictionary<string, double> DamageVariables(string damageVariable, double damage, double strength, double dexterity, double intelligence, double level)
		=> new(StringComparer.OrdinalIgnoreCase)
		{
			[damageVariable] = damage,
			["strengthDV"] = strength,
			["dexterityDV"] = dexterity,
			["intelligenceDV"] = intelligence,
			["characterLevelDV"] = level,
		};

	/// <summary>
	/// One <see cref="DataTable"/> per thread: it is not documented as thread-safe, and equations are evaluated on the
	/// parallel search warm-up as well as on the UI thread.
	/// </summary>
	[ThreadStatic]
	private static DataTable? _calculator;

	/// <summary>The arithmetic of an equation whose variables are already replaced by numbers.</summary>
	private static double Compute(string expression)
		=> Convert.ToDouble((_calculator ??= new DataTable()).Compute(expression, null), CultureInfo.InvariantCulture);

	/// <summary>A value as the evaluator reads it: always with a decimal point, so that divisions are not integer ones.</summary>
	private static string Number(double value)
	{
		var text = value.ToString("0.0###########", CultureInfo.InvariantCulture);
		return value < 0 ? $"({text})" : text;
	}
}
