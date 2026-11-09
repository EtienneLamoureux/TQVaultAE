using System.Buffers.Binary;
using System.Text;

namespace TQVaultAE.Domain.Helpers;

/// <summary>
/// Finds the database record ids a game save file refers to (<c>Player.chr</c>, <c>winsys.dxb</c>), without parsing it:
/// the saves store strings as a 32-bit length followed by the ASCII text, so every such string that looks like a record
/// id (<c>records\...\x.dbr</c>) is taken.
/// </summary>
public static class SaveRecordScanner
{
	private const int MinLength = 12; // "records\x.dbr"

	private const int MaxLength = 512;

	private static ReadOnlySpan<byte> Prefix => "records"u8;

	private static ReadOnlySpan<byte> Suffix => ".dbr"u8;

	/// <summary>
	/// Reads the distinct record ids of a save file.
	/// </summary>
	/// <param name="data">The file content.</param>
	/// <returns>The ids, lower case with backslashes.</returns>
	public static IReadOnlySet<string> ReadRecordIds(ReadOnlySpan<byte> data)
	{
		var ids = new HashSet<string>(StringComparer.Ordinal);

		for (int i = 0; i + 4 + MinLength <= data.Length; i++)
		{
			int length = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(i, 4));
			if (length < MinLength || length > MaxLength || i + 4 + length > data.Length)
				continue;

			var text = data.Slice(i + 4, length);
			if (!StartsWithIgnoreCase(text, Prefix) || !EndsWithIgnoreCase(text, Suffix) || !IsPrintableAscii(text))
				continue;

			ids.Add(Encoding.ASCII.GetString(text).Replace('/', '\\').ToLowerInvariant());
			i += 3 + length; // the string cannot hold another one
		}

		return ids;
	}

	private static bool StartsWithIgnoreCase(ReadOnlySpan<byte> text, ReadOnlySpan<byte> prefix)
		=> Ascii.EqualsIgnoreCase(text[..prefix.Length], prefix);

	private static bool EndsWithIgnoreCase(ReadOnlySpan<byte> text, ReadOnlySpan<byte> suffix)
		=> Ascii.EqualsIgnoreCase(text[^suffix.Length..], suffix);

	private static bool IsPrintableAscii(ReadOnlySpan<byte> text)
	{
		foreach (byte b in text)
		{
			if (b < 0x20 || b > 0x7E)
				return false;
		}

		return true;
	}
}
