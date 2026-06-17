using AwesomeAssertions;
using System.Text;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Helpers;

/// <summary>
/// Unit tests for SpanHelper and related pool classes.
/// Merged from SpanHelperTests, SpanHelperBinaryTests, and SpanHelperArcTests.
/// </summary>
public class SpanHelperTests
{
	#region ToUpperSpan Tests

	[Fact]
	public void ToUpperSpan_WithNormalString_ReturnsUppercaseSpan()
	{
		// Arrange
		var input = "hello";

		// Act
		using var result = SpanHelper.ToUpperSpan(input);

		// Assert
		result.Span.ToString().Should().Be("HELLO");
		result.Length.Should().Be(5);
	}

	[Fact]
	public void ToUpperSpan_WithMixedCase_ReturnsUppercaseSpan()
	{
		// Arrange
		var input = "HeLLo WoRLD";

		// Act
		using var result = SpanHelper.ToUpperSpan(input);

		// Assert
		result.Span.ToString().Should().Be("HELLO WORLD");
	}

	[Fact]
	public void ToUpperSpan_WithEmptyString_ReturnsEmpty()
	{
		// Arrange
		var input = "";

		// Act
		using var result = SpanHelper.ToUpperSpan(input);

		// Assert
		result.Length.Should().Be(0);
	}

	[Fact]
	public void ToUpperSpan_WithNullString_ReturnsEmpty()
	{
		// Arrange
		string? input = null;

		// Act
		using var result = SpanHelper.ToUpperSpan(input!);

		// Assert
		result.Length.Should().Be(0);
	}

	[Fact]
	public void ToUpperSpan_WithSpecialCharacters_HandlesCorrectly()
	{
		// Arrange
		var input = "hello world! 123";

		// Act
		using var result = SpanHelper.ToUpperSpan(input);

		// Assert
		result.Span.ToString().Should().Be("HELLO WORLD! 123");
	}

	#endregion

	#region StartsWithIgnoreCase Tests

	[Theory]
	[InlineData("Hello World", "HELLO", true)]
	[InlineData("hello", "HELLO", true)]
	[InlineData("Hello World", "WORLD", false)]
	[InlineData("Hi", "HELLO", false)]
	[InlineData("HELLO", "HELLO", true)]
	public void StartsWithIgnoreCase_VariousInputs_ReturnsExpected(string span, string prefix, bool expected)
	{
		// Act
		var result = SpanHelper.StartsWithIgnoreCase(span.AsSpan(), prefix.AsSpan());

		// Assert
		result.Should().Be(expected);
	}

