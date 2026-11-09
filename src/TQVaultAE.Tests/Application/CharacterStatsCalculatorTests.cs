using AwesomeAssertions;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Application;

public class CharacterStatsCalculatorTests
{
	/// <summary>The values of records\xpack\game\gameengine.dbr and PlayerGameBalanceAttributes.dbr.</summary>
	private static readonly GameBalance Balance = new()
	{
		Engine = new Dictionary<string, IReadOnlyList<double>>(StringComparer.OrdinalIgnoreCase)
		{
			["playerDefenseCap"] = [80],
			["playerCooldownReductionCap"] = [80],
			["playerDodgePercentCap"] = [80],
			["playerDodgeProjectilePercentCap"] = [80],
			["playerRunSpeedCapMin"] = [50],
			["playerRunSpeedCapMax"] = [166],
			["playerSpellCastSpeedCapMin"] = [50],
			["playerSpellCastSpeedCapMax"] = [300],
		},
		Penalties = new Dictionary<string, IReadOnlyList<double>>(StringComparer.OrdinalIgnoreCase)
		{
			["defensiveFire"] = [0, 0, 0, 0, 0, 0, -40, -40, -40, -40, -40, -40, -100, -100, -100, -100, -100, -100],
			["defensivePhysical"] = [0],
		},
	};

	private static AttributeSum Sum(params (string Name, double Value)[] values)
	{
		var sum = new AttributeSum();
		foreach (var (name, value) in values)
			sum.Add(name, value);
		return sum;
	}

	private static CharacterStats Compute(AttributeSum gear, AttributeSum? skills = null, int difficulty = 0, int dexterity = 50, int intelligence = 50)
		=> CharacterStatsCalculator.Compute(new CharacterStatsInput(50, dexterity, intelligence, gear, skills ?? new AttributeSum(), difficulty, Balance));

	[Theory]
	[InlineData(0, 108)]
	[InlineData(1, 60)]
	[InlineData(2, -12)]
	public void Resistance_FlatAndElementalPlusPenalty_ScaledByItsModifier(int difficulty, int expected)
	{
		// (60 fire + 30 elemental + penalty) × 1.2, capped at 80: Normal 90 × 1.2 = 108 → 80.
		var gear = Sum(("defensiveFire", 60), ("defensiveElementalResistance", 30));
		var skills = Sum(("defensiveFireModifier", 20));

		var raw = CharacterStatsCalculator.Resistance(StatResistance.Fire, Merge(gear, skills), Balance, difficulty);
		var shown = Compute(gear, skills, difficulty).Resistances[StatResistance.Fire];

		raw.Should().Be(Math.Min(80, expected));
		shown.Should().Be(Math.Min(80, expected));
	}

	[Fact]
	public void Resistance_ElementalResistance_DoesNotAddToPoisonOrPhysical()
	{
		var stats = Compute(Sum(("defensiveElementalResistance", 25), ("defensivePhysical", 10)), difficulty: 2);

		stats.Resistances[StatResistance.Cold].Should().Be(25, "the cold penalty is not in this pak");
		stats.Resistances[StatResistance.Poison].Should().Be(0);
		stats.Resistances[StatResistance.Physical].Should().Be(10);
	}

	[Fact]
	public void Resistance_SecondaryOnes_ReadTheirFields()
	{
		var stats = Compute(Sum(("defensiveTotalSpeedResistance", 30), ("defensiveSlowManaLeach", 15), ("defensiveStun", 40), ("defensiveStunModifier", 50)));

		stats.Resistances[StatResistance.Slow].Should().Be(30);
		stats.Resistances[StatResistance.Spirit].Should().Be(15);
		stats.Resistances[StatResistance.Stun].Should().Be(60);
	}

	[Theory]
	[InlineData(20, 420)]
	[InlineData(-30, 350)]
	public void CombinedArmor_ItemsAndSkills_ScaledOnlyByAPositiveModifier(double modifier, int expected)
	{
		var gear = Sum(("defensiveProtection", 300), ("defensiveProtectionModifier", modifier));
		var skills = Sum(("defensiveProtection", 50));

		CharacterStatsCalculator.CombinedArmor(gear, skills).Should().Be(expected);
	}

	[Fact]
	public void Regeneration_UsesTheEquationsWithTheTotalsAndIntelligence()
	{
		// Life: (2 + 3) × 1.5; energy: (2 + 1) + 300 / 100.
		var stats = Compute(Sum(("characterLifeRegen", 3), ("characterLifeRegenModifier", 50), ("characterManaRegen", 1)), intelligence: 300);

		stats.LifeRegen.Should().Be(7.5);
		stats.ManaRegen.Should().Be(6);
	}

	[Fact]
	public void SkillReductions_ScaledByTheirModifier_AndCapped()
	{
		var stats = Compute(Sum(("skillCooldownReduction", 60), ("skillCooldownReductionModifier", 50), ("skillManaCostReduction", 40)));

		stats.CooldownReduction.Should().Be(80, "60 × 1.5 = 90, capped at playerCooldownReductionCap");
		stats.ManaCostReduction.Should().Be(40);
	}

	[Fact]
	public void Speeds_AddTheirModifierAndTheTotalSpeed_BetweenTheCaps()
	{
		var stats = Compute(Sum(("characterRunSpeedModifier", 20), ("characterTotalSpeedModifier", 10), ("characterSpellCastSpeedModifier", 400)));

		stats.RunSpeed.Should().Be(130);
		stats.CastSpeed.Should().Be(300, "510 % is capped at playerSpellCastSpeedCapMax");
		Compute(Sum(("characterRunSpeedModifier", -80))).RunSpeed.Should().Be(50, "playerRunSpeedCapMin");
	}

