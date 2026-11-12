using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Tests the skill lines of the item text: + to skills, granted skills with their activation, description and effects,
/// skill parameters (cost, duration, radius) and the level a triggered skill is shown at.
/// </summary>
public class ItemProviderSkillAttributesTests : ItemProviderTextTestBase
{
	private const string SkillId = """records\test\skill.dbr""";
	private const string BuffId = """records\test\buff.dbr""";
	private const string ControllerId = """records\test\controller.dbr""";

	public ItemProviderSkillAttributesTests()
	{
		this.Translations["ItemAllSkillIncrement"] = "{+%d0} to all Skills";
		this.Translations["ItemMasteryIncrement"] = "{+%d0} to all skills in {%s1}";
		this.Translations["ItemSkillIncrement"] = "{+%d0} to {%s1}";
		this.Translations["tagItemGrantSkill"] = "Grants Skill :";
		this.Translations["xtagAutoSkillCondition01"] = "(Activated on low health)";
		this.Translations["xtagAutoSkillCondition02"] = "(Activated on low energy)";
		this.Translations["xtagAutoSkillCondition03"] = "(Activated upon taking damage)";
		this.Translations["xtagAutoSkillCondition04"] = "(Activated upon taking melee damage)";
		this.Translations["xtagAutoSkillCondition05"] = "(Activated upon taking ranged damage)";
		this.Translations["xtagAutoSkillCondition06"] = "(Activated upon casting a buff)";
		this.Translations["xtagAutoSkillCondition07"] = "(Activated on attack)";
		this.Translations["xtagAutoSkillCondition08"] = "(Activated when equipped)";
		this.Translations["SkillIntFormat"] = "{%d0 %s1}";
		this.Translations["SkillCostFormat"] = "{%.0f0 %s1}";
		this.Translations["ActiveManaCost"] = "Active Energy Cost per Second";
		this.Translations["CooldownTime"] = "^a{%.1f0} Second(s) Recharge";
		this.Translations["SkillSecondFormat"] = "{%.1f0 Second %s1}";
		this.Translations["SkillDistanceFormat"] = "{%.1f0 Meter %s1}";
		this.Translations["ManaCost"] = "Energy Cost";
		this.Translations["ActiveDuration"] = "Duration";
		this.Translations["TargetRadius"] = "Radius";
		this.Translations["TargetNumber"] = "{%d0} Target Maximum";
		this.Translations["tagWarfare"] = "Warfare";
		this.Translations["tagOnslaught"] = "Onslaught";
		this.Translations["tagSkillTest"] = "Test Skill";
		this.Translations["tagSkillTestDescription"] = "Heals you.";
	}

	#region + to skills

