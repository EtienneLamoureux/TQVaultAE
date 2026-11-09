using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application;

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
}
