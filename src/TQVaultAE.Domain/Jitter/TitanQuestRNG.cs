//-----------------------------------------------------------------------
// <copyright file="TitanQuestRNG.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TQVaultAE.Domain.Jitter;

/// <summary>
/// Park-Miller Minimal Standard (MINSTD) Pseudorandom Number Generator
/// using Schrage's multiplication method to prevent 32-bit overflow.
/// 
/// Constants used by Titan Quest:
///   - Multiplier (a) = 16807   (0x41A7)
///   - Quotient  (q) = 127773   (0x1F31D)
///   - Modulus   (m) = 2147483647 (0x7FFFFFFF)
/// 
/// The game falls back on the C runtime's rand() from a zero state; that state is unreachable from a valid seed,
/// so the fallback below does not reproduce it.
/// </summary>
public class TitanQuestRNG
{
	private uint _currentSeed;

	/// <summary>
	/// Creates a new RNG with the given initial seed (the item's Seed value from the .dbr).
	/// </summary>
	/// <param name="initialSeed">Item seed (int from 1..32767 typically).</param>
	public TitanQuestRNG(uint initialSeed)
	{
		this._currentSeed = initialSeed;
	}

	/// <summary>
	/// Creates the RNG as the game does for a new item: it stores <paramref name="itemSeed"/>
	/// then advances once. The same instance is then shared by all five equipment stores.
	/// </summary>
	/// <param name="itemSeed">Item seed as saved in the character/vault file.</param>
	/// <returns>RNG positioned before the first stat roll.</returns>
	public static TitanQuestRNG FromItemSeed(uint itemSeed)
	{
		var rng = new TitanQuestRNG(itemSeed);
		rng.AdvanceSeed();
		return rng;
	}

	/// <summary>
	/// Advances the internal seed by one step using Schrage's method.
	/// Returns the generated non-zero 32-bit value.
	/// </summary>
	/// <returns>Next PRNG output.</returns>
	public int AdvanceSeed()
	{
		while (true)
		{
			// Schrage's method:  next = a * r - m * q   (where r = seed % q)
			// The game computes it as:
			int nextValue = (int)(this._currentSeed * 16807) + (int)(this._currentSeed / 127773) * -2147483647;

			if (nextValue < 1)
			{
				nextValue += 2147483647;
			}

			this._currentSeed = (uint)nextValue;

			if (nextValue != 0)
			{
				return nextValue;
			}

			// Safety fallback (extremely rare edge case)
			this._currentSeed = (uint)(this._currentSeed + 1337 * -16807);
		}
	}

	/// <summary>
	/// An integer in [min, max], as the game draws it:
	/// <code>
	/// d = (uint)r / m             (double)
	/// d = d * (max - min) + min   (double)
	/// f = (float)d + 0.5f         (single)
	/// (int)f                      truncation toward zero
	/// </code>
	/// Truncation toward zero (not floor) means negative non-integer results round
	/// toward zero, e.g. GenerateRange(-4, 4) can never return -4.
	/// </summary>
	/// <param name="min">Lower bound.</param>
	/// <param name="max">Upper bound.</param>
	/// <returns>Engine result for the next PRNG step.</returns>
	public int GenerateRange(int min, int max)
	{
		int raw = this.AdvanceSeed();
		double d = ((double)raw + (raw < 0 ? 4294967296.0 : 0.0)) / 2147483647.0;
		d = d * ((double)max - (double)min) + (double)min;
		float f = (float)d + 0.5f;
		return (int)f;
	}

	/// <summary>
	/// Smallest and largest results <see cref="GenerateRange"/> can return, computed the same way
	/// at the generator's extreme outputs (1 and 2^31 - 2): every integer in between is reachable.
	/// </summary>
	/// <param name="min">Lower bound.</param>
	/// <param name="max">Upper bound.</param>
	/// <returns>Reachable result interval, e.g. (-3, 4) for (-4, 4).</returns>
	public static (int Min, int Max) GenerateRangeBounds(int min, int max)
	{
		static int At(int raw, int min, int max)
		{
			double d = (double)raw / 2147483647.0;
			d = d * ((double)max - (double)min) + (double)min;
			float f = (float)d + 0.5f;
			return (int)f;
		}

		int a = At(1, min, max);
		int b = At(2147483646, min, max);
		return a <= b ? (a, b) : (b, a);
	}

	/// <summary>
	/// A float in [min, max], as the game draws it: <c>(float)((double)(max - min) * ((uint)r / m) + min)</c>,
	/// with <c>max - min</c> computed in single precision.
	/// </summary>
	/// <param name="min">Lower bound.</param>
	/// <param name="max">Upper bound.</param>
	/// <returns>Engine result for the next PRNG step.</returns>
	public float GenerateFloat(float min, float max)
	{
		int raw = this.AdvanceSeed();
		double d = ((double)raw + (raw < 0 ? 4294967296.0 : 0.0)) / 2147483647.0;
		return (float)((double)(max - min) * d + (double)min);
	}

	/// <summary>
	/// Current internal seed value (for debugging / replay).
	/// </summary>
	public uint CurrentSeed => this._currentSeed;
}