using AwesomeAssertions;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

public class GameAttributeOrderTests
{
	private static GameAttributeSlot Slot(ItemAttributesEffectType type, string effect, bool isGlobal = false, bool physicalIsBase = true)
		=> GameAttributeOrder.SlotOf($"{effect}Min", new ItemAttributesData(type, $"{effect}Min", effect, "Min", 0), isGlobal, physicalIsBase);

	private static readonly GameAttributeSlot BaseDamage = Slot(ItemAttributesEffectType.Offense, "offensivePhysical");
	private static readonly GameAttributeSlot FlatFire = Slot(ItemAttributesEffectType.Offense, "offensiveFire");
	private static readonly GameAttributeSlot FlatCold = Slot(ItemAttributesEffectType.Offense, "offensiveCold");
	private static readonly GameAttributeSlot FireBonus = Slot(ItemAttributesEffectType.OffenseModifier, "offensiveFireModifier");
	private static readonly GameAttributeSlot Strength = Slot(ItemAttributesEffectType.Character, "characterStrength");
	private static readonly GameAttributeSlot Intelligence = Slot(ItemAttributesEffectType.Character, "characterIntelligence");
	private static readonly GameAttributeSlot Dexterity = Slot(ItemAttributesEffectType.Character, "characterDexterity");
	private static readonly GameAttributeSlot Armor = Slot(ItemAttributesEffectType.Defense, "defensiveProtection");
	private static readonly GameAttributeSlot FireResistance = Slot(ItemAttributesEffectType.Defense, "defensiveFire");
	private static readonly GameAttributeSlot ElementalResistance = Slot(ItemAttributesEffectType.Defense, "defensiveElementalResistance");

	private static (string, GameAttributeSlot)[] Lines(params (string, GameAttributeSlot)[] lines) => lines;

	[Fact]
	public void Merge_ListsBasePrefixAndSuffixLinesOfOneAttributeTogether()
	{
		var merged = GameAttributeOrder.Merge(GameItemClass.Weapon
			, Lines(("base fire", FlatFire), ("base damage", BaseDamage))
			, Lines(("prefix cold", FlatCold), ("prefix fire", FlatFire))
			, Lines(("suffix fire", FlatFire)));

		merged.Should().Equal("base damage", "", "base fire", "prefix fire", "suffix fire", "prefix cold");
	}

	[Fact]
	public void Merge_Weapon_ListsBaseDamageThenBonusesThenDefenseThenCharacter()
	{
		var merged = GameAttributeOrder.Merge(GameItemClass.Weapon
			, Lines(("base damage", BaseDamage))
			, Lines(("strength", Strength), ("fire resistance", FireResistance))
			, Lines(("flat fire", FlatFire), ("fire bonus", FireBonus)));

		// The % bonuses (output 2) come before the flat damage (output 3).
		merged.Should().Equal("base damage", "", "fire bonus", "flat fire", "fire resistance", "strength");
	}

	[Fact]
	public void Merge_Armor_ListsDefenseBeforeOffense()
	{
		var merged = GameAttributeOrder.Merge(GameItemClass.Armor
			, Lines(("armor", Armor))
			, Lines(("flat fire", FlatFire))
			, Lines(("fire resistance", FireResistance)));

		merged.Should().Equal("armor", "", "fire resistance", "flat fire");
	}

	[Fact]
	public void Merge_FollowsTheGameTypeOrderInsideAStore()
	{
		var merged = GameAttributeOrder.Merge(GameItemClass.Armor
			, Lines(("fire resistance", FireResistance), ("elemental resistance", ElementalResistance))
			, Lines(("dexterity", Dexterity), ("strength", Strength))
			, Lines(("intelligence", Intelligence)));

		merged.Should().Equal("elemental resistance", "fire resistance", "strength", "intelligence", "dexterity");
	}

