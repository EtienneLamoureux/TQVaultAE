//-----------------------------------------------------------------------
// <copyright file="TexHeader.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Buffers.Binary;

namespace TQVaultAE.GameFiles;

/// <summary>
/// Reads the picture size of a <c>.tex</c> texture from its headers, without decoding it.
/// </summary>
/// <remarks>
/// A <c>.tex</c> is <c>"TEX" 0x01</c>, <c>u32 fps</c> (version 1) or <c>"TEX" 0x02</c>, <c>u32 fps</c>, <c>u8 flag</c> (version 2),
/// then frames of <c>u32 size</c> and a DDS payload: <c>"DDS "</c> or <c>"DDSR"</c>, then the DDS header
/// whose height and width are at bytes 8 and 12 after its size field.
/// </remarks>
public static class TexHeader
{
	private const uint TexVersion1 = 0x01584554;
	private const uint TexVersion2 = 0x02584554;
	private const uint DdsMagic = 0x20534444;
	private const uint DdsrMagic = 0x52534444;

	/// <summary>
	/// Reads the width and height in pixels of the first frame.
	/// </summary>
	/// <returns><see langword="false"/> when the data is not a texture or is cut before the size.</returns>
	public static bool TryReadSize(ReadOnlySpan<byte> tex, out int width, out int height)
	{
		width = 0;
		height = 0;

		if (tex.Length < 4)
			return false;

		int ddsOffset = BinaryPrimitives.ReadUInt32LittleEndian(tex) switch
		{
			TexVersion1 => 12,
			TexVersion2 => 13,
			_ => -1,
		};

		// The DDS magic, the header size, the flags, then the height and the width.
		if (ddsOffset < 0 || tex.Length < ddsOffset + 20)
			return false;

		uint ddsMagic = BinaryPrimitives.ReadUInt32LittleEndian(tex[ddsOffset..]);
		if (ddsMagic != DdsMagic && ddsMagic != DdsrMagic)
			return false;

		height = BinaryPrimitives.ReadInt32LittleEndian(tex[(ddsOffset + 12)..]);
		width = BinaryPrimitives.ReadInt32LittleEndian(tex[(ddsOffset + 16)..]);
		return width > 0 && height > 0;
	}
}
