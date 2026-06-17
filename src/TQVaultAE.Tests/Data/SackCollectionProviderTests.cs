using System.Buffers.Binary;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

public class SackCollectionProviderTests
{
	private readonly SackCollectionProvider _provider;
	private readonly Mock<ILogger<SackCollectionProvider>> _mockLogger;
	private readonly TQDataService _tqData;
	private readonly Mock<IItemProvider> _mockItemProvider;

	public SackCollectionProviderTests()
	{
		_mockLogger = new Mock<ILogger<SackCollectionProvider>>();
		_tqData = new TQDataService(Mock.Of<ILogger<TQDataService>>());
		_mockItemProvider = new Mock<IItemProvider>();

		_provider = new SackCollectionProvider(
			_mockLogger.Object,
			_mockItemProvider.Object,
			_tqData);
	}

	#region ParseHeader Tests

	[Fact]
	public void ParseHeader_StashFormat_ParsesCorrectly()
	{
		// Arrange: Stash format: "numItems" + size
		var recordType = "numItems";
		var recordTypeBytes = TQDataService.Encoding1252.GetBytes(recordType);
		var expectedSize = 42;
		var data = new byte[4 + recordTypeBytes.Length + 4];
		int pos = 0;

		// "numItems"
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos, 4), recordTypeBytes.Length);
		pos += 4;
		recordTypeBytes.CopyTo(data, pos);
		pos += recordTypeBytes.Length;

		// size
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos, 4), expectedSize);

		var sc = new SackCollection { SackType = SackType.Stash };
		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var bytesConsumed = _provider.ParseHeader(sc, span, ref offset);

		// Assert
		sc.size.Should().Be(expectedSize);
		offset.Should().Be(data.Length);
		bytesConsumed.Should().Be(data.Length);
	}

	[Fact]
	public void ParseHeader_EquipmentFormat_SetsCorrectSize()
	{
		// Arrange: Equipment format has fixed size
		var data = Array.Empty<byte>();

		var sc = new SackCollection { SackType = SackType.Equipment, IsImmortalThrone = false };
		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var bytesConsumed = _provider.ParseHeader(sc, span, ref offset);

		// Assert
		sc.size.Should().Be(11);
		sc.slots.Should().Be(11);
		bytesConsumed.Should().Be(0);
	}

	[Fact]
	public void ParseHeader_EquipmentIT_SetsCorrectSize()
	{
		// Arrange: Equipment format for Immortal Throne has fixed size
		var data = Array.Empty<byte>();

		var sc = new SackCollection { SackType = SackType.Equipment, IsImmortalThrone = true };
		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var bytesConsumed = _provider.ParseHeader(sc, span, ref offset);

		// Assert
		sc.size.Should().Be(12);
		sc.slots.Should().Be(12);
		bytesConsumed.Should().Be(0);
	}

	[Fact]
	public void ParseHeader_PlayerSackFormat_ParsesCorrectly()
	{
		// Arrange: Player sack format: begin_block + int + tempBool + int + size + int
		var data = new List<byte>();

		// "begin_block"
		var beginBlockBytes = TQDataService.Encoding1252.GetBytes("begin_block");
		data.AddRange(BitConverter.GetBytes(beginBlockBytes.Length));
		data.AddRange(beginBlockBytes);
		data.AddRange(BitConverter.GetBytes(12345)); // beginBlockCrap

		// "tempBool"
		var tempBoolBytes = TQDataService.Encoding1252.GetBytes("tempBool");
		data.AddRange(BitConverter.GetBytes(tempBoolBytes.Length));
		data.AddRange(tempBoolBytes);
		data.AddRange(BitConverter.GetBytes(1)); // tempBool

		// "size"
		var sizeBytes = TQDataService.Encoding1252.GetBytes("size");
		data.AddRange(BitConverter.GetBytes(sizeBytes.Length));
		data.AddRange(sizeBytes);
		var expectedSize = 25;
		data.AddRange(BitConverter.GetBytes(expectedSize));

		var sc = new SackCollection { SackType = SackType.Player };
		ReadOnlySpan<byte> span = data.ToArray();
		int offset = 0;

		// Act
		var bytesConsumed = _provider.ParseHeader(sc, span, ref offset);

		// Assert
		sc.beginBlockCrap.Should().Be(12345);
		sc.tempBool.Should().Be(1);
		sc.size.Should().Be(expectedSize);
		offset.Should().Be(data.Count);
	}

	[Fact]
	public void ParseHeader_OldAndNewAlgorithm_ProduceSameResult()
	{
		// Test with stash format
		var recordType = "numItems";
		var recordTypeBytes = TQDataService.Encoding1252.GetBytes(recordType);
		var data = new byte[4 + recordTypeBytes.Length + 4];
		int pos = 0;
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos, 4), recordTypeBytes.Length);
		pos += 4;
		recordTypeBytes.CopyTo(data, pos);
		pos += recordTypeBytes.Length;
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos, 4), 42);

		var sc1 = new SackCollection { SackType = SackType.Stash };
		var sc2 = new SackCollection { SackType = SackType.Stash };

		// OLD method
		int offset1 = 0;
		ParseHeaderOldMethod(sc1, data, ref offset1);

		// NEW method
		ReadOnlySpan<byte> span = data;
		int offset2 = 0;
		_provider.ParseHeader(sc2, span, ref offset2);

		// Assert
		sc1.size.Should().Be(sc2.size);
		sc1.beginBlockCrap.Should().Be(sc2.beginBlockCrap);
		sc1.tempBool.Should().Be(sc2.tempBool);
	}

	[Fact]
	public void ParseHeader_InvalidTag_ThrowsArgumentException()
	{
		// Arrange: Wrong tag for stash
		var recordType = "wrongTag";
		var recordTypeBytes = TQDataService.Encoding1252.GetBytes(recordType);
		var data = new byte[4 + recordTypeBytes.Length];
		int pos = 0;
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos, 4), recordTypeBytes.Length);
		pos += 4;
		recordTypeBytes.CopyTo(data, pos);

		var sc = new SackCollection { SackType = SackType.Stash };
		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act & Assert - use try-catch to avoid ref local issues with Record.Exception
		ArgumentException? caught = null;
		try
		{
			_provider.ParseHeader(sc, span, ref offset);
		}
		catch (ArgumentException ex)
		{
			caught = ex;
		}

		caught.Should().NotBeNull();
		caught.Should().BeOfType<ArgumentException>();
		caught!.Message.Should().Contain("numItems");
	}

	#endregion

	#region Helper Methods

	private static void ParseHeaderOldMethod(SackCollection sc, byte[] data, ref int offset)
	{
		sc.IsModified = false;

		if (sc.SackType == SackType.Stash)
		{
			using var reader = new BinaryReader(new MemoryStream(data, false));
			var tqData = new TQDataService(Mock.Of<ILogger<TQDataService>>());
			tqData.ValidateNextString("numItems", reader);
			sc.size = reader.ReadInt32();
		}
		else if (sc.SackType == SackType.Equipment)
		{
			if (sc.IsImmortalThrone)
			{
				sc.size = 12;
				sc.slots = 12;
			}
			else
			{
				sc.size = 11;
				sc.slots = 11;
			}
		}
	}

	#endregion
}