	[Fact]
	public void StartsWithIgnoreCase_WithEmptyPrefix_ReturnsTrue()
	{
		// Arrange
		var span = "Hello".AsSpan();
		var prefix = "".AsSpan();

		// Act
		var result = SpanHelper.StartsWithIgnoreCase(span, prefix);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void StartsWithIgnoreCase_WhenSpanShorterThanPrefix_ReturnsFalse()
	{
		// Arrange
		var span = "Hi".AsSpan();
		var prefix = "HELLO".AsSpan();

		// Act
		var result = SpanHelper.StartsWithIgnoreCase(span, prefix);

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	#region EqualsIgnoreCase Tests

	[Theory]
	[InlineData("Hello", "HELLO", true)]
	[InlineData("Hello", "World", false)]
	[InlineData("Hello", "Hell", false)]
	[InlineData("", "", true)]
	[InlineData("HELLO", "HELLO", true)]
	public void EqualsIgnoreCase_VariousInputs_ReturnsExpected(string span, string other, bool expected)
	{
		// Act
		var result = SpanHelper.EqualsIgnoreCase(span.AsSpan(), other.AsSpan());

		// Assert
		result.Should().Be(expected);
	}

	#endregion

	#region ConcatWithSlice Tests

	[Fact]
	public void ConcatWithSlice_WithValidPrefixAndSlice_ReturnsConcatenated()
	{
		// Arrange
		var prefix = "Hello ".AsSpan();
		var span = "Hello World".AsSpan();
		var skipPrefixChars = 6;

		// Act
		var result = SpanHelper.ConcatWithSlice(prefix, span, skipPrefixChars);

		// Assert
		result.Should().Be("Hello World");
	}

	[Fact]
	public void ConcatWithSlice_WithSkipPrefixChars_ReturnsWithoutPrefix()
	{
		// Arrange
		var prefix = "PREFIX:".AsSpan();
		var span = "PREFIX:VALUE".AsSpan();
		var skipPrefixChars = 7;

		// Act
		var result = SpanHelper.ConcatWithSlice(prefix, span, skipPrefixChars);

		// Assert
		result.Should().Be("PREFIX:VALUE");
	}

	[Theory]
	[InlineData("Item: ", "Weapon:Sword:Steel", 0, 6, "Item: Weapon")]
	[InlineData("[", "[Name:Value]", 1, 5, "[Name:")]
	public void ConcatWithSlice_WithSuffixLength_ReturnsCorrectSlice(
		string prefix, string span, int skipPrefixChars, int suffixLength, string expected)
	{
		// Act
		var result = SpanHelper.ConcatWithSlice(prefix.AsSpan(), span.AsSpan(), skipPrefixChars, suffixLength);

		// Assert
		result.Should().Be(expected);
	}

	#endregion

	#region ConcatSpans Tests

	[Theory]
	[InlineData("Hello", " World", "Hello World")]
	[InlineData("", "World", "World")]
	[InlineData("Hello", "", "Hello")]
	[InlineData("", "", "")]
	public void ConcatSpans_VariousInputs_ReturnsExpected(string first, string second, string expected)
	{
		// Act
		var result = SpanHelper.ConcatSpans(first.AsSpan(), second.AsSpan());

		// Assert
		result.Should().Be(expected);
	}

	#endregion

	#region Binary Reading Tests

	[Fact]
	public void ReadInt32LittleEndian_ValidData_ReturnsCorrectValue()
	{
		// Arrange - 0x12345678 in little-endian
		var data = new byte[] { 0x78, 0x56, 0x34, 0x12 };

		// Act
		var result = SpanHelper.ReadInt32LittleEndian(data, 0);

		// Assert
		result.Should().Be(0x12345678);
	}

	[Fact]
	public void ReadInt32LittleEndian_AtOffset_ReturnsCorrectValue()
	{
		// Arrange
		var data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x78, 0x56, 0x34, 0x12 };

		// Act
		var result = SpanHelper.ReadInt32LittleEndian(data, 4);

		// Assert
		result.Should().Be(0x12345678);
	}

	[Fact]
	public void ReadInt32LittleEndian_Zero_ReturnsZero()
	{
		// Arrange
		var data = new byte[] { 0x00, 0x00, 0x00, 0x00 };

		// Act
		var result = SpanHelper.ReadInt32LittleEndian(data, 0);

		// Assert
		result.Should().Be(0);
	}

	[Fact]
	public void ReadUInt32LittleEndian_ValidData_ReturnsCorrectValue()
	{
		// Arrange
		var data = new byte[] { 0x78, 0x56, 0x34, 0x12 };

		// Act
		var result = SpanHelper.ReadUInt32LittleEndian(data, 0);

		// Assert
		result.Should().Be(0x12345678u);
	}

	[Fact]
	public void ReadByte_ValidOffset_ReturnsCorrectValue()
	{
		// Arrange
		var data = new byte[] { 0x00, 0x41, 0x52, 0x43 };

		// Act
		var result = SpanHelper.ReadByte(data, 1);

		// Assert
		result.Should().Be(0x41);
	}

	#endregion

	#region ReadNullTerminatedAscii Tests

	[Fact]
	public void ReadNullTerminatedAscii_NormalString_ReturnsString()
	{
		// Arrange
		var data = "HelloWorld"u8.ToArray();
		var withNull = new byte[data.Length + 1];
		data.CopyTo(withNull, 0);
		withNull[data.Length] = 0x00;

		// Act
		var result = SpanHelper.ReadNullTerminatedAscii(withNull, 0, withNull.Length, out int bytesRead);

		// Assert
		result.Should().Be("HelloWorld");
		bytesRead.Should().Be(11);
	}

	[Fact]
	public void ReadNullTerminatedAscii_EmptyString_ReturnsEmpty()
	{
		// Arrange
		var data = new byte[] { 0x00 };

		// Act
		var result = SpanHelper.ReadNullTerminatedAscii(data, 0, data.Length, out int bytesRead);

		// Assert
		result.Should().BeEmpty();
		bytesRead.Should().Be(1);
	}

	[Fact]
	public void ReadNullTerminatedAscii_WithMarker_ReturnsNull()
	{
		// Arrange - string ending with 0x03 marker (inactive file marker)
		var data = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x03, 0x00 };

		// Act
		var result = SpanHelper.ReadNullTerminatedAscii(data, 0, data.Length, out int bytesRead);

		// Assert
		result.Should().BeNull();
		bytesRead.Should().Be(6);
	}

