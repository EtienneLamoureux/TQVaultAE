using System.Text;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Services;
using TQVaultAE.Tests.DbCli;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// The mod link file in a character's save folder and the mod suggestion, on real files in a temporary folder.
/// </summary>
public sealed class ModLinkServiceTests : IDisposable
{
	private readonly string _root = Path.Combine(Path.GetTempPath(), $"tqvault-modlink-{Guid.NewGuid():N}");
	private readonly string _game;
	private readonly string _save;
	private readonly string _workshop;
	private readonly List<GamePathEntry> _customMaps = [];
	private readonly Mock<IGamePathService> _gamePath = new();
	private readonly ModLinkService _service;

	public ModLinkServiceTests()
	{
		this._game = Path.Combine(this._root, "Titan Quest Anniversary Edition");
		this._save = Path.Combine(this._root, "SaveData", "User", "_HeroA");
		this._workshop = Path.Combine(this._root, "workshop", "content", "475150");
		Directory.CreateDirectory(this._save);

		this._gamePath.Setup(g => g.GetCustomMapList()).Returns(() => [.. this._customMaps]);
		this._gamePath.SetupGet(g => g.GamePathTQIT).Returns(this._game);
		this._gamePath.SetupGet(g => g.GamePathTQ).Returns(this._game);
		this._gamePath.SetupGet(g => g.PlayerSaveFileName).Returns("Player.chr");
		this._gamePath.SetupGet(g => g.PlayerStashFileNameB).Returns("winsys.dxb");

		this._service = new ModLinkService(new Mock<ILogger<ModLinkService>>().Object, this._gamePath.Object, new FileIO(), new PathIO());
	}

	public void Dispose()
	{
		if (Directory.Exists(this._root))
			Directory.Delete(this._root, true);
	}

	private string LinkFile => Path.Combine(this._save, IModLinkService.LinkFileName);

	/// <summary>A Steam Workshop mod, with a database of the given records when there are some.</summary>
	private InstalledMod AddWorkshopMod(string itemId, string name, params string[] records)
	{
		var folder = Path.Combine(this._workshop, itemId, name);
		Directory.CreateDirectory(folder);
		if (records.Length > 0)
			WriteArz(Path.Combine(folder, "database", $"{name}.arz"), records);
		this._customMaps.Add(new GamePathEntry(folder, $"SteamWorkshop : {name}"));
		return new InstalledMod(name, folder, ModSource.SteamWorkshop);
	}

