using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
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

	[Fact]
	public void GetGameBalance_ValueTheRecordsLack_FallsBackOnTheGamesWithAWarning()
	{
		// Arrange: the combat record has the offensive equation only, and there is no player template
		var engine = Class("""records\xpack\game\gameengine.dbr""".ToRecordId(), "");
		engine.Set(Text("defaultCombatManagerRecord", """Records\Game\CombatEquations.dbr"""));
		engine.Set(Text("playerAttributePak", """Records\Game\PlayerGameBalanceAttributes.dbr"""));
		Class("""Records\Game\PlayerGameBalanceAttributes.dbr""".ToRecordId(), "AttributePak");
		var combat = Class("""Records\Game\CombatEquations.dbr""".ToRecordId(), "");
		combat.Set(Text("offensiveAbilityEquation", "offensiveAbilityDV * 2"));
		var log = new ListLogger();
		var service = new CharacterStatsService(this._database.Object, this._items.Object, log);

		// Act
		var balance = service.GetGameBalance();

		// Assert: each value taken from the original game is logged, the values read are not
		balance.DefensiveAbilityEquation.Should().Contain("defensiveAbilityDV");
		log.Warnings.Should().Contain(w => w.Contains("defensiveAbilityEquation") && w.Contains("CombatEquations.dbr"));
		log.Warnings.Should().Contain(w => w.Contains("characterRunSpeed") && w.Contains("1.33"));
		log.Warnings.Should().Contain(w => w.Contains("physicalDamageEquation"));
		log.Warnings.Should().NotContain(w => w.Contains("offensiveAbilityEquation") || w.Contains("record") && w.Contains("not found,"));
	}

	/// <summary>Keeps the warnings a service logs.</summary>
	private sealed class ListLogger : ILogger<CharacterStatsService>
	{
		public List<string> Warnings { get; } = [];

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			if (logLevel == LogLevel.Warning)
				this.Warnings.Add(formatter(state, exception));
		}
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
	public void ClearGameDataCache_ReadsTheGameBalanceAgain()
	{
		var first = this._service.GetGameBalance();
		this._service.GetGameBalance().Should().BeSameAs(first);

		this._service.ClearGameDataCache();

		this._service.GetGameBalance().Should().NotBeSameAs(first, "another map's records may differ");
	}

	[Fact]
	public void GetAttackAnimationDurations_ReadsTheAnimationTableOfTheTemplate()
	{
		var player = Class("""records\xpack\creatures\pc\femalepc01.dbr""".ToRecordId(), "Player");
		player.Set(Text("charAnimationTableName", """Records\Creature\PC\ANM\ANM_FemalePC.dbr"""));
		player.Set(Text("unarmedAttackAnim1", """Creatures\PC\Female\ANM\C.anm"""));
		var template = Class("""Records\Creature\PC\ANM\ANM_FemalePC.dbr""".ToRecordId(), "CharAnimationTable");
		template.Set(Text("unarmedAttackAnim1", """Creatures\PC\Female\ANM\A.anm"""));
		template.Set(Text("unarmedAttackAnim2", """Creatures\PC\Female\ANM\B.anm"""));
		template.Set(new Variable("unarmedAttackAnimSpeed1", VariableDataType.Float, 1) { [0] = 0.5F });
		template.Set(new Variable("unarmedAttackAnimSpeed2", VariableDataType.Float, 1) { [0] = 0.5F });
		template.Set(Text("sHandedAttackAnim1", """Creatures\PC\Female\ANM\C.anm"""));
		this._database.Setup(d => d.LoadResource("""Creatures\PC\Female\ANM\A.anm""".ToRecordId())).Returns(Animation(21));
		this._database.Setup(d => d.LoadResource("""Creatures\PC\Female\ANM\B.anm""".ToRecordId())).Returns(Animation(17));
		this._database.Setup(d => d.LoadResource("""Creatures\PC\Female\ANM\C.anm""".ToRecordId())).Returns(Animation(25));

		var durations = this._service.GetAttackAnimationDurations(female: true);

		durations[WeaponStance.Unarmed].Should().BeApproximately((21 + 17) / 2.0 / 30 / 0.5, 1e-6, "the table's animations and speeds count");
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

		var empty = new Item { PositionX = Item.WeaponSlotIndicator, PositionY = 1 }; // an empty slot saved as an item
		var gear = this._service.GetOffenseGear([sword, shield, spare, ring, empty], useAlternate: false);

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

	/// <summary>Equipment whose items grant skills: a passive, an aura, an attack, a modifier, and a passive on the spare weapon.</summary>
	private Item[] GrantingEquipment()
	{
		var armor = Weapon("ArmorProtective_UpperBody", 0, x: 3);
		var ring = Weapon("ArmorJewelry_Ring", 0, x: 4);
		var sword = Weapon("WeaponMelee_Sword", 0);
		var spare = Weapon("WeaponMelee_Axe", 2);
		var granted = new Dictionary<Item, (RecordId, int)[]>
		{
			[armor] = [(Toughness, 2), (Aura, 4)],
			[ring] = [(Toughness, 5), (Attack, 3), (ToughnessModifier, 2)],
			[sword] = [],
			[spare] = [(DefenseMastery, 7)],
		};
		this._items.Setup(p => p.GetGrantedSkillLevels(It.IsAny<Item>())).Returns((Item item) => granted[item]);
		return [armor, ring, sword, spare];
	}

	[Fact]
	public void GetItemSkillAttributes_WithoutBuffs_CountsThePassivesOnceAtTheHighestGrantedLevel()
	{
		var sum = this._service.GetItemSkillAttributes(this.GrantingEquipment(), useAlternate: false, includeBuffs: false);

		sum.Values.Should().BeEquivalentTo(new Dictionary<string, double> { [Toughness.Raw] = 5 },
			"a passive counts at the level granted, once; an attack and a modifier never; the spare weapon is not in hand");
	}

	[Fact]
	public void GetItemSkillAttributes_WithBuffs_AddsTheAurasFromTheirBuffRecord()
	{
		var sum = this._service.GetItemSkillAttributes(this.GrantingEquipment(), useAlternate: false, includeBuffs: true);

		sum[AuraBuff.Raw].Should().Be(4, "an aura's bonuses are in the buff it casts, at the granted level");
		sum[Aura.Raw].Should().Be(0);
		sum[Attack.Raw].Should().Be(0);
	}

	[Fact]
	public void GetItemSkillAttributes_SecondSetInHand_CountsItsWeaponsSkills()
	{
		var sum = this._service.GetItemSkillAttributes(this.GrantingEquipment(), useAlternate: true, includeBuffs: false);

		sum[DefenseMastery.Raw].Should().Be(7);
	}

	[Fact]
	public void GetItemSkillAttributes_ModItemOnABaseGameCharacter_ReadsTheSkillInTheItemsMap()
	{
		// A passive only the mod has, granted by a ring linked to that mod, on a character of the base game.
		var modSkill = """records\mod\skills\passive.dbr""".ToRecordId();
		string current = string.Empty;
		var itemMods = new Mock<IItemModService>();
		itemMods.Setup(m => m.Scope(It.IsAny<Item?>())).Returns((Item? item) =>
		{
			var previous = current;
			current = item?.Mod ?? string.Empty;
			return new Restore(() => current = previous);
		});
		var passive = new DBRecordCollection(modSkill, "Skill_Passive");
		this._database.Setup(d => d.GetRecordFromFile(modSkill)).Returns(() => current == "ModA" ? passive : null);
		this._items.Setup(p => p.GetRecordAttributes(modSkill, It.IsAny<int>())).Returns((RecordId? id, int level) =>
		{
			var sum = new AttributeSum();
			sum.Add(current.Length == 0 ? "readInTheBaseGame" : id!.Raw, level);
			return sum;
		});
		var ring = Weapon("ArmorJewelry_Ring", 0, x: 4);
		ring.Mod = "ModA";
		ring.ModOrigin = ModOrigin.Own;
		this._items.Setup(p => p.GetGrantedSkillLevels(ring)).Returns([(modSkill, 3)]);
		var service = new CharacterStatsService(this._database.Object, this._items.Object, itemMods: itemMods.Object);

		var result = service.GetItemSkillAttributes([ring], useAlternate: false, includeBuffs: false);

		result.Values.Should().BeEquivalentTo(new Dictionary<string, double> { [modSkill.Raw] = 3 });
		current.Should().BeEmpty("the active map is back after the reading");
	}

	private sealed class Restore(Action restore) : IDisposable
	{
		public void Dispose() => restore();
	}
}
