using System.Globalization;
using EnumsNET;
using TQVaultAE.Config;

namespace TQVaultAE.Services;

/// <summary>
/// One entry of a settings drop down: the value saved and the text shown.
/// </summary>
/// <typeparam name="T">Type of the value saved.</typeparam>
/// <param name="Value">The value saved in the configuration.</param>
/// <param name="DisplayName">The text shown.</param>
public sealed record SettingsChoice<T>(T Value, string DisplayName);

/// <summary>
/// Builds the entries of the settings dialog drop downs that come from the configuration.
/// </summary>
public static class SettingsChoices
{
	private const string DefaultLanguage = "English";

	/// <summary>
	/// Lists the CSV delimiters, shown with their character, e.g. "Pipe (|)", in declaration order.
	/// </summary>
	public static IReadOnlyList<SettingsChoice<CsvDelimiter>> CsvDelimiters()
		=> [.. Enums.GetMembers<CsvDelimiter>()
			.Select(m => new SettingsChoice<CsvDelimiter>(m.Value, $"{m.Name} ({m.Value.ToChar()})"))];

	/// <summary>
	/// Lists the game languages of the configuration, sorted by their name in the UI language.
	/// </summary>
	/// <param name="gameLanguages">The ISO codes, comma separated (<see cref="UserSettings.GameLanguages"/>).</param>
	/// <returns>
	/// The languages, with their English name as value (the name the configuration saves). Empty and unknown codes
	/// are left out; English alone when none is left.
	/// </returns>
	public static IReadOnlyList<SettingsChoice<string>> GameLanguages(string? gameLanguages)
	{
		var languages = (gameLanguages ?? string.Empty)
			.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Select(ToCulture)
			.OfType<CultureInfo>()
			.Select(ci => new SettingsChoice<string>(ci.EnglishName, ci.DisplayName))
			.OrderBy(choice => choice.DisplayName)
			.ToList();

		return languages.Count == 0
			? [new SettingsChoice<string>(DefaultLanguage, DefaultLanguage)]
			: languages;
	}

	private static CultureInfo? ToCulture(string iso)
	{
		try
		{
			return new CultureInfo(iso.ToUpperInvariant(), true);
		}
		catch (CultureNotFoundException)
		{
			return null;
		}
	}
}
