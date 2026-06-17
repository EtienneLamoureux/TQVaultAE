using AwesomeAssertions;
using System.Text;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Unit tests for FileIO wrapper class
/// </summary>
public class FileIOTests : IDisposable
{
	private readonly FileIO _fileIO;
	private readonly DirectoryIO _directoryIO;
	private readonly string _testBasePath;

	public FileIOTests()
	{
		_fileIO = new FileIO();
		_directoryIO = new DirectoryIO();
		_testBasePath = Path.Combine(Path.GetTempPath(), "TQVaultAE_FileTests", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_testBasePath);
	}

	public void Dispose()
	{
		// Cleanup test directory
		try
		{
			if (Directory.Exists(_testBasePath))
				Directory.Delete(_testBasePath, recursive: true);
		}
		catch
		{
			// Ignore cleanup errors
		}
	}

	[Fact]
	public void ReadAllBytes_WithExistingFile_ReturnsBytes()
	{
		// Arrange
		var testFile = Path.Combine(_testBasePath, "bytes_test_" + Guid.NewGuid().ToString("N"));
		var testData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
		File.WriteAllBytes(testFile, testData);

		// Act
		var result = _fileIO.ReadAllBytes(testFile);

		// Assert
		result.Should().BeEquivalentTo(testData);
	}

	[Fact]
	public void ReadAllBytes_WithNonExistentFile_ThrowsFileNotFoundException()
	{
		// Arrange
		var nonExistentFile = Path.Combine(_testBasePath, "non_existent_" + Guid.NewGuid().ToString("N") + ".txt");

		// Act & Assert
		var act = () => _fileIO.ReadAllBytes(nonExistentFile);
		act.Should().Throw<FileNotFoundException>();
	}

	[Fact]
	public void WriteAllBytes_WithValidData_WritesBytesToFile()
	{
		// Arrange
		var testFile = Path.Combine(_testBasePath, "write_bytes_" + Guid.NewGuid().ToString("N") + ".dat");
		var testData = new byte[] { 0xFF, 0xFE, 0xFD, 0xFC };

		// Act
		_fileIO.WriteAllBytes(testFile, testData);

		// Assert
		File.Exists(testFile).Should().BeTrue();
		File.ReadAllBytes(testFile).Should().BeEquivalentTo(testData);
	}

