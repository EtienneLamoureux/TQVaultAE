using System.Collections.ObjectModel;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Affix tables, relic bonus tables, relic removal and artifact crafting of <see cref="ItemProvider"/>.
/// </summary>
public class ItemProviderAffixesTests
{
	private const string Helmet = """records\item\equipmentarmor\helm\helmet01.dbr""";
	private const string Relic1 = """records\item\relics\03_act1_hermessandal.dbr""";
	private const string Relic1Bonus = """records\item\lootmagicalaffixes\relics\bonusesgreece\03\01_hermessandal_attlife.dbr""";
	private const string Relic2 = """records\xpack3\items\relics\03_x3_atlasendurance.dbr""";
	private const string Relic2Bonus = """records\item\lootmagicalaffixes\prefix\default\character_attributeenergy_06.dbr""";

	private readonly Mock<IDatabase> _database = new();
	private readonly Mock<ILootTableCollectionProvider> _lootTables = new();
	private readonly ItemProvider _provider;

	public ItemProviderAffixesTests()
	{
		this._provider = new ItemProvider(
			NullLogger<ItemProvider>.Instance,
			this._database.Object,
			this._lootTables.Object,
			new Mock<IItemAttributeProvider>().Object,
			new TQDataService(NullLogger<TQDataService>.Instance),
			new PathIO(),
			new UserSettings());
	}

	private static LootTableCollection Table(string id) => new(id, []);

	private static Info InfoOf(string id, string recordType, params (string Name, string Value)[] strings)
	{
		var record = new DBRecordCollection(id, recordType);
		record.Set(Text("Class", recordType));
		foreach (var (name, value) in strings)
			record.Set(Text(name, value));

		return new Info(record);
	}

	private static Variable Text(string name, string value)
	{
		var variable = new Variable(name, VariableDataType.StringVar, 1);
		variable[0] = value;
		return variable;
	}

	/// <summary>A helmet with a relic in each slot, both complete.</summary>
	private static Item SocketedHelmet() => new()
	{
		BaseItemId = Helmet,
		baseItemInfo = InfoOf(Helmet, Item.ICLASS_HEAD),
		relicID = Relic1,
		RelicInfo = InfoOf(Relic1, Item.ICLASS_RELIC, ("bonusTableName", "table-relic1")),
		RelicBonusId = Relic1Bonus,
		RelicBonusInfo = InfoOf(Relic1Bonus, "LootRandomizer"),
		Var1 = 3,
		atlantis = true,
		relic2ID = Relic2,
		Relic2Info = InfoOf(Relic2, Item.ICLASS_RELIC, ("bonusTableName", "table-relic2")),
		RelicBonus2Id = Relic2Bonus,
		RelicBonus2Info = InfoOf(Relic2Bonus, "LootRandomizer"),
		Var2 = 2,
		Seed = 1234,
	};

	#region GetAllAvailableAffixes

	[Fact]
	public void GetAllAvailableAffixes_GearThatTakesNoAffix_ReturnsNull()
	{
		this._provider.GetAllAvailableAffixes(GearType.Undefined).Should().BeNull();
		this._lootTables.VerifyNoOtherCalls();
	}

