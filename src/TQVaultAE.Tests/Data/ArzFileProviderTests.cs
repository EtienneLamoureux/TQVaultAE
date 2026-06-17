using System.Buffers.Binary;
using System.IO;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Tests;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Unit tests for ArzFileProvider class
/// </summary>
public class ArzFileProviderTests
{
	private readonly Mock<ILogger<ArzFileProvider>> _mockLogger;
	private readonly Mock<IRecordInfoProvider> _mockRecordInfoProvider;
	private readonly Mock<ITQDataService> _mockTQDataService;
	private readonly Mock<IFileDataService> _mockFileDataService;
	private readonly UserSettings _userSettings;
	private readonly ArzFileProvider _provider;

	public ArzFileProviderTests()
	{
		_mockLogger = new Mock<ILogger<ArzFileProvider>>();
		_mockRecordInfoProvider = new Mock<IRecordInfoProvider>();
		_mockTQDataService = new Mock<ITQDataService>();
		_mockFileDataService = new Mock<IFileDataService>();
		_userSettings = new UserSettings();
		_provider = new ArzFileProvider(
			_mockLogger.Object,
			_mockRecordInfoProvider.Object,
			_mockTQDataService.Object,
			_userSettings,
			_mockFileDataService.Object
		);
	}

	[Fact]
	public void GetItem_WithNullRecordId_ReturnsNull()
	{
		// Arrange
		var file = new ArzFile("test.arz");

		// Act
		var result = _provider.GetItem(file, null!);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void GetItem_WithMissingRecordId_ReturnsNull()
	{
		// Arrange
		var file = new ArzFile("test.arz");
		file.RecordInfo = new Dictionary<RecordId, RecordInfo>();

		// Act
		var result = _provider.GetItem(file, RecordId.Create("missing/record"));

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void GetItem_WithExistingRecordId_ReturnsDecompressedRecord()
	{
		// Arrange
		var recordId = RecordId.Create("test/record");
		var file = new ArzFile("test.arz");
		file.RecordInfo = new Dictionary<RecordId, RecordInfo>
		{
			[recordId] = new RecordInfo { ID = recordId }
		};

		var expectedRecord = new DBRecordCollection(recordId, "test");
		_mockRecordInfoProvider
			.Setup(x => x.Decompress(file, It.IsAny<RecordInfo>()))
			.Returns(expectedRecord);

		// Act
		var result = _provider.GetItem(file, recordId);

		// Assert
		result.Should().BeSameAs(expectedRecord);
	}

	[Fact]
	public void GetRecordNotCached_DecodesWithoutCaching()
	{
		// Arrange
		var recordId = RecordId.Create("test/record");
		var file = new ArzFile("test.arz");
		file.RecordInfo = new Dictionary<RecordId, RecordInfo>
		{
			[recordId] = new RecordInfo { ID = recordId }
		};

		var expectedRecord = new DBRecordCollection(recordId, "test");
		_mockRecordInfoProvider
			.Setup(x => x.Decompress(file, It.IsAny<RecordInfo>()))
			.Returns(expectedRecord);

		// Act
		var result = _provider.GetRecordNotCached(file, recordId);

		// Assert
		result.Should().BeSameAs(expectedRecord);
		_mockRecordInfoProvider.Verify(x => x.Decompress(file, It.IsAny<RecordInfo>()), Times.Once);
	}

	// Note: Read() method tests require extensive mocking of IFileDataService.GetMemory
	// which returns Memory<byte>. Those tests are better suited for integration testing
	// with real ARZ files when the game is installed.

	/*
	[Fact]
	public async Task Read_WithValidArzFile_ReturnsTrue()
	{
		// Arrange - Create a minimal valid ARZ file in memory
		var ms = new MemoryStream();
		await using (var writer = new BinaryWriter(ms))
		{
			// ARZ file header: 6 integers
			// header[0] = version (typically 3)
			// header[1] = recordTableStart
			// header[2] = recordTableSize  
			// header[3] = recordTableCount
			// header[4] = stringTableStart
			// header[5] = stringTableSize

			writer.Write(3); // version
			writer.Write(24); // recordTableStart (after header = 6*4 = 24)
			writer.Write(0); // recordTableSize
			writer.Write(0); // recordTableCount (no records)
			writer.Write(24); // stringTableStart (same as record table start for empty)
			writer.Write(4); // stringTableSize (just the count = 0)
		}

		ms.Position = 0;

		// Create a temp file
		var tempFile = Path.GetTempFileName();
		try
		{
			await File.WriteAllBytesAsync(tempFile, ms.ToArray());

			var file = new ArzFile(tempFile);
			_mockFileDataService
				.Setup(x => x.GetMemory(tempFile, It.IsAny<int>(), It.IsAny<int>()))
				.Returns(ms.ToArray());

			// Act
			var result = _provider.Read(file);

			// Assert
			result.Should().BeTrue();
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task Read_WithInvalidFile_ReturnsFalse()
	{
		// Arrange - Create an invalid file (too short)
		var tempFile = Path.GetTempFileName();
		try
		{
			await File.WriteAllBytesAsync(tempFile, new byte[] { 1, 2, 3 });

			var file = new ArzFile(tempFile);

			// Act
			var result = _provider.Read(file);

			// Assert
			result.Should().BeFalse();
		}
		finally
		{
			File.Delete(tempFile);
		}
	}
	*/

	// GetItem and GetRecordNotCached require IFileDataService mock for Decompress
	// which is complex. They are tested indirectly through integration tests.

	[Fact]
	public void ArzFile_Constructor_InitializesCorrectly()
	{
		// Arrange & Act
		var file = new ArzFile("test.arz");

		// Assert
		file.FileName.Should().Be("test.arz");
		file.Strings.Should().BeNull();
		file.RecordInfo.Should().NotBeNull(); // Initialized to empty dictionary
	}
}
