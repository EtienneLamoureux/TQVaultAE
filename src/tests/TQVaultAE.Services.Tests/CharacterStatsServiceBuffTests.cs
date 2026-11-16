using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Character;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>The sources of the Buffs tab: one sum each, their sections and their state at the start.</summary>
public class CharacterStatsServiceBuffTests
{
	private static readonly RecordId Mastery = """Records\Skills\Defensive\DefensiveMastery.dbr""".ToRecordId();
	private static readonly RecordId Passive = """Records\Skills\Defensive\Toughness.dbr""".ToRecordId();
	private static readonly RecordId PassiveModifier = """Records\Skills\Defensive\Toughness_Mod.dbr""".ToRecordId();
	private static readonly RecordId Aura = """Records\Skills\Defensive\Aura.dbr""".ToRecordId();
	private static readonly RecordId AuraBuff = """Records\Skills\Defensive\Aura_Buff.dbr""".ToRecordId();
	private static readonly RecordId PetAura = """Records\Skills\Nature\Wolf_PetSkill_Pack.dbr""".ToRecordId();
	private static readonly RecordId PetAuraBuff = """Records\Skills\Nature\Wolf_PetSkill_PackBuff.dbr""".ToRecordId();
	private static readonly RecordId Wolf = """Records\Skills\Nature\Pet\Wolf_01.dbr""".ToRecordId();

	private static readonly RecordId CurseApplier1 = """records\item\heraclesway\heraclesshrinebuffer01.dbr""".ToRecordId();
	private static readonly RecordId CurseApplier2 = """records\item\heraclesway\heraclesshrinebuffer02.dbr""".ToRecordId();
	private static readonly RecordId Curse1 = """records\item\heraclesway\buffs\heraclesdebuff01.dbr""".ToRecordId();
	private static readonly RecordId Curse2 = """records\item\heraclesway\buffs\heraclesdebuff02.dbr""".ToRecordId();
	private static readonly RecordId ShrineNormal = """records\skills\shrines\shrine_battle.dbr""".ToRecordId();
	private static readonly RecordId ShrineEpic = """records\skills\shrines\shrine_battle_epic.dbr""".ToRecordId();
	private static readonly RecordId BuffNormal = """records\skills\shrines\shrine_battlebuff.dbr""".ToRecordId();
	private static readonly RecordId BuffEpic = """records\skills\shrines\shrine_battlebuff_epic.dbr""".ToRecordId();
	private static readonly RecordId Tartarus = """records\xpack3\tartarus\tartarusbuffs\playerdebuffs\abilitydrained.dbr""".ToRecordId();
	private static readonly RecordId TartarusEnemy = """records\xpack3\tartarus\tartarusbuffs\enemybuffs\stronger.dbr""".ToRecordId();
	private static readonly RecordId Scroll = """records\xpack\item\scrolls\03_frenzy.dbr""".ToRecordId();
	private static readonly RecordId ScrollSkill = """records\xpack\skills\scroll skills\03_frenzy.dbr""".ToRecordId();
	private static readonly RecordId ScrollBuff = """records\xpack\skills\scroll skills\03_frenzybuff.dbr""".ToRecordId();

	private static readonly MasteryTree Tree = new(Masteries.Defensive, Mastery, "tagDefense", "", "", 40,
	[
		new MasterySkill(Passive, "", "", "", 10, 1, null),
		new MasterySkill(PassiveModifier, "", "", "", 10, 2, Passive),
		new MasterySkill(Aura, "", "", "", 10, 1, null),
		new MasterySkill(PetAura, "", "", "", 10, 1, null),
	]);

	private readonly Mock<IDatabase> _database = new();
	private readonly Mock<IItemProvider> _items = new();
	private readonly ArzFile _arz = new("database.arz");
	private readonly CharacterStatsService _service;

