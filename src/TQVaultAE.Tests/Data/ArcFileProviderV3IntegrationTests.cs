using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Integration tests for ArcFileProvider ReadARCToC_NEW method comparing against original algorithm.
/// Uses real ARC files from the Titan Quest installation for validation.
/// </summary>
public class ArcFileProviderV3IntegrationTests : IDisposable
{
	private readonly ITestOutputHelper _output;
	private readonly Mock<ILogger<ArcFileProvider>> _mockLogger;
	private readonly Mock<IPathIO> _mockPathIO;
	private readonly Mock<IDirectoryIO> _mockDirectoryIO;
	private readonly Mock<IFileDataService> _mockFileDataService;
	private readonly Mock<IDecompressionService> _mockDecompressionService;
	private readonly UserSettings _userSettings;
	private readonly ArcFileProvider _provider;

	public ArcFileProviderV3IntegrationTests(ITestOutputHelper output)
	{
		_output = output;
		_mockLogger = new Mock<ILogger<ArcFileProvider>>();
		_mockPathIO = new Mock<IPathIO>();
		_mockDirectoryIO = new Mock<IDirectoryIO>();
		_mockFileDataService = new Mock<IFileDataService>();
		_mockDecompressionService = new Mock<IDecompressionService>();
		_userSettings = new UserSettings();

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

	/// <summary>
	/// Validates V3 against original algorithm using various ARC files from Titan Quest installation.
	/// Requires Titan Quest game installation to run.
	/// </summary>
	[Theory(Skip = "Requires Titan Quest game installation", SkipType = typeof(TestingConditions), SkipUnless = nameof(TestingConditions.IsGameInstalled))]
	[InlineData("Text/Text_EN.arc")]
	[InlineData("Resources/System.arc")]
	[InlineData("Resources/Items.arc")]
	[InlineData("Audio/Dialog.arc")]
	[InlineData("Resources/Creatures.arc")]
	[InlineData("Resources/Effects.arc")]
	[InlineData("Resources/Particles.arc")]
	public void ValidateV3AgainstOriginal_GameArcFiles_ResultsMatch(string relativePath)
	{
		// Arrange
		var arcPath = Path.Combine(TestingConditions.TitanQuestPath, relativePath);
		if (!File.Exists(arcPath))
		{
			Assert.Fail($"{arcPath} should exists!");
		}

		var file = new ArcFile(arcPath);

		// Act
		var validationResult = _provider.ValidateNEWAgainstOriginal(file);

		// Assert
		validationResult.IsValid.Should().BeTrue(because: $"Failed for {relativePath}: {validationResult.ErrorMessage}");
		validationResult.OriginalCount.Should().BeGreaterThan(0, $"{relativePath} should contain entries");
		validationResult.CountsMatch.Should().BeTrue($"Count mismatch for {relativePath}");
	}

	[Fact(Skip = "Requires Titan Quest game installation with Text_EN.arc", SkipType = typeof(TestingConditions), SkipUnless = nameof(TestingConditions.HasTextEnArc))]
	public void ReadARCToC_NEW_TextEN_ProducesNonEmptyResult()
	{
		// This test specifically checks that V3 produces non-empty results for Text_EN.arc
		// which is essential for the "Could not load Text DB" fix to work

		// Arrange
		var textArcPath = Path.Combine(TestingConditions.TitanQuestPath, "Text", "Text_EN.arc");

		var file = new ArcFile(textArcPath);

		// Act
		var result = _provider.ReadARCToC_NEW(file);

		// Assert
		result.Should().BeTrue("V3 should successfully read Text_EN.arc");
		file.DirectoryEntries.Should().NotBeEmpty("Text_EN.arc should have directory entries");
		file.FileHasBeenRead.Should().BeTrue();

		// Log the count for debugging
		_output.WriteLine($"Text_EN.arc V3 entry count: {file.DirectoryEntries.Count}");
	}

	[Fact(Skip = "Requires Titan Quest game installation with Text_EN.arc", SkipType = typeof(TestingConditions), SkipUnless = nameof(TestingConditions.HasTextEnArc))]
	public void ReadARCToC_Original_TextEN_ProducesNonEmptyResult()
	{
		// Baseline test: verify original algorithm produces non-empty results

		// Arrange
		var textArcPath = Path.Combine(TestingConditions.TitanQuestPath, "Text", "Text_EN.arc");

		var file = new ArcFile(textArcPath);

		// Act - ReadARCToC returns void, call it directly
		_provider.ReadARCToC(file);

		// Assert - check that entries were populated
		file.FileHasBeenRead.Should().BeTrue("Original should successfully read Text_EN.arc");
		file.DirectoryEntries.Should().NotBeEmpty("Text_EN.arc should have directory entries");

		// Log the count for debugging
		_output.WriteLine($"Text_EN.arc Original entry count: {file.DirectoryEntries.Count}");
	}
}
