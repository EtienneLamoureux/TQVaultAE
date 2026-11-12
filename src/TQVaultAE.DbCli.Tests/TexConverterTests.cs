using System.Buffers.Binary;
using AwesomeAssertions;
using ImageMagick;
using TQVaultAE.DbCli.Core;
using static TQVaultAE.Tests.DbCli.DbCliFixtures;

namespace TQVaultAE.Tests.DbCli;

public sealed class TexConverterTests
{
	private static uint U32(byte[] b, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(offset));

	// 2x1 BGRA: pure red then pure blue, both opaque.
	private static readonly byte[] RedBlue = [0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF];

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	public void ToDds_ExtractsFrameAfterVersionSpecificHeader(int version)
	{
		var frame = GameDds(2, 1, 32, RedBlue);
		var tex = Tex(version, frame);

		var dds = TexConverter.ToDds(tex);

		dds.Should().HaveCount(frame.Length);
		dds.AsSpan(128).ToArray().Should().Equal(RedBlue);
	}

	[Fact]
	public void ToDds_32Bit_FixesMagicMasksAlphaAndCaps()
	{
		var dds = TexConverter.ToDds(Tex(1, GameDds(2, 1, 32, RedBlue)));

		"DDS "u8.ToArray().Should().Equal(dds[..4]);
		U32(dds, 92).Should().Be(0x00FF0000u, "red mask");
		U32(dds, 96).Should().Be(0x0000FF00u, "green mask");
		U32(dds, 100).Should().Be(0x000000FFu, "blue mask");
		U32(dds, 104).Should().Be(0xFF000000u, "alpha mask");
		(U32(dds, 80) & 0x1).Should().Be(1u, "ALPHAPIXELS");
		(U32(dds, 108) & 0x1000).Should().Be(0x1000u, "DDSCAPS_TEXTURE");
	}

	[Fact]
	public void ToDds_24Bit_HasNoAlpha()
	{
		var dds = TexConverter.ToDds(Tex(1, GameDds(1, 1, 24, [1, 2, 3])));

		U32(dds, 92).Should().Be(0x00FF0000u);
		U32(dds, 104).Should().Be(0u);
		(U32(dds, 80) & 0x1).Should().Be(0u);
	}

	[Fact]
	public void ToDds_Compressed_LeavesPixelFormatAlone()
	{
		var frame = GameDds(4, 4, 0, new byte[16], fourCC: "DXT5");
		var dds = TexConverter.ToDds(Tex(2, frame));

		dds.AsSpan(76, 32).ToArray().Should().Equal(frame.AsSpan(76, 32).ToArray());
		(U32(dds, 108) & 0x1000).Should().Be(0x1000u);
	}

	[Fact]
	public void FrameCount_AndFrameSelection_ForAnimatedTex()
	{
		var tex = Tex(2, GameDds(1, 1, 32, [1, 1, 1, 1]), GameDds(1, 1, 32, [2, 2, 2, 2]), GameDds(1, 1, 32, [3, 3, 3, 3]));

		TexConverter.FrameCount(tex).Should().Be(3);
		TexConverter.ToDds(tex, 2)[128].Should().Be(3);
		var outOfRange = () => TexConverter.ToDds(tex, 3);
		outOfRange.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Frames_TruncatedFrame_Throws()
	{
		var tex = Tex(1, GameDds(1, 1, 32, [1, 1, 1, 1]));
		Array.Resize(ref tex, tex.Length - 2);

		var act = () => TexConverter.FrameCount(tex);

		act.Should().Throw<InvalidDataException>();
	}

	[Fact]
	public void ToDds_NotTex_Throws()
	{
		var act = () => TexConverter.ToDds("DDS xxxxxxxxxx"u8);

		act.Should().Throw<InvalidDataException>();
	}

	[Fact]
	public void ToPng_KeepsChannelOrder()
	{
		var png = TexConverter.ToPng(Tex(1, GameDds(2, 1, 32, RedBlue)));

		using var image = new MagickImage(png);
		image.Format.Should().Be(MagickFormat.Png);
		using var pixels = image.GetPixels();
		var left = pixels.GetPixel(0, 0).ToColor()!;
		var right = pixels.GetPixel(1, 0).ToColor()!;
		(left.R, left.G, left.B).Should().Be(((byte)255, (byte)0, (byte)0));
		(right.R, right.G, right.B).Should().Be(((byte)0, (byte)0, (byte)255));
	}
}
