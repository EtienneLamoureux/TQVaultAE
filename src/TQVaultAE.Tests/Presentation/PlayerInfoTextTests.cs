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
	public void BuildLines_StartsWithTheLevelAndClass()
	{
		var lines = PlayerInfoText.BuildLines(this._info, this._translations.Object);

		lines.Keys.Take(3).Should().Equal(Resources.CurrentLevel, Resources.Class, Resources.Masteries);
	}

	[Fact]
	public void MasteryNames_JoinsTheTranslatedNamesInTheOrderOfTheMasteries()
	{
		var info = new PlayerInfo(new Mock<IPathIO>().Object);
		// Chosen Earth first: the drop down still lists Defense first.
		info.SkillRecordList.Add(new SkillRecord { skillName = Masteries.Earth.AsString(EnumFormat.Description)!, skillLevel = 1 });
		info.SkillRecordList.Add(new SkillRecord { skillName = Masteries.Defensive.AsString(EnumFormat.Description)!, skillLevel = 1 });
		var characterEdit = new Mock<ICharacterEditService>();
		characterEdit.Setup(x => x.GetMasteryDescriptionTag(It.IsAny<RecordId>()))
			.Returns((RecordId id) => id.Raw.Contains("Earth") ? "tagEarth" : "tagDefense");
		this._translations.Setup(x => x.TranslateXTag("tagEarth", false, false, false)).Returns("Earth Mastery");
		this._translations.Setup(x => x.TranslateXTag("tagDefense", false, false, false)).Returns("Defense Mastery");

		var names = PlayerInfoText.MasteryNames(info, characterEdit.Object, this._translations.Object);

		names.Should().Be("Defense Mastery + Earth Mastery");
	}

	[Fact]
	public void MasteryNames_NoMasteryOrNoSave_IsEmpty()
	{
		var characterEdit = new Mock<ICharacterEditService>();

		PlayerInfoText.MasteryNames(this._info, characterEdit.Object, this._translations.Object).Should().BeEmpty();
		PlayerInfoText.MasteryNames(null, characterEdit.Object, this._translations.Object).Should().BeEmpty();
	}
}
