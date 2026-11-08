using AwesomeAssertions;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Application;

public class CharacterSkillEditorTests
{
	private static readonly RecordId StormMastery = """Records\Skills\Storm\StormMastery.dbr""".ToRecordId();
	private static readonly RecordId IceShard = """Records\Skills\Storm\IceShard.dbr""".ToRecordId();
	private static readonly RecordId Velocity = """Records\Skills\Storm\IceShard_Velocity.dbr""".ToRecordId();
	private static readonly RecordId Squall = """Records\Skills\Storm\Squall.dbr""".ToRecordId();
	private static readonly RecordId EarthMastery = """Records\Skills\Earth\EarthMastery.dbr""".ToRecordId();
	private static readonly RecordId HeatShield = """Records\Skills\Earth\HeatShield.dbr""".ToRecordId();

	/// <summary>Ice Shard (tier 1, max 12), its modifier Velocity (tier 3: mastery 10), Squall (tier 2: mastery 4).</summary>
	private static readonly MasteryTree Storm = new(Masteries.Storm, StormMastery, "tagStorm", "tagStormDesc", "", 40,
	[
		new MasterySkill(IceShard, "tagIceShard", "", "", 12, 1, null),
		new MasterySkill(Squall, "tagSquall", "", "", 12, 2, null),
		new MasterySkill(Velocity, "tagVelocity", "", "", 8, 3, IceShard),
	]);

	private static readonly MasteryTree Earth = new(Masteries.Earth, EarthMastery, "tagEarth", "", "", 40,
	[
		new MasterySkill(HeatShield, "tagHeatShield", "", "", 10, 1, null),
	]);

	private static MasteryTree? Trees(Masteries mastery) => mastery switch
	{
		Masteries.Storm => Storm,
		Masteries.Earth => Earth,
		_ => null,
	};

	private static SkillRecord Skill(string name, int level) => new() { skillName = name, skillLevel = level, skillEnabled = 1 };

	/// <summary>A default attack, Storm 10 with Ice Shard 5, Velocity 3 and Squall 2, then a quest reward.</summary>
	private static List<SkillRecord> StormCharacter() =>
	[
		Skill("""Records/Skills/Default/DefaultWeaponAttack.dbr""", 1),
		Skill(StormMastery.Raw, 10),
		Skill(IceShard.Raw, 5),
		Skill(Velocity.Raw, 3),
		Skill(Squall.Raw, 2),
		Skill("""Records\Quests\Rewards\JG10 - Chirons Reward NORMAL.dbr""", 1),
	];

	private static CharacterSkillEditor Editor(List<SkillRecord> records, int points = 5, int masteriesAllowed = 2)
		=> new(records, masteriesAllowed, points, Trees);

	[Fact]
	public void Constructor_SavedMasteries_FillTheSlotsInSaveOrderAndLeaveTheOthersEmpty()
	{
		var records = StormCharacter();
		records.Insert(1, Skill(EarthMastery.Raw, 1));

		var editor = Editor(records);

		editor.Slots.Should().Equal(Earth, Storm);
		editor.GetMasteryLevel(1).Should().Be(10);
		editor.GetPointsSpent(1).Should().Be(10 + 5 + 3 + 2);
		Editor(StormCharacter()).Slots.Should().Equal(Storm, null);
	}

	[Fact]
	public void SetSkillLevel_Raise_SpendsPointsAndStopsWhenNoneAreLeft()
	{
		var editor = Editor(StormCharacter(), points: 3);

		var level = editor.SetSkillLevel(0, IceShard, 12);

		level.Should().Be(8);
		editor.AvailablePoints.Should().Be(0);
		editor.GetLevel(IceShard).Should().Be(8);
	}

	[Fact]
	public void SetSkillLevel_AboveMaxLevel_IsClampedToTheSkillMax()
	{
		var editor = Editor(StormCharacter(), points: 50);

		editor.SetSkillLevel(0, IceShard, 30).Should().Be(12);
		editor.AvailablePoints.Should().Be(50 - 7);
	}

	[Fact]
	public void SetSkillLevel_SavedLevelAboveMax_CanBeLoweredButNotRaised()
	{
		var records = StormCharacter();
		records[2].skillLevel = 15;
		var editor = Editor(records, points: 10);

		editor.SetSkillLevel(0, IceShard, 16).Should().Be(15);
		editor.SetSkillLevel(0, IceShard, 14).Should().Be(14);
		editor.AvailablePoints.Should().Be(11);
	}

	[Fact]
	public void SetSkillLevel_TierAboveMasteryLevel_StaysLocked()
	{
		var records = StormCharacter();
		records[1].skillLevel = 3;
		records.RemoveRange(3, 2);
		var editor = Editor(records);

		editor.IsUnlocked(0, Storm.Skills[1]).Should().BeFalse("Squall needs mastery 4");
		editor.SetSkillLevel(0, Squall, 2).Should().Be(0);
		editor.AvailablePoints.Should().Be(5);
	}

	[Fact]
	public void SetSkillLevel_ModifierWithoutPointInItsSkill_StaysLocked()
	{
		var records = StormCharacter();
		records.RemoveRange(2, 2);
		var editor = Editor(records);

		editor.SetSkillLevel(0, Velocity, 1).Should().Be(0);
		editor.SetSkillLevel(0, IceShard, 1);
		editor.SetSkillLevel(0, Velocity, 1).Should().Be(1);
	}

