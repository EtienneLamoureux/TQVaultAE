using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;
using TQVaultAE.Services.Win32;

namespace TQVaultAE.Tests.Win32.Services;

/// <summary>
/// Game path resolution of <see cref="GamePathServiceWin"/>: forced path, mods and custom maps, character folders of
/// the original game, Immortal Throne and custom maps, and the archive of a resource id. The file system is mocked
/// through <see cref="IDirectoryIO"/>: nothing is read from the user's install.
/// </summary>
public class GamePathServiceWinTests
{
	private static readonly string SteamLibrary = Path.Combine(Path.GetTempPath(), "SteamLibrary");
	private static readonly string SteamGame = Path.Combine(SteamLibrary, "steamapps", "common", "Titan Quest Anniversary Edition");
	private static readonly string Workshop = Path.Combine(SteamLibrary, "steamapps", "workshop", "content", "475150");
	private static readonly string GogGame = Path.Combine(Path.GetTempPath(), "GOG Games", "Titan Quest Anniversary Edition");

	private readonly Mock<IDirectoryIO> _directoryIO = new();
	private readonly Mock<IFileIO> _fileIO = new();
	private readonly UserSettings _settings = new();
	private readonly GamePathServiceWin _service;

	public GamePathServiceWinTests()
	{
		this._directoryIO.Setup(d => d.GetDirectories(It.IsAny<string>(), It.IsAny<string>())).Throws<DirectoryNotFoundException>();
		this._service = new GamePathServiceWin(
			new Mock<ILogger<GamePathServiceWin>>().Object
			, new Mock<ITQDataService>().Object
			, this._fileIO.Object
			, new PathIO()
			, this._directoryIO.Object
			, this._settings);
	}

	private void AddDirectories(string parent, params string[] children)
	{
		this._directoryIO.Setup(d => d.Exists(parent)).Returns(true);
		this._directoryIO.Setup(d => d.GetDirectories(parent, It.IsAny<string>()))
			.Returns(children.Select(c => Path.Combine(parent, c)).ToArray());
	}

	private string LegacyCustomMaps => Path.Combine(this._service.SaveFolderTQIT, "CustomMaps");

	#region Game path

	[Fact]
	public void ResolveGamePath_ForceGamePath_WinsOverDetection()
	{
		this._settings.ForceGamePath = GogGame;

		this._service.ResolveGamePath().Should().Be(GogGame);
		this._fileIO.Verify(f => f.Exists(It.IsAny<string>()), Times.Never);
	}

	#endregion

	#region Mods and custom maps

	[Fact]
	public void GetCustomMapList_SteamInstall_ListsLegacyMapsThenWorkshopModsByName()
	{
		this._service.GamePathTQIT = SteamGame;
		this.AddDirectories(this.LegacyCustomMaps, "Diablo 2 Lilith");
		this.AddDirectories(Workshop, "1949340853", "756997480");
		this.AddDirectories(Path.Combine(Workshop, "1949340853"), "shadowchampions");
		// One workshop item can hold several mods.
		this.AddDirectories(Path.Combine(Workshop, "756997480"), "Masteries x10", "Masteries");

		var maps = this._service.GetCustomMapList();

		maps.Select(m => (m.Path, m.DisplayName)).Should().Equal(
			(Path.Combine(this.LegacyCustomMaps, "Diablo 2 Lilith"), "Legacy : Diablo 2 Lilith"),
			(Path.Combine(Workshop, "756997480", "Masteries"), "SteamWorkshop : Masteries"),
			(Path.Combine(Workshop, "756997480", "Masteries x10"), "SteamWorkshop : Masteries x10"),
			(Path.Combine(Workshop, "1949340853", "shadowchampions"), "SteamWorkshop : shadowchampions"));
	}

