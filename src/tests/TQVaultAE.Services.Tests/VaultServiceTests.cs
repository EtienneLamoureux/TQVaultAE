using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Unit tests for VaultService class
/// </summary>
public class VaultServiceTests
{
	private readonly Mock<ILogger<VaultService>> _mockLogger;
	private readonly SessionContext _sessionContext;
	private readonly Mock<IItemDatabaseService> _mockItemDatabaseService;
	private readonly Mock<IPlayerCollectionProvider> _mockPlayerCollectionProvider;
	private readonly Mock<IGameFileService> _mockGameFileService;
	private readonly Mock<IGamePathService> _mockGamePathService;
	private readonly Mock<IFileIO> _mockFileIO;
	private readonly Mock<IPathIO> _mockPathIO;
	private readonly UserSettings _userSettings;
	private readonly Mock<IItemProvider> _mockItemProvider;
	private readonly VaultService _vaultService;

	/// <summary>
	/// Initializes test dependencies and VaultService instance
	/// </summary>
	public VaultServiceTests()
	{
		_mockLogger = new Mock<ILogger<VaultService>>();
		_mockItemDatabaseService = new Mock<IItemDatabaseService>();
		_mockPlayerCollectionProvider = new Mock<IPlayerCollectionProvider>();
		_mockGameFileService = new Mock<IGameFileService>();
		_mockGamePathService = new Mock<IGamePathService>();
		_mockFileIO = new Mock<IFileIO>();
		_mockPathIO = new Mock<IPathIO>();
		_userSettings = new UserSettings();
		_mockItemProvider = new Mock<IItemProvider>();

		// Create SessionContext - parameterless data holder
		_sessionContext = new SessionContext();

		_vaultService = new VaultService(
			_mockLogger.Object,
			_sessionContext,
			_mockItemDatabaseService.Object,
			_mockPlayerCollectionProvider.Object,
			_mockGamePathService.Object,
			_mockGameFileService.Object,
			_mockFileIO.Object,
			_mockPathIO.Object,
			_userSettings,
			_mockItemProvider.Object
		);
	}

	/// <summary>
	/// Test CreateVault method with valid parameters creates a new vault with empty sacks
	/// </summary>
	[Fact]
	public void CreateVault_WithNewFile_CreatesVaultWithEmptySacks()
	{
		// Arrange
		var vaultName = "TestVault";
		var vaultFile = """C:\Test\Vaults\TestVault.vault""";
		var oldFormatFile = """C:\Test\Vaults\TestVault""";

		_mockFileIO.Setup(x => x.Exists(oldFormatFile)).Returns(false);

		// Act
		var result = _vaultService.CreateVault(vaultName, vaultFile);

		// Assert
		result.Should().NotBeNull();
		result.PlayerName.Should().Be(vaultName);
		result.PlayerFile.Should().Be(vaultFile);
		result.IsVault.Should().BeTrue();
		result.Sacks.Should().NotBeNull();
		result.Sacks.Length.Should().Be(12);
	}

