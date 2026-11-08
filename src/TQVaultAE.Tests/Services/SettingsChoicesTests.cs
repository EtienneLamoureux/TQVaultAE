using System.Globalization;
using AwesomeAssertions;
using TQVaultAE.Config;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class SettingsChoicesTests
{
	[Fact]
	public void CsvDelimiters_ListsEveryDelimiterWithItsCharacter()
	{
		var choices = SettingsChoices.CsvDelimiters();

		choices.Select(c => c.Value).Should().Equal(Enum.GetValues<CsvDelimiter>());
		choices.Select(c => c.DisplayName).Should().Equal("Comma (,)", "Tab (	)", "Pipe (|)", "Colon (:)", "Semicolon (;)");
	}

	[Fact]
	public void GameLanguages_GivesEnglishNamesSortedByDisplayName()
	{
		var choices = SettingsChoices.GameLanguages("fr,de,en");

		// The display names follow the UI language, so the test does not change the culture (shared by the tests).
		choices.Select(c => c.Value).Should().BeEquivalentTo("English", "French", "German");
		choices.Select(c => c.DisplayName).Should().BeInAscendingOrder(StringComparer.CurrentCulture);
		choices.Should().Contain(c => c.Value == "French" && c.DisplayName == new CultureInfo("fr").DisplayName);
	}

	[Fact]
	public void GameLanguages_SkipsBlankEntries()
	{
		var choices = SettingsChoices.GameLanguages(" fr , ,de,");

		choices.Select(c => c.Value).Should().BeEquivalentTo("French", "German");
	}

	[Fact]
	public void GameLanguages_SkipsInvalidCodes()
	{
		var choices = SettingsChoices.GameLanguages("fr,not a culture!");

		choices.Select(c => c.Value).Should().Equal("French");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData(" , ")]
	public void GameLanguages_NoLanguage_FallsBackToEnglish(string? gameLanguages)
	{
		var choices = SettingsChoices.GameLanguages(gameLanguages);

		choices.Should().ContainSingle().Which.Should().Be(new SettingsChoice<string>("English", "English"));
	}
}
