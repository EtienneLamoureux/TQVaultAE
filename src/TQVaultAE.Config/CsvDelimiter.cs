using System.ComponentModel;
using EnumsNET;

namespace TQVaultAE.Config;

/// <summary>
/// The delimiters of the CSV export, saved by name in <see cref="UserSettings.CSVDelimiter"/>.
/// Each member has its character as description.
/// </summary>
public enum CsvDelimiter
{
	[Description(",")]
	Comma,
	[Description("	")]
	Tab,
	[Description("|")]
	Pipe,
	[Description(":")]
	Colon,
	[Description(";")]
	Semicolon
}

/// <summary>
/// Reads <see cref="CsvDelimiter"/> from the configuration.
/// </summary>
public static class CsvDelimiters
{
	/// <summary>
	/// Reads the delimiter saved in the configuration.
	/// </summary>
	/// <param name="name">The saved value, a member name.</param>
	/// <returns>The delimiter, or <see cref="CsvDelimiter.Comma"/> when the value is missing or unknown.</returns>
	public static CsvDelimiter Parse(string? name)
		=> Enums.TryParse<CsvDelimiter>(name, out var delimiter) ? delimiter : CsvDelimiter.Comma;

	/// <summary>
	/// Gets the character written between the CSV fields.
	/// </summary>
	public static char ToChar(this CsvDelimiter delimiter)
		=> delimiter.AsString(EnumFormat.Description) is { Length: > 0 } description ? description[0] : ',';
}