	[Fact]
	public void ReadNullTerminatedAscii_AtOffset_ReturnsCorrectString()
	{
		// Arrange
		var prefix = new byte[] { 0x00, 0x00, 0x00 };
		var str = "Test"u8.ToArray();
		var withNull = new byte[prefix.Length + str.Length + 1];
		prefix.CopyTo(withNull, 0);
		str.CopyTo(withNull, prefix.Length);
		withNull[withNull.Length - 1] = 0x00;

		// Act
		var result = SpanHelper.ReadNullTerminatedAscii(withNull, 3, withNull.Length - 3, out int bytesRead);

		// Assert
		result.Should().Be("Test");
		bytesRead.Should().Be(5);
	}

	[Fact]
	public void ReadNullTerminatedAscii_NoNullTerminator_ReturnsRemainingBytes()
	{
		// Arrange - string without null terminator
		var data = "NoNullTerminator"u8.ToArray();

		// Act
		var result = SpanHelper.ReadNullTerminatedAscii(data, 0, data.Length, out int bytesRead);

		// Assert
		result.Should().Be("NoNullTerminator");
		bytesRead.Should().Be(data.Length);
	}

	[Fact]
	public void ReadNullTerminatedAscii_OffsetBeyondLength_ReturnsNull()
	{
		// Arrange
		var data = new byte[] { 0x00, 0x00, 0x00 };

		// Act
		var result = SpanHelper.ReadNullTerminatedAscii(data, 10, data.Length, out int bytesRead);

		// Assert
		result.Should().BeNull();
		bytesRead.Should().Be(0);
	}

	#endregion

	#region ByteSpanEquals Tests

	[Theory]
	[InlineData("Hello", "Hello", true)]
	[InlineData("Hello", "World", false)]
	[InlineData("Hello", "Hi", false)]
	public void ByteSpanEquals_VariousInputs_ReturnsExpected(string first, string second, bool expected)
	{
		// Act
		var firstBytes = Encoding.UTF8.GetBytes(first);
		var secondBytes = Encoding.UTF8.GetBytes(second);
		var result = SpanHelper.ByteSpanEquals(firstBytes, secondBytes);

		// Assert
		result.Should().Be(expected);
	}

	[Fact]
	public void ByteSpanEquals_EmptySpans_ReturnsTrue()
	{
		// Arrange
		var first = ReadOnlySpan<byte>.Empty;
		var second = ReadOnlySpan<byte>.Empty;

		// Act
		var result = SpanHelper.ByteSpanEquals(first, second);

		// Assert
		result.Should().BeTrue();
	}

	#endregion

	#region FindPattern Tests

	[Theory]
	[InlineData("Hello World World", 0, "World", 6)]
	[InlineData("Hello World", 0, "Missing", -1)]
	[InlineData("Hello World World", 7, "World", 12)]
	[InlineData("Hi", 0, "Hello World", -1)]
	public void FindPattern_VariousInputs_ReturnsExpected(string data, int start, string pattern, int expected)
	{
		// Act
		var dataBytes = Encoding.UTF8.GetBytes(data);
		var patternBytes = Encoding.UTF8.GetBytes(pattern);
		var result = SpanHelper.FindPattern(dataBytes, start, patternBytes);

		// Assert
		result.Should().Be(expected);
	}

	[Fact]
	public void FindPattern_EmptyPattern_ReturnsMinusOne()
	{
		// Arrange
		var data = "Hello World"u8.ToArray();
		var pattern = ReadOnlySpan<byte>.Empty;

		// Act
		var result = SpanHelper.FindPattern(data, 0, pattern);

		// Assert
		result.Should().Be(-1);
	}

	[Fact]
	public void FindPattern_PatternLongerThanData_ReturnsMinusOne()
	{
		// Arrange
		var data = "Hi"u8.ToArray();
		var pattern = "Hello World"u8.ToArray().AsSpan();

		// Act
		var result = SpanHelper.FindPattern(data, 0, pattern);

		// Assert
		result.Should().Be(-1);
	}

