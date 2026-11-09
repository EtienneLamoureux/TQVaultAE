using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Exceptions;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class StartupServiceTests
{
	private static readonly string TQPath = Path.Combine(Path.GetTempPath(), "TQ");
	private static readonly string TQITPath = Path.Combine(Path.GetTempPath(), "TQIT");
	private static readonly string SaveFolder = Path.Combine(Path.GetTempPath(), "My Games", "Titan Quest");

	private readonly UserSettings _settings = new()
	{
		AutoDetectGamePath = false,
		AutoDetectLanguage = true,
		TQPath = TQPath,
		TQITPath = TQITPath,
		VaultPath = Path.Combine(Path.GetTempPath(), "Vaults"),
	};

	private readonly Mock<IGamePathService> _gamePath = new();
	private readonly Mock<IFileIO> _fileIO = new();
	private readonly Mock<IDirectoryIO> _directoryIO = new();
	private readonly StartupService _service;

	/// <summary>The database.arz files that exist.</summary>
	private readonly HashSet<string> _gameDatabases = new(StringComparer.OrdinalIgnoreCase);

	private static string Database(string gamePath) => Path.Combine(gamePath, "Database", "database.arz");

	public StartupServiceTests()
	{
		this._gamePath.SetupAllProperties();
		this._gamePath.SetupGet(g => g.SaveFolderTQ).Returns(SaveFolder);
		this._gameDatabases.Add(Database(TQPath));
		this._gameDatabases.Add(Database(TQITPath));
		this._fileIO.Setup(f => f.Exists(It.Is<string>(p => p.EndsWith("database.arz"))))
			.Returns<string>(this._gameDatabases.Contains);
		this._service = new StartupService(
			new Mock<ILogger<StartupService>>().Object
			, this._settings
			, this._gamePath.Object
			, this._fileIO.Object
			, this._directoryIO.Object);
	}

	[Fact]
	public void SetupGamePaths_Configured_UsesTheSettingsPaths()
	{
		var notice = this._service.SetupGamePaths();

		this._gamePath.Object.GamePathTQ.Should().Be(TQPath);
		this._gamePath.Object.GamePathTQIT.Should().Be(TQITPath);
		this._gamePath.Object.TQVaultSaveFolder.Should().Be(this._settings.VaultPath);
		this._gamePath.Verify(g => g.ResolveGamePath(), Times.Never);
		notice.Should().Be(StartupNotice.None);
	}

	[Fact]
	public void SetupGamePaths_AutoDetect_UsesTheResolvedPathForBoth()
	{
		this._settings.AutoDetectGamePath = true;
		var detected = Path.Combine(Path.GetTempPath(), "Steam", "TQ");
		this._gameDatabases.Add(Database(detected));
		this._gamePath.Setup(g => g.ResolveGamePath()).Returns(detected);

		this._service.SetupGamePaths();

		this._gamePath.Object.GamePathTQ.Should().Be(detected);
		this._gamePath.Object.GamePathTQIT.Should().Be(detected);
	}

	[Fact]
	public void SetupGamePaths_DetectedOrForcedFolderWithoutDatabase_AsksForTheGameFolder()
	{
		// A stale registry entry, or a ForceGamePath saved before the game moved.
		this._settings.AutoDetectGamePath = true;
		var detected = Path.Combine(Path.GetTempPath(), "Moved", "TQ");
		this._gamePath.Setup(g => g.ResolveGamePath()).Returns(detected);

		var setup = () => this._service.SetupGamePaths();

		setup.Should().Throw<ExGamePathNotFound>().Which.Message.Should().Contain(detected);
	}

	[Fact]
	public void SetupGamePaths_ConfiguredImmortalThronePathWithoutDatabase_AsksForTheGameFolder()
	{
		this._gameDatabases.Remove(Database(TQITPath));

		var setup = () => this._service.SetupGamePaths();

		setup.Should().Throw<ExGamePathNotFound>().Which.Message.Should().Contain(TQITPath);
	}

	[Fact]
	public void SetupGamePaths_NoConfiguredPath_AsksForTheGameFolder()
	{
		this._settings.TQPath = string.Empty;
		this._settings.TQITPath = string.Empty;

		var setup = () => this._service.SetupGamePaths();

		setup.Should().Throw<ExGamePathNotFound>();
	}

	[Fact]
	public void SetupGamePaths_OnlyTheImmortalThronePathSet_IsEnough()
	{
		// LoadARZFile then reads the Immortal Throne database only, which holds every record.
		this._settings.TQPath = string.Empty;

		var setup = () => this._service.SetupGamePaths();

		setup.Should().NotThrow();
	}

	[Fact]
	public void ApplyChosenGamePath_AutoDetect_ForcesThePath()
	{
		this._settings.AutoDetectGamePath = true;
		var chosen = Path.Combine(Path.GetTempPath(), "Chosen");

		this._service.ApplyChosenGamePath(chosen);

		this._settings.ForceGamePath.Should().Be(chosen);
		this._settings.TQPath.Should().Be(TQPath);
		this._settings.TQITPath.Should().Be(TQITPath);
	}

	[Fact]
	public void ApplyChosenGamePath_ConfiguredPaths_ReplacesThemSoTheNextStartUsesIt()
	{
		var chosen = Path.Combine(Path.GetTempPath(), "Chosen");
		this._gameDatabases.Add(Database(chosen));

		this._service.ApplyChosenGamePath(chosen);
		this._service.SetupGamePaths();

		this._gamePath.Object.GamePathTQ.Should().Be(chosen);
		this._gamePath.Object.GamePathTQIT.Should().Be(chosen);
	}

	[Fact]
	public void SetupGamePaths_GameTypeComesFromTheImmortalThronePathWhenSet()
	{
		this._fileIO.Setup(f => f.Exists(Path.Combine(TQPath, "TQ.exe"))).Returns(true);
		this._fileIO.Setup(f => f.Exists(Path.Combine(TQITPath, "TQIT.exe"))).Returns(true);

		this._service.SetupGamePaths();

		this._gamePath.Object.GameType.Should().Be(GameType.TQIT);
	}

	[Fact]
	public void SetupGamePaths_OnlyTheTQPath_GivesItsGameType()
	{
		this._settings.TQITPath = string.Empty;
		this._fileIO.Setup(f => f.Exists(Path.Combine(TQPath, "TQ.exe"))).Returns(true);

		this._service.SetupGamePaths();

		this._gamePath.Object.GameType.Should().Be(GameType.TQAE);
	}

	[Fact]
	public void DetectGameType_NoKnownExecutable_IsTheOriginalGame()
	{
		this._service.DetectGameType(TQPath).Should().Be(GameType.TQ);
	}

	[Fact]
	public void SetupGamePaths_NoVaultPath_AsksTheDefaultPathNotice()
	{
		this._settings.VaultPath = string.Empty;

		this._service.SetupGamePaths().Should().Be(StartupNotice.DefaultVaultPath);
	}

	[Fact]
	public void SetupGamePaths_NoVaultPathAndALegacyShortcut_AsksTheShortcutNotice()
	{
		this._settings.VaultPath = string.Empty;
		var folder = Path.Combine(SaveFolder, "TQVaultData");
		this._directoryIO.Setup(d => d.Exists(folder)).Returns(false);
		this._fileIO.Setup(f => f.Exists(Path.ChangeExtension(folder, ".lnk"))).Returns(true);

		this._service.SetupGamePaths().Should().Be(StartupNotice.LegacyVaultShortcut);
	}

	[Fact]
	public void SetupGamePaths_ShortcutNextToARealFolder_AsksTheDefaultPathNotice()
	{
		this._settings.VaultPath = string.Empty;
		var folder = Path.Combine(SaveFolder, "TQVaultData");
		this._directoryIO.Setup(d => d.Exists(folder)).Returns(true);
		this._fileIO.Setup(f => f.Exists(Path.ChangeExtension(folder, ".lnk"))).Returns(true);

		this._service.SetupGamePaths().Should().Be(StartupNotice.DefaultVaultPath);
	}

	[Fact]
	public void ResolveUICultureName_UILanguageWins()
	{
		this._settings.UILanguage = "French";
		this._settings.AutoDetectLanguage = false;
		this._settings.TQLanguage = "German";

		this._service.ResolveUICultureName().Should().Be("fr");
	}

	[Fact]
	public void ResolveUICultureName_GameLanguageWhenNotAutoDetected()
	{
		this._settings.AutoDetectLanguage = false;
		this._settings.TQLanguage = "german";

		this._service.ResolveUICultureName().Should().Be("de");
	}

	[Fact]
	public void ResolveUICultureName_AutoDetectedWithoutUILanguage_KeepsTheSystemCulture()
	{
		this._settings.TQLanguage = "German";

		this._service.ResolveUICultureName().Should().BeNull();
	}

	[Fact]
	public void FindNeutralCultureName_UnknownLanguage_IsNull()
	{
		StartupService.FindNeutralCultureName("Klingon").Should().BeNull();
	}
}
