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
	public void TotalAttributeCount_CountsEachGroupOnce_WithoutBaseValuesChancesOrDurations()
	{
		var sword = this.SwordWithAttributeCountCost(
			Float("offensivePhysicalMin", 12f), Float("offensivePhysicalMax", 20f), Float("defensiveProtection", 50f),
			Float("offensiveGlobalChance", 30f), Str("characterBaseAttackSpeedTag", "CharacterAttackSpeedFast"),
			Float("characterBaseAttackSpeed", 0.1f), Float("offensiveBaseFireMin", 75.9f), Float("offensiveBaseFireMax", 80f),
			Float("offensiveStunMin", 1f), Float("offensiveStunMax", 2f), Float("offensiveStunChance", 10f),
			Float("offensiveSlowBleedingMin", 5f), Float("offensiveSlowBleedingDurationMin", 3f),
			Int("characterStrength", 5), Int("strengthRequirement", 0), Int("characterDexterity", 0));

		this.Provider.GetRequirementInfo(sword).Str.Should().Be(3 * 10, "stun, bleeding and strength");
	}

	[Fact]
	public void TotalAttributeCount_CountsThePierceRatio()
	{
		var sword = this.SwordWithAttributeCountCost(Float("offensivePierceRatioMin", 30f), Int("characterStrength", 5));

		this.Provider.GetRequirementInfo(sword).Str.Should().Be(2 * 10, "pierce ratio and strength");
	}

	[Fact]
	public void TotalAttributeCount_SumsTheBasePrefixAndSuffix_WithoutTheRelics()
	{
		var sword = this.SwordWithAttributeCountCost(Float("offensiveColdMin", 3f));
		this.AddPrefix(sword, Float("offensiveFireMin", 5f), Float("offensiveFireModifier", 15f));
		this.CreateRecord("""records\test\suffix.dbr""", "LootRandomizer", Float("characterLife", 40f), Float("defensivePoison", 12f));
		sword.suffixID = RecordId.Create("""records\test\suffix.dbr""");
		sword.suffixInfo = this.MockDatabase.Object.GetInfo(sword.suffixID);
		this.CreateRecord("""records\item\relics\test_relic.dbr""", "ItemRelic", Int("characterStrength", 8));
		sword.relicID = RecordId.Create("""records\item\relics\test_relic.dbr""");
		sword.RelicInfo = this.MockDatabase.Object.GetInfo(sword.relicID);
		this.CreateRecord("""records\test\bonus.dbr""", "LootRandomizer", Float("characterDexterity", 12f));
		sword.RelicBonusId = RecordId.Create("""records\test\bonus.dbr""");
		sword.RelicBonusInfo = this.MockDatabase.Object.GetInfo(sword.RelicBonusId);

		this.Provider.GetRequirementInfo(sword).Str.Should().Be((1 + 2 + 2) * 10);
	}

	[Fact]
	public void TotalAttributeCount_CountsEachSkillBonusOnce_WhateverItsLevel()
	{
		var sword = this.SwordWithAttributeCountCost(
			Int("augmentSkillLevel1", 3), Str("augmentSkillName1", """records\test\skill.dbr"""),
			Int("augmentMasteryLevel1", 2), Str("augmentMasteryName1", """records\test\mastery.dbr"""),
			Int("augmentAllLevel", 1), Int("augmentSkillLevel2", 0));

		this.Provider.GetRequirementInfo(sword).Str.Should().Be(3 * 10);
	}

	/// <summary>A sword whose strength requirement is ten times its attribute count.</summary>
	private Item SwordWithAttributeCountCost(params Variable[] attributes)
	{
		this.CreateRecord(CostId, "", Str("swordStrengthEquation", "totalAttCount*10"));
		return this.CreateItem("WeaponMelee_Sword", [Int("itemLevel", 10), Str("itemCostName", CostId), .. attributes]);
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

	#region Sets

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

	[Fact]
	public void GetDBData_TakesTheSizeInCellsFromThePictureHeaders()
	{
		this.CreateRecord("""records\test\sword.dbr""", "WeaponMelee_Sword", Str("bitmap", """items\sword.tex"""));
		this.MockDatabase.Setup(d => d.GetTextureSize(RecordId.Create("""items\sword.tex"""))).Returns((64, 128));
		var item = new Item { BaseItemId = RecordId.Create("""records\test\sword.dbr""") };

		this.Provider.GetDBData(item);

		item.Width.Should().Be(2);
		item.Height.Should().Be(4);
	}

	[Fact]
	public void GetDBData_WithAnUnreadablePicture_IsOneCell()
	{
		var item = new Item { BaseItemId = RecordId.Create("""records\test\unknown.dbr""") };

		this.Provider.GetDBData(item);

		item.Width.Should().Be(1);
		item.Height.Should().Be(1);
	}

	[Theory]
	[InlineData(32, 32, 1, 1)]
	[InlineData(64, 96, 2, 3)]
	[InlineData(16, 16, 1, 1)]
	public void CellSize_DividesThePixelsByTheCellSize(int width, int height, int cellsWide, int cellsHigh)
		=> TQVaultAE.Data.ItemProvider.CellSize((width, height)).Should().Be((cellsWide, cellsHigh));

	#endregion
}
