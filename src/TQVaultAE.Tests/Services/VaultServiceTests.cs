using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

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
			_userSettings
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
		var vaultFile = "C:\\Test\\Vaults\\TestVault.vault";
		var oldFormatFile = "C:\\Test\\Vaults\\TestVault";

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
		var vaultFile = "C:\\Test\\Vaults\\TestVault.vault";
		var oldFormatFile = "C:\\Test\\Vaults\\TestVault";

		_mockPathIO.Setup(x => x.GetDirectoryName(vaultFile)).Returns("C:\\Test\\Vaults");
		_mockPathIO.Setup(x => x.GetFileNameWithoutExtension(vaultFile)).Returns("TestVault");
		_mockPathIO.Setup(x => x.Combine("C:\\Test\\Vaults", "TestVault")).Returns(oldFormatFile);
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
		var vault = new PlayerCollection("TestVault", "C:\\Test\\Vaults\\TestVault.vault")
		{
			IsVault = true
		};

		_sessionContext.Vaults.GetOrAddAtomic("TestVault.vault", _ => vault);

		PlayerCollection? vaultOnError = null;

		// Act
		var result = _vaultService.SaveAllModifiedVaults(ref vaultOnError);

		// Assert
		result.Should().Be(0);
		vaultOnError.Should().BeNull();
		_mockPlayerCollectionProvider.Verify(x => x.Save(It.IsAny<PlayerCollection>(), It.IsAny<string>()), Times.Never);
	}

	/// <summary>
	/// Test SaveAllModifiedVaults with modified vaults and backup disabled saves successfully
	/// </summary>
	[Fact]
	public void SaveAllModifiedVaults_WithModifiedVaults_SavesSuccessfully()
	{
		// Arrange
		var vault = new PlayerCollection("TestVault", "C:\\Test\\Vaults\\TestVault.vault")
		{
			IsVault = true
		};
		// Initialize sacks using CreateEmptySacks and mark one as modified
		vault.CreateEmptySacks(1);
		vault.Sacks[0].IsModified = true;

		_sessionContext.Vaults.GetOrAddAtomic("TestVault.vault", _ => vault);

		PlayerCollection? vaultOnError = null;
		_userSettings.DisableLegacyBackup = true;

		try
		{
			// Act
			var result = _vaultService.SaveAllModifiedVaults(ref vaultOnError);

		// Assert
		result.Should().Be(1);
		// vaultOnError.Should().Be(vault);
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
		var vault = new PlayerCollection("TestVault", "C:\\Test\\Vaults\\TestVault.vault")
		{
			IsVault = true
		};
		// Need to initialize Sacks array first
		var sackCollection = new SackCollection();
		var sacksField = sackCollection.GetType().GetProperty("IsModified");
		sacksField?.SetValue(sackCollection, true);
		vault.Sacks = new SackCollection[1] { sackCollection };

		_sessionContext.Vaults.GetOrAddAtomic("TestVault.vault", _ => vault);

		PlayerCollection? vaultOnError = null;
		_userSettings.DisableLegacyBackup = false;

		// Act
		var result = _vaultService.SaveAllModifiedVaults(ref vaultOnError);

		// Assert
		result.Should().Be(1);
		// vaultOnError.Should().Be(vault);
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
		var vaultFile = "C:\\Test\\Vaults\\TestVault.vault";

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
		var vaultFile = "C:\\Test\\Vaults\\TestVault.vault";

		_mockGamePathService.Setup(x => x.GetVaultFile(vaultName)).Returns(vaultFile);
		_mockFileIO.Setup(x => x.Exists(vaultFile)).Returns(false);
		_mockFileIO.Setup(x => x.Exists("C:\\Test\\Vaults\\TestVault")).Returns(false);

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
		var vaultFile = "C:\\Test\\Vaults\\TestVault.vault";

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
		var vaultFile = "C:\\Test\\Vaults\\TestVault.vault";
		_mockGamePathService.Setup(x => x.GetVaultFile(null)).Returns(vaultFile);
		_mockGamePathService.Setup(x => x.GetVaultFile("")).Returns(vaultFile);
		_mockFileIO.Setup(x => x.Exists(vaultFile)).Returns(false);
		_mockFileIO.Setup(x => x.Exists("C:\\Test\\Vaults\\TestVault")).Returns(false);

		// Act & Assert
		_vaultService.LoadVault(null).Should().NotBeNull();
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
		var vaultFile = "C:\\Test\\Vaults\\TestVault.vault";
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
		var vaultPath = "C:\\NewVaultPath";
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
			_vaultService.UpdateVaultPath(null);

			// Assert
			_userSettings.VaultPath.Should().BeNull();
		}
		finally
		{
			_userSettings.VaultPath = originalPath;
		}
	}
}
