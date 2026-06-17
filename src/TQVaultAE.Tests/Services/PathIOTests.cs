using AwesomeAssertions;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Unit tests for PathIO wrapper class
/// </summary>
public class PathIOTests : IDisposable
{
	private readonly PathIO _pathIO;
	private readonly string _testBasePath;

	public PathIOTests()
	{
		_pathIO = new PathIO();
		_testBasePath = Path.Combine(Path.GetTempPath(), "TQVaultAE_Tests", Guid.NewGuid().ToString("N"));
	}

	public void Dispose()
	{
		// Cleanup any test directories that might have been created
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
	public void Combine_WithTwoPaths_JoinsPathsCorrectly()
	{
		// Arrange
		var path1 = "/home/user";
		var path2 = "documents";

		// Act
		var result = _pathIO.Combine(path1, path2);

		// Assert
		result.Should().NotBeNullOrEmpty();
		result.Should().Contain("user");
		result.Should().Contain("documents");
	}

	[Fact]
	public void Combine_WithThreePaths_JoinsPathsCorrectly()
	{
		// Arrange
		var path1 = "/home/user";
		var path2 = "documents";
		var path3 = "file.txt";

		// Act
		var result = _pathIO.Combine(path1, path2, path3);

		// Assert
		result.Should().NotBeNullOrEmpty();
		result.Should().Contain("user");
		result.Should().Contain("documents");
		result.Should().Contain("file.txt");
	}

	[Fact]
	public void Combine_WithParamsArray_JoinsAllPaths()
	{
		// Arrange
		var paths = new[] { "/home", "user", "documents", "test.txt" };

		// Act
		var result = _pathIO.Combine(paths);

		// Assert
		result.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void GetDirectoryName_WithValidPath_ReturnsDirectory()
	{
		// Arrange
		var fullPath = "/home/user/documents/file.txt";

		// Act
		var result = _pathIO.GetDirectoryName(fullPath);

		// Assert
		result.Should().NotBeNullOrEmpty();
		result.Should().NotContain("file.txt");
	}

	[Fact]
	public void GetDirectoryName_WithRootPath_ReturnsNullOrEmpty()
	{
		// Arrange
		var rootPath = "/";

		// Act
		var result = _pathIO.GetDirectoryName(rootPath);

		// Assert
		result.Should().BeNullOrEmpty();
	}

	[Fact]
	public void GetFileName_WithValidPath_ReturnsFileName()
	{
		// Arrange
		var fullPath = "/home/user/documents/file.txt";

		// Act
		var result = _pathIO.GetFileName(fullPath);

		// Assert
		result.Should().Be("file.txt");
	}

	[Fact]
	public void GetFileName_WithNoFileName_ReturnsEmpty()
	{
		// Arrange
		var directoryPath = "/home/user/documents/";

		// Act
		var result = _pathIO.GetFileName(directoryPath);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void GetFileNameWithoutExtension_WithValidPath_ReturnsNameWithoutExtension()
	{
		// Arrange
		var fullPath = "/home/user/documents/file.txt";

		// Act
		var result = _pathIO.GetFileNameWithoutExtension(fullPath);

		// Assert
		result.Should().Be("file");
	}

	[Fact]
	public void GetFileNameWithoutExtension_WithMultipleDots_ReturnsNameWithoutLastExtension()
	{
		// Arrange
		var fullPath = "/home/user/documents/file.backup.txt";

		// Act
		var result = _pathIO.GetFileNameWithoutExtension(fullPath);

		// Assert
		result.Should().Be("file.backup");
	}

	[Fact]
	public void GetExtension_WithValidPath_ReturnsExtension()
	{
		// Arrange
		var fullPath = "/home/user/documents/file.txt";

		// Act
		var result = _pathIO.GetExtension(fullPath);

		// Assert
		result.Should().Be(".txt");
	}

	[Fact]
	public void GetExtension_WithNoExtension_ReturnsEmpty()
	{
		// Arrange
		var fullPath = "/home/user/documents/file";

		// Act
		var result = _pathIO.GetExtension(fullPath);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void ChangeExtension_WithValidPath_ChangesExtension()
	{
		// Arrange
		var fullPath = "/home/user/documents/file.txt";

		// Act
		var result = _pathIO.ChangeExtension(fullPath, ".md");

		// Assert
		result.Should().EndWith(".md");
	}

	[Fact]
	public void ChangeExtension_WithNoExtension_AddsExtension()
	{
		// Arrange
		var fullPath = "/home/user/documents/file";

		// Act
		var result = _pathIO.ChangeExtension(fullPath, ".txt");

		// Assert
		result.Should().EndWith(".txt");
	}

	[Fact]
	public void ChangeExtension_WithEmptyExtension_RemovesExtension()
	{
		// Arrange
		var fullPath = "/home/user/documents/file.txt";

		// Act
		var result = _pathIO.ChangeExtension(fullPath, string.Empty);

		// Assert
		result.Should().NotContain(".txt");
	}

	[Fact]
	public void GetInvalidPathChars_ReturnsInvalidCharacters()
	{
		// Act
		var result = _pathIO.GetInvalidPathChars();

		// Assert
		result.Should().NotBeNull();
		result.Should().NotBeEmpty();
	}

	[Fact]
	public void GetInvalidFileNameChars_ReturnsInvalidCharacters()
	{
		// Act
		var result = _pathIO.GetInvalidFileNameChars();

		// Assert
		result.Should().NotBeNull();
		result.Should().NotBeEmpty();
		// File name invalid chars should include path separator
		result.Should().Contain(Path.DirectorySeparatorChar);
	}

	[Fact]
	public void Combine_WithWindowsStylePaths_HandlesCorrectly()
	{
		// Arrange
		var path1 = "C:\\Users";
		var path2 = "Documents";

		// Act
		var result = _pathIO.Combine(path1, path2);

		// Assert
		result.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void GetDirectoryName_WithMixedSeparators_HandlesCorrectly()
	{
		// Arrange - handle both Windows and Unix style
		var mixedPath = "/home/user\\documents/file.txt";

		// Act
		var result = _pathIO.GetDirectoryName(mixedPath);

		// Assert
		result.Should().NotBeNullOrEmpty();
	}
}
