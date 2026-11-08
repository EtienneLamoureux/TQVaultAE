using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Tests what <see cref="TQVaultAE.Data.ItemProvider"/> reads around the item text: requirements (from the records and
/// from the item cost equations), stat bonuses, set members and the database data of an item.
/// </summary>
public class ItemProviderRequirementsTests : ItemProviderTextTestBase
{
	private const string CostId = """records\game\itemcost_test.dbr""";

	public ItemProviderRequirementsTests()
	{
		this.Translations["MeetsRequirement"] = "Required {%s0}: {%.0f1}";
		this.Translations["LevelRequirement"] = "Player Level";
		this.Translations["Strength"] = "Strength";
		this.Translations["Dexterity"] = "Dexterity";
		this.Translations["Intelligence"] = "Intelligence";
		this.Translations["tagSetTest"] = "Test Set";
		this.Translations["tagSetRing"] = "Test Ring";
	}

	#region Requirements

	[Fact]
	public void Requirements_KeepTheHighestValueOfEachRecord()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Int("levelRequirement", 20), Int("strengthRequirement", 100));
		this.AddPrefix(item, Int("strengthRequirement", 150), Int("dexterityRequirement", 80), Int("levelRequirement", 10));

		var (requirements, variables) = this.Provider.GetRequirements(item);

		variables.Keys.Should().Equal("Dexterity", "LevelRequirement", "Strength");
		requirements.Should().Equal("Required Dexterity: 80", "Required Player Level: 20", "Required Strength: 150");
	}

	[Fact]
	public void Requirements_IncludeTheSocketedRelics()
	{
		var item = this.CreateItem("WeaponMelee_Sword", Int("levelRequirement", 20));
		this.CreateRecord("""records\item\relics\test_relic.dbr""", "ItemRelic", Int("levelRequirement", 30));
		item.relicID = RecordId.Create("""records\item\relics\test_relic.dbr""");
		item.RelicInfo = this.MockDatabase.Object.GetInfo(item.relicID);
		this.CreateRecord("""records\item\relics\test_relic2.dbr""", "ItemRelic", Int("intelligenceRequirement", 40));
		item.relic2ID = RecordId.Create("""records\item\relics\test_relic2.dbr""");
		item.Relic2Info = this.MockDatabase.Object.GetInfo(item.relic2ID);

		var info = this.Provider.GetRequirementInfo(item);

		info.Lvl.Should().Be(30);
		info.Int.Should().Be(40);
	}

	[Fact]
	public void FormulaRequirements_IncludeTheArtifactLevel()
	{
		this.CreateRecord("""records\test\artifact.dbr""", "ItemArtifact", Int("levelRequirement", 45));
		var formula = this.CreateItem("ItemArtifactFormula", Str("artifactName", """records\test\artifact.dbr"""));

		this.Provider.GetRequirementInfo(formula).Lvl.Should().Be(45);
	}

	[Fact]
	public void Requirements_WithoutTranslations_ShowTheKeysBetweenQuestionMarks()
	{
		this.Translations.Remove("MeetsRequirement");
		this.Translations.Remove("LevelRequirement");
		var item = this.CreateItem("ArmorJewelry_Ring", Int("levelRequirement", 20));

		this.Provider.GetRequirements(item).Requirements.Should().Equal("?Required? ?LevelRequirement?: 20");
	}

	[Fact]
	public void MultiValueRequirement_IsShownRaw()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Ints("levelRequirement", 10, 20));

		this.Provider.GetRequirements(item).Requirements.Should().ContainSingle()
			.Which.Should().StartWith("LevelRequirement: ").And.Contain("20");
	}

	[Fact]
	public void DynamicRequirements_EvaluateTheItemCostEquations_RoundedUp()
	{
		this.CreateRecord(CostId, "",
			Str("swordStrengthEquation", "50+((itemLevel-1)*4.75)"),
			Str("swordDexterityEquation", "50+((itemLevel-1)*3.75)"),
			Str("swordCostEquation", "itemLevel*1000"),
			Str("axeStrengthEquation", "999"));
		var sword = this.CreateItem("WeaponMelee_Sword", Int("itemLevel", 10), Str("itemCostName", CostId));

		var variables = this.Provider.GetRequirementVariables(sword);

		variables.Keys.Should().BeEquivalentTo("Strength", "Dexterity");
		variables["Strength"].GetInt32(0).Should().Be(93); // 92.75
		variables["Dexterity"].GetInt32(0).Should().Be(84); // 83.75
		variables["Strength"].Name.Should().Be("strengthRequirement");
	}

	[Fact]
	public void DynamicRequirements_DoNotOverrideTheRecordRequirements()
	{
		this.CreateRecord(CostId, "", Str("swordStrengthEquation", "50+((itemLevel-1)*4.75)"));
		var sword = this.CreateItem("WeaponMelee_Sword", Int("itemLevel", 10), Str("itemCostName", CostId), Int("strengthRequirement", 200));

		this.Provider.GetRequirementInfo(sword).Str.Should().Be(200);
	}

	[Fact]
	public void DynamicRequirements_UseTheTotalAttributeCount()
	{
		this.CreateRecord(CostId, "", Str("swordStrengthEquation", "totalAttCount*10"));
		var sword = this.CreateItem("WeaponMelee_Sword", Int("itemLevel", 10), Str("itemCostName", CostId));
		sword.attributeCount = 3;

		this.Provider.GetRequirementInfo(sword).Str.Should().Be(30);
	}

	[Fact]
	public void DynamicRequirements_FallBackToTheDefaultItemCostRecord()
	{
		this.CreateRecord("""records\game\itemcost.dbr""", "", Str("swordStrengthEquation", "itemLevel*2"));
		var sword = this.CreateItem("WeaponMelee_Sword", Int("itemLevel", 10));

		this.Provider.GetRequirementInfo(sword).Str.Should().Be(20);
	}

	[Fact]
	public void DynamicRequirements_NeedAnItemLevel()
	{
		this.CreateRecord("""records\game\itemcost.dbr""", "", Str("swordStrengthEquation", "50"));
		var sword = this.CreateItem("WeaponMelee_Sword");

		this.Provider.GetRequirementVariables(sword).Should().BeEmpty();
	}

	[Fact]
	public void DynamicRequirement_ThatCannotBeEvaluated_IsZero()
	{
		this.CreateRecord(CostId, "", Str("swordStrengthEquation", "unknownVariable*2"));
		var sword = this.CreateItem("WeaponMelee_Sword", Int("itemLevel", 10), Str("itemCostName", CostId));

		this.Provider.GetRequirementInfo(sword).Str.Should().Be(0);
	}

	[Fact]
	public void DynamicRequirement_ThatOverflows_IsZero()
	{
		this.CreateRecord(CostId, "", Str("swordStrengthEquation", "itemLevel*10000000000"));
		var sword = this.CreateItem("WeaponMelee_Sword", Int("itemLevel", 10), Str("itemCostName", CostId));

		this.Provider.GetRequirementInfo(sword).Str.Should().Be(0);
	}

	#endregion

	#region Stat bonuses

	[Fact]
	public void StatBonuses_AddTheBonusesOfTheBaseItemAndItsAffixes()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Int("characterStrength", 10), Int("characterLife", 40), Int("offensivePhysicalMin", 5));
		this.AddPrefix(item, Int("characterStrength", 5));
		this.CreateRecord("""records\test\suffix.dbr""", "LootRandomizer", Float("characterDexterity", 7f));
		item.suffixID = RecordId.Create("""records\test\suffix.dbr""");
		item.suffixInfo = this.MockDatabase.Object.GetInfo(item.suffixID);

		var bonuses = this.Provider.GetStatBonuses(item);

		bonuses.Should().BeEquivalentTo(new Dictionary<string, int>
		{
			["CHARACTERSTRENGTH"] = 15,
			["CHARACTERLIFE"] = 40,
			["CHARACTERDEXTERITY"] = 7,
		});
	}

	[Fact]
	public void StatBonusesOfARelic_UseTheValueOfItsCompletionLevel()
	{
		var item = this.CreateItem("WeaponMelee_Sword");
		this.CreateRecord("""records\item\relics\test_relic.dbr""", "ItemRelic",
			Ints("characterLife", 10, 20, 30), Ints("characterMana", 5, 10), Int("completedRelicLevel", 3));
		item.relicID = RecordId.Create("""records\item\relics\test_relic.dbr""");
		item.RelicInfo = this.MockDatabase.Object.GetInfo(item.relicID);

		var bonuses = this.Provider.GetStatBonuses(item);

		bonuses.Should().ContainKey("CHARACTERLIFE").WhoseValue.Should().Be(30);
		bonuses.Should().NotContainKey("CHARACTERMANA", "it has no value for level 3");
	}

	[Fact]
	public void GetStatBonusesFromRecord_IgnoresAMissingRecord()
	{
		var bonuses = new SortedList<string, int>();

		this.Provider.GetStatBonusesFromRecord(bonuses, null);

		bonuses.Should().BeEmpty();
	}

	#endregion

	#region Sets

	[Fact]
	public void SetTranslations_NameTheSetInRare_AndEachKnownMemberIndented()
	{
		this.CreateRecord("""records\test\ring.dbr""", "ArmorJewelry_Ring", Str("itemNameTag", "tagSetRing"));
		this.CreateRecord("""records\test\set.dbr""", "ItemSet",
			Str("setName", "tagSetTest"), Strs("setMembers", """records\test\ring.dbr""", """records\test\missing.dbr"""));
		var item = this.CreateItem("ArmorJewelry_Amulet", Str("itemSetName", """records\test\set.dbr"""));

		var set = this.Provider.GetItemSetTranslations(item);

		set.Should().NotBeNull();
		set!.Translations.Should().BeEquivalentTo(new Dictionary<string, string>
		{
			["tagSetTest"] = $"{Rare}Test Set",
			["""records\test\ring.dbr"""] = $"{Common}    Test Ring",
		});
	}

	[Fact]
	public void SetItems_AreNull_WithoutSetNameRecordOrMembers()
	{
		this.CreateRecord("""records\test\emptyset.dbr""", "ItemSet", Str("setName", "tagSetTest"));
		var noSet = this.CreateItem("ArmorJewelry_Ring");
		var missingSet = this.CreateItem("ArmorJewelry_Amulet", Str("itemSetName", """records\test\missing.dbr"""));
		var emptySet = this.CreateItem("ArmorProtective_Head", Str("itemSetName", """records\test\emptyset.dbr"""));

		this.Provider.GetSetItems(noSet).Should().BeNull();
		this.Provider.GetSetItems(missingSet).Should().BeNull();
		this.Provider.GetSetItems(emptySet).Should().BeNull();
		this.Provider.GetSetItems(new Item()).Should().BeNull();
	}

	#endregion

	#region Database data

	[Fact]
	public void GetDBData_AddsTheMissingDbrExtension_AndLoadsTheInfos()
	{
		this.CreateRecord("""records\test\ring.dbr""", "ArmorJewelry_Ring", Str("bitmap", """items\ring.tex"""));
		this.CreateRecord("""records\test\prefix.dbr""", "LootRandomizer");
		var item = new Item
		{
			BaseItemId = RecordId.Create("""records\test\ring"""),
			prefixID = RecordId.Create("""records\test\prefix"""),
		};

		this.Provider.GetDBData(item);

		item.BaseItemId.Should().Be(RecordId.Create("""records\test\ring.dbr"""));
		item.baseItemInfo.Should().NotBeNull();
		item.prefixInfo.Should().NotBeNull();
		item.TexImageResourceId.Should().Be(RecordId.Create("""items\ring.tex"""));
	}

	[Fact]
	public void GetDBData_ShowsAnIncompleteRelicWithItsShardBitmap()
	{
		this.CreateRecord("""records\item\relics\test_relic.dbr""", "ItemRelic",
			Int("completedRelicLevel", 3), Str("relicBitmap", """items\relic.tex"""), Str("shardBitmap", """items\shard.tex"""));
		var shard = new Item { BaseItemId = RecordId.Create("""records\item\relics\test_relic.dbr"""), Var1 = 1 };
		var complete = new Item { BaseItemId = RecordId.Create("""records\item\relics\test_relic.dbr"""), Var1 = 3 };

		this.Provider.GetDBData(shard);
		this.Provider.GetDBData(complete);

		shard.TexImageResourceId.Should().Be(RecordId.Create("""items\shard.tex"""));
		complete.TexImageResourceId.Should().Be(RecordId.Create("""items\relic.tex"""));
	}

	[Fact]
	public void GetDBData_OfAnUnknownItem_UsesTheDefaultBitmap()
	{
		var item = new Item { BaseItemId = RecordId.Create("""records\test\unknown.dbr""") };

		this.Provider.GetDBData(item);

		item.baseItemInfo.Should().BeNull();
		item.TexImageResourceId.Should().Be(RecordId.Create("DefaultBitmap"));
	}

	#endregion

	private void AddPrefix(Item item, params Variable[] attributes)
	{
		this.CreateRecord("""records\test\prefix.dbr""", "LootRandomizer", attributes);
		item.prefixID = RecordId.Create("""records\test\prefix.dbr""");
		item.prefixInfo = this.MockDatabase.Object.GetInfo(item.prefixID);
	}
}
