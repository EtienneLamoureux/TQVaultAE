using System.Globalization;
using System.Text.RegularExpressions;
using TQVaultAE.Domain.Helpers;

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
		return expression.Eval<double>();
	}

	/// <summary>A value as the evaluator reads it: always with a decimal point, so that divisions are not integer ones.</summary>
	private static string Number(double value)
	{
		var text = value.ToString("0.0###########", CultureInfo.InvariantCulture);
		return value < 0 ? $"({text})" : text;
	}
}
