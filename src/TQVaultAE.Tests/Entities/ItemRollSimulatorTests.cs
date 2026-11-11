using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Jitter;

namespace TQVaultAE.Tests.Entities;

/// <summary>
/// Tests the replay of a new item's rolls with small explicit roll tables,
/// independent of the generated <see cref="EquipmentRollTable"/> data.
/// </summary>
public class ItemRollSimulatorTests
{
	// Minimal Hallowed Circlet table in engine order: Char store, then Defense store.
	private static readonly RollAttribute[] HallowedTable = [
		new(RollStore.Character, 10, 0f, RollScope.All, [new("characterLifeRegen")]),
		new(RollStore.Character, 15, 20f, RollScope.All, [new("characterManaRegenModifier")]),
		new(RollStore.Defense, 1, 20f, RollScope.AffixOnlyArmor, [new("defensiveProtection")]),
		new(RollStore.Defense, 2, 20f, RollScope.All, [new("defensiveLife")]),
	];

	private static DBRecordCollection Record(string id, params (string Name, VariableDataType Type, object Value)[] vars)
	{
		var record = new DBRecordCollection(RecordId.Create(id), "test");
		foreach (var (name, type, value) in vars) {
			var v = new Variable(name, type, 1);
			v[0] = value;
			record.Set(v);
		}
		return record;
	}

	private static DBRecordCollection CircletBase()
		=> Record("C16_Circlet03.dbr",
			("characterManaRegenModifier", VariableDataType.Float, 20f),
			("defensiveProtection", VariableDataType.Integer, 273));

	private static DBRecordCollection HallowedPrefix()
		=> Record("Rare_Hallowed_01.dbr",
			("characterLifeRegen", VariableDataType.Float, 1f),
			("defensiveLife", VariableDataType.Float, 50f),
			("lootRandomizerJitter", VariableDataType.Float, 20f));

	[Fact]
	public void Roll_HallowedCirclet_ReproducesInGameTooltip()
	{
		var result = ItemRollSimulator.Roll(13009, isArmor: true, CircletBase(), HallowedPrefix(), null, HallowedTable);

		result.Base!.GetSingle("characterManaRegenModifier", 0).Should().Be(21f);
		result.Base.GetInt32("defensiveProtection", 0).Should().Be(273, "base armor is never jittered");
		result.Prefix!.GetSingle("characterLifeRegen", 0).Should().Be(1f);
		result.Prefix.GetSingle("defensiveLife", 0).Should().Be(52f);
		result.Rolls.Select(r => (r.Variable, r.Rolled)).Should().Equal(("characterManaRegenModifier", 21f), ("defensiveLife", 52f));
	}

	[Fact]
	public void Roll_DoesNotMutateInputRecords()
	{
		var baseRecord = CircletBase();
		var prefix = HallowedPrefix();

		ItemRollSimulator.Roll(13009, true, baseRecord, prefix, null, HallowedTable);

		baseRecord.GetSingle("characterManaRegenModifier", 0).Should().Be(20f);
		prefix.GetSingle("defensiveLife", 0).Should().Be(50f);
	}

	[Fact]
	public void Roll_AbsentAffixValue_ConsumesNoDraw()
	{
		// Removing the prefix's life regen (a value of 0) must shift the stream: the next roll
		// then uses the draw the life regen roll consumed above.
		var prefix = Record("p.dbr",
			("defensiveLife", VariableDataType.Float, 50f),
			("lootRandomizerJitter", VariableDataType.Float, 20f));

		var withoutRegen = ItemRollSimulator.Roll(13009, true, CircletBase(), prefix, null, HallowedTable);

		var rng = TitanQuestRNG.FromItemSeed(13009);
		Span<float> mana = [20f];
		ItemJitterService.AddJitter(mana, 20f, rng);
		Span<float> life = [50f];
		ItemJitterService.AddJitter(life, 20f, rng);
		withoutRegen.Base!.GetSingle("characterManaRegenModifier", 0).Should().Be(mana[0]);
		withoutRegen.Prefix!.GetSingle("defensiveLife", 0).Should().Be(life[0]);
	}

	[Fact]
	public void Roll_AffixArmorBonus_RollsOnlyOnArmorItems()
	{
		// Affix armor bonus (defensiveProtection on the prefix) rolls at the store's 20% only for Armor items.
		var prefix = Record("p.dbr", ("defensiveProtection", VariableDataType.Integer, 100));
		RollAttribute[] table = [HallowedTable[2]];

		var armor = ItemRollSimulator.Roll(42, isArmor: true, null, prefix, null, table);
		var ring = ItemRollSimulator.Roll(42, isArmor: false, null, prefix, null, table);

		var expected = ItemJitterService.Jitter(100f, 20f, TitanQuestRNG.FromItemSeed(42));
		armor.Prefix!.GetInt32("defensiveProtection", 0).Should().Be((int)expected);
		ring.Prefix!.GetInt32("defensiveProtection", 0).Should().Be(100);
		ring.Rolls.Should().BeEmpty();
	}

