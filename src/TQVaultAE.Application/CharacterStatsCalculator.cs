using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application;

/// <summary>The resistances of the character window: tab I (8) then tab II (8), in the game's order.</summary>
public enum StatResistance
{
	Fire, Cold, Lightning, Pierce, Vitality, Poison, Bleeding, Physical,
	Slow, Trap, Spirit, Disruption, Stun, Freeze, Sleep, Petrify,
}

/// <summary>The damage kinds of tab III's "Damage &amp; Duration Bonuses", in the game's order of the icons.</summary>
public enum StatDamage
{
	Physical, Pierce, Bleeding, Poison, Pet, Fire, Cold, Lightning, Vitality, LifeLeech,

	/// <summary>Energy leech: no icon of its own, the game shows it under life leech.</summary>
	ManaLeech,
}

/// <summary>The monster races of tab III's "Monster Race Bonuses", in the game's order of the icons.</summary>
public enum StatRace
{
	Beastmen, Insects, Undead, Demons, Humanity, Devices, Beasts, Plants, Ghosts, Magical, Giants, Constructs,
}

/// <summary>What the calculator needs: the attributes put by the player, the sums of gear and skills, the difficulty.</summary>
/// <param name="Strength">Strength without bonus.</param>
/// <param name="Dexterity">Dexterity without bonus.</param>
/// <param name="Intelligence">Intelligence without bonus.</param>
/// <param name="Gear">The equipped items and item sets.</param>
/// <param name="Skills">The skills that count.</param>
/// <param name="Difficulty">0 Normal, 1 Epic, 2 Legendary.</param>
/// <param name="Balance">The game's caps, penalties and equations.</param>
public sealed record CharacterStatsInput(int Strength, int Dexterity, int Intelligence, AttributeSum Gear, AttributeSum Skills, int Difficulty, GameBalance Balance);

/// <summary>A damage bonus of tab III: the damage %, and for the damage over time its damage % and duration %.</summary>
public readonly record struct DamageBonus(int Damage, int DurationDamage, int Duration);

/// <summary>
/// Tab III's Pet Bonuses: the damage % and the flat damage the pet bonus records of the gear and the skills give
/// the pets, for physical, elemental and vitality damage.
/// </summary>
public readonly record struct PetDamageBonus(int PhysicalPercent, int ElementalPercent, int VitalityPercent, int Physical, int Elemental, int Vitality);

/// <summary>The values of the character window that the calculator computes, rounded as the game shows them.</summary>
public sealed record CharacterStats
{
	public required IReadOnlyDictionary<StatResistance, int> Resistances { get; init; }
	public int CombinedArmor { get; init; }
	public double LifeRegen { get; init; }
	public double ManaRegen { get; init; }
	public int ManaCostReduction { get; init; }
	public int CooldownReduction { get; init; }
	public int CastSpeed { get; init; }
	public int RunSpeed { get; init; }
	public int OffensiveAbility { get; init; }
	public int DefensiveAbility { get; init; }
	public int Dodge { get; init; }
	public int DeflectProjectile { get; init; }
	public int Block { get; init; }
	public int AttackDamageToHealth { get; init; }
	public int PhysicalToElemental { get; init; }
	public required IReadOnlyDictionary<StatDamage, DamageBonus> DamageBonuses { get; init; }
	public required IReadOnlyDictionary<StatRace, RacialBonus> RaceBonuses { get; init; }
	public PetDamageBonus PetBonuses { get; init; }

	/// <summary>Strength with the bonuses of gear and skills, as the damage equations use it.</summary>
	public double Strength { get; init; }

	/// <summary>Dexterity with the bonuses of gear and skills.</summary>
	public double Dexterity { get; init; }

	/// <summary>Intelligence with the bonuses of gear and skills.</summary>
	public double Intelligence { get; init; }
}

