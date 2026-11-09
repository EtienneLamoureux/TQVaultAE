using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;
using TQVaultAE.Services;
using TQVaultAE.Application.DTOs;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Unit tests for PlayerService class
/// </summary>
public class PlayerServiceTests
{
	private readonly Mock<ILogger<PlayerService>> _mockLogger;
	private readonly Mock<IItemDatabaseService> _mockItemDatabaseService;
	private readonly Mock<IPlayerCollectionProvider> _mockPlayerCollectionProvider;
	private readonly Mock<IGameFileService> _mockGameFileService;
	private readonly Mock<IGamePathService> _mockGamePathService;
	private readonly Mock<ITranslationService> _mockTranslationService;
	private readonly Mock<ITQDataService> _mockTQDataService;
	private readonly Mock<ITagService> _mockTagService;
	private readonly Mock<IStashService> _mockStashService;
	private readonly Mock<FileIO> _mockFileIO;
	private readonly Mock<PathIO> _mockPathIO;
	private readonly SessionContext _sessionContext;
	private readonly UserSettings _userSettings;
	private readonly Mock<IDirectoryIO> _mockDirectoryIO;
	private readonly Mock<IItemProvider> _mockItemProvider;
	private readonly Mock<IDatabase> _mockDatabase;
	private readonly Mock<IGameMapService> _mockGameMapService = new();
	private readonly PlayerService _playerService;

	/// <summary>
	/// Helper to create a PlayerSave with mocked PathIO
	/// </summary>
	private PlayerSave CreatePlayerSave(string folder, bool isIT = false, bool isArchived = false, bool isCustom = false, string customMap = "")
	{
		var mockTranslationService = new Mock<ITranslationService>();

		// Get filename from path - extracts the folder name (e.g., "/Test/_PlayerA" -> "_PlayerA")
		var folderName = Path.GetFileName(folder);

		_mockPathIO.Setup(x => x.GetFileName(folder)).Returns(folderName);
		_mockPathIO.Setup(x => x.GetFileName(customMap)).Returns(string.IsNullOrEmpty(customMap) ? "" : Path.GetFileName(customMap));

		return new PlayerSave(folder, isIT, isArchived, isCustom, customMap, mockTranslationService.Object, _mockPathIO.Object);
	}

	/// <summary>
	/// Initializes test dependencies and PlayerService instance
	/// </summary>
	public PlayerServiceTests()
	{
		_mockLogger = new Mock<ILogger<PlayerService>>();
		_mockItemDatabaseService = new Mock<IItemDatabaseService>();
		_mockPlayerCollectionProvider = new Mock<IPlayerCollectionProvider>();
		_mockGameFileService = new Mock<IGameFileService>();
		_mockGamePathService = new Mock<IGamePathService>();
		_mockTranslationService = new Mock<ITranslationService>();
		_mockTQDataService = new Mock<ITQDataService>();
		_mockTagService = new Mock<ITagService>();
		_mockStashService = new Mock<IStashService>();
		_mockFileIO = new Mock<FileIO>();
		_mockPathIO = new Mock<PathIO> { CallBase = true };
		_userSettings = new UserSettings();
		_mockDirectoryIO = new Mock<IDirectoryIO>();
		_mockItemProvider = new Mock<IItemProvider>();
		_mockDatabase = new Mock<IDatabase>();
		_mockGamePathService.Setup(x => x.PlayerSaveFileName).Returns("Player.chr");

		// Create SessionContext - parameterless data holder
		_sessionContext = new SessionContext();

		_playerService = new PlayerService(
			_mockLogger.Object,
			_sessionContext,
			_mockItemDatabaseService.Object,
			_mockPlayerCollectionProvider.Object,
			_mockStashService.Object,
			_mockGameFileService.Object,
			_mockGamePathService.Object,
			_mockTranslationService.Object,
			_mockTQDataService.Object,
			_mockTagService.Object,
			_mockFileIO.Object,
			_mockPathIO.Object,
			_userSettings,
			_mockDirectoryIO.Object,
			_mockItemProvider.Object,
			_mockDatabase.Object,
			_mockGameMapService.Object
		);
	}

	/// <summary>
	/// Test LoadPlayer method with null PlayerSave
	/// </summary>
	[Fact]
	public void LoadPlayer_WithNullPlayerSave_ReturnsEmptyResult()
	{
		// Act
		var result = _playerService.LoadPlayer(null!);

		// Assert
		result.Should().NotBeNull();
		result.Player.Should().BeNull();
		result.PlayerFile.Should().BeNull();
	}

	[Fact]
	public void LoadCharacterSession_TitanQuestOriginalCharacter_HasNoStashesAndLoadsNoSharedStash()
	{
		var playerSave = CreatePlayerSave("/Test/_");
		this._mockStashService.Setup(x => x.LoadCharacterStash(playerSave, false)).Returns((StashLoadResult?)null);

		var session = this._playerService.LoadCharacterSession(playerSave, loadTransferStash: true, loadRelicVaultStash: true);

		session.HasStashes.Should().BeFalse();
		session.PlayerStash.Should().BeNull();
		session.TransferStash.Should().BeNull();
		session.RelicVaultStash.Should().BeNull();
		this._mockStashService.Verify(x => x.LoadTransferStashFor(It.IsAny<PlayerSave?>(), It.IsAny<bool>()), Times.Never);
		this._mockStashService.Verify(x => x.LoadRelicVaultStashFor(It.IsAny<PlayerSave?>(), It.IsAny<bool>()), Times.Never);
	}

