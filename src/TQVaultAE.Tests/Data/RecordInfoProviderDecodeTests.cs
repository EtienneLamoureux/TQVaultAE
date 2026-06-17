using System.Buffers.Binary;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

public class RecordInfoProviderDecodeTests
{
	private readonly RecordInfoProvider _provider;
	private readonly Mock<ILogger<RecordInfoProvider>> _mockLogger;
	private readonly TQDataService _tqData;
	private readonly Mock<IFileDataService> _mockFileData;
	private readonly Mock<IDecompressionService> _mockDecompression;

	public RecordInfoProviderDecodeTests()
	{
		_mockLogger = new Mock<ILogger<RecordInfoProvider>>();
		_tqData = new TQDataService(Mock.Of<ILogger<TQDataService>>());
		_mockFileData = new Mock<IFileDataService>();
		_mockDecompression = new Mock<IDecompressionService>();

		_provider = new RecordInfoProvider(
			_mockLogger.Object,
			_tqData,
			_mockFileData.Object,
			_mockDecompression.Object);
	}

	[Fact]
	public void Decode_Span_MinimalRecord_ParsesCorrectly()
	{
		// Arrange: Minimal record data (20 bytes)
		// Format: idStringIndex(4) + recordType + offset(4) + compressedSize(4) + padding(8)
		var recordType = "Items";
		var recordTypeBytes = TQDataService.Encoding1252.GetBytes(recordType);
		var data = new byte[4 + 4 + recordTypeBytes.Length + 4 + 4 + 4 + 4];
		int pos = 0;

		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos), 123); // idStringIndex
		pos += 4;
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos), recordTypeBytes.Length); // recordType length
		pos += 4;
		recordTypeBytes.CopyTo(data, pos); // recordType
		pos += recordTypeBytes.Length;
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos), 1000); // offset
		pos += 4;
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos), 500); // compressedSize
		pos += 4;
		// Skip 8 bytes for 2 x int32 padding

		var arzFile = CreateMockArzFile();
		var info = new RecordInfo();
		ReadOnlySpan<byte> span = data;
		int offset = 0;
		int baseOffset = 50;

		// Act
		_provider.Decode(info, span, ref offset, baseOffset, arzFile);

		// Assert
		info.IdStringIndex.Should().Be(123);
		info.RecordType.Should().Be(recordType);
		info.Offset.Should().Be(1000 + baseOffset); // 1000 + 50
		info.CompressedSize.Should().Be(500);
		offset.Should().Be(data.Length);
	}

	[Fact]
	public void Decode_Span_OldAndNewAlgorithm_ProduceSameResult()
	{
		// Arrange: Create record data with various record types
		var recordTypes = new[] { "Items", "Skills", "NPCs", "Quests" };

		foreach (var recordType in recordTypes)
		{
			var recordTypeBytes = TQDataService.Encoding1252.GetBytes(recordType);
			var data = new byte[4 + 4 + recordTypeBytes.Length + 4 + 4 + 4 + 4];
			int pos = 0;
			int idStringIndex = 42;
			int recordOffset = 12345;
			int compressedSize = 999;

			BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos), idStringIndex);
			pos += 4;
			BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos), recordTypeBytes.Length);
			pos += 4;
			recordTypeBytes.CopyTo(data, pos);
			pos += recordTypeBytes.Length;
			BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos), recordOffset);
			pos += 4;
			BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos), compressedSize);
			pos += 4;
			// Skip 8 bytes padding

			var arzFile = CreateMockArzFile();
			int baseOffset = 100;

			// Act: OLD method using BinaryReader
			var oldInfo = DecodeOldMethod(data, baseOffset, arzFile);

			// NEW method using ReadOnlySpan
			var newInfo = new RecordInfo();
			ReadOnlySpan<byte> span = data;
			int offset = 0;
			_provider.Decode(newInfo, span, ref offset, baseOffset, arzFile);

			// Assert
			newInfo.IdStringIndex.Should().Be(oldInfo.IdStringIndex, $"RecordType: {recordType}");
			newInfo.RecordType.Should().Be(oldInfo.RecordType, $"RecordType: {recordType}");
			newInfo.Offset.Should().Be(oldInfo.Offset, $"RecordType: {recordType}");
			newInfo.CompressedSize.Should().Be(oldInfo.CompressedSize, $"RecordType: {recordType}");
		}
	}

	[Fact]
	public void Decode_Span_MultipleRecordsInSequence_ParsesAll()
	{
		// Arrange: 3 records in sequence
		var recordTypes = new[] { "RecordA", "RecordB", "RecordC" };
		var totalDataSize = 0;
		var recordDataList = new List<byte[]>();

		foreach (var rt in recordTypes)
		{
			var rtBytes = TQDataService.Encoding1252.GetBytes(rt);
			var recordData = new byte[4 + 4 + rtBytes.Length + 4 + 4 + 4 + 4];
			int pos = 0;
			BinaryPrimitives.WriteInt32LittleEndian(recordData.AsSpan(pos), 100 + pos);
			pos += 4;
			BinaryPrimitives.WriteInt32LittleEndian(recordData.AsSpan(pos), rtBytes.Length);
			pos += 4;
			rtBytes.CopyTo(recordData, pos);
			pos += rtBytes.Length;
			BinaryPrimitives.WriteInt32LittleEndian(recordData.AsSpan(pos), 1000 + pos);
			pos += 4;
			BinaryPrimitives.WriteInt32LittleEndian(recordData.AsSpan(pos), 500 + pos);
			recordDataList.Add(recordData);
			totalDataSize += recordData.Length;
		}

		// Concatenate all record data
		var combinedData = new byte[totalDataSize];
		int copyPos = 0;
		foreach (var rd in recordDataList)
		{
			rd.CopyTo(combinedData, copyPos);
			copyPos += rd.Length;
		}

		var arzFile = CreateMockArzFile();
		ReadOnlySpan<byte> span = combinedData;
		int offset = 0;

		// Act: Parse all 3 records
		var infos = new List<RecordInfo>();
		for (int i = 0; i < 3; i++)
		{
			var info = new RecordInfo();
			_provider.Decode(info, span, ref offset, 0, arzFile);
			infos.Add(info);
		}

		// Assert
		infos.Should().HaveCount(3);
		infos[0].RecordType.Should().Be("RecordA");
		infos[1].RecordType.Should().Be("RecordB");
		infos[2].RecordType.Should().Be("RecordC");
		offset.Should().Be(totalDataSize);
	}

	[Fact]
	public void Decode_Span_WithBaseOffset_AddsCorrectly()
	{
		// Arrange
		var recordType = "Test";
		var rtBytes = TQDataService.Encoding1252.GetBytes(recordType);
		var data = new byte[4 + 4 + rtBytes.Length + 4 + 4 + 4 + 4];
		int pos = 0;
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos), 1);
		pos += 4;
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos), rtBytes.Length);
		pos += 4;
		rtBytes.CopyTo(data, pos);
		pos += rtBytes.Length;
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos), 1000); // relative offset
		pos += 4;
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos), 200);
		pos += 4;

		var arzFile = CreateMockArzFile();

		// Act & Assert: Test with different base offsets
		int[] baseOffsets = { 0, 50, 100, 1000, -500 };

		foreach (var baseOffset in baseOffsets)
		{
			var info = new RecordInfo();
			ReadOnlySpan<byte> span = data;
			int offset = 0;
			_provider.Decode(info, span, ref offset, baseOffset, arzFile);

			info.Offset.Should().Be(1000 + baseOffset, $"With baseOffset={baseOffset}");
		}
	}

	/// <summary>
	/// OLD method: BinaryReader + MemoryStream
	/// </summary>
	private static RecordInfo DecodeOldMethod(byte[] data, int baseOffset, ArzFile arzFile)
	{
		var tqData = new TQDataService(Mock.Of<ILogger<TQDataService>>());
		using var reader = new BinaryReader(new MemoryStream(data, false));

		var info = new RecordInfo();
		info.IdStringIndex = reader.ReadInt32();
		info.RecordType = tqData.ReadCString(reader);
		info.Offset = reader.ReadInt32() + baseOffset;
		info.CompressedSize = reader.ReadInt32();
		reader.ReadInt32(); // skip
		reader.ReadInt32(); // skip
		info.ID = arzFile.Getstring(info.IdStringIndex);

		return info;
	}

	private static ArzFile CreateMockArzFile()
	{
		var arzFile = new ArzFile("test.arz");
		// Add mock string table
		arzFile.Strings = new string[200];
		for (int i = 0; i < 200; i++)
		{
			arzFile.Strings[i] = $"String_{i}";
		}
		return arzFile;
	}
}
