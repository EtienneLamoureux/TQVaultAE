using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Unit tests for StashService class
/// </summary>
public class StashServiceTests
{
	private readonly Mock<ILogger<StashService>> _mockLogger;
	private readonly SessionContext _sessionContext;
	private readonly Mock<IItemDatabaseService> _mockItemDatabaseService;
	private readonly Mock<IStashProvider> _mockStashProvider;
	private readonly Mock<IGamePathService> _mockGamePathService;
	private readonly Mock<IGameFileService> _mockGameFileService;
	private readonly UserSettings _userSettings;
	private readonly StashService _stashService;

	/// <summary>
	/// Initializes test dependencies and StashService instance
	/// </summary>
	public StashServiceTests()
	{
		_mockLogger = new Mock<ILogger<StashService>>();
		_mockItemDatabaseService = new Mock<IItemDatabaseService>();
		_mockStashProvider = new Mock<IStashProvider>();
		_mockGamePathService = new Mock<IGamePathService>();
		_mockGameFileService = new Mock<IGameFileService>();
		_userSettings = new UserSettings();

		// Create SessionContext - parameterless data holder
		_sessionContext = new SessionContext();

		_stashService = new StashService(
			_mockLogger.Object,
			_sessionContext,
			_mockItemDatabaseService.Object,
			_mockStashProvider.Object,
			_mockGamePathService.Object,
			_mockGameFileService.Object,
			_userSettings
		);
	}

	/// <summary>
	/// Test LoadPlayerStash with null selectedSave returns empty result
	/// </summary>
	[Fact]
	public void LoadPlayerStash_WithNullSelectedSave_ReturnsEmptyResult()
	{
		// Arrange
		PlayerSave? nullSave = null;

		// Act
		var result = _stashService.LoadPlayerStash(nullSave);

		// Assert
		result.Should().NotBeNull();
		result.Stash.Should().BeNull();
		result.StashFile.Should().BeNull();
	}

	/// <summary>
	/// Test LoadPlayerStash with valid player save loads stash successfully
	/// </summary>
	[Fact]
	public void LoadPlayerStash_WithValidPlayerSave_LoadsStashSuccessfully()
	{
		// Arrange
		var playerName = "TestPlayer";
		var stashFile = """C:\Test\Saves\TestPlayer\winsys.dxb""";
		var mockPathIO = new Mock<IPathIO>();
		var mockTranslationService = new Mock<ITranslationService>();
		mockPathIO.Setup(x => x.GetFileName("""C:\Test\Saves\_TestPlayer""")).Returns("_TestPlayer");
		var playerSave = new PlayerSave("""C:\Test\Saves\_TestPlayer""", true, false, false, "", mockTranslationService.Object, mockPathIO.Object);

		_mockGamePathService.Setup(x => x.GetPlayerStashFile(playerName, false)).Returns(stashFile);
		_mockStashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Callback<Stash>(s => s.CreateEmptySack()).Returns(true);

		// Act
		var result = _stashService.LoadPlayerStash(playerSave);

		// Assert
		result.Should().NotBeNull();
		result.StashFile.Should().Be(stashFile);
		result.Stash.Should().NotBeNull();
		result.Stash.StashFound.Should().BeTrue();
		result.Stash.PlayerName.Should().Be("TestPlayer - Immortal Throne");
	}

	/// <summary>
	/// Test LoadPlayerStash handles ArgumentException from StashProvider
	/// </summary>
	[Fact]
	public void LoadPlayerStash_WithLoadException_CapturesException()
	{
		// Arrange
		var playerName = "TestPlayer";
		var stashFile = """C:\Test\Saves\TestPlayer\winsys.dxb""";
		var mockPathIO = new Mock<IPathIO>();
		var mockTranslationService = new Mock<ITranslationService>();
		mockPathIO.Setup(x => x.GetFileName("""C:\Test\Saves\_TestPlayer""")).Returns("_TestPlayer");
		var playerSave = new PlayerSave("""C:\Test\Saves\_TestPlayer""", true, false, false, "", mockTranslationService.Object, mockPathIO.Object);
		var expectedException = new ArgumentException("Invalid stash format");

		_mockGamePathService.Setup(x => x.GetPlayerStashFile(playerName, false)).Returns(stashFile);
		_mockStashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Callback<Stash>(s => s.CreateEmptySack()).Throws(expectedException);

		// Act
		var result = _stashService.LoadPlayerStash(playerSave);

		// Assert
		result.Should().NotBeNull();
		result.Stash.Should().NotBeNull();
		result.Stash.ArgumentException.Should().Be(expectedException);
		result.Stash.StashFound.Should().BeNull();
	}

