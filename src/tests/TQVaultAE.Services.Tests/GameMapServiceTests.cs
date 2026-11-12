using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
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
		=> this._links.Setup(l => l.Resolve(save.Folder, true)).Returns(new CharacterMod(CharacterModState.Linked
			, new InstalledMod(Path.GetFileName(modFolder), modFolder, ModSource.SteamWorkshop), new ModLinkFile { Mod = Path.GetFileName(modFolder) }));

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
	public void Refresh_LabelsCustomMapCharactersByTheirMod()
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

		linked.ModLabel.Should().Be("shadowchampions");
		missing.ModLabel.Should().Contain("Lilith");
		unlinked.ModLabel.Should().NotBeNullOrEmpty();
		main.ModLabel.Should().BeNull();
		(linked.ModState, linked.ModName).Should().Be((CharacterModState.Linked, "shadowchampions"));
		(missing.ModState, missing.ModName).Should().Be((CharacterModState.LinkedModMissing, "Lilith"));
		(unlinked.ModState, unlinked.ModName).Should().Be((CharacterModState.Unlinked, (string?)null));
		(main.ModState, main.ModName).Should().Be((CharacterModState.BaseGame, (string?)null));
		linked.ToString().Should().EndWith("[shadowchampions]");
		main.ToString().Should().NotContain("[");
	}
}
