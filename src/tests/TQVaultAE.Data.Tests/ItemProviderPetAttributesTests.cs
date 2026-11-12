using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Tests the pet lines of the item text: the statistics and abilities of a summoned pet, and the bonus to all pets.
/// </summary>
public class ItemProviderPetAttributesTests : ItemProviderTextTestBase
{
	private const string SpawnSkillId = """records\test\summonwolf.dbr""";
	private const string PetId = """records\test\wolf.dbr""";
	private const string BiteId = """records\test\bite.dbr""";

	public ItemProviderPetAttributesTests()
	{
		this.Translations["SkillPetLimit"] = "{%d0} Summon Limit";
		this.Translations["SkillPetDescriptionHeading"] = "{%s0} Attributes:";
		this.Translations["tagSkillPetTimeToLive"] = "Life Time {%.1f0} Seconds";
		this.Translations["SkillPetDescriptionHealth"] = "{%.0f0}  Health";
		this.Translations["SkillPetDescriptionMana"] = "{%.0f0}  Energy";
		this.Translations["tagSkillPetAbilities"] = "{%s0} Abilities:";
		this.Translations["SkillPetDescriptionDamageMinOnly"] = "{%.0f0}  Damage";
		this.Translations["SkillPetDescriptionDamageMinMax"] = "{%.0f0} - {%.0f1}  Damage";
		this.Translations["xtagPetBonusNameAllPets"] = "Bonus to All Pets:";
		this.Translations["tagWolf"] = "Wolf";
		this.Translations["tagBite"] = "Bite";
		this.Translations["tagHowl"] = "Howl";
	}

	[Fact]
	public void SummonScroll_ShowsThePetStatistics_ThenEachActiveSkillWithItsEffects()
	{
		this.CreateRecord(SpawnSkillId, "Skill_SpawnPet",
			Int("petLimit", 3), Str("spawnObjects", PetId), Float("spawnObjectsTimeToLive", 30f));
		this.CreateRecord(PetId, "Monster",
			Str("description", "tagWolf"),
			Float("characterLife", 500f), Float("characterMana", 100f),
			Float("handHitDamageMin", 10f), Float("handHitDamageMax", 20f),
			Str("skillName0", BiteId), Int("skillLevel0", 2),
			Str("skillName1", """records\test\passive.dbr"""),
			Str("skillName2", """records\test\howl.dbr"""));
		this.CreateRecord(BiteId, "Skill_AttackWeapon", Str("skillDisplayName", "tagBite"), Floats("offensivePhysicalMin", 5f, 10f, 15f));
		this.CreateRecord("""records\test\passive.dbr""", "Skill_Passive", Str("skillDisplayName", "tagBite"));
		this.CreateRecord("""records\test\howl.dbr""", "Skill_BuffSelfDuration", Str("buffSkillName", """records\test\howlbuff.dbr"""));
		this.CreateRecord("""records\test\howlbuff.dbr""", "SkillBuff_Passive", Str("skillDisplayName", "tagHowl"), Int("characterLife", 50));
		var scroll = this.CreateItem("OneShot_Scroll", Str("skillName", SpawnSkillId));

		this.BaseAttributes(scroll).Should().Equal(
			$"{Mundane}3 Summon Limit",
			$"{Mundane}Wolf Attributes:",
			$"{Mundane}Life Time 30.0 Seconds",
			$"{Mundane}500  Health",
			$"{Mundane}100  Energy",
			string.Empty,
			$"{Mundane}Wolf Abilities:",
			$"{Mundane}10 - 20  Damage",
			$"{Mundane}Bite",
			$"{Epic}10 Physical Damage", // the pet has the skill at level 2
			string.Empty,
			$"{Mundane}Howl",
			$"{Epic}+50 Health",
			string.Empty);
	}

	[Fact]
	public void SinglePermanentPet_HasNoLimitNorLifeTime_AndAMinimumDamageOnly()
	{
		this.CreateRecord(SpawnSkillId, "Skill_SpawnPet", Int("petLimit", 1), Str("spawnObjects", PetId));
		this.CreateRecord(PetId, "Monster", Str("description", "tagWolf"), Float("handHitDamageMin", 10f));
		var scroll = this.CreateItem("OneShot_Scroll", Str("skillName", SpawnSkillId));

		this.BaseAttributes(scroll).Should().Equal(
			$"{Mundane}Wolf Attributes:",
			string.Empty,
			$"{Mundane}Wolf Abilities:",
			$"{Mundane}10  Damage");
	}

	[Fact]
	public void PetWithEqualOrNegligibleDamage_ShowsOneValueOrNone()
	{
		this.CreateRecord(SpawnSkillId, "Skill_SpawnPet", Str("spawnObjects", PetId));
		this.CreateRecord(PetId, "Monster", Str("description", "tagWolf"), Float("handHitDamageMin", 12f), Float("handHitDamageMax", 12f));
		var equal = this.CreateItem("OneShot_Scroll", Str("skillName", SpawnSkillId));

		this.BaseAttributes(equal).Should().Contain($"{Mundane}12  Damage");

		this.CreateRecord("""records\test\weak.dbr""", "Skill_SpawnPet", Str("spawnObjects", """records\test\weakpet.dbr"""));
		this.CreateRecord("""records\test\weakpet.dbr""", "Monster", Str("description", "tagWolf"), Float("handHitDamageMin", 1f), Float("handHitDamageMax", 2f));
		var weak = this.CreateItemWithId("""records\test\weakscroll.dbr""", "OneShot_Scroll", Str("skillName", """records\test\weak.dbr"""));

		this.BaseAttributes(weak).Should().NotContain(line => line.Contains("Damage", StringComparison.Ordinal));
	}

