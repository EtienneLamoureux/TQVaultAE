//-----------------------------------------------------------------------
// <copyright file="FileTime.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TQVaultAE.GameFiles;

/// <summary>FILETIME helpers of the ARZ and ARC readers.</summary>
internal static class FileTime
{
	private static readonly long MaxFileTime = DateTime.MaxValue.ToFileTimeUtc();

	/// <summary>Converts a FILETIME, mapping invalid values to <see cref="DateTime.MinValue"/> (UTC).</summary>
	public static DateTime ToUtc(long fileTime) =>
		fileTime > 0 && fileTime <= MaxFileTime ? DateTime.FromFileTimeUtc(fileTime) : DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
}
