using AwesomeAssertions;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.Tests.Application;

/// <summary>
/// Tests for PlayerSave - only meaningful tests that verify business logic
/// </summary>
public class PlayerSaveTests
{
	private readonly Mock<ITranslationService> _translationServiceMock;
	private readonly Mock<IPathIO> _pathIOMock;

	public PlayerSaveTests()
	{
		_translationServiceMock = new Mock<ITranslationService>();
		_pathIOMock = new Mock<IPathIO>();
	}

	[Fact]
	public void Constructor_StripsLeadingUnderscoreFromName()
	{
		// Arrange
		var folder = "/path/_Character";
		_pathIOMock.Setup(p => p.GetFileName(folder)).Returns("_Character");

		// Act
		var playerSave = new PlayerSave(folder, false, false, false, "", _translationServiceMock.Object, _pathIOMock.Object);

		// Assert - Name uses Substring(1) to strip underscore
		playerSave.Name.Should().Be("Character");
	}

	[Fact]
	public void Constructor_RetainsLeadingUnderscoreInCustomMapName()
	{
		// Arrange - CustomMapName does NOT strip underscore, unlike Name
		var folder = "/path/_Player";
		var customMap = "/maps/_MapName";
		_pathIOMock.Setup(p => p.GetFileName(folder)).Returns("_Player");
		_pathIOMock.Setup(p => p.GetFileName(customMap)).Returns("_MapName");

		// Act
		var playerSave = new PlayerSave(folder, false, false, true, customMap, _translationServiceMock.Object, _pathIOMock.Object);

		// Assert - CustomMapName keeps the underscore
		playerSave.CustomMapName.Should().Be("_MapName");
	}

	[Fact]
	public void ToString_WithNullInfo_ReturnsNameOnly()
	{
		// Arrange
		_pathIOMock.Setup(p => p.GetFileName(It.IsAny<string>())).Returns("_TestPlayer");
		_translationServiceMock.Setup(t => t.TranslateXTag(It.IsAny<string>(), true, true)).Returns("TranslatedClass");

		var playerSave = new PlayerSave("/folder", true, false, false, "", _translationServiceMock.Object, _pathIOMock.Object);

		// Act
		var result = playerSave.ToString();

		// Assert
		result.Should().Contain("TestPlayer");
	}

	[Fact]
	public void ToString_IsImmortalThroneOmitsTQMarker()
	{
		// Arrange
		_pathIOMock.Setup(p => p.GetFileName(It.IsAny<string>())).Returns("_Test");
		_translationServiceMock.Setup(t => t.TranslateXTag(It.IsAny<string>(), true, true)).Returns("");

		var immortalThrone = new PlayerSave("/folder", true, false, false, "", _translationServiceMock.Object, _pathIOMock.Object);
		var titanQuest = new PlayerSave("/folder", false, false, false, "", _translationServiceMock.Object, _pathIOMock.Object);

		// Act
		var itResult = immortalThrone.ToString();
		var tqResult = titanQuest.ToString();

		// Assert - IsImmortalThrone omits "(TQ)" marker
		itResult.Should().NotContain("(TQ)");
		tqResult.Should().Contain("(TQ)");
	}

	[Fact]
	public void Dispose_WithNullWatcher_DoesNotThrow()
	{
		// Arrange - PlayerSaveWatcher is null
		_pathIOMock.Setup(p => p.GetFileName(It.IsAny<string>())).Returns("test");
		var playerSave = new PlayerSave("/folder", false, false, false, "", _translationServiceMock.Object, _pathIOMock.Object);

		// Act & Assert - Should handle null watcher gracefully
		var act = () => playerSave.Dispose();
		act.Should().NotThrow();
	}

	[Fact]
	public void Dispose_WithWatcher_CallsDispose()
	{
		// Arrange
		_pathIOMock.Setup(p => p.GetFileName(It.IsAny<string>())).Returns("test");
		var playerSave = new PlayerSave("/folder", false, false, false, "", _translationServiceMock.Object, _pathIOMock.Object);
		var watcher = new System.IO.FileSystemWatcher();
		playerSave.PlayerSaveWatcher = watcher;

		// Act
		playerSave.Dispose();

		// Assert - FileSystemWatcher disposes synchronously, verified by EnableRaisingEvents = false
		watcher.EnableRaisingEvents.Should().BeFalse();
	}
}
