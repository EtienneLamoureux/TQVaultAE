using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Session;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Which map a character is played with, and what switching the active map does.
/// </summary>
public class GameMapServiceTests
{
	private static readonly string Shadow = Path.Combine(Path.GetTempPath(), "workshop", "1949340853", "shadowchampions");
	private static readonly string Masteries = Path.Combine(Path.GetTempPath(), "workshop", "756997480", "Masteries");

	private readonly Mock<IDatabaseMaps> _maps = new();
	private readonly Mock<IGamePathService> _gamePath = new();
	private readonly Mock<IModLinkService> _links = new();
	private readonly Mock<IGameDataCache> _cache = new();
	private string _active = string.Empty;

	public GameMapServiceTests()
	{
		this._maps.SetupGet(m => m.ActiveMapFolder).Returns(() => this._active);
		this._maps.Setup(m => m.Activate(It.IsAny<string?>())).Returns<string?>(map =>
		{
			string key = GameMaps.Normalize(map);
			bool changed = !GameMaps.Same(key, this._active);
			this._active = key;
			return changed;
		});
		this._gamePath.SetupProperty(g => g.MapName, string.Empty);
		this._links.Setup(l => l.Resolve(It.IsAny<string>(), false)).Returns(CharacterMod.BaseGame);
		this._links.Setup(l => l.Resolve(It.IsAny<string>(), true)).Returns(CharacterMod.Unlinked);
	}

	private GameMapService CreateService()
		=> new(new Mock<ILogger<GameMapService>>().Object, this._maps.Object, this._gamePath.Object, this._links.Object
			, [this._cache.Object]);

	private static PlayerSave Save(string name, bool isCustom)
	{
		var pathIO = new Mock<IPathIO>();
		pathIO.Setup(p => p.GetFileName(It.IsAny<string>())).Returns<string>(Path.GetFileName);
		return new PlayerSave(Path.Combine(Path.GetTempPath(), isCustom ? "User" : "Main", "_" + name), true, false, isCustom, string.Empty
			, new Mock<ITranslationService>().Object, pathIO.Object);
	}

	private void LinkTo(PlayerSave save, string modFolder)
	{
		var mod = new InstalledMod(Path.GetFileName(modFolder), modFolder, ModSource.SteamWorkshop);
		this._links.Setup(l => l.Resolve(save.Folder, save.IsCustom)).Returns(new CharacterMod(CharacterModState.Linked
			, mod, new ModLinkFile { Mod = mod.Name }));
		this._links.Setup(l => l.ModIdOfMap(modFolder)).Returns(mod.Id);
	}

	/// <summary>The shared stash files of a map, as GamePathServiceWin names them.</summary>
	private void SharedStashFiles()
	{
		string Sys(string map, string file) => map.Length == 0
			? Path.Combine(Path.GetTempPath(), "Sys", file)
			: Path.Combine(Path.GetTempPath(), "Sys", Path.GetFileName(map), file);
		this._gamePath.Setup(g => g.TransferStashFileFullPathFor(It.IsAny<string>())).Returns<string>(map => Sys(map, "winsys.dxb"));
		this._gamePath.Setup(g => g.RelicVaultStashFileFullPathFor(It.IsAny<string>())).Returns<string>(map => Sys(map, "miscsys.dxb"));
	}

	[Fact]
	public void CharacterShowing_AnotherMapsSharedStash_GivesTheFirstCharacterPlayedWithThatMap()
	{
		this.SharedStashFiles();
		var heroA = Save("HeroA", isCustom: false);
		var heroB = Save("HeroB", isCustom: true);
		var heroC = Save("HeroC", isCustom: true);
		this.LinkTo(heroB, Masteries);
		this.LinkTo(heroC, Shadow);
		string shadowRelics = Path.Combine(Path.GetTempPath(), "Sys", "shadowchampions", "miscsys.dxb");

		this.CreateService().CharacterShowing(shadowRelics, [heroA, heroB, heroC], heroA).Should().BeSameAs(heroC);
	}

	[Fact]
	public void CharacterShowing_SelectedCharacterShowsTheFile_KeepsIt()
	{
		this.SharedStashFiles();
		var heroA = Save("HeroA", isCustom: false);
		// An unlinked custom map character is played with the base game data, and its shared stashes.
		var heroB = Save("HeroB", isCustom: true);
		string transfer = Path.Combine(Path.GetTempPath(), "Sys", "WINSYS.DXB");

		this.CreateService().CharacterShowing(transfer, [heroA, heroB], heroB).Should().BeSameAs(heroB);
	}

	[Fact]
	public void CharacterShowing_NoCharacterPlayedWithTheMap_GivesNone()
	{
		this.SharedStashFiles();
		var heroB = Save("HeroB", isCustom: true);
		this.LinkTo(heroB, Masteries);
		string baseTransfer = Path.Combine(Path.GetTempPath(), "Sys", "winsys.dxb");

		this.CreateService().CharacterShowing(baseTransfer, [heroB], null).Should().BeNull();
	}

