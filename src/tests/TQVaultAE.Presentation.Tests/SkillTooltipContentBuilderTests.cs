using AwesomeAssertions;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

/// <summary>
/// <see cref="SkillTooltipContentBuilder"/>: the lines of the character editor's skill tool tip, as the game's skill window
/// writes them. Without game texts the builder uses its English fallbacks.
/// </summary>
public class SkillTooltipContentBuilderTests
{
	private static readonly RecordId StormMastery = """Records\Skills\Storm\StormMastery.dbr""".ToRecordId();
	private static readonly RecordId IceShard = """Records\Skills\Storm\IceShard.dbr""".ToRecordId();
	private static readonly RecordId Velocity = """Records\Skills\Storm\IceShard_Velocity.dbr""".ToRecordId();

	/// <summary>Ice Shard (tier 1, max 12, ultimate 16) and its modifier Velocity (tier 3: mastery 10).</summary>
	private static readonly MasteryTree Storm = new(Masteries.Storm, StormMastery, "tagStorm", "", "", 40,
	[
		new MasterySkill(IceShard, "tagIceShard", "", "", 12, 1, null) { UltimateLevel = 16 },
		new MasterySkill(Velocity, "tagVelocity", "", "", 8, 3, IceShard),
	]);

	private readonly Mock<ICharacterEditService> _edit = new();
	private readonly Mock<ICharacterStatsService> _stats = new();
	private readonly Mock<ITranslationService> _translation = new();
	private readonly SkillTooltipContentBuilder _builder;

	public SkillTooltipContentBuilderTests()
	{
		this._stats.Setup(s => s.GetGameBalance()).Returns(new GameBalance());
		this._builder = new SkillTooltipContentBuilder(this._edit.Object, this._stats.Object, this._translation.Object);
	}

	private static SkillRecord Skill(RecordId id, int level) => new() { skillName = id.Raw, skillLevel = level, skillEnabled = 1 };

	private static CharacterSkillEditor Editor(int masteryLevel, int iceShard, int points)
		=> new([Skill(StormMastery, masteryLevel), Skill(IceShard, iceShard)], 2, points, mastery => mastery == Masteries.Storm ? Storm : null);

	private List<SkillTipLine> Build(CharacterSkillEditor skills, MasterySkill? skill, Func<double>? cooldown = null,
		IReadOnlyList<SkillAugment>? augments = null)
		=> this._builder.Build(skills, 0, skill, augments ?? [], cooldown ?? (() => 0), difficulty: 0);

	/// <summary>Ice Shard's effects at a level: a recharge, an energy cost that does not change, a damage that grows, each with the item tool tips' leading colour.</summary>
	private void DescribeIceShard()
		=> this._edit.Setup(e => e.DescribeSkill(IceShard, It.IsAny<int>(), It.IsAny<SkillWindowContext?>()))
			.Returns((RecordId id, int level, SkillWindowContext? _) => new GrantedSkill(id, "Ice Shard", level, $"{level}", null, null, [],
				["^w{^a}2.0 Second(s) Recharge", "^w20 Energy Cost", $"^w{level * 10} Cold Damage"]));

	private static string[] Levels(List<SkillTipLine> lines)
		=> [.. lines.Where(l => l.Kind == SkillTipKind.Level).Select(l => l.Text)];

	private static string[] NextLines(List<SkillTipLine> lines)
		=> [.. lines.Where(l => l.Kind == SkillTipKind.Next).Select(l => l.Text)];

	[Fact]
	public void Skill_LevelZero_ShowsOnlyTheFirstLevel()
	{
		// Arrange
		this.DescribeIceShard();

		// Act
		var lines = this.Build(Editor(masteryLevel: 10, iceShard: 0, points: 5), Storm.Skills[0]);

		// Assert
		Levels(lines).Should().Equal("Next Level : 1");
		NextLines(lines).Should().Equal("{^a}2.0 Second(s) Recharge", "20 Energy Cost", "10 Cold Damage");
	}

	[Fact]
	public void Skill_MidLevel_ShowsTheCurrentLevel_ThenWhatTheNextLevelChanges()
	{
		// Arrange
		this.DescribeIceShard();

		// Act
		var lines = this.Build(Editor(masteryLevel: 10, iceShard: 5, points: 5), Storm.Skills[0]);

		// Assert
		Levels(lines).Should().Equal("Current Level : 5", "Next Level : 6");
		lines.Where(l => l.Kind == SkillTipKind.Text).Select(l => l.Text).Should().Contain(["{^a}2.0 Second(s) Recharge", "20 Energy Cost", "50 Cold Damage"]);
		NextLines(lines).Should().Equal("{^a}2.0 Second(s) Recharge", "60 Cold Damage");
		lines.FindIndex(l => l.Text == "Next Level : 6").Should().BeGreaterThan(lines.FindIndex(l => l.Text == "50 Cold Damage"));
	}