/// <summary>
/// Computes the character window values from the save and the database, with the game's formulas:
/// defense values are summed per type, scaled by their % modifier, then capped; the
/// abilities and the regeneration use the game's equations.
/// </summary>
public static class CharacterStatsCalculator
{
	/// <summary>The defensive fields of each resistance: flat value, % modifier, and the elemental resistance added to fire, cold and lightning.</summary>
	private static readonly Dictionary<StatResistance, (string Flat, bool Elemental)> ResistanceFields = new()
	{
		[StatResistance.Fire] = ("defensiveFire", true),
		[StatResistance.Cold] = ("defensiveCold", true),
		[StatResistance.Lightning] = ("defensiveLightning", true),
		[StatResistance.Pierce] = ("defensivePierce", false),
		[StatResistance.Vitality] = ("defensiveLife", false),
		[StatResistance.Poison] = ("defensivePoison", false),
		[StatResistance.Bleeding] = ("defensiveBleeding", false),
		[StatResistance.Physical] = ("defensivePhysical", false),
		// The slow resistance feeds three entries (run, attack, cast) with the same value: their median is that value.
		[StatResistance.Slow] = ("defensiveTotalSpeedResistance", false),
		[StatResistance.Trap] = ("defensiveTrap", false),
		[StatResistance.Spirit] = ("defensiveSlowManaLeach", false),
		[StatResistance.Disruption] = ("defensiveDisruption", false),
		[StatResistance.Stun] = ("defensiveStun", false),
		[StatResistance.Freeze] = ("defensiveFreeze", false),
		[StatResistance.Sleep] = ("defensiveSleep", false),
		[StatResistance.Petrify] = ("defensivePetrify", false),
	};

	/// <summary>The offensive fields of each damage bonus: damage %, damage over time %, duration %; elemental kinds add the elemental %.</summary>
	private static readonly Dictionary<StatDamage, (string? Damage, string? DurationDamage, string? Duration, bool Elemental)> DamageFields = new()
	{
		[StatDamage.Physical] = ("offensivePhysicalModifier", "offensiveSlowPhysicalModifier", "offensiveSlowPhysicalDurationModifier", false),
		[StatDamage.Pierce] = ("offensivePierceModifier", null, null, false),
		[StatDamage.Bleeding] = (null, "offensiveSlowBleedingModifier", "offensiveSlowBleedingDurationModifier", false),
		[StatDamage.Poison] = (null, "offensiveSlowPoisonModifier", "offensiveSlowPoisonDurationModifier", false),
		[StatDamage.Fire] = ("offensiveFireModifier", "offensiveSlowFireModifier", "offensiveSlowFireDurationModifier", true),
		[StatDamage.Cold] = ("offensiveColdModifier", "offensiveSlowColdModifier", "offensiveSlowColdDurationModifier", true),
		[StatDamage.Lightning] = ("offensiveLightningModifier", "offensiveSlowLightningModifier", "offensiveSlowLightningDurationModifier", true),
		[StatDamage.Vitality] = ("offensiveLifeModifier", "offensiveSlowLifeModifier", "offensiveSlowLifeDurationModifier", false),
		[StatDamage.LifeLeech] = (null, "offensiveSlowLifeLeachModifier", "offensiveSlowLifeLeachDurationModifier", false),
		[StatDamage.ManaLeech] = (null, "offensiveSlowManaLeachModifier", "offensiveSlowManaLeachDurationModifier", false),
	};

	/// <summary>The race names of the records (<c>racialBonusRace</c>) each icon stands for.</summary>
	private static readonly Dictionary<StatRace, string[]> RaceNames = new()
	{
		[StatRace.Beastmen] = ["Beastman", "Beastmen"],
		[StatRace.Insects] = ["Insectoid", "Insect", "Insects"],
		[StatRace.Undead] = ["Undead"],
		[StatRace.Demons] = ["Demon", "Demons"],
		[StatRace.Humanity] = ["Humankind"],
		[StatRace.Devices] = ["Device", "Devices"],
		[StatRace.Beasts] = ["Beast", "Beasts"],
		[StatRace.Plants] = ["Plant", "Plants"],
		[StatRace.Ghosts] = ["Ghost", "Ghosts"],
		[StatRace.Magical] = ["Magical"],
		[StatRace.Giants] = ["Giant", "Giants"],
		[StatRace.Constructs] = ["Construct", "Constructs"],
	};

