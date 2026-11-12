using AwesomeAssertions;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Application;

public class WeaponStancesTests
{
	[Theory]
	[InlineData(WeaponStance.Unarmed)]
	[InlineData(WeaponStance.OneHanded, "WeaponMelee_Sword")]
	[InlineData(WeaponStance.OneHanded, "WeaponMelee_Axe", "WeaponArmor_Shield")]
	[InlineData(WeaponStance.DualWield, "WeaponMelee_Mace", "WeaponMelee_Sword")]
	[InlineData(WeaponStance.Bow, "WeaponHunting_Bow")]
	[InlineData(WeaponStance.Staff, "WeaponMagical_Staff")]
	[InlineData(WeaponStance.Spear, "WeaponHunting_Spear", "WeaponArmor_Shield")]
	[InlineData(WeaponStance.Thrown, "WeaponHunting_RangedOneHand")]
	[InlineData(WeaponStance.DualThrown, "WeaponHunting_RangedOneHand", "WeaponHunting_RangedOneHand")]
	[InlineData(WeaponStance.Unarmed, "WeaponArmor_Shield")]
	public void FromWeapons_TheItemClassesInHand_GiveTheStance(WeaponStance expected, params string[] classes)
		=> WeaponStances.FromWeapons(classes).Should().Be(expected);

	[Fact]
	public void MeanDuration_FramesOverRateOverSpeed_Averaged()
	{
		// The male template's unarmed attacks: 21 and 17 frames at 30 fps, speed 0.5.
		var mean = WeaponStances.MeanDuration([(21, 30, 0.5), (17, 30, 0.5)]);

		mean.Should().BeApproximately((1.4 + 17 / 15.0) / 2, 1e-9);
		WeaponStances.MeanDuration([]).Should().Be(0);
	}

	[Fact]
	public void ReadAnimationHeader_ReadsFramesAndRate()
	{
		var data = new byte[64];
		"ANM\u0002"u8.CopyTo(data);
		BitConverter.GetBytes(38).CopyTo(data, 4);
		BitConverter.GetBytes(25).CopyTo(data, 8);
		BitConverter.GetBytes(30).CopyTo(data, 12);

		WeaponStances.ReadAnimationHeader(data).Should().Be((25, 30));
		WeaponStances.ReadAnimationHeader("NOPE............"u8).Should().BeNull();
	}

	[Theory]
	[InlineData(WeaponStance.Unarmed, "unarmed")]
	[InlineData(WeaponStance.OneHanded, "sHanded")]
	[InlineData(WeaponStance.DualWield, "dHanded")]
	[InlineData(WeaponStance.Thrown, "rangedOneHand")]
	[InlineData(WeaponStance.DualThrown, "dualRanged")]
	public void RecordPrefix_MatchesThePlayerTemplateFields(WeaponStance stance, string prefix)
		=> WeaponStances.RecordPrefix(stance).Should().Be(prefix);
}

public class CharacterOffenseCalculatorTests
{
	private static readonly GameBalance Balance = new()
	{
		BaseAttackSpeed = 0.74,
		Engine = new Dictionary<string, IReadOnlyList<double>>(StringComparer.OrdinalIgnoreCase)
		{
			["playerAttackSpeedCapMin"] = [50],
			["playerAttackSpeedCapMax"] = [350],
		},
	};

	private static AttributeSum Sum(params (string Name, double Value)[] values)
	{
		var sum = new AttributeSum();
		foreach (var (name, value) in values)
			sum.Add(name, value);
		return sum;
	}

	// The players' animation tables: unarmed 21 and 17 frames at 30 fps, speed 1.2; the others 25 frames, dual wield at speed 0.9.
	private const double Unarmed = 19 / 30.0 / 1.2;
	private const double OneHanded = 25 / 30.0;
	private const double DualWield = 25 / 30.0 / 0.9;

	private static double Duration(WeaponStance stance) => stance switch
	{
		WeaponStance.Unarmed => Unarmed,
		WeaponStance.DualWield => DualWield,
		_ => OneHanded,
	};

	[Fact]
	public void AttackSpeed_Unarmed_IsAHundred()
		=> CharacterOffenseCalculator.AttackSpeed(Sum(), WeaponStance.Unarmed, Balance, 0, Unarmed, Unarmed).Should().BeApproximately(100, 1e-9);

	// In the game: Copper Knife (0.5) 106 %, Copper Hand Axe (0.35) 93 %, Iron Sudis spear and a bow (0.1) 72 %.
	[Theory]
	[InlineData(WeaponStance.OneHanded, 0.5, 106)]
	[InlineData(WeaponStance.OneHanded, 0.35, 93)]
	[InlineData(WeaponStance.Spear, 0.1, 72)]
	[InlineData(WeaponStance.Bow, 0.1, 72)]
	public void AttackSpeed_Weapon_AddsItsBaseSpeed_TimesTheAnimationRatio(WeaponStance stance, double weapon, int expected)
		=> CharacterStatsCalculator.Round(CharacterOffenseCalculator.AttackSpeed(Sum(("characterBaseAttackSpeed", weapon)), stance, Balance, 0, Unarmed, Duration(stance)))
			.Should().Be(expected);