	[Fact]
	public void Skill_AllPointsSpent_StillShowsTheNextLevelUpToTheUltimateLevel()
	{
		// Arrange
		this.DescribeIceShard();

		// Act
		var lines = this.Build(Editor(masteryLevel: 32, iceShard: 12, points: 5), Storm.Skills[0]);

		// Assert
		Levels(lines).Should().Equal("Current Level : 12", "Next Level : 13");
		lines.Should().NotContain(l => l.Kind == SkillTipKind.CanAdd, "no point can be added past the maximum");
	}

	[Fact]
	public void Skill_LevelRaisedByEquipment_ShowsTheNextLevelAfterTheRaisedOne()
	{
		// Arrange
		this.DescribeIceShard();
		SkillAugment[] gear = [new(SkillAugmentKind.All, null, 1)];

		// Act
		var lines = this.Build(Editor(masteryLevel: 32, iceShard: 12, points: 5), Storm.Skills[0], augments: gear);

		// Assert
		Levels(lines).Should().Equal("Current Level : 13", "Next Level : 14");
		NextLines(lines).Should().Equal("{^a}2.0 Second(s) Recharge", "140 Cold Damage");
	}

	[Fact]
	public void Skill_AtTheUltimateLevel_HasNoNextLevel()
	{
		// Arrange
		this.DescribeIceShard();
		SkillAugment[] gear = [new(SkillAugmentKind.All, null, 6)];

		// Act
		var lines = this.Build(Editor(masteryLevel: 32, iceShard: 12, points: 5), Storm.Skills[0], augments: gear);

		// Assert
		Levels(lines).Should().Equal("Current Level : 16");
		lines.Should().NotContain(l => l.Kind == SkillTipKind.Next);
		lines.Should().NotContain(l => l.Kind == SkillTipKind.CanAdd);
	}

	[Fact]
	public void Mastery_ShowsItsLevels_ThenThePointToAdd_ThenTheHint()
	{
		// Arrange
		var cooldownRead = false;

		// Act
		var lines = this.Build(Editor(masteryLevel: 10, iceShard: 1, points: 5), null, () => { cooldownRead = true; return 0; });

		// Assert
		lines[0].Should().Be(new SkillTipLine("Storm", SkillTipKind.Title));
		lines.Where(l => l.Kind == SkillTipKind.Level).Select(l => l.Text).Should().Equal("Current Level : 10", "Next Level : 11");
		lines.Should().Contain(new SkillTipLine("Press to add unused skill points.", SkillTipKind.CanAdd));
		lines[^1].Kind.Should().Be(SkillTipKind.Hint);
		cooldownRead.Should().BeFalse("the mastery tool tip needs no statistics");
	}

	[Fact]
	public void Skill_MasteryTooLowAndParentWithoutPoint_ListsBothRequirements()
	{
		// Act
		var lines = this.Build(Editor(masteryLevel: 4, iceShard: 0, points: 5), Storm.Skills[1]);

		// Assert
		lines.Where(l => l.Kind == SkillTipKind.Refused).Select(l => l.Text).Should().Equal(
			"This skill requires 6 additional skill points in Storm.",
			"This skill requires the Ice Shard skill.");
		lines.Should().NotContain(l => l.Kind == SkillTipKind.CanAdd);
	}

	[Fact]
	public void Skill_NoPointLeft_SaysSo()
	{
		var lines = this.Build(Editor(masteryLevel: 10, iceShard: 3, points: 0), Storm.Skills[0]);

		lines.Should().Contain(new SkillTipLine("No skill points to add.", SkillTipKind.Refused));
		lines.Where(l => l.Kind == SkillTipKind.Level).Select(l => l.Text).Should().Equal("Current Level : 3", "Next Level : 4");
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(0, 1)]
	[InlineData(12, 0)]
	[InlineData(12, 1)]
	public void Skill_EveryKind_EndsWithTheEquipmentLines_ThenTheHint(int iceShard, int skillIndex)
	{
		// Arrange: a skill or a modifier, without a point or at its maximum, all get the same tail.
		SkillAugment[] gear = [new(SkillAugmentKind.Mastery, StormMastery, 1), new(SkillAugmentKind.All, null, 2)];

		// Act
		var lines = this.Build(Editor(masteryLevel: 32, iceShard: iceShard, points: 5), Storm.Skills[skillIndex], augments: gear);

		// Assert
		lines.TakeLast(5).Should().Equal(
			SkillTipLine.Blank,
			new SkillTipLine("+1 to all skills of Storm (equipment)", SkillTipKind.Gear),
			new SkillTipLine("+2 to all skills (equipment)", SkillTipKind.Gear),
			SkillTipLine.Blank,
			new SkillTipLine(Resources.CESkillPaneHint, SkillTipKind.Hint, Wrap: true));
	}