	[Fact]
	public void Merge_ListsRacialBonusesOfPrefixThenSuffixThenBaseAfterTheStores()
	{
		var racial = GameAttributeOrder.SlotOf("racialBonusPercentDamage", null, false, true);

		var merged = GameAttributeOrder.Merge(GameItemClass.Weapon
			, Lines(("base racial", racial), ("base damage", BaseDamage))
			, Lines(("prefix racial", racial))
			, Lines(("suffix racial", racial), ("strength", Strength)));

		merged.Should().Equal("base damage", "", "strength", "prefix racial", "suffix racial", "base racial");
	}

	[Fact]
	public void Merge_KeepsTheGroupsOfAGlobalChanceUnderTheirHeader()
	{
		var header = GameAttributeOrder.SlotOf("offensiveGlobalChance", null, false, true);
		var globalCold = Slot(ItemAttributesEffectType.Offense, "offensiveCold", isGlobal: true);
		var globalFire = Slot(ItemAttributesEffectType.Offense, "offensiveFire", isGlobal: true);

		var merged = GameAttributeOrder.Merge(GameItemClass.Weapon
			, Lines(("base header", header), ("base cold", globalCold), ("base fire", globalFire))
			, Lines(("prefix header", header), ("prefix fire", globalFire))
			, Lines(("flat fire", FlatFire)));

		// In the % output, so before the flat damage; each record keeps its own order.
		merged.Should().Equal("base header", "base cold", "base fire", "prefix header", "prefix fire", "flat fire");
	}

	[Fact]
	public void Merge_KeepsTheLinesOfAMultiLineGroupInOrder()
	{
		var granted = GameAttributeOrder.SlotOf("itemSkillName", null, false, true);

		var merged = GameAttributeOrder.Merge(GameItemClass.Other
			, Lines(("skill name", granted), ("skill text", granted), ("strength", Strength))
			, Lines()
			, Lines());

		merged.Should().Equal("strength", "skill name", "skill text");
	}

	[Theory]
	[InlineData(true, 1)]
	[InlineData(false, 3)]
	public void SlotOf_FlatPhysicalIsBaseDamageOnlyOnWeaponsAndShields(bool physicalIsBase, int output)
		=> Slot(ItemAttributesEffectType.Offense, "offensivePhysical", physicalIsBase: physicalIsBase).Output.Should().Be(output);

	[Fact]
	public void Merge_Weapon_ListsPierceRatioWithTheBaseDamageBeforeTheAttackSpeed()
	{
		var pierceRatio = Slot(ItemAttributesEffectType.Offense, "offensivePierceRatio");
		var speed = GameAttributeOrder.SlotOf("characterBaseAttackSpeedTag", null, false, true);
		var lifeLeech = Slot(ItemAttributesEffectType.OffenseSlow, "offensiveSlowLifeLeach");

		var merged = GameAttributeOrder.Merge(GameItemClass.Weapon
			, Lines(("damage", BaseDamage), ("pierce ratio", pierceRatio), ("speed", speed))
			, Lines(("life leech", lifeLeech))
			, Lines());

		merged.Should().Equal("damage", "pierce ratio", "speed", "", "life leech");
	}

	[Theory]
	[InlineData("offensiveSlowFire", 2)]
	[InlineData("offensiveSlowRunSpeed", 3)]
	public void SlotOf_DamageOverTimeGoesWithTheBonusesAndDebuffsWithTheFlatDamage(string effect, int output)
		=> Slot(ItemAttributesEffectType.OffenseSlow, effect).Output.Should().Be(output);

	[Fact]
	public void Merge_Armor_BlankLineSeparatesArmorFromAffixes()
	{
		// With or without a prefix, the bonuses are always one blank line below the armor.
		var withAffixes = GameAttributeOrder.Merge(GameItemClass.Armor
			, Lines(("armor", Armor))
			, Lines(("strength", Strength))
			, Lines(("fire resistance", FireResistance)));
		var baseOnly = GameAttributeOrder.Merge(GameItemClass.Armor
			, Lines(("armor", Armor), ("fire resistance", FireResistance))
			, Lines()
			, Lines());

		withAffixes.Should().Equal("armor", "", "fire resistance", "strength");
		baseOnly.Should().Equal("armor", "", "fire resistance");
	}

