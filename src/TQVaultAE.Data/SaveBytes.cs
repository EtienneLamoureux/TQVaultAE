namespace TQVaultAE.Data;

/// <summary>
/// Byte helpers shared by the save file writers.
/// </summary>
internal static class SaveBytes
{
	/// <summary>
	/// The bytes <paramref name="write"/> writes, in an array of their exact length.
	/// </summary>
	public static byte[] Encode(Action<BinaryWriter> write, int capacity = 2048)
	{
		using var stream = new MemoryStream(capacity);
		using (var writer = new BinaryWriter(stream))
			write(writer);

		return stream.ToArray();
	}

	/// <summary>
	/// A copy of <paramref name="source"/> where the bytes from <paramref name="start"/> up to <paramref name="end"/>
	/// (excluded) are replaced by <paramref name="replacement"/>.
	/// </summary>
	public static byte[] Splice(ReadOnlySpan<byte> source, int start, int end, ReadOnlySpan<byte> replacement)
	{
		var result = new byte[start + replacement.Length + (source.Length - end)];
		source[..start].CopyTo(result);
		replacement.CopyTo(result.AsSpan(start));
		source[end..].CopyTo(result.AsSpan(start + replacement.Length));
		return result;
	}
}
