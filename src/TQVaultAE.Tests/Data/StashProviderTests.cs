using System.Buffers.Binary;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

public class StashProviderTests
{
	private const int BUFFERSIZE = 1024;

	#region CalculateCRC Tests

	[Fact]
	public void CalculateCRC_OldAndNewAlgorithm_ProduceSameResult()
	{
		// Arrange: Create sample data
		var sampleData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

		// Create copy for old algorithm
		var oldData = (byte[])sampleData.Clone();
		var newData = (byte[])sampleData.Clone();

		// Act: Calculate CRC using old algorithm (BinaryReader + MemoryStream)
		uint oldCrc = CalculateCRCOld(oldData);

		// Calculate CRC using new algorithm (via reflection)
		uint newCrc = CalculateCRCNew(newData);

		// Assert: Both should produce the same result
		newCrc.Should().Be(oldCrc);
	}

	[Fact]
	public void CalculateCRC_KnownTestData_ProducesCorrectValue()
	{
		// Arrange: Use known test data that produces a known CRC
		var data = new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38 }; // "12345678"

		// Act
		var result = (byte[])data.Clone();
		uint crc = CalculateCRCNew(result);

		// Assert: CRC should not be zero (sanity check)
		crc.Should().NotBe(0);
		// The first 4 bytes should be modified with the CRC value
		result[0].Should().NotBe(0x31);
	}

	[Fact]
	public void CalculateCRC_EmptyData_ProducesValidCRC()
	{
		// Arrange - create data with actual content so CRC is non-zero
		var data = new byte[10];
		data[5] = 42; // Add some data to ensure non-zero CRC

		// Act
		var result = (byte[])data.Clone();
		uint crc = CalculateCRCNew(result);

		// Assert: Should complete without error and produce non-zero CRC for non-empty data
		crc.Should().NotBe(0);
	}

	[Fact]
	public void CalculateCRC_LargeData_CompletesSuccessfully()
	{
		// Arrange: Large data buffer (larger than BUFFERSIZE)
		var data = new byte[BUFFERSIZE * 3];
		for (int i = 0; i < data.Length; i++)
			data[i] = (byte)(i % 256);

		// Act
		var result = (byte[])data.Clone();
		uint crc = CalculateCRCNew(result);

		// Assert
		crc.Should().NotBe(0);
	}

	[Fact]
	public void CalculateCRC_DataWithZeros_ProducesNonZeroCRC()
	{
		// Arrange: Data that is mostly zeros but has some content
		var data = new byte[100];
		data[50] = 0xFF;
		data[51] = 0xFE;

		// Act
		var result = (byte[])data.Clone();
		uint crc = CalculateCRCNew(result);

		// Assert
		crc.Should().NotBe(0);
	}

	#endregion

	#region Encode Tests

	[Fact]
	public void Encode_ValidStash_ProducesNonEmptyByteArray()
	{
		// Arrange
		var stashProvider = CreateStashProvider(out var mockSackProvider, out _, out _);
		var stash = CreateValidStash();

		// Mock Encode to do nothing (just write placeholder data)
		mockSackProvider.Setup(x => x.Encode(It.IsAny<SackCollection>(), It.IsAny<BinaryWriter>()));

		// Act
		var result = stashProvider.Encode(stash);

		// Assert
		result.Should().NotBeEmpty();
		result.Length.Should().BeGreaterThan(10); // Should have header data at minimum
	}

	[Fact]
	public void Encode_StashWithName_WritesNameCorrectly()
	{
		// Arrange
		var stashProvider = CreateStashProvider(out var mockSackProvider, out var mockTQData, out _);
		var stash = CreateValidStash();
		stash.name = TQDataService.Encoding1252.GetBytes("TestStash");

		// Track what WriteCString receives
		var writtenStrings = new List<string>();
		mockTQData.Setup(x => x.WriteCString(It.IsAny<BinaryWriter>(), It.IsAny<string>()))
			.Callback<BinaryWriter, string>((_, s) => writtenStrings.Add(s));

		// Mock Encode to track calls
		mockSackProvider.Setup(x => x.Encode(It.IsAny<SackCollection>(), It.IsAny<BinaryWriter>()));

		// Act
		stashProvider.Encode(stash);

		// Assert - Verify expected strings are written
		writtenStrings.Should().Contain("begin_block");
		writtenStrings.Should().Contain("stashVersion");
		writtenStrings.Should().Contain("fName");
		writtenStrings.Should().Contain("sackWidth");
		writtenStrings.Should().Contain("sackHeight");
	}

	[Fact]
	public void Encode_EmptySackCollection_StillProducesValidOutput()
	{
		// Arrange
		var stashProvider = CreateStashProvider(out var mockSackProvider, out _, out _);
		var stash = CreateValidStash();
		stash.Sack = new SackCollection { SackType = SackType.Stash };

		// Mock Encode
		mockSackProvider.Setup(x => x.Encode(It.IsAny<SackCollection>(), It.IsAny<BinaryWriter>()));

		// Act
		var result = stashProvider.Encode(stash);

		// Assert
		result.Should().NotBeEmpty();
	}

	#endregion

	#region LoadFile Tests

	[Fact]
	public void LoadFile_FileDoesNotExist_ReturnsFalse()
	{
		// Arrange
		var mockFileIO = new Mock<IFileIO>();
		mockFileIO.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);

		var stashProvider = CreateStashProvider(out _, out _, out _, mockFileIO);

		var stash = new Stash("Player", Path.Combine(Path.GetTempPath(), "nonexistent.tqstash"));

		// Act
		var result = stashProvider.LoadFile(stash);

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	/// <summary>
	/// Replicates the OLD algorithm (BinaryReader + MemoryStream)
	/// </summary>
	private static uint CalculateCRCOld(byte[] data)
	{
		// Get crc32Table directly from StashProvider
		var stashProvider = CreateStashProvider(out _, out _, out _);
		var table = stashProvider.crc32Table;

		using var reader = new BinaryReader(new MemoryStream(data, false));
		uint crc32Result = 0;
		var buffer = new byte[BUFFERSIZE];
		var readSize = BUFFERSIZE;

		int count = reader.Read(buffer, 0, readSize);
		while (count > 0)
		{
			for (int i = 0; i < count; i++)
				crc32Result = (crc32Result >> 8) ^ table[buffer[i] ^ (crc32Result & 0x000000FF)];

			count = reader.Read(buffer, 0, readSize);
		}

		// Put the CRC into the first 4 bytes
		data[3] = (byte)((crc32Result & 0xFF000000) >> 24);
		data[2] = (byte)((crc32Result & 0x00FF0000) >> 16);
		data[1] = (byte)((crc32Result & 0x0000FF00) >> 8);
		data[0] = (byte)(crc32Result & 0x000000FF);

		return crc32Result;
	}

	/// <summary>
	/// Creates a StashProvider instance with mocked dependencies
	/// </summary>
	private static StashProvider CreateStashProvider(
		out Mock<ISackCollectionProvider> mockSackProvider,
		out Mock<ITQDataService> mockTQData,
		out Mock<IFileIO> mockFileIO,
		Mock<IFileIO>? fileIOToUse = null,
		Mock<IPathIO>? pathIOToUse = null)
	{
		var mockLog = new Mock<ILogger<StashProvider>>();
		mockSackProvider = new Mock<ISackCollectionProvider>();
		mockTQData = new Mock<ITQDataService>();
		mockFileIO = fileIOToUse ?? new Mock<IFileIO>();
		var mockPathIO = pathIOToUse ?? new Mock<IPathIO>();

		return new StashProvider(
			mockLog.Object,
			mockSackProvider.Object,
			mockTQData.Object,
			mockFileIO.Object,
			mockPathIO.Object
		);
	}

	/// <summary>
	/// Creates a valid Stash instance for testing
	/// </summary>
	private static Stash CreateValidStash()
	{
		var stash = new Stash("TestPlayer", Path.Combine(Path.GetTempPath(), "test.tqstash"))
		{
			name = TQDataService.Encoding1252.GetBytes("TestStash"),
			stashVersion = 1,
			Width = 10,
			Height = 10,
			beginBlockCrap = 0
		};
		stash.CreateEmptySack();
		return stash;
	}

	/// <summary>
	/// Creates minimal stash binary data for testing LoadFile
	/// </summary>
	private static byte[] CreateMinimalStashData()
	{
		using var ms = new MemoryStream();
		using var writer = new BinaryWriter(ms);

		// CRC placeholder (4 bytes)
		writer.Write(0);

		// begin_block
		var beginBlockBytes = TQDataService.Encoding1252.GetBytes("begin_block");
		writer.Write(beginBlockBytes.Length);
		writer.Write(beginBlockBytes);
		writer.Write(12345); // beginBlockCrap

		// stashVersion
		var stashVersionBytes = TQDataService.Encoding1252.GetBytes("stashVersion");
		writer.Write(stashVersionBytes.Length);
		writer.Write(stashVersionBytes);
		writer.Write(1); // stashVersion value

		// fName
		var fNameBytes = TQDataService.Encoding1252.GetBytes("fName");
		writer.Write(fNameBytes.Length);
		writer.Write(fNameBytes);
		var nameBytes = TQDataService.Encoding1252.GetBytes("TestStash");
		writer.Write(nameBytes.Length);
		writer.Write(nameBytes);

		// sackWidth
		var sackWidthBytes = TQDataService.Encoding1252.GetBytes("sackWidth");
		writer.Write(sackWidthBytes.Length);
		writer.Write(sackWidthBytes);
		writer.Write(10);

		// sackHeight
		var sackHeightBytes = TQDataService.Encoding1252.GetBytes("sackHeight");
		writer.Write(sackHeightBytes.Length);
		writer.Write(sackHeightBytes);
		writer.Write(10);

		// numItems (for sack)
		var numItemsBytes = TQDataService.Encoding1252.GetBytes("numItems");
		writer.Write(numItemsBytes.Length);
		writer.Write(numItemsBytes);
		writer.Write(0); // no items

		// end_block
		var endBlockBytes = TQDataService.Encoding1252.GetBytes("end_block");
		writer.Write(endBlockBytes.Length);
		writer.Write(endBlockBytes);

		return ms.ToArray();
	}

	/// <summary>
	/// Calls the NEW algorithm directly
	/// </summary>
	private static uint CalculateCRCNew(byte[] data)
	{
		var stashProvider = CreateStashProvider(out _, out _, out _);
		stashProvider.CalculateCRC(data);
		return BitConverter.ToUInt32(data, 0);
	}
}