	public CharacterStatsServiceBuffTests()
	{
		Record(Mastery, "Skill_Mastery");
		Record(Passive, "Skill_Passive").Set(Text("skillDisplayName", "tagToughness"));
		Record(PassiveModifier, "Skill_Modifier");
		Record(Aura, "Skill_BuffRadiusToggled").Set(Text("buffSkillName", AuraBuff.Raw));
		Record(AuraBuff, "SkillBuff_Passive").Set(Text("skillUpBitmapName", """InGameUI\aura.tex"""));
		Record(PetAura, "Skill_BuffRadius").Set(Text("buffSkillName", PetAuraBuff.Raw));
		Record(PetAuraBuff, "SkillBuff_Passive");
		Record(Wolf, "Pet").Set(Text("skillName3", PetAura.Raw));

		Applier(CurseApplier1, Curse1, -1);
		Applier(CurseApplier2, Curse2, -1);
		Record(Curse1, "SkillBuff_Passive").Set(Text("skillDisplayName", "x4tagShrine01On"));
		Record(Curse2, "SkillBuff_Passive");
		Applier(ShrineNormal, BuffNormal, 60);
		Applier(ShrineEpic, BuffEpic, 60);
		Record(BuffNormal, "SkillBuff_Passive").Set(Text("skillDisplayName", "tagBattleShrine"));
		Record(BuffEpic, "SkillBuff_Passive").Set(Text("skillDisplayName", "tagBattleShrine"));
		Record(Tartarus, "SkillBuff_Passive");
		Record(TartarusEnemy, "SkillBuff_Passive");
		Record(Scroll, "OneShot_Scroll").Set(Text("skillName", ScrollSkill.Raw));
		Record(ScrollSkill, "Skill_BuffSelfDuration").Set(Text("buffSkillName", ScrollBuff.Raw));
		Record(ScrollBuff, "SkillBuff_Passive");
		this._database.Setup(d => d.ArzFile).Returns(this._arz);

		// Each record's sum tells which record was read and at which level.
		this._items
			.Setup(p => p.GetRecordAttributes(It.IsAny<RecordId?>(), It.IsAny<int>()))
			.Returns((RecordId? id, int level) =>
			{
				var sum = new AttributeSum();
				sum.Add(id!.Raw, level);
				return sum;
			});
		this._items.Setup(p => p.GetAttributeSums(It.IsAny<Item>())).Returns((Item item) =>
		{
			var sum = new AttributeSum();
			sum.Add(item.ItemClass, 1);
			return sum;
		});
		this._items.Setup(p => p.GetGrantedSkillLevels(It.IsAny<Item>())).Returns([]);
		this._items.Setup(p => p.GetSetBonusSources(It.IsAny<IEnumerable<Item>>())).Returns([]);

		this._service = new CharacterStatsService(this._database.Object, this._items.Object, Mock.Of<ILogger<CharacterStatsService>>(), Mock.Of<IItemModService>());
	}

	private DBRecordCollection Record(RecordId id, string recordClass)
	{
		var record = new DBRecordCollection(id, recordClass);
		this._database.Setup(d => d.GetRecordFromFile(id)).Returns(record);
		this._arz.RecordInfo[id] = new ArzRecordInfo(0, id.Raw, recordClass, 0, 0, default);
		return record;
	}

	private void Applier(RecordId id, RecordId buff, float duration)
	{
		var record = Record(id, "FixedItemSkill_Buff");
		record.Set(Text("buffSkillName", buff.Raw));
		record.Set(new Variable("skillActiveDuration", VariableDataType.Float, 1) { [0] = duration });
	}

	private static Variable Text(string name, string value) => new(name, VariableDataType.StringVar, 1) { [0] = value };

	private static SkillRecord Skill(RecordId id, int level) => new() { skillName = id.Raw, skillLevel = level, skillEnabled = 1 };

	private static Item Gear(string itemClass, int row, int x = Item.WeaponSlotIndicator)
	{
		var record = new DBRecordCollection($"""records\item\{itemClass}{x}{row}.dbr""".ToRecordId(), itemClass);
		record.Set(Text("Class", itemClass));
		return new Item { PositionX = x, PositionY = row, baseItemInfo = new Info(record), BaseItemId = record.Id };
	}