	[Fact]
	public void Abilities_UseTheEquationWithTheTotalDexterity()
	{
		// (100 + 120 dexterity) × (1 + 10 / 100); dexterity = (100 + 20) × 1.
		var stats = Compute(Sum(("characterOffensiveAbility", 100), ("characterOffensiveAbilityModifier", 10), ("characterDexterity", 20), ("characterDefensiveAbility", 30)), dexterity: 100);

		stats.OffensiveAbility.Should().Be(242);
		stats.DefensiveAbility.Should().Be(150);
	}

	[Fact]
	public void DodgeAndDeflect_AreCapped()
	{
		var stats = Compute(Sum(("characterDodgePercent", 95), ("characterDeflectProjectile", 12)));

		stats.Dodge.Should().Be(80);
		stats.DeflectProjectile.Should().Be(12);
	}

	[Theory]
	[InlineData(0, 10, 0)]
	[InlineData(20, 5, 25)]
	[InlineData(90, 30, 100)]
	public void Block_ShieldChancePlusModifiers_NothingWithoutShield(double shield, double modifier, int expected)
		=> Compute(Sum(("defensiveBlockChance", shield)), Sum(("defensiveBlockModifier", modifier))).Block.Should().Be(expected);

	[Fact]
	public void DamageBonuses_AddTheTotalDamage_AndTheElementalOnlyToElements()
	{
		var stats = Compute(Sum(("offensiveTotalDamageModifier", 10), ("offensiveElementalModifier", 5), ("offensiveFireModifier", 30),
			("offensiveSlowFireModifier", 20), ("offensiveSlowFireDurationModifier", 50), ("offensivePierceModifier", 15)));

		stats.DamageBonuses[StatDamage.Fire].Should().Be(new DamageBonus(45, 35, 50));
		stats.DamageBonuses[StatDamage.Pierce].Should().Be(new DamageBonus(25, 0, 0));
		stats.DamageBonuses.Should().NotContainKey(StatDamage.Pet);
	}

	[Fact]
	public void LifeLeech_IsTheAttackDamageConvertedToHealth()
		=> Compute(Sum(("offensiveLifeLeechMin", 12))).AttackDamageToHealth.Should().Be(12);

	[Fact]
	public void RaceBonuses_SingularAndPluralNames_GoToTheSameIcon()
	{
		var gear = new AttributeSum();
		gear.AddRace("Beastman", new RacialBonus(10, 5, 0));
		gear.AddRace("Beastmen", new RacialBonus(0, 15, 8));
		gear.AddRace("Titan", new RacialBonus(0, 50, 0));

		var stats = Compute(gear);

		stats.RaceBonuses[StatRace.Beastmen].Should().Be(new RacialBonus(10, 20, 8));
		stats.RaceBonuses.Values.Sum(r => r.PercentDamage).Should().Be(20, "Titan has no icon");
	}

	[Theory]
	[InlineData(2.5, 3)]
	[InlineData(2.49, 2)]
	[InlineData(-0.5, 0)]
	[InlineData(-12.6, -13)]
	public void Round_AddsAHalfThenTakesTheIntegerBelow(double value, int expected)
		=> CharacterStatsCalculator.Round(value).Should().Be(expected);

	private static AttributeSum Merge(AttributeSum a, AttributeSum b)
	{
		var sum = new AttributeSum();
		sum.Add(a);
		sum.Add(b);
		return sum;
	}
}

public class GameEquationTests
{
	[Fact]
	public void Evaluate_ReplacesVariables_CaseInsensitive_MissingOnesCountZero()
		=> GameEquation.Evaluate("(manaRegen + (intelligence / 100)) * elapsedTime + bonusDV",
			new Dictionary<string, double> { ["MANAREGEN"] = 2, ["intelligence"] = 150, ["elapsedTime"] = 1 })
			.Should().Be(3.5);

	[Fact]
	public void Evaluate_IntegerValues_DivideAsDecimals()
		=> GameEquation.Evaluate("a / b", new Dictionary<string, double> { ["a"] = 1, ["b"] = 4 }).Should().Be(0.25);

	[Fact]
	public void Evaluate_NegativeValue_KeepsItsSign()
		=> GameEquation.Evaluate("10 - a", new Dictionary<string, double> { ["a"] = -5 }).Should().Be(15);

	[Fact]
	public void Evaluate_PowerOperator_IsRefused()
	{
		var act = () => GameEquation.Evaluate("(100 * (0.88^x))", new Dictionary<string, double>());

		act.Should().Throw<FormatException>();
	}
}

public class GameBalanceTests
{
	[Theory]
	[InlineData(0, 0)]
	[InlineData(1, -40)]
	[InlineData(2, -100)]
	public void Penalty_TakesTheFirstBalanceLevelOfTheDifficulty(int difficulty, double expected)
	{
		var balance = new GameBalance
		{
			Penalties = new Dictionary<string, IReadOnlyList<double>>(StringComparer.OrdinalIgnoreCase)
			{
				["defensiveFire"] = [0, 0, 0, 0, 0, 0, -40, -40, -40, -40, -40, -40, -100, -100, -100, -100, -100, -100],
			},
		};

		balance.Penalty("DefensiveFire", difficulty).Should().Be(expected);
		balance.Penalty("defensivePhysical", difficulty).Should().Be(0);
	}

	[Fact]
	public void EngineValue_ArrayByDifficulty_LastValuePastTheEnd_DefaultWhenMissing()
	{
		var balance = new GameBalance
		{
			Engine = new Dictionary<string, IReadOnlyList<double>>(StringComparer.OrdinalIgnoreCase) { ["cap"] = [70, 75] },
		};

		balance.EngineValue("cap", 1).Should().Be(75);
		balance.EngineValue("cap", 2).Should().Be(75);
		balance.EngineValue("other", 0).Should().Be(80);
	}
}
