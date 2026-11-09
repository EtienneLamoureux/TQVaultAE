using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application;

/// <summary>The gear as the attack damage reads it: each weapon in hand on its own (not shields), the other items and the item sets.</summary>
public sealed record OffenseGear(IReadOnlyList<AttributeSum> Weapons, AttributeSum Other);

/// <summary>What the damage of an attack needs.</summary>
/// <param name="Gear">The weapons in hand and the other items.</param>
/// <param name="AllGear">Every item worn (weapons, shield, others, sets): their % modifiers apply to every attack.</param>
/// <param name="Skills">The skills that count.</param>
/// <param name="Strength">Total strength.</param>
/// <param name="Dexterity">Total dexterity.</param>
/// <param name="Intelligence">Total intelligence.</param>
/// <param name="Level">Character level (life leech equation).</param>
/// <param name="AttackSpeed">The attack speed shown, in %, unrounded.</param>
public sealed record OffenseInput(OffenseGear Gear, AttributeSum AllGear, AttributeSum Skills, double Strength, double Dexterity, double Intelligence, int Level, double AttackSpeed, GameBalance Balance);

/// <summary>Tab I's offense: the weapon part and the part the gear and skills add, their sum shown as average damage, and DPS.</summary>
public readonly record struct OffenseValues(int WeaponDamage, int AddedDamage, int AverageDamage, int Dps);

/// <summary>
/// The Offense values of the character window, tab I (docs: character-stats study, offense.md). Not checked against
/// the game yet.
/// </summary>
public static class CharacterOffenseCalculator
{
	/// <summary>
	/// The attack speed shown, in % (the game <c>the game</c>):
	/// attacks per second / template speed × (unarmed attack duration / stance attack duration) × 100.
	/// <list type="bullet">
	/// <item>attacks per second = (template speed + flat) × (1 + (attack speed % + total speed %) / 100) + the weapons'
	/// <c>characterBaseAttackSpeed</c> (halved when dual wielding), between <c>playerAttackSpeedCapMin/Max</c> % of the
	/// template speed (<c>the game</c>); the total speed % is assumed to count, as for the other speeds;</item>
	/// <item>the durations are the stances' mean attack animation durations; a missing one counts as the same.</item>
	/// </list>
	/// </summary>
	/// <param name="all">The gear (weapons in hand) and skill sums.</param>
	/// <param name="unarmedDuration">Mean attack animation duration without weapon, in seconds.</param>
	/// <param name="stanceDuration">Mean attack animation duration of the stance, in seconds.</param>
	public static double AttackSpeed(AttributeSum all, WeaponStance stance, GameBalance balance, int difficulty, double unarmedDuration, double stanceDuration)
	{
		ArgumentNullException.ThrowIfNull(all);
		ArgumentNullException.ThrowIfNull(balance);
		double template = balance.BaseAttackSpeed;
		if (template <= 0)
			return 100;

		double character = Math.Max(0, (template + all["characterAttackSpeed"])
			* (1 + (all["characterAttackSpeedModifier"] + all["characterTotalSpeedModifier"]) / 100));
		double weapons = Math.Max(0, all["characterBaseAttackSpeed"]);
		if (stance == WeaponStance.DualWield)
			weapons *= 0.5;

		double perSecond = character + weapons;
		double capMax = balance.EngineValue("playerAttackSpeedCapMax", difficulty, 0);
		double capMin = balance.EngineValue("playerAttackSpeedCapMin", difficulty, 0);
		if (capMax > 0)
			perSecond = Math.Min(perSecond, capMax / 100 * template);
		perSecond = Math.Max(perSecond, capMin / 100 * template);

		double animation = unarmedDuration > 0 && stanceDuration > 0 ? unarmedDuration / stanceDuration : 1;
		return perSecond / template * animation * 100;
	}

	private const string Avg = "Average";
	private const string TotalDamageModifier = "offensiveTotalDamageModifier";
	private const string ElementalModifier = "offensiveElementalModifier";

	/// <summary>
	/// The average damage and DPS shown (the game <c>the game</c>, TQ.exe tab I):
	/// <list type="bullet">
	/// <item>per hand: the weapon's damage with every item's and skill's flat damage and % modifiers, each damage range at
	/// its middle, through the game's equations; the weapon part (the weapon with its own modifiers) and the added part;</item>
	/// <item>dual wield: half of both hands; no weapon: the template's hand hits;</item>
	/// <item>shown = round(weapon part) + round(added part); DPS = trunc(shown × attack speed / 100).</item>
	/// </list>
	/// </summary>
	public static OffenseValues Damage(OffenseInput input)
	{
		ArgumentNullException.ThrowIfNull(input);
		var flatShared = new AttributeSum();
		flatShared.Add(input.Gear.Other);
		flatShared.Add(input.Skills);
		var modifiers = new AttributeSum();
		modifiers.Add(input.AllGear);
		modifiers.Add(input.Skills);

		double weaponPart = 0, total = 0;
		List<AttributeSum?> weapons = input.Gear.Weapons.Count == 0 ? [null] : [.. input.Gear.Weapons.Take(2)];
		foreach (var weapon in weapons)
		{
			var flat = new AttributeSum();
			flat.Add(flatShared);
			if (weapon is not null)
				flat.Add(weapon);

			total += AttackDamage(weapon, flat, modifiers, input);
			weaponPart += weapon is null ? 0 : AttackDamage(weapon, weapon, weapon, input);
		}

		weaponPart /= weapons.Count;
		total /= weapons.Count;
		int weaponShown = (int)Math.Round(weaponPart, MidpointRounding.ToEven);
		int added = (int)Math.Round(Math.Max(0, total - weaponPart), MidpointRounding.ToEven);
		int shown = weaponShown + added;
		int dps = (int)Math.Max(0, Math.Truncate(shown * input.AttackSpeed / 100));
		return new OffenseValues(weaponShown, added, shown, dps);
	}