	[Fact]
	public void AttackSpeed_Modifiers_ScaleTheTemplateAndWeaponSpeeds()
	{
		// (0.74 + 0.4) × 1.3, same animation.
		var speed = CharacterOffenseCalculator.AttackSpeed(
			Sum(("characterBaseAttackSpeed", 0.4), ("characterAttackSpeedModifier", 20), ("characterTotalSpeedModifier", 10)),
			WeaponStance.OneHanded, Balance, 0, 1, 1);

		speed.Should().BeApproximately((0.74 + 0.4) * 1.3 / 0.74 * 100, 1e-6);
	}

	// In the game, a male character: Copper Shamshir (0.45) with +17 % (its own) and +16 % (Weapon Training) 135 %, the
	// Alexandrian Broadsword (0.5) with +16 % 123 %, both dual wielded with +33 % 124 %.
	[Theory]
	[InlineData(WeaponStance.OneHanded, 0.45, 33, 135)]
	[InlineData(WeaponStance.OneHanded, 0.5, 16, 123)]
	[InlineData(WeaponStance.DualWield, 0.95, 33, 124)]
	public void AttackSpeed_WithAttackSpeedBonus_MatchesTheGame(WeaponStance stance, double weapons, double modifier, int expected)
		=> CharacterStatsCalculator.Round(CharacterOffenseCalculator.AttackSpeed(
				Sum(("characterBaseAttackSpeed", weapons), ("characterAttackSpeedModifier", modifier)), stance, Balance, 0, Unarmed, Duration(stance)))
			.Should().Be(expected);

	[Fact]
	public void AttackSpeed_DualWield_HalvesTheWeaponsBaseSpeed()
		=> CharacterOffenseCalculator.AttackSpeed(Sum(("characterBaseAttackSpeed", 0.8)), WeaponStance.DualWield, Balance, 0, 1, 1)
			.Should().BeApproximately((0.74 + 0.4) / 0.74 * 100, 1e-6);

	[Fact]
	public void AttackSpeed_IsCappedInAttacksPerSecond_BeforeTheAnimationRatio()
	{
		var speed = CharacterOffenseCalculator.AttackSpeed(Sum(("characterAttackSpeedModifier", 500)), WeaponStance.OneHanded, Balance, 0, 2, 1);

		speed.Should().BeApproximately(350 * 2, 1e-6);
	}

	private static OffenseInput Input(IReadOnlyList<AttributeSum> weapons, AttributeSum? other = null, AttributeSum? skills = null,
		double strength = 0, double dexterity = 0, double intelligence = 0, double attackSpeed = 100)
	{
		var all = new AttributeSum();
		foreach (var weapon in weapons)
			all.Add(weapon);
		if (other is not null)
			all.Add(other);

		return new OffenseInput(new OffenseGear(weapons, other ?? new AttributeSum()), all, skills ?? new AttributeSum(),
			strength, dexterity, intelligence, 50, attackSpeed, Balance);
	}

	[Fact]
	public void Damage_BareHands_HitAsAWeapon()
	{
		// A character in the game: hands 2-4, +5% total damage, strength 71.5, no weapon: 3.15 × 1.143 + 2.86 = 6.46, shows 6.
		var values = CharacterOffenseCalculator.Damage(Input([], skills: Sum(("offensiveTotalDamageModifier", 5)), strength: 71.5)
			with { Balance = Balance with { HandHitDamageMin = 2, HandHitDamageMax = 4 } });

		values.Should().Be(new OffenseValues(0, 6, 6, 6));
	}

	[Fact]
	public void Damage_PierceShare_OfThePhysicalResult_GoesThroughThePierceEquation()
	{
		// A character's Iron Sudis in the game: 17-31 physical, 30 % pierce, 2-8 lightning, 99 Str, 115 Dex, 50 Int, 72 %:
		// 44, DPS 31.
		var spear = Sum(("offensivePhysicalAverage", 24), ("offensivePierceRatioAverage", 30), ("offensiveLightningAverage", 5));
		var values = CharacterOffenseCalculator.Damage(Input([spear], strength: 99, dexterity: 115, intelligence: 50, attackSpeed: 72));

		values.AverageDamage.Should().Be(44);
		values.Dps.Should().Be(31);
	}