	[Fact]
	public void GetAllAvailableAffixes_Helmet_KeepsItsTablesMonsterTablesAndUniqueTablesGroupedByKindAndDlc()
	{
		var headPrefix = Table("headprefix");
		var headSuffixIT = Table("headsuffix-it");
		var monsterSuffixEE = Table("monstersuffix-ee");
		var uniquePrefix = Table("uniqueprefix");
		var brokenArmor = Table("brokenarmor");
		var swordPrefix = Table("swordprefix");
		var tables = new Dictionary<RecordId, LootTableCollection>
		{
			[@"records\item\lootmagicalaffixes\prefix\tablesarmor\headmelee_l01.dbr"] = headPrefix,
			[@"records\xpack\item\lootmagicalaffixes\suffix\tablesarmor\headmage_l01.dbr"] = headSuffixIT,
			[@"records\xpack4\item\lootmagicalaffixes\suffix\tablesarmor\chinamonstersuffix_l05.dbr"] = monsterSuffixEE,
			[@"records\item\lootmagicalaffixes\prefix\tablesunique\anything_l01.dbr"] = uniquePrefix,
			[@"records\item\lootmagicalaffixes\broken\table_armor_l01.dbr"] = brokenArmor,
			[@"records\item\lootmagicalaffixes\prefix\tablesweapons\sword_l01.dbr"] = swordPrefix,
		};
		this._lootTables.SetupGet(l => l.AllLootRandomizerTable).Returns(new ReadOnlyDictionary<RecordId, LootTableCollection>(tables));

		var affixes = this._provider.GetAllAvailableAffixes(GearType.Head)!;

		affixes.Prefix.Keys.Should().Equal(GameDlc.TitanQuest);
		affixes.Prefix[GameDlc.TitanQuest].Should().BeEquivalentTo([headPrefix, uniquePrefix]);
		affixes.Suffix.Keys.Should().Equal(GameDlc.ImmortalThrone, GameDlc.EternalEmbers);
		affixes.Suffix[GameDlc.ImmortalThrone].Should().Equal(headSuffixIT);
		affixes.Suffix[GameDlc.EternalEmbers].Should().Equal(monsterSuffixEE);
		affixes.Broken[GameDlc.TitanQuest].Should().Equal(brokenArmor);
	}

	#endregion

	#region GetItemAffixes

	[Fact]
	public void GetItemAffixes_ItemWithoutAffixTables_ReturnsNull()
	{
		this._database.Setup(d => d.GetItemAffixTableMap(It.IsAny<RecordId>())).Returns((ReadOnlyCollection<AffixTableMapItem>?)null);
		this._database.Setup(d => d.GetItemAffixTableMap((RecordId)Relic1))
			.Returns(new List<AffixTableMapItem> { new("loot", [], [], []) }.AsReadOnly());

		this._provider.GetItemAffixes(Helmet).Should().BeNull();
		this._provider.GetItemAffixes(Relic1).Should().BeNull();
	}

	private static LootTableCollection Table(string id, params (string Affix, float Weight)[] entries)
		=> new(id, entries.ToDictionary(e => (RecordId)e.Affix, e => (e.Weight, LootRandomizerItem.Default(e.Affix))));

	[Fact]
	public void GetItemAffixes_LootTableWithWeightedTables_MixesThemAsTheGamePicks()
	{
		// Table share x share in the table, summed over the tables; a missing table keeps its share (no affix).
		RecordId loot = @"records\xpack2\loot\table_a.dbr";
		RecordId prefixA = @"records\item\lootmagicalaffixes\prefix\tablesarmor\a.dbr";
		RecordId prefixB = @"records\item\lootmagicalaffixes\prefix\tablesarmor\b.dbr";
		RecordId missing = @"records\item\lootmagicalaffixes\prefix\tablesarmor\missing.dbr";
		this._database.Setup(d => d.GetItemAffixTableMap((RecordId)Helmet)).Returns(new List<AffixTableMapItem>
		{
			new(loot, [], [new(prefixA, 60), new(prefixB, 20), new(missing, 20)], []),
		}.AsReadOnly());
		this._lootTables.Setup(l => l.LoadTable(prefixA)).Returns(Table("a", ("strong", 1), ("quick", 3)));
		this._lootTables.Setup(l => l.LoadTable(prefixB)).Returns(Table("b", ("strong", 1)));

		var affixes = this._provider.GetItemAffixes(Helmet)!;

		affixes.Broken.Should().BeEmpty();
		affixes.Suffix.Should().BeEmpty();
		var mix = affixes.Prefix.Should().ContainKey(GameDlc.Ragnarok).WhoseValue.Should().ContainSingle().Subject;
		mix.TableId.Should().Be(loot);
		mix.Data[(RecordId)"strong"].WeightPercent.Should().BeApproximately(0.6F * 0.25F + 0.2F, 1e-6F);
		mix.Data[(RecordId)"quick"].WeightPercent.Should().BeApproximately(0.6F * 0.75F, 1e-6F);
	}

