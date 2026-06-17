using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Comprehensive validation tests for ArcFileProvider ReadARCToC_NEW method.
/// Uses local test data files for reproducible tests.
/// </summary>
public class ArcFileProviderAllFilesTest : IDisposable
{
	private readonly ITestOutputHelper _output;
	private readonly Mock<ILogger<ArcFileProvider>> _mockLogger;
	private readonly Mock<IPathIO> _mockPathIO;
	private readonly Mock<IDirectoryIO> _mockDirectoryIO;
	private readonly Mock<IFileDataService> _mockFileDataService;
	private readonly Mock<IDecompressionService> _mockDecompressionService;
	private readonly UserSettings _userSettings;
	private readonly ArcFileProvider _provider;

	// Path to test data directory
	private static readonly string TestDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

	// Local Particles.arc file for basic validation
	private static readonly string LocalParticlesArcPath = Path.Combine(TestDataDir, "Particles.arc");

	// Edge case file that should be excluded from tests (empty/invalid arc file)
	private const string EdgeCaseFile = "Resources/XPack4/Underground.arc";

	/// <summary>
	/// Gets all ARC files from the Titan Quest game directory, excluding edge case files.
	/// </summary>
	private static IEnumerable<string> GetArcFiles()
	{
		var gamePath = TestingConditions.TitanQuestPath;
		if (gamePath is null || !Directory.Exists(gamePath))
			yield break;

		var arcFiles = Directory.GetFiles(gamePath, "*.arc", SearchOption.AllDirectories);
		foreach (var arcFile in arcFiles)
		{
			// Skip edge case file that is empty/invalid
			if (arcFile.Replace('\\', '/').EndsWith(EdgeCaseFile, StringComparison.OrdinalIgnoreCase))
				continue;

			yield return arcFile;
		}
	}

	public ArcFileProviderAllFilesTest(ITestOutputHelper output)
	{
		_output = output;
		_mockLogger = new Mock<ILogger<ArcFileProvider>>();
		_mockPathIO = new Mock<IPathIO>();
		_mockDirectoryIO = new Mock<IDirectoryIO>();
		_mockFileDataService = new Mock<IFileDataService>();
		_mockDecompressionService = new Mock<IDecompressionService>();
		_userSettings = new UserSettings();
		_userSettings.ARCFileDebugLevel = 3;

		_provider = new ArcFileProvider(
			_mockLogger.Object,
			_mockPathIO.Object,
			_mockDirectoryIO.Object,
			_userSettings,
			_mockFileDataService.Object,
			_mockDecompressionService.Object
		);
	}

	public void Dispose()
	{
		// Cleanup if needed
	}

	[Fact]
	public void ValidateV3AgainstOriginal_ParticlesArc_ResultsMatch()
	{
		// Skip if local test file doesn't exist
		if (!File.Exists(LocalParticlesArcPath))
		{
			Assert.Fail($"{LocalParticlesArcPath} should exists!");
		}

		// Arrange
		var file = new ArcFile(LocalParticlesArcPath);

		// Act
		var validationResult = _provider.ValidateNEWAgainstOriginal(file);

		// Assert
		validationResult.CountsMatch.Should().BeTrue(
			$"Entry counts should match. Original={validationResult.OriginalCount}, V3={validationResult.V3Count}. " +
			$"Error: {validationResult.ErrorMessage}"
		);

		validationResult.IsValid.Should().BeTrue(
			$"V3 should match original. Original={validationResult.OriginalCount}, V3={validationResult.V3Count}. " +
			$"Error: {validationResult.ErrorMessage}"
		);
	}

	[Fact]
	public void ReadARCToC_NEW_ParticlesArc_ProducesValidResult()
	{
		// Skip if local test file doesn't exist
		if (!File.Exists(LocalParticlesArcPath))
		{
			Assert.Fail($"{LocalParticlesArcPath} should exists!");
		}

		// Arrange
		var file = new ArcFile(LocalParticlesArcPath);

		// Act
		var result = _provider.ReadARCToC_NEW(file);

		// Assert
		result.Should().BeTrue("V3 should successfully read Particles.arc");
		file.FileHasBeenRead.Should().BeTrue();
		file.DirectoryEntries.Should().NotBeEmpty("Particles.arc should have directory entries");
	}

	[Fact]
	public void ReadARCToC_OLD_ParticlesArc_ProducesValidResult()
	{
		// Skip if local test file doesn't exist
		if (!File.Exists(LocalParticlesArcPath))
		{
			Assert.Fail($"{LocalParticlesArcPath} should exists!");
		}

		// Arrange
		var file = new ArcFile(LocalParticlesArcPath);

		// Act
		_provider.ReadARCToC_OLD(file);

		// Assert
		file.FileHasBeenRead.Should().BeTrue();
		file.DirectoryEntries.Should().NotBeEmpty("Particles.arc should have directory entries per OLD");
	}