	[Theory]
	[InlineData(true, true)]
	[InlineData(true, false)]
	[InlineData(false, true)]
	[InlineData(false, false)]
	public void LoadCharacterSession_ImmortalThroneCharacter_LoadsItsStashAndTheSharedStashesAskedFor(bool loadTransfer, bool loadRelic)
	{
		var playerSave = CreatePlayerSave("/Test/_", isIT: true);
		var playerStash = StashLoadResult.Empty();
		var transfer = StashLoadResult.Empty();
		var relic = StashLoadResult.Empty();
		this._mockStashService.Setup(x => x.LoadCharacterStash(playerSave, false)).Returns(playerStash);
		this._mockStashService.Setup(x => x.LoadTransferStashFor(playerSave, false)).Returns(transfer);
		this._mockStashService.Setup(x => x.LoadRelicVaultStashFor(playerSave, false)).Returns(relic);

		var session = this._playerService.LoadCharacterSession(playerSave, loadTransfer, loadRelic);

		session.HasStashes.Should().BeTrue();
		session.PlayerStash.Should().BeSameAs(playerStash);
		session.TransferStash.Should().BeSameAs(loadTransfer ? transfer : null);
		session.RelicVaultStash.Should().BeSameAs(loadRelic ? relic : null);
		this._mockStashService.Verify(x => x.LoadTransferStashFor(playerSave, false), loadTransfer ? Times.Once() : Times.Never());
		this._mockStashService.Verify(x => x.LoadRelicVaultStashFor(playerSave, false), loadRelic ? Times.Once() : Times.Never());
	}

	/// <summary>
	/// Test LoadPlayer method with empty/whitespace PlayerSave name
	/// </summary>
	[Fact]
	public void LoadPlayer_WithEmptyPlayerSaveName_ReturnsEmptyResult()
	{
		// Arrange - Use Unix paths for cross-platform compatibility
		var playerSave = CreatePlayerSave("/Test/_");

		// Act
		var result = _playerService.LoadPlayer(playerSave);

		// Assert
		result.Should().NotBeNull();
		result.Player.Should().BeNull();
		result.PlayerFile.Should().BeNull();
	}

	/// <summary>
	/// Test LoadPlayer method with valid PlayerSave
	/// </summary>
	[Fact]
	public void LoadPlayer_WithValidPlayerSave_LoadsSuccessfully()
	{
		// Arrange
		var playerFolder = "/Test/Players/_TestPlayer";
		var playerFile = Path.Combine("/Test/Players/_TestPlayer", "Player.chr");
		var playerSave = CreatePlayerSave(playerFolder);

		_mockPlayerCollectionProvider.Setup(x => x.LoadFile(It.IsAny<PlayerCollection>(), null));

		// Act
		var result = _playerService.LoadPlayer(playerSave);

		// Assert
		result.Should().NotBeNull();
		result.PlayerFile.Should().Be(playerFile);
		_mockTagService.Verify(x => x.LoadTags(playerSave), Times.Once);
	}

	[Fact]
	public void LoadPlayer_KeepsTheClassNameOfItsOwnMap_WhenAnotherMapIsActiveLater()
	{
		// Arrange - read with its mod's text, which renames the class
		var playerSave = CreatePlayerSave("/Test/Players/_TestPlayer", isIT: true);
		_mockTranslationService.Setup(x => x.TranslateXTag("tagCClass01", true, true)).Returns("Woundmender");
		_mockPlayerCollectionProvider.Setup(x => x.LoadFile(It.IsAny<PlayerCollection>(), null))
			.Callback<PlayerCollection, string>((pc, path) => pc.PlayerInfo = new PlayerInfo(_mockPathIO.Object) { Class = "tagCClass01", CurrentLevel = 56 });

		// Act - then another map's text is active
		_playerService.LoadPlayer(playerSave);
		_mockTranslationService.Setup(x => x.TranslateXTag("tagCClass01", true, true)).Returns("Berserker");

		// Assert
		playerSave.ToString().Should().StartWith("TestPlayer, Woundmender");
		_mockGameMapService.Verify(x => x.MarkLoaded(playerSave), Times.Once);
	}

	/// <summary>
	/// Test LoadPlayer method handles ArgumentException from LoadFile
	/// </summary>
	[Fact]
	public void LoadPlayer_WithLoadFileException_ReturnsResultWithException()
	{
		// Arrange
		var playerFolder = "/Test/Players/_TestPlayer";
		var playerFile = Path.Combine("/Test/Players/_TestPlayer", "Player.chr");
		var playerSave = CreatePlayerSave(playerFolder);


		_mockPlayerCollectionProvider.Setup(x => x.LoadFile(It.IsAny<PlayerCollection>(), null))
			.Callback<PlayerCollection, string>((pc, path) => pc.ArgumentException = new ArgumentException("Test error"));

		// Act
		var result = _playerService.LoadPlayer(playerSave);

		// Assert
		result.Should().NotBeNull();
		result.Player.Should().NotBeNull();
		result.Player.ArgumentException.Should().NotBeNull();
		result.Player.ArgumentException.Message.Should().Be("Test error");
	}

