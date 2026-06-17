using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Detailed debugging tests for comparing OLD vs V3 ARC reading algorithms.
/// This helps identify exactly where V3 fails compared to OLD.
/// </summary>
public class ArcFileProviderDebugTests : IDisposable
{
	private readonly ITestOutputHelper _output;
	private readonly Mock<ILogger<ArcFileProvider>> _mockLogger;
	private readonly Mock<IPathIO> _mockPathIO;
	private readonly Mock<IDirectoryIO> _mockDirectoryIO;
	private readonly Mock<IFileDataService> _mockFileDataService;
	private readonly Mock<IDecompressionService> _mockDecompressionService;
	private readonly UserSettings _userSettings;
	private readonly ArcFileProvider _provider;

	public ArcFileProviderDebugTests(ITestOutputHelper output)
	{
		_output = output;
		_mockLogger = new Mock<ILogger<ArcFileProvider>>();
		_mockPathIO = new Mock<IPathIO>();
		_mockDirectoryIO = new Mock<IDirectoryIO>();
		_mockFileDataService = new Mock<IFileDataService>();
		_mockDecompressionService = new Mock<IDecompressionService>();
		_userSettings = new UserSettings();
		_userSettings.ARCFileDebugLevel = 3; // Maximum debug logging

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

	[Fact(Skip = "Requires Titan Quest game installation with Text_fr.arc", SkipType = typeof(TestingConditions), SkipUnless = nameof(TestingConditions.HasTextFrArc))]
	public void Debug_TextFR_Arc_CompareOLDvsV3_StepByStep()
	{
		// This test specifically debugs the Text_fr.arc file that fails in production

		// Arrange
		var textFrArcPath = Path.Combine(TestingConditions.TitanQuestPath, "Text", "Text_fr.arc");

		textFrArcPath.Should().NotBeNullOrEmpty("Text_FR.arc path should be valid");
		File.Exists(textFrArcPath).Should().BeTrue($"File should exist: {textFrArcPath}");

		var debugOutput = new System.Text.StringBuilder();
		debugOutput.AppendLine($"=== Debugging {textFrArcPath} ===");

		// Read header bytes to verify file structure
		using (var fs = new FileStream(textFrArcPath, FileMode.Open, FileAccess.Read))
		{
			var headerBytes = new byte[0x21];
			fs.Read(headerBytes, 0, headerBytes.Length);

			debugOutput.AppendLine($"Header bytes: {BitConverter.ToString(headerBytes.Take(10).ToArray())}");
			debugOutput.AppendLine($"ARC magic: {(char)headerBytes[0]}{(char)headerBytes[1]}{(char)headerBytes[2]}");

			int numEntries = SpanHelper.ReadInt32LittleEndian(headerBytes, 0x08);
			int numParts = SpanHelper.ReadInt32LittleEndian(headerBytes, 0x0C);
			int tocOffset = SpanHelper.ReadInt32LittleEndian(headerBytes, 0x18);

			debugOutput.AppendLine($"numEntries={numEntries}, numParts={numParts}, tocOffset={tocOffset}");
		}

		// Create two copies - one for OLD, one for V3
		var fileOLD = new ArcFile(textFrArcPath);
		var fileV3 = new ArcFile(textFrArcPath);

		// Act - Read with OLD algorithm
		_provider.ReadARCToC_OLD(fileOLD);
		debugOutput.AppendLine($"\nOLD algorithm result: {fileOLD.DirectoryEntries.Count} entries");

		// Act - Read with V3 algorithm
		_provider.ReadARCToC_NEW(fileV3);
		debugOutput.AppendLine($"V3 algorithm result: {fileV3.DirectoryEntries.Count} entries");

		// Detailed comparison
		debugOutput.AppendLine("\n=== Comparing entries step by step ===");

		// Find entries that OLD has but V3 doesn't
		var missingInV3 = fileOLD.DirectoryEntries.Keys
			.Where(k => !fileV3.DirectoryEntries.ContainsKey(k))
			.ToList();

		// Find entries that V3 has but OLD doesn't
		var extraInV3 = fileV3.DirectoryEntries.Keys
			.Where(k => !fileOLD.DirectoryEntries.ContainsKey(k))
			.ToList();

		debugOutput.AppendLine($"\nEntries missing in V3: {missingInV3.Count}");
		foreach (var key in missingInV3.Take(10))
		{
			debugOutput.AppendLine($"  - {key}");
		}

		debugOutput.AppendLine($"\nExtra entries in V3: {extraInV3.Count}");
		foreach (var key in extraInV3.Take(10))
		{
			debugOutput.AppendLine($"  - {key}");
		}

		// Compare common entries field by field
		var commonKeys = fileOLD.DirectoryEntries.Keys
			.Intersect(fileV3.DirectoryEntries.Keys)
			.ToList();

		debugOutput.AppendLine($"\nCommon entries: {commonKeys.Count}");

		var fieldMismatches = new List<string>();
		foreach (var key in commonKeys)
		{
			var oldEntry = fileOLD.DirectoryEntries[key];
			var v3Entry = fileV3.DirectoryEntries[key];

			if (oldEntry.StorageType != v3Entry.StorageType)
				fieldMismatches.Add($"{key}: StorageType OLD={oldEntry.StorageType} V3={v3Entry.StorageType}");
			if (oldEntry.FileOffset != v3Entry.FileOffset)
				fieldMismatches.Add($"{key}: FileOffset OLD={oldEntry.FileOffset} V3={v3Entry.FileOffset}");
			if (oldEntry.CompressedSize != v3Entry.CompressedSize)
				fieldMismatches.Add($"{key}: CompressedSize OLD={oldEntry.CompressedSize} V3={v3Entry.CompressedSize}");
			if (oldEntry.RealSize != v3Entry.RealSize)
				fieldMismatches.Add($"{key}: RealSize OLD={oldEntry.RealSize} V3={v3Entry.RealSize}");
		}

		debugOutput.AppendLine($"\nField mismatches in common entries: {fieldMismatches.Count}");
		foreach (var mismatch in fieldMismatches.Take(20))
		{
			debugOutput.AppendLine($"  - {mismatch}");
		}

		// Now let's check which entries are marked as IsActive in OLD vs V3
		debugOutput.AppendLine("\n=== Checking IsActive status ===");

		// Read raw file data to check each entry's IsActive status
		using (var fs = new FileStream(textFrArcPath, FileMode.Open, FileAccess.Read))
		using (var br = new BinaryReader(fs))
		{
			// Read header
			br.ReadBytes(8); // Skip 8 bytes
			int numEntries = br.ReadInt32();
			int numParts = br.ReadInt32();
			br.ReadBytes(8); // Skip to tocOffset
			int tocOffset = br.ReadInt32();

			debugOutput.AppendLine($"File header: numEntries={numEntries}, numParts={numParts}, tocOffset={tocOffset}");

			// Read part table
			var parts = new (int offset, int compressedSize, int realSize)[numParts];
			fs.Seek(tocOffset, SeekOrigin.Begin);
			for (int i = 0; i < numParts; i++)
			{
				parts[i] = (br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
			}

			// Calculate where records start
			int fileLength = (int)fs.Length;
			int fileRecordOffsetBytes = 44 * numEntries;
			long fileRecordStart = fileLength - fileRecordOffsetBytes;

			debugOutput.AppendLine($"fileLength={fileLength}, fileRecordOffsetBytes={fileRecordOffsetBytes}, fileRecordStart={fileRecordStart}");

			// Read and display first few records
			fs.Seek(fileRecordStart, SeekOrigin.Begin);
			for (int i = 0; i < Math.Min(10, numEntries); i++)
			{
				int storageType = br.ReadInt32();
				int fileOffset = br.ReadInt32();
				int compressedSize = br.ReadInt32();
				int realSize = br.ReadInt32();
				br.ReadInt32(); // crap
				br.ReadInt32(); // crap
				br.ReadInt32(); // crap
				int numberOfParts = br.ReadInt32();
				int firstPart = br.ReadInt32();
				int filenameLength = br.ReadInt32();
				int filenameOffset = br.ReadInt32();

				bool isActiveOLD = (storageType == 1) ? true : numberOfParts > 0;
				bool isActiveV3 = (storageType == 1) ? true : (firstPart < 0 || firstPart + numberOfParts > numParts ? false : numberOfParts > 0);

				debugOutput.AppendLine($"Record[{i}]: storageType={storageType}, numberOfParts={numberOfParts}, firstPart={firstPart}, isActiveOLD={isActiveOLD}, isActiveV3={isActiveV3}");
			}
		}

		// Summary
		debugOutput.AppendLine($"\n=== SUMMARY ===");
		debugOutput.AppendLine($"OLD entries: {fileOLD.DirectoryEntries.Count}");
		debugOutput.AppendLine($"V3 entries: {fileV3.DirectoryEntries.Count}");
		debugOutput.AppendLine($"Missing in V3: {missingInV3.Count}");
		debugOutput.AppendLine($"Field mismatches: {fieldMismatches.Count}");

		// Write to file for inspection
		var debugFile = Path.Combine(Path.GetTempPath(), $"arc_debug_{Guid.NewGuid():N}.txt");
		File.WriteAllText(debugFile, debugOutput.ToString());
		_output.WriteLine($"Debug output written to: {debugFile}");

		// Assert - for now just log, don't fail
		fileOLD.DirectoryEntries.Count.Should().BeGreaterThan(0, "OLD should find entries");
		fileV3.DirectoryEntries.Count.Should().BeGreaterThan(0, "V3 should find entries");
		fileV3.DirectoryEntries.Count.Should().Be(fileOLD.DirectoryEntries.Count, "V3 should match OLD count");
	}

	[Fact(Skip = "Requires Titan Quest game installation with Text_EN.arc", SkipType = typeof(TestingConditions), SkipUnless = nameof(TestingConditions.HasTextEnArc))]
	public void Debug_TextEN_Arc_CompareOLDvsV3_StepByStep()
	{
		// This test compares Text_EN.arc which WORKS in tests

		// Arrange
		var textEnArcPath = Path.Combine(TestingConditions.TitanQuestPath, "Text", "Text_EN.arc");

		_output.WriteLine($"=== Debugging {textEnArcPath} ===");

		var fileOLD = new ArcFile(textEnArcPath);
		var fileV3 = new ArcFile(textEnArcPath);

		_provider.ReadARCToC_OLD(fileOLD);
		_provider.ReadARCToC_NEW(fileV3);

		_output.WriteLine($"OLD entries: {fileOLD.DirectoryEntries.Count}");
		_output.WriteLine($"V3 entries: {fileV3.DirectoryEntries.Count}");

		// Assert
		fileOLD.DirectoryEntries.Count.Should().BeGreaterThan(0);
		fileV3.DirectoryEntries.Count.Should().BeGreaterThan(0);
		fileV3.DirectoryEntries.Count.Should().Be(fileOLD.DirectoryEntries.Count);
	}
}