	public static CharacterStats Compute(CharacterStatsInput input)
	{
		ArgumentNullException.ThrowIfNull(input);
		var all = new AttributeSum();
		all.Add(input.Gear);
		all.Add(input.Skills);
		var balance = input.Balance;
		int difficulty = input.Difficulty;

		double dexterity = Total(input.Dexterity, all, "characterDexterity");
		double intelligence = Total(input.Intelligence, all, "characterIntelligence");

		return new CharacterStats
		{
			Strength = Total(input.Strength, all, "characterStrength"),
			Dexterity = dexterity,
			Intelligence = intelligence,
			Resistances = Enum.GetValues<StatResistance>().ToDictionary(r => r, r => Round(Resistance(r, all, balance, difficulty))),
			CombinedArmor = CombinedArmor(input.Gear, input.Skills),
			LifeRegen = GameEquation.Evaluate(balance.LifeRegenEquation, new Dictionary<string, double>
			{
				["lifeRegen"] = Total(balance.BaseLifeRegen, all, "characterLifeRegen"),
				["elapsedTime"] = 1,
			}),
			ManaRegen = GameEquation.Evaluate(balance.ManaRegenEquation, new Dictionary<string, double>
			{
				["manaRegen"] = Total(balance.BaseManaRegen, all, "characterManaRegen"),
				["intelligence"] = intelligence,
				["elapsedTime"] = 1,
			}),
			// The window caps the energy cost reduction at 100 (gameengine's 80 applies in play).
			ManaCostReduction = Round(Math.Min(100, Scaled(all, "skillManaCostReduction"))),
			CooldownReduction = Round(Math.Min(balance.EngineValue("playerCooldownReductionCap", difficulty), Scaled(all, "skillCooldownReduction"))),
			CastSpeed = Round(Speed(balance.BaseCastSpeed, all, "characterSpellCastSpeed",
				balance.EngineValue("playerSpellCastSpeedCapMin", difficulty, 0), balance.EngineValue("playerSpellCastSpeedCapMax", difficulty, 0))),
			RunSpeed = Round(Speed(balance.BaseRunSpeed, all, "characterRunSpeed",
				balance.EngineValue("playerRunSpeedCapMin", difficulty, 0), balance.EngineValue("playerRunSpeedCapMax", difficulty, 0))),
			OffensiveAbility = Round(Ability(balance.OffensiveAbilityEquation, "offensiveAbility", all, "characterOffensiveAbility", dexterity)),
			DefensiveAbility = Round(Ability(balance.DefensiveAbilityEquation, "defensiveAbility", all, "characterDefensiveAbility", dexterity)),
			Dodge = Round(Math.Min(balance.EngineValue("playerDodgePercentCap", difficulty), Total(0, all, "characterDodgePercent"))),
			DeflectProjectile = Round(Math.Min(balance.EngineValue("playerDodgeProjectilePercentCap", difficulty), Total(0, all, "characterDeflectProjectile"))),
			Block = Round(Block(input.Gear, all)),
			AttackDamageToHealth = Round(all["offensiveLifeLeechMin"]),
			PhysicalToElemental = 0,
			DamageBonuses = Enum.GetValues<StatDamage>().Where(DamageFields.ContainsKey).ToDictionary(d => d, d => Damage(d, all)),
			RaceBonuses = Enum.GetValues<StatRace>().ToDictionary(r => r, r => Race(r, all)),
			PetBonuses = PetBonuses(all),
		};
	}

	/// <summary>Rounded as the window shows numbers: + 0.5 then to the integer below.</summary>
	public static int Round(double value) => (int)Math.Floor(value + 0.5);

	/// <summary>A character attribute: (base + flat) × (1 + % / 100), not under 0.</summary>
	public static double Total(double baseValue, AttributeSum sum, string field)
		=> Math.Max(0, (baseValue + sum[field]) * (1 + sum[field + "Modifier"] / 100));

	/// <summary>A skill attribute: flat × (1 + % / 100).</summary>
	private static double Scaled(AttributeSum sum, string field)
		=> sum[field] * (1 + sum[field + "Modifier"] / 100);

	/// <summary>
	/// A resistance: (flat + difficulty penalty) × (1 + % / 100), capped at <c>playerDefenseCap</c>. The penalty is part
	/// of the flat sum, so the % scales it too.
	/// </summary>
	public static double Resistance(StatResistance resistance, AttributeSum sum, GameBalance balance, int difficulty)
	{
		var (flat, elemental) = ResistanceFields[resistance];
		double value = sum[flat] + (elemental ? sum["defensiveElementalResistance"] : 0) + balance.Penalty(flat, difficulty);
		value *= 1 + sum[flat + "Modifier"] / 100;
		double cap = balance.EngineValue("playerDefenseCap", difficulty);
		return cap > 0 ? Math.Min(value, cap) : value;
	}