	[Fact]
	public void GetItemAffixes_UnreachableTables_GivenOnlyWhenAskedAndAsTheyAre()
	{
		RecordId loot = @"records\loot\table_a.dbr";
		RecordId prefix = @"records\item\lootmagicalaffixes\prefix\tablesarmor\a.dbr";
		RecordId neverSuffix = @"records\item\lootmagicalaffixes\suffix\tablesarmor\a.dbr";
		RecordId missing = @"records\item\lootmagicalaffixes\suffix\tablesarmor\missing.dbr";
		this._database.Setup(d => d.GetItemAffixTableMap((RecordId)Helmet)).Returns(new List<AffixTableMapItem>
		{
			new(loot, [], [new(prefix, 1)], []),
			new(loot, [], [], [new(neverSuffix, 0), new(missing, 0)], Unreachable: true),
		}.AsReadOnly());
		var suffixTable = Table("s", ("ofLife", 1));
		this._lootTables.Setup(l => l.LoadTable(prefix)).Returns(Table("p", ("strong", 1)));
		this._lootTables.Setup(l => l.LoadTable(neverSuffix)).Returns(suffixTable);

		var rolledOnly = this._provider.GetItemAffixes(Helmet)!;
		var withUnreachable = this._provider.GetItemAffixes(Helmet, includeUnreachable: true)!;

		rolledOnly.Unreachable.Should().BeEmpty();
		rolledOnly.Suffix.Should().BeEmpty();
		withUnreachable.Prefix.Should().ContainSingle();
		withUnreachable.Suffix.Should().BeEmpty(); // never rolled: no distribution
		withUnreachable.Unreachable.Should().Equal((2, suffixTable)); // the missing table is left out
	}

	[Fact]
	public void GetItemAffixes_OnlyUnreachableTables_NullUnlessAsked()
	{
		RecordId loot = @"records\loot\table_a.dbr";
		RecordId neverPrefix = @"records\item\lootmagicalaffixes\prefix\tablesarmor\a.dbr";
		this._database.Setup(d => d.GetItemAffixTableMap((RecordId)Helmet)).Returns(new List<AffixTableMapItem>
		{
			new(loot, [], [new(neverPrefix, 0)], [], Unreachable: true),
		}.AsReadOnly());
		this._lootTables.Setup(l => l.LoadTable(neverPrefix)).Returns(Table("p", ("strong", 1)));

		this._provider.GetItemAffixes(Helmet).Should().BeNull();
		this._provider.GetItemAffixes(Helmet, includeUnreachable: true)!.Unreachable.Should().ContainSingle();
	}

	[Fact]
	public void GetItemAffixes_TwoLootTables_GivesOneDistributionEachAndLoadsOnce()
	{
		RecordId lootA = @"records\loot\table_a.dbr";
		RecordId lootB = @"records\loot\table_b.dbr";
		RecordId prefix = @"records\item\lootmagicalaffixes\prefix\tablesarmor\a.dbr";
		RecordId suffix = @"records\item\lootmagicalaffixes\suffix\tablesarmor\a.dbr";
		RecordId missingSuffix = @"records\item\lootmagicalaffixes\suffix\tablesarmor\missing.dbr";
		this._database.Setup(d => d.GetItemAffixTableMap((RecordId)Helmet)).Returns(new List<AffixTableMapItem>
		{
			new(lootA, [], [new(prefix, 1)], [new(missingSuffix, 1)]),
			new(lootB, [], [new(prefix, 1)], [new(suffix, 1)]),
		}.AsReadOnly());
		this._lootTables.Setup(l => l.LoadTable(prefix)).Returns(Table("p", ("strong", 1)));
		this._lootTables.Setup(l => l.LoadTable(suffix)).Returns(Table("s", ("ofLife", 1)));

		var affixes = this._provider.GetItemAffixes(Helmet)!;
		var again = this._provider.GetItemAffixes(Helmet);

		again.Should().BeSameAs(affixes);
		this._database.Verify(d => d.GetItemAffixTableMap(It.IsAny<RecordId>()), Times.Once);
		affixes.Prefix[GameDlc.TitanQuest].Select(t => t.TableId).Should().Equal(lootA, lootB);
		// Loot table A only names a missing suffix table: no distribution.
		affixes.Suffix[GameDlc.TitanQuest].Select(t => t.TableId).Should().Equal(lootB);
	}

