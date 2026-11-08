using AwesomeAssertions;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// The skills edited in the character edit dialog, written to the test character (Warfare 40, 13 skill points left)
/// and read back.
/// </summary>
public sealed class CharacterSkillsRoundTripTests : IDisposable
{
	private static readonly RecordId WarfareMastery = """Records\Skills\Warfare\WarfareMastery.dbr""".ToRecordId();
	private static readonly RecordId Onslaught = """Records\Skills\Warfare\Onslaught.dbr""".ToRecordId();
	private static readonly RecordId IgnorePain = """Records\Skills\Warfare\Onslaught_IgnorePain.dbr""".ToRecordId();
	private static readonly RecordId StormMastery = """Records\Skills\Storm\StormMastery.dbr""".ToRecordId();
	private static readonly RecordId IceShard = """Records\Skills\Storm\IceShard.dbr""".ToRecordId();

	private readonly SaveFixtures _fixtures = new();
	private readonly CharacterEditService _service;

	public CharacterSkillsRoundTripTests()
	{
		var trees = new Mock<ISkillTreeProvider>();
		trees.Setup(t => t.GetMasteryTree(Masteries.Warfare)).Returns(new MasteryTree(Masteries.Warfare, WarfareMastery, "", "", "", 40,
		[
			new MasterySkill(Onslaught, "", "", "", 12, 1, null),
			new MasterySkill(IgnorePain, "", "", "", 8, 2, Onslaught),
		]));
		trees.Setup(t => t.GetMasteryTree(Masteries.Storm)).Returns(new MasteryTree(Masteries.Storm, StormMastery, "", "", "", 40,
		[
			new MasterySkill(IceShard, "", "", "", 12, 1, null),
		]));
		this._service = new CharacterEditService(new PathIO(), this._fixtures.PlayerProvider, this._fixtures.Database.Object, trees.Object, this._fixtures.ItemProvider);
	}

	public void Dispose() => this._fixtures.Dispose();

	private static CharacterEditRequest RequestFrom(PlayerInfo info, CharacterSkillEditor editor)
		=> new(
			Level: info.CurrentLevel
			, XpText: info.CurrentXP.ToString()
			, DifficultyUnlocked: null
			, SkillPoints: editor.AvailablePoints
			, AttributePoints: info.AttributesPoints
			, Strength: info.BaseStrength
			, Dexterity: info.BaseDexterity
			, Intelligence: info.BaseIntelligence
			, Health: info.BaseHealth
			, Mana: info.BaseMana
			, SkillRecords: editor.BuildSkillRecords()
			, MasteriesChanged: editor.MasteriesChanged);

	private void Apply(PlayerCollection pc, Action<CharacterSkillEditor> edit)
	{
		var editor = this._service.CreateSkillEditor(pc.PlayerInfo!);
		edit(editor);
		this._service.Apply(pc, RequestFrom(pc.PlayerInfo!, editor)).Should().BeTrue();
	}

	private static (string Name, int Level)[] Skills(PlayerCollection pc)
		=> pc.PlayerInfo!.SkillRecordList.Select(s => (s.skillName, s.skillLevel)).ToArray();

	[Fact]
	public void Apply_UnchangedSkills_KeepsEveryByteOfTheSave()
	{
		var pc = this._fixtures.LoadPlayer();
		var original = pc.rawData!.ToArray();

		this.Apply(pc, _ => { });

		pc.rawData.Should().Equal(original);
	}

	[Fact]
	public void Apply_RaisedSkill_IsReadBackAndLoweringItAgainGivesBackTheSameSave()
	{
		var pc = this._fixtures.LoadPlayer();
		var original = pc.rawData!.ToArray();
		var before = Skills(pc);

		this.Apply(pc, e => e.SetSkillLevel(0, Onslaught, 10).Should().Be(10));
		var reread = this._fixtures.Reload(this._fixtures.PlayerProvider.Encode(pc));

		Skills(reread).Should().Equal(before.Select(s => s.Name == Onslaught.Raw ? (s.Name, 10) : s));
		reread.PlayerInfo!.SkillPoints.Should().Be(11);
		reread.PlayerInfo.Class.Should().Be("tagCClass08");

		this.Apply(pc, e => e.SetSkillLevel(0, Onslaught, 8));
		pc.rawData.Should().Equal(original);
	}

	[Fact]
	public void Apply_LastPointOfSkillTaken_RemovesItAndItsModifierFromTheSave()
	{
		var pc = this._fixtures.LoadPlayer();
		var before = Skills(pc);

		this.Apply(pc, e => e.SetSkillLevel(0, Onslaught, 0));
		var reread = this._fixtures.Reload(this._fixtures.PlayerProvider.Encode(pc));

		Skills(reread).Should().Equal(before.Where(s => s.Name != Onslaught.Raw && s.Name != IgnorePain.Raw));
		reread.PlayerInfo!.SkillPoints.Should().Be(13 + 8 + 6);
	}

	[Fact]
	public void Apply_SecondMastery_IsAddedAtTheEndAndClearsTheClass()
	{
		var pc = this._fixtures.LoadPlayer();
		var before = Skills(pc);
		var sacks = pc.Select(s => s.Count).ToArray();

		this.Apply(pc, e =>
		{
			e.SetMastery(1, Masteries.Storm).Should().BeTrue();
			e.SetSkillLevel(1, IceShard, 2);
		});
		var reread = this._fixtures.Reload(this._fixtures.PlayerProvider.Encode(pc));

		Skills(reread).Should().Equal([.. before, (StormMastery.Raw, 1), (IceShard.Raw, 2)]);
		reread.PlayerInfo!.SkillPoints.Should().Be(10);
		reread.PlayerInfo.Class.Should().BeEmpty();
		reread.Select(s => s.Count).Should().Equal(sacks, "the items after the skills are not touched");
	}

	[Fact]
	public void Apply_MasteryRemoved_GivesBackEveryPointIncludingQuestRewards()
	{
		var pc = this._fixtures.LoadPlayer();
		var info = pc.PlayerInfo!;
		// Quests gave 300 more unspent points, beyond the 3 per level the level gives.
		info.SkillPoints += 300;
		var spent = info.SkillRecordList.Sum(s => s.skillLevel);
		(info.SkillPoints + spent).Should().BeGreaterThan(PlayerLevel.GetLevelChangePoints(1, info.CurrentLevel).SkillPoints);
		// The tree of this test has the mastery (40), Onslaught (8) and Ignore Pain (6).
		var total = info.SkillPoints + 40 + 8 + 6;

		this.Apply(pc, e => e.SetMastery(0, null).Should().BeTrue());
		var reread = this._fixtures.Reload(this._fixtures.PlayerProvider.Encode(pc));

		reread.PlayerInfo!.SkillPoints.Should().Be(total);
		Skills(reread).Select(s => s.Name).Should().NotContain([WarfareMastery.Raw, Onslaught.Raw, IgnorePain.Raw]);
	}
}
