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
			.Callback<ArzFile>(arz =>
			{
				this._arzRead.Add(arz.FileName);
				foreach (var (_, record) in this._arzRecords.Where(r => r.Key.Arz == arz.FileName))
					arz.RecordInfo[record.Id] = new ArzRecordInfo(0, record.Id.Raw, record.RecordType, 0, 0, default);
			});
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

	private DBRecordCollection AddRecord(string arzFile, string id, string recordType = "test")
	{
		var record = new DBRecordCollection(id, recordType);
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

	#region Affix tables

	private static Variable Str(string name, string value)
	{
		var variable = new Variable(name, VariableDataType.StringVar, 1);
		variable[0] = value;
		return variable;
	}

	private static Variable Int(string name, int value)
	{
		var variable = new Variable(name, VariableDataType.Integer, 1);
		variable[0] = value;
		return variable;
	}

	private static Variable Flt(string name, float value)
	{
		var variable = new Variable(name, VariableDataType.Float, 1);
		variable[0] = value;
		return variable;
	}

	/// <summary>A fixed weight loot item table dropping <paramref name="items"/> (weight 1 each), every kind rolled at 100 %.</summary>
	private DBRecordCollection AddFixedLootTable(string id, params string[] items)
	{
		var arz = Path.Combine(GameAE, "Database", "database.arz");
		var table = this.AddRecord(arz, id, "LootItemTable_FixedWeight");
		for (int i = 0; i < items.Length; i++)
		{
			table.Set(Str($"lootName{i + 1}", items[i]));
			table.Set(Int($"lootWeight{i + 1}", 1));
		}

		table.Set(Flt("brokenRandomizerChance", 100F));
		table.Set(Flt("prefixRandomizerChance", 100F));
		table.Set(Flt("suffixRandomizerChance", 100F));
		return table;
	}

	/// <summary>One line per affix table of the map: loot item table, kind, affix table and weight.</summary>
	private static string[]? Describe(IEnumerable<AffixTableMapItem>? map)
		=> map?.SelectMany(m => m.Broken.Select(t => ("broken", t)).Concat(m.Prefix.Select(t => ("prefix", t))).Concat(m.Suffix.Select(t => ("suffix", t)))
			.Select(k => $"{m.LootTable.Raw} {k.Item1} {k.Item2.Table.Raw} {k.Item2.Weight}"))
			.ToArray();

	/// <summary>Changes the value of a variable the record already has.</summary>
	private static void Change(DBRecordCollection record, string name, object value)
		=> record.First(v => v.Name == name)[0] = value;

	/// <summary>Adds a weighted affix table to a slot of a loot item table.</summary>
	private static void AddAffixTable(DBRecordCollection lootTable, string kind, int slot, string table, int weight = 10)
	{
		lootTable.Set(Str($"{kind}RandomizerName{slot}", table));
		lootTable.Set(Int($"{kind}RandomizerWeight{slot}", weight));
	}

	[Fact]
	public void GetItemAffixTableMap_FixedWeightLootTable_MapsEachLootNameToTheNumberedAffixTables()
	{
		this.UseAnniversaryEdition();
		var table = this.AddFixedLootTable("""records\loot\table_a.dbr""", """records\item\sword.dbr""", """records\item\axe.dbr""", "");
		table.Set(Int("bothPrefixSuffix", 50));
		table.Set(Int("brokenOnly", 10));
		table.Set(Str("brokenRandomizerName1", ""));
		AddAffixTable(table, "prefix", 2, """records\affix\prefix2.dbr""");
		AddAffixTable(table, "prefix", 1, """records\affix\prefix1.dbr""");
		AddAffixTable(table, "suffix", 1, """records\affix\suffix1.dbr""");
		AddAffixTable(table, "broken", 3, """records\affix\broken3.dbr""");
		table.Set(Str("suffixRandomizerName4", " "));
		var db = this.CreateDatabase();

		db.LoadDBFile();

		var expected = new[]
		{
			"""records\loot\table_a.dbr broken records\affix\broken3.dbr 10""",
			"""records\loot\table_a.dbr prefix records\affix\prefix2.dbr 10""",
			"""records\loot\table_a.dbr prefix records\affix\prefix1.dbr 10""",
			"""records\loot\table_a.dbr suffix records\affix\suffix1.dbr 10""",
		};
		Describe(db.GetItemAffixTableMap("""records\item\sword.dbr""")).Should().BeEquivalentTo(expected);
		Describe(db.GetItemAffixTableMap("""records\item\axe.dbr""")).Should().BeEquivalentTo(expected);
	}

	[Fact]
	public void GetItemAffixTableMap_FixedWeightClassInAnotherCase_StillReadsItsLootNames()
	{
		// The class filter ignores case: the class test that picks lootName<N> over itemNames must too.
		this.UseAnniversaryEdition();
		var arz = Path.Combine(GameAE, "Database", "database.arz");
		var table = this.AddRecord(arz, """records\loot\table_c.dbr""", "lootitemtable_fixedweight");
		table.Set(Str("lootName1", """records\item\sword.dbr"""));
		table.Set(Int("lootWeight1", 5));
		table.Set(Flt("prefixRandomizerChance", 20F));
		AddAffixTable(table, "prefix", 1, """records\affix\prefix1.dbr""");
		var db = this.CreateDatabase();

		db.LoadDBFile();

		Describe(db.GetItemAffixTableMap("""records\item\sword.dbr""")).Should()
			.Equal("""records\loot\table_c.dbr prefix records\affix\prefix1.dbr 10""");
	}

	[Fact]
	public void GetItemAffixTableMap_DynamicWeightLootTable_MapsItsItemNames()
	{
		this.UseAnniversaryEdition();
		var arz = Path.Combine(GameAE, "Database", "database.arz");
		var table = this.AddRecord(arz, """records\loot\table_b.dbr""", "LootItemTable_DynWeight");
		var itemNames = new Variable("itemNames", VariableDataType.StringVar, 2);
		itemNames[0] = """records\item\ring.dbr""";
		itemNames[1] = """records\item\amulet.dbr""";
		table.Set(itemNames);
		table.Set(Flt("prefixRandomizerChance", 30F));
		AddAffixTable(table, "prefix", 1, """records\affix\prefix1.dbr""");
		var db = this.CreateDatabase();

		db.LoadDBFile();

		var expected = new[] { """records\loot\table_b.dbr prefix records\affix\prefix1.dbr 10""" };
		Describe(db.GetItemAffixTableMap("""records\item\ring.dbr""")).Should().Equal(expected);
		Describe(db.GetItemAffixTableMap("""records\item\amulet.dbr""")).Should().Equal(expected);
	}

	[Fact]
	public void GetItemAffixTableMap_ItemSlotWithoutWeight_IsNeverDroppedSoGetsNoAffixes()
	{
		this.UseAnniversaryEdition();
		var table = this.AddFixedLootTable("""records\loot\table_a.dbr""", """records\item\sword.dbr""", """records\item\axe.dbr""");
		Change(table, "lootWeight2", 0);
		AddAffixTable(table, "prefix", 1, """records\affix\prefix1.dbr""");
		var db = this.CreateDatabase();

		db.LoadDBFile();

		db.GetItemAffixTableMap("""records\item\sword.dbr""").Should().ContainSingle();
		db.GetItemAffixTableMap("""records\item\axe.dbr""").Should().BeNull();
	}

	[Fact]
	public void GetItemAffixTableMap_AffixTableWithoutWeight_IsLeftOut()
	{
		// Weight 0 and no weight at all: the game never picks the table.
		this.UseAnniversaryEdition();
		var table = this.AddFixedLootTable("""records\loot\table_a.dbr""", """records\item\sword.dbr""");
		AddAffixTable(table, "prefix", 1, """records\affix\prefix1.dbr""");
		AddAffixTable(table, "suffix", 1, """records\affix\suffix1.dbr""", weight: 0);
		table.Set(Str("suffixRandomizerName2", """records\affix\suffix2.dbr"""));
		var db = this.CreateDatabase();

		db.LoadDBFile();

		Describe(db.GetItemAffixTableMap("""records\item\sword.dbr""")).Should()
			.Equal("""records\loot\table_a.dbr prefix records\affix\prefix1.dbr 10""");
	}

	[Fact]
	public void GetItemAffixTableMap_KindWithoutChance_IsLeftOut()
	{
		this.UseAnniversaryEdition();
		var table = this.AddFixedLootTable("""records\loot\table_a.dbr""", """records\item\sword.dbr""");
		Change(table, "suffixRandomizerChance", 0F);
		AddAffixTable(table, "prefix", 1, """records\affix\prefix1.dbr""");
		AddAffixTable(table, "suffix", 1, """records\affix\suffix1.dbr""");
		var db = this.CreateDatabase();

		db.LoadDBFile();

		Describe(db.GetItemAffixTableMap("""records\item\sword.dbr""")).Should()
			.Equal("""records\loot\table_a.dbr prefix records\affix\prefix1.dbr 10""");
	}

	[Fact]
	public void GetItemAffixTableMap_SameTableInTwoSlots_AddsItsWeights()
	{
		this.UseAnniversaryEdition();
		var table = this.AddFixedLootTable("""records\loot\table_a.dbr""", """records\item\sword.dbr""", """records\item\sword.dbr""");
		AddAffixTable(table, "prefix", 1, """records\affix\prefix1.dbr""", weight: 30);
		AddAffixTable(table, "prefix", 2, """records\affix\prefix2.dbr""", weight: 5);
		AddAffixTable(table, "prefix", 3, """records\affix\prefix1.dbr""", weight: 20);
		var db = this.CreateDatabase();

		db.LoadDBFile();

		// The item named in two slots still gets the table once.
		Describe(db.GetItemAffixTableMap("""records\item\sword.dbr""")).Should().BeEquivalentTo(
			"""records\loot\table_a.dbr prefix records\affix\prefix1.dbr 50""",
			"""records\loot\table_a.dbr prefix records\affix\prefix2.dbr 5""");
	}

	[Fact]
	public void GetItemAffixTableMap_ItemInTwoLootTables_KeepsEachLootTableApart()
	{
		this.UseAnniversaryEdition();
		var tableA = this.AddFixedLootTable("""records\loot\table_a.dbr""", """records\item\sword.dbr""");
		AddAffixTable(tableA, "prefix", 1, """records\affix\prefix1.dbr""", weight: 3);
		var tableB = this.AddFixedLootTable("""records\loot\table_b.dbr""", """records\item\sword.dbr""");
		AddAffixTable(tableB, "prefix", 1, """records\affix\prefix1.dbr""", weight: 7);
		var db = this.CreateDatabase();

		db.LoadDBFile();

		db.GetItemAffixTableMap("""records\item\sword.dbr""").Should().HaveCount(2);
		Describe(db.GetItemAffixTableMap("""records\item\sword.dbr""")).Should().BeEquivalentTo(
			"""records\loot\table_a.dbr prefix records\affix\prefix1.dbr 3""",
			"""records\loot\table_b.dbr prefix records\affix\prefix1.dbr 7""");
	}

	[Fact]
	public void AllLootRandomizer_CustomMap_FlagsTheAffixRecordsOfTheModDatabase()
	{
		// New and overridden affixes alike come from the mod database; the others from the game's.
		this.UseAnniversaryEdition();
		this.UseCustomMap();
		var gameArz = Path.Combine(GameAE, "Database", "database.arz");
		var modArz = Path.Combine(ModFolder, "database", "shadowchampions.arz");
		this.AddArz(modArz);
		this.AddRecord(gameArz, """records\affixase.dbr""", "LootRandomizer");
		this.AddRecord(gameArz, """records\affix\overridden.dbr""", "LootRandomizer");
		this.AddRecord(modArz, """records\affix\overridden.dbr""", "LootRandomizer");
		this.AddRecord(modArz, """records\affix\modonly.dbr""", "LootRandomizer");
		var db = this.CreateDatabase();

		db.LoadDBFile();

		db.AllLootRandomizer.ToDictionary(a => a.Key.Raw, a => a.Value.FromMod).Should().BeEquivalentTo(new Dictionary<string, bool>
		{
			["""records\affixase.dbr"""] = false,
			["""records\affix\overridden.dbr"""] = true,
			["""records\affix\modonly.dbr"""] = true,
		});
	}

	[Theory]
	[InlineData(0, 0, 0, 0, 0, false, true, true)] // all zero: the game rolls a prefix and a suffix
	[InlineData(0, 10, 0, 0, 0, false, false, false)] // only "no affix"
	[InlineData(0, 10, 0, 0, 5, true, false, false)] // broken only
	[InlineData(0, 10, 5, 0, 0, false, true, false)] // prefix only
	[InlineData(0, 10, 0, 5, 0, false, false, true)] // suffix only
	[InlineData(5, 0, 0, 0, 0, false, true, true)] // both
	public void GetItemAffixTableMap_KindWeights_KeepOnlyTheKindsTheGameCanRoll(
		int both, int none, int prefixOnly, int suffixOnly, int brokenOnly, bool hasBroken, bool hasPrefix, bool hasSuffix)
	{
		this.UseAnniversaryEdition();
		var table = this.AddFixedLootTable("""records\loot\table_a.dbr""", """records\item\sword.dbr""");
		table.Set(Int("bothPrefixSuffix", both));
		table.Set(Int("noPrefixNoSuffix", none));
		table.Set(Int("prefixOnly", prefixOnly));
		table.Set(Int("suffixOnly", suffixOnly));
		table.Set(Int("brokenOnly", brokenOnly));
		AddAffixTable(table, "broken", 1, """records\affix\broken1.dbr""");
		AddAffixTable(table, "prefix", 1, """records\affix\prefix1.dbr""");
		AddAffixTable(table, "suffix", 1, """records\affix\suffix1.dbr""");
		var db = this.CreateDatabase();

		db.LoadDBFile();

		var map = db.GetItemAffixTableMap("""records\item\sword.dbr""");
		if (!(hasBroken || hasPrefix || hasSuffix))
		{
			map.Should().BeNull();
			return;
		}

		var tables = map.Should().ContainSingle().Subject;
		tables.Broken.Should().HaveCount(hasBroken ? 1 : 0);
		tables.Prefix.Should().HaveCount(hasPrefix ? 1 : 0);
		tables.Suffix.Should().HaveCount(hasSuffix ? 1 : 0);
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
	public void LoadResource_ResourceMisplacedInTheExpansionFolder_IsFoundThere()
	{
		// The record says Items\..., the archive is Resources\XPack\Items.arc.
		this.UseAnniversaryEdition();
		this._gamePath.Setup(g => g.ResolveArcFileName(It.IsAny<RecordId>()))
			.Returns((Path.Combine(GameAE, "Resources", "Items.arc"), false));
		this.AddArcEntry(Path.Combine(GameAE, "Resources", "XPack", "Items.arc"), """EquipmentRing\ring.tex""", [8]);
		var db = this.CreateDatabase();

		db.LoadResource("""Items\EquipmentRing\ring.tex""").Should().Equal(8);
	}

	[Theory]
	[InlineData(false, null)]
	[InlineData(true, 9)]
	public void LoadResource_FallbackToARagnarokArchive_OnlyWhenRagnarokIsInstalled(bool installed, int? expected)
	{
		this.UseAnniversaryEdition();
		this._gamePath.SetupGet(g => g.IsRagnarokInstalled).Returns(installed);
		this._gamePath.Setup(g => g.ResolveArcFileName(It.IsAny<RecordId>()))
			.Returns((Path.Combine(GameAE, "Resources", "Items.arc"), false));
		this.AddArcEntry(Path.Combine(GameAE, "Resources", "XPack2", "Items.arc"), """EquipmentRing\ring.tex""", [9]);
		var db = this.CreateDatabase();

		var data = db.LoadResource("""Items\EquipmentRing\ring.tex""");

		if (expected is null)
			data.Should().BeNull();
		else
			data.Should().Equal((byte)expected.Value);
	}

	[Fact]
	public void LoadResource_ExpansionResourceInTheBaseGameArchive_IsFoundWithoutItsPrefix()
	{
		// The record says XPack\Items\..., the archive is Resources\Items.arc.
		this.UseAnniversaryEdition();
		this._gamePath.Setup(g => g.ResolveArcFileName(It.IsAny<RecordId>()))
			.Returns((Path.Combine(GameAE, "Resources", "XPack", "Items.arc"), true));
		this.AddArcEntry(Path.Combine(GameAE, "Resources", "Items.arc"), """Relics\icon.tex""", [10]);
		var db = this.CreateDatabase();

		db.LoadResource("""XPack\Items\Relics\icon.tex""").Should().Equal(10);
	}

	[Fact]
	public void LoadResource_ExpansionFolderBeforeTheBaseGame_WhenBothHaveTheResource()
	{
		this.UseAnniversaryEdition();
		this._gamePath.SetupGet(g => g.IsEmbersInstalled).Returns(true);
		this._gamePath.Setup(g => g.ResolveArcFileName(It.IsAny<RecordId>()))
			.Returns((Path.Combine(GameAE, "Resources", "Missing.arc"), false));
		this.AddArcEntry(Path.Combine(GameAE, "Resources", "XPack4", "Items.arc"), """EquipmentRing\ring.tex""", [11]);
		this.AddArcEntry(Path.Combine(GameAE, "Resources", "Items.arc"), """EquipmentRing\ring.tex""", [12]);
		var db = this.CreateDatabase();

		db.LoadResource("""Items\EquipmentRing\ring.tex""").Should().Equal(11);
	}

	[Fact]
	public void LoadResource_DiscImmortalThrone_FallsBackToTheTitanQuestFolderForTheBaseGameArchive()
	{
		// Separate installs: the resolved archive is in the Immortal Throne folder, the base game one in Titan Quest.
		this._gamePath.SetupGet(g => g.GamePathTQ).Returns(GameTQ);
		this._gamePath.SetupGet(g => g.GamePathTQIT).Returns(GameTQIT);
		this._gamePath.SetupGet(g => g.GamePathAreDifferent).Returns(true);
		this._gamePath.SetupGet(g => g.GameType).Returns(GameType.TQIT);
		this._gamePath.Setup(g => g.ResolveArcFileName(It.IsAny<RecordId>()))
			.Returns((Path.Combine(GameTQIT, "Resources", "Items.arc"), false));
		this.AddArcEntry(Path.Combine(GameTQ, "Resources", "Items.arc"), """EquipmentRing\ring.tex""", [13]);
		var db = this.CreateDatabase();

		db.LoadResource("""Items\EquipmentRing\ring.tex""").Should().Equal(13);
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
	public async Task Selector_WhileAModLoads_AMapAlreadyLoadedStaysUsable()
	{
		var cancel = TestContext.Current.CancellationToken;
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
		var preload = Task.Run(() => selector.Preload(ModFolder), cancel);
		reading.Wait(TimeSpan.FromSeconds(10), cancel).Should().BeTrue();
		var baseGameRead = Task.Run(() =>
		{
			using (selector.Use(string.Empty))
				return selector.GetRecordFromFile("""records\item\a.dbr""");
		}, cancel);
		bool readWhileLoading = await Task.WhenAny(baseGameRead, Task.Delay(TimeSpan.FromSeconds(5), cancel)) == baseGameRead;
		release.Set();
		bool preloadEnded = await Task.WhenAny(preload, Task.Delay(TimeSpan.FromSeconds(10), cancel)) == preload;

		// Assert
		readWhileLoading.Should().BeTrue();
		preloadEnded.Should().BeTrue();
		(await baseGameRead).Should().BeSameAs(gameRecord);
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