	#endregion

	#region CreateSpan Tests

	[Fact]
	public void CreateSpan_ValidArray_ReturnsCorrectSpan()
	{
		// Arrange
		var array = new byte[] { 1, 2, 3, 4, 5 };

		// Act
		var result = SpanHelper.CreateSpan(array, 1, 3);

		// Assert
		result.Length.Should().Be(3);
		result[0].Should().Be(2);
		result[2].Should().Be(4);
	}

	#endregion

	#region ReadArcNullTerminatedString Tests

	[Fact]
	public void ReadArcNullTerminatedString_NormalString_ReturnsString()
	{
		// Arrange - "testfile.txt\0"
		var data = "testfile.txt"u8.ToArray();
		var withNull = new byte[data.Length + 1];
		data.CopyTo(withNull, 0);

		// Act
		var result = SpanHelper.ReadArcNullTerminatedString(withNull, 0, withNull.Length, out int bytesConsumed);

		// Assert
		result.Should().Be("testfile.txt");
		bytesConsumed.Should().Be(data.Length + 1);
	}

	[Fact]
	public void ReadArcNullTerminatedString_EmptyString_ReturnsEmpty()
	{
		// Arrange - single null byte
		var data = new byte[] { 0x00 };

		// Act
		var result = SpanHelper.ReadArcNullTerminatedString(data, 0, data.Length, out int bytesConsumed);

		// Assert
		result.Should().BeEmpty();
		bytesConsumed.Should().Be(1);
	}

	[Fact]
	public void ReadArcNullTerminatedString_WithInactiveMarker_HandlesCorrectly()
	{
		// Arrange - filename followed by 0x03 marker and null (as in ARC files for inactive entries)
		var data = new byte[] { 0x41, 0x42, 0x03, 0x00 };

		// Act
		var result = SpanHelper.ReadArcNullTerminatedString(data, 0, data.Length, out int bytesConsumed);

		// Assert
		result.Should().Be("AB");
		bytesConsumed.Should().Be(3);
	}

	[Fact]
	public void ReadArcNullTerminatedString_0x03AtStart_ReturnsNullOrEmpty()
	{
		// Arrange - 0x03 as first byte (inactive file marker at start of filename area)
		var data = new byte[] { 0x03, 0x00 };

		// Act
		var result = SpanHelper.ReadArcNullTerminatedString(data, 0, data.Length, out int bytesConsumed);

		// Assert
		result.Should().BeNullOrEmpty();
	}

	[Fact]
	public void ReadArcNullTerminatedString_NoNullTerminator_ReturnsAllBytes()
	{
		// Arrange - string without null terminator
		var data = "NoNull"u8.ToArray();

		// Act
		var result = SpanHelper.ReadArcNullTerminatedString(data, 0, data.Length, out int bytesConsumed);

		// Assert
		result.Should().Be("NoNull");
		bytesConsumed.Should().Be(data.Length);
	}

	[Fact]
	public void ReadArcNullTerminatedString_AtOffset_ReturnsCorrectSlice()
	{
		// Arrange
		var prefix = new byte[] { 0x00, 0x00 };
		var str = "test"u8.ToArray();
		var combined = new byte[prefix.Length + str.Length + 1];
		prefix.CopyTo(combined, 0);
		str.CopyTo(combined, prefix.Length);
		combined[combined.Length - 1] = 0x00;

		// Act
		var result = SpanHelper.ReadArcNullTerminatedString(combined, 2, combined.Length - 2, out int bytesConsumed);

		// Assert
		result.Should().Be("test");
		bytesConsumed.Should().Be(str.Length + 1);
	}

	[Fact]
	public void ReadArcNullTerminatedString_ExceedMaxBufferSize_StopsAtLimit()
	{
		// Arrange - long string without null terminator
		var longData = new byte[3000];
		for (int i = 0; i < 3000; i++)
			longData[i] = (byte)('A' + (i % 26));

		// Act
		var result = SpanHelper.ReadArcNullTerminatedString(longData, 0, 2048, out int bytesConsumed);

		// Assert
		result.Should().NotBeNull();
		bytesConsumed.Should().BeLessThanOrEqualTo(2048);
	}

