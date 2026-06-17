using System.Buffers.Binary;
using System.Text;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Data;

namespace TQVaultAE.Tests.Data;

public class TQDataServiceTests
{
	private readonly TQDataService _tqData;

	public TQDataServiceTests()
	{
		var mockLogger = new Mock<ILogger<TQDataService>>();
		_tqData = new TQDataService(mockLogger.Object);
	}

	#region ReadCString Tests

	[Fact]
	public void ReadCString_Span_EmptyString_ReturnsEmpty()
	{
		// Arrange: length=0
		byte[] data = new byte[]
		{
			0x00, 0x00, 0x00, 0x00  // length = 0
		};
		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result = _tqData.ReadCString(span, ref offset);

		// Assert
		result.Should().BeEmpty();
		offset.Should().Be(4); // 4 bytes for length
	}

	[Fact]
	public void ReadCString_Span_SimpleAsciiString_ReturnsCorrectString()
	{
		// Arrange: "test"
		byte[] data = new byte[]
		{
			0x04, 0x00, 0x00, 0x00,  // length = 4
			0x74, 0x65, 0x73, 0x74   // "test"
		};
		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result = _tqData.ReadCString(span, ref offset);

		// Assert
		result.Should().Be("test");
		offset.Should().Be(8); // 4 bytes length + 4 bytes data
	}