	[Fact]
	public void PetSkills_SkipNonRecordIds_AndShowUnnamedSkillsByTheirTag()
	{
		this.CreateRecord(SpawnSkillId, "Skill_SpawnPet", Str("spawnObjects", PetId));
		this.CreateRecord(PetId, "Monster", Str("description", "tagWolf"),
			Str("skillName0", "notarecord.dbr"),
			Str("skillName1", """records\test\unnamed.dbr"""));
		this.CreateRecord("""records\test\unnamed.dbr""", "Skill_AttackWeapon", Str("skillDisplayName", "tagNotTranslated"));
		var scroll = this.CreateItem("OneShot_Scroll", Str("skillName", SpawnSkillId));

		this.BaseAttributes(scroll).Should().EndWith([$"{Legendary}tagNotTranslated", string.Empty]);
	}

	[Fact]
	public void PetStatistics_WithoutTranslations_FallBackToEnglish()
	{
		foreach (var tag in new[] { "SkillPetLimit", "SkillPetDescriptionHeading", "tagSkillPetTimeToLive", "SkillPetDescriptionHealth",
			"SkillPetDescriptionMana", "tagSkillPetAbilities", "SkillPetDescriptionDamageMinMax" })
			this.Translations.Remove(tag);
		this.CreateRecord(SpawnSkillId, "Skill_SpawnPet",
			Int("petLimit", 2), Str("spawnObjects", PetId), Float("spawnObjectsTimeToLive", 15f));
		this.CreateRecord(PetId, "Monster", Str("description", "tagWolf"),
			Float("characterLife", 500f), Float("characterMana", 100f), Float("handHitDamageMin", 10f), Float("handHitDamageMax", 20f));
		var scroll = this.CreateItem("OneShot_Scroll", Str("skillName", SpawnSkillId));

		this.BaseAttributes(scroll).Should().Equal(
			$"{Mundane}2 Summon Limit",
			$"{Mundane}Wolf Attributes:",
			$"{Mundane}Life Time 15 Seconds",
			$"{Mundane}500  Health",
			$"{Mundane}100  Energy",
			string.Empty,
			$"{Mundane}Wolf Abilities:",
			$"{Mundane}10 - 20  Damage");
	}

	[Fact]
	public void SummonSkill_WithAMissingPetRecord_ShowsOnlyTheLimit()
	{
		this.CreateRecord(SpawnSkillId, "Skill_SpawnPet", Int("petLimit", 3), Str("spawnObjects", PetId));
		var scroll = this.CreateItem("OneShot_Scroll", Str("skillName", SpawnSkillId));

		this.BaseAttributes(scroll).Should().Equal($"{Mundane}3 Summon Limit");
	}

	[Fact]
	public void GetPetSkillLevel_TakesTheLevelOfTheSkillInThePetRecord()
	{
		this.CreateRecord(SpawnSkillId, "Skill_SpawnPet", Str("spawnObjects", PetId));
		this.CreateRecord(PetId, "Monster",
			Str("skillName0", """records\test\other.dbr"""), Int("skillLevel0", 9),
			Str("skillName1", BiteId), Int("skillLevel1", 4));
		var bite = this.CreateRecord(BiteId, "Skill_AttackWeapon");
		var buff = this.CreateRecord("""records\test\buff.dbr""", "SkillBuff_Passive");
		var scroll = this.CreateItem("OneShot_Scroll", Str("skillName", SpawnSkillId));

		this.Provider.GetPetSkillLevel(scroll, bite, bite.Id, 0).Should().Be(3);
		this.Provider.GetPetSkillLevel(scroll, buff, buff.Id, 7).Should().Be(7, "only attack skills are looked up");
		this.Provider.GetPetSkillLevel(this.CreateItem("OneShot_PotionHealth"), bite, bite.Id, 7).Should().Be(7, "the item summons no pet");
	}

	[Fact]
	public void PetBonus_ShowsAHeader_ThenTheIndentedBonuses()
	{
		this.CreateRecord("""records\test\petbonus.dbr""", "PetBonus", Int("characterLife", 50));
		var item = this.CreateItem("ArmorJewelry_Amulet", Str("petBonusName", """records\test\petbonus.dbr"""));

		this.BaseAttributes(item).Should().Equal(
			$"{Relic}{StringHelper.TQNewLineTag}Bonus to All Pets:",
			$"{Epic}    +50 Health",
			string.Empty);
	}

	[Fact]
	public void PetBonus_WithoutTranslation_OrRecord_ShowsAnEnglishHeaderOnly()
	{
		this.Translations.Remove("xtagPetBonusNameAllPets");
		var item = this.CreateItem("ArmorJewelry_Amulet", Str("petBonusName", """records\test\missing.dbr"""));

		this.BaseAttributes(item).Should().Equal($"{Legendary}{StringHelper.TQNewLineTag}?Bonus to All Pets:?");
	}
}
