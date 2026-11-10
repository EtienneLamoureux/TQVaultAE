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

	private const double Unarmed = 1.2667;
	private const double OneHanded = 0.8333;

	[Fact]
	public void AttackSpeed_Unarmed_IsAHundred()
		=> CharacterOffenseCalculator.AttackSpeed(Sum(), WeaponStance.Unarmed, Balance, 0, Unarmed, Unarmed).Should().BeApproximately(100, 1e-9);

	[Fact]
	public void AttackSpeed_Sword_AddsItsBaseSpeed_AndTheAnimationRatio()
	{
		// (0.74 + 0.4) / 0.74 × (1.2667 / 0.8333) × 100.
		var speed = CharacterOffenseCalculator.AttackSpeed(Sum(("characterBaseAttackSpeed", 0.4)), WeaponStance.OneHanded, Balance, 0, Unarmed, OneHanded);

		speed.Should().BeApproximately(1.14 / 0.74 * (Unarmed / OneHanded) * 100, 1e-6);
	}

	[Fact]
	public void AttackSpeed_Modifiers_ScaleTheCharacterPartOnly()
	{
		// 0.74 × 1.3 + 0.4, same animation.
		var speed = CharacterOffenseCalculator.AttackSpeed(
			Sum(("characterBaseAttackSpeed", 0.4), ("characterAttackSpeedModifier", 20), ("characterTotalSpeedModifier", 10)),
			WeaponStance.OneHanded, Balance, 0, 1, 1);

		speed.Should().BeApproximately((0.74 * 1.3 + 0.4) / 0.74 * 100, 1e-6);
	}

	[Fact]
	public void AttackSpeed_DualWield_HalvesTheWeaponsBaseSpeed()
		=> CharacterOffenseCalculator.AttackSpeed(Sum(("characterBaseAttackSpeed", 0.8)), WeaponStance.DualWield, Balance, 0, 1, 1)
			.Should().BeApproximately((0.74 + 0.4) / 0.74 * 100, 1e-6);

	[Fact]
	public void AttackSpeed_IsCappedInAttacksPerSecond_BeforeTheAnimationRatio()
	{
		var speed = CharacterOffenseCalculator.AttackSpeed(Sum(("characterAttackSpeedModifier", 500)), WeaponStance.Unarmed, Balance, 0, 2, 1);

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
