namespace TQVaultAE.Application;

/// <summary>The attribute a damage equation scales with, as the character window names it.</summary>
public enum MultiplierAttribute
{
	/// <summary>Dexterity: "Global Multiplier (Dex)" (<c>x4tagCAttribBonusDex</c>).</summary>
	Dexterity,

	/// <summary>Intelligence: "Global Multiplier (Int)" (<c>x4tagCAttribBonusInt</c>).</summary>
	Intelligence,

	/// <summary>Character level: "Global Multiplier (Lvl)" (<c>x4tagCAttribBonusLvl</c>).</summary>
	Level,
}

/// <summary>A damage's global multiplier: how much one point of the damage is worth with the character's attributes.</summary>
/// <param name="Value">The multiplier, such as 1.46.</param>
/// <param name="Attribute">The attribute it comes from.</param>
public readonly record struct GlobalMultiplier(double Value, MultiplierAttribute Attribute);

/// <summary>
/// The "*1.46 Global Multiplier (Dex)" lines of the character window's damage rollovers: the slope of the damage's
/// equation (<c>combatequations.dbr</c>) for the character's attributes, the value of one point of damage minus that of
/// none. Bleeding at 230 Dexterity: <c>bleedingDamageDV * ((dexterityDV / 500) + 1)</c> gives 1.46.
/// </summary>
public static class DamageMultiplier
{
	private static readonly (string Variable, MultiplierAttribute Attribute)[] Attributes =
	[
		("dexterityDV", MultiplierAttribute.Dexterity),
		("intelligenceDV", MultiplierAttribute.Intelligence),
		("characterLevelDV", MultiplierAttribute.Level),
	];

	/// <summary>
	/// The global multiplier of a damage equation, or <see langword="null"/> when it does not scale with exactly one of
	/// Dexterity, Intelligence or the level (the physical and pierce equations use Strength), or does not grow.
	/// </summary>
	/// <param name="equation">The equation, such as <c>bleedingDamageDV * ((dexterityDV / 500) + 1)</c>.</param>
	/// <param name="damageVariable">The damage variable in it, such as <c>bleedingDamageDV</c>.</param>
	public static GlobalMultiplier? Of(string equation, string damageVariable, double strength, double dexterity, double intelligence, int level)
	{
		if (equation.Contains("strengthDV", StringComparison.OrdinalIgnoreCase))
			return null;

		var used = Attributes.Where(a => equation.Contains(a.Variable, StringComparison.OrdinalIgnoreCase)).ToList();
		if (used.Count != 1)
			return null;

		double At(double damage) => GameEquation.Evaluate(equation, new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
		{
			[damageVariable] = damage,
			["strengthDV"] = strength,
			["dexterityDV"] = dexterity,
			["intelligenceDV"] = intelligence,
			["characterLevelDV"] = level,
		});

		var value = At(1) - At(0);
		return value > 0 ? new GlobalMultiplier(value, used[0].Attribute) : null;
	}
}
