using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Validation helper for comparing ARC algorithm implementations.
/// This code was moved from ArcFileProvider as it is test code, not production code.
/// </summary>
public static class ArcAlgorithmValidationHelper
{
	/// <summary>
	/// Validates that NEW produces identical results to the original algorithm.
	/// </summary>
	/// <param name="provider">The ArcFileProvider instance.</param>
	/// <param name="file">The ArcFile to validate.</param>
	/// <returns>Validation result with details.</returns>
	public static ArcValidationResult ValidateNEWAgainstOriginal(this ArcFileProvider provider, ArcFile file)
	{
		// Create copies for testing
		var fileOriginal = new ArcFile(file.FileName);
		var fileV3 = new ArcFile(file.FileName);

		// Run original algorithm
		provider.ReadARCToC_OLD(fileOriginal);

		// Run V3 algorithm
		provider.ReadARCToC_NEW(fileV3);

		// Compare results
		var result = new ArcValidationResult
		{
			OriginalCount = fileOriginal.DirectoryEntries.Count,
			V3Count = fileV3.DirectoryEntries.Count,
			CountsMatch = fileOriginal.DirectoryEntries.Count == fileV3.DirectoryEntries.Count
		};

		if (!result.CountsMatch)
		{
			result.ErrorMessage = $"Count mismatch: Original={result.OriginalCount}, V3={result.V3Count}";
			return result;
		}

		foreach (var kvp in fileOriginal.DirectoryEntries)
		{
			if (!fileV3.DirectoryEntries.TryGetValue(kvp.Key, out var v3Entry))
			{
				result.MissingKeys ??= new List<string>();
				result.MissingKeys.Add(kvp.Key);
				result.ErrorMessage = $"V3 missing key: {kvp.Key}";
				continue;
			}

			var original = kvp.Value;

			// Compare key fields
			if (original.StorageType != v3Entry.StorageType ||
				original.FileOffset != v3Entry.FileOffset ||
				original.CompressedSize != v3Entry.CompressedSize ||
				original.RealSize != v3Entry.RealSize)
			{
				result.FieldMismatch ??= new Dictionary<string, (int orig, int v3)>();
				result.FieldMismatch[kvp.Key] = (original.StorageType, v3Entry.StorageType);
				result.ErrorMessage = $"Mismatch for key {kvp.Key}";
			}
		}

		if (result.MissingKeys is null && result.FieldMismatch is null)
		{
			result.IsValid = true;
		}

		return result;
	}
}

/// <summary>
/// Result of ARC algorithm validation comparison.
/// </summary>
public class ArcValidationResult
{
	public bool IsValid { get; set; }
	public bool CountsMatch { get; set; }
	public int OriginalCount { get; set; }
	public int V3Count { get; set; }
	public string? ErrorMessage { get; set; }
	public List<string>? MissingKeys { get; set; }
	public Dictionary<string, (int orig, int v3)>? FieldMismatch { get; set; }
}
