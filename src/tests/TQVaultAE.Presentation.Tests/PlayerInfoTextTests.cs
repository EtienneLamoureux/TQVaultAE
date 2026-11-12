using AwesomeAssertions;
using EnumsNET;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

public class PlayerInfoTextTests
{
	private const string ClassTag = "tagCClass01";

	private readonly Mock<ITranslationService> _translations = new();

	private readonly PlayerInfo _info = new(new Mock<IPathIO>().Object)
	{
		Class = ClassTag,
		CurrentLevel = 42,
		Money = 1234,
		DifficultyUnlocked = 1,
	};

	[Fact]
	public void BuildLines_TranslatesTheClassMasteryAndDifficulty()
	{
		this._translations.Setup(x => x.TranslateXTag(ClassTag, true, true, false)).Returns("Conqueror");
		this._translations.Setup(x => x.TranslateMastery(ClassTag)).Returns("Warfare / Defense");
		this._translations.Setup(x => x.TranslateDifficulty(1)).Returns("Epic");

		var lines = PlayerInfoText.BuildLines(this._info, this._translations.Object);

		lines[Resources.Class].Should().Be("Conqueror");
		lines[Resources.Masteries].Should().Be("Warfare / Defense");
		lines[Resources.DifficultyUnlocked].Should().Be("Epic");
		lines[Resources.CurrentLevel].Should().Be("42");
		lines[Resources.Money].Should().Be("1234");
	}

	[Fact]
	public void BuildLines_MasteryNotFound_ShowsNothing()
	{
		// Without a known mastery, the translation falls back to the "Masteries" label itself.
		this._translations.Setup(x => x.TranslateMastery(ClassTag)).Returns(Resources.Masteries);

		var lines = PlayerInfoText.BuildLines(this._info, this._translations.Object);

		lines[Resources.Masteries].Should().BeEmpty();
	}

	[Fact]
	public void BuildLines_UntranslatedValues_FallBackToEmptyAndUnknown()
	{
		this._translations.Setup(x => x.TranslateXTag(It.IsAny<string>(), true, true, false)).Returns((string)null!);
		this._translations.Setup(x => x.TranslateMastery(It.IsAny<string>())).Returns((string)null!);
		this._translations.Setup(x => x.TranslateDifficulty(It.IsAny<int>())).Returns((string)null!);

		var lines = PlayerInfoText.BuildLines(this._info, this._translations.Object);

		lines[Resources.Class].Should().BeEmpty();
		lines[Resources.Masteries].Should().BeEmpty();
		lines[Resources.DifficultyUnlocked].Should().Be("unknown");
	}

	[Fact]
	public void BuildLines_WithElectrum_ShowsItRightAfterTheMoney()
	{
		this._info.Electrum = 83433;

		var lines = PlayerInfoText.BuildLines(this._info, this._translations.Object);

		lines[Resources.Electrum].Should().Be("83433");
		var keys = lines.Keys.ToList();
		keys.IndexOf(Resources.Electrum).Should().Be(keys.IndexOf(Resources.Money) + 1);
		keys.IndexOf(Resources.SkillPoints).Should().Be(keys.IndexOf(Resources.Electrum) + 1);
	}

	[Fact]
	public void BuildLines_WithoutElectrum_LeavesTheLineOutAndKeepsTheOrder()
	{
		this._info.Electrum = null;

		var lines = PlayerInfoText.BuildLines(this._info, this._translations.Object);

		lines.Should().NotContainKey(Resources.Electrum);
		var keys = lines.Keys.ToList();
		keys.IndexOf(Resources.SkillPoints).Should().Be(keys.IndexOf(Resources.Money) + 1);
		keys.Last().Should().Be(Resources.CriticalHitsReceived);
	}

	[Fact]
	public void BuildLines_StartsWithTheLevelAndClass()
	{
		var lines = PlayerInfoText.BuildLines(this._info, this._translations.Object);

		lines.Keys.Take(3).Should().Equal(Resources.CurrentLevel, Resources.Class, Resources.Masteries);
	}

	[Fact]
	public void MasteryNames_JoinsTheTranslatedNamesInThePaneOrder()
	{
		// Chosen Earth first: the drop down still lists Defense first. A mod's own mastery in the Earth pane is named
		// by its tree.
		var earth = new MasteryTree(Masteries.Earth, """records\skills\lifegiving\lifegiving_mastery.dbr""".ToRecordId(), "tagLifegiving", "", "", 40, []);
		var defense = new MasteryTree(Masteries.Defensive, Masteries.Defensive.AsString(EnumFormat.Description)!.ToRecordId(), "tagDefense", "", "", 40, []);
		this._translations.Setup(x => x.TranslateXTag("tagLifegiving", false, false, false)).Returns("Lifegiving Mastery");
		this._translations.Setup(x => x.TranslateXTag("tagDefense", false, false, false)).Returns("Defense Mastery");

		var names = PlayerInfoText.MasteryNames([earth, defense], this._translations.Object);

		names.Should().Be("Defense Mastery + Lifegiving Mastery");
	}

	[Fact]
	public void MasteryNames_NoMastery_IsEmpty()
		=> PlayerInfoText.MasteryNames([], this._translations.Object).Should().BeEmpty();

	private static readonly MasteryTree Storm = new(Masteries.Storm, """Records\Skills\Storm\StormMastery.dbr""".ToRecordId(), "tagStorm", "", "", 40, []);

	[Fact]
	public void MasterySummaryLine_Plural_TranslatesTheNameAndCountsSkillsAndPoints()
	{
		this._translations.Setup(x => x.TranslateXTag("tagStorm", true, false, false)).Returns("Storm Nexus");

		var (name, summary) = PlayerInfoText.MasterySummaryLine(new MasterySummary(Storm, 4, 20), this._translations.Object);

		name.Should().Be("Storm Nexus");
		summary.Should().Be(string.Format(Resources.CEMasterySummary,
			string.Format(Resources.CEMasterySkills, 4), string.Format(Resources.CEMasteryPoints, 20)));
	}

	[Fact]
	public void MasterySummaryLine_OneSkillOnePoint_IsSingularAndFallsBackToTheMasteryName()
	{
		var (name, summary) = PlayerInfoText.MasterySummaryLine(new MasterySummary(Storm, 1, 1), this._translations.Object);

		name.Should().Be("Storm");
		summary.Should().Be(string.Format(Resources.CEMasterySummary,
			string.Format(Resources.CEMasterySkill, 1), string.Format(Resources.CEMasteryPoint, 1)));
	}
}
