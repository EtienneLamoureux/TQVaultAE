using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Character;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation;
using TQVaultAE.Services;
using TQVaultAE.Tests.Shared;

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

	/// <summary>The buff sources the service gives for a request; none by default.</summary>
	private Func<BuffSourceRequest, IReadOnlyList<BuffSource>> _sources = _ => [];

	public CharacterPanelViewBuilderTests()
	{
		// A character without equipment or skill bonus, and no game text: the builder falls back on its own texts.
		this._stats.Setup(s => s.GetBuffSources(It.IsAny<BuffSourceRequest>())).Returns((BuffSourceRequest r) => this._sources(r));
		this._stats.Setup(s => s.GetBuffScrolls()).Returns([]);
		this._stats.Setup(s => s.GetGameBalance()).Returns(new GameBalance());
		this._stats.Setup(s => s.GetWeaponStance(It.IsAny<IEnumerable<Item>>(), It.IsAny<bool>())).Returns(WeaponStance.Unarmed);
		this._stats.Setup(s => s.GetAttackAnimationDurations(It.IsAny<bool>())).Returns(new Dictionary<WeaponStance, double>());

		this._builder = new CharacterPanelViewBuilder(this._stats.Object, this._translation.Object);
	}

	private static CharacterPanelInput Input(int level = 30, long xp = 1000, int points = 0, bool editable = true, bool canUndo = false, bool classCleared = false,
		IReadOnlyList<MasteryTree>? trees = null)
	{
		var info = new PlayerInfo() { CurrentLevel = 12, Class = "tagCClassName001" };
		return new CharacterPanelInput
		{
			Player = new PlayerCollection("Hero", @"C:\saves\_Hero\Player.chr") { PlayerInfo = info },
			Info = info,
			Attributes = [300, 300, 50, 60, 70],
			AttributePoints = points,
			Level = level,
			Xp = xp,
			Skills = [],
			Trees = trees ?? [],
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
	public void Build_MasteriesChanged_ShowsTheClassTheNewMasteriesMake()
	{
		this._translation.Setup(t => t.TranslateXTag("tagCClassName001", true, true)).Returns("Warrior");
		this._translation.Setup(t => t.TranslateXTag("x4tagNeidanWarfare", true, true)).Returns("Daoist");
		MasteryTree Tree(Masteries mastery) => new(mastery, """records\mastery.dbr""".ToRecordId(), "", "", "", 40, []);

		this._builder.Build(Input()).ClassName.Should().Be("Warrior");
		this._builder.Build(Input(classCleared: true, trees: [Tree(Masteries.Warfare), Tree(Masteries.Neidan)])).ClassName.Should().Be("Daoist");
	}

	[Fact]
	public void Build_ModMasteriesWhoseClassASaveCleared_ShowsTheClassTheirTreesMake()
	{
		// A class set gave a mod's masteries (own records, read with the character's map) and cleared the save's class.
		this._translation.Setup(t => t.TranslateXTag("tagCClass36", true, true)).Returns("Mod Class");
		MasteryTree[] trees =
		[
			TestMasteries.ModTree(Masteries.Spirit, """records\skills\modmasteries\witchmastery.dbr"""),
			TestMasteries.ModTree(Masteries.Nature, """records\skills\modmasteries\healermastery.dbr"""),
		];
		var input = Input(trees: trees);
		input.Info.Class = string.Empty;
		input.Info.MasteryTrees = trees;
		input.Info.SkillRecordList.AddRange(trees.Select(t => new SkillRecord { skillName = t.Id.Raw, skillLevel = 32 }));

		this._builder.Build(input).ClassName.Should().Be("Mod Class");
	}

	[Fact]
	public void FromSave_TakesTheSavedValues_InTheGameOrder()
	{
		// Arrange
		var info = new PlayerInfo()
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
	public void ComputeStats_SameEquipmentAndSkills_ReadsTheSourcesOnce()
	{
		// Arrange: the panel asks for the view at every paint; a toggle changes nothing the sources are read from
		var input = Input();

		// Act
		this._builder.ComputeStats(input);
		this._builder.Build(input);
		this._builder.ComputeStats(input);

		// Assert
		this._stats.Verify(s => s.GetBuffSources(It.IsAny<BuffSourceRequest>()), Times.Once);
	}

	[Fact]
	public void Sources_ABuffOnAddingLevelsToEverySkill_ReadsTheSourcesAgainAtThoseLevels()
	{
		// Arrange: a standard whose aura gives +1 to all skills, off at first
		var requests = new List<BuffSourceRequest>();
		this._sources = r =>
		{
			requests.Add(r);
			return [Source("pet:standard", BuffGroup.Pet, false, ("allSkillEnhancement", 1))];
		};
		var input = Input();
		var standard = this._builder.Sources(input).Single();

		// Act
		input.Buffs.Set(standard, true);
		this._builder.Sources(input);
		this._builder.Sources(input);

		// Assert: read again once with every skill one level up, not more
		requests.Should().HaveCount(2);
		requests[0].Augments.Should().BeEmpty();
		requests[1].Augments.Should().ContainSingle().Which.Should().Be(new SkillAugment(SkillAugmentKind.All, null, 1));
	}

	private static BuffSource Source(string id, BuffGroup group, bool defaultOn, params (string Name, double Value)[] values)
	{
		var sum = new AttributeSum();
		foreach (var (name, value) in values)
			sum.Add(name, value);
		return new BuffSource
		{
			Id = id, Group = group, NameTag = id, DefaultOn = defaultOn, Sum = sum, Icon = group == BuffGroup.Gear ? null : "icon.tex",
			GearRole = group == BuffGroup.Gear ? BuffGearRole.Other : BuffGearRole.None,
		};
	}

	/// <summary>Gear sources giving 100 energy per item worn, recording the items of each request.</summary>
	private List<List<Item>> SetupEnergyPerItem()
	{
		var summed = new List<List<Item>>();
		this._sources = request =>
		{
			summed.Add([.. request.Equipment]);
			return [.. request.Equipment.Select((item, i) => Source($"item:{i}", BuffGroup.Gear, true, ("characterMana", 100)) with { Item = item })];
		};
		return summed;
	}

	private static CharacterPanelInput SavedHero(SackCollection sack, IReadOnlyList<Item> worn)
	{
		var info = new PlayerInfo() { BaseHealth = 300, BaseMana = 300, BaseStrength = 50, BaseIntelligence = 60, BaseDexterity = 70 };
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
	public void ComputeStats_SourcesOn_AddToTheirSum_TheOthersNot()
	{
		// Arrange: a learned passive gives 10 strength, a passive an item grants 4 more, an aura (off) 100
		this._sources = _ =>
		[
			Source("skill:passive", BuffGroup.Passive, true, ("characterStrength", 10)),
			Source("granted:passive", BuffGroup.Passive, true, ("characterStrength", 4)),
			Source("skill:aura", BuffGroup.Active, false, ("characterStrength", 100)),
		];

		// Act
		this._builder.ComputeStats(Input());

		// Assert
		this._builder.SkillSum!["characterStrength"].Should().Be(14);
		this._builder.GearSum!.Values.Should().BeEmpty();
	}

	[Fact]
	public void ToggleBuff_TurnsASourceOnOrOff_TheStatisticsFollow()
	{
		this._sources = _ => [Source("skill:aura", BuffGroup.Active, false, ("characterStrength", 100)), Source("curse:a", BuffGroup.Curse, true, ("characterStrength", -20))];
		var input = Input();
		string Strength() => this._builder.Build(input).Attributes[CharacterPanelInput.Strength].Value;

		var before = Strength();
		this._builder.ToggleBuff(input.Buffs, "skill:aura").Should().BeTrue();
		var withAura = Strength();
		this._builder.ToggleBuff(input.Buffs, "CURSE:A").Should().BeTrue("keys ignore case");
		var withoutCurse = Strength();

		(before, withAura, withoutCurse).Should().Be(("30", "130", "150"));
		this._builder.ToggleBuff(input.Buffs, "skill:unknown").Should().BeFalse();
	}

	[Fact]
	public void Sources_AskedWithTheSavedBuffsTheDifficultyAndTheScroll()
	{
		BuffSourceRequest? asked = null;
		this._sources = r =>
		{
			asked = r;
			return [];
		};
		var input = Input() with { Difficulty = 2 };
		input.Info.SavedBuffs.Add("""records\item\heraclesway\buffs\heraclesdebuff02.dbr""");
		input.Buffs.PickScroll("""records\item\scroll.dbr""");

		this._builder.ComputeStats(input);

		asked!.SavedBuffs.Should().ContainSingle();
		(asked.Difficulty, asked.Scroll?.Raw).Should().Be((2, """records\item\scroll.dbr"""));
	}

	#region Buffs tab

	[Fact]
	public void Build_BuffsTab_OneSectionPerGroupWithSources_EmptySourcesLeftOut()
	{
		this._sources = _ =>
		[
			Source("skill:passive", BuffGroup.Passive, true, ("characterStrength", 10)),
			Source("skill:nothing", BuffGroup.Passive, true),
			Source("curse:a", BuffGroup.Curse, false, ("defensiveFire", -30)),
		];

		var buffs = this._builder.Build(Input()).Buffs;

		buffs.Sections.Select(s => s.Group).Should().Equal(BuffGroup.Passive, BuffGroup.Curse);
		buffs.Sections[0].Entries.Should().ContainSingle().Which.Should().Match<BuffEntryView>(e => e.Id == "skill:passive" && e.On && e.Icon == "icon.tex");
		buffs.Sections[1].Entries[0].On.Should().BeFalse();
		buffs.Sections[1].Entries[0].ToolTip.Blocks[0].Lines[0].Should().StartWith("Off");
		buffs.Scrolls.Should().ContainSingle().Which.Id.Should().BeNull("only the empty choice when the game has no scroll");
		buffs.SelectedScroll.Should().Be(0);
	}

	[Fact]
	public void StatsView_ResistanceRollover_ListsEachSourceRawThenItsEffect_AndTheDifficulty()
	{
		// Arrange: fire +36 from an item, +10% from a passive, -30 from a curse; Epic penalty -40
		var balance = new GameBalance { Penalties = new Dictionary<string, IReadOnlyList<double>>(StringComparer.OrdinalIgnoreCase) { ["defensiveFire"] = [.. Enumerable.Repeat(0.0, GameBalance.BalanceLevelsPerDifficulty), -40] } };
		this._stats.Setup(s => s.GetGameBalance()).Returns(balance);
		this._sources = _ =>
		[
			Source("item:0", BuffGroup.Gear, true, ("defensiveFire", 36)),
			Source("skill:passive", BuffGroup.Passive, true, ("defensiveFireModifier", 10)),
			Source("curse:a", BuffGroup.Curse, true, ("defensiveFire", -30)),
			Source("skill:off", BuffGroup.Active, false, ("defensiveFire", 100)),
		];

		// Act
		var stats = this._builder.StatsView(Input() with { Difficulty = 1 });

		// Assert: (36 - 30 - 40) x 1.1 = -37.4 shown -37; without the item (-70) x 1.1 = -77
		stats.Resistances[(int)StatResistance.Fire].Value.Should().Be("-37%");
		var sources = stats.Resistances[(int)StatResistance.Fire].ToolTip.Blocks.Should().HaveCount(2).And.Subject.Last();
		sources.Title.Should().Be("Sources");
		sources.Lines.Should().Equal("item:0: +36 (+40)", "skill:passive: +10% (-3)", "curse:a: -30 (-33)", "Difficulty: -40");
	}

	#endregion

	[Fact]
	public void StatsView_AttackSpeedRollover_ShowsTheWeaponsBaseSpeed()
	{
		// A weapon's own speed (characterBaseAttackSpeed) counts in the attack speed: its source shows its raw value.
		this._stats.Setup(s => s.GetGameBalance()).Returns(new GameBalance());
		this._sources = _ => [Source("item:0", BuffGroup.Gear, true, ("characterBaseAttackSpeed", 1))];

		var stats = this._builder.StatsView(Input());

		var sources = stats.Offense[1].ToolTip.Blocks.Should().ContainSingle(b => b.Title == "Sources").Subject;
		sources.Lines.Should().ContainSingle().Which.Should().StartWith("item:0: +1 (");
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
	#region Weapon set counted

	[Theory]
	[InlineData(false, null, false)]
	[InlineData(false, true, true)]
	[InlineData(true, false, false)]
	[InlineData(true, null, true)]
	public void ComputeStats_CountsTheWeaponSetChosen_OrTheSavedOne(bool saved, bool? chosen, bool expected)
	{
		// Arrange: the save has one set in hand, the panel may count the other
		var input = Input() with { AlternateWeapons = chosen };
		input.Player.UseAlternate = saved;

		// Act
		this._builder.ComputeOffense(input);

		// Assert: the sources and the stance are those of the set counted
		input.CountsAlternateWeapons.Should().Be(expected);
		this._stats.Verify(s => s.GetBuffSources(It.Is<BuffSourceRequest>(r => r.UseAlternate == expected)), Times.Once);
		this._stats.Verify(s => s.GetWeaponStance(It.IsAny<IEnumerable<Item>>(), expected), Times.Once);
	}

	[Fact]
	public void ComputeStats_AnotherWeaponSetChosen_ReadsTheSourcesAndTheStanceAgain()
	{
		// Arrange: the view was built with the set in hand
		var input = Input();
		this._builder.ComputeOffense(input);

		// Act: the second set's gem clicked, then the first again
		this._builder.ComputeOffense(input with { AlternateWeapons = true });
		this._builder.ComputeOffense(input with { AlternateWeapons = false });

		// Assert
		this._stats.Verify(s => s.GetBuffSources(It.Is<BuffSourceRequest>(r => r.UseAlternate)), Times.Once);
		this._stats.Verify(s => s.GetBuffSources(It.Is<BuffSourceRequest>(r => !r.UseAlternate)), Times.Exactly(2));
		this._stats.Verify(s => s.GetWeaponStance(It.IsAny<IEnumerable<Item>>(), It.IsAny<bool>()), Times.Exactly(3));
	}

	[Fact]
	public void StatsView_WeaponSets_LightTheSetCounted_AndNameTheSetInHandInTheSave()
	{
		// Arrange: set II in hand in the save, set I counted
		var input = Input() with { AlternateWeapons = false };
		input.Player.UseAlternate = true;

		// Act
		var sets = this._builder.StatsView(input).WeaponSets;

		// Assert
		sets.Select(s => (s.Name, s.Counted)).Should().Equal(("I", true), ("II", false));
		var culture = System.Globalization.CultureInfo.CurrentCulture;
		var inSave = string.Format(culture, Resources.WeaponSetInSave, "II");
		sets[0].ToolTip.Blocks.Single().Lines.Should().Equal(string.Format(culture, Resources.WeaponSetCounted, "I"), inSave);
		sets[1].ToolTip.Blocks.Single().Lines.Should().Equal(string.Format(culture, Resources.WeaponSetPick, "II"), inSave);
	}

	[Fact]
	public void FromSave_WeaponSetChosen_IsCounted()
	{
		var info = new PlayerInfo();
		var player = new PlayerCollection("HeroA", @"C:\saves\_HeroA\Player.chr") { PlayerInfo = info, UseAlternate = false };

		CharacterPanelInput.FromSave(player, info, [], [], []).CountsAlternateWeapons.Should().BeFalse();
		CharacterPanelInput.FromSave(player, info, [], [], [], alternateWeapons: true).CountsAlternateWeapons.Should().BeTrue();
	}

	#endregion
}
