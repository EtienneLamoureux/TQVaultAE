using System.Text;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// How <see cref="Database"/> finds and loads database.arz, the text archives and the resources of the game, of a
/// separate Immortal Throne install and of a mod or custom map, on a file system faked through
/// <see cref="IFileIO"/>, <see cref="IDirectoryIO"/> and the ARC/ARZ providers.
/// </summary>
public class DatabaseLoadingTests
{
	private static readonly string Root = Path.Combine(Path.GetTempPath(), "tqvault-db-loading");
	private static readonly string GameAE = Path.Combine(Root, "Titan Quest Anniversary Edition");
	private static readonly string GameTQ = Path.Combine(Root, "Titan Quest");
	private static readonly string GameTQIT = Path.Combine(Root, "Titan Quest - Immortal Throne");
	private static readonly string ModFolder = Path.Combine(Root, "workshop", "content", "475150", "1949340853", "shadowchampions");
	private static readonly string SettingsTQ = Path.Combine(Root, "My Games", "Titan Quest", "Settings", "options.txt");
	private static readonly string SettingsTQIT = Path.Combine(Root, "My Games", "Titan Quest - Immortal Throne", "Settings", "options.txt");

	private readonly HashSet<string> _files = new(StringComparer.OrdinalIgnoreCase);
	private readonly HashSet<string> _directories = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, string> _fileContents = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, byte[]> _looseFiles = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<(string Arc, string Entry), byte[]> _arcEntries = [];
	private readonly Dictionary<(string Arz, string Id), DBRecordCollection> _arzRecords = [];
	private readonly List<string> _arzRead = [];

	private readonly Mock<IGamePathService> _gamePath = new();
	private readonly Mock<IFileIO> _fileIO = new();
	private readonly Mock<IDirectoryIO> _directoryIO = new();
	private readonly Mock<IArcFileProvider> _arcProvider = new();
	private readonly Mock<IArzFileProvider> _arzProvider = new();
	private readonly UserSettings _settings = new() { AutoDetectLanguage = false, TQLanguage = "English" };