	[Fact]
	public void ReadArcNullTerminatedString_NegativeOffset_ReturnsNull()
	{
		// Arrange
		var data = new byte[] { 0x41, 0x00 };

		// Act
		var result = SpanHelper.ReadArcNullTerminatedString(data, -1, data.Length, out int bytesConsumed);

		// Assert
		result.Should().BeNull();
		bytesConsumed.Should().Be(0);
	}

	[Fact]
	public void ReadArcNullTerminatedString_OffsetBeyondLength_ReturnsNull()
	{
		// Arrange
		var data = new byte[] { 0x41, 0x00, 0x00 };

		// Act
		var result = SpanHelper.ReadArcNullTerminatedString(data, 10, data.Length, out int bytesConsumed);

		// Assert
		result.Should().BeNull();
		bytesConsumed.Should().Be(0);
	}

	[Fact]
	public void ReadArcNullTerminatedString_ConsecutiveInactiveMarkers_ReturnsCorrectConsumed()
	{
		// Arrange - two consecutive 0x03 bytes followed by a valid string
		var data = new byte[] { 0x03, 0x03, 0x66, 0x69, 0x6C, 0x65, 0x00 }; // \x03\x03"file"\0

		// Act - first 0x03
		var result1 = SpanHelper.ReadArcNullTerminatedString(data, 0, data.Length, out int consumed1);
		// Second 0x03 (caller would start here)
		var result2 = SpanHelper.ReadArcNullTerminatedString(data, consumed1, data.Length - consumed1, out int consumed2);

		// Assert
		result1.Should().BeNull();
		consumed1.Should().Be(1);
		result2.Should().BeNull();
		consumed2.Should().Be(1);
		// At offset 2, we should have "file"
		var result3 = SpanHelper.ReadArcNullTerminatedString(data, consumed1 + consumed2, data.Length - consumed1 - consumed2, out int consumed3);
		result3.Should().Be("file");
		consumed3.Should().Be(5);
	}

	#endregion

	#region ReadInt32Array Tests

	[Fact]
	public void ReadInt32Array_ValidData_ReturnsAllValues()
	{
		// Arrange - 4 int32 values = 16 bytes
		var data = new byte[]
		{
			0x01, 0x00, 0x00, 0x00, // 1
			0x02, 0x00, 0x00, 0x00, // 2
			0x03, 0x00, 0x00, 0x00, // 3
			0x04, 0x00, 0x00, 0x00  // 4
		};

		// Act
		var result = SpanHelper.ReadInt32Array(data, 0, 4);

		// Assert
		result.Should().HaveCount(4);
		result.Should().Equal(new[] { 1, 2, 3, 4 });
	}

	[Fact]
	public void ReadInt32Array_WithOffset_ReturnsCorrectValues()
	{
		// Arrange - prefix bytes then 2 int32 values
		var prefix = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
		var values = new byte[]
		{
			0x0A, 0x00, 0x00, 0x00, // 10
			0x14, 0x00, 0x00, 0x00  // 20
		};
		var data = new byte[prefix.Length + values.Length];
		prefix.CopyTo(data, 0);
		values.CopyTo(data, prefix.Length);

		// Act
		var result = SpanHelper.ReadInt32Array(data, 4, 2);

		// Assert
		result.Should().HaveCount(2);
		result.Should().Equal(new[] { 10, 20 });
	}

	[Fact]
	public void ReadInt32Array_EmptyArray_ReturnsEmptyArray()
	{
		// Arrange
		var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };

		// Act
		var result = SpanHelper.ReadInt32Array(data, 0, 0);