	[Fact]
	public async Task PreloadMapsAsync_LoadsEachLinkedModOnce_WithoutActivatingIt()
	{
		// Arrange: two characters on one mod, one on another, a campaign and an unlinked character
		var heroA = Save("HeroA", isCustom: true);
		var heroC = Save("HeroC", isCustom: true);
		var heroE = Save("HeroE", isCustom: true);
		this.LinkTo(heroA, Shadow);
		this.LinkTo(heroC, Shadow);
		this.LinkTo(heroE, Masteries);

		// Act
		await this.CreateService().PreloadMapsAsync([heroA, Save("HeroD", isCustom: false), heroC, Save("HeroB", isCustom: true), heroE]);

		// Assert
		this._maps.Verify(m => m.Preload(Shadow), Times.Once);
		this._maps.Verify(m => m.Preload(Masteries), Times.Once);
		this._maps.Verify(m => m.Preload(It.IsAny<string?>()), Times.Exactly(2));
		this._maps.Verify(m => m.Activate(It.IsAny<string?>()), Times.Never);
	}

	[Fact]
	public async Task PreloadMapsAsync_AFailedLoad_IsLoggedAndTheOthersGoOn()
	{
		var heroA = Save("HeroA", isCustom: true);
		var heroE = Save("HeroE", isCustom: true);
		this.LinkTo(heroA, Shadow);
		this.LinkTo(heroE, Masteries);
		this._maps.Setup(m => m.Preload(Shadow)).Throws(new IOException("locked"));

		await this.CreateService().PreloadMapsAsync([heroA, heroE]);

		this._maps.Verify(m => m.Preload(Masteries), Times.Once);
	}

	[Fact]
	public void MapFor_MainCampaignCharacter_IsTheBaseGame()
		=> this.CreateService().MapFor(Save("HeroC", isCustom: false)).Should().BeEmpty();

	[Fact]
	public void MapFor_LinkedCharacter_IsItsMod()
	{
		var heroA = Save("HeroA", isCustom: true);
		this.LinkTo(heroA, Shadow);

		this.CreateService().MapFor(heroA).Should().Be(Shadow);
	}

	[Fact]
	public void MapFor_UnlinkedCharacter_IsTheBaseGame()
		=> this.CreateService().MapFor(Save("HeroB", isCustom: true)).Should().BeEmpty();

	[Fact]
	public void MapFor_CharacterLinkedToAModNotInstalled_IsTheBaseGame()
	{
		var heroB = Save("HeroB", isCustom: true);
		this._links.Setup(l => l.Resolve(heroB.Folder, true)).Returns(new CharacterMod(CharacterModState.LinkedModMissing
			, null, new ModLinkFile { Mod = Path.GetFileName(Masteries) }));

		this.CreateService().MapFor(heroB).Should().BeEmpty();
	}

	[Fact]
	public void ActivateFor_AnotherMap_SwitchesTheDatabaseAndTheStashPaths_AndEmptiesTheCaches()
	{
		var service = this.CreateService();
		var heroA = Save("HeroA", isCustom: true);
		this.LinkTo(heroA, Shadow);
		int raised = 0;
		service.ActiveMapChanged += (_, _) => raised++;

		service.ActivateFor(heroA).Should().BeTrue();

		service.ActiveMap.Should().Be(Shadow);
		this._gamePath.Object.MapName.Should().Be(Shadow);
		this._cache.Verify(c => c.ClearGameDataCache(), Times.Once);
		raised.Should().Be(1);
		heroA.CustomMap.Should().Be(Shadow);
	}

	[Fact]
	public void ActivateFor_SameMap_KeepsTheCaches()
	{
		var service = this.CreateService();
		int raised = 0;
		service.ActiveMapChanged += (_, _) => raised++;

		service.ActivateFor(Save("HeroC", isCustom: false)).Should().BeFalse();

		this._gamePath.Object.MapName.Should().Be("main");
		this._cache.Verify(c => c.ClearGameDataCache(), Times.Never);
		raised.Should().Be(0);
	}

	[Fact]
	public void BounceMod_MainCampaignCharacterLinkedToAMod_ReadsItsData_KeepsTheBaseGameSharedStashes()
	{
		var service = this.CreateService();
		var heroC = Save("HeroC", isCustom: false);
		this.LinkTo(heroC, Shadow);

		service.ActivateFor(heroC).Should().BeTrue();

		service.MapFor(heroC).Should().Be(Shadow);
		service.ActiveMap.Should().Be(Shadow);
		service.SharedStashMapFor(heroC).Should().BeEmpty();
		service.ActiveSharedStashMap.Should().BeEmpty();
		this._gamePath.Object.MapName.Should().Be("main");
		// The base game's shared stashes take its data.
		service.SharedStashDataOf(string.Empty).Should().Be(new SharedStashData(Shadow, service.ModNameFor(heroC)));
	}

