using AwesomeAssertions;
using TQVaultAE.Config;

namespace TQVaultAE.Tests.Config;

public class CsvDelimitersTests
{
	[Theory]
	[InlineData("Comma", ',')]
	[InlineData("Tab", '	')]
	[InlineData("Pipe", '|')]
	[InlineData("Colon", ':')]
	[InlineData("Semicolon", ';')]
	public void CsvDelimiterChar_KnownName_GivesItsCharacter(string saved, char expected)
	{
		var settings = new UserSettings { CSVDelimiter = saved };

		settings.CsvDelimiterChar.Should().Be(expected);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("Space")]
	[InlineData("semicolon")]
	public void Parse_MissingOrUnknownName_FallsBackToComma(string? saved)
	{
		CsvDelimiters.Parse(saved).Should().Be(CsvDelimiter.Comma);
	}

	[Fact]
	public void CsvDelimiterChar_IsNotWrittenToTheConfigFile()
	{
		var serializer = new System.Xml.Serialization.XmlSerializer(typeof(UserSettings));
		using var writer = new StringWriter();

		serializer.Serialize(writer, new UserSettings { CSVDelimiter = "Pipe" });

		writer.ToString().Should().Contain("<CSVDelimiter>Pipe</CSVDelimiter>")
			.And.NotContain(nameof(UserSettings.CsvDelimiterChar));
	}
}