	/// <summary>
	/// Test LoadPlayerStash with fromFileWatcher true updates existing stash
	/// </summary>
	[Fact]
	public void LoadPlayerStash_WithFromFileWatcherTrue_UpdatesExistingStash()
	{
		// Arrange
		var playerName = "TestPlayer";
		var stashFile = """C:\Test\Saves\TestPlayer\winsys.dxb""";
		var mockPathIO = new Mock<IPathIO>();
		var mockTranslationService = new Mock<ITranslationService>();
		mockPathIO.Setup(x => x.GetFileName("""C:\Test\Saves\_TestPlayer""")).Returns("_TestPlayer");
		var playerSave = new PlayerSave("""C:\Test\Saves\_TestPlayer""", true, false, false, "", mockTranslationService.Object, mockPathIO.Object);

		_mockGamePathService.Setup(x => x.GetPlayerStashFile(playerName, false)).Returns(stashFile);
		_mockStashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Callback<Stash>(s => s.CreateEmptySack()).Returns(true);

		// First load
		var result1 = _stashService.LoadPlayerStash(playerSave, false);
		var originalStash = result1.Stash;

		// Second load with fromFileWatcher
		var result2 = _stashService.LoadPlayerStash(playerSave, true);

		// Assert
		result2.Should().NotBeNull();
		result2.Stash.Should().NotBeSameAs(originalStash); // Should be updated
		result2.StashFile.Should().Be(stashFile);
	}

	private static PlayerSave CreateSave(bool isImmortalThrone)
	{
		var mockPathIO = new Mock<IPathIO>();
		mockPathIO.Setup(x => x.GetFileName("""C:\Test\Saves\_TestPlayer""")).Returns("_TestPlayer");
		return new PlayerSave("""C:\Test\Saves\_TestPlayer""", isImmortalThrone, false, false, "", new Mock<ITranslationService>().Object, mockPathIO.Object);
	}

	[Fact]
	public void SharedStashLoads_TitanQuestOriginalCharacter_LoadNothing()
	{
		var save = CreateSave(isImmortalThrone: false);

		this._stashService.LoadCharacterStash(save).Should().BeNull();
		this._stashService.LoadTransferStashFor(save).Should().BeNull();
		this._stashService.LoadRelicVaultStashFor(save).Should().BeNull();

		this._mockStashProvider.Verify(x => x.LoadFile(It.IsAny<Stash>()), Times.Never);
		this._sessionContext.Stashes.Should().BeEmpty();
	}

