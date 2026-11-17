using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Character;

/// <summary>
/// The attribute part of a class set: the points spent in each attribute, read from the attribute values, and the
/// values a set asks for. Attributes in the game's order: Health, Energy, Strength, Intelligence, Dexterity.
/// </summary>
public static class ClassSetAttributeRules
{
	/// <summary>The minimum of each attribute.</summary>
	private static readonly int[] Minimums = [PlayerLevel.MinHealth, PlayerLevel.MinMana, PlayerLevel.MinStrength, PlayerLevel.MinIntelligence, PlayerLevel.MinDexterity];

	/// <summary>How much of each attribute one point buys.</summary>
	private static int Increment(int attribute) => PlayerLevel.IncrementPerPoint(attribute < 2);

	/// <summary>The points spent in each attribute over its minimum (a value under it counts none).</summary>
	/// <param name="values">The attribute values.</param>
	public static ClassSetAttributes PointsOf(IReadOnlyList<int> values)
		=> ClassSetAttributes.FromArray([.. Enumerable.Range(0, 5).Select(i => Math.Max(0, values[i] - Minimums[i]) / Increment(i))]);

	/// <summary>
	/// The attribute values a class set asks for, within the points the character has: the points left plus those spent
	/// now, given in the game's order while they last. What a value holds beyond whole points (a quest reward of 18
	/// intelligence) stays, so no point and no part of one is lost; every point not given stays in the pool.
	/// </summary>
	/// <param name="values">The attribute values now.</param>
	/// <param name="points">The points the set spends in each attribute.</param>
	/// <param name="pool">The attribute points left now.</param>
	/// <returns>The values, and false when the points did not pay for the whole set.</returns>
	public static (int[] Values, bool Complete) Targets(IReadOnlyList<int> values, ClassSetAttributes points, int pool)
	{
		var spent = PointsOf(values).ToArray();
		var wanted = points.ToArray();
		var available = pool + spent.Sum();
		var targets = new int[5];
		bool complete = true;
		for (int i = 0; i < 5; i++)
		{
			var given = Math.Clamp(wanted[i], 0, Math.Max(0, available));
			complete &= given == wanted[i];
			available -= given;
			// The value less its whole points, plus the points given.
			targets[i] = values[i] + (given - spent[i]) * Increment(i);
		}

		return (targets, complete);
	}
}
