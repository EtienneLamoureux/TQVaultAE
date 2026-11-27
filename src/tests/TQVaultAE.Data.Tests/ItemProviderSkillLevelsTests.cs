using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Tests the skill levels <see cref="TQVaultAE.Data.ItemProvider"/> reads for the item texts: the skills an item grants
/// with their level, the level a triggered skill and a pet skill are shown at.
/// </summary>
public class ItemProviderSkillLevelsTests : ItemProviderTextTestBase
{
	private const string SkillId = """records\test\skill.dbr""";
	private const string BuffId = """records\test\buff.dbr""";
	private const string ControllerId = """records\test\controller.dbr""";
	private const string SpawnSkillId = """records\test\summonwolf.dbr""";
	private const string PetId = """records\test\wolf.dbr""";
	private const string BiteId = """records\test\bite.dbr""";

	[Fact]
	public void GetGrantedSkillLevels_ListsEverySkillWithItsLevel_HiddenOnesIncluded()
	{
		this.CreateRecord("""records\test\hidden.dbr""", "Skill_Passive", Ints("characterLife", 10));
		this.CreateRecord("""records\test\prefix.dbr""", "LootRandomizer", Str("itemSkillName", """records\test\hidden.dbr"""), Int("itemSkillLevel", 3));
		var item = this.CreateItem("ArmorJewelry_Amulet", Str("itemSkillName", """records\test\skill.dbr"""));
		item.prefixID = RecordId.Create("""records\test\prefix.dbr""");

		this.Provider.GetGrantedSkillLevels(item).Should().Equal(
			(RecordId.Create("""records\test\skill.dbr"""), 1), (RecordId.Create("""records\test\hidden.dbr"""), 3));
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
}
