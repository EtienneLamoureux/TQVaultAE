//-----------------------------------------------------------------------
// <copyright file="TexConverter.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Buffers.Binary;

namespace TQVaultAE.GameFiles;

/// <summary>
/// Turns a <c>.tex</c> texture into standard DDS images, one per frame (tqdb extract, TQVault and ARZExplorer pictures).
/// </summary>
/// <remarks>
/// A <c>.tex</c> is <c>"TEX" 0x01</c>, <c>u32 fps</c> (version 1) or <c>"TEX" 0x02</c>, <c>u32 fps</c>, <c>u8 flag</c>
/// (version 2), then frames of <c>u32 size</c> and a DDS image whose magic is <c>"DDS "</c> or <c>"DDSR"</c>.
/// </remarks>
public static class TexConverter
{
	// DDS layout, offsets from the start of the file (magic included).
	internal const int DdsHeaderSize = 128;
	internal const int OffHeaderSize = 4;
	internal const int OffPfSize = 76;
	internal const int OffPfFlags = 80;
	internal const int OffPfFourCC = 84;
	internal const int OffPfBitCount = 88;
	internal const int OffPfRMask = 92;
	internal const int OffPfGMask = 96;
	internal const int OffPfBMask = 100;
	internal const int OffPfAMask = 104;
	internal const int OffCaps1 = 108;

	internal const uint DdpfAlphaPixels = 0x1;
	internal const uint DdpfFourCC = 0x4;
	internal const uint DdpfRgb = 0x40;
	internal const uint DdsCapsTexture = 0x1000;

	internal const uint MagicDds = 0x20534444; // "DDS "
	internal const uint MagicDdsr = 0x52534444; // "DDSR"

	/// <summary>
	/// <see langword="true"/> when the data starts with a <c>.tex</c> header of a known version.
	/// </summary>
	public static bool IsTex(ReadOnlySpan<byte> tex)
		=> FirstFrameOffset(tex) > 0;

	/// <summary>
	/// Number of frames in a .tex (animated textures have several).
	/// </summary>
	public static int FrameCount(ReadOnlySpan<byte> tex)
		=> Frames(tex, int.MaxValue).Count;

	/// <summary>
	/// Valid .dds of one frame: magic set to "DDS ", RGB(A) masks, RGB and ALPHAPIXELS flags filled for uncompressed
	/// formats, DDSCAPS_TEXTURE set. Only the frames up to <paramref name="frame"/> are read.
	/// </summary>
	/// <exception cref="InvalidDataException">The data is not a texture, or is cut or damaged.</exception>
	public static byte[] ToDds(ReadOnlySpan<byte> tex, int frame = 0)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(frame);
		var frames = Frames(tex, frame + 1);
		if (frame >= frames.Count)
			throw new ArgumentOutOfRangeException(nameof(frame), $"Texture has {frames.Count} frame(s).");
		var (offset, length) = frames[frame];
		var dds = tex.Slice(offset, length).ToArray();
		FixDdsHeader(dds);
		return dds;
	}

	/// <summary>
	/// Offset of the first frame's size, 0 when the data is not a known <c>.tex</c>.
	/// </summary>
	private static int FirstFrameOffset(ReadOnlySpan<byte> tex)
	{
		if (tex.Length < 8 || tex[0] != (byte)'T' || tex[1] != (byte)'E' || tex[2] != (byte)'X')
			return 0;
		return tex[3] switch
		{
			1 => 8, // 'TEX' 01, u32 fps
			2 => 9, // 'TEX' 02, u32 fps, u8 flag
			_ => 0,
		};
	}

	/// <summary>
	/// Ranges (offset, length) of the first <paramref name="maxFrames"/> embedded DDS images.
	/// </summary>
	internal static List<(int Offset, int Length)> Frames(ReadOnlySpan<byte> tex, int maxFrames)
	{
		int pos = FirstFrameOffset(tex);
		if (pos == 0)
			throw new InvalidDataException(tex.Length >= 4 && tex[..3].SequenceEqual("TEX"u8)
				? $"Unsupported TEX version {tex[3]}."
				: "Not a TEX texture.");

		var frames = new List<(int, int)>();
		while (frames.Count < maxFrames && pos + 4 <= tex.Length)
		{
			int size = BinaryPrimitives.ReadInt32LittleEndian(tex[pos..]);
			pos += 4;
			if (size <= 0)
				break;
			if (size > tex.Length - pos)
				throw new InvalidDataException($"TEX frame {frames.Count} declares {size} bytes, only {tex.Length - pos} left.");
			frames.Add((pos, size));
			pos += size;
		}
		if (frames.Count == 0)
			throw new InvalidDataException("TEX contains no frame.");
		return frames;
	}

	/// <summary>
	/// In-place DDS header fix-ups that make the game's images readable by standard decoders.
	/// </summary>
	internal static void FixDdsHeader(Span<byte> dds)
	{
		if (dds.Length < DdsHeaderSize)
			throw new InvalidDataException("DDS frame shorter than its header.");
		uint magic = BinaryPrimitives.ReadUInt32LittleEndian(dds);
		if (magic != MagicDds && magic != MagicDdsr)
			throw new InvalidDataException($"Unknown texture magic 0x{magic:X8}.");
		if (BinaryPrimitives.ReadInt32LittleEndian(dds[OffHeaderSize..]) != 124 || BinaryPrimitives.ReadInt32LittleEndian(dds[OffPfSize..]) != 32)
			throw new InvalidDataException("Invalid DDS header.");

		BinaryPrimitives.WriteUInt32LittleEndian(dds, MagicDds);

		uint flags = BinaryPrimitives.ReadUInt32LittleEndian(dds[OffPfFlags..]);
		int bitCount = BinaryPrimitives.ReadInt32LittleEndian(dds[OffPfBitCount..]);
		if ((flags & DdpfFourCC) == 0 && bitCount >= 24)
		{
			BinaryPrimitives.WriteUInt32LittleEndian(dds[OffPfRMask..], 0x00FF0000);
			BinaryPrimitives.WriteUInt32LittleEndian(dds[OffPfGMask..], 0x0000FF00);
			BinaryPrimitives.WriteUInt32LittleEndian(dds[OffPfBMask..], 0x000000FF);
			flags |= DdpfRgb;
			if (bitCount == 32)
			{
				flags |= DdpfAlphaPixels;
				BinaryPrimitives.WriteUInt32LittleEndian(dds[OffPfAMask..], 0xFF000000);
			}
			BinaryPrimitives.WriteUInt32LittleEndian(dds[OffPfFlags..], flags);
		}

		uint caps = BinaryPrimitives.ReadUInt32LittleEndian(dds[OffCaps1..]);
		BinaryPrimitives.WriteUInt32LittleEndian(dds[OffCaps1..], caps | DdsCapsTexture);
	}
}