	private static void WriteArz(string file, string[] records)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(file)!);
		File.WriteAllBytes(file, DbCliFixtures.BuildArz([.. records.Select(r => new DbCliFixtures.ArzRec(r, "Test", 0))], new List<string>()));
	}

	private void WriteGameDatabase(params string[] records)
		=> WriteArz(Path.Combine(this._game, "Database", "database.arz"), records);

	/// <summary>A save file holding the records as the game stores strings (32-bit length, then the text).</summary>
	private void WriteSaveFile(string name, params string[] records)
	{
		using var stream = new MemoryStream();
		stream.Write(Encoding.ASCII.GetBytes("header"));
		foreach (var record in records)
		{
			stream.Write(BitConverter.GetBytes(record.Length));
			stream.Write(Encoding.ASCII.GetBytes(record));
		}

		File.WriteAllBytes(Path.Combine(this._save, name), stream.ToArray());
	}

	#region Link file

	[Fact]
	public void Link_ThenResolve_FindsTheLinkedMod()
	{
		var mod = this.AddWorkshopMod("1949340853", "shadowchampions");

		this._service.Link(this._save, mod).IsSuccess.Should().BeTrue();

		var resolved = this._service.Resolve(this._save, isCustomCharacter: true);
		resolved.State.Should().Be(CharacterModState.Linked);
		resolved.Mod.Should().Be(mod);
		File.ReadAllText(this.LinkFile).Should().Contain("\"mod\": \"shadowchampions\"");
	}

	[Fact]
	public void Link_ReplacesThePreviousLink()
	{
		var first = this.AddWorkshopMod("1", "Masteries");
		var second = this.AddWorkshopMod("2", "Lilith");

		this._service.Link(this._save, first);
		this._service.Link(this._save, second);

		this._service.Resolve(this._save, true).Mod.Should().Be(second);
	}

	/// <summary>A custom map (legacy) mod.</summary>
	private InstalledMod AddCustomMap(string name)
	{
		var folder = Path.Combine(this._root, "CustomMaps", name);
		Directory.CreateDirectory(folder);
		this._customMaps.Add(new GamePathEntry(folder, $"Legacy : {name}"));
		return new InstalledMod(name, folder, ModSource.Legacy);
	}

	[Fact]
	public void Link_WritesWhereTheModIsInstalledFrom()
	{
		var mod = this.AddWorkshopMod("1949340853", "shadowchampions");

		this._service.Link(this._save, mod);

		File.ReadAllText(this.LinkFile).Should().Contain("\"source\": \"workshop:1949340853\"")
			.And.NotContain("folder", "the name and the source find the mod on any computer; a path would also carry the user's");
		this._service.ReadLink(this._save)!.Source.Should().Be("workshop:1949340853");
	}

	[Fact]
	public void Resolve_HomonymsLinkWithSource_FindsTheModOfThatSource_IgnoringAFolder()
	{
		// The custom map was linked; the Workshop mod has the same folder name. A "folder" (older files) is not read.
		this.AddWorkshopMod("1949340853", "shadowchampions");
		var customMap = this.AddCustomMap("shadowchampions");
		File.WriteAllText(this.LinkFile, """{ "version": 1, "mod": "shadowchampions", "source": "custommaps", "folder": "D:\\gone\\shadowchampions" }""");

		var resolved = this._service.Resolve(this._save, true);

		resolved.Mod.Should().Be(customMap);
	}

	[Fact]
	public void Resolve_LinkWithoutSource_IsUnlinked()
	{
		// The name and the source make the mod: a link naming the mod alone is not read, even when its folder exists.
		var mod = this.AddWorkshopMod("1949340853", "shadowchampions");
		File.WriteAllText(this.LinkFile, $$"""{ "version": 1, "mod": "shadowchampions", "folder": {{System.Text.Json.JsonSerializer.Serialize(mod.Folder)}} }""");

		this._service.ReadLink(this._save).Should().BeNull();
		this._service.Resolve(this._save, true).State.Should().Be(CharacterModState.Unlinked);
	}

	[Fact]
	public void FindMod_TellsHomonymsApartByTheirSource()
	{
		var workshop = this.AddWorkshopMod("1949340853", "shadowchampions");
		var customMap = this.AddCustomMap("shadowchampions");

		workshop.Id.Should().Be("shadowchampions|workshop:1949340853");
		customMap.Id.Should().Be("shadowchampions|custommaps");
		this._service.FindMod("shadowchampions|custommaps").Should().Be(customMap);
		this._service.FindMod("SHADOWCHAMPIONS|workshop:1949340853").Should().Be(workshop);
	}

	[Fact]
	public void FindMod_NameWithoutSource_FindsNothing()
	{
		this.AddWorkshopMod("756997480", "Masteries");

		this._service.FindMod("Masteries").Should().BeNull();
		this._service.FindMod("Masteries|workshop:756997480").Should().NotBeNull();
		this._service.FindMod(null).Should().BeNull();
	}

	[Fact]
	public void FindMod_SourceNotInstalledHere_AndHomonyms_FindsNothing()
	{
		// Neither homonym is the one named: no guess.
		this.AddWorkshopMod("1949340853", "shadowchampions");
		this.AddCustomMap("shadowchampions");

		this._service.FindMod("shadowchampions|workshop:1").Should().BeNull();
	}

	[Fact]
	public void FindMod_SourceNotInstalledHere_FindsNothingEvenWithTheOnlyModOfThatName()
	{
		// Exported on a computer with the Workshop mod, imported where it is a custom map: another mod.
		this.AddCustomMap("shadowchampions");

		this._service.FindMod("shadowchampions|workshop:1949340853").Should().BeNull();
	}

	[Fact]
	public void Resolve_ModNowInstalledTheOtherWay_SaysTheLinkedModIsMissing()
	{
		var before = this.AddWorkshopMod("1949340853", "shadowchampions");
		this._service.Link(this._save, before);
		this._customMaps.Clear();
		var after = new InstalledMod("shadowchampions", Path.Combine(this._root, "CustomMaps", "shadowchampions"), ModSource.Legacy);
		this._customMaps.Add(new GamePathEntry(after.Folder, "Legacy : shadowchampions"));
		this._service.GetInstalledMods(refresh: true);

		var resolved = this._service.Resolve(this._save, true);

		resolved.State.Should().Be(CharacterModState.LinkedModMissing);
		resolved.Mod.Should().BeNull();
	}

	[Fact]
	public void Resolve_ModNoLongerInstalled_KeepsTheLinkAndSaysTheModIsMissing()
	{
		this._service.Link(this._save, this.AddWorkshopMod("890319896", "Lilith"));
		this._customMaps.Clear();

		this._service.GetInstalledMods(refresh: true);

		var resolved = this._service.Resolve(this._save, true);

		resolved.State.Should().Be(CharacterModState.LinkedModMissing);
		resolved.Mod.Should().BeNull();
		resolved.Link!.Mod.Should().Be("Lilith");
	}

	[Fact]
	public void Resolve_MainCampaignCharacter_IsBaseGameEvenWithALinkFile()
	{
		this._service.Link(this._save, this.AddWorkshopMod("1", "Masteries"));

		this._service.Resolve(this._save, isCustomCharacter: false).Should().Be(CharacterMod.BaseGame);
	}

	[Theory]
	[InlineData("not json")]
	[InlineData("""{ "version": 1 }""")]
	public void Resolve_UnreadableOrEmptyLinkFile_IsUnlinked(string content)
	{
		File.WriteAllText(this.LinkFile, content);

		this._service.Resolve(this._save, true).State.Should().Be(CharacterModState.Unlinked);
	}

	[Fact]
	public void Unlink_RemovesTheLinkFile_AndSucceedsWhenThereIsNone()
	{
		this._service.Link(this._save, this.AddWorkshopMod("1", "Masteries"));

		this._service.Unlink(this._save).IsSuccess.Should().BeTrue();
		File.Exists(this.LinkFile).Should().BeFalse();
		this._service.Unlink(this._save).IsSuccess.Should().BeTrue();
		this._service.Resolve(this._save, true).State.Should().Be(CharacterModState.Unlinked);
	}

	[Fact]
	public void Link_SaveFolderGone_ReportsTheFailure()
	{
		var mod = this.AddWorkshopMod("1", "Masteries");
		Directory.Delete(this._save, true);

		ModLinkResult result = this._service.Link(this._save, mod);

		result.IsSuccess.Should().BeFalse();
		result.Error.Should().BeAssignableTo<IOException>();
	}

	[Fact]
	public void GetInstalledMods_TellsTheSourceAndNamesTheModAfterItsFolder()
	{
		this.AddWorkshopMod("756997480", "Masteries");
		var legacy = Path.Combine(this._root, "CustomMaps", "Diablo 2 Lilith");
		this._customMaps.Add(new GamePathEntry(legacy + Path.DirectorySeparatorChar, "Legacy : Diablo 2 Lilith"));

		var mods = this._service.GetInstalledMods(refresh: true);

		mods.Select(m => (m.Name, m.Source)).Should().Equal(("Masteries", ModSource.SteamWorkshop), ("Diablo 2 Lilith", ModSource.Legacy));
	}

	#endregion

	#region Suggestion

	[Fact]
	public void Suggest_PicksTheModHavingTheMostOfTheRecordsTheGameLacks()
	{
		this.WriteGameDatabase("""records\item\ring.dbr""");
		var expected = this.AddWorkshopMod("1949340853", "shadowchampions", """records\customitems\sword.dbr""", """records\customitems\shield.dbr""");
		this.AddWorkshopMod("756997480", "Masteries", """records\customitems\sword.dbr""");
		this.AddWorkshopMod("1114210901", "ProjectKingsRoad");
		this.WriteSaveFile("Player.chr", """records\item\ring.dbr""", """records\customitems\sword.dbr""");
		this.WriteSaveFile("winsys.dxb", """records\customitems\shield.dbr""");

		var suggestion = this._service.Suggest(this._save);

		suggestion.ModOnlyRecords.Should().Be(2);
		suggestion.Best!.Mod.Should().Be(expected);
		suggestion.Best.Matched.Should().Be(2);
		suggestion.Matches.Select(m => m.Mod.Name).Should().Equal("shadowchampions", "Masteries");
	}

	[Fact]
	public void Suggest_OnlyBaseGameRecords_CannotTell()
	{
		this.WriteGameDatabase("""records\item\ring.dbr""");
		this.AddWorkshopMod("1", "Masteries", """records\item\ring.dbr""");
		this.WriteSaveFile("Player.chr", """records\item\ring.dbr""");

		var suggestion = this._service.Suggest(this._save);

		suggestion.OnlyBaseGameRecords.Should().BeTrue();
		suggestion.Best.Should().BeNull();
	}

	[Fact]
	public void Suggest_TwoModsEquallyGood_SuggestsNeither()
	{
		this.WriteGameDatabase("""records\item\ring.dbr""");
		this.AddWorkshopMod("890319896", "Lilith", """records\customitems\sword.dbr""");
		this.AddWorkshopMod("1", "Diablo 2 Lilith", """records\customitems\sword.dbr""");
		this.WriteSaveFile("Player.chr", """records\customitems\sword.dbr""");

		var suggestion = this._service.Suggest(this._save);

		suggestion.Matches.Should().HaveCount(2);
		suggestion.Best.Should().BeNull();
	}

	[Fact]
	public void Suggest_NeverLinks()
	{
		this.WriteGameDatabase("""records\item\ring.dbr""");
		this.AddWorkshopMod("1", "Masteries", """records\customitems\sword.dbr""");
		this.WriteSaveFile("Player.chr", """records\customitems\sword.dbr""");

		this._service.Suggest(this._save).Best.Should().NotBeNull();

		File.Exists(this.LinkFile).Should().BeFalse();
	}

	[Fact]
	public void SuggestFor_ListsEveryModHavingTheRecords_BestFirst()
	{
		this.WriteGameDatabase("""records\item\ring.dbr""");
		this.AddWorkshopMod("890319896", "Lilith", """records\customitems\sword.dbr""");
		this.AddWorkshopMod("1", "Diablo 2 Lilith", """records\customitems\sword.dbr""", """records\customitems\shield.dbr""");

		var suggestion = this._service.SuggestFor(["""records\item\ring.dbr""", """records\customitems\sword.dbr""", """RECORDS\CUSTOMITEMS\SHIELD.DBR"""]);

		suggestion.ModOnlyRecords.Should().Be(2);
		suggestion.Matches.Select(m => (m.Mod.Name, m.Matched)).Should().Equal(("Diablo 2 Lilith", 2), ("Lilith", 1));
	}

	[Fact]
	public void SuggestFor_RemembersWhereRecordsWereFound_UntilTheModsAreReadAgain()
	{
		this.WriteGameDatabase("""records\item\ring.dbr""");
		this.AddWorkshopMod("1", "Masteries", """records\customitems\sword.dbr""");
		string[] records = ["""records\customitems\sword.dbr"""];
		this._service.SuggestFor(records).Best!.Mod.Name.Should().Be("Masteries");

		// Without its database, the mod is still remembered as having the record...
		foreach (var arz in Directory.GetFiles(this._root, "Masteries.arz", SearchOption.AllDirectories))
			File.Delete(arz);
		this._service.SuggestFor(records).Best!.Mod.Name.Should().Be("Masteries");

		// ...until the installed mods are read again.
		this._service.GetInstalledMods(refresh: true);
		this._service.SuggestFor(records).Matches.Should().BeEmpty();
	}

	#endregion
}
