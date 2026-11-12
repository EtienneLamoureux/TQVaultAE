using AwesomeAssertions;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
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

	/// <summary>Ice Shard (tier 1, max 12) and its modifier Velocity (tier 3: mastery 10).</summary>
	private static readonly MasteryTree Storm = new(Masteries.Storm, StormMastery, "tagStorm", "", "", 40,
	[
		new MasterySkill(IceShard, "tagIceShard", "", "", 12, 1, null),
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

	private List<SkillTipLine> Build(CharacterSkillEditor skills, MasterySkill? skill, Func<double>? cooldown = null)
		=> this._builder.Build(skills, 0, skill, [], cooldown ?? (() => 0), difficulty: 0);

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

	[Fact]
	public void EmptySlot_HasNoToolTip()
		=> this._builder.Build(Editor(masteryLevel: 10, iceShard: 1, points: 5), 1, null, [], () => 0, 0).Should().BeEmpty();
}
