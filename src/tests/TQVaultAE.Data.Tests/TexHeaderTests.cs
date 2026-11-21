using AwesomeAssertions;
using System.Buffers.Binary;

namespace TQVaultAE.Tests.Data;

public class TexHeaderTests
{
	/// <summary>
	/// Builds a texture with one frame: the TEX header, the frame size, then a 128-byte DDS header.
	/// </summary>
	internal static byte[] Tex(int version, int width, int height, string ddsMagic = "DDSR")
	{
		int ddsOffset = version == 1 ? 12 : 13;
		var data = new byte[ddsOffset + 128];
		data[0] = (byte)'T';
		data[1] = (byte)'E';
		data[2] = (byte)'X';
		data[3] = (byte)version;
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(ddsOffset - 4), 128);
		System.Text.Encoding.ASCII.GetBytes(ddsMagic).CopyTo(data, ddsOffset);
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(ddsOffset + 4), 124);
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(ddsOffset + 12), height);
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(ddsOffset + 16), width);
		return data;
	}

	[Theory]
	[InlineData(1, "DDSR")]
	[InlineData(1, "DDS ")]
	[InlineData(2, "DDSR")]
	public void TryReadSize_ReadsTheWidthAndHeightOfBothVersions(int version, string ddsMagic)
	{
		var ok = TexHeader.TryReadSize(Tex(version, 64, 96, ddsMagic), out int width, out int height);

		ok.Should().BeTrue();
		(width, height).Should().Be((64, 96));
	}

	[Fact]
	public void TryReadSize_RejectsAnotherFormat()
	{
		var data = Tex(1, 64, 96);
		data[0] = (byte)'X';

		TexHeader.TryReadSize(data, out _, out _).Should().BeFalse();
	}

	[Fact]
	public void TryReadSize_RejectsAFrameWithoutDdsMagic()
	{
		TexHeader.TryReadSize(Tex(1, 64, 96, "ABCD"), out _, out _).Should().BeFalse();
	}

	[Fact]
	public void TryReadSize_RejectsDataCutBeforeTheSize()
	{
		TexHeader.TryReadSize(Tex(2, 64, 96).AsSpan(0, 30), out _, out _).Should().BeFalse();
	}
}