	[Fact]
	public void Exists_WithExistingFile_ReturnsTrue()
	{
		// Arrange
		var testFile = Path.Combine(_testBasePath, "exists_test_" + Guid.NewGuid().ToString("N") + ".txt");
		File.WriteAllText(testFile, "content");

		// Act
		var result = _fileIO.Exists(testFile);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void Exists_WithNonExistentFile_ReturnsFalse()
	{
		// Arrange
		var nonExistentFile = Path.Combine(_testBasePath, "non_existent_" + Guid.NewGuid().ToString("N") + ".txt");

		// Act
		var result = _fileIO.Exists(nonExistentFile);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void ReadAllLines_WithExistingFile_ReturnsLines()
	{
		// Arrange
		var testFile = Path.Combine(_testBasePath, "lines_test_" + Guid.NewGuid().ToString("N") + ".txt");
		var lines = new[] { "line1", "line2", "line3" };
		File.WriteAllLines(testFile, lines);

		// Act
		var result = _fileIO.ReadAllLines(testFile);

		// Assert
		result.Should().BeEquivalentTo(lines);
	}

	[Fact]
	public void ReadAllLines_WithEmptyFile_ReturnsEmptyArray()
	{
		// Arrange
		var testFile = Path.Combine(_testBasePath, "empty_" + Guid.NewGuid().ToString("N") + ".txt");
		File.WriteAllText(testFile, string.Empty);

		// Act
		var result = _fileIO.ReadAllLines(testFile);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void ReadAllLines_WithNonExistentFile_ThrowsFileNotFoundException()
	{
		// Arrange
		var nonExistentFile = Path.Combine(_testBasePath, "non_existent_" + Guid.NewGuid().ToString("N") + ".txt");

		// Act & Assert
		var act = () => _fileIO.ReadAllLines(nonExistentFile);
		act.Should().Throw<FileNotFoundException>();
	}

	[Fact]
	public void WriteAllLines_WithValidData_WritesLinesToFile()
	{
		// Arrange
		var testFile = Path.Combine(_testBasePath, "write_lines_" + Guid.NewGuid().ToString("N") + ".txt");
		var lines = new[] { "line1", "line2", "line3" };

		// Act
		_fileIO.WriteAllLines(testFile, lines);

		// Assert
		File.ReadAllLines(testFile).Should().BeEquivalentTo(lines);
	}

	[Fact]
	public void WriteAllLines_WithEmptyCollection_WritesEmptyFile()
	{
		// Arrange
		var testFile = Path.Combine(_testBasePath, "empty_lines_" + Guid.NewGuid().ToString("N") + ".txt");

		// Act
		_fileIO.WriteAllLines(testFile, Array.Empty<string>());

		// Assert
		File.ReadAllText(testFile).Should().BeEmpty();
	}

	[Fact]
	public void ReadAllText_WithExistingFile_ReturnsContent()
	{
		// Arrange
		var testFile = Path.Combine(_testBasePath, "text_test_" + Guid.NewGuid().ToString("N") + ".txt");
		var content = "Hello, World! This is a test.";
		File.WriteAllText(testFile, content);

		// Act
		var result = _fileIO.ReadAllText(testFile);

		// Assert
		result.Should().Be(content);
	}

	[Fact]
	public void ReadAllText_WithEmptyFile_ReturnsEmptyString()
	{
		// Arrange
		var testFile = Path.Combine(_testBasePath, "empty_text_" + Guid.NewGuid().ToString("N") + ".txt");
		File.WriteAllText(testFile, string.Empty);

		// Act
		var result = _fileIO.ReadAllText(testFile);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void ReadAllText_WithNonExistentFile_ThrowsFileNotFoundException()
	{
		// Arrange
		var nonExistentFile = Path.Combine(_testBasePath, "non_existent_" + Guid.NewGuid().ToString("N") + ".txt");

		// Act & Assert
		var act = () => _fileIO.ReadAllText(nonExistentFile);
		act.Should().Throw<FileNotFoundException>();
	}

	[Fact]
	public void WriteAllText_WithValidData_WritesTextToFile()
	{
		// Arrange
		var testFile = Path.Combine(_testBasePath, "write_text_" + Guid.NewGuid().ToString("N") + ".txt");
		var content = "Hello, World!";

		// Act
		_fileIO.WriteAllText(testFile, content);

		// Assert
		File.ReadAllText(testFile).Should().Be(content);
	}

	[Fact]
	public void Delete_WithExistingFile_DeletesFile()
	{
		// Arrange
		var testFile = Path.Combine(_testBasePath, "to_delete_" + Guid.NewGuid().ToString("N") + ".txt");
		File.WriteAllText(testFile, "content");

		// Act
		_fileIO.Delete(testFile);

		// Assert
		File.Exists(testFile).Should().BeFalse();
	}

	[Fact]
	public void Copy_WithExistingFile_CopiesFile()
	{
		// Arrange
		var sourceFile = Path.Combine(_testBasePath, "source_" + Guid.NewGuid().ToString("N") + ".txt");
		var destFile = Path.Combine(_testBasePath, "dest_" + Guid.NewGuid().ToString("N") + ".txt");
		File.WriteAllText(sourceFile, "content to copy");

		// Act
		_fileIO.Copy(sourceFile, destFile);

		// Assert
		File.Exists(destFile).Should().BeTrue();
		File.ReadAllText(destFile).Should().Be("content to copy");
	}

	[Fact]
	public void Copy_WithOverwrite_WritesOverExistingFile()
	{
		// Arrange
		var sourceFile = Path.Combine(_testBasePath, "source_" + Guid.NewGuid().ToString("N") + ".txt");
		var destFile = Path.Combine(_testBasePath, "dest_" + Guid.NewGuid().ToString("N") + ".txt");
		File.WriteAllText(sourceFile, "new content");
		File.WriteAllText(destFile, "old content");

		// Act
		_fileIO.Copy(sourceFile, destFile, overwrite: true);

		// Assert
		File.ReadAllText(destFile).Should().Be("new content");
	}

	[Fact]
	public void Copy_WithoutOverwrite_ThrowsWhenDestExists()
	{
		// Arrange
		var sourceFile = Path.Combine(_testBasePath, "source_" + Guid.NewGuid().ToString("N") + ".txt");
		var destFile = Path.Combine(_testBasePath, "dest_" + Guid.NewGuid().ToString("N") + ".txt");
		File.WriteAllText(sourceFile, "new content");
		File.WriteAllText(destFile, "old content");

		// Act & Assert
		var act = () => _fileIO.Copy(sourceFile, destFile, overwrite: false);
		act.Should().Throw<IOException>();
	}

	[Fact]
	public void Copy_WithNonExistentSource_ThrowsFileNotFoundException()
	{
		// Arrange
		var nonExistentFile = Path.Combine(_testBasePath, "non_existent_" + Guid.NewGuid().ToString("N") + ".txt");
		var destFile = Path.Combine(_testBasePath, "dest_" + Guid.NewGuid().ToString("N") + ".txt");

		// Act & Assert
		var act = () => _fileIO.Copy(nonExistentFile, destFile);
		act.Should().Throw<FileNotFoundException>();
	}

	[Fact]
	public void Move_WithExistingFile_MovesFile()
	{
		// Arrange
		var sourceFile = Path.Combine(_testBasePath, "source_" + Guid.NewGuid().ToString("N") + ".txt");
		var destFile = Path.Combine(_testBasePath, "dest_" + Guid.NewGuid().ToString("N") + ".txt");
		File.WriteAllText(sourceFile, "content to move");

		// Act
		_fileIO.Move(sourceFile, destFile);

		// Assert
		File.Exists(destFile).Should().BeTrue();
		File.Exists(sourceFile).Should().BeFalse();
		File.ReadAllText(destFile).Should().Be("content to move");
	}

	[Fact]
	public void Move_WithNonExistentSource_ThrowsFileNotFoundException()
	{
		// Arrange
		var nonExistentFile = Path.Combine(_testBasePath, "non_existent_" + Guid.NewGuid().ToString("N") + ".txt");
		var destFile = Path.Combine(_testBasePath, "dest_" + Guid.NewGuid().ToString("N") + ".txt");

		// Act & Assert
		var act = () => _fileIO.Move(nonExistentFile, destFile);
		act.Should().Throw<FileNotFoundException>();
	}

	[Fact]
	public void Move_ToExistingDestination_ThrowsIOException()
	{
		// Arrange
		var sourceFile = Path.Combine(_testBasePath, "source_" + Guid.NewGuid().ToString("N") + ".txt");
		var destFile = Path.Combine(_testBasePath, "dest_" + Guid.NewGuid().ToString("N") + ".txt");
		File.WriteAllText(sourceFile, "source content");
		File.WriteAllText(destFile, "dest content");

		// Act & Assert
		var act = () => _fileIO.Move(sourceFile, destFile);
		act.Should().Throw<IOException>();
	}

	[Fact]
	public void ReadAllText_WithEncoding_ReturnsContentWithEncoding()
	{
		// Arrange
		var testFile = Path.Combine(_testBasePath, "encoding_test_" + Guid.NewGuid().ToString("N") + ".txt");
		var content = "Hello with special chars: \u00E9\u00E8\u00EA";
		File.WriteAllText(testFile, content, Encoding.UTF8);

		// Act
		var result = _fileIO.ReadAllText(testFile, Encoding.UTF8);

		// Assert
		result.Should().Be(content);
	}

	[Fact]
	public void WriteAllText_WithEncoding_WritesContentWithEncoding()
	{
		// Arrange
		var testFile = Path.Combine(_testBasePath, "write_encoding_" + Guid.NewGuid().ToString("N") + ".txt");
		var content = "Content with special chars: \u00C4\u00D6\u00DC";

		// Act
		_fileIO.WriteAllText(testFile, content, Encoding.UTF8);

		// Assert
		File.ReadAllText(testFile, Encoding.UTF8).Should().Be(content);
	}

	[Fact]
	public void ReadAllText_WithUnicodeContent_HandlesCorrectly()
	{
		// Arrange
		var testFile = Path.Combine(_testBasePath, "unicode_test_" + Guid.NewGuid().ToString("N") + ".txt");
		var content = "Unicode: \u4E2D\u6587\u65E5\u672C\u8A9E \u0420\u0443\u0441\u0441\u043A\u0438\u0439";
		File.WriteAllText(testFile, content);

		// Act
		var result = _fileIO.ReadAllText(testFile);

		// Assert
		result.Should().Be(content);
	}

	[Fact]
	public void WriteAllLines_WithLargeContent_WritesAllLines()
	{
		// Arrange
		var testFile = Path.Combine(_testBasePath, "large_lines_" + Guid.NewGuid().ToString("N") + ".txt");
		var lines = Enumerable.Range(1, 1000).Select(i => $"Line {i}").ToArray();

		// Act
		_fileIO.WriteAllLines(testFile, lines);

		// Assert
		var result = _fileIO.ReadAllLines(testFile);
		result.Should().HaveCount(1000);
		result[999].Should().Be("Line 1000");
	}

	[Fact]
	public void WriteAllText_WithNewlines_PreservesNewlines()
	{
		// Arrange
		var testFile = Path.Combine(_testBasePath, "newlines_" + Guid.NewGuid().ToString("N") + ".txt");
		var content = "Line 1\nLine 2\r\nLine 3";

		// Act
		_fileIO.WriteAllText(testFile, content);

		// Assert
		File.ReadAllText(testFile).Should().Be(content);
	}
}
