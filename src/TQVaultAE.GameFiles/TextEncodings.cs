//-----------------------------------------------------------------------
// <copyright file="TextEncodings.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Text;

namespace TQVaultAE.GameFiles;

/// <summary>Encodings used by the game files.</summary>
public static class TextEncodings
{
	private static readonly Lazy<Encoding> LazyCp1252 = new(() =>
	{
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		return Encoding.GetEncoding(1252);
	});

	/// <summary>Windows-1252 (ARC names, BOM-less text files).</summary>
	public static Encoding Cp1252 => LazyCp1252.Value;
}
