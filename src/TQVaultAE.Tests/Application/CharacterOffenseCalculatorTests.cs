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

	[Fact]
	public void AttackSpeed_StanceWithoutAnimation_KeepsTheRatioAtOne()
		=> CharacterOffenseCalculator.AttackSpeed(Sum(("characterBaseAttackSpeed", 0.37)), WeaponStance.Thrown, Balance, 0, Unarmed, 0)
			.Should().BeApproximately(150, 1e-6);
}
