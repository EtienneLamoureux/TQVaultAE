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
		var input = CharacterPanelInput.FromSave(player, info, [], [], []);

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

	/// <summary>Gear sums giving 100 energy per item summed, recording the items of each call.</summary>
	private List<List<Item>> SetupEnergyPerItem()
	{
		var summed = new List<List<Item>>();
		this._stats.Setup(s => s.GetGearAttributes(It.IsAny<IEnumerable<Item>>(), It.IsAny<bool>()))
			.Returns((IEnumerable<Item> items, bool _) =>
			{
				var list = items.ToList();
				summed.Add(list);
				var sum = new AttributeSum();
				sum.Add("characterMana", 100 * list.Count);
				return sum;
			});
		return summed;
	}

	private static CharacterPanelInput SavedHero(SackCollection sack, IReadOnlyList<Item> worn)
	{
		var info = new PlayerInfo(new PathIO()) { BaseHealth = 300, BaseMana = 300, BaseStrength = 50, BaseIntelligence = 60, BaseDexterity = 70 };
		var player = new PlayerCollection("HeroA", @"C:\saves\_HeroA\Player.chr") { PlayerInfo = info, EquipmentSack = sack };
		return CharacterPanelInput.FromSave(player, info, worn, [], []);
	}

	[Fact]
	public void Build_FromSave_CountsTheWornItems_NotTheItemInHandStillInItsSlot()
	{
		// Arrange: an amulet picked up from its slot stays in the equipment sack until it is placed elsewhere
		var summed = this.SetupEnergyPerItem();
		var ring = new Item();
		var amulet = new Item();
		var sack = new SackCollection { SackType = SackType.Equipment };
		sack.AddItem(ring);
		sack.AddItem(amulet);

		// Act
		var view = this._builder.Build(SavedHero(sack, [ring]));

		// Assert
		summed.Should().ContainSingle().Which.Should().Equal(ring);
		view.Attributes[CharacterPanelInput.Energy].Value.Should().Be("400 / 400");
	}

	[Fact]
	public void Build_EquipThenUnequip_EnergyReturnsToTheBaseValue()
	{
		// Arrange
		this.SetupEnergyPerItem();
		var amulet = new Item();
		var sack = new SackCollection { SackType = SackType.Equipment };

		// Act: the same builder (the panel keeps it) shows the character bare, with the amulet, then bare again
		var bare = this._builder.Build(SavedHero(sack, [])).Attributes[CharacterPanelInput.Energy].Value;
		var worn = this._builder.Build(SavedHero(sack, [amulet])).Attributes[CharacterPanelInput.Energy].Value;
		var unequipped = this._builder.Build(SavedHero(sack, [])).Attributes[CharacterPanelInput.Energy].Value;

		// Assert
		(bare, worn, unequipped).Should().Be(("300 / 300", "400 / 400", "300 / 300"));
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

	#region Rollovers

	[Fact]
	public void Build_AttributeRollover_IsOneSentenceWithoutTitle_TellingTheClicksOnlyWhenEditable()
	{
		var editable = this._builder.Build(Input(editable: true)).Attributes[CharacterPanelInput.Strength].ToolTip;
		var readOnly = this._builder.Build(Input(editable: false)).Attributes[CharacterPanelInput.Strength].ToolTip;

		editable.Blocks.Should().ContainSingle().Which.Title.Should().BeNull();
		editable.Blocks[0].Lines.Should().ContainSingle().Which.Should().StartWith("Base 50, +0 ").And.Contain("Shift");
		readOnly.Blocks.Should().ContainSingle().Which.Lines.Should().ContainSingle().Which.Should().NotContain("Shift");
	}

	[Fact]
	public void StatsView_RowsWithoutRollover_HaveNone()
	{
		var stats = this._builder.StatsView(Input());

		stats.Armor.ToolTip.IsEmpty.Should().BeTrue();
		stats.Tab2Rows.Should().OnlyContain(r => r.ToolTip.IsEmpty);
		stats.Offense[0].ToolTip.Blocks.Should().ContainSingle().Which.Title.Should().BeNull();
		stats.Offense.Skip(1).Should().OnlyContain(r => r.ToolTip.IsEmpty);
	}

	[Fact]
	public void StatsView_ResistanceRollover_IsItsNameAsTitle()
	{
		var fire = this._builder.StatsView(Input()).Resistances[(int)StatResistance.Fire].ToolTip;

		fire.Blocks.Should().ContainSingle().Which.Should().BeEquivalentTo(new StatToolTipBlock("Fire Resistance", []));
		fire.Text.Should().Be("Fire Resistance");
	}

	[Fact]
	public void StatsView_ElementalDamageRollover_HasTheDamageBlockThenTheDamageOverTimeBlock()
	{
		// Fire, the sixth damage icon: its direct damage, then its burn "with +n% Improved Duration"
		var fire = this._builder.StatsView(Input()).Damages[5].ToolTip;

		fire.Blocks.Select(b => b.Title).Should().Equal("Fire", "Fire");
		fire.Blocks[0].Lines[0].Should().EndWith("%");
		fire.Blocks[1].Lines[0].Should().Contain("Improved Duration");
	}

	[Fact]
	public void StatsView_DamageWithoutOverTimePart_HasOneBlock_AndOverTimeOnlyDamageHasItsNameAsTitle()
	{
		var damages = this._builder.StatsView(Input()).Damages;

		damages[0].ToolTip.Blocks.Should().ContainSingle().Which.Title.Should().Be("Physical");
		damages[2].ToolTip.Blocks.Should().ContainSingle().Which.Title.Should().Be("Bleeding");
		damages[2].ToolTip.Blocks[0].Lines[0].Should().Contain("Improved Duration");
	}

	[Fact]
	public void StatsView_LifeLeechRollover_ShowsEnergyLeechAsASecondBlock()
	{
		var leech = this._builder.StatsView(Input()).Damages[9].ToolTip;

		leech.Blocks.Select(b => b.Title).Should().Equal("LifeLeech", "Energy Leech");
	}

	[Fact]
	public void StatsView_PetAndRaceRollovers_AreOneTitledBlock()
	{
		var stats = this._builder.StatsView(Input());

		var pet = stats.Damages[4].ToolTip.Blocks.Should().ContainSingle().Subject;
		pet.Title.Should().Be("Pet");
		pet.Lines.Should().HaveCount(6);
		var beastmen = stats.Races[0].ToolTip.Blocks.Should().ContainSingle().Subject;
		beastmen.Title.Should().Be("Beastmen");
		beastmen.Lines.Should().HaveCount(3).And.OnlyContain(l => l.Contains("Beastmen"));
	}

	[Fact]
	public void StatToolTip_Text_PutsATitleOverItsLines_AndABlankLineBetweenBlocks()
	{
		var nl = Environment.NewLine;
		var tip = new StatToolTip([new StatToolTipBlock("Cold", ["+10%", "*1.20"]), new StatToolTipBlock("Frostburn", ["+5%"])]);

		tip.Text.Should().Be($"Cold{nl}+10%{nl}*1.20{nl}{nl}Frostburn{nl}+5%");
		StatToolTip.Sentence("Base 50.").Text.Should().Be("Base 50.");
	}

	[Fact]
	public void StatToolTip_EmptySentence_IsNone()
	{
		StatToolTip.Sentence(string.Empty).Should().BeSameAs(StatToolTip.None);
		StatToolTip.None.IsEmpty.Should().BeTrue();
		StatToolTip.None.Text.Should().BeEmpty();
	}

	#endregion
}
