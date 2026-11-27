using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Session;
using TQVaultAE.Config;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Unit tests for GameFileService class
/// </summary>
public class GameFileServiceTests
{
	private readonly Mock<ILogger<GameFileService>> _mockLogger;
	private readonly Mock<IGamePathService> _mockGamePathService;
	private readonly Mock<IFileIO> _mockFileIO;
	private readonly Mock<IPathIO> _mockPathIO;
	private readonly Mock<IDirectoryIO> _mockDirectoryIO;
	private readonly Mock<UserSettings> _mockUserSettings;
	private readonly GameFileService _gameFileService;

	public GameFileServiceTests()
	{
		_mockLogger = new Mock<ILogger<GameFileService>>();
		_mockGamePathService = new Mock<IGamePathService>();
		_mockFileIO = new Mock<IFileIO>();
		_mockPathIO = new Mock<IPathIO>();
		_mockDirectoryIO = new Mock<IDirectoryIO>();
		_mockUserSettings = new Mock<UserSettings>();

		// Default pathIO setup for PlayerSave constructor
		_mockPathIO.Setup(x => x.GetFileName(It.IsAny<string>())).Returns<string>(s =>
		{
			var lastSep = s.LastIndexOfAny(['\\', '/']);
			return lastSep >= 0 ? s[(lastSep + 1)..] : s;
		});

		_gameFileService = new GameFileService(
			_mockLogger.Object,
			_mockGamePathService.Object,
			_mockFileIO.Object,
			_mockPathIO.Object,
			_mockDirectoryIO.Object,
			_mockUserSettings.Object
		);
	}

	#region BackupFile Tests

	[Fact]
	public void BackupFile_WithExistingFile_ReturnsBackupPath()
	{
		// Arrange
		var prefix = "backup";
		var file = """C:\Test\player.txt""";
		var backupPath = """C:\Test\backup_player.txt""";

		_mockFileIO.Setup(x => x.Exists(file)).Returns(true);
		_mockGamePathService.Setup(x => x.ConvertFilePathToBackupPath(prefix, file)).Returns(backupPath);
		_mockGamePathService.Setup(x => x.PlayerStashFileNameB).Returns("player stash.dxg"); // Not matching
		_mockPathIO.Setup(x => x.GetFileName(file)).Returns("player.txt");

		// Act
		var result = _gameFileService.BackupFile(prefix, file);

		// Assert
		result.Should().Be(backupPath);
		_mockFileIO.Verify(x => x.Copy(file, backupPath), Times.Once);
	}