	[Fact]
	public void GetCustomMapList_InstallOutsideSteam_ListsOnlyLegacyMaps()
	{
		this._service.GamePathTQIT = GogGame;
		this.AddDirectories(this.LegacyCustomMaps, "Diablo 2 Lilith");

		var maps = this._service.GetCustomMapList();

		maps.Select(m => m.DisplayName).Should().Equal("Legacy : Diablo 2 Lilith");
		this._directoryIO.Verify(d => d.GetDirectories(It.Is<string>(p => p.Contains("workshop")), It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void GetCustomMapList_NoCustomMapsNorWorkshopFolder_ReturnsEmpty()
	{
		this._service.GamePathTQIT = SteamGame;

		this._service.GetCustomMapList().Should().BeEmpty();
	}

	[Fact]
	public void StashFiles_CustomMap_UseTheSysSubfolderNamedAfterTheMap()
	{
		this._service.MapName = Path.Combine(Workshop, "1949340853", "shadowchampions");
		var sys = Path.Combine(this._service.SaveFolderTQIT, "SaveData", "Sys");

		this._service.TransferStashFileFullPath.Should().Be(Path.Combine(sys, "shadowchampions", "winsys.dxb"));
		this._service.RelicVaultStashFileFullPath.Should().Be(Path.Combine(sys, "shadowchampions", "miscsys.dxb"));
	}

	[Theory]
	[InlineData("")]
	[InlineData("main")]
	[InlineData(" Main ")]
	public void StashFiles_MainCampaign_UseTheSysFolder(string mapName)
	{
		this._service.MapName = mapName;
		var sys = Path.Combine(this._service.SaveFolderTQIT, "SaveData", "Sys");

		this._service.IsCustom.Should().BeFalse();
		this._service.TransferStashFileFullPath.Should().Be(Path.Combine(sys, "winsys.dxb"));
	}

	#endregion

	#region Characters

	private (string TQ, string TQArchive, string TQIT, string TQITArchive) AddCharacterFolders(string mapFolder)
	{
		var tq = Path.Combine(this._service.SaveFolderTQ, "SaveData", mapFolder);
		var tqit = Path.Combine(this._service.SaveFolderTQIT, "SaveData", mapFolder);
		var tqArchive = Path.Combine(tq, "ArchivedCharacters");
		var tqitArchive = Path.Combine(tqit, "ArchivedCharacters");
		this.AddDirectories(tq, "_HeroG");
		this.AddDirectories(tqArchive, "_OldTQ");
		this.AddDirectories(tqit, "_HeroA");
		this.AddDirectories(tqitArchive, "_HeroF");
		return (tq, tqArchive, tqit, tqitArchive);
	}

	[Fact]
	public void GetCharacterList_OriginalTitanQuestSupportOff_ListsOnlyImmortalThroneCharacters()
	{
		var (_, _, tqit, tqitArchive) = this.AddCharacterFolders("Main");

		this._service.GetCharacterList().Should().Equal(
			Path.Combine(tqit, "_HeroA"),
			Path.Combine(tqitArchive, "_HeroF"));
	}

	[Fact]
	public void GetCharacterList_OriginalTitanQuestSupportOn_AddsTheTitanQuestCharacters()
	{
		this._settings.EnableOriginalTQSupport = true;
		var (tq, tqArchive, tqit, tqitArchive) = this.AddCharacterFolders("Main");

		this._service.GetCharacterList().Should().Equal(
			Path.Combine(tq, "_HeroG"),
			Path.Combine(tqit, "_HeroA"),
			Path.Combine(tqArchive, "_OldTQ"),
			Path.Combine(tqitArchive, "_HeroF"));
	}

	[Fact]
	public void GetCharacterList_CustomMap_ListsTheUserFolderCharacters()
	{
		this._service.MapName = Path.Combine(Workshop, "1949340853", "shadowchampions");
		this.AddCharacterFolders("Main");
		var (_, _, tqit, tqitArchive) = this.AddCharacterFolders("User");

		this._service.GetCharacterList().Should().Equal(
			Path.Combine(tqit, "_HeroA"),
			Path.Combine(tqitArchive, "_HeroF"));
	}

	[Fact]
	public void GetPlayerFile_CustomMapArchivedCharacter_IsInTheUserArchiveFolder()
	{
		this._service.MapName = Path.Combine(Workshop, "1949340853", "shadowchampions");

		this._service.GetPlayerFile("HeroA", true, true).Should().Be(
			Path.Combine(this._service.SaveFolderTQIT, "SaveData", "User", "ArchivedCharacters", "_HeroA", "Player.chr"));
	}

	[Theory]
	[InlineData("Main")]
	[InlineData("User")]
	public void ArchiveTogglePath_GoesToTheArchiveAndBack(string mapFolder)
	{
		var folder = Path.Combine(this._service.SaveFolderTQIT, "SaveData", mapFolder, "_HeroA");
		var archived = Path.Combine(this._service.SaveFolderTQIT, "SaveData", mapFolder, "ArchivedCharacters", "_HeroA");

		this._service.ArchiveTogglePath(folder).Should().Be(archived);
		this._service.ArchiveTogglePath(archived).Should().Be(folder);
	}

	#endregion

	#region Resource archives

	[Theory]
	[InlineData("""Items\EquipmentHelm\x.tex""", """Resources\Items.arc""", false)]
	[InlineData("""XPack\Items\x.tex""", """Resources\XPack\Items.arc""", true)]
	[InlineData("""XPack2\Items\x.tex""", """Resources\XPack2\Items.arc""", true)]
	[InlineData("""XPack3\Items\x.tex""", """Resources\XPack3\Items.arc""", true)]
	[InlineData("""XPack4\Item\x.tex""", """Resources\XPack4\Item.arc""", true)]
	[InlineData("""XPack2\Dialog\Caravan\x.mp3""", """Resources\XPack2\Dialog.arc""", true)]
	[InlineData("""Sounds\UI\UI_BAGCHANGE.WAV""", """Audio\Sounds.arc""", false)]
	[InlineData("""Dialog\Greece\Athens_Bodyguard_A1.mp3""", """Audio\Dialog.arc""", false)]
	[InlineData("""PlayerSounds\PCFemaleBowVox01.wav""", """Audio\PlayerSounds.arc""", false)]
	[InlineData("""Music\Bosses\EgyptBossGenericLoop01.wav""", """Audio\Music.arc""", false)]
	// Immortal Throne sounds are in Resources\Sounds.arc, under "xpack/"
	[InlineData("""Sounds\XPack\Ambient\Elysium\ambloop_elysium_day.mp3""", """Resources\Sounds.arc""", false)]
	public void ResolveArcFileName_MapsTheFirstSegmentToTheArchive(string resourceId, string archive, bool isDlc)
	{
		this._service.GamePathTQIT = SteamGame;

		var (arcFileName, isDLC) = this._service.ResolveArcFileName(resourceId);

		arcFileName.Should().BeEquivalentTo(Path.Combine(SteamGame, archive));
		isDLC.Should().Be(isDlc);
	}

	#endregion
}