	public DatabaseLoadingTests()
	{
		this._fileIO.Setup(f => f.Exists(It.IsAny<string>())).Returns<string>(this._files.Contains);
		this._fileIO.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns<string>(p => this._fileContents[p]);
		this._fileIO.Setup(f => f.ReadAllBytes(It.IsAny<string>())).Returns<string>(p => this._looseFiles[p]);
		this._directoryIO.Setup(d => d.Exists(It.IsAny<string>())).Returns<string>(this._directories.Contains);
		this._directoryIO.Setup(d => d.GetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(Array.Empty<string>());

		// The real provider strips the archive name (first segment) from the id before looking up the entry.
		this._arcProvider.Setup(a => a.GetData(It.IsAny<ArcFile>(), It.IsAny<RecordId>()))
			.Returns<ArcFile, RecordId>((arc, id) =>
			{
				var entry = id.Normalized.Contains('\\') ? id.Normalized[(id.Normalized.IndexOf('\\') + 1)..] : id.Normalized;
				return this._arcEntries.TryGetValue((arc.FileName.ToUpperInvariant(), entry.ToUpperInvariant()), out var data) ? data : null;
			});
		this._arzProvider.Setup(a => a.Read(It.IsAny<ArzFile>()))
			.Callback<ArzFile>(arz => this._arzRead.Add(arz.FileName));
		this._arzProvider.Setup(a => a.GetItem(It.IsAny<ArzFile>(), It.IsAny<RecordId>()))
			.Returns<ArzFile, RecordId>((arz, id) => this._arzRecords.TryGetValue((arz.FileName, id.Normalized), out var r) ? r : null);

		this._gamePath.SetupGet(g => g.SettingsFileTQ).Returns(SettingsTQ);
		this._gamePath.SetupGet(g => g.SettingsFileTQIT).Returns(SettingsTQIT);
		this._gamePath.SetupGet(g => g.MapName).Returns(string.Empty);
	}

	private Database CreateDatabase()
		=> new(
			new Mock<ILogger<Database>>().Object
			, this._arcProvider.Object
			, this._arzProvider.Object
			, new Mock<IItemAttributeProvider>().Object
			, this._gamePath.Object
			, new Mock<ITQDataService>().Object
			, this._fileIO.Object
			, this._directoryIO.Object
			, new PathIO()
			, this._settings);

	/// <summary>Anniversary Edition (Steam, GOG): one folder for the game and Immortal Throne.</summary>
	private void UseAnniversaryEdition()
	{
		this._gamePath.SetupGet(g => g.GamePathTQ).Returns(GameAE);
		this._gamePath.SetupGet(g => g.GamePathTQIT).Returns(GameAE);
		this._gamePath.SetupGet(g => g.GamePathAreDifferent).Returns(false);
		this._gamePath.SetupGet(g => g.GameType).Returns(GameType.TQAE);
		this.AddDirectory(GameAE, "Text");
		this.AddArcEntry(Path.Combine(GameAE, "Text", "Text_EN.arc"), "commonequipment.txt", "tagBase=Base text");
		this.AddArcEntry(Path.Combine(GameAE, "Text", "Text_EN.arc"), "xui.txt", "tagIT=Immortal Throne text");
	}

	/// <summary>A custom map or Steam Workshop mod, as selected in the settings.</summary>
	private void UseCustomMap()
	{
		this._gamePath.SetupGet(g => g.IsCustom).Returns(true);
		this._gamePath.SetupGet(g => g.MapName).Returns(ModFolder);
	}

	private void AddDirectory(params string[] parts) => this._directories.Add(Path.Combine(parts));

	private void AddArcEntry(string arcFile, string entry, string content)
		=> this.AddArcEntry(arcFile, entry, Encoding.ASCII.GetBytes(content + "\r\n"));

	private void AddArcEntry(string arcFile, string entry, byte[] data)
	{
		this._files.Add(arcFile);
		// Windows paths: the code may build "Text_en.arc" for "Text_EN.arc".
		this._arcEntries[(arcFile.ToUpperInvariant(), entry.ToUpperInvariant())] = data;
	}

	private void AddArz(string arzFile) => this._files.Add(arzFile);

	/// <summary>A file of a mod shipped unpacked (resource folders instead of archives).</summary>
	private void AddLooseFile(string file, byte[] data)
	{
		this._files.Add(file);
		this._looseFiles[file] = data;
	}

	private DBRecordCollection AddRecord(string arzFile, string id)
	{
		var record = new DBRecordCollection(id, "test");
		this._arzRecords[(arzFile, ((RecordId)id).Normalized)] = record;
		return record;
	}

	#region database.arz

	[Fact]
	public void LoadDBFile_AnniversaryEdition_ReadsTheGameDatabaseOnce()
	{
		this.UseAnniversaryEdition();
		var db = this.CreateDatabase();

		db.LoadDBFile();

		this._arzRead.Should().Equal(Path.Combine(GameAE, "Database", "database.arz"));
		db.ArzFileIT.Should().BeSameAs(db.ArzFile);
		db.ArzFileMod.Should().BeNull();
	}

	[Fact]
	public void LoadDBFile_SeparateImmortalThroneInstall_ImmortalThroneRecordsWin()
	{
		this._gamePath.SetupGet(g => g.GamePathTQ).Returns(GameTQ);
		this._gamePath.SetupGet(g => g.GamePathTQIT).Returns(GameTQIT);
		this._gamePath.SetupGet(g => g.GamePathAreDifferent).Returns(true);
		this._gamePath.SetupGet(g => g.GameType).Returns(GameType.TQIT);
		this.AddDirectory(GameTQIT, "Text");
		this.AddArcEntry(Path.Combine(GameTQIT, "Text", "Text_EN.arc"), "commonequipment.txt", "tagBase=Base text");
		var tqArz = Path.Combine(GameTQ, "Database", "database.arz");
		var itArz = Path.Combine(GameTQIT, "Database", "database.arz");
		this.AddRecord(tqArz, """records\item\shared.dbr""");
		var fromIT = this.AddRecord(itArz, """records\item\shared.dbr""");
		var onlyTQ = this.AddRecord(tqArz, """records\item\tqonly.dbr""");
		var db = this.CreateDatabase();

		db.LoadDBFile();

		this._arzRead.Should().Equal(tqArz, itArz);
		db.GetRecordFromFile("""records\item\shared.dbr""").Should().BeSameAs(fromIT);
		db.GetRecordFromFile("""records\item\tqonly.dbr""").Should().BeSameAs(onlyTQ);
	}

	[Fact]
	public void LoadDBFile_CustomMap_ReadsTheDatabaseNamedAfterTheModFolderAndItsRecordsWin()
	{
		this.UseAnniversaryEdition();
		this.UseCustomMap();
		var gameArz = Path.Combine(GameAE, "Database", "database.arz");
		var modArz = Path.Combine(ModFolder, "database", "shadowchampions.arz");
		this.AddArz(modArz);
		this.AddRecord(gameArz, """records\item\overridden.dbr""");
		var fromMod = this.AddRecord(modArz, """records\item\overridden.dbr""");
		var modOnly = this.AddRecord(modArz, """records\customitems\mi\n_feathertalisman.dbr""");
		var db = this.CreateDatabase();

		db.LoadDBFile();

		this._arzRead.Should().Equal(gameArz, modArz);
		db.ArzFileMod!.FileName.Should().Be(modArz);
		db.GetRecordFromFile("""records\item\overridden.dbr""").Should().BeSameAs(fromMod);
		db.GetRecordFromFile("""records\customitems\mi\n_feathertalisman.dbr""").Should().BeSameAs(modOnly);
	}

	[Fact]
	public void LoadDBFile_CustomMapWithoutDatabase_KeepsOnlyTheGameDatabase()
	{
		this.UseAnniversaryEdition();
		this.UseCustomMap();
		var gameRecord = this.AddRecord(Path.Combine(GameAE, "Database", "database.arz"), """records\item\a.dbr""");
		var db = this.CreateDatabase();

		db.LoadDBFile();

		db.ArzFileMod.Should().BeNull();
		this._arzRead.Should().ContainSingle();
		db.GetRecordFromFile("""records\item\a.dbr""").Should().BeSameAs(gameRecord);
	}

	#endregion

	#region Text archives

	[Fact]
	public void LoadDBFile_AnniversaryEdition_LoadsTheBaseAndImmortalThroneTexts()
	{
		this.UseAnniversaryEdition();
		var db = this.CreateDatabase();

		db.LoadDBFile();

		db.GetFriendlyName("tagBase").Should().Be("Base text");
		db.GetFriendlyName("tagIT").Should().Be("Immortal Throne text");
	}

	[Fact]
	public void LoadDBFile_GameLanguageFrench_PrefersTheFrenchTextArchive()
	{
		this.UseAnniversaryEdition();
		this._settings.TQLanguage = "French";
		this.AddArcEntry(Path.Combine(GameAE, "Text", "Text_FR.arc"), "commonequipment.txt", "tagBase=Texte de base");
		var db = this.CreateDatabase();

		db.LoadDBFile();

		db.GetFriendlyName("tagBase").Should().Be("Texte de base");
	}

	[Fact]
	public void LoadDBFile_CustomMap_ModStringsReplaceTheGameTexts()
	{
		this.UseAnniversaryEdition();
		this.UseCustomMap();
		// Workshop mods ship resources\Text.arc with a single modstrings.txt entry.
		this.AddArcEntry(Path.Combine(ModFolder, "resources", "text.arc"), "modstrings.txt", "tagBase=Mod text\r\ntagModOnly=New mod text");
		var db = this.CreateDatabase();

		db.LoadDBFile();

		db.GetFriendlyName("tagBase").Should().Be("Mod text");
		db.GetFriendlyName("tagModOnly").Should().Be("New mod text");
		db.GetFriendlyName("tagIT").Should().Be("Immortal Throne text");
	}

	[Fact]
	public void LoadDBFile_CustomMapWithTextInTheGameLanguage_PrefersItOverTextArc()
	{
		// Diablo 2 Lilith ships text.arc and text_de/en/es/fr.arc.
		this.UseAnniversaryEdition();
		this.UseCustomMap();
		this._settings.TQLanguage = "French";
		this.AddArcEntry(Path.Combine(ModFolder, "resources", "text.arc"), "modstrings.txt", "tagModOnly=English mod text");
		this.AddArcEntry(Path.Combine(ModFolder, "resources", "text_fr.arc"), "modstrings.txt", "tagModOnly=Texte du mod");
		var db = this.CreateDatabase();

		db.LoadDBFile();

		db.GetFriendlyName("tagModOnly").Should().Be("Texte du mod");
	}

	[Fact]
	public void LoadDBFile_CustomMapWithoutTextInTheGameLanguage_FallsBackToTextArc()
	{
		this.UseAnniversaryEdition();
		this.UseCustomMap();
		this._settings.TQLanguage = "French";
		this.AddArcEntry(Path.Combine(ModFolder, "resources", "text.arc"), "modstrings.txt", "tagModOnly=English mod text");
		this.AddArcEntry(Path.Combine(ModFolder, "resources", "text_de.arc"), "modstrings.txt", "tagModOnly=Modtext");
		var db = this.CreateDatabase();

		db.LoadDBFile();

		db.GetFriendlyName("tagModOnly").Should().Be("English mod text");
	}

	[Fact]
	public void LoadDBFile_ModShippedUnpacked_ReadsTheLooseModStrings()
	{
		// "Masteries x10" ships resources\text\ModStrings.txt instead of resources\text.arc.
		this.UseAnniversaryEdition();
		this.UseCustomMap();
		this.AddLooseFile(Path.Combine(ModFolder, "resources", "text", "ModStrings.txt"), Encoding.ASCII.GetBytes("tagModOnly=Loose mod text\r\n"));
		var db = this.CreateDatabase();

		db.LoadDBFile();

		db.GetFriendlyName("tagModOnly").Should().Be("Loose mod text");
	}

	[Theory]
	[InlineData("French", "FR")]
	[InlineData("english", "EN")]
	[InlineData("Czech", "CZ")]
	[InlineData("Portuguese", "BR")]
	[InlineData("Chinese", "CH")]
	[InlineData("Klingon", null)]
	public void GameLanguageFileCode_UsesTheGameCodes(string language, string? code)
		=> Database.GameLanguageFileCode(language).Should().Be(code);

	[Fact]
	public void LoadDBFile_DiscImmortalThroneWithoutTextFolder_ReadsTheBaseTextFromTheTitanQuestFolder()
	{
		// Unpatched disc install: TQ\Text holds the base game text and TQIT\Resources the Immortal Throne one.
		this._gamePath.SetupGet(g => g.GamePathTQ).Returns(GameTQ);
		this._gamePath.SetupGet(g => g.GamePathTQIT).Returns(GameTQIT);
		this._gamePath.SetupGet(g => g.GamePathAreDifferent).Returns(true);
		this._gamePath.SetupGet(g => g.GameType).Returns(GameType.TQIT);
		this.AddDirectory(GameTQ, "Text");
		this.AddArcEntry(Path.Combine(GameTQ, "Text", "Text_EN.arc"), "commonequipment.txt", "tagBase=Base text from TQ");
		this.AddArcEntry(Path.Combine(GameTQIT, "Resources", "Text_EN.arc"), "xui.txt", "tagIT=Immortal Throne text");
		var db = this.CreateDatabase();

		db.LoadDBFile();

		db.GetFriendlyName("tagBase").Should().Be("Base text from TQ");
		db.GetFriendlyName("tagIT").Should().Be("Immortal Throne text");
	}

	#endregion

	#region Resources

	[Fact]
	public void LoadResource_CustomMap_PrefersTheModArchive()
	{
		this.UseAnniversaryEdition();
		this.UseCustomMap();
		var id = """Items\EquipmentRing\UIBitmaps\ring.tex""";
		var gameArc = Path.Combine(GameAE, "Resources", "Items.arc");
		this._gamePath.Setup(g => g.ResolveArcFileName(It.IsAny<RecordId>())).Returns((gameArc, false));
		this.AddArcEntry(gameArc, """EquipmentRing\UIBitmaps\ring.tex""", [1]);
		this.AddArcEntry(Path.Combine(ModFolder, "resources", "Items.arc"), """EquipmentRing\UIBitmaps\ring.tex""", [2]);
		var db = this.CreateDatabase();

		db.LoadResource(id).Should().Equal(2);
	}

	[Fact]
	public void LoadResource_CustomMapWithoutTheResource_FallsBackToTheGameArchive()
	{
		this.UseAnniversaryEdition();
		this.UseCustomMap();
		var id = """Items\EquipmentRing\UIBitmaps\ring.tex""";
		var gameArc = Path.Combine(GameAE, "Resources", "Items.arc");
		this._gamePath.Setup(g => g.ResolveArcFileName(It.IsAny<RecordId>())).Returns((gameArc, false));
		this.AddArcEntry(gameArc, """EquipmentRing\UIBitmaps\ring.tex""", [1]);
		this.AddArcEntry(Path.Combine(ModFolder, "resources", "Items.arc"), """EquipmentRing\other.tex""", [2]);
		var db = this.CreateDatabase();

		db.LoadResource(id).Should().Equal(1);
	}

	[Fact]
	public void LoadResource_CustomMapExpansionResource_ReadsTheModArchiveNamedAfterTheExpansion()
	{
		// Shadow Champions ships resources\XPack2.arc whose entries start after the "XPack2\" prefix.
		this.UseAnniversaryEdition();
		this.UseCustomMap();
		this._gamePath.Setup(g => g.ResolveArcFileName(It.IsAny<RecordId>()))
			.Returns((Path.Combine(GameAE, "Resources", "XPack2", "Quests.arc"), true));
		this.AddArcEntry(Path.Combine(ModFolder, "resources", "XPack2.arc"), """quests\x2sq17_giesela.qst""", [3]);
		var db = this.CreateDatabase();

		db.LoadResource("""XPack2\quests\x2sq17_giesela.qst""").Should().Equal(3);
	}

	[Fact]
	public void LoadResource_LegacyCustomMapExpansionFolder_ReadsItLikeTheGame()
	{
		// Diablo 2 Lilith ships resources\xpack\item.arc, laid out like the game's Resources\XPack folder.
		this.UseAnniversaryEdition();
		this.UseCustomMap();
		var gameArc = Path.Combine(GameAE, "Resources", "XPack", "Item.arc");
		this._gamePath.Setup(g => g.ResolveArcFileName(It.IsAny<RecordId>())).Returns((gameArc, true));
		this.AddArcEntry(gameArc, """equipmentweapons\shield\u_shield80afx.tex""", [1]);
		this.AddArcEntry(Path.Combine(ModFolder, "resources", "xpack", "item.arc"), """equipmentweapons\shield\u_shield80afx.tex""", [5]);
		var db = this.CreateDatabase();

		db.LoadResource("""XPack\Item\equipmentweapons\shield\u_shield80afx.tex""").Should().Equal(5);
	}

	[Fact]
	public void LoadResource_ModShippedUnpacked_ReadsTheLooseFile()
	{
		// "Masteries x10" ships resources\textures\... folders instead of resources\textures.arc.
		this.UseAnniversaryEdition();
		this.UseCustomMap();
		this._gamePath.Setup(g => g.ResolveArcFileName(It.IsAny<RecordId>()))
			.Returns((Path.Combine(GameAE, "Resources", "textures.arc"), false));
		this.AddLooseFile(Path.Combine(ModFolder, "resources", "textures", "alchemybuttons", "alchbuttonup01.tex"), [6]);
		var db = this.CreateDatabase();

		db.LoadResource("""textures\alchemybuttons\alchbuttonup01.tex""").Should().Equal(6);
	}

	[Fact]
	public void LoadResource_ModArchiveAndLooseFile_PrefersTheArchive()
	{
		this.UseAnniversaryEdition();
		this.UseCustomMap();
		this._gamePath.Setup(g => g.ResolveArcFileName(It.IsAny<RecordId>()))
			.Returns((Path.Combine(GameAE, "Resources", "textures.arc"), false));
		this.AddArcEntry(Path.Combine(ModFolder, "resources", "textures.arc"), """alchemybuttons\alchbuttonup01.tex""", [7]);
		this.AddLooseFile(Path.Combine(ModFolder, "resources", "textures", "alchemybuttons", "alchbuttonup01.tex"), [6]);
		var db = this.CreateDatabase();

		db.LoadResource("""textures\alchemybuttons\alchbuttonup01.tex""").Should().Equal(7);
	}

	[Fact]
	public void LoadResource_ExpansionResource_ReadsTheExpansionArchiveWithoutItsPrefix()
	{
		this.UseAnniversaryEdition();
		var dlcArc = Path.Combine(GameAE, "Resources", "XPack4", "Item.arc");
		this._gamePath.Setup(g => g.ResolveArcFileName(It.IsAny<RecordId>())).Returns((dlcArc, true));
		this.AddArcEntry(dlcArc, """Relics\icon.tex""", [4]);
		var db = this.CreateDatabase();

		db.LoadResource("""XPack4\Item\Relics\icon.tex""").Should().Equal(4);
	}

	[Fact]
	public void LoadResource_MissingArchive_ReturnsNull()
	{
		this.UseAnniversaryEdition();
		this._gamePath.Setup(g => g.ResolveArcFileName(It.IsAny<RecordId>()))
			.Returns((Path.Combine(GameAE, "Resources", "Items.arc"), false));
		var db = this.CreateDatabase();

		db.LoadResource("""Items\missing.tex""").Should().BeNull();
		this._arcProvider.Verify(a => a.ReadARCToC(It.IsAny<ArcFile>()), Times.Never);
	}

	[Fact]
	public void GetTextureSize_ReadsTheHeaders_AndKeepsOnlyTheSize()
	{
		this.UseAnniversaryEdition();
		var gameArc = Path.Combine(GameAE, "Resources", "Items.arc");
		this._gamePath.Setup(g => g.ResolveArcFileName(It.IsAny<RecordId>())).Returns((gameArc, false));
		this.AddArcEntry(gameArc, """EquipmentRing\UIBitmaps\ring.tex""", TexHeaderTests.Tex(version: 1, width: 64, height: 96));
		var db = this.CreateDatabase();
		RecordId id = """Items\EquipmentRing\UIBitmaps\ring.tex""";

		db.GetTextureSize(id).Should().Be((64, 96));
		db.GetTextureSize(id).Should().Be((64, 96));
		db.LoadResource(id).Should().NotBeNull();

		// One read for the size (then cached), one for the data: the size lookup does not keep the texture data.
		this._arcProvider.Verify(a => a.GetData(It.IsAny<ArcFile>(), It.IsAny<RecordId>()), Times.Exactly(2));
	}

	[Fact]
	public void GetTextureSize_OfAMissingResource_ReturnsNull()
	{
		this.UseAnniversaryEdition();
		this._gamePath.Setup(g => g.ResolveArcFileName(It.IsAny<RecordId>()))
			.Returns((Path.Combine(GameAE, "Resources", "Items.arc"), false));
		var db = this.CreateDatabase();

		db.GetTextureSize("""Items\missing.tex""").Should().BeNull();
		db.GetTextureSize("DefaultBitmap").Should().BeNull();
	}

	#endregion

	#region Game language from the game settings

	private void SetGameSettings(string? tqLanguage, string? tqitLanguage)
	{
		this._settings.AutoDetectLanguage = true;
		if (tqLanguage is not null)
		{
			this._files.Add(SettingsTQ);
			this._fileContents[SettingsTQ] = $"language                  = {tqLanguage}\r\n";
		}

		if (tqitLanguage is not null)
		{
			this._files.Add(SettingsTQIT);
			this._fileContents[SettingsTQIT] = $"language                  = \"{tqitLanguage}\"\r\n";
		}
	}

	[Theory]
	[InlineData(GameType.TQAE)]
	[InlineData(GameType.TQIT)]
	public void GameLanguage_ImmortalThroneOrAnniversary_ReadsTheImmortalThroneSettings(GameType gameType)
	{
		// Both settings files exist when the original game was played once: the Anniversary Edition and Immortal
		// Throne only write the Immortal Throne one, so the other is stale.
		this._gamePath.SetupGet(g => g.GameType).Returns(gameType);
		this.SetGameSettings(tqLanguage: "French", tqitLanguage: "English");
		var db = this.CreateDatabase();

		db.GameLanguage.Should().Be("ENGLISH");
	}

	[Fact]
	public void GameLanguage_OriginalTitanQuest_ReadsTheTitanQuestSettings()
	{
		this._gamePath.SetupGet(g => g.GameType).Returns(GameType.TQ);
		this.SetGameSettings(tqLanguage: "French", tqitLanguage: "English");
		var db = this.CreateDatabase();

		db.GameLanguage.Should().Be("FRENCH");
	}

	[Fact]
	public void GameLanguage_AnniversaryWithOnlyTheTitanQuestSettings_FallsBackToThem()
	{
		this._gamePath.SetupGet(g => g.GameType).Returns(GameType.TQAE);
		this.SetGameSettings(tqLanguage: "German", tqitLanguage: null);
		var db = this.CreateDatabase();

		db.GameLanguage.Should().Be("GERMAN");
	}

	#endregion

	#region One database per map

	private DatabaseSelector CreateSelector()
		=> new(new Mock<ILogger<DatabaseSelector>>().Object, this.CreateDatabase);

	[Fact]
	public void Selector_SwitchingMaps_EachMapAnswersWithItsOwnRecords()
	{
		this.UseAnniversaryEdition();
		var gameArz = Path.Combine(GameAE, "Database", "database.arz");
		var modArz = Path.Combine(ModFolder, "database", "shadowchampions.arz");
		this.AddArz(modArz);
		var fromGame = this.AddRecord(gameArz, """records\item\overridden.dbr""");
		var fromMod = this.AddRecord(modArz, """records\item\overridden.dbr""");
		this.AddRecord(modArz, """records\customitems\modonly.dbr""");
		var selector = this.CreateSelector();
		selector.LoadDBFile(); // the base game, as set at startup

		selector.GetRecordFromFile("""records\customitems\modonly.dbr""").Should().BeNull();
		selector.Activate(ModFolder).Should().BeTrue();
		selector.GetRecordFromFile("""records\item\overridden.dbr""").Should().BeSameAs(fromMod);
		selector.GetRecordFromFile("""records\customitems\modonly.dbr""").Should().NotBeNull();
		selector.Activate("main").Should().BeTrue();
		selector.GetRecordFromFile("""records\item\overridden.dbr""").Should().BeSameAs(fromGame);
		selector.GetRecordFromFile("""records\customitems\modonly.dbr""").Should().BeNull();
	}

	[Fact]
	public void Selector_GoingBackToAMap_KeepsItsDatabaseLoaded()
	{
		this.UseAnniversaryEdition();
		var gameArz = Path.Combine(GameAE, "Database", "database.arz");
		var modArz = Path.Combine(ModFolder, "database", "shadowchampions.arz");
		this.AddArz(modArz);
		var selector = this.CreateSelector();
		selector.LoadDBFile();

		selector.Activate(ModFolder);
		selector.Activate(string.Empty);
		selector.Activate(ModFolder + Path.DirectorySeparatorChar).Should().BeTrue();
		selector.Activate(ModFolder).Should().BeFalse();

		// The game database once for each map, the mod's once.
		this._arzRead.Should().Equal(gameArz, gameArz, modArz);
		selector.LoadedMapFolders.Should().BeEquivalentTo([string.Empty, ModFolder]);
		selector.ActiveMapFolder.Should().Be(ModFolder);
	}

	[Fact]
	public void Selector_Preload_LoadsTheMapWithoutActivatingIt()
	{
		this.UseAnniversaryEdition();
		var gameArz = Path.Combine(GameAE, "Database", "database.arz");
		var modArz = Path.Combine(ModFolder, "database", "shadowchampions.arz");
		this.AddArz(modArz);
		var selector = this.CreateSelector();
		selector.LoadDBFile();

		selector.Preload(ModFolder);

		selector.LoadedMapFolders.Should().BeEquivalentTo([string.Empty, ModFolder]);
		selector.ActiveMapFolder.Should().BeEmpty();

		// Its first use finds it loaded.
		selector.Activate(ModFolder).Should().BeTrue();
		this._arzRead.Should().Equal(gameArz, gameArz, modArz);
	}

	[Fact]
	public void Selector_WhileAModLoads_AMapAlreadyLoadedStaysUsable()
	{
		// Arrange: the mod's database blocks in its read until released
		this.UseAnniversaryEdition();
		var gameArz = Path.Combine(GameAE, "Database", "database.arz");
		var modArz = Path.Combine(ModFolder, "database", "shadowchampions.arz");
		this.AddArz(modArz);
		var gameRecord = this.AddRecord(gameArz, """records\item\a.dbr""");
		using var reading = new ManualResetEventSlim();
		using var release = new ManualResetEventSlim();
		this._arzProvider.Setup(a => a.Read(It.IsAny<ArzFile>())).Callback<ArzFile>(arz =>
		{
			if (arz.FileName != modArz)
				return;
			reading.Set();
			release.Wait(TimeSpan.FromSeconds(10));
		});
		var selector = this.CreateSelector();
		selector.LoadDBFile();

		// Act: the mod loads in the background; meanwhile another thread reads the base game
		var preload = Task.Run(() => selector.Preload(ModFolder));
		reading.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue();
		var baseGameRead = Task.Run(() =>
		{
			using (selector.Use(string.Empty))
				return selector.GetRecordFromFile("""records\item\a.dbr""");
		});
		bool readWhileLoading = baseGameRead.Wait(TimeSpan.FromSeconds(5));
		release.Set();
		preload.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue();

		// Assert
		readWhileLoading.Should().BeTrue();
		baseGameRead.Result.Should().BeSameAs(gameRecord);
		selector.LoadedMapFolders.Should().BeEquivalentTo([string.Empty, ModFolder]);
	}

	[Fact]
	public void Selector_FailedLoad_IsTriedAgainAtTheNextUse()
	{
		this.UseAnniversaryEdition();
		var modArz = Path.Combine(ModFolder, "database", "shadowchampions.arz");
		this.AddArz(modArz);
		int modReads = 0;
		this._arzProvider.Setup(a => a.Read(It.IsAny<ArzFile>())).Callback<ArzFile>(arz =>
		{
			if (arz.FileName == modArz && ++modReads == 1)
				throw new IOException("locked");
		});
		var selector = this.CreateSelector();
		selector.LoadDBFile();

		var preload = () => selector.Preload(ModFolder);

		preload.Should().Throw<IOException>();
		selector.LoadedMapFolders.Should().BeEquivalentTo([string.Empty]);
		selector.Activate(ModFolder).Should().BeTrue();
		modReads.Should().Be(2);
		selector.ActiveMapFolder.Should().Be(ModFolder);
	}

	[Fact]
	public void Database_LoadedForAMod_KeepsItsModWhenTheSessionMapChanges()
	{
		this.UseAnniversaryEdition();
		this.UseCustomMap();
		var id = """Items\EquipmentRing\UIBitmaps\ring.tex""";
		var gameArc = Path.Combine(GameAE, "Resources", "Items.arc");
		this._gamePath.Setup(g => g.ResolveArcFileName(It.IsAny<RecordId>())).Returns((gameArc, false));
		this.AddArcEntry(gameArc, """EquipmentRing\UIBitmaps\ring.tex""", [1]);
		this.AddArcEntry(Path.Combine(ModFolder, "resources", "Items.arc"), """EquipmentRing\UIBitmaps\ring.tex""", [2]);
		var db = this.CreateDatabase();
		db.LoadDBFile();

		// Another character's map becomes the session's: this database still reads its own mod.
		this._gamePath.SetupGet(g => g.IsCustom).Returns(false);
		this._gamePath.SetupGet(g => g.MapName).Returns("main");

		db.LoadResource(id).Should().Equal(2);
	}

	#endregion
}
