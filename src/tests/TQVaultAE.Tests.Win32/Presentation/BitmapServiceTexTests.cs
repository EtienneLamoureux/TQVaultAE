using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.Tests.Win32.Presentation;

/// <summary>
/// Tests for <see cref="BitmapService.LoadFromTexMemory"/>: the u32 after the 'TEX' magic is the frame rate
/// (non-zero for animated textures), not a texture offset, and only the first frame is decoded.
/// </summary>
public class BitmapServiceTexTests
{
	private readonly BitmapService _service = new(NullLogger<BitmapService>.Instance);

	[Theory]
	[InlineData(0)]
	[InlineData(30)]
	public void LoadFromTexMemory_Version1_DecodesFirstFrameWhateverTheFps(int fps)
	{
		byte[] tex = BuildTex(version: 1, fps, Dds(3, 2), Dds(5, 7));

		using var bitmap = this._service.LoadFromTexMemory(tex, 0, tex.Length)!;

		bitmap.Width.Should().Be(3);
		bitmap.Height.Should().Be(2);
	}

	[Fact]
	public void LoadFromTexMemory_Version2AnimatedTexture_DecodesFirstFrame()
	{
		byte[] tex = BuildTex(version: 2, fps: 15, Dds(4, 4), Dds(8, 8));

		using var bitmap = this._service.LoadFromTexMemory(tex, 0, tex.Length)!;

		bitmap.Width.Should().Be(4);
		bitmap.Height.Should().Be(4);
	}

	[Fact]
	public void LoadFromTexMemory_FrameLengthBeyondData_Throws()
	{
		byte[] tex = BuildTex(version: 1, fps: 0, Dds(2, 2));
		BitConverter.GetBytes(tex.Length).CopyTo(tex, 8);

		var act = () => this._service.LoadFromTexMemory(tex, 0, tex.Length);

		act.Should().Throw<InvalidDataException>();
	}

	private static byte[] BuildTex(int version, int fps, params byte[][] frames)
	{
		var ms = new MemoryStream();
		var w = new BinaryWriter(ms);
		w.Write((byte)'T');
		w.Write((byte)'E');
		w.Write((byte)'X');
		w.Write((byte)version);
		w.Write(fps);
		if (version == 2)
			w.Write((byte)0);
		foreach (var f in frames) {
			w.Write(f.Length);
			w.Write(f);
		}
		w.Flush();
		return ms.ToArray();
	}

	/// <summary>Uncompressed 32-bit DDS with magic "DDSR" as found in the game textures.</summary>
	private static byte[] Dds(int width, int height)
	{
		var ms = new MemoryStream();
		var w = new BinaryWriter(ms);
		w.Write("DDSR"u8);
		w.Write(124); // header size
		w.Write(0x100F); // caps | height | width | pitch | pixelformat
		w.Write(height);
		w.Write(width);
		w.Write(width * 4); // pitch
		w.Write(0); // depth
		w.Write(0); // mipmaps
		for (int i = 0; i < 11; i++)
			w.Write(0);
		w.Write(32); // pixel format size
		w.Write(0x41); // RGB | ALPHAPIXELS
		w.Write(0); // fourcc
		w.Write(32); // bit count
		w.Write(0x00FF0000);
		w.Write(0x0000FF00);
		w.Write(0x000000FF);
		unchecked {
			w.Write((int)0xFF000000);
		}
		w.Write(0x1000); // caps
		w.Write(0);
		w.Write(0);
		w.Write(0);
		w.Write(0);
		for (int i = 0; i < width * height; i++)
			w.Write(unchecked((int)0xFF336699));
		w.Flush();
		return ms.ToArray();
	}
}
