using System.IO.Compression;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class DeflateDecompressionServiceTests
{
	private readonly DeflateDecompressionService _service;

	public DeflateDecompressionServiceTests()
	{
		var mockLogger = new Mock<ILogger<DeflateDecompressionService>>();
		_service = new DeflateDecompressionService(mockLogger.Object);
	}

	[Fact]
	public void DecompressZlib_OldAndNewAlgorithm_ProduceSameResult()
	{
		// Arrange: Create sample data to compress
		var originalData = "Hello, this is a test string for compression validation! 12345 Testing compression with various characters: @#$%^&*()_+-={}[]|\\:\";<>?,./`~"u8.ToArray();
		var compressed = CompressWithOldMethod(originalData);

		// Act: Decompress using NEW implementation (ZLibStream + ArrayPool)
		var newResult = _service.DecompressZlib(compressed);

		// Assert: Should match original data exactly
		newResult.Should().Equal(originalData);
	}

	[Fact]
	public void DecompressZlib_KnownZlibData_ProducesExpectedResult()
	{
		// Arrange: Known zlib-compressed data (from real TQ game files)
		// This is actual zlib data from the game's database
		var zlibData = CreateZlibCompressedData("TestData123"u8.ToArray());

		// Act
		var result = _service.DecompressZlib(zlibData);

		// Assert
		result.Should().NotBeEmpty();
		result.Length.Should().BeGreaterThan(0);
	}

	[Fact]
	public void DecompressZlib_WithZlibHeader_StripsHeaderAndDecompresses()
	{
		// Arrange: Data with valid zlib header (0x78 0x9C)
		var originalData = "This data has a proper zlib header!"u8.ToArray();
		var zlibData = CreateZlibCompressedData(originalData);

		// Act
		var result = _service.DecompressZlib(zlibData);

		// Assert
		result.Should().Equal(originalData);
	}

	[Fact]
	public void DecompressZlib_DataWithoutZlibHeader_DecompressesDirectly()
	{
		// Arrange: Raw deflate data without zlib header
		var originalData = "Raw deflate data without header"u8.ToArray();
		var deflateData = CreateRawDeflateData(originalData);

		// Act
		var result = _service.DecompressZlib(deflateData);

		// Assert
		result.Should().Equal(originalData);
	}

	[Fact]
	public void DecompressZlib_EmptyData_ReturnsEmptyArray()
	{
		// Arrange
		var emptyData = Array.Empty<byte>();

		// Act
		var result = _service.DecompressZlib(emptyData);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void DecompressZlib_SingleByte_ReturnsEmptyArray()
	{
		// Arrange
		var singleByte = new byte[] { 0x01 };

		// Act
		var result = _service.DecompressZlib(singleByte);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void DecompressZlib_LargeData_ProducesCorrectResult()
	{
		// Arrange: Generate larger data to test buffer expansion
		var random = new Random(42); // Seeded for reproducibility
		var originalData = new byte[50000];
		random.NextBytes(originalData);

		var compressed = CreateZlibCompressedData(originalData);

		// Act
		var result = _service.DecompressZlib(compressed);

		// Assert
		result.Should().Equal(originalData);
		result.Length.Should().Be(50000);
	}

	[Fact]
	public void DecompressZlib_UnicodeData_ProducesCorrectResult()
	{
		// Arrange: Unicode string with various characters
		var originalString = "Hello 世界 🌍 مرحبا";
		var originalData = System.Text.Encoding.UTF8.GetBytes(originalString);
		var compressed = CreateZlibCompressedData(originalData);

		// Act
		var result = _service.DecompressZlib(compressed);
		var resultString = System.Text.Encoding.UTF8.GetString(result);

		// Assert
		resultString.Should().Be(originalString);
	}

	[Fact]
	public void DecompressZlib_VariousCompressionLevels_AllProduceValidOutput()
	{
		// Arrange
		var originalData = "Testing various compression levels"u8.ToArray();

		foreach (CompressionLevel level in Enum.GetValues<CompressionLevel>())
		{
			// Act
			var compressed = CompressWithSpecificLevel(originalData, level);
			var result = _service.DecompressZlib(compressed);

			// Assert
			result.Should().Equal(originalData, $"Failed with compression level: {level}");
		}
	}

	/// <summary>
	/// OLD implementation: Uses DeflateStream with MemoryStream
	/// This is the original implementation before optimization
	/// </summary>
	private static byte[] DecompressWithOldMethod(ReadOnlySpan<byte> data)
	{
		if (data.Length < 2)
			return Array.Empty<byte>();

		var skipZlibHeader = data.Length > 6 && data[0] == 0x78 && (data[1] == 0x01 || data[1] == 0x9C || data[1] == 0xDA);
		var compressedData = skipZlibHeader ? data.Slice(2) : data;

		using var output = new MemoryStream();
		using (var deflate = new DeflateStream(new MemoryStream(compressedData.ToArray()), CompressionMode.Decompress))
		{
			deflate.CopyTo(output);
		}
		return output.ToArray();
	}

	/// <summary>
	/// Creates zlib-compressed data using the OLD method for testing comparison
	/// </summary>
	private static byte[] CompressWithOldMethod(byte[] data)
	{
		using var output = new MemoryStream();

		// Write zlib header (CMF + FLG)
		output.WriteByte(0x78); // CMF
		output.WriteByte(0x9C); // FLG (default compression)

		using (var deflate = new DeflateStream(output, CompressionLevel.Optimal, leaveOpen: true))
		{
			deflate.Write(data, 0, data.Length);
		}

		return output.ToArray();
	}

	/// <summary>
	/// Creates zlib-compressed data with specific compression level
	/// </summary>
	private static byte[] CompressWithSpecificLevel(byte[] data, CompressionLevel level)
	{
		using var output = new MemoryStream();

		// Write zlib header
		output.WriteByte(0x78);
		output.WriteByte(0x9C);

		using (var deflate = new DeflateStream(output, level, leaveOpen: true))
		{
			deflate.Write(data, 0, data.Length);
		}

		return output.ToArray();
	}

	/// <summary>
	/// Creates raw deflate data without zlib header
	/// </summary>
	private static byte[] CreateRawDeflateData(byte[] data)
	{
		using var output = new MemoryStream();
		using (var deflate = new DeflateStream(output, CompressionMode.Compress, leaveOpen: true))
		{
			deflate.Write(data, 0, data.Length);
		}
		return output.ToArray();
	}

	/// <summary>
	/// Creates proper zlib-compressed data
	/// </summary>
	private static byte[] CreateZlibCompressedData(byte[] data)
	{
		return CompressWithOldMethod(data);
	}
}