		// Assert
		result.Should().BeEmpty();
	}

	#endregion

	#region ReadArcPartEntry Tests

	[Fact]
	public void ReadArcPartEntry_ValidData_ReturnsCorrectStruct()
	{
		// Arrange - 12 bytes: fileOffset(4), compressedSize(4), realSize(4)
		var data = new byte[]
		{
			0xE8, 0x03, 0x00, 0x00, // 1000
			0xD0, 0x07, 0x00, 0x00, // 2000
			0xB8, 0x0B, 0x00, 0x00  // 3000
		};

		// Act
		var result = SpanHelper.ReadArcPartEntry(data, 0);

		// Assert
		result.FileOffset.Should().Be(1000);
		result.CompressedSize.Should().Be(2000);
		result.RealSize.Should().Be(3000);
	}

	[Fact]
	public void ReadArcPartEntry_WithOffset_ReturnsCorrectStruct()
	{
		// Arrange - prefix bytes then part entry
		var prefix = new byte[] { 0x00, 0x00, 0x00, 0x00 };
		var partEntry = new byte[]
		{
			0x50, 0x01, 0x00, 0x00, // 336
			0xA0, 0x2E, 0x00, 0x00, // 11936
			0xF8, 0x3A, 0x00, 0x00  // 15096
		};
		var data = new byte[prefix.Length + partEntry.Length];
		prefix.CopyTo(data, 0);
		partEntry.CopyTo(data, prefix.Length);

		// Act
		var result = SpanHelper.ReadArcPartEntry(data, 4);

		// Assert
		result.FileOffset.Should().Be(336);
		result.CompressedSize.Should().Be(11936);
		result.RealSize.Should().Be(15096);
	}

	[Fact]
	public void ReadArcPartEntry_AllZeros_ReturnsZeros()
	{
		// Arrange
		var data = new byte[12];

		// Act
		var result = SpanHelper.ReadArcPartEntry(data, 0);

		// Assert
		result.FileOffset.Should().Be(0);
		result.CompressedSize.Should().Be(0);
		result.RealSize.Should().Be(0);
	}

	#endregion

	#region ReadArcDirEntry Tests

	[Fact]
	public void ReadArcDirEntry_ValidData_ReturnsCorrectStruct()
	{
		// Arrange - 44 bytes for directory entry
		var data = new byte[]
		{
			0x03, 0x00, 0x00, 0x00, // StorageType = 3 (compressed)
			0xE8, 0x03, 0x00, 0x00, // FileOffset = 1000
			0xD0, 0x07, 0x00, 0x00, // CompressedSize = 2000
			0xB8, 0x0B, 0x00, 0x00, // RealSize = 3000
			0x00, 0x00, 0x00, 0x00, // crap1 = 0
			0x00, 0x00, 0x00, 0x00, // crap2 = 0
			0x00, 0x00, 0x00, 0x00, // crap3 = 0
			0x02, 0x00, 0x00, 0x00, // NumberOfParts = 2
			0x00, 0x00, 0x00, 0x00, // FirstPart = 0
			0x0A, 0x00, 0x00, 0x00, // FilenameLength = 10
			0x00, 0x10, 0x00, 0x00  // FilenameOffset = 4096
		};

		// Act
		var result = SpanHelper.ReadArcDirEntry(data, 0);

		// Assert
		result.StorageType.Should().Be(3);
		result.FileOffset.Should().Be(1000);
		result.CompressedSize.Should().Be(2000);
		result.RealSize.Should().Be(3000);
		result.NumberOfParts.Should().Be(2);
		result.FirstPart.Should().Be(0);
		result.FilenameLength.Should().Be(10);
		result.FilenameOffset.Should().Be(4096);
	}

	[Fact]
	public void ReadArcDirEntry_UncompressedStorageType_ReturnsCorrectStruct()
	{
		// Arrange - StorageType = 1 (uncompressed/stored)
		var data = new byte[]
		{
			0x01, 0x00, 0x00, 0x00, // StorageType = 1 (stored)
			0xE8, 0x03, 0x00, 0x00, // FileOffset = 1000
			0xE8, 0x03, 0x00, 0x00, // CompressedSize = 1000
			0xE8, 0x03, 0x00, 0x00, // RealSize = 1000
			0x00, 0x00, 0x00, 0x00, // crap1
			0x00, 0x00, 0x00, 0x00, // crap2
			0x00, 0x00, 0x00, 0x00, // crap3
			0x00, 0x00, 0x00, 0x00, // NumberOfParts = 0
			0x00, 0x00, 0x00, 0x00, // FirstPart = 0
			0x0A, 0x00, 0x00, 0x00, // FilenameLength = 10
			0x00, 0x10, 0x00, 0x00  // FilenameOffset = 4096
		};

		// Act
		var result = SpanHelper.ReadArcDirEntry(data, 0);

		// Assert
		result.StorageType.Should().Be(1);
		result.FileOffset.Should().Be(1000);
		result.CompressedSize.Should().Be(1000);
		result.RealSize.Should().Be(1000);
		result.NumberOfParts.Should().Be(0);
	}

	[Fact]
	public void ReadArcDirEntry_WithOffset_ReturnsCorrectStruct()
	{
		// Arrange - prefix bytes then directory entry
		var prefix = new byte[44];
		var dirEntry = new byte[]
		{
			0x03, 0x00, 0x00, 0x00, // StorageType
			0x50, 0x01, 0x00, 0x00, // FileOffset = 336
			0x00, 0x04, 0x00, 0x00, // CompressedSize = 1024
			0x00, 0x10, 0x00, 0x00, // RealSize = 4096
			0x00, 0x00, 0x00, 0x00, // crap1
			0x00, 0x00, 0x00, 0x00, // crap2
			0x00, 0x00, 0x00, 0x00, // crap3
			0x03, 0x00, 0x00, 0x00, // NumberOfParts = 3
			0x01, 0x00, 0x00, 0x00, // FirstPart = 1
			0x08, 0x00, 0x00, 0x00, // FilenameLength = 8
			0x00, 0x20, 0x00, 0x00  // FilenameOffset = 8192
		};
		dirEntry.CopyTo(prefix, 0);

		// Act
		var result = SpanHelper.ReadArcDirEntry(prefix, 0);

		// Assert
		result.StorageType.Should().Be(3);
		result.FileOffset.Should().Be(336);
		result.NumberOfParts.Should().Be(3);
		result.FirstPart.Should().Be(1);
	}

	#endregion

	#region IsActiveArcEntry Tests

	[Theory]
	[InlineData(1, 0, true)]    // StorageType 1 = uncompressed = active
	[InlineData(1, 5, true)]    // StorageType 1 = uncompressed = active
	[InlineData(3, 0, false)]   // StorageType 3 = compressed but no parts = inactive
	[InlineData(3, 1, true)]    // StorageType 3 = compressed with parts = active
	[InlineData(3, 5, true)]    // StorageType 3 = compressed with parts = active
	[InlineData(0, 0, false)]   // StorageType 0 = inactive
	[InlineData(2, 0, false)]   // StorageType 2 = inactive
	[InlineData(4, 0, false)]   // StorageType 4 = inactive
	public void IsActiveArcEntry_VariousTypes_ReturnsExpected(int storageType, int numberOfParts, bool expected)
	{
		// Act
		var result = SpanHelper.IsActiveArcEntry(storageType, numberOfParts);

		// Assert
		result.Should().Be(expected);
	}

	#endregion
}