	/// <summary>
	/// Test GetPlayerSaveList returns sorted list
	/// </summary>
	[Fact]
	public void GetPlayerSaveList_WithCharacters_ReturnsSortedList()
	{
		// Arrange - Use Unix paths for cross-platform compatibility
		var characterFolders = new[] {
			"/Test/_PlayerC",
			"/Test/_PlayerA",
			"/Test/_PlayerB"
		};

		_mockGamePathService.Setup(x => x.GetCharacterList()).Returns(characterFolders);
		_mockGamePathService.Setup(x => x.SaveDirNameTQIT).Returns("Titan Quest - Immortal Throne");
		_mockGamePathService.Setup(x => x.ArchiveDirName).Returns("ArchivedCharacters");
		_mockGamePathService.Setup(x => x.IsCustom).Returns(false);
		_mockGamePathService.Setup(x => x.MapName).Returns("");

		// Mock PathIO.GetFileName for each character folder
		_mockPathIO.Setup(x => x.GetFileName("/Test/_PlayerC")).Returns("_PlayerC");
		_mockPathIO.Setup(x => x.GetFileName("/Test/_PlayerA")).Returns("_PlayerA");
		_mockPathIO.Setup(x => x.GetFileName("/Test/_PlayerB")).Returns("_PlayerB");
		_mockPathIO.Setup(x => x.GetFileName("")).Returns("");

		// Act
		var result = _playerService.GetPlayerSaveList();

		// Assert
		result.Should().HaveCount(3);
		result[0].Name.Should().Be("PlayerA"); // Should be sorted, Substring(1) removes underscore
		result[1].Name.Should().Be("PlayerB");
		result[2].Name.Should().Be("PlayerC");
	}

	/// <summary>
	/// Test GetPlayerSaveList identifies Immortal Throne characters
	/// </summary>
	[Fact]
	public void GetPlayerSaveList_WithTQITCharacter_SetsIsImmortalThrone()
	{
		// Arrange - Use Unix paths for cross-platform compatibility
		var characterFolders = new[] {
			"/Test/Titan Quest - Immortal Throne/TestPlayer"
		};

		_mockGamePathService.Setup(x => x.GetCharacterList()).Returns(characterFolders);
		_mockGamePathService.Setup(x => x.SaveDirNameTQIT).Returns("Titan Quest - Immortal Throne");
		_mockGamePathService.Setup(x => x.ArchiveDirName).Returns("ArchivedCharacters");
		_mockGamePathService.Setup(x => x.IsCustom).Returns(false);
		_mockGamePathService.Setup(x => x.MapName).Returns("");

		_mockPathIO.Setup(x => x.GetFileName(It.IsAny<string>()))
			.Returns<string>(s => string.IsNullOrEmpty(s) ? "" : Path.GetFileName(s));

		// Act
		var result = _playerService.GetPlayerSaveList();

		// Assert
		result.Should().HaveCount(1);
		result[0].IsImmortalThrone.Should().BeTrue();
	}

	/// <summary>
	/// Test GetPlayerSaveList with empty character list
	/// </summary>
	[Fact]
	public void GetPlayerSaveList_WithEmptyList_ReturnsEmptyArray()
	{
		// Arrange
		var characterFolders = Array.Empty<string>();

		_mockGamePathService.Setup(x => x.GetCharacterList()).Returns(characterFolders);

		// Act
		var result = _playerService.GetPlayerSaveList();

		// Assert
		result.Should().BeEmpty();
	}

	/// <summary>
	/// Test GetPlayerSaveListReadOnly returns same as GetPlayerSaveList
	/// </summary>
	[Fact]
	public void GetPlayerSaveListReadOnly_ReturnsSameAsGetPlayerSaveList()
	{
		// Arrange
		var characterFolders = new[] { "/Test/_PlayerA" };
		_mockGamePathService.Setup(x => x.GetCharacterList()).Returns(characterFolders);
		_mockGamePathService.Setup(x => x.SaveDirNameTQIT).Returns("");
		_mockGamePathService.Setup(x => x.ArchiveDirName).Returns("ArchivedCharacters");
		_mockGamePathService.Setup(x => x.IsCustom).Returns(false);
		_mockGamePathService.Setup(x => x.MapName).Returns("");
		_mockPathIO.Setup(x => x.GetFileName("/Test/_PlayerA")).Returns("_PlayerA");
		_mockPathIO.Setup(x => x.GetFileName("")).Returns("");

		// Act
		var result = _playerService.GetPlayerSaveList();
		var resultReadOnly = _playerService.GetPlayerSaveListReadOnly();

		// Assert
		resultReadOnly.Should().HaveCount(result.Length);
		resultReadOnly.Should().BeEquivalentTo(result);
	}

	/// <summary>
	/// Test AlterNameInPlayerFileSave functionality
	/// </summary>
	[Fact]
	public void AlterNameInPlayerFileSave_WithValidData_CallsTQDataService()
	{
		// Arrange
		var newName = "NewPlayerName";
		var saveFolder = "/Test/SaveFolder";
		var playerFileName = "player.chr";
		var playerFile = Path.Combine(saveFolder, playerFileName);
		var originalContent = new byte[] { 0x01, 0x02, 0x03 };

		_mockGamePathService.Setup(x => x.PlayerSaveFileName).Returns(playerFileName);
		_mockPathIO.Setup(x => x.Combine(saveFolder, playerFileName)).Returns(playerFile);
		_mockFileIO.Setup(x => x.ReadAllBytes(playerFile)).Returns(originalContent);

		// Act
		_playerService.AlterNameInPlayerFileSave(newName, saveFolder);

		// Assert
		_mockTQDataService.Verify(x => x.ReplaceUnicodeValueAfter(
			ref It.Ref<byte[]>.IsAny,
			"myPlayerName",
			newName,
			0), Times.Once);
		_mockFileIO.Verify(x => x.ReadAllBytes(playerFile), Times.Once);
		_mockFileIO.Verify(x => x.WriteAllBytes(playerFile, It.IsAny<byte[]>()), Times.Once);
	}