	private IReadOnlyList<BuffSource> Sources(IReadOnlyList<Item>? equipment = null, IReadOnlyList<RecordId>? saved = null, int difficulty = 0, RecordId? scroll = null)
		=> this._service.GetBuffSources(new BuffSourceRequest(equipment ?? [], false,
			[Skill(Mastery, 10), Skill(Passive, 3), Skill(PassiveModifier, 2), Skill(Aura, 4), Skill(PetAura, 5)],
			[], [Tree], saved ?? [], difficulty, scroll));

	[Fact]
	public void GetBuffSources_Skills_PassivesOnWithTheirModifier_BuffsAndPetAurasOff()
	{
		var sources = this.Sources();

		var passive = sources.Single(s => s.Id == "skill:" + Passive.Normalized);
		passive.Group.Should().Be(BuffGroup.Passive);
		passive.DefaultOn.Should().BeTrue();
		passive.NameTag.Should().Be("tagToughness");
		passive.Sum.Values.Should().BeEquivalentTo(new Dictionary<string, double> { [Passive.Raw] = 3, [PassiveModifier.Raw] = 2 },
			"a modifier adds to the skill it modifies and toggles with it");
		sources.Should().NotContain(s => s.Id == "skill:" + PassiveModifier.Normalized);

		var aura = sources.Single(s => s.Id == "skill:" + Aura.Normalized);
		aura.Group.Should().Be(BuffGroup.Active);
		aura.DefaultOn.Should().BeFalse();
		aura.Icon.Should().Be("""InGameUI\aura.tex""", "a toggled aura without icon of its own shows its buff's");
		aura.Sum[AuraBuff.Raw].Should().Be(4);

		var pet = sources.Single(s => s.Id == "skill:" + PetAura.Normalized);
		pet.Group.Should().Be(BuffGroup.Pet, "a pet casts it");
		pet.DefaultOn.Should().BeFalse();
		sources.Single(s => s.Id == "skill:" + Mastery.Normalized).Group.Should().Be(BuffGroup.Passive);
	}

	[Fact]
	public void GetBuffSources_Curses_AllOffered_TheSavedOnesOn()
	{
		var curses = this.Sources(saved: [Curse2]).Where(s => s.Group == BuffGroup.Curse).ToList();

		curses.Select(c => (c.RecordId, c.DefaultOn)).Should().BeEquivalentTo([(Curse1, false), (Curse2, true)]);
		curses[0].NameTag.Should().Be("x4tagShrine01On");
		curses[1].Sum[Curse2.Raw].Should().Be(1);
	}

