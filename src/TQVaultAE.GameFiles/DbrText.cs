//-----------------------------------------------------------------------
// <copyright file="DbrText.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Globalization;

namespace TQVaultAE.GameFiles;

/// <summary>
/// The text form of a database record (<c>.dbr</c>), as the game's tools write it: one <c>name,v1;v2,</c> line per
/// variable, floats with 6 decimals, booleans 1/0, invariant culture.
/// </summary>
public static class DbrText
{
	/// <summary>One value as written in a <c>.dbr</c> line.</summary>
	public static string Value(object? value) => value switch
	{
		null => string.Empty,
		string s => s,
		bool b => b ? "1" : "0",
		float f => f.ToString("f6", CultureInfo.InvariantCulture),
		double d => d.ToString("f6", CultureInfo.InvariantCulture),
		IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
		_ => value.ToString() ?? string.Empty,
	};

	/// <summary>One value of a variable of floats: 6 decimals whatever number type holds it.</summary>
	public static string FloatValue(object? value)
		=> value is IFormattable number ? number.ToString("f6", CultureInfo.InvariantCulture) : Value(value);

	/// <summary>The line of one variable: <c>name,v1;v2,</c> (no line break).</summary>
	public static string Line(string name, IReadOnlyList<object?> values)
	{
		using var writer = new StringWriter(CultureInfo.InvariantCulture);
		Write(writer, name, values);
		return writer.ToString();
	}

	/// <summary>Writes the line of one variable, <c>name,v1;v2,</c>, without a line break.</summary>
	public static void Write(TextWriter writer, string name, IReadOnlyList<object?> values)
	{
		ArgumentNullException.ThrowIfNull(writer);
		writer.Write(name);
		writer.Write(',');
		for (int i = 0; i < values.Count; i++)
		{
			if (i > 0)
				writer.Write(';');
			writer.Write(Value(values[i]));
		}
		writer.Write(',');
	}
}
