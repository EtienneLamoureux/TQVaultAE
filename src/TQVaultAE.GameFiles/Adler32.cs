//-----------------------------------------------------------------------
// <copyright file="Adler32.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TQVaultAE.GameFiles;

/// <summary>Incremental Adler-32 (RFC 1950).</summary>
public struct Adler32
{
	private const uint Mod = 65521;

	// Largest n such that 255n(n+1)/2 + (n+1)(Mod-1) fits in 32 bits.
	private const int NMax = 5552;

	private uint _a;
	private uint _b;
	private bool _started;

	/// <summary>Current checksum (1 for no data).</summary>
	public readonly uint Value => this._started ? (this._b << 16) | this._a : 1u;

	/// <summary>Adds bytes to the checksum.</summary>
	public void Append(ReadOnlySpan<byte> data)
	{
		if (!this._started)
		{
			this._a = 1;
			this._b = 0;
			this._started = true;
		}

		uint a = this._a;
		uint b = this._b;
		while (data.Length > 0)
		{
			int n = Math.Min(data.Length, NMax);
			var chunk = data[..n];
			int i = 0;
			for (; i + 4 <= n; i += 4)
			{
				a += chunk[i];
				b += a;
				a += chunk[i + 1];
				b += a;
				a += chunk[i + 2];
				b += a;
				a += chunk[i + 3];
				b += a;
			}
			for (; i < n; i++)
			{
				a += chunk[i];
				b += a;
			}
			a %= Mod;
			b %= Mod;
			data = data[n..];
		}
		this._a = a;
		this._b = b;
	}

	/// <summary>One-shot Adler-32.</summary>
	public static uint Compute(ReadOnlySpan<byte> data)
	{
		var adler = new Adler32();
		adler.Append(data);
		return adler.Value;
	}
}