	/// <summary>
	/// Test LoadPlayerStash returns result from StashService
	/// </summary>
	[Fact]
	public void LoadPlayerStash_WithValidPlayerSave_ReturnsStashResult()
	{
		// Arrange
		var playerSave = CreatePlayerSave("/Test/Players/_TestPlayer", isIT: true);
		var expectedResult = new StashLoadResult { StashFile = "/path/to/stash.dxt" };

		_mockStashService.Setup(x => x.LoadPlayerStash(playerSave, false))
			.Returns(expectedResult);

		// Act
		var result = _playerService.LoadPlayerStash(playerSave, false);

		// Assert
		result.Should().NotBeNull();
		result.StashFile.Should().Be(expectedResult.StashFile);
		_mockStashService.Verify(x => x.LoadPlayerStash(playerSave, false), Times.Once);
	}

	/// <summary>
	/// Test SaveAllModifiedPlayers with no modified players
	/// </summary>
	[Fact]
	public void SaveAllModifiedPlayers_WithNoModifiedPlayers_ReturnsFalse()
	{
		// Arrange
		// Act
		var result = this.SaveAllModifiedPlayers();

		// Assert
		result.Should().BeFalse();
	}

	/// <summary>
	/// Test GetPlayerSaveList with archived character
	/// </summary>
	[Fact]
	public void GetPlayerSaveList_WithArchivedCharacter_SetsIsArchived()
	{
		// Arrange
		var characterFolders = new[] { "/Test/ArchivedCharacters/_ArchivedPlayer" };

		_mockGamePathService.Setup(x => x.GetCharacterList()).Returns(characterFolders);
		_mockGamePathService.Setup(x => x.SaveDirNameTQIT).Returns("");
		_mockGamePathService.Setup(x => x.ArchiveDirName).Returns("ArchivedCharacters");
		_mockGamePathService.Setup(x => x.IsCustom).Returns(false);
		_mockGamePathService.Setup(x => x.MapName).Returns("");

		_mockPathIO.Setup(x => x.GetFileName("/Test/ArchivedCharacters/_ArchivedPlayer")).Returns("_ArchivedPlayer");
		_mockPathIO.Setup(x => x.GetFileName("")).Returns("");

		// Act
		var result = _playerService.GetPlayerSaveList();

		// Assert
		result.Should().HaveCount(1);
		result[0].IsArchived.Should().BeTrue();
	}

	/// <summary>
	/// Test UpdatePlayerFilePath with null old path does nothing
	/// </summary>
	[Fact]
	public void UpdatePlayerFilePath_WithNullOldPath_DoesNothing()
	{
		// Act
		_playerService.UpdatePlayerFilePath(null!, "/new/path");

		// Assert - should not throw
		_sessionContext.Players.Should().BeEmpty();
	}

	/// <summary>
	/// Test UpdatePlayerFilePath with empty old path does nothing
	/// </summary>
	[Fact]
	public void UpdatePlayerFilePath_WithEmptyOldPath_DoesNothing()
	{
		// Act
		_playerService.UpdatePlayerFilePath("", "/new/path");

		// Assert - should not throw
		_sessionContext.Players.Should().BeEmpty();
	}

	/// <summary>
	/// Test UpdatePlayerFilePath with non-existent path does nothing
	/// </summary>
	[Fact]
	public void UpdatePlayerFilePath_WithNonExistentPath_DoesNothing()
	{
		// Act
		_playerService.UpdatePlayerFilePath("/non/existent", "/new/path");

		// Assert - should not throw
		_sessionContext.Players.Should().BeEmpty();
	}