	/// <summary>The damage of one attack with a weapon (or bare hands when <paramref name="weapon"/> is null).</summary>
	/// <param name="flat">The flat damage that counts (the weapon's included).</param>
	/// <param name="modifiers">The % modifiers that count.</param>
	internal static double AttackDamage(AttributeSum? weapon, AttributeSum flat, AttributeSum modifiers, OffenseInput input)
	{
		var balance = input.Balance;
		double total = modifiers[TotalDamageModifier];
		double elemental = modifiers[ElementalModifier];
		double Scale(double value, double percent) => Math.Max(0, value * (1 + percent / 100));
		double Equation(string name, double value, string variable)
			=> value <= 0 ? 0 : GameEquation.Evaluate(balance.DamageEquation(name), new Dictionary<string, double>
			{
				[variable] = value,
				["strengthDV"] = input.Strength,
				["dexterityDV"] = input.Dexterity,
				["intelligenceDV"] = input.Intelligence,
				["characterLevelDV"] = input.Level,
			});

		double damage = 0;

		// Base physical: the weapon's (weapon equation, pierce ratio), or the hand hits (non-weapon equation).
		if (weapon is not null)
		{
			double physical = Equation("physicalDamageEquation", Scale(weapon["offensivePhysical" + Avg], modifiers["offensivePhysicalModifier"] + total), "physicalDamageDV");
			double ratio = Math.Clamp(weapon["offensivePierceRatio" + Avg] * (1 + modifiers["offensivePierceRatioModifier"] / 100), 0, 100);
			double pierce = ratio / 100 * physical;
			damage += physical - pierce;
			damage += Equation("pierceDamageEquation", Scale(pierce, modifiers["offensivePierceModifier"] + total), "pierceDamageDV");
		}
		else
		{
			double hands = (balance.HandHitDamageMin + Math.Max(balance.HandHitDamageMin, balance.HandHitDamageMax)) / 2;
			damage += Equation("physicalNonWeaponDamageEquation", Scale(hands, modifiers["offensivePhysicalModifier"] + total), "physicalDamageDV");
		}

		// Flat damage of the other items and the skills (and the weapon's other damage).
		double otherPhysical = flat["offensivePhysical" + Avg] - (weapon?["offensivePhysical" + Avg] ?? 0);
		damage += Scale(otherPhysical, modifiers["offensivePhysicalModifier"] + total);
		damage += flat["offensiveBonusPhysical" + Avg];
		damage += Equation("pierceNonWeaponDamageEquation", Scale(flat["offensivePierce" + Avg], modifiers["offensivePierceModifier"] + total), "pierceDamageDV");
		foreach (var element in (string[])["Fire", "Cold", "Lightning"])
			damage += Equation("elementalDamageEquation", Scale(flat[$"offensive{element}{Avg}"], modifiers[$"offensive{element}Modifier"] + elemental + total), "elementalDamageDV");
		damage += Scale(flat["offensiveLife" + Avg], modifiers["offensiveLifeModifier"] + total);
		damage += Equation("poisonDamageEquation", Scale(flat["offensivePoison" + Avg], modifiers["offensivePoisonModifier"] + total), "poisonDamageDV");

		// Damage over time: its per-second value, added once (the window does not multiply it by the duration).
		(string Kind, string Equation, string Variable, bool Elemental)[] overTime =
		[
			("Bleeding", "bleedingDurationDamageEquation", "bleedingDamageDV", false),
			("Poison", "poisonDurationDamageEquation", "poisonDamageDV", false),
			("Fire", "elementalDurationDamageEquation", "elementalDamageDV", true),
			("Cold", "elementalDurationDamageEquation", "elementalDamageDV", true),
			("Lightning", "elementalDurationDamageEquation", "elementalDamageDV", true),
			("Physical", "physicalDurationDamageEquation", "physicalDamageDV", false),
			("LifeLeach", "lifeLeachDurationDamageEquation", "lifeLeachDamageDV", false),
		];
		foreach (var (kind, equation, variable, isElemental) in overTime)
		{
			double percent = modifiers[$"offensiveSlow{kind}Modifier"] + total + (isElemental ? elemental : 0);
			damage += Equation(equation, Scale(flat[$"offensiveSlow{kind}{Avg}"], percent), variable);
		}

		damage += Scale(flat["offensiveSlowLife" + Avg], modifiers["offensiveSlowLifeModifier"] + total);
		return damage;
	}
}