	// In the game: a character's Copper Knife (50 Str/Dex) 10, another's (71.5 Str, 60 Dex, +5 % total damage) 11 from
	// 11.58 of the weapon and 0.42 added: each part is truncated.
	[Theory]
	[InlineData(50, 50, 0, 10)]
	[InlineData(71.5, 60, 5, 11)]
	public void Damage_EachPart_IsTruncated(double strength, double dexterity, double totalDamage, int expected)
	{
		var knife = Sum(("offensivePhysicalAverage", 6), ("offensivePierceRatioAverage", 15));

		CharacterOffenseCalculator.Damage(Input([knife], skills: Sum(("offensiveTotalDamageModifier", totalDamage)), strength: strength, dexterity: dexterity))
			.AverageDamage.Should().Be(expected);
	}

	[Fact]
	public void Damage_Staff_CountsItsBaseElementalDamage_AndEnergyLeech()
	{
		// A character's Draining Stout Staff of Fire in the game: 16 fire of the staff, +10 % fire, 6 fire, 24 energy leech
		// over 3 seconds, 259 Int, level 22, 74 %: 59, DPS 43.
		var staff = Sum(("offensiveBaseFireAverage", 16), ("offensiveFireAverage", 6), ("offensiveFireModifier", 10),
			("offensiveSlowManaLeachAverage", 8), ("offensiveSlowManaLeachDurationAverage", 3));
		var values = CharacterOffenseCalculator.Damage(Input([staff], strength: 50, dexterity: 154, intelligence: 259, attackSpeed: 74.14) with { Level = 22 });

		values.AverageDamage.Should().Be(59);
		values.Dps.Should().Be(43);
	}

	[Fact]
	public void Damage_WeaponPhysical_UsesTheWeaponEquation()
	{
		// (100 × (500/500 + 1)) + 500 × 0.04 = 220.
		var values = CharacterOffenseCalculator.Damage(Input([Sum(("offensivePhysicalAverage", 100))], strength: 500));

		values.Should().Be(new OffenseValues(220, 0, 220, 220));
	}

	[Fact]
	public void Damage_PierceRatio_MovesAShareOfThePhysicalToPierce()
	{
		// 100 physical, 40 % pierce ratio, +50 % pierce: 60 + 40 × 1.5 (no attribute: equations keep the value).
		var weapon = Sum(("offensivePhysicalAverage", 100), ("offensivePierceRatioAverage", 40), ("offensivePierceModifier", 50));

		CharacterOffenseCalculator.Damage(Input([weapon])).AverageDamage.Should().Be(120);
	}

	[Fact]
	public void Damage_Elemental_AddsItsTheElementalAndTheTotalModifiers_ThenTheIntelligenceEquation()
	{
		// 100 fire × (1 + (20 + 10 + 10) / 100) = 140; × (0/650 + 1) + 0.
		var ring = Sum(("offensiveFireAverage", 100), ("offensiveFireModifier", 20), ("offensiveElementalModifier", 10), ("offensiveTotalDamageModifier", 10));

		var values = CharacterOffenseCalculator.Damage(Input([Sum(("offensivePhysicalAverage", 10))], other: ring));

		values.AverageDamage.Should().Be(10 * 11 / 10 + 140);
		values.WeaponDamage.Should().Be(10, "the weapon part keeps only the weapon's own modifiers");
	}

	[Fact]
	public void Damage_OverTime_AddsItsPerSecondValueOnce()
	{
		// 30 burn per second, +100 % burn, intelligence 500: 60 × (500/500 + 1).
		var skills = Sum(("offensiveSlowFireAverage", 30), ("offensiveSlowFireModifier", 100), ("offensiveSlowFireDurationAverage", 5));

		var values = CharacterOffenseCalculator.Damage(Input([], skills: skills, intelligence: 500));

		values.AddedDamage.Should().Be(120 + 3, "the burn, then the hand hits (3)");
	}

	[Fact]
	public void Damage_BonusPhysical_CountsAsIs()
		=> CharacterOffenseCalculator.Damage(Input([], other: Sum(("offensiveBonusPhysicalAverage", 46), ("offensivePhysicalModifier", 100)))).AverageDamage
			.Should().Be(46 + 6, "the bonus is not scaled; the hand hits are");

	[Fact]
	public void Damage_DualWield_IsHalfOfBothHands()
	{
		var values = CharacterOffenseCalculator.Damage(Input([Sum(("offensivePhysicalAverage", 100)), Sum(("offensivePhysicalAverage", 50))]));

		values.AverageDamage.Should().Be(75);
	}

	[Fact]
	public void Damage_Dps_IsTheAverageDamageTimesTheAttackSpeed_Truncated()
		=> CharacterOffenseCalculator.Damage(Input([Sum(("offensivePhysicalAverage", 99))], attackSpeed: 234.4)).Dps.Should().Be(232);

	[Fact]
	public void AttackSpeed_StanceWithoutAnimation_KeepsTheRatioAtOne()
		=> CharacterOffenseCalculator.AttackSpeed(Sum(("characterBaseAttackSpeed", 0.37)), WeaponStance.Thrown, Balance, 0, Unarmed, 0)
			.Should().BeApproximately(150, 1e-6);
}
