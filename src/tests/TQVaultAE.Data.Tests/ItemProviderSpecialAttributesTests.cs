using AwesomeAssertions;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Tests the attribute lines with their own layout: global chances, racial bonuses, formulae reagents and cost,
/// raw attributes, armor, shields, weapon speed and damage qualifiers.
/// </summary>
public class ItemProviderSpecialAttributesTests : ItemProviderTextTestBase
{
	public ItemProviderSpecialAttributesTests()
	{
		this.Translations["GlobalPercentChanceOfAllTag"] = "{%.0f0}% Chance of:";
		this.Translations["GlobalPercentChanceOfOneTag"] = "{%.0f0}% Chance for one of the following:";
		this.Translations["RacialBonusPercentDamage"] = "{%+.0f0}% Damage to {%s1}";
		this.Translations["racialBonusRaceUndead"] = "Undead";
		this.Translations["racialBonusRaceDemon"] = "Demons";
		this.Translations["xtagArtifactCost"] = "Gold Cost :  {%t0}";
		this.Translations["tagWolfFang"] = "Wolf Fang";
		this.Translations["ShieldBlockRecoveryTime"] = "{%.2f0} second Block Recovery";
		this.Translations["CharacterAttackSpeedFast"] = "Speed:  Fast";
		this.Translations["tagDamageAbsorptionTitle"] = "Protects Against :";
		this.Translations["formatQualifyingDamage"] = "{     %s0}";
		this.Translations["tagQualifyingDamageFire"] = "Fire";
	}

	[Fact]
	public void GlobalChance_IntroducesTheIndentedGlobalEffects()
	{
		var item = this.CreateItem("ArmorJewelry_Ring",
			Float("offensiveGlobalChance", 25f), Float("offensiveFireMin", 10f), Int("offensiveFireGlobal", 1));

		this.BaseAttributes(item).Should().Equal($"{Epic}25% Chance of:", $"{Epic}    10 Fire Damage");
	}

	[Fact]
	public void GlobalChanceOfXorEffects_IsAChanceForOneOfThem()
	{
		var item = this.CreateItem("ArmorJewelry_Ring",
			Float("offensiveGlobalChance", 25f), Float("offensiveFireMin", 10f), Int("offensiveFireGlobal", 1), Int("offensiveFireXOR", 1));

		this.BaseAttributes(item).Should().Equal($"{Epic}25% Chance for one of the following:", $"{Epic}    10 Fire Damage");
	}

	[Fact]
	public void GlobalChance_WithoutGlobalEffects_IsNotShown()
	{
		var alone = this.CreateItem("ArmorJewelry_Ring", Float("offensiveGlobalChance", 25f));
		var withLocal = this.CreateItem("ArmorJewelry_Amulet", Float("offensiveGlobalChance", 25f), Float("offensiveFireMin", 10f));

		this.BaseAttributes(alone).Should().BeEmpty();
		this.BaseAttributes(withLocal).Should().Equal($"{Epic}10 Fire Damage");
	}

	[Fact]
	public void GlobalChance_WithoutTranslation_KeepsTheChanceValue()
	{
		this.Translations.Remove("GlobalPercentChanceOfAllTag");
		var item = this.CreateItem("ArmorJewelry_Ring",
			Float("offensiveGlobalChance", 25f), Float("offensiveFireMin", 10f), Int("offensiveFireGlobal", 1));

		this.BaseAttributes(item)[0].Should().Be($"{Legendary}25.0% ?GlobalPercentChanceOfAllTag?");
	}

	[Fact]
	public void RacialBonus_ShowsOneLinePerRace()
	{
		var item = this.CreateItem("ArmorJewelry_Ring",
			Float("racialBonusPercentDamage", 30f), Strs("racialBonusRace", "Undead", "Demon"));

		this.BaseAttributes(item).Should().Equal($"{Epic}+30% Damage to Undead", $"{Epic}+30% Damage to Demons");
	}

