using AwesomeAssertions;
using TQVaultAE.Application.Character;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Application;

/// <summary><see cref="CharacterSkillEditor.CaptureClassSet"/> and <see cref="CharacterSkillEditor.ApplyClassSet"/>: the masteries of a class set.</summary>
public class CharacterSkillEditorClassSetTests
{
	private static readonly RecordId StormMastery = """Records\Skills\Storm\StormMastery.dbr""".ToRecordId();
	private static readonly RecordId IceShard = """Records\Skills\Storm\IceShard.dbr""".ToRecordId();
	private static readonly RecordId Velocity = """Records\Skills\Storm\IceShard_Velocity.dbr""".ToRecordId();
	private static readonly RecordId EarthMastery = """Records\Skills\Earth\EarthMastery.dbr""".ToRecordId();
	private static readonly RecordId HeatShield = """Records\Skills\Earth\HeatShield.dbr""".ToRecordId();
	private const string QuestReward = """Records\Quests\Rewards\JG10 - Chirons Reward NORMAL.dbr""";

	/// <summary>Ice Shard (tier 1), its modifier Velocity (tier 3: mastery 10).</summary>
	private static readonly MasteryTree Storm = new(Masteries.Storm, StormMastery, "tagStorm", "", "", 40,
	[
		new MasterySkill(IceShard, "tagIceShard", "", "", 12, 1, null),
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

	/// <summary>Earth 4 with Heat Shield 2, a quest reward: 6 points spent in the masteries.</summary>
	private static CharacterSkillEditor EarthCharacter(int points, int masteriesAllowed = 2)
		=> new([Skill(EarthMastery.Raw, 4), Skill(HeatShield.Raw, 2), Skill(QuestReward, 1)], masteriesAllowed, points, Trees);

	/// <summary>Storm 10, Velocity 3 listed before Ice Shard 5 (Velocity needs Ice Shard and mastery 10): 18 points.</summary>
	private static readonly ClassSetMastery StormSet = new(Masteries.Storm, 10, [new(Velocity.Raw, 3), new(IceShard.Raw, 5)]);

	[Fact]
	public void ApplyClassSet_EmptySet_GivesEveryMasteryPointBackAndKeepsTheOtherSkills()
	{
		var editor = EarthCharacter(points: 10);

		var complete = editor.ApplyClassSet([]);

		complete.Should().BeTrue();
		editor.Slots.Should().AllSatisfy(slot => slot.Should().BeNull());
		editor.AvailablePoints.Should().Be(16, "the 6 points of Earth and Heat Shield go back to the 10 left");
		editor.BuildSkillRecords().Select(r => r.skillName).Should().Equal(QuestReward);
	}

	[Fact]
	public void ApplyClassSet_ModifierListedBeforeItsSkill_ReachesEveryLevelWithinThePoints()
	{
		var editor = EarthCharacter(points: 12);

		var complete = editor.ApplyClassSet([StormSet]);

		complete.Should().BeTrue();
		editor.Slots.Should().Equal(Storm, null);
		editor.GetLevel(IceShard).Should().Be(5);
		editor.GetLevel(Velocity).Should().Be(3);
		editor.AvailablePoints.Should().Be(0, "12 left + 6 given back pay the 18 points of the set");
		editor.CaptureClassSet().Single().SameAs(StormSet).Should().BeTrue();
	}

	[Fact]
	public void ApplyClassSet_TooFewPoints_AppliesInPartAndLosesNoPoint()
	{
		var editor = EarthCharacter(points: 2);

		var complete = editor.ApplyClassSet([StormSet]);

		complete.Should().BeFalse();
		editor.GetMasteryLevel(0).Should().Be(8, "the 8 points there are (2 left + 6 given back) go to the mastery first");
		(editor.GetPointsSpent(0) + editor.AvailablePoints).Should().Be(8);
	}

	[Fact]
	public void ApplyClassSet_MoreMasteriesThanSlots_AppliesTheFirstAndSaysItIsPartial()
	{
		var editor = EarthCharacter(points: 30, masteriesAllowed: 1);

		var complete = editor.ApplyClassSet([StormSet, new ClassSetMastery(Masteries.Earth, 4, [])]);

		complete.Should().BeFalse();
		editor.Slots.Should().Equal(Storm);
		editor.AvailablePoints.Should().Be(30 + 6 - 18);
	}

	[Fact]
	public void CaptureThenApply_OnTheSameCharacter_GivesTheSameBuildBack()
	{
		var editor = EarthCharacter(points: 10);
		var captured = editor.CaptureClassSet();
		editor.ApplyClassSet([StormSet]);

		var complete = editor.ApplyClassSet(captured);

		complete.Should().BeTrue();
		editor.GetMasteryLevel(0).Should().Be(4);
		editor.GetLevel(HeatShield).Should().Be(2);
		editor.AvailablePoints.Should().Be(10);
	}
}