	[Fact]
	public void GetBuffSources_SavedBuffThatIsNoCurse_ListedOnWithTheCurses()
	{
		var other = """records\mod\buffs\permanent.dbr""".ToRecordId();
		Record(other, "SkillBuff_Passive");

		var curses = this.Sources(saved: [other]).Where(s => s.Group == BuffGroup.Curse).ToList();

		curses.Should().Contain(s => s.RecordId == other && s.DefaultOn);
		curses.Should().HaveCount(3);
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, true)]
	[InlineData(2, true)]
	public void GetBuffSources_Shrines_OnePerBuff_TheDifficultysVariant(int difficulty, bool epic)
	{
		var shrines = this.Sources(difficulty: difficulty).Where(s => s.Group == BuffGroup.Shrine).ToList();

		shrines.Should().ContainSingle("both variants are one buff by their name").Which.RecordId.Should().Be(epic ? BuffEpic : BuffNormal,
			"Legendary has no variant of its own here: the closest one");
		shrines[0].DefaultOn.Should().BeFalse();
	}

	[Fact]
	public void GetBuffSources_Tartarus_OnlyTheEffectsOnThePlayer()
		=> this.Sources().Where(s => s.Group == BuffGroup.Tartarus).Select(s => s.RecordId).Should().Equal(Tartarus);

	[Fact]
	public void GetBuffSources_Scroll_OnlyThePickedOne_ItsBuffOff()
	{
		this.Sources().Should().NotContain(s => s.Group == BuffGroup.Scroll);

		var scroll = this.Sources(scroll: Scroll).Single(s => s.Group == BuffGroup.Scroll);

		scroll.DefaultOn.Should().BeFalse();
		scroll.Sum[ScrollBuff.Raw].Should().Be(1, "a scroll's buff is read at level 1");
	}

	[Fact]
	public void GetBuffScrolls_ListsTheScrollsWhoseSkillBuffsTheCharacter()
	{
		var attack = """records\xpack\item\scrolls\02_blast.dbr""".ToRecordId();
		var attackSkill = """records\xpack\skills\scroll skills\02_blast.dbr""".ToRecordId();
		Record(attack, "OneShot_Scroll").Set(Text("skillName", attackSkill.Raw));
		Record(attackSkill, "Skill_AttackRadius");

		this._service.GetBuffScrolls().Select(s => s.Id).Should().Equal(Scroll);
	}

	[Fact]
	public void GetBuffSources_Gear_OneOnSourcePerItem_WeaponsShieldsAndSetsApart()
	{
		var sword = Gear("WeaponMelee_Sword", 0);
		var shield = Gear("WeaponArmor_Shield", 1);
		var spare = Gear("WeaponMelee_Axe", 2);
		var ring = Gear("ArmorJewelry_Ring", 0, x: 3);
		var empty = new Item { PositionX = Item.WeaponSlotIndicator, PositionY = 1 };
		var setId = """records\item\sets\set01.dbr""".ToRecordId();
		var setSum = new AttributeSum();
		setSum.Add("set", 1);
		this._items.Setup(p => p.GetSetBonusSources(It.IsAny<IEnumerable<Item>>())).Returns([new SetBonus(setId, "tagSet01", [ring], setSum)]);

		var gear = this.Sources([sword, shield, spare, ring, empty]).Where(s => s.Group == BuffGroup.Gear).ToList();

		gear.Select(g => (g.Item?.ItemClass, g.GearRole)).Should().Equal(
			("WeaponMelee_Sword", BuffGearRole.Weapon), ("WeaponArmor_Shield", BuffGearRole.Shield),
			("ArmorJewelry_Ring", BuffGearRole.Other), (null, BuffGearRole.Other));
		gear.Should().OnlyContain(g => g.DefaultOn && g.IsGear);
		gear[^1].NameTag.Should().Be("tagSet01");
		gear[^1].Origins.Should().Equal(ring);
	}

	[Fact]
	public void GetOffenseGear_SameAsTheGearSourcesAllOn()
	{
		var sword = Gear("WeaponMelee_Sword", 0);
		var ring = Gear("ArmorJewelry_Ring", 0, x: 3);

		var offense = this._service.GetOffenseGear([sword, ring], useAlternate: false);
		var fromSources = BuffSums.Offense(this.Sources([sword, ring]), _ => true);

		offense.Weapons.Should().HaveCount(1);
		offense.Weapons[0].Values.Should().BeEquivalentTo(fromSources.Weapons[0].Values);
		offense.Other.Values.Should().BeEquivalentTo(fromSources.Other.Values);
	}

	[Fact]
	public void GetBuffSources_GrantedSkills_OneSourcePerSkill_WithEveryItemGrantingIt()
	{
		var armor = Gear("ArmorProtective_UpperBody", 0, x: 3);
		var ring = Gear("ArmorJewelry_Ring", 0, x: 4);
		this._items.Setup(p => p.GetGrantedSkillLevels(armor)).Returns([(Passive, 2), (Aura, 4)]);
		this._items.Setup(p => p.GetGrantedSkillLevels(ring)).Returns([(Passive, 5)]);

		var granted = this.Sources([armor, ring]).Where(s => s.Id.StartsWith("granted:", StringComparison.Ordinal)).ToList();

		granted.Single(s => s.RecordId == Passive).Should().Match<BuffSource>(s => s.Level == 5 && s.Group == BuffGroup.Passive && s.DefaultOn && s.Origins.Count == 2);
		granted.Single(s => s.RecordId == Aura).Should().Match<BuffSource>(s => s.Group == BuffGroup.Active && !s.DefaultOn);
	}
}
