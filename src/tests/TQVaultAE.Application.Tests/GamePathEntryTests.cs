using AwesomeAssertions;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Tests.Application;

public class GamePathEntryTests
{
	[Theory]
	[InlineData("Display Name", "Display Name")]
	[InlineData(null, null)]
	[InlineData("", "")]
	public void GamePathEntry_ToString_WithDisplayName(string? displayName, string? expected)
	{
		// Arrange
		var entry = new GamePathEntry("/path", displayName);

		// Act
		var result = entry.ToString();

		// Assert
		result.Should().Be(expected ?? "/path");
	}

	[Fact]
	public void GamePathEntry_ToString_WithNullDisplayNameAndPath_ReturnsEmpty()
	{
		// Arrange
		var entry = new GamePathEntry(null!, null!);

		// Act
		var result = entry.ToString();

		// Assert
		result.Should().Be("Empty");
	}

	[Fact]
	public void GamePathEntry_ToString_WithEmptyStrings_ReturnsEmpty()
	{
		// Arrange
		var entry = new GamePathEntry("", "");

		// Act
		var result = entry.ToString();

		// Assert
		result.Should().BeEmpty();
	}
}
