namespace TQVaultAE.Application;

/// <summary>
/// What the game reads from its records to compute the character window: the player caps
/// (<c>records\xpack\game\gameengine.dbr</c>), the difficulty penalties (<c>playerAttributePak</c>,
/// <c>PlayerGameBalanceAttributes.dbr</c>), the designer equations (<c>combatequations.dbr</c>,
/// <c>healthmanaregen.dbr</c>) and the base values of the player template (<c>records\xpack\creatures\pc\malepc01.dbr</c>).
/// </summary>
public sealed record GameBalance
{
	/// <summary>Number of game balance levels per difficulty in the penalty arrays (18 values for 3 difficulties).</summary>
	public const int BalanceLevelsPerDifficulty = 6;

	/// <summary>The numeric fields of gameengine (the <c>player*Cap*</c> fields), arrays by difficulty.</summary>
	public IReadOnlyDictionary<string, IReadOnlyList<double>> Engine { get; init; } = new Dictionary<string, IReadOnlyList<double>>(StringComparer.OrdinalIgnoreCase);

	/// <summary>The numeric fields of the player game balance pak, arrays by game balance level.</summary>
	public IReadOnlyDictionary<string, IReadOnlyList<double>> Penalties { get; init; } = new Dictionary<string, IReadOnlyList<double>>(StringComparer.OrdinalIgnoreCase);

	public string OffensiveAbilityEquation { get; init; } = "(offensiveAbilityDV + ((dexterityDV * 1) + bonusDV)) * (1 + (offensiveAbilityModifierDV / 100))";
	public string DefensiveAbilityEquation { get; init; } = "(defensiveAbilityDV + ((dexterityDV * 1) + bonusDV)) * (1 + (defensiveAbilityModifierDV / 100))";
	public string LifeRegenEquation { get; init; } = "lifeRegen * elapsedTime";
	public string ManaRegenEquation { get; init; } = "(manaRegen + (intelligence / 100)) * elapsedTime";

	/// <summary>The player template's <c>characterLifeRegen</c>.</summary>
	public double BaseLifeRegen { get; init; } = 2;

	/// <summary>The player template's <c>characterManaRegen</c>.</summary>
	public double BaseManaRegen { get; init; } = 2;

	/// <summary>The player template's <c>characterRunSpeed</c>.</summary>
	public double BaseRunSpeed { get; init; } = 1.33;

	/// <summary>
	/// The damage equations of <c>combatequations.dbr</c> by field name (<c>physicalDamageEquation</c>...); a missing one
	/// keeps the original game's (<see cref="DamageEquation"/>).
	/// </summary>
	public IReadOnlyDictionary<string, string> CombatEquations { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	/// <summary>The original game's damage equations, used when the record lacks one.</summary>
	private static readonly Dictionary<string, string> DefaultDamageEquations = new(StringComparer.OrdinalIgnoreCase)
	{
		["physicalDamageEquation"] = "(physicalDamageDV * ((strengthDV/500)+1)) + (strengthDV*0.04)",
		["physicalNonWeaponDamageEquation"] = "physicalDamageDV * ((strengthDV/650)+1)",
		["pierceDamageEquation"] = "(((pierceDamageDV / ((strengthDV/500)+1)) * ((strengthDV/2000)+1)) * ((dexterityDV/400)+1)) + (dexterityDV*0.03)",
		["pierceNonWeaponDamageEquation"] = "pierceDamageDV * ((dexterityDV/1000) +1)",
		["elementalDamageEquation"] = "(elementalDamageDV * ((intelligenceDV / 650) + 1)) + (intelligenceDV*0.025)",
		["poisonDamageEquation"] = "poisonDamageDV * ((dexterityDV / 500) + 1)",
		["bleedingDurationDamageEquation"] = "bleedingDamageDV * ((dexterityDV / 500) + 1)",
		["elementalDurationDamageEquation"] = "elementalDamageDV * ((intelligenceDV / 500) + 1)",
		["physicalDurationDamageEquation"] = "physicalDamageDV * ((intelligenceDV / 500) +1)",
		["poisonDurationDamageEquation"] = "poisonDamageDV * ((dexterityDV / 500) + 1)",
		["lifeLeachDurationDamageEquation"] = "lifeLeachDamageDV  * (1 + (characterLevelDV / 40))",
	};

	/// <summary>A damage equation by its field name.</summary>
	public string DamageEquation(string name)
		=> this.CombatEquations.TryGetValue(name, out var equation) && !string.IsNullOrWhiteSpace(equation)
			? equation
			: DefaultDamageEquations.GetValueOrDefault(name, "0");

	/// <summary>The player template's <c>handHitDamageMin</c> and <c>handHitDamageMax</c>: the damage without weapon.</summary>
	public double HandHitDamageMin { get; init; } = 2;
	public double HandHitDamageMax { get; init; } = 4;

	/// <summary>The player template's <c>characterAttackSpeed</c>: attacks per second without weapon.</summary>
	public double BaseAttackSpeed { get; init; } = 0.74;

	/// <summary>The player template's <c>characterSpellCastSpeed</c>.</summary>
	public double BaseCastSpeed { get; init; } = 2.8;

	/// <summary>
	/// A gameengine value for a difficulty (0 Normal, 1 Epic, 2 Legendary): the last one past the end of the array,
	/// <paramref name="fallback"/> when missing (the game reads the caps with a default of 80).
	/// </summary>
	public double EngineValue(string name, int difficulty, double fallback = 80)
		=> this.Engine.TryGetValue(name, out var values) && values.Count > 0
			? values[Math.Clamp(difficulty, 0, values.Count - 1)]
			: fallback;

	/// <summary>
	/// The difficulty penalty of a defensive field (<c>defensiveFire</c>: 0, -40, -100). The pak is indexed by the game
	/// balance level; the game takes it from <c>the game</c>, assumed to be the first level of the
	/// difficulty's block of <see cref="BalanceLevelsPerDifficulty"/> (not verified in game).
	/// </summary>
	public double Penalty(string field, int difficulty)
	{
		if (!this.Penalties.TryGetValue(field, out var values) || values.Count == 0)
			return 0;

		int level = Math.Max(0, difficulty) * BalanceLevelsPerDifficulty;
		return values[Math.Min(level, values.Count - 1)];
	}
}