	/// <summary>
	/// Test UpdatePlayerFilePath with valid paths updates player file path
	/// </summary>
	[Fact]
	public void UpdatePlayerFilePath_WithValidPaths_UpdatesPlayerFilePath()
	{
		// Arrange - Create a player in the session context
		var playerFile = Path.Combine("/Test/Players/_TestPlayer", "Player.chr");
		var oldPath = playerFile;
		var newPath = "/Test/Players/Archived/TestPlayer.plr";
		var playerFolder = "/Test/Players/_TestPlayer";
		var playerSave = CreatePlayerSave(playerFolder);

		_mockPlayerCollectionProvider.Setup(x => x.LoadFile(It.IsAny<PlayerCollection>(), null));

		// Load the player first
		_playerService.LoadPlayer(playerSave);

		// Act
		_playerService.UpdatePlayerFilePath(oldPath, newPath);

		// Assert - old path should be removed, new path should exist
		_sessionContext.Players.ContainsKey(oldPath).Should().BeFalse();
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void ArchiveOrUnarchive_WhenMoved_KeysTheLoadedPlayerByItsNewPath(bool archive)
	{
		// Arrange - a loaded player at the path before the move
		var activeFolder = "/Test/Players/_TestPlayer";
		var archivedFolder = "/Test/Players/Archived/_TestPlayer";
		var activePath = Path.Combine(activeFolder, "Player.chr");
		var archivedPath = Path.Combine(archivedFolder, "Player.chr");
		var (fromPath, toPath) = archive ? (activePath, archivedPath) : (archivedPath, activePath);
		var playerSave = CreatePlayerSave(archive ? activeFolder : archivedFolder, isArchived: !archive);
		var player = new PlayerCollection("TestPlayer", fromPath);
		_sessionContext.Players.TryAdd(fromPath, new Lazy<PlayerCollection>(() => player));
		_mockGameFileService.Setup(x => x.Archive(playerSave)).Callback(() => playerSave.Folder = archivedFolder).Returns(true);
		_mockGameFileService.Setup(x => x.Unarchive(playerSave)).Callback(() => playerSave.Folder = activeFolder).Returns(true);

		// Act
		bool moved = archive ? _playerService.Archive(playerSave) : _playerService.Unarchive(playerSave);

		// Assert - every archive path (one save, all, by tag) keeps the session in step with the files
		moved.Should().BeTrue();
		_sessionContext.Players.ContainsKey(fromPath).Should().BeFalse();
		_sessionContext.Players[toPath].Value.PlayerFile.Should().Be(toPath);
	}

	[Fact]
	public void Unarchive_WhenTheMoveFails_KeepsThePlayerWhereItWas()
	{
		// Arrange
		var archivedFolder = "/Test/Players/Archived/_TestPlayer";
		var archivedPath = Path.Combine(archivedFolder, "Player.chr");
		var playerSave = CreatePlayerSave(archivedFolder, isArchived: true);
		_sessionContext.Players.TryAdd(archivedPath, new Lazy<PlayerCollection>(() => new PlayerCollection("TestPlayer", archivedPath)));
		_mockGameFileService.Setup(x => x.Unarchive(playerSave)).Returns(false);

		// Act
		bool moved = _playerService.Unarchive(playerSave);

		// Assert
		moved.Should().BeFalse();
		_sessionContext.Players.Should().ContainKey(archivedPath);
	}

	/// <summary>
	/// Creates a save named after <paramref name="name"/>, with its player file paths and the given tags.
	/// </summary>
	private PlayerSave CreateArchivableSave(string name, bool isArchived, params string[] tags)
	{
		var save = CreatePlayerSave(isArchived ? $"/Test/Players/Archived/_{name}" : $"/Test/Players/_{name}", isArchived: isArchived);
		foreach (var tag in tags)
			save.Tags.Add(tag, System.Drawing.Color.Red);

		return save;
	}

	[Fact]
	public void SetArchived_WithTag_ArchivesOnlyTheActiveSavesHavingTheTag()
	{
		// Arrange
		var taggedActive = CreateArchivableSave("TaggedActive", isArchived: false, "Done");
		var taggedArchived = CreateArchivableSave("TaggedArchived", isArchived: true, "Done");
		var otherTag = CreateArchivableSave("OtherTag", isArchived: false, "Main");
		var untagged = CreateArchivableSave("Untagged", isArchived: false);
		_mockGameFileService.Setup(x => x.Archive(It.IsAny<PlayerSave>())).Returns(true);

		// Act
		var result = _playerService.SetArchived([taggedActive, taggedArchived, otherTag, untagged], archive: true, "Done");

		// Assert
		result.Changed.Should().Equal(taggedActive);
		result.Failed.Should().BeEmpty();
		_mockGameFileService.Verify(x => x.Archive(It.IsAny<PlayerSave>()), Times.Once);
		_mockGameFileService.Verify(x => x.Unarchive(It.IsAny<PlayerSave>()), Times.Never);
	}

	[Fact]
	public void SetArchived_WithoutTag_UnarchivesEveryArchivedSave()
	{
		// Arrange
		var archivedTagged = CreateArchivableSave("ArchivedTagged", isArchived: true, "Done");
		var archivedUntagged = CreateArchivableSave("ArchivedUntagged", isArchived: true);
		var active = CreateArchivableSave("Active", isArchived: false);
		_mockGameFileService.Setup(x => x.Unarchive(It.IsAny<PlayerSave>())).Returns(true);

		// Act
		var result = _playerService.SetArchived([archivedTagged, archivedUntagged, active], archive: false, tag: null);

		// Assert
		result.Changed.Should().Equal(archivedTagged, archivedUntagged);
		result.AnyChanged.Should().BeTrue();
		_mockGameFileService.Verify(x => x.Unarchive(active), Times.Never);
		_mockGameFileService.Verify(x => x.Archive(It.IsAny<PlayerSave>()), Times.Never);
	}

	[Fact]
	public void SetArchived_WhenAMoveFailsOrThrows_ReportsItAndMovesTheOthers()
	{
		// Arrange
		var refused = CreateArchivableSave("Refused", isArchived: false);
		var throwing = CreateArchivableSave("Throwing", isArchived: false);
		var moved = CreateArchivableSave("Moved", isArchived: false);
		_mockGameFileService.Setup(x => x.Archive(refused)).Returns(false);
		_mockGameFileService.Setup(x => x.Archive(throwing)).Throws(new IOException("locked"));
		_mockGameFileService.Setup(x => x.Archive(moved)).Callback(() => moved.Folder = "/Test/Players/Archived/_Moved").Returns(true);
		var movedActivePath = Path.Combine("/Test/Players/_Moved", "Player.chr");
		_sessionContext.Players.TryAdd(movedActivePath, new Lazy<PlayerCollection>(() => new PlayerCollection("Moved", movedActivePath)));

		// Act
		var result = _playerService.SetArchived([refused, throwing, moved], archive: true, tag: null);

		// Assert - the moved save is keyed by its new path, as one by one
		result.Changed.Should().Equal(moved);
		result.Failed.Should().Equal(refused, throwing);
		_sessionContext.Players.Should().ContainKey(Path.Combine("/Test/Players/Archived/_Moved", "Player.chr"));
		_sessionContext.Players.Should().NotContainKey(movedActivePath);
	}

	[Fact]
	public void SetArchived_WhenNothingMatches_ChangesNothing()
	{
		// Arrange
		var archived = CreateArchivableSave("Archived", isArchived: true, "Done");

		// Act
		var result = _playerService.SetArchived([archived], archive: true, "Done");

		// Assert
		result.AnyChanged.Should().BeFalse();
		result.Failed.Should().BeEmpty();
		_mockGameFileService.Verify(x => x.Archive(It.IsAny<PlayerSave>()), Times.Never);
	}

	[Fact]
	public void GetPlayerSaveList_LoadsTheTagsOfEverySave()
	{
		// Arrange
		var characterFolders = new[] { "/Test/_PlayerA", "/Test/_PlayerB" };
		_mockGamePathService.Setup(x => x.GetCharacterList()).Returns(characterFolders);
		_mockGamePathService.Setup(x => x.SaveDirNameTQIT).Returns("Titan Quest - Immortal Throne");
		_mockGamePathService.Setup(x => x.ArchiveDirName).Returns("ArchivedCharacters");
		_mockGamePathService.Setup(x => x.MapName).Returns("");
		_mockPathIO.Setup(x => x.GetFileName("/Test/_PlayerA")).Returns("_PlayerA");
		_mockPathIO.Setup(x => x.GetFileName("/Test/_PlayerB")).Returns("_PlayerB");
		_mockTagService.Setup(x => x.LoadTags(It.IsAny<PlayerSave>()))
			.Callback<PlayerSave>(ps => ps.Tags[ps.Name] = System.Drawing.Color.Blue);

		// Act
		var result = _playerService.GetPlayerSaveList();

		// Assert - displaying a save no longer loads its tags, so the list must
		result.Should().AllSatisfy(ps => ps.Tags.Should().ContainKey(ps.Name));
	}

	/// <summary>
	/// Test LoadPlayer with fromFileWatcher updates existing player
	/// </summary>
	[Fact]
	public void LoadPlayer_WithFromFileWatcherTrue_UpdatesExistingPlayer()
	{
		// Arrange
		var playerFolder = "/Test/Players/_TestPlayer";
		var playerFile = Path.Combine("/Test/Players/_TestPlayer", "Player.chr");
		var playerSave = CreatePlayerSave(playerFolder);


		// First load
		var result1 = _playerService.LoadPlayer(playerSave, false);
		var originalPlayer = result1.Player;

		// Act - second load with fromFileWatcher
		var result2 = _playerService.LoadPlayer(playerSave, true);

		// Assert
		result2.Should().NotBeNull();
		result2.Player.Should().NotBeSameAs(originalPlayer);
		result2.PlayerFile.Should().Be(playerFile);
	}

	/// <summary>
	/// Test SaveAllModifiedPlayers with backup enabled calls backup services
	/// </summary>
	[Fact]
	public void SaveAllModifiedPlayers_WithBackupEnabled_CallsBackupServices()
	{
		// Arrange - Create a modified player in session context
		var playerFolder = "/Test/Players/_TestPlayer";
		var playerFile = Path.Combine("/Test/Players/_TestPlayer", "Player.chr");
		var playerSave = CreatePlayerSave(playerFolder);


		// Create a sack collection that will be marked as modified
		var sackCollection = new SackCollection();
		sackCollection.IsModified = true;

		_mockPlayerCollectionProvider.Setup(x => x.LoadFile(It.IsAny<PlayerCollection>(), null))
			.Callback<PlayerCollection, string>((pc, path) =>
			{
				pc.Sacks = [sackCollection];
			});

		// Load the player
		_playerService.LoadPlayer(playerSave);

		_userSettings.DisableLegacyBackup = false;

		// Act
		var result = this.SaveAllModifiedPlayers();

		// Assert
		result.Should().BeTrue();
		_mockGameFileService.Verify(x => x.BackupFile(It.IsAny<string>(), playerFile), Times.Once);
		_mockGameFileService.Verify(x => x.BackupStupidPlayerBackupFolder(playerFile), Times.Once);
	}

	/// <summary>
	/// Test LoadPlayer with exception logs error and returns error result
	/// </summary>
	[Fact]
	public void LoadPlayer_WithUnexpectedException_LogsErrorAndReturnsErrorResult()
	{
		// Arrange
		var playerFolder = "/Test/Players/_TestPlayer";
		var playerFile = Path.Combine("/Test/Players/_TestPlayer", "Player.chr");
		var playerSave = CreatePlayerSave(playerFolder);

		_mockPlayerCollectionProvider.Setup(x => x.LoadFile(It.IsAny<PlayerCollection>(), null))
			.Callback<PlayerCollection, string>((pc, path) => throw new InvalidOperationException("Unexpected error"));

		// Act
		var result = _playerService.LoadPlayer(playerSave);

		// Assert
		result.Should().NotBeNull();
		result.Error.Should().NotBeNull();
		result.Error.Message.Should().Be("Unexpected error");
	}

	/// <summary>
	/// Test AlterNameInPlayerFileSave with empty save folder does not throw
	/// </summary>
	[Fact]
	public void AlterNameInPlayerFileSave_WithEmptySaveFolder_DoesNotThrow()
	{
		// Arrange
		var newName = "NewPlayerName";
		var saveFolder = "";

		_mockGamePathService.Setup(x => x.PlayerSaveFileName).Returns("player.chr");

		// Act
		var act = () => _playerService.AlterNameInPlayerFileSave(newName, saveFolder);

		// Assert
		act.Should().NotThrow();
	}

	private const string SourceFolder = """C:\Saves\Main\_Hero""";
	private const string NewFolder = """C:\Saves\Main\_Copy""";

	private PlayerSave SetupDuplication()
	{
		var source = CreatePlayerSave(SourceFolder);
		_mockPathIO.Setup(x => x.GetInvalidPathChars()).Returns(['|']);
		_mockPathIO.Setup(x => x.GetInvalidFileNameChars()).Returns(['/', '\\', ':', '*', '?', '"', '<', '>', '|']);
		_mockGameFileService.Setup(x => x.GetDuplicateCharacterFolder(SourceFolder, It.IsAny<string>()))
			.Returns<string, string>((_, name) => $"""C:\Saves\Main\_{name}""");
		return source;
	}

	[Theory]
	[InlineData("", CharacterNameError.Empty)]
	[InlineData("   ", CharacterNameError.Empty)]
	[InlineData("Bad:Name", CharacterNameError.InvalidCharacters)]
	[InlineData("Pipe|Name", CharacterNameError.InvalidCharacters)]
	[InlineData("HERO", CharacterNameError.AlreadyUsed)]
	[InlineData("Copy", CharacterNameError.None)]
	public void ValidateNewCharacterName_ReturnsFirstProblem(string newName, CharacterNameError expected)
	{
		var source = SetupDuplication();

		var error = _playerService.ValidateNewCharacterName(source, newName, ["Hero", "Mage"]);

		error.Should().Be(expected);
	}

	[Fact]
	public void ValidateNewCharacterName_WhenTheTargetFolderExists_ReportsAlreadyUsed()
	{
		var source = SetupDuplication();
		_mockDirectoryIO.Setup(x => x.Exists(NewFolder)).Returns(true);

		var error = _playerService.ValidateNewCharacterName(source, "Copy", ["Hero"]);

		error.Should().Be(CharacterNameError.AlreadyUsed);
	}

	[Fact]
	public void DuplicateCharacter_CopiesFilesAndRenamesThePlayerFile()
	{
		var source = SetupDuplication();
		var newPlayerFile = """C:\Saves\Main\_Copy\Player.chr""";
		_mockGameFileService.Setup(x => x.DuplicateCharacterFiles(SourceFolder, "Copy")).Returns(NewFolder);
		_mockGamePathService.Setup(x => x.PlayerSaveFileName).Returns("Player.chr");
		_mockPathIO.Setup(x => x.Combine(NewFolder, "Player.chr")).Returns(newPlayerFile);
		_mockFileIO.Setup(x => x.ReadAllBytes(newPlayerFile)).Returns([1, 2, 3]);

		var result = _playerService.DuplicateCharacter(source, "Copy");

		result.IsSuccess.Should().BeTrue();
		result.NewFolder.Should().Be(NewFolder);
		_mockFileIO.Verify(x => x.WriteAllBytes(newPlayerFile, It.IsAny<byte[]>()), Times.Once);
		_mockDirectoryIO.Verify(x => x.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
	}

	[Fact]
	public void DuplicateCharacter_WhenRenamingFails_RemovesTheNewFolder()
	{
		var source = SetupDuplication();
		var failure = new IOException("disk full");
		_mockDirectoryIO.SetupSequence(x => x.Exists(NewFolder)).Returns(false).Returns(true);
		_mockGameFileService.Setup(x => x.DuplicateCharacterFiles(SourceFolder, "Copy")).Returns(NewFolder);
		_mockFileIO.Setup(x => x.ReadAllBytes(It.IsAny<string>())).Throws(failure);

		var result = _playerService.DuplicateCharacter(source, "Copy");

		result.IsSuccess.Should().BeFalse();
		result.Error.Should().BeSameAs(failure);
		_mockDirectoryIO.Verify(x => x.Delete(NewFolder, true), Times.Once);
	}

	[Fact]
	public void DuplicateCharacter_WhenCopyFailsHalfway_RemovesThePartialFolder()
	{
		var source = SetupDuplication();
		_mockDirectoryIO.SetupSequence(x => x.Exists(NewFolder)).Returns(false).Returns(true);
		_mockGameFileService.Setup(x => x.DuplicateCharacterFiles(SourceFolder, "Copy")).Throws(new IOException("copy failed"));

		var result = _playerService.DuplicateCharacter(source, "Copy");

		result.IsSuccess.Should().BeFalse();
		_mockDirectoryIO.Verify(x => x.Delete(NewFolder, true), Times.Once);
	}

	[Fact]
	public void DuplicateCharacter_WhenTheTargetFolderExists_LeavesItUntouched()
	{
		var source = SetupDuplication();
		_mockDirectoryIO.Setup(x => x.Exists(NewFolder)).Returns(true);

		var result = _playerService.DuplicateCharacter(source, "Copy");

		result.IsSuccess.Should().BeFalse();
		result.Error.Should().BeOfType<IOException>();
		_mockGameFileService.Verify(x => x.DuplicateCharacterFiles(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		_mockDirectoryIO.Verify(x => x.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
	}

	[Fact]
	public void DuplicateCharacter_WhenRollbackFails_StillReturnsTheOriginalError()
	{
		var source = SetupDuplication();
		var failure = new IOException("copy failed");
		_mockDirectoryIO.SetupSequence(x => x.Exists(NewFolder)).Returns(false).Returns(true);
		_mockGameFileService.Setup(x => x.DuplicateCharacterFiles(SourceFolder, "Copy")).Throws(failure);
		_mockDirectoryIO.Setup(x => x.Delete(NewFolder, true)).Throws(new UnauthorizedAccessException());

		var result = _playerService.DuplicateCharacter(source, "Copy");

		result.Error.Should().BeSameAs(failure);
	}

	#region Requirements and stat bonuses

	private static Item BaseGameSword() => new() { BaseItemId = RecordId.Create("""records\item\equipmentweapon\sword\us_n_01.dbr""") };

	private static Item ImmortalThroneSword() => new() { BaseItemId = RecordId.Create("""records\xpack\items\weapons\swords\u_e_01.dbr""") };

	private static PlayerCollection PlayerWith(int level = 10, int strength = 100, bool immortalThrone = true)
		=> new("test", "test.chr")
		{
			IsImmortalThrone = immortalThrone,
			PlayerInfo = new PlayerInfo(new Mock<IPathIO>().Object) { CurrentLevel = level, BaseStrength = strength },
		};

	private void SetupRequirements(Item item, int level)
	{
		var result = new ToFriendlyNameResult(item);
		result.RequirementVariables.Add(Variable.KEY_LEVELREQ, new Variable(Variable.KEY_LEVELREQ, VariableDataType.Integer, 1) { [0] = level });
		_mockItemProvider.Setup(x => x.GetFriendlyNames(item, FriendlyNamesExtraScopes.Requirements, true)).Returns(result);
	}

	[Fact]
	public void CurrentPlayerMeetsRequirements_WithoutCurrentPlayer_ReturnsTrue()
	{
		var item = BaseGameSword();
		SetupRequirements(item, 99);

		_playerService.CurrentPlayerMeetsRequirements(item).Should().BeTrue();
	}

	[Theory]
	[InlineData(10, true)]
	[InlineData(11, false)]
	public void CurrentPlayerMeetsRequirements_ComparesTheLevelRequirement(int required, bool expected)
	{
		var item = BaseGameSword();
		SetupRequirements(item, required);
		_sessionContext.CurrentPlayer = PlayerWith(level: 10);

		_playerService.CurrentPlayerMeetsRequirements(item).Should().Be(expected);
	}

	[Fact]
	public void CurrentPlayerMeetsRequirements_ItemWithoutRequirements_ReturnsTrue()
	{
		var item = BaseGameSword();
		_mockItemProvider.Setup(x => x.GetFriendlyNames(item, FriendlyNamesExtraScopes.Requirements, true)).Returns(new ToFriendlyNameResult(item));
		_sessionContext.CurrentPlayer = PlayerWith(level: 1);

		_playerService.CurrentPlayerMeetsRequirements(item).Should().BeTrue();
	}

	[Fact]
	public void IsSuitableForCurrentPlayer_OriginalPlayer_RefusesExpansionItems()
	{
		_sessionContext.CurrentPlayer = PlayerWith(immortalThrone: false);

		_playerService.IsSuitableForCurrentPlayer(ImmortalThroneSword()).Should().BeFalse();
		_playerService.IsSuitableForCurrentPlayer(BaseGameSword()).Should().BeTrue();
	}

	[Fact]
	public void IsSuitableForCurrentPlayer_WithoutCurrentPlayer_AcceptsEveryItem()
		=> _playerService.IsSuitableForCurrentPlayer(ImmortalThroneSword()).Should().BeTrue();

	[Fact]
	public void UpdateGearStatBonuses_SkipsTheSecondaryWeaponsAndTheIgnoredItem()
	{
		var player = PlayerWith();
		player.PlayerInfo!.GearBonus = new PlayerStatBonus { StrengthBonus = 50 };
		var ring = new Item { PositionX = 0, PositionY = 0 };
		var primary = new Item { PositionX = Item.WeaponSlotIndicator, PositionY = 0 };
		var secondary = new Item { PositionX = Item.WeaponSlotIndicator, PositionY = 2 };
		var secondaryShield = new Item { PositionX = Item.WeaponSlotIndicator, PositionY = 3 };
		var dragged = new Item { PositionX = 1, PositionY = 0 };
		_mockItemProvider.Setup(x => x.GetStatBonuses(It.IsAny<Item>()))
			.Returns(() => new SortedList<string, int> { ["CHARACTERSTRENGTH"] = 1 });

		_playerService.UpdateGearStatBonuses(player, [ring, primary, secondary, secondaryShield, dragged], dragged);

		// The previous bonus is cleared, then the ring and the primary weapon count.
		player.PlayerInfo.GearBonus.StrengthBonus.Should().Be(2);
		_mockItemProvider.Verify(x => x.GetStatBonuses(secondary), Times.Never);
		_mockItemProvider.Verify(x => x.GetStatBonuses(secondaryShield), Times.Never);
		_mockItemProvider.Verify(x => x.GetStatBonuses(dragged), Times.Never);
	}

	[Fact]
	public void UpdateSkillStatBonuses_SumsTheBonusesOfEverySkillAtItsLevel()
	{
		var player = PlayerWith();
		player.PlayerInfo!.SkillBonus = new PlayerStatBonus { StrengthBonus = 50 };
		player.PlayerInfo.SkillRecordList.Add(new SkillRecord { skillName = """records\skills\a.dbr""", skillLevel = 2 });
		player.PlayerInfo.SkillRecordList.Add(new SkillRecord { skillName = """records\skills\b.dbr""", skillLevel = 3 });
		_mockItemProvider
			.Setup(x => x.GetStatBonusesFromRecord(It.IsAny<SortedList<string, int>>(), It.IsAny<DBRecordCollection?>(), It.IsAny<int>()))
			.Callback((SortedList<string, int> bonuses, DBRecordCollection? _, int level)
				=> bonuses["CHARACTERSTRENGTH"] = bonuses.GetValueOrDefault("CHARACTERSTRENGTH") + level);

		_playerService.UpdateSkillStatBonuses(player);

		player.PlayerInfo.SkillBonus.StrengthBonus.Should().Be(5);
		_mockDatabase.Verify(x => x.GetRecordFromFile(It.IsAny<RecordId>()), Times.Exactly(2));
	}

	[Fact]
	public void UpdateSkillStatBonuses_PlayerWithoutInformation_DoesNothing()
	{
		var player = new PlayerCollection("test", "test.chr") { PlayerInfo = null };

		_playerService.UpdateSkillStatBonuses(player);

		_mockDatabase.Verify(x => x.GetRecordFromFile(It.IsAny<RecordId>()), Times.Never);
	}

	#endregion

	/// <summary>
	/// Saves every modified file the way the save orchestrator does, one by one.
	/// </summary>
	private bool SaveAllModifiedPlayers()
	{
		var modified = this._playerService.GetModifiedPlayers();
		foreach (var (file, player) in modified)
			this._playerService.SaveModifiedPlayer(file, player);

		return modified.Count > 0;
	}
}