	/// <summary>Ice Shard with a description; its effects as <see cref="DescribeIceShard"/>.</summary>
	private MasterySkill DescribedIceShard(string description)
	{
		this.DescribeIceShard();
		this._translation.Setup(t => t.TranslateXTag("tagIceShardDesc", false, false, false)).Returns(description);
		return Storm.Skills[0] with { DescriptionTag = "tagIceShardDesc" };
	}

	[Fact]
	public void Description_LineBreak_StartsANewWrappedLine()
	{
		// Arrange: Echoes of the Ancestors' description, its yellow sentence after a line break.
		var skill = this.DescribedIceShard("Upon vanquishing enemies, blast waves deal damage around you.{^n}{^Y}Can trigger on enemy death.");

		// Act
		var lines = this.Build(Editor(masteryLevel: 10, iceShard: 0, points: 5), skill);

		// Assert
		lines.Skip(1).Take(3).Should().Equal(
			new SkillTipLine("Upon vanquishing enemies, blast waves deal damage around you.", SkillTipKind.Text, Wrap: true),
			new SkillTipLine("{^Y}Can trigger on enemy death.", SkillTipKind.Text, Wrap: true),
			SkillTipLine.Blank);
	}

	[Fact]
	public void Description_ColorGoesOnAfterALineBreak_AndTwoBreaksLeaveAnEmptyLine()
	{
		// Arrange
		var skill = this.DescribedIceShard("{^y}First line. {^n}Still yellow.{^n}{^n}{^w}White.{^n}");

		// Act
		var lines = this.Build(Editor(masteryLevel: 10, iceShard: 0, points: 5), skill);

		// Assert: the trailing break adds nothing before the blank of the next level.
		lines.Skip(1).Take(6).Should().Equal(
			new SkillTipLine("{^y}First line.", SkillTipKind.Text, Wrap: true),
			new SkillTipLine("{^Y}Still yellow.", SkillTipKind.Text, Wrap: true),
			SkillTipLine.Blank,
			new SkillTipLine("{^w}White.", SkillTipKind.Text, Wrap: true),
			SkillTipLine.Blank,
			new SkillTipLine("Next Level : 1", SkillTipKind.Level));
	}

	[Fact]
	public void Skill_NextLevelStartingWithAGroup_HasAnEmptyLineUnderNextLevel()
	{
		// Arrange: Echoes of the Ancestors at level 2, as the game shows it.
		this._edit.Setup(e => e.DescribeSkill(IceShard, It.IsAny<int>(), It.IsAny<SkillWindowContext?>()))
			.Returns((RecordId id, int level, SkillWindowContext? _) => new GrantedSkill(id, "Echoes", level, $"{level}", null, null, [],
				["^w33.0% Chance to be Used : Shen Pao", "", "^wChance for one of the following:", $"^w{16 * level + 22} Fire Damage"]));

		// Act
		var lines = this.Build(Editor(masteryLevel: 32, iceShard: 2, points: 0), Storm.Skills[0]);

		// Assert
		var current = lines.FindIndex(l => l.Text == "Current Level : 2");
		lines.Skip(current).Take(11).Select(l => (l.Text, l.Kind)).Should().Equal(
			("Current Level : 2", SkillTipKind.Level),
			("33.0% Chance to be Used : Shen Pao", SkillTipKind.Text),
			("", SkillTipKind.Blank),
			("Chance for one of the following:", SkillTipKind.Text),
			("54 Fire Damage", SkillTipKind.Text),
			("", SkillTipKind.Blank),
			("Next Level : 3", SkillTipKind.Level),
			("", SkillTipKind.Blank),
			("Chance for one of the following:", SkillTipKind.Next),
			("70 Fire Damage", SkillTipKind.Next),
			("", SkillTipKind.Blank));
	}

	[Fact]
	public void EmptySlot_HasNoToolTip()
		=> this._builder.Build(Editor(masteryLevel: 10, iceShard: 1, points: 5), 1, null, [], () => 0, 0).Should().BeEmpty();
}