	/// <summary>
	/// Test CreateVault method when old format file exists converts it
	/// </summary>
	[Fact]
	public void CreateVault_WhenOldFileExists_LoadsOldFormat()
	{
		// Arrange
		var vaultName = "TestVault";
		var vaultFile = """C:\Test\Vaults\TestVault.vault""";
		var oldFormatFile = """C:\Test\Vaults\TestVault""";

		_mockPathIO.Setup(x => x.GetDirectoryName(vaultFile)).Returns("""C:\Test\Vaults""");
		_mockPathIO.Setup(x => x.GetFileNameWithoutExtension(vaultFile)).Returns("TestVault");
		_mockPathIO.Setup(x => x.Combine("""C:\Test\Vaults""", "TestVault")).Returns(oldFormatFile);
		_mockFileIO.Setup(x => x.Exists(oldFormatFile)).Returns(true);
		_mockPlayerCollectionProvider
			.Setup(x => x.LoadFile(It.IsAny<PlayerCollection>(), oldFormatFile))
			.Callback<PlayerCollection, string>((vault, _) =>
			{
				// Simulate LoadFile creating sacks array
				var vaultType = vault.GetType();
				var sacksField = vaultType.GetField("Sacks", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
				sacksField?.SetValue(vault, Array.Empty<SackCollection>());
			});

		// Act
		var result = _vaultService.CreateVault(vaultName, vaultFile);

		// Assert
		result.Should().NotBeNull();
		result.PlayerName.Should().Be(vaultName);
		result.PlayerFile.Should().Be(vaultFile);
		result.IsVault.Should().BeTrue();
		_mockPlayerCollectionProvider.Verify(x => x.LoadFile(It.IsAny<PlayerCollection>(), oldFormatFile), Times.Once);
	}

	/// <summary>
	/// Test SaveAllModifiedVaults with no modified vaults returns zero
	/// </summary>
	[Fact]
	public void SaveAllModifiedVaults_WithNoModifiedVaults_ReturnsZero()
	{
		// Arrange
		var vault = new PlayerCollection("TestVault", """C:\Test\Vaults\TestVault.vault""")
		{
			IsVault = true
		};

		_sessionContext.Vaults.GetOrAddAtomic("TestVault.vault", _ => vault);

		// Act
		var result = this.SaveAllModifiedVaults();

		// Assert
		result.Should().Be(0);
		_mockPlayerCollectionProvider.Verify(x => x.Save(It.IsAny<PlayerCollection>(), It.IsAny<string>()), Times.Never);
	}

	/// <summary>
	/// Test SaveAllModifiedVaults with modified vaults and backup disabled saves successfully
	/// </summary>
	[Fact]
	public void SaveAllModifiedVaults_WithModifiedVaults_SavesSuccessfully()
	{
		// Arrange
		var vault = new PlayerCollection("TestVault", """C:\Test\Vaults\TestVault.vault""")
		{
			IsVault = true
		};
		// Initialize sacks using CreateEmptySacks and mark one as modified
		vault.CreateEmptySacks(1);
		vault.Sacks[0].IsModified = true;

		_sessionContext.Vaults.GetOrAddAtomic("TestVault.vault", _ => vault);

		_userSettings.DisableLegacyBackup = true;

		try
		{
			// Act
			var result = this.SaveAllModifiedVaults();

		// Assert
		result.Should().Be(1);
		_mockGameFileService.Verify(x => x.BackupFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		_mockPlayerCollectionProvider.Verify(x => x.Save(vault, "TestVault.vault"), Times.Once);
		vault.IsModified.Should().BeFalse();
		}
		finally
		{
			_userSettings.DisableLegacyBackup = false;
		}
	}

	/// <summary>
	/// Test SaveAllModifiedVaults with modified vaults and backup enabled calls backup
	/// </summary>
	[Fact]
	public void SaveAllModifiedVaults_WithBackupEnabled_CallsBackupAndSave()
	{
		// Arrange
		var vault = new PlayerCollection("TestVault", """C:\Test\Vaults\TestVault.vault""")
		{
			IsVault = true
		};
		// Need to initialize Sacks array first
		var sackCollection = new SackCollection();
		var sacksField = sackCollection.GetType().GetProperty("IsModified");
		sacksField?.SetValue(sackCollection, true);
		vault.Sacks = [sackCollection];

		_sessionContext.Vaults.GetOrAddAtomic("TestVault.vault", _ => vault);

		_userSettings.DisableLegacyBackup = false;

		// Act
		var result = this.SaveAllModifiedVaults();

		// Assert
		result.Should().Be(1);
		_mockGameFileService.Verify(x => x.BackupFile("TestVault", "TestVault.vault"), Times.Once);
		_mockPlayerCollectionProvider.Verify(x => x.Save(vault, "TestVault.vault"), Times.Once);
		vault.IsModified.Should().BeFalse();
	}

	/// <summary>
	/// Test LoadVault with existing vault file loads successfully
	/// </summary>
	[Fact]
	public void LoadVault_WithExistingFile_LoadsSuccessfully()
	{
		// Arrange
		var vaultName = "TestVault";
		var vaultFile = """C:\Test\Vaults\TestVault.vault""";

		_mockGamePathService.Setup(x => x.GetVaultFile(vaultName)).Returns(vaultFile);
		_mockFileIO.Setup(x => x.Exists(vaultFile)).Returns(true);
		_mockPlayerCollectionProvider.Setup(x => x.LoadFile(It.IsAny<PlayerCollection>(), vaultFile));

		// Act
		var result = _vaultService.LoadVault(vaultName);

		// Assert
		result.Should().NotBeNull();
		result.Vault.Should().NotBeNull();
		result.VaultLoaded.Should().BeTrue();
		result.Filename.Should().Be(vaultFile);
		result.Vault.IsVault.Should().BeTrue();
	}

	/// <summary>
	/// Test LoadVault with non-existent vault file creates new vault
	/// </summary>
	[Fact]
	public void LoadVault_WithNonExistentFile_CreatesNewVault()
	{
		// Arrange
		var vaultName = "TestVault";
		var vaultFile = """C:\Test\Vaults\TestVault.vault""";

		_mockGamePathService.Setup(x => x.GetVaultFile(vaultName)).Returns(vaultFile);
		_mockFileIO.Setup(x => x.Exists(vaultFile)).Returns(false);
		_mockFileIO.Setup(x => x.Exists("""C:\Test\Vaults\TestVault""")).Returns(false);

		// Act
		var result = _vaultService.LoadVault(vaultName);

		// Assert
		result.Should().NotBeNull();
		result.Vault.Should().NotBeNull();
		result.VaultLoaded.Should().BeTrue();
		result.Filename.Should().Be(vaultFile);
		result.Vault.IsVault.Should().BeTrue();
		result.Vault.Sacks.Should().NotBeNull();
		result.Vault.Sacks.Length.Should().Be(12);
	}

	/// <summary>
	/// Test LoadVault caches vault in SessionContext
	/// </summary>
	[Fact]
	public void LoadVault_CachesVaultInSessionContext()
	{
		// Arrange
		var vaultName = "TestVault";
		var vaultFile = """C:\Test\Vaults\TestVault.vault""";

		_mockGamePathService.Setup(x => x.GetVaultFile(vaultName)).Returns(vaultFile);
		_mockFileIO.Setup(x => x.Exists(vaultFile)).Returns(true);
		_mockPlayerCollectionProvider.Setup(x => x.LoadFile(It.IsAny<PlayerCollection>(), vaultFile));

		// Act
		var result1 = _vaultService.LoadVault(vaultName);
		var result2 = _vaultService.LoadVault(vaultName);

		// Assert
		result1.Should().NotBeNull();
		result2.Should().NotBeNull();
		result1.Vault.Should().BeSameAs(result2.Vault);
		_mockPlayerCollectionProvider.Verify(x => x.LoadFile(It.IsAny<PlayerCollection>(), vaultFile), Times.Once);
	}

	/// <summary>
	/// Test LoadVault with null/empty vault name returns null
	/// </summary>
	[Fact]
	public void LoadVault_WithNullOrEmptyName_DoesNotThrow()
	{
		// Arrange - mock GetVaultFile to return a valid path even for null name
		var vaultFile = """C:\Test\Vaults\TestVault.vault""";
		_mockGamePathService.Setup(x => x.GetVaultFile(null!)).Returns(vaultFile);
		_mockGamePathService.Setup(x => x.GetVaultFile("")).Returns(vaultFile);
		_mockFileIO.Setup(x => x.Exists(vaultFile)).Returns(false);
		_mockFileIO.Setup(x => x.Exists("""C:\Test\Vaults\TestVault""")).Returns(false);

		// Act & Assert
		_vaultService.LoadVault(null!).Should().NotBeNull();
		_vaultService.LoadVault("").Should().NotBeNull();
	}

	/// <summary>
	/// Test LoadVault handles ArgumentException from PlayerCollectionProvider
	/// </summary>
	[Fact]
	public void LoadVault_WithLoadException_CapturesException()
	{
		// Arrange
		var vaultName = "TestVault";
		var vaultFile = """C:\Test\Vaults\TestVault.vault""";
		var expectedException = new ArgumentException("Invalid file format");

		_mockGamePathService.Setup(x => x.GetVaultFile(vaultName)).Returns(vaultFile);
		_mockFileIO.Setup(x => x.Exists(vaultFile)).Returns(true);
		_mockPlayerCollectionProvider.Setup(x => x.LoadFile(It.IsAny<PlayerCollection>(), vaultFile)).Throws(expectedException);

		// Act
		var result = _vaultService.LoadVault(vaultName);

		// Assert
		result.Should().NotBeNull();
		result.Vault.Should().NotBeNull();
		result.VaultLoaded.Should().BeTrue(); // VaultLoaded is set to true even when exception is caught
		result.ArgumentException.Should().Be(expectedException);
	}

	/// <summary>
	/// Test UpdateVaultPath updates configuration
	/// </summary>
	[Fact]
	public void UpdateVaultPath_UpdatesConfiguration()
	{
		// Arrange
		var vaultPath = """C:\NewVaultPath""";
		var originalPath = _userSettings.VaultPath;

		try
		{
			// Act
			_vaultService.UpdateVaultPath(vaultPath);

			// Assert
			_userSettings.VaultPath.Should().Be(vaultPath);
		}
		finally
		{
			_userSettings.VaultPath = originalPath;
		}
	}

	/// <summary>
	/// Test UpdateVaultPath with null path handles gracefully
	/// </summary>
	[Fact]
	public void UpdateVaultPath_WithNullPath_HandlesGracefully()
	{
		// Arrange
		var originalPath = _userSettings.VaultPath;

		try
		{
			// Act
			_vaultService.UpdateVaultPath(null!);

			// Assert
			_userSettings.VaultPath.Should().BeNull();
		}
		finally
		{
			_userSettings.VaultPath = originalPath;
		}
	}

	private static readonly string[] ExistingVaults = ["Main Vault", "Gear"];

	private void SetupInvalidFileNameChars()
		=> _mockPathIO.Setup(x => x.GetInvalidFileNameChars()).Returns(['/', '\\', ':', '*', '?', '"', '<', '>', '|']);

	private (string Source, string Target) SetupVaultFiles(string sourceName, string targetName)
	{
		var sourceFile = $"""C:\Vaults\{sourceName}.vault.json""";
		var targetFile = $"""C:\Vaults\{targetName}.vault.json""";
		_mockGamePathService.Setup(x => x.GetVaultFile(sourceName)).Returns(sourceFile);
		_mockGamePathService.Setup(x => x.GetVaultFile(targetName)).Returns(targetFile);
		return (sourceFile, targetFile);
	}

	[Theory]
	[InlineData(VaultMaintenance.New, null, "", VaultMaintenanceError.EmptyName)]
	[InlineData(VaultMaintenance.New, null, "   ", VaultMaintenanceError.EmptyName)]
	[InlineData(VaultMaintenance.New, null, "Bad:Name", VaultMaintenanceError.InvalidCharacters)]
	[InlineData(VaultMaintenance.New, null, "gear", VaultMaintenanceError.DuplicateName)]
	[InlineData(VaultMaintenance.New, null, "Rings", VaultMaintenanceError.None)]
	[InlineData(VaultMaintenance.Copy, "Gear", "", VaultMaintenanceError.EmptyName)]
	[InlineData(VaultMaintenance.Copy, "Missing", "Rings", VaultMaintenanceError.SourceMissing)]
	[InlineData(VaultMaintenance.Copy, "Gear", "Ri/ngs", VaultMaintenanceError.InvalidCharacters)]
	[InlineData(VaultMaintenance.Copy, "Gear", "MAIN VAULT", VaultMaintenanceError.DuplicateName)]
	[InlineData(VaultMaintenance.Copy, "gear", "Rings", VaultMaintenanceError.None)]
	[InlineData(VaultMaintenance.Rename, "Gear", "", VaultMaintenanceError.EmptyName)]
	[InlineData(VaultMaintenance.Rename, null, "Rings", VaultMaintenanceError.SourceMissing)]
	[InlineData(VaultMaintenance.Rename, "Gear", "Main Vault", VaultMaintenanceError.DuplicateName)]
	[InlineData(VaultMaintenance.Rename, "Gear", "Rings", VaultMaintenanceError.None)]
	[InlineData(VaultMaintenance.Rename, "Gear", "gear", VaultMaintenanceError.None)]
	[InlineData(VaultMaintenance.Rename, "gear", "GEAR", VaultMaintenanceError.None)]
	[InlineData(VaultMaintenance.Rename, "Gear", "Gear", VaultMaintenanceError.DuplicateName)]
	[InlineData(VaultMaintenance.Rename, "Main Vault", "gear", VaultMaintenanceError.DuplicateName)]
	[InlineData(VaultMaintenance.Copy, "Gear", "gear", VaultMaintenanceError.DuplicateName)]
	[InlineData(VaultMaintenance.Delete, "Missing", null, VaultMaintenanceError.SourceMissing)]
	[InlineData(VaultMaintenance.Delete, "Gear", null, VaultMaintenanceError.None)]
	public void ValidateMaintenance_ReturnsFirstProblem(VaultMaintenance action, string? source, string? target, VaultMaintenanceError expected)
	{
		SetupInvalidFileNameChars();

		var error = _vaultService.ValidateMaintenance(action, source, target, ExistingVaults);

		error.Should().Be(expected);
	}

	[Fact]
	public void ValidateMaintenance_EmptyTargetWins_OverMissingSource()
	{
		SetupInvalidFileNameChars();

		var error = _vaultService.ValidateMaintenance(VaultMaintenance.Copy, "Missing", "", ExistingVaults);

		error.Should().Be(VaultMaintenanceError.EmptyName);
	}

	[Fact]
	public void CopyVault_WhenSourceExistsAndTargetDoesNot_CopiesAndDropsStaleTargetFromCache()
	{
		var (sourceFile, targetFile) = SetupVaultFiles("Gear", "Rings");
		_mockFileIO.Setup(x => x.Exists(sourceFile)).Returns(true);
		_mockFileIO.Setup(x => x.Exists(targetFile)).Returns(false);
		_sessionContext.Vaults.GetOrAddAtomic(targetFile, _ => new PlayerCollection("Rings", targetFile));

		var copied = _vaultService.CopyVault("Gear", "Rings");

		copied.Should().BeTrue();
		_mockFileIO.Verify(x => x.Copy(sourceFile, targetFile), Times.Once);
		_sessionContext.Vaults.ContainsKey(targetFile).Should().BeFalse();
	}

	[Theory]
	[InlineData(false, false)]
	[InlineData(true, true)]
	public void CopyVault_WhenSourceMissingOrTargetExists_DoesNothing(bool sourceExists, bool targetExists)
	{
		var (sourceFile, targetFile) = SetupVaultFiles("Gear", "Rings");
		_mockFileIO.Setup(x => x.Exists(sourceFile)).Returns(sourceExists);
		_mockFileIO.Setup(x => x.Exists(targetFile)).Returns(targetExists);

		var copied = _vaultService.CopyVault("Gear", "Rings");

		copied.Should().BeFalse();
		_mockFileIO.Verify(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void DeleteVault_WhenFileExists_DeletesAndDropsFromCache()
	{
		var vaultFile = """C:\Vaults\Gear.vault.json""";
		_mockGamePathService.Setup(x => x.GetVaultFile("Gear")).Returns(vaultFile);
		_mockFileIO.Setup(x => x.Exists(vaultFile)).Returns(true);
		_sessionContext.Vaults.GetOrAddAtomic(vaultFile, _ => new PlayerCollection("Gear", vaultFile));

		var deleted = _vaultService.DeleteVault("Gear");

		deleted.Should().BeTrue();
		_mockFileIO.Verify(x => x.Delete(vaultFile), Times.Once);
		_sessionContext.Vaults.ContainsKey(vaultFile).Should().BeFalse();
		// Searches must not find the items of the deleted vault any more.
		_mockItemDatabaseService.Verify(x => x.RemoveContainerFromDatabase(vaultFile), Times.Once);
	}

	[Fact]
	public void DeleteVault_OnAFixedDrive_MovesTheFileToTheRecycleBin()
	{
		var vaultFile = """C:\Vaults\Gear.vault.json""";
		_mockGamePathService.Setup(x => x.GetVaultFile("Gear")).Returns(vaultFile);
		_mockFileIO.Setup(x => x.Exists(vaultFile)).Returns(true);
		_mockFileIO.Setup(x => x.CanMoveToRecycleBin(vaultFile)).Returns(true);

		var deleted = _vaultService.DeleteVault("Gear");

		deleted.Should().BeTrue();
		_mockFileIO.Verify(x => x.MoveToRecycleBin(vaultFile), Times.Once);
		_mockFileIO.Verify(x => x.Delete(It.IsAny<string>()), Times.Never);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void GetDeleteInfo_CountsTheItemsAndTheBagsHoldingThem(bool recycleBin)
	{
		var vaultFile = """C:\Vaults\Gear.vault.json""";
		_mockGamePathService.Setup(x => x.GetVaultFile("Gear")).Returns(vaultFile);
		_mockFileIO.Setup(x => x.CanMoveToRecycleBin(vaultFile)).Returns(recycleBin);
		var vault = new PlayerCollection("Gear", vaultFile) { IsVault = true };
		vault.CreateEmptySacks(4);
		var sacks = vault.Sacks!;
		sacks[0].AddItem(new Item { BaseItemId = RecordId.Create("records/item/ring.dbr") });
		sacks[0].AddItem(new Item { BaseItemId = RecordId.Create("records/item/amulet.dbr") });
		sacks[2].AddItem(new Item { BaseItemId = RecordId.Create("records/item/sword.dbr") });
		_sessionContext.Vaults.GetOrAddAtomic(vaultFile, _ => vault);

		var info = _vaultService.GetDeleteInfo("Gear");

		info.Should().Be(new VaultDeleteInfo(ItemCount: 3, BagCount: 2, ToRecycleBin: recycleBin));
	}

	[Fact]
	public void EmptyBag_RemovesTheItemsAndDropsThemFromTheSearch()
	{
		var sack = new SackCollection();
		var ring = new Item { BaseItemId = RecordId.Create("records/item/ring.dbr") };
		var sword = new Item { BaseItemId = RecordId.Create("records/item/sword.dbr") };
		sack.AddItem(ring);
		sack.AddItem(sword);

		_vaultService.EmptyBag(sack);

		sack.IsEmpty.Should().BeTrue();
		sack.IsModified.Should().BeTrue();
		_mockItemDatabaseService.Verify(x => x.RemoveItemFromDatabase(ring), Times.Once);
		_mockItemDatabaseService.Verify(x => x.RemoveItemFromDatabase(sword), Times.Once);
	}

	private static PlayerCollection VaultWithBags(params int[] disabledTooltips)
	{
		var vault = new PlayerCollection("Gear", """C:\Vaults\Gear.vault.json""") { Sacks = [new SackCollection(), new SackCollection()] };
		vault.DisabledTooltipBagId.AddRange(disabledTooltips);
		foreach (var sack in vault.Sacks)
			sack.IsModified = false;
		return vault;
	}

	[Theory]
	[InlineData(BagTooltipChange.Disable, new int[0], new[] { 2 })]
	[InlineData(BagTooltipChange.Enable, new[] { 2, 5 }, new[] { 5 })]
	[InlineData(BagTooltipChange.EnableAll, new[] { 2, 5 }, new int[0])]
	[InlineData(BagTooltipChange.DisableAll, new[] { 2 }, new[] { 0, 1, 2, 3 })]
	public void ChangeBagTooltip_ChangesTheListAndMarksTheVaultModified(BagTooltipChange change, int[] before, int[] after)
	{
		var vault = VaultWithBags(before);

		_vaultService.ChangeBagTooltip(vault, change, bag: 2, bagCount: 4);

		vault.DisabledTooltipBagId.Should().BeEquivalentTo(after);
		// The list is saved in the vault file: the change must make the vault savable.
		vault.IsModified.Should().BeTrue();
	}

	[Fact]
	public void ChangeBagTooltip_NothingChanges_VaultStaysUnmodified()
	{
		var vault = VaultWithBags(2);

		_vaultService.ChangeBagTooltip(vault, BagTooltipChange.Disable, bag: 2, bagCount: 4);

		vault.DisabledTooltipBagId.Should().Equal(2);
		vault.IsModified.Should().BeFalse();
	}

	private static PlayerCollection VaultWithRememberedBags(int main, int secondary)
	{
		var vault = VaultWithBags();
		vault.currentlySelectedSackNumber = main;
		vault.currentlyFocusedSackNumber = secondary;
		return vault;
	}

	[Theory]
	[InlineData(false, 7, 3)]
	[InlineData(true, 1, 7)]
	public void RememberSelectedBag_WritesThePanelBagOnlyAndMarksTheVaultModified(bool secondary, int expectedMain, int expectedSecondary)
	{
		var vault = VaultWithRememberedBags(main: 1, secondary: 3);

		var changed = _vaultService.RememberSelectedBag(vault, secondary, 7);

		changed.Should().BeTrue();
		vault.currentlySelectedSackNumber.Should().Be(expectedMain);
		vault.currentlyFocusedSackNumber.Should().Be(expectedSecondary);
		// The bags are saved in the vault file: the change must make the vault savable.
		vault.IsModified.Should().BeTrue();
	}

	[Theory]
	[InlineData(false, 1)]
	[InlineData(true, 3)]
	public void RememberSelectedBag_SameBag_VaultStaysUnmodified(bool secondary, int bag)
	{
		var vault = VaultWithRememberedBags(main: 1, secondary: 3);

		var changed = _vaultService.RememberSelectedBag(vault, secondary, bag);

		changed.Should().BeFalse();
		vault.IsModified.Should().BeFalse();
	}

	[Theory]
	// The bag remembered for the panel wins over the bag shown before.
	[InlineData(false, 4, 9, 2, 12, 4)]
	[InlineData(true, 4, 9, 2, 12, 9)]
	// Nothing remembered (-1): the panel keeps its bag.
	[InlineData(false, -1, 9, 2, 12, 2)]
	[InlineData(true, 4, -1, 2, 12, 2)]
	// The bag shown before is kept within the bags.
	[InlineData(false, -1, -1, 11, 3, 2)]
	[InlineData(false, -1, -1, -5, 3, 0)]
	// A remembered bag beyond the bags (a damaged file) is kept within them too.
	[InlineData(false, 20, -1, 0, 12, 11)]
	// No bags: never below 0.
	[InlineData(false, -1, -1, -2, 0, 0)]
	public void ResolveInitialBag_PicksTheRememberedBagElseTheCurrentOneWithinTheBags(bool secondary, int rememberedMain, int rememberedSecondary, int currentBag, int count, int expected)
	{
		var vault = VaultWithRememberedBags(rememberedMain, rememberedSecondary);

		var bag = _vaultService.ResolveInitialBag(vault, secondary, currentBag, count);

		bag.Should().Be(expected);
		vault.IsModified.Should().BeFalse();
	}

	[Fact]
	public void ResolveInitialBag_WithoutVault_KeepsTheCurrentBagWithinTheBags()
	{
		_vaultService.ResolveInitialBag(null, false, 2, 4).Should().Be(2);
		_vaultService.ResolveInitialBag(null, false, 6, 4).Should().Be(3);
	}

	[Fact]
	public void DeleteVault_WhenFileMissing_OnlyDropsFromCache()
	{
		var vaultFile = """C:\Vaults\Gear.vault.json""";
		_mockGamePathService.Setup(x => x.GetVaultFile("Gear")).Returns(vaultFile);
		_mockFileIO.Setup(x => x.Exists(vaultFile)).Returns(false);
		_sessionContext.Vaults.GetOrAddAtomic(vaultFile, _ => new PlayerCollection("Gear", vaultFile));

		var deleted = _vaultService.DeleteVault("Gear");

		deleted.Should().BeFalse();
		_mockFileIO.Verify(x => x.Delete(It.IsAny<string>()), Times.Never);
		_sessionContext.Vaults.ContainsKey(vaultFile).Should().BeFalse();
	}

	[Fact]
	public void RenameVault_ToMainVault_MovesAndDropsBothNamesFromCache()
	{
		var (oldFile, newFile) = SetupVaultFiles("Gear", VaultService.MAINVAULT);
		_mockFileIO.Setup(x => x.Exists(oldFile)).Returns(true);
		_mockFileIO.Setup(x => x.Exists(newFile)).Returns(false);
		_sessionContext.Vaults.GetOrAddAtomic(oldFile, _ => new PlayerCollection("Gear", oldFile));
		// The Main Vault is always listed, so an empty unsaved one may be cached.
		_sessionContext.Vaults.GetOrAddAtomic(newFile, _ => new PlayerCollection(VaultService.MAINVAULT, newFile));

		var renamed = _vaultService.RenameVault("Gear", VaultService.MAINVAULT);

		renamed.Should().BeTrue();
		_mockFileIO.Verify(x => x.Move(oldFile, newFile), Times.Once);
		_sessionContext.Vaults.ContainsKey(oldFile).Should().BeFalse();
		_sessionContext.Vaults.ContainsKey(newFile).Should().BeFalse();
		// The items found under the old name go; they come back under the new one when it is loaded.
		_mockItemDatabaseService.Verify(x => x.RemoveContainerFromDatabase(oldFile), Times.Once);
	}

	[Fact]
	public void RenameVault_OnlyByCase_MovesTheFileAlthoughTheTargetNameExists()
	{
		// File names ignore case on Windows: "gear.vault.json" exists as soon as "Gear.vault.json" does.
		var (oldFile, newFile) = SetupVaultFiles("Gear", "gear");
		_mockFileIO.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
		_sessionContext.Vaults.GetOrAddAtomic(oldFile, _ => new PlayerCollection("Gear", oldFile));

		var renamed = _vaultService.RenameVault("Gear", "gear");

		renamed.Should().BeTrue();
		_mockFileIO.Verify(x => x.Move(oldFile, newFile), Times.Once);
		_sessionContext.Vaults.ContainsKey(oldFile).Should().BeFalse();
	}

	[Theory]
	[InlineData(false, false)]
	[InlineData(true, true)]
	public void RenameVault_WhenSourceMissingOrTargetExists_KeepsFileAndCache(bool sourceExists, bool targetExists)
	{
		var (oldFile, newFile) = SetupVaultFiles("Gear", "Rings");
		_mockFileIO.Setup(x => x.Exists(oldFile)).Returns(sourceExists);
		_mockFileIO.Setup(x => x.Exists(newFile)).Returns(targetExists);
		_sessionContext.Vaults.GetOrAddAtomic(oldFile, _ => new PlayerCollection("Gear", oldFile));

		var renamed = _vaultService.RenameVault("Gear", "Rings");

		renamed.Should().BeFalse();
		_mockFileIO.Verify(x => x.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		_sessionContext.Vaults.ContainsKey(oldFile).Should().BeTrue();
	}

	[Fact]
	public void BuildVaultChoices_PutsLeadingEntryThenMainVaultThenOthersInOrder()
	{
		var choices = _vaultService.BuildVaultChoices(["Arms", "Main Vault", "Zeal"], "<Maintain>");

		choices.Should().Equal("<Maintain>", "Main Vault", "Arms", "Zeal");
	}

	[Fact]
	public void BuildVaultChoices_ListsMainVaultWithoutItsFile_WhenAlwaysIncluded()
	{
		var choices = _vaultService.BuildVaultChoices(["Arms"], "<Maintain>");

		choices.Should().Equal("<Maintain>", "Main Vault", "Arms");
	}

	[Fact]
	public void BuildVaultChoices_LeavesOutMissingMainVault_WhenNotAlwaysIncluded()
	{
		var choices = _vaultService.BuildVaultChoices(["Arms", "Zeal"], alwaysIncludeMainVault: false);

		choices.Should().Equal("Arms", "Zeal");
	}

	[Fact]
	public void BuildVaultChoices_SkipsOnlyTheExcludedVault_AndKeepsTheOnesAfterIt()
	{
		var choices = _vaultService.BuildVaultChoices(["Arms", "Rings", "Zeal"], "<Maintain>", "Rings");

		choices.Should().Equal("<Maintain>", "Main Vault", "Arms", "Zeal");
	}

	[Fact]
	public void BuildVaultChoices_ExcludedMainVault_IsNotAddedBack()
	{
		var choices = _vaultService.BuildVaultChoices(["Arms", "Main Vault"], "<Select>", VaultService.MAINVAULT);

		choices.Should().Equal("<Select>", "Arms");
	}

	private static PlayerCollection CreateVaultWithItemInBag(int bags, int filledBag)
	{
		var vault = new PlayerCollection("Gear", """C:\Vaults\Gear.vault.json""") { IsVault = true };
		vault.CreateEmptySacks(bags);
		vault.Sacks[filledBag].AddItem(new Item());
		return vault;
	}

	[Theory]
	[InlineData(0, 1, BagTransferCheck.Allowed)]
	[InlineData(0, 2, BagTransferCheck.DestinationNotEmpty)]
	[InlineData(0, 0, BagTransferCheck.Invalid)]
	[InlineData(0, -1, BagTransferCheck.Invalid)]
	[InlineData(0, 3, BagTransferCheck.Invalid)]
	[InlineData(3, 1, BagTransferCheck.Invalid)]
	public void CheckBagTransfer_ChecksBoundsSameBagAndDestinationContent(int source, int destination, BagTransferCheck expected)
	{
		var vault = CreateVaultWithItemInBag(bags: 3, filledBag: 2);

		var check = _vaultService.CheckBagTransfer(vault, source, destination);

		check.Should().Be(expected);
	}

	[Fact]
	public void MoveBag_GivesTheItemsOfTheShiftedBagsTheirNewBagNumber()
	{
		// Arrange: the item of bag 2 is found by the search in bag 2
		var vault = CreateVaultWithItemInBag(bags: 3, filledBag: 2);
		var item = vault.GetSack(2)!.Single();
		item.Place.SackNumber = 2;

		// Act: bag 2 goes first
		var moved = _vaultService.MoveBag(vault, 2, 0);

		// Assert
		moved.Should().BeTrue();
		vault.GetSack(0)!.Should().Contain(item);
		item.Place.SackNumber.Should().Be(0);
	}

	[Fact]
	public void MoveBag_SameBag_DoesNothing()
		=> _vaultService.MoveBag(CreateVaultWithItemInBag(bags: 3, filledBag: 2), 1, 1).Should().BeFalse();

	[Fact]
	public void CopyBag_IndexesTheCopies_AndDropsTheOverwrittenItems()
	{
		// Arrange
		var vault = CreateVaultWithItemInBag(bags: 3, filledBag: 2);
		vault.GetSack(0)!.AddItem(new Item());
		var overwritten = vault.GetSack(2)!.Single();

		// Act
		var copied = _vaultService.CopyBag(vault, 0, 2);

		// Assert
		copied.Should().BeTrue();
		var copy = vault.GetSack(2)!.Single();
		copy.Should().NotBeSameAs(vault.GetSack(0)!.Single());
		copy.Place.SackNumber.Should().Be(2);
		_mockItemDatabaseService.Verify(x => x.RemoveItemFromDatabase(overwritten), Times.Once);
		_mockItemDatabaseService.Verify(x => x.TryAddItemToDatabase(copy), Times.Once);
	}

	[Fact]
	public void BuildCsvExport_WholeVault_NumbersRowsAcrossBagsWithOneBasedBagIds()
	{
		_userSettings.CSVDelimiter = "Semicolon";
		var vault = CreateVaultWithItemInBag(bags: 3, filledBag: 2);
		vault.GetSack(0)!.AddItem(new Item());
		_mockItemProvider
			.Setup(x => x.GetFriendlyNames(It.IsAny<Item>(), FriendlyNamesExtraScopes.Requirements, true))
			.Returns<Item, FriendlyNamesExtraScopes?, bool>((item, _, _) => new ToFriendlyNameResult(item));

		var lines = _vaultService.BuildCsvExport(vault, null).Split(Environment.NewLine);

		lines.Should().HaveCount(3);
		lines[0].Should().StartWith("Row;Vault;BagId;");
		lines[1].Should().StartWith("0;Gear;1;");
		lines[2].Should().StartWith("1;Gear;3;");
	}

	[Fact]
	public void BuildCsvExport_OneBag_ExportsOnlyThatBag()
	{
		var vault = CreateVaultWithItemInBag(bags: 3, filledBag: 2);
		vault.GetSack(0)!.AddItem(new Item());
		_mockItemProvider
			.Setup(x => x.GetFriendlyNames(It.IsAny<Item>(), FriendlyNamesExtraScopes.Requirements, true))
			.Returns<Item, FriendlyNamesExtraScopes?, bool>((item, _, _) => new ToFriendlyNameResult(item));

		var lines = _vaultService.BuildCsvExport(vault, 2).Split(Environment.NewLine);

		lines.Should().HaveCount(2);
		lines[1].Should().StartWith("0,Gear,3,");
	}

	[Fact]
	public void BuildCsvExport_UnknownDelimiterSetting_UsesComma()
	{
		_userSettings.CSVDelimiter = "Space";
		var vault = CreateVaultWithItemInBag(bags: 1, filledBag: 0);
		_mockItemProvider
			.Setup(x => x.GetFriendlyNames(It.IsAny<Item>(), FriendlyNamesExtraScopes.Requirements, true))
			.Returns<Item, FriendlyNamesExtraScopes?, bool>((item, _, _) => new ToFriendlyNameResult(item));

		var lines = _vaultService.BuildCsvExport(vault, null).Split(Environment.NewLine);

		lines[0].Should().StartWith("Row,Vault,BagId,");
		lines[1].Should().StartWith("0,Gear,1,");
	}

	/// <summary>
	/// Saves every modified file the way the save orchestrator does, one by one.
	/// </summary>
	private int SaveAllModifiedVaults()
	{
		var modified = this._vaultService.GetModifiedVaults();
		foreach (var (file, vault) in modified)
			this._vaultService.SaveModifiedVault(file, vault);

		return modified.Count;
	}

	[Theory]
	[InlineData(true, """D:\Vaults""")]
	[InlineData(false, """C:\Mine""")]
	public void GetVaultList_UpdatesTheVaultPathOnlyWhenTheFolderFellBackToItsDefault(bool folderChanged, string expectedPath)
	{
		this._userSettings.VaultPath = """C:\Mine""";
		this._mockGamePathService.Setup(g => g.GetVaultList()).Returns(["Main Vault", "Rings"]);
		this._mockGamePathService.SetupGet(g => g.VaultFolderChanged).Returns(folderChanged);
		this._mockGamePathService.SetupGet(g => g.TQVaultSaveFolder).Returns("""D:\Vaults""");

		var vaults = this._vaultService.GetVaultList();

		vaults.Should().Equal("Main Vault", "Rings");
		this._userSettings.VaultPath.Should().Be(expectedPath);
	}

	[Theory]
	[InlineData(true, "Rings")]
	[InlineData(false, VaultService.MAINVAULT)]
	public void ResolveVaultToLoad_LoadLastVault_ChecksTheVaultFile(bool fileExists, string expected)
	{
		this._userSettings.LoadLastVault = true;
		this._userSettings.LastVaultName = "Rings";
		this._mockGamePathService.Setup(g => g.GetVaultFile("Rings")).Returns("""C:\Vaults\Rings.vault""");
		this._mockFileIO.Setup(f => f.Exists("""C:\Vaults\Rings.vault""")).Returns(fileExists);

		this._vaultService.ResolveVaultToLoad("Potions").Should().Be(expected);
	}

	private static BagButtonIconInfo CustomIcon(string label)
		=> BagButtonIconInfo.Create(BagButtonDisplayMode.CustomIcon | BagButtonDisplayMode.Label, label, "on.tex", "off.tex", null);

	[Fact]
	public void CopyBagIcon_ThenPasteOnTwoBags_EachGetsItsOwnCopyAndIsModified()
	{
		var source = new SackCollection { BagButtonIconInfo = CustomIcon("Rings") };
		var first = new SackCollection();
		var second = new SackCollection();

		this._vaultService.CopyBagIcon(source);
		source.BagButtonIconInfo!.Label = "Changed later";

		this._vaultService.PasteBagIcon(first).Should().BeTrue();
		this._vaultService.PasteBagIcon(second).Should().BeTrue();

		first.BagButtonIconInfo!.Label.Should().Be("Rings");
		second.BagButtonIconInfo.Should().NotBeSameAs(first.BagButtonIconInfo);
		first.IsModified.Should().BeTrue();
		second.IsModified.Should().BeTrue();
	}

	[Fact]
	public void PasteBagIcon_DefaultIconCopiedOnACustomBag_ResetsItsIcon()
	{
		this._vaultService.CopyBagIcon(new SackCollection());
		var target = new SackCollection { BagButtonIconInfo = CustomIcon("Potions") };

		this._vaultService.PasteBagIcon(target).Should().BeTrue();

		target.BagButtonIconInfo.Should().BeNull();
		target.IsModified.Should().BeTrue();
	}

	[Fact]
	public void PasteBagIcon_SameIcon_LeavesTheBagUnmodified()
	{
		this._vaultService.CopyBagIcon(new SackCollection { BagButtonIconInfo = CustomIcon("Rings") });
		var target = new SackCollection { BagButtonIconInfo = CustomIcon("Rings") };

		this._vaultService.PasteBagIcon(target).Should().BeTrue();

		target.IsModified.Should().BeFalse();
	}

	[Fact]
	public void PasteBagIcon_IconCopiedButNoBag_IsRefused()
	{
		this._vaultService.CopyBagIcon(new SackCollection { BagButtonIconInfo = CustomIcon("Rings") });

		this._vaultService.PasteBagIcon(null).Should().BeFalse();
	}
}
