using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
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
	private readonly Mock<IUIService> _mockUIService;
	private readonly Mock<IFileIO> _mockFileIO;
	private readonly Mock<IPathIO> _mockPathIO;
	private readonly Mock<IDirectoryIO> _mockDirectoryIO;
	private readonly Mock<UserSettings> _mockUserSettings;
	private readonly GameFileService _gameFileService;

	public GameFileServiceTests()
	{
		_mockLogger = new Mock<ILogger<GameFileService>>();
		_mockGamePathService = new Mock<IGamePathService>();
		_mockUIService = new Mock<IUIService>();
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
			_mockUIService.Object,
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
		var file = @"C:\Test\player.txt";
		var backupPath = @"C:\Test\backup_player.txt";

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
		var file = @"C:\Test\nonexistent.txt";

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
		var file = @"C:\Test\player stash";
		var backupPath = @"C:\Test\backup_player stash";
		var dxgFile = @"C:\Test\player stash.dxg";
		var dxgBackupPath = @"C:\Test\backup_player stash.dxg";

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
		var playerSaveDirectory = @"C:\Test\MyGames\Titan Quest\SaveData\_MyCharacter";
		var newname = "NewCharacter";
		var baseFolder = @"C:\Test\MyGames\Titan Quest\SaveData";
		var newFolder = @"C:\Test\MyGames\Titan Quest\SaveData\_NewCharacter";
		var playerFile = @"C:\Test\MyGames\Titan Quest\SaveData\_MyCharacter\player.chr";
		var newPlayerFile = @"C:\Test\MyGames\Titan Quest\SaveData\_NewCharacter\player.chr";
		var stashFileB = @"C:\Test\MyGames\Titan Quest\SaveData\_MyCharacter\stash.dxg";
		var stashFileG = @"C:\Test\MyGames\Titan Quest\SaveData\_MyCharacter\stash.gsl";
		var settingsFile = @"C:\Test\MyGames\Titan Quest\SaveData\_MyCharacter\settings.txt";

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
	}

	[Fact]
	public void DuplicateCharacterFiles_WithNoOptionalFiles_CopiesOnlyRequired()
	{
		// Arrange
		var playerSaveDirectory = @"C:\Test\MyGames\Titan Quest\SaveData\_MyCharacter";
		var newname = "NewCharacter";
		var baseFolder = @"C:\Test\MyGames\Titan Quest\SaveData";
		var newFolder = @"C:\Test\MyGames\Titan Quest\SaveData\_NewCharacter";
		var playerFile = @"C:\Test\MyGames\Titan Quest\SaveData\_MyCharacter\player.chr";

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

	#region Archive Tests

	[Fact]
	public void Archive_WithValidPlayerSave_MovesToArchiveFolder()
	{
		// Arrange
		var playerSave = CreateTestPlayerSave();
		var oldFolder = playerSave.Folder;
		var archiveDir = @"C:\Test\MyGames\Titan Quest\SaveData\ArchivedCharacters";
		var newFolder = @"C:\Test\MyGames\Titan Quest\SaveData\ArchivedCharacters\_MyCharacter";

		_mockGamePathService.Setup(x => x.GetBaseCharacterFolder(false, true)).Returns(archiveDir);
		_mockGamePathService.Setup(x => x.ArchiveTogglePath(oldFolder)).Returns(newFolder);
		_mockDirectoryIO.Setup(x => x.Exists(archiveDir)).Returns(false);
		_mockDirectoryIO.Setup(x => x.CreateDirectory(archiveDir));
		_mockDirectoryIO.Setup(x => x.Move(oldFolder, newFolder));

		// Act
		var result = _gameFileService.Archive(playerSave);

		// Assert
		result.Should().BeTrue();
		playerSave.IsArchived.Should().BeTrue();
		playerSave.Folder.Should().Be(newFolder);
	}

	[Fact]
	public void Archive_WithNullPlayerSave_ReturnsFalse()
	{
		// Act
		var result = _gameFileService.Archive(null!);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void Archive_WithAlreadyArchivedPlayerSave_ReturnsTrue()
	{
		// Arrange
		var playerSave = CreateTestPlayerSave();
		playerSave.IsArchived = true;

		// Act
		var result = _gameFileService.Archive(playerSave);

		// Assert
		result.Should().BeTrue();
		_mockDirectoryIO.Verify(x => x.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void Archive_WhenMoveFails_ReturnsFalse()
	{
		// Arrange
		var playerSave = CreateTestPlayerSave();
		var oldFolder = playerSave.Folder;
		var archiveDir = @"C:\Test\MyGames\Titan Quest\SaveData\ArchivedCharacters";
		var newFolder = @"C:\Test\MyGames\Titan Quest\SaveData\ArchivedCharacters\_MyCharacter";

		_mockGamePathService.Setup(x => x.GetBaseCharacterFolder(false, true)).Returns(archiveDir);
		_mockGamePathService.Setup(x => x.ArchiveTogglePath(oldFolder)).Returns(newFolder);
		_mockDirectoryIO.Setup(x => x.Exists(archiveDir)).Returns(true);
		_mockDirectoryIO.Setup(x => x.Move(oldFolder, newFolder)).Throws(new IOException("Move failed"));

		// Act
		var result = _gameFileService.Archive(playerSave);

		// Assert
		result.Should().BeFalse();
		playerSave.IsArchived.Should().BeFalse();
		_mockUIService.Verify(x => x.ShowError(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<ShowMessageButtons>()), Times.Once);
	}

	#endregion

	#region Unarchive Tests

	[Fact]
	public void Unarchive_WithValidPlayerSave_MovesFromArchiveFolder()
	{
		// Arrange
		var playerSave = CreateTestPlayerSave();
		playerSave.IsArchived = true;
		var oldFolder = playerSave.Folder;
		var newFolder = @"C:\Test\MyGames\Titan Quest\SaveData\_MyCharacter";

		_mockGamePathService.Setup(x => x.ArchiveTogglePath(oldFolder)).Returns(newFolder);
		_mockDirectoryIO.Setup(x => x.Move(oldFolder, newFolder));

		// Act
		var result = _gameFileService.Unarchive(playerSave);

		// Assert
		result.Should().BeTrue();
		playerSave.IsArchived.Should().BeFalse();
		playerSave.Folder.Should().Be(newFolder);
	}

	[Fact]
	public void Unarchive_WithNullPlayerSave_ReturnsFalse()
	{
		// Act
		var result = _gameFileService.Unarchive(null!);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void Unarchive_WithNotArchivedPlayerSave_ReturnsTrue()
	{
		// Arrange
		var playerSave = CreateTestPlayerSave();
		playerSave.IsArchived = false;

		// Act
		var result = _gameFileService.Unarchive(playerSave);

		// Assert
		result.Should().BeTrue();
		_mockDirectoryIO.Verify(x => x.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void Unarchive_WhenMoveFails_ReturnsFalse()
	{
		// Arrange
		var playerSave = CreateTestPlayerSave();
		playerSave.IsArchived = true;
		var oldFolder = playerSave.Folder;
		var newFolder = @"C:\Test\MyGames\Titan Quest\SaveData\_MyCharacter";

		_mockGamePathService.Setup(x => x.ArchiveTogglePath(oldFolder)).Returns(newFolder);
		_mockDirectoryIO.Setup(x => x.Move(oldFolder, newFolder)).Throws(new IOException("Move failed"));

		// Act
		var result = _gameFileService.Unarchive(playerSave);

		// Assert
		result.Should().BeFalse();
		playerSave.IsArchived.Should().BeTrue(); // Should remain unchanged
		_mockUIService.Verify(x => x.ShowError(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<ShowMessageButtons>()), Times.Once);
	}

	#endregion

	#region DoYouWantToReplaceLocalCharacterSave Tests

	[Fact]
	public void DoYouWantToReplaceLocalCharacterSave_WhenUserConfirmsBoth_ReturnsTrue()
	{
		// Arrange
		var warningArgs = new ShowMessageUserEventHandlerEventArgs
		{
			IsOK = true,
			Buttons = ShowMessageButtons.OK
		};

		_mockUIService.Setup(x => x.ShowWarning(It.IsAny<string>(), null, ShowMessageButtons.OKCancel))
			.Callback<string, Exception?, ShowMessageButtons>((msg, ex, btn) => warningArgs.Message = msg)
			.Returns(warningArgs);

		// Act
		var result = _gameFileService.DoYouWantToReplaceLocalCharacterSave();

		// Assert
		result.Should().BeTrue();
		_mockUIService.Verify(x => x.ShowWarning(It.IsAny<string>(), null, ShowMessageButtons.OKCancel), Times.Exactly(2));
	}

	[Fact]
	public void DoYouWantToReplaceLocalCharacterSave_WhenUserCancelsFirst_ReturnsFalse()
	{
		// Arrange
		var warningArgs = new ShowMessageUserEventHandlerEventArgs
		{
			IsOK = false,
			Buttons = ShowMessageButtons.OKCancel
		};

		_mockUIService.Setup(x => x.ShowWarning(It.IsAny<string>(), null, ShowMessageButtons.OKCancel))
			.Returns(warningArgs);

		// Act
		var result = _gameFileService.DoYouWantToReplaceLocalCharacterSave();

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void DoYouWantToReplaceLocalCharacterSave_WhenUserConfirmsFirstButCancelsSecond_ReturnsFalse()
	{
		// Arrange
		var firstCall = true;
		var warningArgs = new ShowMessageUserEventHandlerEventArgs
		{
			IsOK = true,
			Buttons = ShowMessageButtons.OKCancel
		};

		_mockUIService.Setup(x => x.ShowWarning(It.IsAny<string>(), null, ShowMessageButtons.OKCancel))
			.Returns(() =>
			{
				if (firstCall)
				{
					firstCall = false;
					return warningArgs;
				}
				return new ShowMessageUserEventHandlerEventArgs { IsOK = false, Buttons = ShowMessageButtons.OKCancel };
			});

		// Act
		var result = _gameFileService.DoYouWantToReplaceLocalCharacterSave();

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	#region DoYouWantToReplaceLocalVault Tests

	[Fact]
	public void DoYouWantToReplaceLocalVault_WhenUserConfirmsBoth_ReturnsTrue()
	{
		// Arrange
		var warningArgs = new ShowMessageUserEventHandlerEventArgs
		{
			IsOK = true,
			Buttons = ShowMessageButtons.OK
		};

		_mockUIService.Setup(x => x.ShowWarning(It.IsAny<string>(), null, ShowMessageButtons.OKCancel))
			.Callback<string, Exception?, ShowMessageButtons>((msg, ex, btn) => warningArgs.Message = msg)
			.Returns(warningArgs);

		// Act
		var result = _gameFileService.DoYouWantToReplaceLocalVault();

		// Assert
		result.Should().BeTrue();
		_mockUIService.Verify(x => x.ShowWarning(It.IsAny<string>(), null, ShowMessageButtons.OKCancel), Times.Exactly(2));
	}

	[Fact]
	public void DoYouWantToReplaceLocalVault_WhenUserCancelsFirst_ReturnsFalse()
	{
		// Arrange
		var warningArgs = new ShowMessageUserEventHandlerEventArgs
		{
			IsOK = false,
			Buttons = ShowMessageButtons.OKCancel
		};

		_mockUIService.Setup(x => x.ShowWarning(It.IsAny<string>(), null, ShowMessageButtons.OKCancel))
			.Returns(warningArgs);

		// Act
		var result = _gameFileService.DoYouWantToReplaceLocalVault();

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void DoYouWantToReplaceLocalVault_WhenUserConfirmsFirstButCancelsSecond_ReturnsFalse()
	{
		// Arrange
		var firstCall = true;
		var warningArgs = new ShowMessageUserEventHandlerEventArgs
		{
			IsOK = true,
			Buttons = ShowMessageButtons.OKCancel
		};

		_mockUIService.Setup(x => x.ShowWarning(It.IsAny<string>(), null, ShowMessageButtons.OKCancel))
			.Returns(() =>
			{
				if (firstCall)
				{
					firstCall = false;
					return warningArgs;
				}
				return new ShowMessageUserEventHandlerEventArgs { IsOK = false, Buttons = ShowMessageButtons.OKCancel };
			});

		// Act
		var result = _gameFileService.DoYouWantToReplaceLocalVault();

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	/// <summary>
	/// Helper to create test PlayerSave instance
	/// </summary>
	private PlayerSave CreateTestPlayerSave()
	{
		// Need to provide proper pathIO setup for PlayerSave constructor
		return new PlayerSave(
			@"C:\Test\_MyCharacter", // folder with underscore prefix
			false, // isImmortalThrone
			false, // isArchived
			false, // isCustom
			"", // customMap - empty string
			null!, // translate - not used in tests
			_mockPathIO.Object
		);
	}
}