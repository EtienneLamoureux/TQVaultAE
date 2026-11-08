using System.Collections;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

/// <summary>
/// Checks every translated <see cref="Resources"/> string against the neutral (English) one, so a translation can
/// not drop, add or alter a placeholder: <c>{0}</c> and <c>{1:p3}</c> for <see cref="string.Format(string, object[])"/>,
/// <c>{%.0f0}</c> and <c>{^D}</c> for the game's own format.
/// </summary>
public partial class ResourceTranslationTests
{
	[GeneratedRegex("""\{[^{}]*\}""")]
	private static partial Regex PlaceholderRegex();

	[GeneratedRegex("""^\{(\d+)(,-?\d+)?(:[^{}]*)?\}$""")]
	private static partial Regex CompositeFormatItemRegex();

	/// <summary>The cultures that have a satellite assembly next to the tests.</summary>
	public static TheoryData<string> Cultures()
	{
		var data = new TheoryData<string>();
		var assemblyName = typeof(Resources).Assembly.GetName().Name + ".resources.dll";
		foreach (var directory in Directory.GetDirectories(AppContext.BaseDirectory).Order(StringComparer.Ordinal))
		{
			if (File.Exists(Path.Combine(directory, assemblyName)))
			{
				data.Add(Path.GetFileName(directory));
			}
		}

		return data;
	}

	[Fact]
	public void Cultures_FindsTheShippedTranslations()
		=> Cultures().Select(row => row.Data).Should().Contain(["de", "es", "fr", "pl", "pt", "ru", "zh"]);

	[Theory]
	[MemberData(nameof(Cultures))]
	public void Translation_KeepsThePlaceholdersOfTheNeutralString(string cultureName)
	{
		var neutral = ReadStrings(CultureInfo.InvariantCulture);
		var translated = ReadStrings(CultureInfo.GetCultureInfo(cultureName));

		var mismatches = translated
			.Where(entry => neutral.ContainsKey(entry.Key))
			.Where(entry => !Placeholders(entry.Value).SequenceEqual(Placeholders(neutral[entry.Key])))
			.Select(entry => $"{entry.Key}: \"{entry.Value}\" vs neutral \"{neutral[entry.Key]}\"")
			.ToList();

		mismatches.Should().BeEmpty();
	}

	[Theory]
	[MemberData(nameof(Cultures))]
	public void Translation_OfACompositeFormat_Formats(string cultureName)
	{
		var neutral = ReadStrings(CultureInfo.InvariantCulture);
		var translated = ReadStrings(CultureInfo.GetCultureInfo(cultureName));
		var culture = CultureInfo.GetCultureInfo(cultureName);

		var failures = new List<string>();
		foreach (var (key, value) in translated)
		{
			if (!neutral.TryGetValue(key, out var neutralValue) || !IsCompositeFormat(neutralValue, out var argumentCount))
			{
				continue;
			}

			var arguments = Enumerable.Repeat<object>(1, argumentCount).ToArray();
			try
			{
				_ = string.Format(culture, value, arguments);
			}
			catch (FormatException ex)
			{
				failures.Add($"{key}: \"{value}\" ({ex.Message})");
			}
		}

		failures.Should().BeEmpty();
	}

	private static Dictionary<string, string> ReadStrings(CultureInfo culture)
	{
		var set = Resources.ResourceManager.GetResourceSet(culture, createIfNotExists: true, tryParents: false);
		set.Should().NotBeNull($"the {culture.Name} resources should load");

		return set!.Cast<DictionaryEntry>()
			.Where(entry => entry.Value is string)
			.ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value!);
	}

	private static List<string> Placeholders(string value)
		=> PlaceholderRegex().Matches(value.Replace("{{", string.Empty).Replace("}}", string.Empty))
			.Select(match => match.Value)
			.Order(StringComparer.Ordinal)
			.ToList();

	/// <summary>
	/// Whether <paramref name="value"/> is meant for <see cref="string.Format(string, object[])"/>: it has format items
	/// and nothing but format items in braces. Returns the argument count it needs.
	/// </summary>
	private static bool IsCompositeFormat(string value, out int argumentCount)
	{
		argumentCount = 0;
		var placeholders = Placeholders(value);
		if (placeholders.Count == 0)
		{
			return false;
		}

		foreach (var placeholder in placeholders)
		{
			var match = CompositeFormatItemRegex().Match(placeholder);
			if (!match.Success)
			{
				return false;
			}

			argumentCount = Math.Max(argumentCount, int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) + 1);
		}

		return true;
	}
}
