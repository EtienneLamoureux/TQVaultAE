using AwesomeAssertions;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class CharacterStatsServiceTests
{
	private static readonly RecordId DefenseMastery = """Records\Skills\Defensive\DefensiveMastery.dbr""".ToRecordId();
	private static readonly RecordId Toughness = """Records\Skills\Defensive\Toughness.dbr""".ToRecordId();
	private static readonly RecordId ToughnessModifier = """Records\Skills\Defensive\Toughness_Mod.dbr""".ToRecordId();
	private static readonly RecordId Aura = """Records\Skills\Defensive\Aura.dbr""".ToRecordId();
	private static readonly RecordId AuraBuff = """Records\Skills\Defensive\Aura_Buff.dbr""".ToRecordId();
	private static readonly RecordId AuraModifier = """Records\Skills\Defensive\Aura_Mod.dbr""".ToRecordId();
	private static readonly RecordId Attack = """Records\Skills\Defensive\Attack.dbr""".ToRecordId();

	private static readonly MasteryTree Defense = new(Masteries.Defensive, DefenseMastery, "tagDefense", "", "", 40,
	[
		new MasterySkill(Toughness, "", "", "", 10, 1, null),
		new MasterySkill(ToughnessModifier, "", "", "", 10, 2, Toughness),
		new MasterySkill(Aura, "", "", "", 10, 1, null),
		new MasterySkill(AuraModifier, "", "", "", 10, 2, Aura),
		new MasterySkill(Attack, "", "", "", 10, 1, null),
	]);

	private readonly Mock<IDatabase> _database = new();
	private readonly Mock<IItemProvider> _items = new();
	private readonly CharacterStatsService _service;

	public CharacterStatsServiceTests()
	{
		Class(DefenseMastery, "Skill_Mastery");
		Class(Toughness, "Skill_Passive");
		Class(ToughnessModifier, "Skill_Modifier");
		var aura = Class(Aura, "Skill_BuffRadiusToggled");
		var buffName = new Variable("buffSkillName", VariableDataType.StringVar, 1) { [0] = AuraBuff.Raw };
		aura.Set(buffName);
		Class(AuraModifier, "Skill_Modifier");
		Class(Attack, "Skill_AttackWeapon");

		// Each record's sum tells which record was read and at which level.
		this._items
			.Setup(p => p.GetRecordAttributes(It.IsAny<RecordId?>(), It.IsAny<int>()))
			.Returns((RecordId? id, int level) =>
			{
				var sum = new AttributeSum();
				sum.Add(id!.Raw, level);
				return sum;
			});

		this._service = new CharacterStatsService(this._database.Object, this._items.Object);
	}

	private DBRecordCollection Class(RecordId id, string recordClass)
	{
		var record = new DBRecordCollection(id, recordClass);
		this._database.Setup(d => d.GetRecordFromFile(id)).Returns(record);
		return record;
	}

	private static SkillRecord Skill(RecordId id, int level) => new() { skillName = id.Raw, skillLevel = level, skillEnabled = 1 };

	private static SkillRecord[] AllSkills =>
	[
		Skill(DefenseMastery, 20), Skill(Toughness, 4), Skill(ToughnessModifier, 2),
		Skill(Aura, 3), Skill(AuraModifier, 1), Skill(Attack, 5),
	];

	[Fact]
	public void GetSkillAttributes_WithoutBuffs_CountsMasteryPassivesAndTheirModifiers()
	{
		SkillAugment[] augments = [new(SkillAugmentKind.Mastery, DefenseMastery, 1)];

		var sum = this._service.GetSkillAttributes(AllSkills, augments, [Defense], includeBuffs: false);

		sum.Values.Should().BeEquivalentTo(new Dictionary<string, double>
		{
			[DefenseMastery.Raw] = 20,
			[Toughness.Raw] = 5,
			[ToughnessModifier.Raw] = 3,
		});
	}

	[Fact]
	public void GetSkillAttributes_WithBuffs_ReadsTheBuffRecordAndTheBuffModifiers()
	{
		var sum = this._service.GetSkillAttributes(AllSkills, [], [Defense], includeBuffs: true);

		sum[AuraBuff.Raw].Should().Be(3, "an aura's bonuses are in the buff it casts");
		sum[Aura.Raw].Should().Be(0);
		sum[AuraModifier.Raw].Should().Be(1);
		sum[Attack.Raw].Should().Be(0, "an attack's bonuses go to its hits");
	}

	[Fact]
	public void GetGameBalance_ReadsCapsPenaltiesAndEquationsFromTheRecordsTheEngineNames()
	{
		var engine = Class("""records\xpack\game\gameengine.dbr""".ToRecordId(), "");
		engine.Set(Text("defaultCombatManagerRecord", """Records\Game\CombatEquations.dbr"""));
		engine.Set(Text("playerAttributePak", """Records\Game\PlayerGameBalanceAttributes.dbr"""));
		engine.Set(new Variable("playerDefenseCap", VariableDataType.Float, 1) { [0] = 75F });
		var combat = Class("""Records\Game\CombatEquations.dbr""".ToRecordId(), "");
		combat.Set(Text("offensiveAbilityEquation", "offensiveAbilityDV * 2"));
		var pak = Class("""Records\Game\PlayerGameBalanceAttributes.dbr""".ToRecordId(), "AttributePak");
		var fire = new Variable("defensiveFire", VariableDataType.Integer, 18);
		for (int i = 0; i < 18; i++)
			fire[i] = i < 6 ? 0 : i < 12 ? -40 : -100;
		pak.Set(fire);

		var balance = this._service.GetGameBalance();

		balance.EngineValue("playerDefenseCap", 0).Should().Be(75);
		balance.Penalty("defensiveFire", 1).Should().Be(-40);
		balance.OffensiveAbilityEquation.Should().Be("offensiveAbilityDV * 2");
		balance.DefensiveAbilityEquation.Should().Contain("defensiveAbilityDV", "a missing equation keeps the game's");
		balance.BaseRunSpeed.Should().Be(1.33, "a missing player template keeps the game's values");
	}

	private static Variable Text(string name, string value) => new(name, VariableDataType.StringVar, 1) { [0] = value };

	private static byte[] Animation(int frames, int frameRate = 30)
	{
		var data = new byte[32];
		"ANM\u0002"u8.CopyTo(data);
		BitConverter.GetBytes(frames).CopyTo(data, 8);
		BitConverter.GetBytes(frameRate).CopyTo(data, 12);
		return data;
	}

	[Fact]
	public void GetAttackAnimationDurations_ReadsTheTemplateAnimationsAndSpeeds()
	{
		var template = Class("""records\xpack\creatures\pc\femalepc01.dbr""".ToRecordId(), "Player");
		template.Set(Text("unarmedAttackAnim1", """Creatures\PC\Female\ANM\A.anm"""));
		template.Set(Text("unarmedAttackAnim2", """Creatures\PC\Female\ANM\B.anm"""));
		template.Set(new Variable("unarmedAttackAnimSpeed1", VariableDataType.Float, 1) { [0] = 0.5F });
		template.Set(new Variable("unarmedAttackAnimSpeed2", VariableDataType.Float, 1) { [0] = 0.5F });
		template.Set(Text("sHandedAttackAnim1", """Creatures\PC\Female\ANM\C.anm"""));
		this._database.Setup(d => d.LoadResource("""Creatures\PC\Female\ANM\A.anm""".ToRecordId())).Returns(Animation(21));
		this._database.Setup(d => d.LoadResource("""Creatures\PC\Female\ANM\B.anm""".ToRecordId())).Returns(Animation(17));
		this._database.Setup(d => d.LoadResource("""Creatures\PC\Female\ANM\C.anm""".ToRecordId())).Returns(Animation(25));

		var durations = this._service.GetAttackAnimationDurations(female: true);

		durations[WeaponStance.Unarmed].Should().BeApproximately((1.4 + 17 / 15.0) / 2, 1e-6);
		durations[WeaponStance.OneHanded].Should().BeApproximately(25 / 30.0, 1e-6, "a missing speed is 1");
		durations.Should().NotContainKey(WeaponStance.Bow);
	}

	private static Item Weapon(string itemClass, int row, int x = Item.WeaponSlotIndicator)
	{
		var record = new DBRecordCollection($"""records\item\{itemClass}{x}{row}.dbr""".ToRecordId(), itemClass);
		record.Set(Text("Class", itemClass));
		return new Item { PositionX = x, PositionY = row, baseItemInfo = new Info(record) };
	}

	[Fact]
	public void GetOffenseGear_WeaponsInHandApart_ShieldAndOtherSetLeftOut()
	{
		var sword = Weapon("WeaponMelee_Sword", 0);
		var shield = Weapon("WeaponArmor_Shield", 1);
		var spare = Weapon("WeaponMelee_Axe", 2);
		var ring = Weapon("ArmorJewelry_Ring", 0, x: 3);
		this._items.Setup(p => p.GetAttributeSums(It.IsAny<Item>())).Returns((Item item) =>
		{
			var sum = new AttributeSum();
			sum.Add(item.ItemClass, 1);
			return sum;
		});
		this._items.Setup(p => p.GetSetBonuses(It.IsAny<IEnumerable<Item>>())).Returns(() =>
		{
			var sum = new AttributeSum();
			sum.Add("set", 1);
			return sum;
		});

		var gear = this._service.GetOffenseGear([sword, shield, spare, ring], useAlternate: false);

		gear.Weapons.Should().ContainSingle().Which["WeaponMelee_Sword"].Should().Be(1);
		gear.Other.Values.Keys.Should().BeEquivalentTo(["ArmorJewelry_Ring", "set"]);
	}

	[Fact]
	public void GetWeaponStance_OnlyTheSetInHandCounts()
	{
		Item[] equipment = [Weapon("WeaponMelee_Sword", 0), Weapon("WeaponArmor_Shield", 1), Weapon("WeaponHunting_Bow", 2)];

		this._service.GetWeaponStance(equipment, useAlternate: false).Should().Be(WeaponStance.OneHanded);
		this._service.GetWeaponStance(equipment, useAlternate: true).Should().Be(WeaponStance.Bow);
	}

	[Fact]
	public void GetSkillAttributes_SkillWithoutPoint_IsSkipped()
	{
		var sum = this._service.GetSkillAttributes([Skill(Toughness, 0)], [new(SkillAugmentKind.All, null, 3)], [Defense], includeBuffs: true);

		sum.Values.Should().BeEmpty();
	}
}
