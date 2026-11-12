using AwesomeAssertions;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Presentation;

/// <summary>
/// <see cref="CharacterPanelViewBuilder"/>: what the statistics panel of the game's character window shows, built from
/// the state the character editor or the main window gives.
/// </summary>
public class CharacterPanelViewBuilderTests
{
	private readonly Mock<ICharacterStatsService> _stats = new();
	private readonly Mock<ITranslationService> _translation = new();
	private readonly CharacterPanelViewBuilder _builder;

	public CharacterPanelViewBuilderTests()
	{
		// A character without equipment or skill bonus, and no game text: the builder falls back on its own texts.
		this._stats.Setup(s => s.GetGearAttributes(It.IsAny<IEnumerable<Item>>(), It.IsAny<bool>())).Returns(() => new AttributeSum());
		this._stats.Setup(s => s.GetSkillAttributes(It.IsAny<IEnumerable<SkillRecord>>(), It.IsAny<IReadOnlyList<SkillAugment>>(), It.IsAny<IReadOnlyList<MasteryTree>>(), It.IsAny<bool>()))
			.Returns(() => new AttributeSum());
		this._stats.Setup(s => s.GetItemSkillAttributes(It.IsAny<IEnumerable<Item>>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(() => new AttributeSum());
		this._stats.Setup(s => s.GetGameBalance()).Returns(new GameBalance());
		this._stats.Setup(s => s.GetOffenseGear(It.IsAny<IEnumerable<Item>>(), It.IsAny<bool>())).Returns(new OffenseGear([], new AttributeSum()));
		this._stats.Setup(s => s.GetWeaponStance(It.IsAny<IEnumerable<Item>>(), It.IsAny<bool>())).Returns(WeaponStance.Unarmed);
		this._stats.Setup(s => s.GetAttackAnimationDurations(It.IsAny<bool>())).Returns(new Dictionary<WeaponStance, double>());

		this._builder = new CharacterPanelViewBuilder(this._stats.Object, this._translation.Object);
	}

	private static CharacterPanelInput Input(int level = 30, long xp = 1000, int points = 0, bool editable = true, bool canUndo = false, bool classCleared = false)
	{
		var info = new PlayerInfo(new PathIO()) { CurrentLevel = 12, Class = "tagCClassName001" };
		return new CharacterPanelInput
		{
			Player = new PlayerCollection("Hero", @"C:\saves\_Hero\Player.chr") { PlayerInfo = info },
			Info = info,
			Attributes = [300, 300, 50, 60, 70],
			AttributePoints = points,
			Level = level,
			Xp = xp,
			Skills = [],
			Trees = [],
			Augments = [],
			Editable = editable,
			CanUndo = canUndo,
			ClassCleared = classCleared,
		};
	}

	[Fact]
	public void Build_ShowsTheLevelAndXpGiven_NotTheSavedOnes()
	{
		// Arrange: the editor changed the level of a character saved at level 12

		// Act
		var view = this._builder.Build(Input(level: 30, xp: 1000));

		// Assert
		view.LevelText.Should().Be("Level : 30");
		view.XpText.Should().Contain("1");
	}

	[Fact]
	public void Build_ListsTheAttributesInTheGameOrderWithTheirBaseValue()
	{
		var view = this._builder.Build(Input());

		view.Attributes.Should().HaveCount(5);
		view.Attributes.Select(a => a.Value).Should().Equal("300 / 300", "300 / 300", "50", "60", "70");
	}

	[Theory]
	[InlineData(3, true, true)]
	[InlineData(0, true, false)]
	[InlineData(3, false, false)]
	public void Build_OffersToSpendPoints_OnlyWhenEditableWithPointsLeft(int points, bool editable, bool expected)
	{
		var view = this._builder.Build(Input(points: points, editable: editable));

		view.CanRaise.Should().Be(expected);
		(view.PointsText.Length > 0).Should().Be(points > 0);
	}

	[Theory]
	[InlineData(true, true, true)]
	[InlineData(true, false, false)]
	[InlineData(false, true, false)]
	public void Build_OffersUndo_OnlyWhenEditableAndAnAttributeChanged(bool editable, bool changed, bool expected)
	{
		this._builder.Build(Input(editable: editable, canUndo: changed)).CanUndo.Should().Be(expected);
	}

	[Fact]
	public void Build_MasteriesChanged_ShowsNoClass()
	{
		this._translation.Setup(t => t.TranslateXTag("tagCClassName001", true, true)).Returns("Warrior");

		this._builder.Build(Input()).ClassName.Should().Be("Warrior");
		this._builder.Build(Input(classCleared: true)).ClassName.Should().BeEmpty();
	}

	[Fact]
	public void FromSave_TakesTheSavedValues_InTheGameOrder()
	{
		// Arrange
		var info = new PlayerInfo(new PathIO())
		{
			BaseHealth = 500, BaseMana = 400, BaseStrength = 100, BaseIntelligence = 60, BaseDexterity = 80,
			CurrentLevel = 42, CurrentXP = 12345, AttributesPoints = 2, DifficultyUnlocked = 3,
		};
		var player = new PlayerCollection("Hero", @"C:\saves\_Hero\Player.chr") { PlayerInfo = info };

		// Act
		var input = CharacterPanelInput.FromSave(player, info, [], []);

		// Assert
		input.Attributes[CharacterPanelInput.Health].Should().Be(500);
		input.Attributes[CharacterPanelInput.Energy].Should().Be(400);
		input.Attributes[CharacterPanelInput.Strength].Should().Be(100);
		input.Attributes[CharacterPanelInput.Intelligence].Should().Be(60);
		input.Attributes[CharacterPanelInput.Dexterity].Should().Be(80);
		(input.Level, input.Xp, input.AttributePoints, input.Difficulty).Should().Be((42, 12345L, 2, 2));
		input.Editable.Should().BeFalse();
	}

	[Fact]
	public void ComputeStats_SameEquipmentAndSkills_ReadsTheGearOnce()
	{
		// Arrange: the panel asks for the view at every paint
		var input = Input();

		// Act
		this._builder.ComputeStats(input);
		this._builder.ComputeStats(input);

		// Assert
		this._stats.Verify(s => s.GetGearAttributes(It.IsAny<IEnumerable<Item>>(), It.IsAny<bool>()), Times.Once);
		this._stats.Verify(s => s.GetSkillAttributes(It.IsAny<IEnumerable<SkillRecord>>(), It.IsAny<IReadOnlyList<SkillAugment>>(), It.IsAny<IReadOnlyList<MasteryTree>>(), It.IsAny<bool>()), Times.Once);
	}

	[Fact]
	public void ComputeStats_SkillsTheItemsGrant_AddToTheSkillSum()
	{
		// Arrange: a learned skill gives 10 strength, a skill an item grants 4 more
		AttributeSum Strength(double value)
		{
			var sum = new AttributeSum();
			sum.Add("characterStrength", value);
			return sum;
		}

		this._stats.Setup(s => s.GetSkillAttributes(It.IsAny<IEnumerable<SkillRecord>>(), It.IsAny<IReadOnlyList<SkillAugment>>(), It.IsAny<IReadOnlyList<MasteryTree>>(), It.IsAny<bool>()))
			.Returns(() => Strength(10));
		this._stats.Setup(s => s.GetItemSkillAttributes(It.IsAny<IEnumerable<Item>>(), It.IsAny<bool>(), false)).Returns(() => Strength(4));

		// Act
		this._builder.ComputeStats(Input());

		// Assert
		this._builder.SkillSum!["characterStrength"].Should().Be(14);
	}
}
