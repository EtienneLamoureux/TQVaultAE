//-----------------------------------------------------------------------
// <copyright file="ItemJitterService.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TQVaultAE.Domain.Jitter;

/// <summary>
/// The Titan Quest AE item stat jitter, matching the values the game shows.
/// The roll order is in <see cref="EquipmentRollTable"/>; background in issue #552
/// (https://github.com/EtienneLamoureux/TQVaultAE/issues/552).
/// </summary>
/// <remarks>
/// The game uses two distinct entry points with different gating rules:
/// <list type="bullet">
/// <item><see cref="RollCharAffix"/>: affix values of the Char and Skill stores. Always rolls when the value is
/// non-zero, even with 0% jitter.</item>
/// <item><see cref="AddJitter"/>: base-item values, and affix values of the Damage, Retaliation and
/// Defense stores. Rolls only when the percent is &gt; 0, capped at 50%.</item>
/// </list>
/// </remarks>
public static class ItemJitterService
{
	/// <summary>
	/// Base-item jitter percent the equipment stores use for most attributes.
	/// </summary>
	public const float StoreBaseJitterPercent = 20.0f;

	/// <summary>
	/// Cap of every <see cref="AddJitter"/>. Not applied to Char/Skill affix rolls.
	/// </summary>
	public const float MaxJitterPercent = 50.0f;

	/// <summary>
	/// The jitter of one value, the same for every kind of attribute:
	/// <code>
	/// if (v == 0) return v;
	/// k = (int)((pct / 100f) * v); if (k == 0) k = 1;
	/// r = (float)rng.GenerateRange(-k, k) + v;
	/// return fabsf(r) &lt; 1f ? v : r;
	/// </code>
	/// </summary>
	/// <param name="value">Value to jitter.</param>
	/// <param name="jitterPercent">Jitter percent (no cap here).</param>
	/// <param name="rng">Item RNG.</param>
	/// <returns>Jittered value.</returns>
	public static float Jitter(float value, float jitterPercent, TitanQuestRNG rng)
	{
		if (value == 0f)
			return value;

		int k = (int)((jitterPercent / 100f) * value);
		if (k == 0)
			k = 1;

		float rolled = (float)rng.GenerateRange(-k, k) + value;
		return System.Math.Abs(rolled) < 1f ? value : rolled;
	}

	/// <summary>
	/// Smallest and largest results <see cref="Jitter"/> can produce for <paramref name="value"/>, independent of the seed.
	/// </summary>
	/// <param name="value">Value to jitter.</param>
	/// <param name="jitterPercent">Jitter percent (no cap here).</param>
	/// <returns>Reachable interval; (value, value) for 0.</returns>
	public static (float Min, float Max) JitterBounds(float value, float jitterPercent)
	{
		if (value == 0f)
			return (value, value);

		int k = (int)((jitterPercent / 100f) * value);
		if (k == 0)
			k = 1;

		var (lo, hi) = TitanQuestRNG.GenerateRangeBounds(-k, k);
		float min = float.MaxValue, max = float.MinValue;
		for (int offset = lo; offset <= hi; offset++) {
			float rolled = (float)offset + value;
			float result = System.Math.Abs(rolled) < 1f ? value : rolled;
			min = System.Math.Min(min, result);
			max = System.Math.Max(max, result);
		}

		return (min, max);
	}

	/// <summary>
	/// The roll of a character or skill attribute read from an affix: the affix's <c>lootRandomizerJitter</c>
	/// (default 0) is used uncapped, and a roll is consumed whenever the affix value is non-zero.
	/// </summary>
	/// <param name="affixValue">Attribute value read from the affix record.</param>
	/// <param name="lootRandomizerJitter">Affix record's <c>lootRandomizerJitter</c>, 0 when absent.</param>
	/// <param name="rng">Item RNG.</param>
	/// <returns>Jittered affix value.</returns>
	public static float RollCharAffix(float affixValue, float lootRandomizerJitter, TitanQuestRNG rng)
		=> Jitter(affixValue, lootRandomizerJitter, rng);

	/// <summary>
	/// The roll of a base-item value, or of a damage, retaliation or defense affix value:
	/// does nothing unless <paramref name="jitterPercent"/> &gt; 0; caps it at 50; then applies
	/// <see cref="Jitter"/> to every value in order.
	/// </summary>
	/// <param name="values">Values to jitter in place, in engine order.</param>
	/// <param name="jitterPercent">Jitter percent.</param>
	/// <param name="rng">Item RNG.</param>
	public static void AddJitter(Span<float> values, float jitterPercent, TitanQuestRNG rng)
	{
		if (!(jitterPercent > 0f))
			return;

		if (jitterPercent >= MaxJitterPercent)
			jitterPercent = MaxJitterPercent;

		for (int i = 0; i < values.Length; i++)
			values[i] = Jitter(values[i], jitterPercent, rng);
	}
}