	[Theory]
	[InlineData(10f, 30f)]
	[InlineData(10f, 0f)]
	[InlineData(10f, 8f)]
	public void Roll_DamagePair_RollsMinThenRange(float min, float max)
	{
		// Absolute damage is stored as (min, max - min): the second draw is the range, and there is
		// no second draw when max is absent or not above min. In-game max = rolled min + rolled range.
		RollAttribute[] table = [
			new(RollStore.Damage, 1, 20f, RollScope.All,
				[new("offensiveFireMin"), new("offensiveFireMax", RangeOf: "offensiveFireMin")], Requires: ["offensiveFireMin"]),
			new(RollStore.Character, 2, 20f, RollScope.All, [new("characterStrength")]),
		];
		var baseRecord = Record("b.dbr",
			("offensiveFireMin", VariableDataType.Float, min),
			("offensiveFireMax", VariableDataType.Float, max),
			("characterStrength", VariableDataType.Float, 50f));

		var result = ItemRollSimulator.Roll(99, false, baseRecord, null, null, table);

		var rng = TitanQuestRNG.FromItemSeed(99);
		var rolledMin = ItemJitterService.Jitter(min, 20f, rng);
		var rolledRange = max > min ? ItemJitterService.Jitter(max - min, 20f, rng) : 0f;
		Span<float> strength = [50f];
		ItemJitterService.AddJitter(strength, 20f, rng); // proves the draw count: the next roll stays in sync
		result.Base!.GetSingle("offensiveFireMin", 0).Should().Be(rolledMin);
		if (max > min)
			result.Base.GetSingle("offensiveFireMax", 0).Should().Be(rolledMin + rolledRange);
		else
			result.Base.GetSingle("offensiveFireMax", 0).Should().Be(max);
		result.Base.GetSingle("characterStrength", 0).Should().Be(strength[0]);
	}

	[Fact]
	public void Roll_DamagePairWithoutMin_HasNoInstance()
	{
		RollAttribute[] table = [
			new(RollStore.Damage, 1, 20f, RollScope.All,
				[new("offensiveFireMin"), new("offensiveFireMax", RangeOf: "offensiveFireMin")], Requires: ["offensiveFireMin"]),
		];
		var baseRecord = Record("b.dbr", ("offensiveFireMax", VariableDataType.Float, 30f));

		var result = ItemRollSimulator.Roll(99, false, baseRecord, null, null, table);

		result.Base!.GetSingle("offensiveFireMax", 0).Should().Be(30f);
		result.Rolls.Should().BeEmpty();
	}

	[Theory]
	[InlineData(82f, 25f, 102f)] // 82 + (int)20.5
	[InlineData(14f, 25f, 17f)]  // 14 + (int)3.5: not (int)(14 * 1.25) rounding
	[InlineData(-10f, 25f, -12f)] // truncation toward zero: -10 + (int)-2.5
	public void Scale_AddsTruncatedPercent(float value, float percent, float expected)
		=> ItemRollSimulator.Scale(value, percent).Should().Be(expected);

	[Fact]
	public void Roll_AttributeScalePercent_ScalesAfterJitter()
	{
		// Rings/amulets: the base item's attributeScalePercent scales every value once all stores are rolled,
		// including affix values; it never consumes RNG draws.
		RollAttribute[] table = [
			new(RollStore.Character, 1, 20f, RollScope.All, [new("characterMana")]),
			new(RollStore.Character, 2, 0f, RollScope.All, [new("characterAttackSpeed")], Scales: false),
		];
		var ring = Record("ring.dbr", ("attributeScalePercent", VariableDataType.Float, 25f));
		var suffix = Record("s.dbr",
			("characterMana", VariableDataType.Float, 86f),
			("characterAttackSpeed", VariableDataType.Float, 10f),
			("lootRandomizerJitter", VariableDataType.Float, 18f));

		var result = ItemRollSimulator.Roll(21654, false, ring, null, suffix, table);

		var rng = TitanQuestRNG.FromItemSeed(21654);
		var mana = ItemJitterService.RollCharAffix(86f, 18f, rng);
		var speed = ItemJitterService.RollCharAffix(10f, 18f, rng);
		result.Suffix!.GetSingle("characterMana", 0).Should().Be(ItemRollSimulator.Scale(mana, 25f));
		result.Suffix.GetSingle("characterAttackSpeed", 0).Should().Be(speed, "the game does not scale the speeds");
	}

	[Fact]
	public void Roll_AttributeScalePercent_ScalesDamageMinAndKeepsRange()
	{
		// The scaling applies to the damage min only: in-game max = scaled min + unscaled range.
		RollAttribute[] table = [
			new(RollStore.Damage, 1, 20f, RollScope.None,
				[new("offensiveBaseFireMin"), new("offensiveBaseFireMax", RangeOf: "offensiveBaseFireMin")]),
		];
		var weapon = Record("w.dbr",
			("attributeScalePercent", VariableDataType.Float, 50f),
			("offensiveBaseFireMin", VariableDataType.Float, 10f),
			("offensiveBaseFireMax", VariableDataType.Float, 30f));

		var result = ItemRollSimulator.Roll(1, false, weapon, null, null, table);

		result.Base!.GetSingle("offensiveBaseFireMin", 0).Should().Be(15f);
		result.Base.GetSingle("offensiveBaseFireMax", 0).Should().Be(35f);
	}