/// <summary>
/// Tests for PooledCharArray class.
/// </summary>
public class PooledCharArrayTests
{
	[Fact]
	public void Rent_WithPositiveLength_ReturnsPooledArray()
	{
		// Act
		using var result = PooledCharArray.Rent(10);

		// Assert
		result.Length.Should().Be(10);
		result.Span.Length.Should().Be(10);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-5)]
	public void Rent_WithZeroOrNegativeLength_ReturnsEmpty(int length)
	{
		// Act
		using var result = PooledCharArray.Rent(length);

		// Assert
		result.Length.Should().Be(0);
	}

	[Fact]
	public void FromString_WithValidString_ReturnsPooledArray()
	{
		// Arrange
		var input = "Hello";

		// Act
		using var result = PooledCharArray.FromString(input);

		// Assert
		result.Length.Should().Be(5);
		result.Span.ToString().Should().Be("Hello");
	}

	[Theory]
	[InlineData("")]
	[InlineData(null)]
	public void FromString_WithEmptyOrNull_ReturnsEmpty(string? input)
	{
		// Act
		using var result = PooledCharArray.FromString(input!);

		// Assert
		result.Length.Should().Be(0);
	}

	[Fact]
	public void Empty_HasZeroLength()
	{
		// Assert
		PooledCharArray.Empty.Length.Should().Be(0);
	}

	[Fact]
	public void Dispose_ReturnsArrayToPool()
	{
		// Arrange
		var pooled = PooledCharArray.Rent(100);
		pooled.Span[0] = 'X';

		// Act - dispose should not throw
		pooled.Dispose();

		// Assert - after dispose, Length returns 0
		pooled.Length.Should().Be(0);
	}

	[Fact]
	public void Span_AfterDispose_ReturnsEmpty()
	{
		// Arrange
		var pooled = PooledCharArray.Rent(10);
		pooled.Span[0] = 'A';

		// Act
		pooled.Dispose();

		// Assert
		pooled.Span.Length.Should().Be(0);
	}
}