	[Fact]
	public void RacialBonus_UsesTheRaceName_WhenItHasNoTranslation()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Float("racialBonusPercentDamage", 30f), Str("racialBonusRace", "Insectoid"));

		this.BaseAttributes(item).Should().Equal($"{Epic}+30% Damage to Insectoid");
	}

	[Fact]
	public void Formulae_ListsItsReagentsIndented_ThenItsGoldCost()
	{
		this.CreateRecord("""records\test\reagent.dbr""", "ItemRelic", Str("description", "tagWolfFang"));
		var formula = this.CreateItem("ItemArtifactFormula",
			Str("reagent1BaseName", """records\test\reagent.dbr"""), Int("artifactCreationCost", 500));

		var lines = this.BaseAttributes(formula);

		lines.Should().Contain($"{Common}    Wolf Fang");
		lines.Should().ContainInConsecutiveOrder(string.Empty, $"{Rare}Gold Cost :  500");
	}

	[Fact]
	public void Formulae_WithAMissingReagentRecord_ShowsTheRawAttribute()
	{
		var formula = this.CreateItem("ItemArtifactFormula", Str("reagent1BaseName", """records\test\missing.dbr"""));

		var lines = this.BaseAttributes(formula);

		lines.Should().ContainSingle().Which.Should().Contain("reagent1BaseName");
	}

	[Fact]
	public void UnknownAttribute_IsShownRaw_InTheLegendaryColor()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Int("someUnknownCount", 3));

		this.BaseAttributes(item).Should().Equal($"{Legendary}?someUnknownCount?: 3");
	}

	[Fact]
	public void UnknownAttribute_WithAFormatTranslation_IsFormattedInTheEpicColor()
	{
		this.Translations["someUnknownCount"] = "{%d0} Extra Things";
		var item = this.CreateItem("ArmorJewelry_Ring", Int("someUnknownCount", 3));

		this.BaseAttributes(item).Should().Equal($"{Epic}3 Extra Things");
	}

	[Fact]
	public void ArmorProtection_OnTheArmorItself_IsMundane_AndFollowedByAnEmptyRow()
	{
		var armor = this.CreateItem("ArmorProtective_UpperBody", Float("defensiveProtection", 100f));

		var lines = this.BaseAttributes(armor);

		lines.Should().ContainSingle().Which.Should().StartWith($"{Mundane}100 Armor");
	}

	[Fact]
	public void ArmorProtection_OnJewelry_IsAMagicalBonus()
	{
		var ring = this.CreateItem("ArmorJewelry_Ring", Float("defensiveProtection", 100f));

		this.BaseAttributes(ring).Should().Equal($"{Epic}+100 Armor");
	}

	[Fact]
	public void ShieldBlock_IsMundane_WithItsChance()
	{
		var shield = this.CreateItem("WeaponArmor_Shield",
			Float("defensiveBlock", 100f), Float("defensiveBlockChance", 25f), Float("blockRecoveryTime", 0.5f));

		var lines = this.BaseAttributes(shield);

		lines.Should().Contain($"{Mundane}25.0% Chance of 100 Damage Blocked");
		lines.Should().Contain($"{Mundane}0.50 second Block Recovery");
	}

	[Fact]
	public void WeaponAttackSpeed_IsShownOnTheWeaponOnly()
	{
		var sword = this.CreateItem("WeaponMelee_Sword", Str("characterBaseAttackSpeedTag", "CharacterAttackSpeedFast"));
		var ring = this.CreateItem("ArmorJewelry_Ring", Str("characterBaseAttackSpeedTag", "CharacterAttackSpeedFast"));

		this.BaseAttributes(sword).Should().Equal($"{Mundane}Speed:  Fast");
		this.BaseAttributes(ring).Should().BeEmpty();
	}

	[Fact]
	public void DamageQualifier_UsesTheGameFormat_WithItsIndent()
	{
		var armor = this.CreateItem("ArmorProtective_UpperBody", Int("fireDamageQualifier", 1));

		this.BaseAttributes(armor).Should().Equal("Protects Against :", $"{Mundane}     Fire");
	}

	[Fact]
	public void DamageQualifier_WithoutTranslations_FallsBackToEnglish()
	{
		this.Translations.Remove("tagDamageAbsorptionTitle");
		this.Translations.Remove("formatQualifyingDamage");
		var armor = this.CreateItem("ArmorProtective_UpperBody", Int("fireDamageQualifier", 1));

		this.BaseAttributes(armor).Should().Equal("Protects Against :", $"{Mundane}Fire");
	}

	[Fact]
	public void UnfilteredRecord_ShowsStringValuesToo()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Str("itemNameTag", "tagSomething"), Int("characterStrength", 5));

		var filtered = this.BaseAttributes(item, filterExtra: true);
		var unfiltered = this.BaseAttributes(item, filterExtra: false);

		filtered.Should().Equal($"{Epic}+5 Strength");
		unfiltered.Should().Contain(line => line.Contains("itemNameTag", StringComparison.Ordinal));
	}
}