	[Fact]
	public void ReadCString_Span_ExtendedCharacters_ReturnsCorrectString()
	{
		// Arrange: "café" with extended characters
		string input = "café";
		byte[] stringBytes = TQDataService.Encoding1252.GetBytes(input);
		byte[] data = new byte[4 + stringBytes.Length];
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), stringBytes.Length);
		stringBytes.CopyTo(data, 4);

		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result = _tqData.ReadCString(span, ref offset);

		// Assert
		result.Should().Be(input);
	}

	[Fact]
	public void ReadCString_Span_MultipleStringsInSequence_ParsesAll()
	{
		// Arrange: ["first", "second", ""]
		var strings = new[] { "first", "second", "" };
		// Each string needs: 4 bytes (length) + byte count
		var totalLength = strings.Sum(s => 4 + TQDataService.Encoding1252.GetByteCount(s));
		var data = new byte[totalLength];
		int pos = 0;

		foreach (var str in strings)
		{
			var bytes = TQDataService.Encoding1252.GetBytes(str);
			BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos, 4), bytes.Length);
			pos += 4;
			bytes.CopyTo(data, pos);
			pos += bytes.Length;
		}

		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result1 = _tqData.ReadCString(span, ref offset);
		var result2 = _tqData.ReadCString(span, ref offset);
		var result3 = _tqData.ReadCString(span, ref offset);

		// Assert
		result1.Should().Be("first");
		result2.Should().Be("second");
		result3.Should().BeEmpty();
		offset.Should().Be(data.Length);
	}

	/// <summary>
	/// Verifies ReadCString with known C-string data.
	/// Note: BinaryWriter uses different format, so we test with direct data construction.
	/// </summary>
	[Fact]
	public void ReadCString_Span_KnownData_ProducesCorrectResult()
	{
		// Arrange: "Hello" with CP1252 encoding
		string testString = "Hello";
		byte[] stringBytes = TQDataService.Encoding1252.GetBytes(testString);
		var data = new byte[4 + stringBytes.Length];
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), stringBytes.Length);
		stringBytes.CopyTo(data, 4);

		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result = _tqData.ReadCString(span, ref offset);

		// Assert
		result.Should().Be(testString);
	}

	#endregion

	#region ReadCString BinaryReader Tests

	[Fact]
	public void ReadCString_BinaryReader_SimpleAsciiString_ReturnsCorrectString()
	{
		// Arrange
		string testString = "test";
		byte[] stringBytes = TQDataService.Encoding1252.GetBytes(testString);
		var data = new byte[4 + stringBytes.Length];
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), stringBytes.Length);
		stringBytes.CopyTo(data, 4);

		using var ms = new MemoryStream(data);
		using var reader = new BinaryReader(ms);

		// Act
		var result = _tqData.ReadCString(reader);

		// Assert
		result.Should().Be(testString);
	}

	[Fact]
	public void ReadCString_BinaryReader_ExtendedCharacters_ReturnsCorrectString()
	{
		// Arrange
		string input = "café";
		byte[] stringBytes = TQDataService.Encoding1252.GetBytes(input);
		var data = new byte[4 + stringBytes.Length];
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), stringBytes.Length);
		stringBytes.CopyTo(data, 4);

		using var ms = new MemoryStream(data);
		using var reader = new BinaryReader(ms);

		// Act
		var result = _tqData.ReadCString(reader);

		// Assert
		result.Should().Be(input);
	}

	[Fact]
	public void ReadCString_BinaryReader_EmptyString_ReturnsEmpty()
	{
		// Arrange
		var data = new byte[] { 0x00, 0x00, 0x00, 0x00 };

		using var ms = new MemoryStream(data);
		using var reader = new BinaryReader(ms);

		// Act
		var result = _tqData.ReadCString(reader);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void ReadCString_BinaryReader_MultipleReadsInSequence()
	{
		// Arrange: Write multiple strings to stream
		var strings = new[] { "first", "second", "" };
		using var ms = new MemoryStream();
		using var writer = new BinaryWriter(ms);
		foreach (var str in strings)
		{
			var bytes = TQDataService.Encoding1252.GetBytes(str);
			writer.Write(bytes.Length);
			writer.Write(bytes);
		}
		ms.Position = 0;

		using var reader = new BinaryReader(ms);

		// Act
		var result1 = _tqData.ReadCString(reader);
		var result2 = _tqData.ReadCString(reader);
		var result3 = _tqData.ReadCString(reader);

		// Assert
		result1.Should().Be("first");
		result2.Should().Be("second");
		result3.Should().BeEmpty();
	}

	#endregion

	#region ReadUTF16String Tests

	[Fact]
	public void ReadUTF16String_Span_EmptyString_ReturnsEmpty()
	{
		// Arrange: charCount = 0
		byte[] data = new byte[]
		{
			0x00, 0x00, 0x00, 0x00  // charCount = 0
		};
		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result = _tqData.ReadUTF16String(span, ref offset);

		// Assert
		result.Should().BeEmpty();
		offset.Should().Be(4); // 4 bytes for charCount
	}

	[Fact]
	public void ReadUTF16String_Span_SimpleAsciiString_ReturnsCorrectString()
	{
		// Arrange: "test" in UTF-16 LE (4 characters)
		byte[] data = new byte[]
		{
			0x04, 0x00, 0x00, 0x00,  // charCount = 4
			0x74, 0x00,              // 't'
			0x65, 0x00,              // 'e'
			0x73, 0x00,              // 's'
			0x74, 0x00               // 't'
		};
		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result = _tqData.ReadUTF16String(span, ref offset);

		// Assert
		result.Should().Be("test");
		offset.Should().Be(12); // 4 + (4 * 2)
	}

	[Fact]
	public void ReadUTF16String_Span_UnicodeCharacters_ReturnsCorrectString()
	{
		// Arrange: "tëst" with special character
		string input = "tëst";
		int charCount = input.Length;
		var data = new byte[4 + charCount * 2];
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), charCount);

		// Write UTF-16 LE
		Encoding.Unicode.GetBytes(input, data.AsSpan(4, charCount * 2));

		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result = _tqData.ReadUTF16String(span, ref offset);

		// Assert
		result.Should().Be(input);
	}

	[Fact]
	public void ReadUTF16String_Span_MultipleStringsInSequence_ParsesAll()
	{
		// Arrange: ["alpha", "beta"]
		var strings = new[] { "alpha", "beta" };
		// Each string needs: 4 bytes (char count) + (charCount * 2) bytes (UTF-16)
		var totalLength = strings.Sum(s => 4 + s.Length * 2);
		var data = new byte[totalLength];
		int pos = 0;

		foreach (var str in strings)
		{
			BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos, 4), str.Length);
			pos += 4;
			Encoding.Unicode.GetBytes(str, data.AsSpan(pos, str.Length * 2));
			pos += str.Length * 2;
		}

		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result1 = _tqData.ReadUTF16String(span, ref offset);
		var result2 = _tqData.ReadUTF16String(span, ref offset);

		// Assert
		result1.Should().Be("alpha");
		result2.Should().Be("beta");
		offset.Should().Be(data.Length);
	}

	/// <summary>
	/// Verifies ReadUTF16String with known UTF-16 data.
	/// Note: BinaryWriter uses different format, so we test with direct data construction.
	/// </summary>
	[Fact]
	public void ReadUTF16String_Span_KnownData_ProducesCorrectResult()
	{
		// Arrange: UTF-16 LE string "Test"
		string testString = "Test";
		int charCount = testString.Length;
		var data = new byte[4 + charCount * 2];
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), charCount);
		Encoding.Unicode.GetBytes(testString, data.AsSpan(4, charCount * 2));

		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result = _tqData.ReadUTF16String(span, ref offset);

		// Assert
		result.Should().Be(testString);
	}

	#endregion

	#region ReadUTF16String BinaryReader Tests

	[Fact]
	public void ReadUTF16String_BinaryReader_SimpleAsciiString_ReturnsCorrectString()
	{
		// Arrange: "test" in UTF-16 LE
		string testString = "test";
		int charCount = testString.Length;
		var data = new byte[4 + charCount * 2];
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), charCount);
		Encoding.Unicode.GetBytes(testString, data.AsSpan(4, charCount * 2));

		using var ms = new MemoryStream(data);
		using var reader = new BinaryReader(ms);

		// Act
		var result = _tqData.ReadUTF16String(reader);

		// Assert
		result.Should().Be(testString);
	}

	[Fact]
	public void ReadUTF16String_BinaryReader_EmptyString_ReturnsEmpty()
	{
		// Arrange
		var data = new byte[] { 0x00, 0x00, 0x00, 0x00 };

		using var ms = new MemoryStream(data);
		using var reader = new BinaryReader(ms);

		// Act
		var result = _tqData.ReadUTF16String(reader);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void ReadUTF16String_BinaryReader_UnicodeCharacters_ReturnsCorrectString()
	{
		// Arrange
		string input = "tëst";
		int charCount = input.Length;
		var data = new byte[4 + charCount * 2];
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), charCount);
		Encoding.Unicode.GetBytes(input, data.AsSpan(4, charCount * 2));

		using var ms = new MemoryStream(data);
		using var reader = new BinaryReader(ms);

		// Act
		var result = _tqData.ReadUTF16String(reader);

		// Assert
		result.Should().Be(input);
	}

	#endregion

	#region BinaryFindKey Tests

	/// <summary>
	/// OLD IMPLEMENTATION: Byte-by-byte comparison (for comparison testing)
	/// </summary>
	private static (int indexOf, int nextOffset) BinaryFindKeyOld(ReadOnlySpan<byte> dataSource, ReadOnlySpan<byte> key, int offset = 0)
	{
		int i = offset, j = 0;
		for (; i <= (dataSource.Length - key.Length); i++)
		{
			if (dataSource[i] == key[0])
			{
				j = 1;
				for (; j < key.Length && dataSource[i + j] == key[j]; j++) ;
				if (j == key.Length)
					goto found;

			}
		}
		// Not found
		return (-1, 0);
		found:
		return (i, i + key.Length);
	}

	[Fact]
	public void BinaryFindKey_FindsKeyAtStart()
	{
		// Arrange: Data with key at start
		var data = new byte[] { 0x6E, 0x75, 0x6D, 0x49, 0x74, 0x65, 0x6D, 0x73 }; // "numItems"
		var key = TQDataService.Encoding1252.GetBytes("numItems");

		// Act
		var result = _tqData.BinaryFindKey(data, key);

		// Assert
		result.indexOf.Should().Be(0);
		result.nextOffset.Should().Be(8);
	}

	[Fact]
	public void BinaryFindKey_FindsKeyInMiddle()
	{
		// Arrange: Data with key in the middle
		var data = new byte[100];
		var key = TQDataService.Encoding1252.GetBytes("test");
		key.CopyTo(data, 50);

		// Act
		var result = _tqData.BinaryFindKey(data, key);

		// Assert
		result.indexOf.Should().Be(50);
		result.nextOffset.Should().Be(54);
	}

	[Fact]
	public void BinaryFindKey_NotFound_ReturnsNegativeOne()
	{
		// Arrange: Data without the key
		var data = new byte[] { 0x00, 0x01, 0x02, 0x03 };
		var key = TQDataService.Encoding1252.GetBytes("missing");

		// Act
		var result = _tqData.BinaryFindKey(data, key);

		// Assert
		result.indexOf.Should().Be(-1);
	}

	[Fact]
	public void BinaryFindKey_OldAndNew_ProduceSameResult()
	{
		// Arrange: Various test cases
		var testCases = new[]
		{
			(new byte[] { 0x6E, 0x75, 0x6D, 0x49, 0x74, 0x65, 0x6D, 0x73 }, TQDataService.Encoding1252.GetBytes("numItems")), // Key at start
			(new byte[] { 0x00, 0x00, 0x6E, 0x75, 0x6D, 0x49, 0x74, 0x65, 0x6D, 0x73 }, TQDataService.Encoding1252.GetBytes("numItems")), // Key in middle
			(new byte[] { 0x01, 0x02, 0x03, 0x04 }, TQDataService.Encoding1252.GetBytes("missing")), // Not found
		};

		foreach (var (data, key) in testCases)
		{
			// Act - OLD implementation
			var oldResult = BinaryFindKeyOld(data.AsSpan(), key.AsSpan());

			// Act - NEW implementation (uses SequenceEqual)
			var newResult = _tqData.BinaryFindKey(data, key);

			// Assert - Results should be identical
			newResult.indexOf.Should().Be(oldResult.indexOf, $"indexOf mismatch for key '{TQDataService.Encoding1252.GetString(key)}'");
			newResult.nextOffset.Should().Be(oldResult.nextOffset, $"nextOffset mismatch for key '{TQDataService.Encoding1252.GetString(key)}'");
		}
	}

	[Fact]
	public void BinaryFindKey_WithOffset_SkipsBytesCorrectly()
	{
		// Arrange: Data with key but offset before it
		var data = new byte[20];
		var key = TQDataService.Encoding1252.GetBytes("test");
		key.CopyTo(data, 10);

		// Act - Search starting at offset 5
		var result = _tqData.BinaryFindKey(data, key, 5);

		// Assert
		result.indexOf.Should().Be(10);
	}

	[Fact]
	public void BinaryFindKey_StringOverload_WorksCorrectly()
	{
		// Arrange: String overload expects data with length prefix (like BinaryWriter format)
		// "numItems" = 8 chars, so we need: 4 bytes (length=8) + 8 bytes ("numItems")
		var data = new byte[] { 0x08, 0x00, 0x00, 0x00, 0x6E, 0x75, 0x6D, 0x49, 0x74, 0x65, 0x6D, 0x73 };

		// Act
		var result = _tqData.BinaryFindKey(data, "numItems");

		// Assert: Returns position after the length prefix
		result.indexOf.Should().Be(4);
		result.nextOffset.Should().Be(12);
	}

	[Fact]
	public void BinaryFindKey_StringOverload_NotFound_ReturnsNegativeOne()
	{
		// Arrange: Data with length prefix but different key
		var data = new byte[] { 0x08, 0x00, 0x00, 0x00, 0x6E, 0x75, 0x6D, 0x49, 0x74, 0x65, 0x6D, 0x73 };

		// Act
		var result = _tqData.BinaryFindKey(data, "missing");

		// Assert
		result.indexOf.Should().Be(-1);
	}

	[Fact]
	public void BinaryFindKey_KeyAtEnd_ReturnsCorrectOffset()
	{
		// Arrange: Key at the very end
		var data = new byte[50];
		var key = TQDataService.Encoding1252.GetBytes("end");
		key.CopyTo(data, 47);

		// Act
		var result = _tqData.BinaryFindKey(data, key);

		// Assert
		result.indexOf.Should().Be(47);
		result.nextOffset.Should().Be(50);
	}

	#endregion

	#region ValidateNextString Tests

	[Fact]
	public void ValidateNextString_BinaryReader_MatchingString_DoesNotThrow()
	{
		// Arrange
		var data = new byte[] { 0x04, 0x00, 0x00, 0x00, 0x74, 0x65, 0x73, 0x74 }; // "test"
		using var ms = new MemoryStream(data);
		using var reader = new BinaryReader(ms);

		// Act & Assert - Should not throw
		var action = () => _tqData.ValidateNextString("test", reader);
		action.Should().NotThrow();
	}

	[Fact]
	public void ValidateNextString_BinaryReader_NonMatchingString_ThrowsArgumentException()
	{
		// Arrange
		var data = new byte[] { 0x04, 0x00, 0x00, 0x00, 0x74, 0x65, 0x73, 0x74 }; // "test"
		using var ms = new MemoryStream(data);
		using var reader = new BinaryReader(ms);

		// Act & Assert
		var action = () => _tqData.ValidateNextString("expected", reader);
		action.Should().Throw<ArgumentException>();
	}

	[Fact]
	public void ValidateNextString_BinaryReader_CaseInsensitiveMatch()
	{
		// Arrange
		var data = new byte[] { 0x04, 0x00, 0x00, 0x00, 0x54, 0x45, 0x53, 0x54 }; // "TEST" in uppercase
		using var ms = new MemoryStream(data);
		using var reader = new BinaryReader(ms);

		// Act & Assert - Should match case-insensitively
		var action = () => _tqData.ValidateNextString("test", reader);
		action.Should().NotThrow();
	}

	[Fact]
	public void ValidateNextString_Span_MatchingString_ReturnsTrue()
	{
		// Arrange
		var data = new byte[] { 0x04, 0x00, 0x00, 0x00, 0x74, 0x65, 0x73, 0x74 }; // "test"
		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result = _tqData.ValidateNextString("test", span, ref offset);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void ValidateNextString_Span_NonMatchingString_ReturnsFalse()
	{
		// Arrange
		var data = new byte[] { 0x04, 0x00, 0x00, 0x00, 0x74, 0x65, 0x73, 0x74 }; // "test"
		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result = _tqData.ValidateNextString("expected", span, ref offset);

		// Assert
		result.Should().BeFalse();
		offset.Should().Be(0); // Offset should be restored
	}

	[Fact]
	public void ValidateNextString_Span_CaseInsensitiveMatch()
	{
		// Arrange
		var data = new byte[] { 0x04, 0x00, 0x00, 0x00, 0x54, 0x45, 0x53, 0x54 }; // "TEST" in uppercase
		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result = _tqData.ValidateNextString("test", span, ref offset);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void ValidateNextString_Span_MultipleValidationsInSequence()
	{
		// Arrange: ["begin_block", "numItems", "size"]
		var strings = new[] { "begin_block", "numItems", "size" };
		var totalLength = strings.Sum(s => 4 + TQDataService.Encoding1252.GetByteCount(s));
		var data = new byte[totalLength];
		int pos = 0;

		foreach (var str in strings)
		{
			var bytes = TQDataService.Encoding1252.GetBytes(str);
			BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos, 4), bytes.Length);
			pos += 4;
			bytes.CopyTo(data, pos);
			pos += bytes.Length;
		}

		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act & Assert
		_tqData.ValidateNextString("begin_block", span, ref offset).Should().BeTrue();
		_tqData.ValidateNextString("numItems", span, ref offset).Should().BeTrue();
		_tqData.ValidateNextString("size", span, ref offset).Should().BeTrue();
	}

	#endregion

	#region MatchNextString Tests

	[Fact]
	public void MatchNextString_MatchingString_ReturnsTrue()
	{
		// Arrange
		var data = new byte[] { 0x04, 0x00, 0x00, 0x00, 0x74, 0x65, 0x73, 0x74 }; // "test"
		using var ms = new MemoryStream(data);
		using var reader = new BinaryReader(ms);

		// Act
		var result = _tqData.MatchNextString("test", reader);

		// Assert
		result.Should().BeTrue();
		// Reader position should be restored
		reader.BaseStream.Position.Should().Be(0);
	}

	[Fact]
	public void MatchNextString_NonMatchingString_ReturnsFalse()
	{
		// Arrange
		var data = new byte[] { 0x04, 0x00, 0x00, 0x00, 0x74, 0x65, 0x73, 0x74 }; // "test"
		using var ms = new MemoryStream(data);
		using var reader = new BinaryReader(ms);

		// Act
		var result = _tqData.MatchNextString("expected", reader);

		// Assert
		result.Should().BeFalse();
		reader.BaseStream.Position.Should().Be(0); // Position restored
	}

	[Fact]
	public void MatchNextString_CaseInsensitiveMatch()
	{
		// Arrange
		var data = new byte[] { 0x04, 0x00, 0x00, 0x00, 0x54, 0x45, 0x53, 0x54 }; // "TEST"
		using var ms = new MemoryStream(data);
		using var reader = new BinaryReader(ms);

		// Act
		var result = _tqData.MatchNextString("test", reader);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void MatchNextString_DoesNotAdvanceReaderPosition()
	{
		// Arrange
		var data = new byte[] { 0x04, 0x00, 0x00, 0x00, 0x74, 0x65, 0x73, 0x74, 0x01, 0x02, 0x03, 0x04 };
		using var ms = new MemoryStream(data);
		using var reader = new BinaryReader(ms);

		// Act
		_tqData.MatchNextString("test", reader);

		// Assert - Position should be restored to start, reading Int32 gives length = 4
		reader.ReadInt32().Should().Be(4);
	}

	#endregion

	#region WriteCString Tests

	[Fact]
	public void WriteCString_SimpleString_WritesCorrectData()
	{
		// Arrange
		using var ms = new MemoryStream();
		using var writer = new BinaryWriter(ms);

		// Act
		_tqData.WriteCString(writer, "test");

		// Assert
		var result = ms.ToArray();
		result.Length.Should().Be(8); // 4 bytes length + 4 bytes "test"
		BinaryPrimitives.ReadInt32LittleEndian(result.AsSpan(0, 4)).Should().Be(4);
		result[4].Should().Be(0x74); // 't'
		result[5].Should().Be(0x65); // 'e'
		result[6].Should().Be(0x73); // 's'
		result[7].Should().Be(0x74); // 't'
	}

	[Fact]
	public void WriteCString_EmptyString_WritesZeroLength()
	{
		// Arrange
		using var ms = new MemoryStream();
		using var writer = new BinaryWriter(ms);

		// Act
		_tqData.WriteCString(writer, string.Empty);

		// Assert
		var result = ms.ToArray();
		result.Length.Should().Be(4); // Just length = 0
		BinaryPrimitives.ReadInt32LittleEndian(result.AsSpan(0, 4)).Should().Be(0);
	}

	[Fact]
	public void WriteCString_ExtendedCharacters_WritesCorrectBytes()
	{
		// Arrange
		using var ms = new MemoryStream();
		using var writer = new BinaryWriter(ms);

		// Act
		_tqData.WriteCString(writer, "café");

		// Assert
		var result = ms.ToArray();
		result.Length.Should().Be(8); // 4 bytes length + 4 bytes (but é is 1 byte in CP1252)
		// café in CP1252 is: 63 61 66 E9
		BinaryPrimitives.ReadInt32LittleEndian(result.AsSpan(0, 4)).Should().Be(4);
	}

	[Fact]
	public void WriteCString_CanReadBackWithReadCString()
	{
		// Arrange
		using var ms = new MemoryStream();
		using var writer = new BinaryWriter(ms);

		var original = "Hello, World!";
		_tqData.WriteCString(writer, original);

		// Act - Read it back
		ms.Position = 0;
		var result = _tqData.ReadCString(new BinaryReader(ms));

		// Assert
		result.Should().Be(original);
	}

	#endregion

	#region WriteIntAfter Tests

	[Fact]
	public void WriteIntAfter_KeyExists_OverwritesValue()
	{
		// Arrange: Data with "numItems" key followed by value 10
		// BinaryFindKey(string) prepends length, so layout is: [len(4 bytes)][key bytes][value(4 bytes)]
		var data = new byte[100];
		var key = TQDataService.Encoding1252.GetBytes("numItems");
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), key.Length);
		key.CopyTo(data, 4);
		// Value at position 4+8=12 (after key)
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(12, 4), 10);

		// Act
		var result = _tqData.WriteIntAfter(data, "numItems", 99);

		// Assert
		result.indexOf.Should().Be(4);
		result.valueAsInt.Should().Be(10); // Old value
		BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(12, 4)).Should().Be(99); // New value
	}

	[Fact]
	public void WriteIntAfter_KeyNotFound_ReturnsNegativeOne()
	{
		// Arrange
		var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };

		// Act
		var result = _tqData.WriteIntAfter(data, "missing", 99);

		// Assert
		result.indexOf.Should().Be(-1);
	}

	[Fact]
	public void WriteIntAfter_WithOffset_StartsSearchFromOffset()
	{
		// Arrange: Key appears twice, offset should find the second one
		// First occurrence at offset 5
		var data = new byte[100];
		var key = TQDataService.Encoding1252.GetBytes("test");
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(5, 4), key.Length);
		key.CopyTo(data, 9);
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(13, 4), 1);

		// Second occurrence at offset 50
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(50, 4), key.Length);
		key.CopyTo(data, 54);
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(58, 4), 2);

		// Act - Start search after first occurrence
		var result = _tqData.WriteIntAfter(data, "test", 99, offset: 20);

		// Assert - Should find second occurrence
		result.indexOf.Should().Be(54);
		result.valueAsInt.Should().Be(2);
	}

	#endregion

	#region WriteFloatAfter Tests

	[Fact]
	public void WriteFloatAfter_KeyExists_OverwritesValue()
	{
		// Arrange: Data with "health" key followed by float value
		// BinaryFindKey(string) prepends length, so layout is: [len(4 bytes)][key bytes][value(4 bytes)]
		var data = new byte[100];
		var key = TQDataService.Encoding1252.GetBytes("health");
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), key.Length);
		key.CopyTo(data, 4);
		var floatBytes = BitConverter.GetBytes(3.14f);
		floatBytes.CopyTo(data, 10); // key is 6 chars, value at position 4+6=10

		// Act
		var result = _tqData.WriteFloatAfter(data, "health", 2.71f);

		// Assert
		result.indexOf.Should().Be(4);
		BitConverter.ToSingle(data, 10).Should().BeApproximately(2.71f, 0.001f);
	}

	[Fact]
	public void WriteFloatAfter_KeyNotFound_ReturnsNegativeOne()
	{
		// Arrange
		var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };

		// Act
		var result = _tqData.WriteFloatAfter(data, "missing", 1.5f);

		// Assert
		result.indexOf.Should().Be(-1);
	}

	#endregion

	#region ReadIntAfter Tests

	[Fact]
	public void ReadIntAfter_KeyExists_ReturnsCorrectValue()
	{
		// Arrange: Data with "numItems" key followed by value 42
		// BinaryFindKey with string prepends length, so layout is: [len(4 bytes)][key bytes]
		var data = new byte[100];
		var key = TQDataService.Encoding1252.GetBytes("numItems");
		// Key length prefix
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), key.Length);
		key.CopyTo(data, 4);
		// Value follows the key (key is 8 chars = positions 4-11, value at 12)
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(12, 4), 42);

		// Act
		var result = _tqData.ReadIntAfter(data, "numItems");

		// Assert
		result.indexOf.Should().Be(4);
		result.valueOffset.Should().Be(12);
		result.valueAsInt.Should().Be(42);
		result.nextOffset.Should().Be(16);
	}

	[Fact]
	public void ReadIntAfter_KeyNotFound_ReturnsNegativeOne()
	{
		// Arrange
		var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };

		// Act
		var result = _tqData.ReadIntAfter(data, "missing");

		// Assert
		result.indexOf.Should().Be(-1);
		result.valueAsInt.Should().Be(0);
	}

	[Fact]
	public void ReadIntAfter_WithOffset_StartsSearchFromOffset()
	{
		// Arrange: Key at offset 10 - need to account for length prefix in data
		var data = new byte[100];
		var key = TQDataService.Encoding1252.GetBytes("test");
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(10, 4), key.Length);
		key.CopyTo(data, 14);
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(18, 4), 123);

		// Act
		var result = _tqData.ReadIntAfter(data, "test", offset: 10);

		// Assert
		result.valueAsInt.Should().Be(123);
	}

	#endregion

	#region ReadFloatAfter Tests

	[Fact]
	public void ReadFloatAfter_KeyExists_ReturnsCorrectValue()
	{
		// Arrange: Data with key-with-prefix at position 0
		// BinaryFindKey prepends 4-byte length to key, so layout must be [keyLength][key][value]
		var data = new byte[100];
		var keyBytes = TQDataService.Encoding1252.GetBytes("health");
		// Write key length (4 bytes) + key bytes at position 0
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), keyBytes.Length);
		keyBytes.CopyTo(data, 4);
		// Float value follows the key-with-prefix at position 10 (4 bytes length + 6 bytes key)
		var floatBytes = BitConverter.GetBytes(99.5f);
		floatBytes.CopyTo(data, 10);

		// Act
		var result = _tqData.ReadFloatAfter(data, "health");

		// Assert - indexOf is compensated to point to where key-without-prefix starts (position 4)
		result.indexOf.Should().Be(4);
		result.valueAsFloat.Should().BeApproximately(99.5f, 0.001f);
	}

	[Fact]
	public void ReadFloatAfter_KeyNotFound_ReturnsNegativeOne()
	{
		// Arrange
		var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };

		// Act
		var result = _tqData.ReadFloatAfter(data, "missing");

		// Assert
		result.indexOf.Should().Be(-1);
	}

	#endregion

	#region ReadCStringAfter Tests

	[Fact]
	public void ReadCStringAfter_KeyExists_ReturnsCorrectValue()
	{
		// Arrange: Data with key-with-prefix at position 0 for BinaryFindKey to find it
		var data = new byte[100];
		var keyBytes = TQDataService.Encoding1252.GetBytes("name");
		// Write key length (4 bytes) + key bytes at position 0
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), keyBytes.Length);
		keyBytes.CopyTo(data, 4);
		// String value follows the key-with-prefix
		var stringValue = "TestString";
		var stringBytes = TQDataService.Encoding1252.GetBytes(stringValue);
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(8, 4), stringBytes.Length);
		stringBytes.CopyTo(data, 12);

		// Act
		var result = _tqData.ReadCStringAfter(data, "name");

		// Assert - indexOf is compensated to point to where key-without-prefix starts (position 4)
		result.indexOf.Should().Be(4);
		result.valueAsString.Should().Be(stringValue);
		result.valueLen.Should().Be(stringBytes.Length);
	}

	[Fact]
	public void ReadCStringAfter_KeyNotFound_ReturnsNegativeOne()
	{
		// Arrange
		var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };

		// Act
		var result = _tqData.ReadCStringAfter(data, "missing");

		// Assert
		result.indexOf.Should().Be(-1);
		result.valueAsString.Should().BeEmpty();
	}

	[Fact]
	public void ReadCStringAfter_EmptyString_ReturnsEmpty()
	{
		// Arrange
		var data = new byte[100];
		var key = TQDataService.Encoding1252.GetBytes("name");
		key.CopyTo(data, 0);
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(8, 4), 0); // Empty string

		// Act
		var result = _tqData.ReadCStringAfter(data, "name");

		// Assert
		result.valueAsString.Should().BeEmpty();
		result.valueLen.Should().Be(0);
	}

	#endregion

	#region ReadUnicodeStringAfter Tests

	[Fact]
	public void ReadUnicodeStringAfter_KeyExists_ReturnsCorrectValue()
	{
		// Arrange: Data with "name" key followed by unicode string
		// BinaryFindKey(string) prepends length, so layout is: [len(4 bytes)][key bytes][charCount(4 bytes)][unicode bytes]
		var data = new byte[100];
		var key = TQDataService.Encoding1252.GetBytes("name");
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), key.Length);
		key.CopyTo(data, 4);
		var stringValue = "Test";
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(8, 4), stringValue.Length); // char count
		Encoding.Unicode.GetBytes(stringValue, 0, stringValue.Length, data, 12);

		// Act
		var result = _tqData.ReadUnicodeStringAfter(data, "name");

		// Assert
		result.indexOf.Should().Be(4);
		result.valueAsString.Should().Be(stringValue);
	}

	[Fact]
	public void ReadUnicodeStringAfter_KeyNotFound_ReturnsNegativeOne()
	{
		// Arrange
		var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };

		// Act
		var result = _tqData.ReadUnicodeStringAfter(data, "missing");

		// Assert
		result.indexOf.Should().Be(-1);
	}

	#endregion

	#region BinaryFindEndBlockOf Tests

	[Fact]
	public void BinaryFindEndBlockOf_NestedBlocks_FindsCorrectEnd()
	{
		// Arrange: Simple case with single begin_block -> end_block
		// Data layout uses BinaryFindKey(string) which prepends length
		var data = new byte[200];
		var testKey = TQDataService.Encoding1252.GetBytes("testKey");
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), testKey.Length);
		testKey.CopyTo(data, 4);

		// begin_block at position 20 (need 4 bytes for length + 10 bytes for "begin_block" = 14 bytes)
		var beginBlock = TQDataService.Encoding1252.GetBytes("begin_block");
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(20, 4), beginBlock.Length);
		beginBlock.CopyTo(data, 24);

		// end_block at position 45 (need 4 bytes for length + 8 bytes for "end_block" = 12 bytes, at 45+12=57)
		var endBlock = TQDataService.Encoding1252.GetBytes("end_block");
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(45, 4), endBlock.Length);
		endBlock.CopyTo(data, 49);

		// Act - Find end block of "testKey"
		var result = _tqData.BinaryFindEndBlockOf(data, "testKey");

		// Assert - should find end_block at position 45
		result.indexOf.Should().Be(49);
	}

	[Fact]
	public void BinaryFindEndBlockOf_KeyNotFound_ReturnsNegativeOne()
	{
		// Arrange
		var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };

		// Act
		var result = _tqData.BinaryFindEndBlockOf(data, "missing");

		// Assert
		result.indexOf.Should().Be(-1);
	}

	[Fact]
	public void BinaryFindEndBlockOf_KeyExistsButNoEndBlock_ReturnsNegativeOne()
	{
		// Arrange: Key exists but no end_block
		var data = new byte[50];
		var testKey = TQDataService.Encoding1252.GetBytes("testKey");
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), testKey.Length);
		testKey.CopyTo(data, 4);
		// No end_block

		// Act
		var result = _tqData.BinaryFindEndBlockOf(data, "testKey");

		// Assert
		result.indexOf.Should().Be(-1);
	}

	#endregion

	#region RemoveCStringValueAfter Tests

	[Fact]
	public void RemoveCStringValueAfter_KeyExists_RemovesValue()
	{
		// Arrange: Data with key-with-prefix at position 0 for BinaryFindKey to find it
		var original = new byte[100];
		var keyBytes = TQDataService.Encoding1252.GetBytes("name");
		// Write key length (4 bytes) + key bytes at position 0
		BinaryPrimitives.WriteInt32LittleEndian(original.AsSpan(0, 4), keyBytes.Length);
		keyBytes.CopyTo(original, 4);
		var stringBytes = TQDataService.Encoding1252.GetBytes("test");
		BinaryPrimitives.WriteInt32LittleEndian(original.AsSpan(8, 4), stringBytes.Length);
		stringBytes.CopyTo(original, 12);
		// Rest of data
		original[16] = 0xFF;

		// Act
		var result = _tqData.RemoveCStringValueAfter(ref original, "name");

		// Assert
		result.Should().BeTrue();
		// Value length should be 0 (removed)
		BinaryPrimitives.ReadInt32LittleEndian(original.AsSpan(8, 4)).Should().Be(0);
	}

	[Fact]
	public void RemoveCStringValueAfter_KeyNotFound_ReturnsFalse()
	{
		// Arrange
		var original = new byte[] { 0x01, 0x02, 0x03, 0x04 };

		// Act
		var result = _tqData.RemoveCStringValueAfter(ref original, "missing");

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	#region ReplaceUnicodeValueAfter Tests

	[Fact]
	public void ReplaceUnicodeValueAfter_KeyExists_ReplacesValue()
	{
		// Arrange: Data with key-with-prefix at position 0 for BinaryFindKey to find it
		var original = new byte[100];
		var keyBytes = TQDataService.Encoding1252.GetBytes("name");
		// Write key length (4 bytes) + key bytes at position 0
		BinaryPrimitives.WriteInt32LittleEndian(original.AsSpan(0, 4), keyBytes.Length);
		keyBytes.CopyTo(original, 4);
		var oldString = "Old";
		var oldBytes = Encoding.Unicode.GetBytes(oldString);
		BinaryPrimitives.WriteInt32LittleEndian(original.AsSpan(8, 4), oldString.Length);
		oldBytes.CopyTo(original, 12);
		// Rest of data
		original[18] = 0xFF;

		// Act
		var result = _tqData.ReplaceUnicodeValueAfter(ref original, "name", "New");

		// Assert
		result.Should().BeTrue();
		// New string should be "New" (3 chars = 6 bytes)
		var newLen = BinaryPrimitives.ReadInt32LittleEndian(original.AsSpan(8, 4));
		newLen.Should().Be(3);
	}

	[Fact]
	public void ReplaceUnicodeValueAfter_KeyNotFound_ReturnsFalse()
	{
		// Arrange
		var original = new byte[] { 0x01, 0x02, 0x03, 0x04 };

		// Act
		var result = _tqData.ReplaceUnicodeValueAfter(ref original, "missing", "New");

		// Assert
		result.Should().BeFalse();
	}

	#endregion
}