	[Fact]
	public void SetSkillLevel_LastPointOfSkillTaken_ResetsItsModifiersAndRefundsThem()
	{
		var editor = Editor(StormCharacter(), points: 0);

		editor.SetSkillLevel(0, IceShard, 0);

		editor.GetLevel(Velocity).Should().Be(0);
		editor.GetLevel(Squall).Should().Be(2);
		editor.AvailablePoints.Should().Be(5 + 3);
	}

	[Fact]
	public void SetMasteryLevel_BelowATier_ResetsItsSkillsAndTheirModifiers()
	{
		var editor = Editor(StormCharacter(), points: 0);

		var level = editor.SetMasteryLevel(0, 3);

		level.Should().Be(3);
		editor.GetLevel(Squall).Should().Be(0, "tier 2 needs mastery 4");
		editor.GetLevel(Velocity).Should().Be(0, "tier 3 needs mastery 10");
		editor.GetLevel(IceShard).Should().Be(5);
		editor.AvailablePoints.Should().Be(7 + 2 + 3);
	}

	[Fact]
	public void SetMasteryLevel_Zero_KeepsLevelOne()
	{
		var editor = Editor(StormCharacter(), points: 0);

		editor.SetMasteryLevel(0, 0).Should().Be(1);
		editor.Slots[0].Should().Be(Storm);
	}

	[Fact]
	public void SetMasteryLevel_Raise_IsLimitedByPointsAndMaxLevel()
	{
		var editor = Editor(StormCharacter(), points: 100);

		editor.SetMasteryLevel(0, 99).Should().Be(40);
		editor.AvailablePoints.Should().Be(70);
	}

	[Fact]
	public void SetMastery_Replaced_RefundsItsPointsAndStartsTheNewOneAtLevelOne()
	{
		var editor = Editor(StormCharacter(), points: 0);

		editor.SetMastery(0, Masteries.Earth).Should().BeTrue();

		editor.Slots[0].Should().Be(Earth);
		editor.GetLevel(EarthMastery).Should().Be(1);
		editor.GetLevel(IceShard).Should().Be(0);
		editor.AvailablePoints.Should().Be(10 + 5 + 3 + 2 - 1);
		editor.MasteriesChanged.Should().BeTrue();
	}

	[Fact]
	public void SetMastery_AlreadyInOtherSlotOrNoPoint_IsRefusedWithNothingChanged()
	{
		var editor = Editor(StormCharacter(), points: 0);

		editor.SetMastery(1, Masteries.Storm).Should().BeFalse();
		editor.SetMastery(1, Masteries.Earth).Should().BeFalse("no skill point left");
		editor.SetMastery(1, Masteries.Dream).Should().BeFalse("the game has no tree for it here");

		editor.Slots.Should().Equal(Storm, null);
		editor.Changed.Should().BeFalse();
	}

	[Fact]
	public void SetMastery_None_RefundsAllItsPoints()
	{
		var editor = Editor(StormCharacter(), points: 1);

		editor.SetMastery(0, null).Should().BeTrue();

		editor.Slots[0].Should().BeNull();
		editor.AvailablePoints.Should().Be(1 + 20);
	}

	[Fact]
	public void BuildSkillRecords_NoChange_GivesBackTheSameRecordsInTheSameOrder()
	{
		var records = StormCharacter();
		var editor = Editor(records);

		editor.SetSkillLevel(0, IceShard, 6);
		editor.SetSkillLevel(0, IceShard, 5);

		editor.Changed.Should().BeFalse();
		editor.BuildSkillRecords().Should().Equal(records);
	}

	[Fact]
	public void BuildSkillRecords_Edits_UpdateInPlaceRemoveEmptiedAndAppendNewSkills()
	{
		var records = StormCharacter();
		records[2].skillActive = 1;
		var editor = Editor(records, points: 10);

		editor.SetSkillLevel(0, IceShard, 7);
		editor.SetSkillLevel(0, Squall, 0);
		editor.SetMastery(1, Masteries.Earth);
		editor.SetSkillLevel(1, HeatShield, 2);

		var result = editor.BuildSkillRecords();

		result.Select(r => (r.skillName, r.skillLevel)).Should().Equal(
			(records[0].skillName, 1),
			(StormMastery.Raw, 10),
			(IceShard.Raw, 7),
			(Velocity.Raw, 3),
			(records[5].skillName, 1),
			(EarthMastery.Raw, 1),
			(HeatShield.Raw, 2));
		result[0].Should().BeSameAs(records[0], "skills of no mastery are kept as they are");
		result[2].skillActive.Should().Be(1, "a changed skill keeps its other values");
		result[5].skillEnabled.Should().Be(1);
	}

	[Fact]
	public void BuildSkillRecords_MasteryRemoved_DropsItsSkillsAndKeepsTheOthers()
	{
		var records = StormCharacter();
		var editor = Editor(records);

		editor.SetMastery(0, null);

		editor.BuildSkillRecords().Should().Equal(records[0], records[5]);
	}
}