	[Fact]
	public void LoadCharacterStash_ImmortalThroneCharacter_LoadsItsStash()
	{
		var stashFile = """C:\Test\Saves\TestPlayer\winsys.dxb""";
		this._mockGamePathService.Setup(x => x.GetPlayerStashFile("TestPlayer", false)).Returns(stashFile);
		this._mockStashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Returns(true);

		var result = this._stashService.LoadCharacterStash(CreateSave(isImmortalThrone: true));

		result.Should().NotBeNull();
		result!.StashFile.Should().Be(stashFile);
		result.Stash.Should().NotBeNull();
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void SharedStashLoads_ImmortalThroneOrNoCharacter_LoadTheSharedFiles(bool withCharacter)
	{
		var transferFile = """C:\Test\Saves\Sys\winsys.dxb""";
		var relicFile = """C:\Test\Saves\Sys\miscsys.dxb""";
		this._mockGamePathService.Setup(x => x.TransferStashFileFullPath).Returns(transferFile);
		this._mockGamePathService.Setup(x => x.RelicVaultStashFileFullPath).Returns(relicFile);
		this._mockStashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Returns(true);
		var save = withCharacter ? CreateSave(isImmortalThrone: true) : null;

		var transfer = this._stashService.LoadTransferStashFor(save);
		var relic = this._stashService.LoadRelicVaultStashFor(save);

		transfer!.StashFile.Should().Be(transferFile);
		relic!.StashFile.Should().Be(relicFile);
		this._sessionContext.Stashes.Keys.Should().BeEquivalentTo([transferFile, relicFile]);
	}

	/// <summary>
	/// Test LoadTransferStash loads transfer stash successfully
	/// </summary>
	[Fact]
	public void LoadTransferStash_LoadsSuccessfully()
	{
		// Arrange
		var transferStashFile = """C:\Test\Saves\Sys\winsys.dxb""";

		_mockGamePathService.Setup(x => x.TransferStashFileFullPath).Returns(transferStashFile);
		_mockStashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Callback<Stash>(s => s.CreateEmptySack()).Returns(true);

		// Act
		var result = _stashService.LoadTransferStash();

		// Assert
		result.Should().NotBeNull();
		result.StashFile.Should().Be(transferStashFile);
		result.Stash.Should().NotBeNull();
		result.Stash.StashFound.Should().BeTrue();
		result.Stash.IsImmortalThrone.Should().BeTrue();
	}

	/// <summary>
	/// Test LoadTransferStash handles ArgumentException from StashProvider
	/// </summary>
	[Fact]
	public void LoadTransferStash_WithLoadException_CapturesException()
	{
		// Arrange
		var transferStashFile = """C:\Test\Saves\Sys\winsys.dxb""";
		var expectedException = new ArgumentException("Invalid transfer stash format");

		_mockGamePathService.Setup(x => x.TransferStashFileFullPath).Returns(transferStashFile);
		_mockStashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Callback<Stash>(s => s.CreateEmptySack()).Throws(expectedException);

		// Act
		var result = _stashService.LoadTransferStash();

		// Assert
		result.Should().NotBeNull();
		result.Stash.Should().NotBeNull();
		result.Stash.ArgumentException.Should().Be(expectedException);
		result.Stash.StashFound.Should().BeNull();
	}

	/// <summary>
	/// Test LoadRelicVaultStash loads relic vault stash successfully
	/// </summary>
	[Fact]
	public void LoadRelicVaultStash_LoadsSuccessfully()
	{
		// Arrange
		var relicVaultStashFile = """C:\Test\Saves\Sys\miscsys.dxb""";

		_mockGamePathService.Setup(x => x.RelicVaultStashFileFullPath).Returns(relicVaultStashFile);
		_mockStashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Callback<Stash>(s => s.CreateEmptySack()).Returns(true);

		// Act
		var result = _stashService.LoadRelicVaultStash();

		// Assert
		result.Should().NotBeNull();
		result.StashFile.Should().Be(relicVaultStashFile);
		result.Stash.Should().NotBeNull();
		result.Stash.StashFound.Should().BeTrue();
		result.Stash.IsImmortalThrone.Should().BeTrue();
		result.Stash.Sack!.StashType.Should().Be(StashType.RelicVaultStash);
	}

	/// <summary>
	/// Test LoadRelicVaultStash handles ArgumentException from StashProvider
	/// </summary>
	[Fact]
	public void LoadRelicVaultStash_WithLoadException_CapturesException()
	{
		// Arrange
		var relicVaultStashFile = """C:\Test\Saves\Sys\miscsys.dxb""";
		var expectedException = new ArgumentException("Invalid relic vault format");

		_mockGamePathService.Setup(x => x.RelicVaultStashFileFullPath).Returns(relicVaultStashFile);
		_mockStashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Callback<Stash>(s => s.CreateEmptySack()).Throws(expectedException);

		// Act
		var result = _stashService.LoadRelicVaultStash();

		// Assert
		result.Should().NotBeNull();
		result.Stash.Should().NotBeNull();
		result.Stash.ArgumentException.Should().Be(expectedException);
		result.Stash.StashFound.Should().BeNull();
	}

	/// <summary>
	/// Test SaveAllModifiedStashes with no modified stashes returns zero
	/// </summary>
	[Fact]
	public void SaveAllModifiedStashes_WithNoModifiedStashes_ReturnsZero()
	{
		// Arrange
		var stash = new Stash("TestPlayer", """C:\Test\Saves\TestPlayer\winsys.dxb""")
		{
			IsImmortalThrone = true
		};
		stash.CreateEmptySack();

		_sessionContext.Stashes.GetOrAddAtomic("TestPlayer.dxb", _ => stash);

		// Act
		var result = this.SaveAllModifiedStashes();

		// Assert
		result.Should().Be(0);
		_mockStashProvider.Verify(x => x.Save(It.IsAny<Stash>(), It.IsAny<string>()), Times.Never);
	}

	/// <summary>
	/// Test SaveAllModifiedStashes with modified stashes saves successfully
	/// </summary>
	[Fact]
	public void SaveAllModifiedStashes_WithModifiedStashes_SavesSuccessfully()
	{
		// Arrange
		var stash = new Stash("TestPlayer", """C:\Test\Saves\TestPlayer\winsys.dxb""")
		{
			IsImmortalThrone = true
		};
		stash.CreateEmptySack();
		stash.Sack.IsModified = true;

		_sessionContext.Stashes.GetOrAddAtomic("TestPlayer.dxb", _ => stash);

		_userSettings.DisableLegacyBackup = true;

		try
		{
			// Act
			var result = this.SaveAllModifiedStashes();

			// Assert
			result.Should().Be(1);
			_mockGameFileService.Verify(x => x.BackupFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			_mockStashProvider.Verify(x => x.Save(stash, "TestPlayer.dxb"), Times.Once);
			stash.IsModified.Should().BeFalse();
		}
		finally
		{
			_userSettings.DisableLegacyBackup = false;
		}
	}

	/// <summary>
	/// Test SaveAllModifiedStashes with backup enabled calls backup and save
	/// </summary>
	[Fact]
	public void SaveAllModifiedStashes_WithBackupEnabled_CallsBackupAndSave()
	{
		// Arrange
		var stash = new Stash("TestPlayer", """C:\Test\Saves\TestPlayer\winsys.dxb""")
		{
			IsImmortalThrone = true
		};
		stash.CreateEmptySack();
		stash.Sack.IsModified = true;

		_sessionContext.Stashes.GetOrAddAtomic("TestPlayer.dxb", _ => stash);

		_userSettings.DisableLegacyBackup = false;

		// Act
		var result = this.SaveAllModifiedStashes();

		// Assert
		result.Should().Be(1);
		_mockGameFileService.Verify(x => x.BackupFile("TestPlayer - Immortal Throne", "TestPlayer.dxb"), Times.Once);
		_mockStashProvider.Verify(x => x.Save(stash, "TestPlayer.dxb"), Times.Once);
		stash.IsModified.Should().BeFalse();
	}

	/// <summary>
	/// Test SaveAllModifiedStashes with multiple modified stashes saves all
	/// </summary>
	[Fact]
	public void SaveAllModifiedStashes_WithMultipleModifiedStashes_SavesAll()
	{
		// Arrange
		var stash1 = new Stash("TestPlayer1", """C:\Test\Saves\TestPlayer1\winsys.dxb""")
		{
			IsImmortalThrone = true
		};
		stash1.CreateEmptySack();
		stash1.Sack.IsModified = true;

		var stash2 = new Stash("TestPlayer2", """C:\Test\Saves\TestPlayer2\winsys.dxb""")
		{
			IsImmortalThrone = true
		};
		stash2.CreateEmptySack();
		stash2.Sack.IsModified = true;

		_sessionContext.Stashes.GetOrAddAtomic("TestPlayer1.dxb", _ => stash1);
		_sessionContext.Stashes.GetOrAddAtomic("TestPlayer2.dxb", _ => stash2);

		_userSettings.DisableLegacyBackup = true;

		try
		{
			// Act
			var result = this.SaveAllModifiedStashes();

			// Assert
			result.Should().Be(2);
			_mockStashProvider.Verify(x => x.Save(stash1, "TestPlayer1.dxb"), Times.Once);
			_mockStashProvider.Verify(x => x.Save(stash2, "TestPlayer2.dxb"), Times.Once);
			stash1.IsModified.Should().BeFalse();
			stash2.IsModified.Should().BeFalse();
		}
		finally
		{
			_userSettings.DisableLegacyBackup = false;
		}
	}

	/// <summary>
	/// Test SaveAllModifiedStashes with null stash in collection handles gracefully
	/// </summary>
	[Fact]
	public void SaveAllModifiedStashes_WithNullStash_HandlesGracefully()
	{
		// Arrange
		_sessionContext.Stashes.GetOrAddAtomic("TestPlayer.dxb", _ => null!);

		// Act
		var result = this.SaveAllModifiedStashes();

		// Assert
		result.Should().Be(0);
	}

	/// <summary>
	/// Test LoadTransferStash with fromFileWatcher updates existing stash
	/// </summary>
	[Fact]
	public void LoadTransferStash_WithFromFileWatcherTrue_UpdatesExistingStash()
	{
		// Arrange
		var transferStashFile = """C:\Test\Saves\Sys\winsys.dxb""";

		_mockGamePathService.Setup(x => x.TransferStashFileFullPath).Returns(transferStashFile);
		_mockStashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Callback<Stash>(s => s.CreateEmptySack()).Returns(true);

		// First load
		var result1 = _stashService.LoadTransferStash(false);
		var originalStash = result1.Stash;

		// Second load with fromFileWatcher
		var result2 = _stashService.LoadTransferStash(true);

		// Assert
		result2.Should().NotBeNull();
		result2.Stash.Should().NotBeSameAs(originalStash);
	}

	/// <summary>
	/// Test LoadRelicVaultStash with fromFileWatcher updates existing stash
	/// </summary>
	[Fact]
	public void LoadRelicVaultStash_WithFromFileWatcherTrue_UpdatesExistingStash()
	{
		// Arrange
		var relicVaultStashFile = """C:\Test\Saves\Sys\miscsys.dxb""";

		_mockGamePathService.Setup(x => x.RelicVaultStashFileFullPath).Returns(relicVaultStashFile);
		_mockStashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Callback<Stash>(s => s.CreateEmptySack()).Returns(true);

		// First load
		var result1 = _stashService.LoadRelicVaultStash(false);
		var originalStash = result1.Stash;

		// Second load with fromFileWatcher
		var result2 = _stashService.LoadRelicVaultStash(true);

		// Assert
		result2.Should().NotBeNull();
		result2.Stash.Should().NotBeSameAs(originalStash);
	}

	/// <summary>
	/// Test SaveAllModifiedStashes with exception during save
	/// </summary>
	[Fact]
	public void SaveAllModifiedStashes_WithExceptionDuringSave_ThrowsException()
	{
		// Arrange
		var stash = new Stash("TestPlayer", """C:\Test\Saves\TestPlayer\winsys.dxb""")
		{
			IsImmortalThrone = true
		};
		stash.CreateEmptySack();
		stash.Sack.IsModified = true;

		_sessionContext.Stashes.GetOrAddAtomic("TestPlayer.dxb", _ => stash);

		_mockStashProvider.Setup(x => x.Save(It.IsAny<Stash>(), It.IsAny<string>()))
			.Throws(new IOException("Disk full"));

		_userSettings.DisableLegacyBackup = true;

		// Act & Assert
		var act = () => this.SaveAllModifiedStashes();
		act.Should().Throw<IOException>();
		stash.IsModified.Should().BeTrue("a failed save keeps the stash modified, so Retry and the next save write it again");
	}

	/// <summary>
	/// Test LoadPlayerStash with archived player uses correct path
	/// </summary>
	[Fact]
	public void LoadPlayerStash_WithArchivedPlayer_UsesArchivedPath()
	{
		// Arrange
		var playerName = "TestPlayer";
		var stashFile = """C:\Test\Saves\TestPlayer\winsys.dxb""";
		var mockPathIO = new Mock<IPathIO>();
		var mockTranslationService = new Mock<ITranslationService>();
		mockPathIO.Setup(x => x.GetFileName("""C:\Test\Saves\_TestPlayer""")).Returns("_TestPlayer");
		var playerSave = new PlayerSave("""C:\Test\Saves\_TestPlayer""", false, true, false, "", mockTranslationService.Object, mockPathIO.Object); // IsArchived = true

		_mockGamePathService.Setup(x => x.GetPlayerStashFile(playerName, true)).Returns(stashFile); // IsArchived = true
		_mockStashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Callback<Stash>(s => s.CreateEmptySack()).Returns(true);

		// Act
		var result = _stashService.LoadPlayerStash(playerSave);

		// Assert
		result.Should().NotBeNull();
		_mockGamePathService.Verify(x => x.GetPlayerStashFile(playerName, true), Times.Once);
	}

	[Theory]
	[InlineData(-5, BagIdConstants.BAGID_EQUIPMENTPANEL)]
	[InlineData(-1, BagIdConstants.BAGID_EQUIPMENTPANEL)]
	[InlineData(BagIdConstants.BAGID_TRANSFERSTASH, BagIdConstants.BAGID_TRANSFERSTASH)]
	[InlineData(BagIdConstants.BAGID_RELICVAULTSTASH, BagIdConstants.BAGID_RELICVAULTSTASH)]
	[InlineData(4, BagIdConstants.BAGID_RELICVAULTSTASH)]
	[InlineData(12, BagIdConstants.BAGID_RELICVAULTSTASH)]
	public void ClampBagId_KeepsTheIdWithinTheStashPanelBags(int bagId, int expected)
	{
		_stashService.ClampBagId(bagId).Should().Be(expected);
	}

	private static Stash CreateStashWithSack(string name)
	{
		var stash = new Stash(name, $"""C:\Saves\{name}.dxb""");
		stash.CreateEmptySack();
		return stash;
	}

	[Fact]
	public void GetBagSack_ReturnsTheSackOfEachBag()
	{
		var player = new PlayerCollection("Hero", """C:\Saves\_Hero\Player.chr""") { EquipmentSack = new SackCollection() };
		var transfer = CreateStashWithSack("transfer");
		var stash = CreateStashWithSack("stash");
		var relic = CreateStashWithSack("relic");

		_stashService.GetBagSack(BagIdConstants.BAGID_EQUIPMENTPANEL, player, transfer, stash, relic).Should().BeSameAs(player.EquipmentSack);
		_stashService.GetBagSack(BagIdConstants.BAGID_TRANSFERSTASH, player, transfer, stash, relic).Should().BeSameAs(transfer.Sack);
		_stashService.GetBagSack(BagIdConstants.BAGID_PLAYERSTASH, player, transfer, stash, relic).Should().BeSameAs(stash.Sack);
		_stashService.GetBagSack(BagIdConstants.BAGID_RELICVAULTSTASH, player, transfer, stash, relic).Should().BeSameAs(relic.Sack);
	}

	[Fact]
	public void GetBagSack_OutOfRangeId_GivesTheClampedBag()
	{
		var relic = CreateStashWithSack("relic");

		_stashService.GetBagSack(7, null, null, null, relic).Should().BeSameAs(relic.Sack);
	}

	[Fact]
	public void GetBagSack_StashWithoutSacksOrMissing_GivesNull()
	{
		var emptyStash = CreateStashWithSack("stash");
		emptyStash.numberOfSacks = 0;

		_stashService.GetBagSack(BagIdConstants.BAGID_PLAYERSTASH, null, null, emptyStash, null).Should().BeNull();
		_stashService.GetBagSack(BagIdConstants.BAGID_TRANSFERSTASH, null, null, emptyStash, null).Should().BeNull();
		_stashService.GetBagSack(BagIdConstants.BAGID_EQUIPMENTPANEL, null, null, emptyStash, null).Should().BeNull();
	}

	private static Stash CreateStashWithSack(string name, int width, int height)
	{
		var stash = CreateStashWithSack(name);
		stash.Width = width;
		stash.Height = height;
		return stash;
	}

	[Fact]
	public void GetBag_StashBags_GiveTheirSackAndSize()
	{
		var transfer = CreateStashWithSack("transfer", 8, 16);
		var stash = CreateStashWithSack("stash", 10, 18);
		var relic = CreateStashWithSack("relic", 12, 14);

		this._stashService.GetBag(BagIdConstants.BAGID_TRANSFERSTASH, null, transfer, stash, relic)
			.Should().Be(new StashBag(BagIdConstants.BAGID_TRANSFERSTASH, transfer.Sack, 8, 16, 16));
		this._stashService.GetBag(BagIdConstants.BAGID_PLAYERSTASH, null, transfer, stash, relic)
			.Should().Be(new StashBag(BagIdConstants.BAGID_PLAYERSTASH, stash.Sack, 10, 18, 18));
		this._stashService.GetBag(BagIdConstants.BAGID_RELICVAULTSTASH, null, transfer, stash, relic)
			.Should().Be(new StashBag(BagIdConstants.BAGID_RELICVAULTSTASH, relic.Sack, 12, 14, 14));
	}

	[Fact]
	public void GetBag_SmallPlayerStash_IsCenteredAsIfFifteenCellsHigh()
	{
		var smallStash = CreateStashWithSack("stash", 10, 5);
		var smallTransfer = CreateStashWithSack("transfer", 10, 5);

		var stashBag = this._stashService.GetBag(BagIdConstants.BAGID_PLAYERSTASH, null, null, smallStash, null)!;
		var transferBag = this._stashService.GetBag(BagIdConstants.BAGID_TRANSFERSTASH, null, smallTransfer, null, null)!;

		stashBag.Height.Should().Be(5);
		stashBag.CenteringHeight.Should().Be(15);
		stashBag.CenteringMargin(18, 20).Should().Be((8, 5));
		transferBag.CenteringMargin(18, 20).Should().Be((8, 15));
	}

	[Fact]
	public void GetBag_StashNotLoaded_GivesNull()
	{
		var stash = CreateStashWithSack("stash", 10, 18);

		this._stashService.GetBag(BagIdConstants.BAGID_TRANSFERSTASH, null, null, stash, null).Should().BeNull();
		this._stashService.GetBag(BagIdConstants.BAGID_RELICVAULTSTASH, null, null, stash, null).Should().BeNull();
	}

	[Fact]
	public void GetBag_Equipment_IsDescribedEvenWithoutCharacter()
	{
		var player = new PlayerCollection("Hero", """C:\Saves\_Hero\Player.chr""") { EquipmentSack = new SackCollection() };

		var withPlayer = this._stashService.GetBag(BagIdConstants.BAGID_EQUIPMENTPANEL, player, null, null, null)!;
		var withoutPlayer = this._stashService.GetBag(BagIdConstants.BAGID_EQUIPMENTPANEL, null, null, null, null)!;

		withPlayer.IsEquipment.Should().BeTrue();
		withPlayer.Sack.Should().BeSameAs(player.EquipmentSack);
		withoutPlayer.IsEquipment.Should().BeTrue();
		withoutPlayer.Sack.Should().BeNull();
	}

	[Theory]
	[InlineData(-3, BagIdConstants.BAGID_EQUIPMENTPANEL)]
	[InlineData(9, BagIdConstants.BAGID_RELICVAULTSTASH)]
	public void GetBag_OutOfRangeId_DescribesTheClampedBag(int bagId, int expected)
	{
		var relic = CreateStashWithSack("relic", 12, 14);

		this._stashService.GetBag(bagId, null, null, null, relic)!.BagId.Should().Be(expected);
	}

	[Fact]
	public void CenteringMargin_SackLargerThanPanel_IsZero()
	{
		var bag = new StashBag(BagIdConstants.BAGID_TRANSFERSTASH, null, 20, 25, 25);

		bag.CenteringMargin(18, 20).Should().Be((0, 0));
	}

	/// <summary>
	/// Saves every modified file the way the save orchestrator does, one by one.
	/// </summary>
	private int SaveAllModifiedStashes()
	{
		var modified = this._stashService.GetModifiedStashes();
		foreach (var (file, stash) in modified)
			this._stashService.SaveModifiedStash(file, stash);

		return modified.Count;
	}
}
