using AwesomeAssertions;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Tests.Application;

public class GamePathEntryTests
{
	[Fact]
	public void GamePathEntry_Constructor_SetsProperties()
	{
		// Arrange
		var path = "/path/to/game";
		var displayName = "My Game";

		// Act
		var entry = new GamePathEntry(path, displayName);

		// Assert
		entry.Path.Should().Be(path);
		entry.DisplayName.Should().Be(displayName);
	}

	[Theory]
	[InlineData("Game Path", "Game Path")]
	[InlineData("", "")]
	[InlineData("Special Chars: @#$%", "Special Chars: @#$%")]
	public void GamePathEntry_DisplayName_VariousInputs(string displayName, string expected)
	{
		// Act
		var entry = new GamePathEntry("/some/path", displayName);

		// Assert
		entry.DisplayName.Should().Be(expected);
	}

	[Theory]
	[InlineData("/unix/path")]
	[InlineData("C:\\Windows\\Path")]
	[InlineData("Relative/Path")]
	[InlineData("")]
	public void GamePathEntry_Path_VariousInputs(string path)
	{
		// Act
		var entry = new GamePathEntry(path, "Display");

		// Assert
		entry.Path.Should().Be(path);
	}

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
	public void GamePathEntry_ToString_WithNullDisplayName_ReturnsPath()
	{
		// Arrange
		var path = "/actual/path";
		var entry = new GamePathEntry(path, null!);

		// Act
		var result = entry.ToString();

		// Assert
		result.Should().Be(path);
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

	[Fact]
	public void GamePathEntry_ReadonlyPath_RetainsValue()
	{
		// Arrange
		var originalPath = "/original/path";
		var entry = new GamePathEntry(originalPath, "Display");

		// Act & Assert
		entry.Path.Should().Be(originalPath);
		// readonly properties retain their initial values
	}

	[Fact]
	public void GamePathEntry_ReadonlyDisplayName_RetainsValue()
	{
		// Arrange
		var originalDisplay = "Original Display";
		var entry = new GamePathEntry("/path", originalDisplay);

		// Act & Assert
		entry.DisplayName.Should().Be(originalDisplay);
		// readonly properties retain their initial values
	}

	[Theory]
	[InlineData("Titan Quest AE", "/games/tq", "Titan Quest AE")]
	[InlineData("Default", "C:\\Games\\Default", "Default")]
	[InlineData("Steam Install", "/home/user/.steam", "Steam Install")]
	public void GamePathEntry_VariousUseCases(string displayName, string path, string expectedDisplay)
	{
		// Act
		var entry = new GamePathEntry(path, displayName);

		// Assert
		entry.DisplayName.Should().Be(expectedDisplay);
		entry.Path.Should().Be(path);
		entry.ToString().Should().Be(displayName);
	}
}
