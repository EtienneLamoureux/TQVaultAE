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
	/// attacks per second / template speed × 100, times the weapon factor when a weapon is in hand.
	/// <list type="bullet">
	/// <item>attacks per second = (template speed + flat) × (1 + (attack speed % + total speed %) / 100) + the weapons'
	/// <c>characterBaseAttackSpeed</c> (halved when dual wielding), between <c>playerAttackSpeedCapMin/Max</c> % of the
	/// template speed (<c>the game</c>); the total speed % is assumed to count, as for the other speeds;</item>
	/// <item>the weapon factor is the mean length of the template's unarmed attack animations in seconds (frames / frame
	/// rate, without their template speed), 19/30 for both templates. The binary divides the unarmed set's attack
	/// duration by the current set's, but with the files' values that gives 1.52, where three in-game readings need
	/// 0.633 whatever the stance: Copper Knife 106 %, Copper Hand Axe 93 % (one-handed, male), Iron Sudis 72 % (spear,
	/// female). An empirical fit: the bow, whose animations differ, is not checked.</item>
	/// </list>
	/// </summary>
	/// <param name="all">The gear (weapons in hand) and skill sums.</param>
	/// <param name="unarmedAttackSeconds">Mean length of the unarmed attack animations, in seconds; 0 when unknown.</param>
	public static double AttackSpeed(AttributeSum all, WeaponStance stance, GameBalance balance, int difficulty, double unarmedAttackSeconds)
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

		double animation = stance == WeaponStance.Unarmed || unarmedAttackSeconds <= 0 ? 1 : unarmedAttackSeconds;
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
	/// <item>dual wield: half of both hands; no weapon: the template's hand hits, through the weapon equation;</item>
	/// <item>shown = trunc(weapon part) + trunc(added part); DPS = trunc(shown × attack speed / 100). Truncated, as five
	/// in-game readings need: a character's Copper Knife 11.58 + 0.42 shows 11, where rounding gave 12.</item>
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
		int weaponShown = (int)Math.Truncate(weaponPart);
		int added = (int)Math.Truncate(Math.Max(0, total - weaponPart));
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

		// Base physical: the weapon's (weapon equation, pierce ratio), or the hand hits (non-weapon equation). The pierce
		// ratio takes its share of the physical result, which goes through the pierce equation: a character's Iron Sudis
		// (17-31, 30 % pierce, 99 Str, 115 Dex) shows 44 in the game, another character's Copper Knife (4-8, 15 %, 50 Str/Dex) 10.
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
			// The hands hit as a weapon: a character (strength 71.5, +5% total damage, no weapon) shows 6 in the game, the
			// weapon equation's 3.15 × (1 + 71.5 / 500) + 71.5 × 0.04 = 6.46, where the non-weapon one gives 3.5.
			double hands = (balance.HandHitDamageMin + Math.Max(balance.HandHitDamageMin, balance.HandHitDamageMax)) / 2;
			damage += Equation("physicalDamageEquation", Scale(hands, modifiers["offensivePhysicalModifier"] + total), "physicalDamageDV");
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