	[Fact]
	public void Ranges_OverManySeeds_ContainEveryRollAndReachBothEnds()
	{
		// Ring + suffix of essence: Char affix roll (18%, uncapped), then 25% scaling, then a base AddJitter at 20%.
		RollAttribute[] table = [
			new(RollStore.Character, 1, 0f, RollScope.All, [new("characterMana")]),
			new(RollStore.Character, 2, 20f, RollScope.All, [new("characterManaRegenModifier")]),
		];
		var ring = Record("ring.dbr",
			("attributeScalePercent", VariableDataType.Float, 25f),
			("characterManaRegenModifier", VariableDataType.Float, 12f));
		var suffix = Record("s.dbr",
			("characterMana", VariableDataType.Float, 86f),
			("lootRandomizerJitter", VariableDataType.Float, 18f));

		var expected = ItemRollSimulator.Roll(1, false, ring, null, suffix, table);
		var manaRange = expected.RangesOf(RollSource.Suffix)["characterMana"];
		var regenRange = expected.RangesOf(RollSource.Base)["characterManaRegenModifier"];

		var mana = new HashSet<float>();
		var regen = new HashSet<float>();
		for (uint seed = 1; seed <= 4000; seed++) {
			var result = ItemRollSimulator.Roll(seed, false, ring, null, suffix, table);
			mana.Add(result.Suffix!.GetSingle("characterMana", 0));
			regen.Add(result.Base!.GetSingle("characterManaRegenModifier", 0));
			result.RangesOf(RollSource.Suffix)["characterMana"].Should().Be(manaRange, "ranges do not depend on the seed");
		}

		// 86 at 18%: k = 15, offsets -14..+15 -> 72..101, scaled +25% -> 90..126.
		manaRange.Should().Be(new ValueRange(90f, 126f));
		mana.Min().Should().Be(manaRange.Min);
		mana.Max().Should().Be(manaRange.Max);
		regen.Min().Should().Be(regenRange.Min);
		regen.Max().Should().Be(regenRange.Max);
	}

	[Fact]
	public void Ranges_DamagePair_MaxRangeAddsMinAndRangeBounds()
	{
		RollAttribute[] table = [
			new(RollStore.Damage, 1, 20f, RollScope.All,
				[new("offensiveFireMin"), new("offensiveFireMax", RangeOf: "offensiveFireMin")], Requires: ["offensiveFireMin"]),
		];
		var weapon = Record("w.dbr",
			("offensiveFireMin", VariableDataType.Float, 10f),
			("offensiveFireMax", VariableDataType.Float, 30f));

		var ranges = ItemRollSimulator.Roll(1, false, weapon, null, null, table).RangesOf(RollSource.Base);

		// min 10 at 20%: k = 2 -> 9..12; range 20 at 20%: k = 4 -> 17..24; max = min + range -> 26..36.
		ranges["offensiveFireMin"].Should().Be(new ValueRange(9f, 12f));
		ranges["offensiveFireMax"].Should().Be(new ValueRange(26f, 36f));
	}

	[Fact]
	public void Ranges_StaticValue_HasNoRange()
	{
		// Base armor is never rolled: no range is reported, so the tooltip shows none.
		var result = ItemRollSimulator.Roll(13009, true, CircletBase(), HallowedPrefix(), null, HallowedTable);

		result.RangesOf(RollSource.Base).Should().NotContainKey("defensiveProtection");
		result.RangesOf(RollSource.Base).Should().ContainKey("characterManaRegenModifier");
	}

	[Fact]
	public void Roll_DamageAffixWithoutJitterPercent_IsNotRolled()
	{
		// Damage/Retaliation/Defense affixes go through AddJitter: lootRandomizerJitter 0 means no roll,
		// unlike Char/Skill affixes which always roll.
		RollAttribute[] table = [
			new(RollStore.Damage, 1, 20f, RollScope.All, [new("offensiveFireMin"), new("offensiveFireMax")]),
			new(RollStore.Character, 2, 20f, RollScope.All, [new("characterStrength")]),
		];
		var prefix = Record("p.dbr",
			("offensiveFireMin", VariableDataType.Float, 10f),
			("offensiveFireMax", VariableDataType.Float, 20f),
			("characterStrength", VariableDataType.Float, 10f));

		var result = ItemRollSimulator.Roll(7, false, null, prefix, null, table);

		result.Prefix!.GetSingle("offensiveFireMin", 0).Should().Be(10f);
		result.Prefix.GetSingle("offensiveFireMax", 0).Should().Be(20f);
		var expected = ItemJitterService.RollCharAffix(10f, 0f, TitanQuestRNG.FromItemSeed(7));
		result.Prefix.GetSingle("characterStrength", 0).Should().Be(expected);
	}
}
