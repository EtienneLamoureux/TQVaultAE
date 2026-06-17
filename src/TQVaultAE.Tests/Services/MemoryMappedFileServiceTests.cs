using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Unit tests for MemoryMappedFileService class
/// </summary>
public class MemoryMappedFileServiceTests
{
	private readonly Mock<ILogger<MemoryMappedFileService>> _mockLogger;
	private readonly MemoryMappedFileService _service;

	public MemoryMappedFileServiceTests()
	{
		_mockLogger = new Mock<ILogger<MemoryMappedFileService>>();
		_service = new MemoryMappedFileService(_mockLogger.Object);
	}

	[Fact]
	public void GetReadOnlySpan_WithValidFile_ReturnsSpan()
	{
		// Arrange - create temp file with test data
		var tempFile = Path.GetTempFileName();
		try
		{
			var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			File.WriteAllBytes(tempFile, testData);

			// Act
			var result = _service.GetReadOnlySpan(tempFile, 0, 5);

			// Assert
			result.Length.Should().Be(5);
			result.ToArray().Should().BeEquivalentTo(new byte[] { 1, 2, 3, 4, 5 });
		}
		finally
		{
			_service.Release(tempFile);
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void GetReadOnlySpan_WithOffset_ReturnsCorrectData()
	{
		// Arrange
		var tempFile = Path.GetTempFileName();
		try
		{
			var testData = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
			File.WriteAllBytes(tempFile, testData);

			// Act
			var result = _service.GetReadOnlySpan(tempFile, 5, 4);

			// Assert
			result.Length.Should().Be(4);
			result.ToArray().Should().BeEquivalentTo(new byte[] { 60, 70, 80, 90 });
		}
		finally
		{
			_service.Release(tempFile);
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void GetReadOnlySpan_WithInvalidFile_ReturnsEmpty()
	{
		// Arrange - use temp path for cross-platform compatibility
		var nonExistentFile = Path.Combine(Path.GetTempPath(), "NonExistentFile.dat");

		// Act
		var result = _service.GetReadOnlySpan(nonExistentFile, 0, 100);

		// Assert
		result.Length.Should().Be(0);
	}

	[Fact]
	public void GetMemory_WithValidFile_ReturnsMemory()
	{
		// Arrange
		var tempFile = Path.GetTempFileName();
		try
		{
			var testData = new byte[] { 5, 10, 15, 20, 25 };
			File.WriteAllBytes(tempFile, testData);

			// Act
			var result = _service.GetMemory(tempFile, 0, 5);

			// Assert
			result.Length.Should().Be(5);
			result.ToArray().Should().BeEquivalentTo(new byte[] { 5, 10, 15, 20, 25 });
		}
		finally
		{
			_service.Release(tempFile);
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void GetMemory_WithOffset_ReturnsCorrectData()
	{
		// Arrange
		var tempFile = Path.GetTempFileName();
		try
		{
			var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			File.WriteAllBytes(tempFile, testData);

			// Act
			var result = _service.GetMemory(tempFile, 3, 4);

			// Assert
			result.Length.Should().Be(4);
			result.ToArray().Should().BeEquivalentTo(new byte[] { 4, 5, 6, 7 });
		}
		finally
		{
			_service.Release(tempFile);
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void GetMemory_WithInvalidFile_ReturnsEmpty()
	{
		// Arrange - use temp path for cross-platform compatibility
		var nonExistentFile = Path.Combine(Path.GetTempPath(), "NonExistentFile.dat");

		// Act
		var result = _service.GetMemory(nonExistentFile, 0, 100);

		// Assert
		result.Length.Should().Be(0);
	}

	[Fact]
	public void Read_WithUnmanagedType_ReturnsValue()
	{
		// Arrange
		var tempFile = Path.GetTempFileName();
		try
		{
			// Write an int value
			var testValue = 12345;
			var testData = BitConverter.GetBytes(testValue);
			File.WriteAllBytes(tempFile, testData);

			// Act
			var result = _service.Read<int>(tempFile, 0);

			// Assert
			result.Should().Be(testValue);
		}
		finally
		{
			_service.Release(tempFile);
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void Read_WithStructType_ReturnsValue()
	{
		// Arrange
		var tempFile = Path.GetTempFileName();
		try
		{
			// Test with a long (8 bytes)
			var testValue = 9876543210L;
			var testData = BitConverter.GetBytes(testValue);
			File.WriteAllBytes(tempFile, testData);

			// Act
			var result = _service.Read<long>(tempFile, 0);

			// Assert
			result.Should().Be(testValue);
		}
		finally
		{
			_service.Release(tempFile);
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void Read_WithInvalidFile_ReturnsDefault()
	{
		// Arrange - use temp path for cross-platform compatibility
		var nonExistentFile = Path.Combine(Path.GetTempPath(), "NonExistentFile.dat");

		// Act
		var result = _service.Read<int>(nonExistentFile, 0);

		// Assert
		result.Should().Be(0);
	}

	[Fact]
	public void Release_WithValidFile_ClearsCache()
	{
		// Arrange
		var tempFile = Path.GetTempFileName();
		try
		{
			var testData = new byte[] { 1, 2, 3 };
			File.WriteAllBytes(tempFile, testData);

			// First read to cache the MMF
			var result1 = _service.GetMemory(tempFile, 0, 3);
			result1.Length.Should().Be(3);

			// Act
			_service.Release(tempFile);

			// Read again - should re-create MMF since we released it
			var result2 = _service.GetMemory(tempFile, 0, 3);
			result2.Length.Should().Be(3);
		}
		finally
		{
			_service.Release(tempFile);
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void ReleaseAll_ClearsAllCaches()
	{
		// Arrange
		var tempFile1 = Path.GetTempFileName();
		var tempFile2 = Path.GetTempFileName();
		try
		{
			File.WriteAllBytes(tempFile1, new byte[] { 1, 2, 3 });
			File.WriteAllBytes(tempFile2, new byte[] { 4, 5, 6 });

			// Read both files to cache them
			_service.GetMemory(tempFile1, 0, 3).Length.Should().Be(3);
			_service.GetMemory(tempFile2, 0, 3).Length.Should().Be(3);

			// Act
			_service.ReleaseAll();

			// Read again - should work since we re-created
			_service.GetMemory(tempFile1, 0, 3).Length.Should().Be(3);
			_service.GetMemory(tempFile2, 0, 3).Length.Should().Be(3);
		}
		finally
		{
			_service.ReleaseAll();
			File.Delete(tempFile1);
			File.Delete(tempFile2);
		}
	}

	[Fact]
	public void GetReadOnlySpan_AfterRelease_RereadsFile()
	{
		// Arrange
		var tempFile = Path.GetTempFileName();
		try
		{
			File.WriteAllBytes(tempFile, new byte[] { 10, 20, 30 });

			// First read
			var result1 = _service.GetReadOnlySpan(tempFile, 0, 3);
			result1[0].Should().Be(10);

			// Release first to avoid file locking
			_service.Release(tempFile);

			// Modify file
			File.WriteAllBytes(tempFile, new byte[] { 99, 88, 77 });

			// and re-read
			var result2 = _service.GetReadOnlySpan(tempFile, 0, 3);

			// Assert - should get new data since file was modified
			result2[0].Should().Be(99);
		}
		finally
		{
			_service.Release(tempFile);
			File.Delete(tempFile);
		}
	}
}