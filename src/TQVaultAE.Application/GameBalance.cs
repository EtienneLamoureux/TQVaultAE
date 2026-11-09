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
