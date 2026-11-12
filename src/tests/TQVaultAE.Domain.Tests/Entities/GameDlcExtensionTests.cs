using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

/// <summary>
/// Tests for GameDlc extension methods - meaningful tests only
/// </summary>
public class GameDlcExtensionTests
{
	[Theory]
	[InlineData(GameDlc.TitanQuest, "TQ")]
	[InlineData(GameDlc.ImmortalThrone, "IT")]
	[InlineData(GameDlc.Ragnarok, "RAG")]
	[InlineData(GameDlc.Atlantis, "ATL")]
	[InlineData(GameDlc.EternalEmbers, "EEM")]
	public void GetCode_ReturnsCorrectCode(GameDlc dlc, string expected)
	{
		// Act
		var result = dlc.GetCode();

		// Assert
		result.Should().Be(expected);
	}

	[Theory]
	[InlineData(GameDlc.TitanQuest, "tagBackground01")]
	[InlineData(GameDlc.ImmortalThrone, "tagBackground02")]
	[InlineData(GameDlc.Ragnarok, "tagBackground03")]
	[InlineData(GameDlc.Atlantis, "tagBackground04")]
	[InlineData(GameDlc.EternalEmbers, "x4tagBackground05")]
	public void GetTranslationTag_ReturnsCorrectTag(GameDlc dlc, string expected)
	{
		// Act
		var result = dlc.GetTranslationTag();

		// Assert
		result.Should().Be(expected);
	}

	[Theory]
	[InlineData(GameDlc.TitanQuest, "")]
	[InlineData(GameDlc.ImmortalThrone, "(IT)")]
	[InlineData(GameDlc.Ragnarok, "(RAG)")]
	[InlineData(GameDlc.Atlantis, "(ATL)")]
	[InlineData(GameDlc.EternalEmbers, "(EEM)")]
	public void GetSuffix_ReturnsExpectedSuffix(GameDlc dlc, string expected)
	{
		// Act
		var result = dlc.GetSuffix();

		// Assert
		result.Should().Be(expected);
	}

	[Fact]
	public void AllDlcs_HaveUniqueCodes()
	{
		// Act
		var codes = Enum.GetValues<GameDlc>().Select(d => d.GetCode()).ToList();

		// Assert - Ensures no code collision
		codes.Should().OnlyHaveUniqueItems();
	}

	[Fact]
	public void AllDlcs_HaveUniqueTranslationTags()
	{
		// Act
		var tags = Enum.GetValues<GameDlc>().Select(d => d.GetTranslationTag()).ToList();

		// Assert - Ensures no tag collision
		tags.Should().OnlyHaveUniqueItems();
	}

	[Fact]
	public void AllDlcs_HaveNonEmptyCodes()
	{
		// Act & Assert - Ensures all DLCs have codes
		foreach (var dlc in Enum.GetValues<GameDlc>())
			dlc.GetCode().Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void AllDlcs_HaveNonEmptyTranslationTags()
	{
		// Act & Assert - Ensures all DLCs have translation tags
		foreach (var dlc in Enum.GetValues<GameDlc>())
			dlc.GetTranslationTag().Should().NotBeNullOrEmpty();
	}
}