	#endregion

	#region Bonus tables

	[Fact]
	public void BonusTableSocketedRelic_TwoCompleteRelics_LoadsTheBonusTableOfEach()
	{
		var table1 = Table("table-relic1");
		var table2 = Table("table-relic2");
		this._lootTables.Setup(l => l.LoadTable((RecordId)"table-relic1")).Returns(table1);
		this._lootTables.Setup(l => l.LoadTable((RecordId)"table-relic2")).Returns(table2);

		var found = this._provider.BonusTableSocketedRelic(SocketedHelmet(), out var relicTable1, out var relicTable2);

		found.Should().BeTrue();
		relicTable1.Should().BeSameAs(table1);
		relicTable2.Should().BeSameAs(table2);
	}

	[Fact]
	public void BonusTableSocketedRelic_RelicsWithoutCompletionBonus_ReturnsFalse()
	{
		var item = SocketedHelmet();
		item.RelicBonusInfo = null;
		item.RelicBonus2Info = null;

		var found = this._provider.BonusTableSocketedRelic(item, out var relicTable1, out var relicTable2);

		found.Should().BeFalse();
		relicTable1.Should().BeNull();
		relicTable2.Should().BeNull();
		this._lootTables.VerifyNoOtherCalls();
	}

	[Fact]
	public void BonusTableRelicOrArtifact_Relic_LoadsItsBonusTable()
	{
		var table = Table("relic-bonus");
		this._lootTables.Setup(l => l.LoadTable((RecordId)"relic-bonus")).Returns(table);
		var relic = new Item { BaseItemId = Relic1, baseItemInfo = InfoOf(Relic1, Item.ICLASS_RELIC, ("bonusTableName", "relic-bonus")) };

		this._provider.BonusTableRelicOrArtifact(relic).Should().BeSameAs(table);
	}

	[Theory]
	[InlineData("""records\xpack\item\artifacts\e_la_darkcore.dbr""", """records\xpack\item\artifacts\arcaneformulae\e_la_darkcore_formula.dbr""")]
	// The formula of Kingslayer (Sands of Kronos) is not named after the artifact (record ids ignore case).
	[InlineData("""records\xpack\item\artifacts\E_GA_SANDOFKRONOS.dbr""", """records\xpack\item\artifacts\arcaneformulae\E_GA_SANDSOFKRONOS_formula.dbr""")]
	public void BonusTableRelicOrArtifact_Artifact_LoadsTheBonusTableOfItsFormula(string artifactId, string formulaId)
	{
		var formula = new DBRecordCollection(formulaId, Item.ICLASS_FORMULA);
		formula.Set(Text("artifactBonusTableName", "artifact-bonus"));
		this._database.Setup(d => d.GetRecordFromFile(It.Is<RecordId>(r => string.Equals(r.Raw, formulaId, StringComparison.OrdinalIgnoreCase)))).Returns(formula);
		var table = Table("artifact-bonus");
		this._lootTables.Setup(l => l.LoadTable((RecordId)"artifact-bonus")).Returns(table);
		var artifact = new Item { BaseItemId = artifactId, baseItemInfo = InfoOf(artifactId, Item.ICLASS_ARTIFACT) };

		this._provider.BonusTableRelicOrArtifact(artifact).Should().BeSameAs(table);
	}