	[Fact]
	public void SharedStashDataOf_BaseGameFiles_KeepTheDataOfTheLastCharacterShownWithThem()
	{
		var service = this.CreateService();
		var heroC = Save("HeroC", isCustom: false);
		var heroB = Save("HeroB", isCustom: true);
		this.LinkTo(heroC, Shadow);
		this.LinkTo(heroB, Masteries);

		service.ActivateFor(heroC);
		service.ActivateFor(heroB);

		// A custom map character shows its mod's files, read with that mod; the base game's keep the bounce mod.
		service.ActiveSharedStashMap.Should().Be(Masteries);
		service.SharedStashDataOf(Masteries).Map.Should().Be(Masteries);
		service.SharedStashDataOf(string.Empty).Map.Should().Be(Shadow);

		service.ActivateFor(Save("HeroD", isCustom: false));
		service.SharedStashDataOf(string.Empty).Should().Be(new SharedStashData(string.Empty, null));
	}

	[Fact]
	public void ActivateFor_SameDatabaseOtherSharedStashes_TellsTheChange()
	{
		// A bounce character and a custom map character of the same mod: one database, two pairs of shared stashes.
		var service = this.CreateService();
		var heroC = Save("HeroC", isCustom: false);
		var heroB = Save("HeroB", isCustom: true);
		this.LinkTo(heroC, Shadow);
		this.LinkTo(heroB, Shadow);
		service.ActivateFor(heroC);
		int raised = 0;
		service.ActiveMapChanged += (_, _) => raised++;

		service.ActivateFor(heroB).Should().BeTrue();
		this._gamePath.Object.MapName.Should().Be(Shadow);
		service.ActivateFor(heroC).Should().BeTrue();
		service.ActivateFor(heroC).Should().BeFalse();

		raised.Should().Be(2);
		this._cache.Verify(c => c.ClearGameDataCache(), Times.Once);
	}

	[Fact]
	public void CharacterShowing_BaseGameSharedStash_PrefersACharacterOfTheDataItIsReadWith()
	{
		this.SharedStashFiles();
		var heroA = Save("HeroA", isCustom: false);
		var heroC = Save("HeroC", isCustom: false);
		var heroB = Save("HeroB", isCustom: true);
		this.LinkTo(heroC, Shadow);
		this.LinkTo(heroB, Masteries);
		var service = this.CreateService();
		service.ActivateFor(heroC);
		service.ActivateFor(heroB);
		string transfer = Path.Combine(Path.GetTempPath(), "Sys", "winsys.dxb");

		service.CharacterShowing(transfer, [heroA, heroB, heroC], heroB).Should().BeSameAs(heroC);
	}

	[Fact]
	public void LoadedWithOtherMap_TellsWhenACharacterWasReadWithAnotherDatabase()
	{
		var service = this.CreateService();
		var heroB = Save("HeroB", isCustom: true);
		service.LoadedWithOtherMap(heroB).Should().BeFalse(); // never read

		service.MarkLoaded(heroB); // read with the base game data, unlinked
		service.LoadedWithOtherMap(heroB).Should().BeFalse();

		this.LinkTo(heroB, Shadow);
		service.ActivateFor(heroB);
		service.LoadedWithOtherMap(heroB).Should().BeTrue();

		service.MarkLoaded(heroB);
		service.LoadedWithOtherMap(heroB).Should().BeFalse();
	}

	[Fact]
	public void Refresh_GivesCustomMapCharactersTheStateAndNameOfTheirMod()
	{
		var service = this.CreateService();
		var linked = Save("HeroA", isCustom: true);
		this.LinkTo(linked, Shadow);
		var missing = Save("Carl", isCustom: true);
		this._links.Setup(l => l.Resolve(missing.Folder, true))
			.Returns(new CharacterMod(CharacterModState.LinkedModMissing, null, new ModLinkFile { Mod = "Lilith" }));
		var unlinked = Save("HeroB", isCustom: true);
		var main = Save("HeroC", isCustom: false);

		foreach (var save in new[] { linked, missing, unlinked, main })
			service.Refresh(save);

		(linked.ModState, linked.ModName).Should().Be((CharacterModState.Linked, "shadowchampions"));
		(missing.ModState, missing.ModName).Should().Be((CharacterModState.LinkedModMissing, "Lilith"));
		(unlinked.ModState, unlinked.ModName).Should().Be((CharacterModState.Unlinked, (string?)null));
		(main.ModState, main.ModName).Should().Be((CharacterModState.BaseGame, (string?)null));
	}
}