	/// <summary>
	/// The combined armor: the protection of the items and of the skills, each part scaled by the total protection %
	/// when it is positive, truncated (tab I).
	/// </summary>
	public static int CombinedArmor(AttributeSum gear, AttributeSum skills)
	{
		double protection = gear["defensiveProtection"] + skills["defensiveProtection"];
		double modifier = gear["defensiveProtectionModifier"] + skills["defensiveProtectionModifier"];
		if (modifier > 0)
			protection *= 1 + modifier / 100;

		return (int)Math.Max(0, protection);
	}

	/// <summary>
	/// A speed in % of the player template's: (base + flat) × (1 + (% + total speed %) / 100) / base, between the caps.
	/// The total speed % is assumed to add to each speed.
	/// </summary>
	private static double Speed(double baseValue, AttributeSum sum, string field, double capMin, double capMax)
	{
		if (baseValue <= 0)
			return 100;

		double total = Math.Max(0, (baseValue + sum[field]) * (1 + (sum[field + "Modifier"] + sum["characterTotalSpeedModifier"]) / 100));
		double percent = 100 * total / baseValue;
		if (capMax > 0)
			percent = Math.Min(percent, capMax);

		return Math.Max(percent, capMin);
	}

	/// <summary>
	/// Offensive or defensive ability: the designer equation with the ability, its modifier (a separate value) and the
	/// total dexterity, no bonus, not under 0.
	/// </summary>
	private static double Ability(string equation, string variable, AttributeSum sum, string field, double dexterity)
		=> Math.Max(0, GameEquation.Evaluate(equation, new Dictionary<string, double>
		{
			[variable + "DV"] = sum[field],
			[variable + "ModifierDV"] = sum[field + "Modifier"],
			["dexterityDV"] = dexterity,
			["bonusDV"] = 0,
		}));

	/// <summary>The block chance: the shield's <c>defensiveBlockChance</c> plus the block modifiers, 0 without a shield, at most 100.</summary>
	private static double Block(AttributeSum gear, AttributeSum all)
	{
		double shield = gear["defensiveBlockChance"];
		return shield <= 0 ? 0 : Math.Min(100, shield + all["defensiveBlockModifier"]);
	}

	/// <summary>A damage bonus of tab III: its own % plus the total damage % (and the elemental % for fire, cold and lightning).</summary>
	private static DamageBonus Damage(StatDamage damage, AttributeSum sum)
	{
		var (direct, overTime, duration, elemental) = DamageFields[damage];
		double common = sum["offensiveTotalDamageModifier"] + (elemental ? sum["offensiveElementalModifier"] : 0);
		return new DamageBonus(
			direct is null ? 0 : Round(sum[direct] + common),
			overTime is null ? 0 : Round(sum[overTime] + common),
			duration is null ? 0 : Round(sum[duration]));
	}

	/// <summary>
	/// The pets' damage bonuses: the sums of the pet bonus records (<see cref="AttributeSum.PetBonusPrefix"/>), each type
	/// on its own (no total damage % added, unlike the character's lines); flat damages at the middle of their range.
	/// </summary>
	private static PetDamageBonus PetBonuses(AttributeSum sum)
	{
		double Pet(string field) => sum[AttributeSum.PetBonusPrefix + field];
		return new PetDamageBonus(
			Round(Pet("offensivePhysicalModifier")), Round(Pet("offensiveElementalModifier")), Round(Pet("offensiveLifeModifier")),
			Round(Pet("offensivePhysicalAverage")), Round(Pet("offensiveElementalAverage")), Round(Pet("offensiveLifeAverage")));
	}

	/// <summary>The racial bonuses of every race name an icon stands for; the defense % capped as the game caps it.</summary>
	private static RacialBonus Race(StatRace race, AttributeSum sum)
	{
		var bonus = default(RacialBonus);
		foreach (var name in RaceNames[race])
			if (sum.Races.TryGetValue(name, out var found))
				bonus = bonus.Add(found);

		return bonus;
	}
}