	[Fact]
	public void BonusTableRelicOrArtifact_ArtifactWithoutFormulaOrOtherItem_ReturnsNull()
	{
		var artifact = new Item { BaseItemId = """records\xpack\item\artifacts\e_la_darkcore.dbr""", baseItemInfo = InfoOf("a", Item.ICLASS_ARTIFACT) };

		this._provider.BonusTableRelicOrArtifact(artifact).Should().BeNull();
		this._provider.BonusTableRelicOrArtifact(SocketedHelmet()).Should().BeNull();
		this._provider.BonusTableRelicOrArtifact(new Item { BaseItemId = Helmet }).Should().BeNull("an item unknown to the database has no table");
		this._lootTables.Verify(l => l.LoadTable(It.IsAny<RecordId>()), Times.Never);
	}

	#endregion

	#region Relics and artifacts

	[Fact]
	public void RemoveRelic1_SocketedItem_ReturnsTheRelicWithItsBonusAndEmptiesTheFirstSlotOnly()
	{
		var item = SocketedHelmet();

		var relic = this._provider.RemoveRelic1(item)!;

		RecordId.RawOf(relic.BaseItemId).Should().Be(Relic1);
		RecordId.RawOf(relic.RelicBonusId).Should().Be(Relic1Bonus);
		relic.Var1.Should().Be(3);
		RecordId.RawOf(item.relicID).Should().BeEmpty();
		RecordId.RawOf(item.RelicBonusId).Should().BeEmpty();
		item.Var1.Should().Be(0);
		item.RelicInfo.Should().BeNull();
		item.RelicBonusInfo.Should().BeNull();
		item.IsModified.Should().BeTrue();
		RecordId.RawOf(item.relic2ID).Should().Be(Relic2, "the second slot is not touched");
		item.Var2.Should().Be(2);
	}

	[Fact]
	public void RemoveRelic2_SocketedItem_ReturnsTheRelicWithItsBonusAndEmptiesTheSecondSlotOnly()
	{
		var item = SocketedHelmet();

		var relic = this._provider.RemoveRelic2(item)!;

		RecordId.RawOf(relic.BaseItemId).Should().Be(Relic2);
		RecordId.RawOf(relic.RelicBonusId).Should().Be(Relic2Bonus);
		relic.Var1.Should().Be(2, "a relic keeps its level in Var1");
		RecordId.RawOf(item.relic2ID).Should().BeEmpty();
		RecordId.RawOf(item.RelicBonus2Id).Should().BeEmpty();
		item.Var2.Should().Be(Item.var2Default);
		item.Relic2Info.Should().BeNull();
		item.RelicBonus2Info.Should().BeNull();
		item.IsModified.Should().BeTrue();
		RecordId.RawOf(item.relicID).Should().Be(Relic1, "the first slot is not touched");
	}

	[Fact]
	public void RemoveRelic_EmptySlots_ReturnNullAndChangeNothing()
	{
		var item = new Item { BaseItemId = Helmet, baseItemInfo = InfoOf(Helmet, Item.ICLASS_HEAD), relicID = RecordId.Empty, relic2ID = RecordId.Empty };

		this._provider.RemoveRelic1(item).Should().BeNull();
		this._provider.RemoveRelic2(item).Should().BeNull();
		item.IsModified.Should().BeFalse();
	}

	[Fact]
	public void CraftArtifact_Formula_ReturnsTheArtifactItMakes()
	{
		const string artifactId = """records\xpack\item\artifacts\e_la_darkcore.dbr""";
		var formula = new Item
		{
			BaseItemId = """records\xpack\item\artifacts\arcaneformulae\e_la_darkcore_formula.dbr""",
			baseItemInfo = InfoOf("formula", Item.ICLASS_FORMULA, ("artifactName", artifactId)),
		};

		var artifact = this._provider.CraftArtifact(formula)!;

		RecordId.RawOf(artifact.BaseItemId).Should().Be(artifactId);
		formula.IsModified.Should().BeFalse("crafting leaves the formula as it is (upstream #267)");
		this._database.Verify(d => d.GetInfo(It.Is<RecordId>(r => r.Raw == artifactId)));
	}

	[Fact]
	public void CraftArtifact_NotAFormula_ReturnsNull()
	{
		this._provider.CraftArtifact(SocketedHelmet()).Should().BeNull();
	}

	#endregion
}
