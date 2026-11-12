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
		result.Should().Equal([1, 2, 3, 4]);
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
		result.Should().Equal([10, 20]);
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
