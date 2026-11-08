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
		var previous = CultureInfo.CurrentUICulture;
		CultureInfo.CurrentUICulture = new CultureInfo("en-US");
		try
		{
			var choices = SettingsChoices.GameLanguages("fr,de,en");

			choices.Select(c => c.Value).Should().Equal("English", "French", "German");
		}
		finally
		{
			CultureInfo.CurrentUICulture = previous;
		}
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