	[Fact]
	public void V3AndOLD_ParticlesArc_SameEntryCount()
	{
		// Skip if local test file doesn't exist
		if (!File.Exists(LocalParticlesArcPath))
		{
			Assert.Fail($"{LocalParticlesArcPath} should exists!");
		}

		// Arrange
		var fileOLD = new ArcFile(LocalParticlesArcPath);
		var fileV3 = new ArcFile(LocalParticlesArcPath);

		// Act
		_provider.ReadARCToC_OLD(fileOLD);
		_provider.ReadARCToC_NEW(fileV3);

		// Assert
		fileOLD.DirectoryEntries.Count.Should().Be(
			fileV3.DirectoryEntries.Count,
			"OLD and V3 should produce same count for Particles.arc"
		);
	}

	/// <summary>
	/// Extended validation test - requires Titan Quest game installation.
	/// This test requires 180 ARC files from the game directory.
	/// </summary>
	[Fact(Skip = "Requires Titan Quest game installation", SkipType = typeof(TestingConditions), SkipUnless = nameof(TestingConditions.IsGameInstalled))]
	public void ValidateV3AgainstOriginal_AllArcFiles_ResultsMatch()
	{
		int passed = 0, failed = 0;
		var failures = new List<string>();

		foreach (var arcFile in GetArcFiles().OrderBy(f => f))
		{
			var gamePath = TestingConditions.TitanQuestPath;
			var relativePath = arcFile.Substring(gamePath!.Length + 1);

			var file = new ArcFile(arcFile);
			var validationResult = _provider.ValidateNEWAgainstOriginal(file);

			if (validationResult.IsValid && validationResult.CountsMatch)
			{
				passed++;
				_output.WriteLine($"[PASS] {relativePath}: {validationResult.OriginalCount} entries");
			}
			else
			{
				failed++;
				failures.Add($"{relativePath}: Original={validationResult.OriginalCount}, V3={validationResult.V3Count}, Error={validationResult.ErrorMessage}");
				_output.WriteLine($"[FAIL] {relativePath}: {validationResult.ErrorMessage}");
			}
		}

		_output.WriteLine($"\n=== SUMMARY ===\nPassed: {passed}, Failed: {failed}");
		failed.Should().Be(0, $"All ARC files should pass validation. Failures:\n{string.Join("\n", failures)}");
	}

	/// <summary>
	/// Extended validation test - requires Titan Quest game installation.
	/// </summary>
	[Fact(Skip = "Requires Titan Quest game installation", SkipType = typeof(TestingConditions), SkipUnless = nameof(TestingConditions.IsGameInstalled))]
	public void ReadARCToC_NEW_AllArcFiles_ProducesValidResult()
	{
		int passed = 0, failed = 0;

		foreach (var arcFile in GetArcFiles())
		{
			var file = new ArcFile(arcFile);
			var result = _provider.ReadARCToC_NEW(file);

			if (result && file.FileHasBeenRead)
			{
				passed++;
				_output.WriteLine($"[PASS] {Path.GetFileName(arcFile)}");
			}
			else
			{
				failed++;
				_output.WriteLine($"[FAIL] {Path.GetFileName(arcFile)}");
			}
		}

		failed.Should().Be(0, $"All ARC files should be readable. Failures: {failed}");
	}

	/// <summary>
	/// Extended validation test - requires Titan Quest game installation.
	/// </summary>
	[Fact(Skip = "Requires Titan Quest game installation", SkipType = typeof(TestingConditions), SkipUnless = nameof(TestingConditions.IsGameInstalled))]
	public void ReadARCToC_OLD_AllArcFiles_ProducesValidResult()
	{
		int passed = 0, failed = 0;

		foreach (var arcFile in GetArcFiles())
		{
			var file = new ArcFile(arcFile);
			_provider.ReadARCToC_OLD(file);

			if (file.FileHasBeenRead && file.DirectoryEntries.Count > 0)
			{
				passed++;
			}
			else
			{
				failed++;
				_output.WriteLine($"[FAIL] {Path.GetFileName(arcFile)}: entries={file.DirectoryEntries.Count}");
			}
		}

		failed.Should().Be(0, $"All ARC files should be readable by OLD. Failures: {failed}");
	}

	/// <summary>
	/// Extended validation test - requires Titan Quest game installation.
	/// </summary>
	[Fact(Skip = "Requires Titan Quest game installation", SkipType = typeof(TestingConditions), SkipUnless = nameof(TestingConditions.IsGameInstalled))]
	public void V3AndOLD_AllArcFiles_SameEntryCount()
	{
		int passed = 0, failed = 0;

		foreach (var arcFile in GetArcFiles())
		{
			var fileOLD = new ArcFile(arcFile);
			var fileV3 = new ArcFile(arcFile);

			_provider.ReadARCToC_OLD(fileOLD);
			_provider.ReadARCToC_NEW(fileV3);

			if (fileOLD.DirectoryEntries.Count == fileV3.DirectoryEntries.Count)
			{
				passed++;
			}
			else
			{
				failed++;
				_output.WriteLine($"[FAIL] {Path.GetFileName(arcFile)}: OLD={fileOLD.DirectoryEntries.Count}, V3={fileV3.DirectoryEntries.Count}");
			}
		}

		failed.Should().Be(0, $"OLD and V3 should produce same count for all files. Failures: {failed}");
	}
}
