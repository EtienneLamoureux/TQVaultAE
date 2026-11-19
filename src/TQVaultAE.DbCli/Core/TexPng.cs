//-----------------------------------------------------------------------
// <copyright file="TexPng.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using ImageMagick;

namespace TQVaultAE.DbCli.Core;

/// <summary>PNG conversion of textures (the DDS step is <see cref="TexConverter"/>).</summary>
public static class TexPng
{
	/// <summary>PNG of one frame (Magick.NET).</summary>
	public static byte[] FromTex(ReadOnlySpan<byte> tex, int frame = 0)
	{
		var dds = TexConverter.ToDds(tex, frame);
		using var image = new MagickImage(dds, MagickFormat.Dds);
		return image.ToByteArray(MagickFormat.Png);
	}
}