	[Fact]
	public void AugmentAllLevel_IsAPlusToAllSkills()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Int("augmentAllLevel", 2));

		this.BaseAttributes(item).Should().Equal($"{Epic}+2 to all Skills");
	}

	[Fact]
	public void AugmentAllLevel_WithoutTranslation_FallsBackToEnglish()
	{
		this.Translations.Remove("ItemAllSkillIncrement");
		var item = this.CreateItem("ArmorJewelry_Ring", Int("augmentAllLevel", 2));

		this.BaseAttributes(item).Should().Equal($"{Legendary}?+2 to all skills?");
	}

	[Fact]
	public void AugmentMasteryLevel_NamesTheMasteryFromItsRecord()
	{
		this.CreateRecord("""records\test\warfaremastery.dbr""", "Skill_Mastery", Str("skillDisplayName", "tagWarfare"));
		var item = this.CreateItem("ArmorJewelry_Ring",
			Int("augmentMasteryLevel1", 1), Str("augmentMasteryName1", """records\test\warfaremastery.dbr"""));

		this.BaseAttributes(item).Should().Equal($"{Epic}+1 to all skills in Warfare");
	}

	[Fact]
	public void AugmentMasteryLevel_WithAMissingMasteryRecord_UsesItsFileName()
	{
		var item = this.CreateItem("ArmorJewelry_Ring",
			Int("augmentMasteryLevel2", 1), Str("augmentMasteryName2", """records\test\warfaremastery.dbr"""));

		this.BaseAttributes(item).Should().Equal($"{Legendary}+1 to all skills in warfaremastery");
	}

	[Fact]
	public void AugmentMasteryLevel_WithoutMasteryName_IsNotShown()
	{
		// The game drops such a bonus, like on the Clashing Suit.
		var item = this.CreateItem("ArmorJewelry_Ring", Int("augmentMasteryLevel1", 1), Int("characterStrength", 5));

		this.BaseAttributes(item).Should().Equal($"{Epic}+5 Strength");
	}

	[Fact]
	public void AugmentSkillLevel_NamesTheSkill()
	{
		this.CreateRecord(SkillId, "Skill_AttackWeapon", Str("skillDisplayName", "tagOnslaught"));
		var item = this.CreateItem("ArmorJewelry_Ring", Int("augmentSkillLevel1", 2), Str("augmentSkillName1", SkillId));

		this.BaseAttributes(item).Should().Equal($"{Epic}+2 to Onslaught");
	}

	[Fact]
	public void AugmentSkillLevel_OfABuffSkill_TakesTheNameOfTheBuff()
	{
		this.CreateRecord(SkillId, "Skill_BuffSelfToggled", Str("buffSkillName", BuffId));
		this.CreateRecord(BuffId, "SkillBuff_Passive", Str("skillDisplayName", "tagOnslaught"));
		var item = this.CreateItem("ArmorJewelry_Ring", Int("augmentSkillLevel2", 1), Str("augmentSkillName2", SkillId));

		this.BaseAttributes(item).Should().Equal($"{Epic}+1 to Onslaught");
	}

	[Fact]
	public void AugmentSkillLevel_OfAPetModifier_TakesTheNameOfThePetSkill()
	{
		this.CreateRecord(SkillId, "SkillSecondary_PetModifier", Str("petSkillName", """records\test\petskill.dbr"""));
		this.CreateRecord("""records\test\petskill.dbr""", "Skill_AttackWeapon", Str("skillDisplayName", "tagOnslaught"));
		var item = this.CreateItem("ArmorJewelry_Ring", Int("augmentSkillLevel1", 1), Str("augmentSkillName1", SkillId));

		this.BaseAttributes(item).Should().Equal($"{Epic}+1 to Onslaught");
	}

	[Fact]
	public void AugmentSkillLevel_OfAPetModifierOfABuff_FollowsBothRedirections()
	{
		this.CreateRecord(SkillId, "SkillSecondary_PetModifier", Str("petSkillName", """records\test\petskill.dbr"""));
		this.CreateRecord("""records\test\petskill.dbr""", "Skill_BuffOther", Str("buffSkillName", BuffId));
		this.CreateRecord(BuffId, "SkillBuff_Passive", Str("skillDisplayName", "tagOnslaught"));
		var item = this.CreateItem("ArmorJewelry_Ring", Int("augmentSkillLevel1", 1), Str("augmentSkillName1", SkillId));

		this.BaseAttributes(item).Should().Equal($"{Epic}+1 to Onslaught");
	}

	[Fact]
	public void AugmentSkillLevel_OfAnUnnamedSkill_UsesItsFileName()
	{
		this.CreateRecord(SkillId, "Skill_AttackWeapon");
		var item = this.CreateItem("ArmorJewelry_Ring", Int("augmentSkillLevel1", 1), Str("augmentSkillName1", SkillId));

		this.BaseAttributes(item).Should().Equal($"{Epic}+1 to skill");
	}

	[Fact]
	public void AugmentSkillLevel_WithoutTranslation_FallsBackToEnglish()
	{
		this.Translations.Remove("ItemSkillIncrement");
		this.CreateRecord(SkillId, "Skill_AttackWeapon", Str("skillDisplayName", "tagOnslaught"));
		var item = this.CreateItem("ArmorJewelry_Ring", Int("augmentSkillLevel1", 2), Str("augmentSkillName1", SkillId));

		this.BaseAttributes(item).Should().Equal($"{Legendary}?+2 to skill Onslaught?");
	}

	[Fact]
	public void AugmentSkillLevel_WithoutSkillName_IsShownRaw()
	{
		var item = this.CreateItem("ArmorJewelry_Ring", Int("augmentSkillLevel1", 2));

		this.BaseAttributes(item).Should().Equal($"{Legendary}?augmentSkillLevel1?: 2");
	}

	[Fact]
	public void AttributeCount_CountsEachGroupOnce_AndSkillLevelsByTheirValue()
	{
		// The count feeds the "totalAttCount" of the requirement equations.
		this.CreateRecord(SkillId, "Skill_AttackWeapon", Str("skillDisplayName", "tagOnslaught"));
		var item = this.CreateItem("ArmorJewelry_Ring",
			Int("augmentSkillLevel1", 2), Str("augmentSkillName1", SkillId),
			Int("characterStrength", 5),
			Float("offensiveStunMin", 1f), Float("offensiveStunMax", 2f), Float("offensiveStunChance", 10f),
			Float("offensivePhysicalMin", 3f));

		this.BaseAttributes(item);

		item.attributeCount.Should().Be(2 + 1 + 1, "skill level 2, strength, stun; base physical damage is not counted");
		item.isAttributeCounted.Should().BeTrue();
	}

	#endregion

	#region Granted skills

	[Theory]
	[InlineData("LowHealth", "(Activated on low health)")]
	[InlineData("LowMana", "(Activated on low energy)")]
	[InlineData("HitByEnemy", "(Activated upon taking damage)")]
	[InlineData("HitByMelee", "(Activated upon taking melee damage)")]
	[InlineData("HitByProjectile", "(Activated upon taking ranged damage)")]
	[InlineData("CastBuff", "(Activated upon casting a buff)")]
	[InlineData("AttackEnemy", "(Activated on attack)")]
	[InlineData("OnEquip", "(Activated when equipped)")]
	public void GrantedSkill_WithAnAutoController_NamesWhenItTriggers(string triggerType, string activation)
	{
		this.CreateRecord(ControllerId, "ItemSkillAutoController", Str("triggerType", triggerType));
		this.CreateRecord(SkillId, "Skill_BuffSelfDuration", Str("skillDisplayName", "tagSkillTest"));
		var item = this.CreateItem("ArmorJewelry_Amulet", Str("itemSkillName", SkillId), Str("itemSkillAutoController", ControllerId));

		this.BaseAttributes(item).Should().ContainInConsecutiveOrder(
			string.Empty,
			$"{Mundane}Grants Skill :",
			$"{Mundane}Test Skill {activation}");
	}

	[Fact]
	public void GrantedSkill_WithAnUnknownTrigger_IsAMagicalLineWithoutActivation()
	{
		this.CreateRecord(ControllerId, "ItemSkillAutoController", Str("triggerType", "SomethingNew"));
		this.CreateRecord(SkillId, "Skill_BuffSelfDuration", Str("skillDisplayName", "tagSkillTest"));
		var item = this.CreateItem("ArmorJewelry_Amulet", Str("itemSkillName", SkillId), Str("itemSkillAutoController", ControllerId));

		this.BaseAttributes(item).Should().Contain($"{Epic}Test Skill");
	}

	[Fact]
	public void UnnamedGrantedSkill_WithAnActivation_ShowsTheActivationWithoutLeadingSpace()
	{
		this.CreateRecord(ControllerId, "ItemSkillAutoController", Str("triggerType", "LowHealth"));
		this.CreateRecord(SkillId, "Skill_BuffSelfDuration");
		var item = this.CreateItem("ArmorJewelry_Amulet", Str("itemSkillName", SkillId), Str("itemSkillAutoController", ControllerId));

		this.BaseAttributes(item).Should().ContainInConsecutiveOrder($"{Mundane}Grants Skill :", $"{Mundane}(Activated on low health)");
	}

	[Fact]
	public void GrantedSkill_WithAnAutoController_ShowsItsDescriptionLevelAndEffects()
	{
		this.Settings.ShowSkillLevel = true;
		this.CreateRecord(ControllerId, "ItemSkillAutoController", Str("triggerType", "LowHealth"));
		this.CreateRecord(SkillId, "Skill_BuffSelfDuration",
			Str("skillDisplayName", "tagSkillTest"), Str("skillBaseDescription", "tagSkillTestDescription"), Ints("characterLife", 10, 20, 30));
		var item = this.CreateItem("ArmorJewelry_Amulet",
			Str("itemSkillName", SkillId), Str("itemSkillAutoController", ControllerId), Int("itemSkillLevel", 2));

		this.BaseAttributes(item).Should().Equal(
			string.Empty,
			$"{Mundane}Grants Skill :",
			$"{Mundane}Test Skill (Activated on low health)",
			$"{Mundane}    Heals you.",
			$"{Mundane}Level:   2",
			string.Empty,
			$"{Epic}+20 Health");
	}

	[Fact]
	public void GrantedSkill_WithoutAutoController_IsOnlyNamed()
	{
		this.CreateRecord(SkillId, "Skill_BuffSelfDuration",
			Str("skillDisplayName", "tagSkillTest"), Str("skillBaseDescription", "tagSkillTestDescription"), Ints("characterLife", 10));
		var item = this.CreateItem("ArmorJewelry_Amulet", Str("itemSkillName", SkillId));

		this.BaseAttributes(item).Should().Equal(string.Empty, $"{Mundane}Grants Skill :", $"{Epic}Test Skill");
	}

	[Fact]
	public void GrantedBuffSkill_ShowsTheBuffDescription_AndTheEffectsAtTheItemSkillLevel()
	{
		this.Settings.ShowSkillLevel = true;
		this.CreateRecord(ControllerId, "ItemSkillAutoController", Str("triggerType", "OnEquip"));
		this.CreateRecord(SkillId, "Skill_BuffRadiusToggled", Str("buffSkillName", BuffId));
		this.CreateRecord(BuffId, "SkillBuff_Passive",
			Str("skillDisplayName", "tagSkillTest"), Str("skillBaseDescription", "tagSkillTestDescription"), Ints("characterLife", 10, 20, 30));
		var item = this.CreateItem("ArmorJewelry_Amulet",
			Str("itemSkillName", SkillId), Str("itemSkillAutoController", ControllerId), Int("itemSkillLevel", 3));

		this.BaseAttributes(item).Should().Equal(
			string.Empty,
			$"{Mundane}Grants Skill :",
			$"{Mundane}Test Skill (Activated when equipped)",
			$"{Mundane}    Heals you.",
			$"{Mundane}    Level:   3",
			string.Empty,
			$"{Epic}+30 Health");
	}

	[Fact]
	public void GrantedSkill_WithoutSkillLevelSetting_HidesTheLevel()
	{
		this.Settings.ShowSkillLevel = false;
		this.CreateRecord(ControllerId, "ItemSkillAutoController", Str("triggerType", "LowHealth"));
		this.CreateRecord(SkillId, "Skill_BuffSelfDuration",
			Str("skillDisplayName", "tagSkillTest"), Str("skillBaseDescription", "tagSkillTestDescription"));
		var item = this.CreateItem("ArmorJewelry_Amulet",
			Str("itemSkillName", SkillId), Str("itemSkillAutoController", ControllerId), Int("itemSkillLevel", 2));

		this.BaseAttributes(item).Should().NotContain(line => line.Contains("Level", StringComparison.Ordinal));
	}

	[Fact]
	public void GrantedSkill_WithAnUntranslatedName_UsesTheSkillFileName()
	{
		this.CreateRecord(SkillId, "Skill_BuffSelfDuration", Str("skillDisplayName", "tagNotTranslated"));
		var item = this.CreateItem("ArmorJewelry_Amulet", Str("itemSkillName", SkillId));

		this.BaseAttributes(item).Should().Contain($"{Epic}skill");
	}

	[Fact]
	public void GrantedSkill_WithAMissingRecord_IsShownRaw()
	{
		var item = this.CreateItem("ArmorJewelry_Amulet", Str("itemSkillName", SkillId));

		this.BaseAttributes(item).Should().ContainSingle().Which.Should().Contain("itemSkillName");
	}

	[Fact]
	public void TriggeredSkill_IsShownAtTheLevelGrantedByTheItem()
	{
		this.CreateRecord(ControllerId, "ItemSkillAutoController", Str("triggerType", "LowHealth"));
		this.CreateRecord(SkillId, "Skill_BuffSelfDuration", Str("skillDisplayName", "tagSkillTest"), Ints("characterLife", 10, 20, 30, 40));
		var item = this.CreateItem("ArmorJewelry_Amulet",
			Str("itemSkillName", SkillId), Str("itemSkillAutoController", ControllerId), Int("itemSkillLevel", 4));

		this.BaseAttributes(item).Should().EndWith($"{Epic}+40 Health");
	}

	[Fact]
	public void GetTriggeredSkillLevel_UsesTheItemLevel_OnlyForTheGrantedSkillOrItsBuff()
	{
		this.CreateRecord(SkillId, "Skill_BuffRadiusToggled", Str("buffSkillName", BuffId));
		var buff = this.CreateRecord(BuffId, "SkillBuff_Passive");
		var other = this.CreateRecord("""records\test\other.dbr""", "SkillBuff_Passive");
		var item = this.CreateItem("ArmorJewelry_Amulet",
			Str("itemSkillName", SkillId), Str("itemSkillAutoController", ControllerId), Int("itemSkillLevel", 5));
		var skill = this.MockDatabase.Object.GetRecordFromFile(RecordId.Create(SkillId))!;

		this.Provider.GetTriggeredSkillLevel(item, skill, skill.Id, 0).Should().Be(4);
		this.Provider.GetTriggeredSkillLevel(item, buff, buff.Id, 0).Should().Be(4);
		this.Provider.GetTriggeredSkillLevel(item, other, other.Id, 7).Should().Be(7);
		this.Provider.GetTriggeredSkillLevel(new Item(), skill, skill.Id, 7).Should().Be(7);
	}

	#endregion

	#region Skill parameters

	[Fact]
	public void ScrollSkill_ShowsItsParameters_WithTheGameUnitFormats()
	{
		this.CreateRecord(SkillId, "Skill_AttackRadius",
			Int("skillManaCost", 50), Float("skillActiveDuration", 10f), Float("skillTargetRadius", 5f), Int("skillTargetNumber", 3));
		var scroll = this.CreateItem("OneShot_Scroll", Str("skillName", SkillId));

		this.BaseAttributes(scroll).Should().BeEquivalentTo(
			$"{Epic}50 Energy Cost",
			$"{Epic}10.0 Second Duration",
			$"{Epic}5.0 Meter Radius",
			$"{Epic}3 Target Maximum");
	}

	[Fact]
	public void SkillParameter_WithoutUnitFormat_FallsBackToEnglish()
	{
		this.Translations.Remove("SkillSecondFormat");
		this.CreateRecord(SkillId, "Skill_AttackRadius", Float("skillActiveDuration", 10f));
		var scroll = this.CreateItem("OneShot_Scroll", Str("skillName", SkillId));

		this.BaseAttributes(scroll).Should().Equal($"{Legendary}?10 Duration?");
	}

	[Fact]
	public void SkillParameter_WithoutLabel_ShowsTheAttributeBetweenQuestionMarks()
	{
		this.Translations.Remove("ManaCost");
		this.CreateRecord(SkillId, "Skill_AttackRadius", Int("skillManaCost", 50));
		var scroll = this.CreateItem("OneShot_Scroll", Str("skillName", SkillId));

		this.BaseAttributes(scroll).Should().Equal($"{Epic}50 ?ManaCost?");
	}

	[Theory]
	[InlineData(0.5f, "1")]
	[InlineData(1.5f, "2")]
	[InlineData(0.4f, "0")]
	[InlineData(12f, "12")]
	public void SkillCost_IsRoundedToAWholeNumber_HalfAwayFromZero(float cost, string shown)
	{
		// Ravages of Time: 0.5 energy per second, which the game writes "1".
		this.CreateRecord(SkillId, "Skill_Modifier", Float("skillActiveManaCost", cost));
		var scroll = this.CreateItem("OneShot_Scroll", Str("skillName", SkillId));

		this.BaseAttributes(scroll).Should().Equal($"{Epic}{shown} Active Energy Cost per Second");
	}

	[Fact]
	public void SkillWindow_ListsTheEffectsInTheGameOrder()
	{
		// Earth Enchantment level 5 in the game: recharge, active energy cost, radius, the % bonuses, the flat damage,
		// then the energy reserved (character store).
		this.Translations["characterManaLimitReserve"] = "{%.0f0} Energy Reserved";
		this.CreateRecord(SkillId, "Skill_BuffRadiusToggled", Str("skillDisplayName", "tagSkillTest"),
			Int("characterManaLimitReserve", 75), Float("offensiveFireMin", 5f), Float("offensiveFireMax", 8f),
			Float("offensiveSlowFireModifier", 44f), Float("offensiveFireModifier", 44f), Float("skillTargetRadius", 8.4f),
			Float("skillActiveManaCost", 1f), Float("skillCooldownTime", 4f));

		var skill = this.Provider.GetSkillAtLevel(SkillId, 1, TQVaultAE.Application.SkillWindowContext.None);

		skill!.Attributes.Select(a => a.RemoveLeadingColorTag()).Should().Equal(
			"{^a}4.0 Second(s) Recharge",
			"1 Active Energy Cost per Second",
			"8.4 Meter Radius",
			"+44% Fire Damage",
			"+44% Burn Damage",
			"5 ~ 8 Fire Damage",
			"75 Energy Reserved");
	}

	[Fact]
	public void SkillWindow_ChanceOfActivating_ComesBeforeTheEnergyCost()
	{
		this.Translations["SkillActivationChance"] = "Chance of Activating";
		this.CreateRecord(SkillId, "Skill_PassiveOnHitBuffSelf", Str("skillDisplayName", "tagSkillTest"),
			Int("skillManaCost", 20), Float("skillActiveDuration", 10f), Float("onHitActivationChance", 8f), Float("skillCooldownTime", 12f));

		var skill = this.Provider.GetSkillAtLevel(SkillId, 1, TQVaultAE.Application.SkillWindowContext.None);

		skill!.Attributes.Select(a => a.RemoveLeadingColorTag()).Should().Equal(
			"{^a}12.0 Second(s) Recharge",
			"8% Chance of Activating",
			"20 Energy Cost",
			"10.0 Second Duration");
	}

	[Fact]
	public void SkillWindow_EnergyCost_IsRoundedLikeTheGame()
	{
		this.CreateRecord(SkillId, "Skill_AttackRadius", Str("skillDisplayName", "tagSkillTest"), Floats("skillManaCost", 10.5f, 12.5f));

		var skill = this.Provider.GetSkillAtLevel(SkillId, 2, TQVaultAE.Application.SkillWindowContext.None);

		skill!.Attributes.Should().Equal($"{Mundane}13 Energy Cost");
	}

	#endregion
}
