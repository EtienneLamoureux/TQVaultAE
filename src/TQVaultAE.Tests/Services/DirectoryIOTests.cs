using AwesomeAssertions;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Unit tests for DirectoryIO wrapper class
/// </summary>
public class DirectoryIOTests : IDisposable
{
	private readonly DirectoryIO _directoryIO;
	private readonly PathIO _pathIO;
	private readonly string _testBasePath;

	public DirectoryIOTests()
	{
		_directoryIO = new DirectoryIO();
		_pathIO = new PathIO();
		_testBasePath = Path.Combine(Path.GetTempPath(), "TQVaultAE_DirectoryTests", Guid.NewGuid().ToString("N"));
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
	public void Exists_WithExistingDirectory_ReturnsTrue()
	{
		// Arrange
		Directory.CreateDirectory(_testBasePath);

		// Act
		var result = _directoryIO.Exists(_testBasePath);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void Exists_WithNonExistingDirectory_ReturnsFalse()
	{
		// Arrange
		var nonExistentPath = Path.Combine(_testBasePath, "non_existent_" + Guid.NewGuid().ToString("N"));

		// Act
		var result = _directoryIO.Exists(nonExistentPath);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void CreateDirectory_WithValidPath_CreatesDirectory()
	{
		// Arrange
		var newDirPath = Path.Combine(_testBasePath, "new_directory_" + Guid.NewGuid().ToString("N"));

		// Act
		_directoryIO.CreateDirectory(newDirPath);

		// Assert
		Directory.Exists(newDirPath).Should().BeTrue();
	}

	[Fact]
	public void CreateDirectory_WithNestedPath_CreatesAllDirectories()
	{
		// Arrange
		var nestedPath = Path.Combine(_testBasePath, "level1", "level2", "level3_" + Guid.NewGuid().ToString("N"));

		// Act
		_directoryIO.CreateDirectory(nestedPath);

		// Assert
		Directory.Exists(nestedPath).Should().BeTrue();
	}

	[Fact]
	public void GetFiles_WithExistingDirectory_ReturnsFiles()
	{
		// Arrange
		var testDir = Path.Combine(_testBasePath, "files_test_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(testDir);
		File.WriteAllText(Path.Combine(testDir, "file1.txt"), "content1");
		File.WriteAllText(Path.Combine(testDir, "file2.txt"), "content2");

		// Act
		var result = _directoryIO.GetFiles(testDir);

		// Assert
		result.Should().HaveCount(2);
		result.Should().Contain(f => f.EndsWith("file1.txt"));
		result.Should().Contain(f => f.EndsWith("file2.txt"));
	}

	[Fact]
	public void GetFiles_WithEmptyDirectory_ReturnsEmptyArray()
	{
		// Arrange
		var emptyDir = Path.Combine(_testBasePath, "empty_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(emptyDir);

		// Act
		var result = _directoryIO.GetFiles(emptyDir);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void GetFiles_WithSearchPattern_ReturnsMatchingFiles()
	{
		// Arrange
		var testDir = Path.Combine(_testBasePath, "pattern_test_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(testDir);
		File.WriteAllText(Path.Combine(testDir, "file1.txt"), "content1");
		File.WriteAllText(Path.Combine(testDir, "file2.log"), "content2");
		File.WriteAllText(Path.Combine(testDir, "file3.txt"), "content3");

		// Act
		var result = _directoryIO.GetFiles(testDir, "*.txt");

		// Assert
		result.Should().HaveCount(2);
		result.Should().OnlyContain(f => f.EndsWith(".txt"));
	}

	[Fact]
	public void GetFiles_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
	{
		// Arrange
		var nonExistentPath = Path.Combine(_testBasePath, "non_existent_" + Guid.NewGuid().ToString("N"));

		// Act & Assert - Linux throws DirectoryNotFoundException, behavior differs from Windows
		var act = () => _directoryIO.GetFiles(nonExistentPath);
		act.Should().Throw<DirectoryNotFoundException>();
	}

	[Fact]
	public void GetDirectories_WithExistingDirectory_ReturnsSubdirectories()
	{
		// Arrange
		var testDir = Path.Combine(_testBasePath, "dirs_test_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(testDir);
		Directory.CreateDirectory(Path.Combine(testDir, "subdir1"));
		Directory.CreateDirectory(Path.Combine(testDir, "subdir2"));

		// Act
		var result = _directoryIO.GetDirectories(testDir);

		// Assert
		result.Should().HaveCount(2);
	}

	[Fact]
	public void GetDirectories_WithEmptyDirectory_ReturnsEmptyArray()
	{
		// Arrange
		var emptyDir = Path.Combine(_testBasePath, "empty_dirs_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(emptyDir);

		// Act
		var result = _directoryIO.GetDirectories(emptyDir);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void GetDirectories_WithSearchPattern_ReturnsMatchingDirectories()
	{
		// Arrange
		var testDir = Path.Combine(_testBasePath, "dir_pattern_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(testDir);
		Directory.CreateDirectory(Path.Combine(testDir, "test_folder1"));
		Directory.CreateDirectory(Path.Combine(testDir, "test_folder2"));
		Directory.CreateDirectory(Path.Combine(testDir, "other_folder"));

		// Act
		var result = _directoryIO.GetDirectories(testDir, "test_*");

		// Assert
		result.Should().HaveCount(2);
		result.Should().OnlyContain(d => d.Contains("test_"));
	}

	[Fact]
	public void GetDirectories_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
	{
		// Arrange
		var nonExistentPath = Path.Combine(_testBasePath, "non_existent_" + Guid.NewGuid().ToString("N"));

		// Act & Assert - Linux throws DirectoryNotFoundException, behavior differs from Windows
		var act = () => _directoryIO.GetDirectories(nonExistentPath);
		act.Should().Throw<DirectoryNotFoundException>();
	}

	[Fact]
	public void Delete_WithExistingEmptyDirectory_DeletesDirectory()
	{
		// Arrange
		var testDir = Path.Combine(_testBasePath, "to_delete_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(testDir);

		// Act
		_directoryIO.Delete(testDir);

		// Assert
		Directory.Exists(testDir).Should().BeFalse();
	}

	[Fact]
	public void Delete_WithNonEmptyDirectory_ThrowsException()
	{
		// Arrange
		var testDir = Path.Combine(_testBasePath, "non_empty_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(testDir);
		File.WriteAllText(Path.Combine(testDir, "file.txt"), "content");

		// Act & Assert
		var act = () => _directoryIO.Delete(testDir);
		act.Should().Throw<IOException>();
	}

	[Fact]
	public void Delete_WithNonEmptyDirectoryRecursive_DeletesDirectoryAndContents()
	{
		// Arrange
		var testDir = Path.Combine(_testBasePath, "recursive_delete_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(testDir);
		Directory.CreateDirectory(Path.Combine(testDir, "subdir"));
		File.WriteAllText(Path.Combine(testDir, "file.txt"), "content");
		File.WriteAllText(Path.Combine(testDir, "subdir", "nested.txt"), "nested content");

		// Act
		_directoryIO.Delete(testDir, recursive: true);

		// Assert
		Directory.Exists(testDir).Should().BeFalse();
	}

	[Fact]
	public void Delete_WithNonExistentDirectory_ThrowsException()
	{
		// Arrange
		var nonExistentPath = Path.Combine(_testBasePath, "non_existent_" + Guid.NewGuid().ToString("N"));

		// Act & Assert
		var act = () => _directoryIO.Delete(nonExistentPath);
		act.Should().Throw<DirectoryNotFoundException>();
	}

	[Fact]
	public void Move_WithExistingDirectory_MovesDirectory()
	{
		// Arrange
		var sourceDir = Path.Combine(_testBasePath, "source_" + Guid.NewGuid().ToString("N"));
		var destDir = Path.Combine(_testBasePath, "dest_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(sourceDir);
		File.WriteAllText(Path.Combine(sourceDir, "file.txt"), "content");

		// Act
		_directoryIO.Move(sourceDir, destDir);

		// Assert
		Directory.Exists(destDir).Should().BeTrue();
		Directory.Exists(sourceDir).Should().BeFalse();
		File.Exists(Path.Combine(destDir, "file.txt")).Should().BeTrue();
	}

	[Fact]
	public void Move_WithNonExistentSource_ThrowsException()
	{
		// Arrange
		var nonExistentPath = Path.Combine(_testBasePath, "non_existent_" + Guid.NewGuid().ToString("N"));
		var destPath = Path.Combine(_testBasePath, "dest_" + Guid.NewGuid().ToString("N"));

		// Act & Assert
		var act = () => _directoryIO.Move(nonExistentPath, destPath);
		act.Should().Throw<DirectoryNotFoundException>();
	}

	[Fact]
	public void Move_ToExistingDestination_ThrowsException()
	{
		// Arrange
		var sourceDir = Path.Combine(_testBasePath, "source_" + Guid.NewGuid().ToString("N"));
		var destDir = Path.Combine(_testBasePath, "dest_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(sourceDir);
		Directory.CreateDirectory(destDir);

		// Act & Assert
		var act = () => _directoryIO.Move(sourceDir, destDir);
		act.Should().Throw<IOException>();
	}

	[Fact]
	public void Exists_WithRootDirectory_ReturnsTrue()
	{
		// Arrange - use root directory that should exist on all platforms
		var rootPath = Path.GetPathRoot(_testBasePath);

		// Act
		var result = _directoryIO.Exists(rootPath);

		// Assert
		result.Should().BeTrue();
	}
}