	[Fact]
	public void BackupFile_WithNonExistingFile_ReturnsNull()
	{
		// Arrange
		var prefix = "backup";
		var file = """C:\Test\nonexistent.txt""";

		_mockFileIO.Setup(x => x.Exists(file)).Returns(false);

		// Act
		var result = _gameFileService.BackupFile(prefix, file);

		// Assert
		result.Should().BeNull();
		_mockFileIO.Verify(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void BackupFile_WithPlayerStashFile_BackupsDxgFile()
	{
		// Arrange
		var prefix = "backup";
		// Use a file that is NOT already .dxg, so ChangeExtension produces a different file
		var file = """C:\Test\player stash""";
		var backupPath = """C:\Test\backup_player stash""";
		var dxgFile = """C:\Test\player stash.dxg""";
		var dxgBackupPath = """C:\Test\backup_player stash.dxg""";

		_mockFileIO.Setup(x => x.Exists(file)).Returns(true);
		_mockGamePathService.Setup(x => x.ConvertFilePathToBackupPath(prefix, file)).Returns(backupPath);
		_mockPathIO.Setup(x => x.GetFileName(file)).Returns("player stash");
		// Set PlayerStashFileNameB to match the file we're backing up
		_mockGamePathService.Setup(x => x.PlayerStashFileNameB).Returns("player stash");
		_mockPathIO.Setup(x => x.ChangeExtension(file, ".dxg")).Returns(dxgFile);
		_mockFileIO.Setup(x => x.Exists(dxgFile)).Returns(true);
		_mockGamePathService.Setup(x => x.ConvertFilePathToBackupPath(prefix, dxgFile)).Returns(dxgBackupPath);

		// Act
		var result = _gameFileService.BackupFile(prefix, file);

		// Assert
		result.Should().Be(dxgBackupPath); // Last assignment to backupFile is the dxg backup
		_mockFileIO.Verify(x => x.Copy(file, backupPath), Times.Once);
		_mockFileIO.Verify(x => x.Copy(dxgFile, dxgBackupPath), Times.Once);
	}

	#endregion

	#region DuplicateCharacterFiles Tests

	[Fact]
	public void DuplicateCharacterFiles_CreatesNewFolderWithFiles()
	{
		// Arrange
		var playerSaveDirectory = """C:\Test\MyGames\Titan Quest\SaveData\_MyCharacter""";
		var newname = "NewCharacter";
		var baseFolder = """C:\Test\MyGames\Titan Quest\SaveData""";
		var newFolder = """C:\Test\MyGames\Titan Quest\SaveData\_NewCharacter""";
		var playerFile = """C:\Test\MyGames\Titan Quest\SaveData\_MyCharacter\player.chr""";
		var newPlayerFile = """C:\Test\MyGames\Titan Quest\SaveData\_NewCharacter\player.chr""";
		var stashFileB = """C:\Test\MyGames\Titan Quest\SaveData\_MyCharacter\stash.dxg""";
		var stashFileG = """C:\Test\MyGames\Titan Quest\SaveData\_MyCharacter\stash.gsl""";
		var settingsFile = """C:\Test\MyGames\Titan Quest\SaveData\_MyCharacter\settings.txt""";

		_mockPathIO.Setup(x => x.GetDirectoryName(playerSaveDirectory)).Returns(baseFolder);
		_mockPathIO.Setup(x => x.Combine(baseFolder, "_NewCharacter")).Returns(newFolder);
		_mockPathIO.Setup(x => x.Combine(playerSaveDirectory, "player.chr")).Returns(playerFile);
		_mockPathIO.Setup(x => x.Combine(playerSaveDirectory, "stash.dxg")).Returns(stashFileB);
		_mockPathIO.Setup(x => x.Combine(playerSaveDirectory, "stash.gsl")).Returns(stashFileG);
		_mockPathIO.Setup(x => x.Combine(playerSaveDirectory, "settings.txt")).Returns(settingsFile);
		_mockPathIO.Setup(x => x.Combine(newFolder, "player.chr")).Returns(newPlayerFile);
		_mockPathIO.Setup(x => x.Combine(newFolder, "stash.dxg")).Returns(newFolder + "\\stash.dxg");
		_mockPathIO.Setup(x => x.Combine(newFolder, "stash.gsl")).Returns(newFolder + "\\stash.gsl");
		_mockPathIO.Setup(x => x.Combine(newFolder, "settings.txt")).Returns(newFolder + "\\settings.txt");
		var loadoutsFile = playerSaveDirectory + "\\" + ILoadoutService.FileName;
		var newLoadoutsFile = newFolder + "\\" + ILoadoutService.FileName;
		_mockPathIO.Setup(x => x.Combine(playerSaveDirectory, ILoadoutService.FileName)).Returns(loadoutsFile);
		_mockPathIO.Setup(x => x.Combine(newFolder, ILoadoutService.FileName)).Returns(newLoadoutsFile);
		_mockFileIO.Setup(x => x.Exists(loadoutsFile)).Returns(true);
		var classSetsFile = playerSaveDirectory + "\\" + IClassSetService.FileName;
		var newClassSetsFile = newFolder + "\\" + IClassSetService.FileName;
		_mockPathIO.Setup(x => x.Combine(playerSaveDirectory, IClassSetService.FileName)).Returns(classSetsFile);
		_mockPathIO.Setup(x => x.Combine(newFolder, IClassSetService.FileName)).Returns(newClassSetsFile);
		_mockFileIO.Setup(x => x.Exists(classSetsFile)).Returns(true);
		var linkFile = playerSaveDirectory + "\\" + IModLinkService.LinkFileName;
		var newLinkFile = newFolder + "\\" + IModLinkService.LinkFileName;
		_mockPathIO.Setup(x => x.Combine(playerSaveDirectory, IModLinkService.LinkFileName)).Returns(linkFile);
		_mockPathIO.Setup(x => x.Combine(newFolder, IModLinkService.LinkFileName)).Returns(newLinkFile);
		_mockFileIO.Setup(x => x.Exists(linkFile)).Returns(true);

		_mockGamePathService.Setup(x => x.PlayerSaveFileName).Returns("player.chr");
		_mockGamePathService.Setup(x => x.PlayerStashFileNameB).Returns("stash.dxg");
		_mockGamePathService.Setup(x => x.PlayerStashFileNameG).Returns("stash.gsl");
		_mockGamePathService.Setup(x => x.PlayerSettingsFileName).Returns("settings.txt");

		_mockFileIO.Setup(x => x.Exists(playerFile)).Returns(true);
		_mockFileIO.Setup(x => x.Exists(stashFileB)).Returns(true);
		_mockFileIO.Setup(x => x.Exists(stashFileG)).Returns(true);
		_mockFileIO.Setup(x => x.Exists(settingsFile)).Returns(true);

		// Act
		var result = _gameFileService.DuplicateCharacterFiles(playerSaveDirectory, newname);

		// Assert
		result.Should().Be(newFolder);
		_mockDirectoryIO.Verify(x => x.CreateDirectory(newFolder), Times.Once);
		_mockFileIO.Verify(x => x.Copy(playerFile, newPlayerFile), Times.Once);
		_mockFileIO.Verify(x => x.Copy(stashFileB, It.IsAny<string>()), Times.Once);
		_mockFileIO.Verify(x => x.Copy(stashFileG, It.IsAny<string>()), Times.Once);
		_mockFileIO.Verify(x => x.Copy(settingsFile, It.IsAny<string>()), Times.Once);
		_mockFileIO.Verify(x => x.Copy(loadoutsFile, newLoadoutsFile), Times.Once, "the loadouts follow the duplicate");
		_mockFileIO.Verify(x => x.Copy(classSetsFile, newClassSetsFile), Times.Once, "the class sets follow the duplicate");
		_mockFileIO.Verify(x => x.Copy(linkFile, newLinkFile), Times.Once, "the copy of a mod-linked character plays with the same mod");
	}

	[Fact]
	public void DuplicateCharacterFiles_WithNoOptionalFiles_CopiesOnlyRequired()
	{
		// Arrange
		var playerSaveDirectory = """C:\Test\MyGames\Titan Quest\SaveData\_MyCharacter""";
		var newname = "NewCharacter";
		var baseFolder = """C:\Test\MyGames\Titan Quest\SaveData""";
		var newFolder = """C:\Test\MyGames\Titan Quest\SaveData\_NewCharacter""";
		var playerFile = """C:\Test\MyGames\Titan Quest\SaveData\_MyCharacter\player.chr""";

		_mockPathIO.Setup(x => x.GetDirectoryName(playerSaveDirectory)).Returns(baseFolder);
		_mockPathIO.Setup(x => x.Combine(baseFolder, "_NewCharacter")).Returns(newFolder);
		_mockPathIO.Setup(x => x.Combine(playerSaveDirectory, "player.chr")).Returns(playerFile);

		_mockGamePathService.Setup(x => x.PlayerSaveFileName).Returns("player.chr");
		_mockGamePathService.Setup(x => x.PlayerStashFileNameB).Returns("stash.dxg");
		_mockGamePathService.Setup(x => x.PlayerStashFileNameG).Returns("stash.gsl");
		_mockGamePathService.Setup(x => x.PlayerSettingsFileName).Returns("settings.txt");

		_mockFileIO.Setup(x => x.Exists(playerFile)).Returns(true);
		_mockFileIO.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);

		// Act
		var result = _gameFileService.DuplicateCharacterFiles(playerSaveDirectory, newname);

		// Assert
		result.Should().Be(newFolder);
		_mockFileIO.Verify(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(1)); // Only player.chr
	}

	#endregion

	#region ToggleArchive Tests

	private const string ArchiveDir = """C:\Test\MyGames\Titan Quest\SaveData\ArchivedCharacters""";
	private const string ArchivedFolder = """C:\Test\MyGames\Titan Quest\SaveData\ArchivedCharacters\_MyCharacter""";

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void ToggleArchive_MovesTheFolder_AndTheSaveFollows(bool archive)
	{
		// Arrange
		var playerSave = CreateTestPlayerSave();
		playerSave.IsArchived = !archive;
		var oldFolder = playerSave.Folder;
		_mockGamePathService.Setup(x => x.GetBaseCharacterFolder(false, false, true)).Returns(ArchiveDir);
		_mockGamePathService.Setup(x => x.ArchiveTogglePath(oldFolder)).Returns(ArchivedFolder);

		// Act
		_gameFileService.ToggleArchive(playerSave, archive);

		// Assert
		_mockDirectoryIO.Verify(x => x.Move(oldFolder, ArchivedFolder), Times.Once);
		playerSave.IsArchived.Should().Be(archive);
		playerSave.Folder.Should().Be(ArchivedFolder);
	}

	[Fact]
	public void ToggleArchive_FirstArchive_CreatesTheArchiveFolder()
	{
		var playerSave = CreateTestPlayerSave();
		_mockGamePathService.Setup(x => x.GetBaseCharacterFolder(false, false, true)).Returns(ArchiveDir);
		_mockGamePathService.Setup(x => x.ArchiveTogglePath(playerSave.Folder)).Returns(ArchivedFolder);
		_mockDirectoryIO.Setup(x => x.Exists(ArchiveDir)).Returns(false);

		_gameFileService.ToggleArchive(playerSave, archive: true);

		_mockDirectoryIO.Verify(x => x.CreateDirectory(ArchiveDir), Times.Once);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void ToggleArchive_AlreadyInThatState_DoesNothing(bool archive)
	{
		var playerSave = CreateTestPlayerSave();
		playerSave.IsArchived = archive;

		_gameFileService.ToggleArchive(playerSave, archive);

		_mockDirectoryIO.Verify(x => x.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void ToggleArchive_WhenMoveFails_ThrowsToTheCaller_AndLeavesTheSave()
	{
		// Arrange
		var playerSave = CreateTestPlayerSave();
		var oldFolder = playerSave.Folder;
		_mockGamePathService.Setup(x => x.GetBaseCharacterFolder(false, false, true)).Returns(ArchiveDir);
		_mockGamePathService.Setup(x => x.ArchiveTogglePath(oldFolder)).Returns(ArchivedFolder);
		_mockDirectoryIO.Setup(x => x.Exists(ArchiveDir)).Returns(true);
		_mockDirectoryIO.Setup(x => x.Move(oldFolder, ArchivedFolder)).Throws(new IOException("Move failed"));

		// Act
		var act = () => _gameFileService.ToggleArchive(playerSave, archive: true);

		// Assert: the caller collects the failures ("Archive all" shows one message for all of them)
		act.Should().Throw<IOException>();
		playerSave.IsArchived.Should().BeFalse();
		playerSave.Folder.Should().Be(oldFolder);
	}

	#endregion

	/// <summary>
	/// Helper to create test PlayerSave instance
	/// </summary>
	private PlayerSave CreateTestPlayerSave()
	{
		// Need to provide proper pathIO setup for PlayerSave constructor
		return new PlayerSave(
			"""C:\Test\_MyCharacter""", // folder with underscore prefix
			false, // isImmortalThrone
			false, // isArchived
			false, // isCustom
			"", // customMap - empty string
			null!, // translate - not used in tests
			_mockPathIO.Object
		);
	}
}