	[Fact]
	public void Merge_Armor_PrefixArmorGoesBelowTheBlankLine()
	{
		var merged = GameAttributeOrder.Merge(GameItemClass.Armor
			, Lines(("armor", Armor))
			, Lines(("prefix armor", Armor))
			, Lines(("fire resistance", FireResistance)));

		merged.Should().Equal("armor", "", "prefix armor", "fire resistance");
	}

	[Fact]
	public void Merge_Weapon_BlankLineAfterDamageAndSpeed()
	{
		var speed = GameAttributeOrder.SlotOf("characterBaseAttackSpeedTag", null, false, true);

		var merged = GameAttributeOrder.Merge(GameItemClass.Weapon
			, Lines(("base damage", BaseDamage), ("speed", speed))
			, Lines(("fire bonus", FireBonus))
			, Lines());

		merged.Should().Equal("base damage", "speed", "", "fire bonus");
	}

	[Fact]
	public void Merge_NoBlankLineWithoutBaseStatsOrWithoutBonuses()
	{
		var bonusesOnly = GameAttributeOrder.Merge(GameItemClass.Armor
			, Lines()
			, Lines(("strength", Strength))
			, Lines(("fire resistance", FireResistance)));
		var armorOnly = GameAttributeOrder.Merge(GameItemClass.Armor, Lines(("armor", Armor)), Lines(), Lines());

		bonusesOnly.Should().Equal("fire resistance", "strength");
		armorOnly.Should().Equal("armor");
	}

	[Fact]
	public void Merge_DropsTheEmptyAttackSpeedLineOfANonWeapon()
	{
		var speed = GameAttributeOrder.SlotOf("characterBaseAttackSpeedTag", null, false, true);

		var merged = GameAttributeOrder.Merge(GameItemClass.Armor
			, Lines(("armor", Armor), ("", speed), ("fire resistance", FireResistance))
			, Lines()
			, Lines());

		merged.Should().Equal("armor", "", "fire resistance");
	}

	[Fact]
	public void MergeWithRanges_EachLineKeepsItsRange_TheBlankLineHasNone()
	{
		var merged = GameAttributeOrder.MergeWithRanges(GameItemClass.Weapon
			, [("base fire", FlatFire, "(1 - 2)"), ("base damage", BaseDamage, "(10 - 20)")]
			, [("prefix fire", FlatFire, null)]
			, [("suffix strength", Strength, "(5 - 9)")]);

		merged.Should().Equal(
			("base damage", "(10 - 20)"), ("", null), ("base fire", "(1 - 2)"), ("prefix fire", null), ("suffix strength", "(5 - 9)"));
	}

	[Fact]
	public void MergeWithSources_EachLineKeepsTheListItCameFrom_RacialBonusesIncluded()
	{
		var racial = GameAttributeOrder.SlotOf("racialBonusPercentDamage", null, false, true);

		var merged = GameAttributeOrder.MergeWithSources(GameItemClass.Weapon
			, [("base racial", racial, null), ("base damage", BaseDamage, null)]
			, [("prefix racial", racial, null)]
			, [("suffix racial", racial, null), ("strength", Strength, null)]);

		// The game lists the racial bonuses prefix first, but each line still belongs to its record.
		merged.Select(l => (l.Text, l.Source)).Should().Equal(
			("base damage", 0), ("", 0), ("strength", 2), ("prefix racial", 1), ("suffix racial", 2), ("base racial", 0));
	}

	[Fact]
	public void SlotOf_UnknownAttribute_IsListedLast()
		=> GameAttributeOrder.SlotOf("someNewAttribute", null, false, true).Should().Be(GameAttributeSlot.Other);
}
