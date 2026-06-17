using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace TQVaultAE.Domain.Helpers;

/// <summary>
/// Provides optimized span-based string and binary operations to reduce allocations.
/// </summary>
public static class SpanHelper
{
	/// <summary>
	/// Gets an uppercase ReadOnlySpan of the string without allocation.
	/// Uses pooled array for the conversion.
	/// </summary>
	/// <param name="str">The string to convert to uppercase span.</param>
	/// <returns>A pooled char array containing the uppercase string.</returns>
	public static PooledCharArray ToUpperSpan(string str)
	{
		if (string.IsNullOrEmpty(str))
			return PooledCharArray.Empty;

		// Use pooled array for conversion
		var pooled = PooledCharArray.Rent(str.Length);

		// Copy and convert to uppercase in one pass
		for (int i = 0; i < str.Length; i++)
			pooled.Span[i] = char.ToUpperInvariant(str[i]);

		return pooled;
	}

	/// <summary>
	/// Checks if the string starts with the specified prefix (case-insensitive).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool StartsWithIgnoreCase(ReadOnlySpan<char> span, ReadOnlySpan<char> prefix)
	{
		if (span.Length < prefix.Length)
			return false;

		for (int i = 0; i < prefix.Length; i++)
		{
			if (char.ToUpperInvariant(span[i]) != char.ToUpperInvariant(prefix[i]))
				return false;
		}
		return true;
	}

	/// <summary>
	/// Checks if two spans are equal (case-insensitive).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool EqualsIgnoreCase(ReadOnlySpan<char> span, ReadOnlySpan<char> other)
	{
		if (span.Length != other.Length)
			return false;

		for (int i = 0; i < span.Length; i++)
		{
			if (char.ToUpperInvariant(span[i]) != char.ToUpperInvariant(other[i]))
				return false;
		}
		return true;
	}

	/// <summary>
	/// Creates a new string by concatenating a prefix with a sliced portion of the input span.
	/// </summary>
	public static string ConcatWithSlice(ReadOnlySpan<char> prefix, ReadOnlySpan<char> span, int skipPrefixChars)
	{
		int suffixLength = span.Length - skipPrefixChars;
		var result = new char[prefix.Length + suffixLength];
		prefix.CopyTo(result);
		span.Slice(skipPrefixChars).CopyTo(result.AsSpan(prefix.Length));
		return new string(result);
	}

	/// <summary>
	/// Creates a new string by concatenating a prefix with a middle portion of the input span.
	/// </summary>
	public static string ConcatWithSlice(ReadOnlySpan<char> prefix, ReadOnlySpan<char> span, int skipPrefixChars, int suffixLength)
	{
		var result = new char[prefix.Length + suffixLength];
		prefix.CopyTo(result);
		span.Slice(skipPrefixChars, suffixLength).CopyTo(result.AsSpan(prefix.Length));
		return new string(result);
	}

	/// <summary>
	/// Creates a new string by concatenating two spans.
	/// </summary>
	public static string ConcatSpans(ReadOnlySpan<char> first, ReadOnlySpan<char> second)
	{
		var result = new char[first.Length + second.Length];
		first.CopyTo(result);
		second.CopyTo(result.AsSpan(first.Length));
		return new string(result);
	}

	#region Binary Reading Operations for ReadOnlySpan<byte>

	/// <summary>
	/// Reads a 32-bit signed integer in little-endian format.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int ReadInt32LittleEndian(ReadOnlySpan<byte> span, int offset)
		=> BinaryPrimitives.ReadInt32LittleEndian(span.Slice(offset));

	/// <summary>
	/// Reads a 32-bit unsigned integer in little-endian format.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint ReadUInt32LittleEndian(ReadOnlySpan<byte> span, int offset)
		=> BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(offset));

	/// <summary>
	/// Reads a single byte at the specified offset.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte ReadByte(ReadOnlySpan<byte> span, int offset)
		=> span[offset];

	/// <summary>
	/// Reads an ASCII string that is null-terminated.
	/// Returns the string without the null terminator.
	/// </summary>
	/// <param name="span">The span containing the data.</param>
	/// <param name="offset">Starting offset for reading.</param>
	/// <param name="maxLength">Maximum bytes to read (for bounds safety).</param>
	/// <param name="bytesRead">Returns the number of bytes read including the null terminator.</param>
	/// <returns>The ASCII string, or null if no data found.</returns>
	public static string? ReadNullTerminatedAscii(ReadOnlySpan<byte> span, int offset, int maxLength, out int bytesRead)
	{
		bytesRead = 0;
		if (offset >= span.Length)
			return null;

		int end = offset;
		while (end < span.Length && end < offset + maxLength)
		{
			if (span[end] == 0x00)
			{
				bytesRead = end - offset + 1;
				if (bytesRead == 1)
					return string.Empty;

				// Check for 0x03 marker (inactive/null file marker)
				if (span[end - 1] == 0x03)
				{
					bytesRead = end - offset; // Exclude 0x03
					return null;
				}

				return Encoding.ASCII.GetString(span.Slice(offset, bytesRead - 1));
			}
			end++;
		}

		// No null terminator found - read up to maxLength
		bytesRead = Math.Min(maxLength, span.Length - offset);
		if (bytesRead == 0)
			return null;

		return Encoding.ASCII.GetString(span.Slice(offset, bytesRead));
	}

	/// <summary>
	/// Checks if two byte spans are equal.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool ByteSpanEquals(ReadOnlySpan<byte> first, ReadOnlySpan<byte> second)
	{
		if (first.Length != second.Length)
			return false;

		for (int i = 0; i < first.Length; i++)
		{
			if (first[i] != second[i])
				return false;
		}
		return true;
	}

	/// <summary>
	/// Creates a byte span from an array segment.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<byte> CreateSpan(byte[] array, int offset, int length)
		=> new ReadOnlySpan<byte>(array, offset, length);

	/// <summary>
	/// Searches for a byte pattern in a span and returns the starting index.
	/// </summary>
	/// <param name="span">The span to search in.</param>
	/// <param name="start">The starting position for the search.</param>
	/// <param name="pattern">The byte pattern to find.</param>
	/// <returns>The index of the first match, or -1 if not found.</returns>
	public static int FindPattern(ReadOnlySpan<byte> span, int start, ReadOnlySpan<byte> pattern)
	{
		if (pattern.Length == 0 || start >= span.Length)
			return -1;

		for (int i = start; i <= span.Length - pattern.Length; i++)
		{
			bool match = true;
			for (int j = 0; j < pattern.Length; j++)
			{
				if (span[i + j] != pattern[j])
				{
					match = false;
					break;
				}
			}
			if (match)
				return i;
		}
		return -1;
	}

	/// <summary>
	/// Reads a null-terminated ASCII string from a span, handling the 0x03 marker
	/// used in ARC files to mark inactive/null file entries.
	/// Matches the original algorithm behavior from ArcFileProvider.ReadARCToC.
	/// </summary>
	/// <param name="span">The span containing the data.</param>
	/// <param name="offset">Starting offset for reading.</param>
	/// <param name="maxBufferSize">Maximum buffer size (typically 2048 for ARC files).</param>
	/// <param name="bytesConsumed">Returns the number of bytes consumed including the terminator.</param>
	/// <returns>
	/// The ASCII string without the terminator, "Null File {index}" format for 0x03 markers,
	/// or null if offset is out of bounds.
	/// </returns>
	public static string? ReadArcNullTerminatedString(ReadOnlySpan<byte> span, int offset, int maxBufferSize, out int bytesConsumed)
	{
		bytesConsumed = 0;
		if (offset >= span.Length || offset < 0)
			return null;

		int bufferSize = 0;

		while (offset + bufferSize < span.Length && bufferSize < maxBufferSize)
		{
			byte currentByte = span[offset + bufferSize];
			bufferSize++;

			if (currentByte == 0x00)
			{
				// Null terminator found - string ends here
				bytesConsumed = bufferSize;
				if (bufferSize == 1)
					return string.Empty;

				return Encoding.ASCII.GetString(span.Slice(offset, bufferSize - 1));
			}

			if (currentByte == 0x03)
			{
				// 0x03 marker indicates inactive/null file
				// Match original behavior: backup, set buffer[bufferSize-1] = 0x00, break
				bufferSize--; // Back up to exclude 0x03
				bytesConsumed = bufferSize + 1; // But count it as consumed
				if (bufferSize == 0)
					return null; // No filename available

				return Encoding.ASCII.GetString(span.Slice(offset, bufferSize));
			}
		}

		// No terminator found - return what we have
		bytesConsumed = bufferSize;
		if (bufferSize == 0)
			return null;

		return Encoding.ASCII.GetString(span.Slice(offset, bufferSize));
	}

	/// <summary>
	/// Reads a sequence of Int32 values from a byte span.
	/// </summary>
	/// <param name="span">The span to read from.</param>
	/// <param name="offset">Starting offset.</param>
	/// <param name="count">Number of Int32 values to read.</param>
	/// <returns>Array of Int32 values.</returns>
	public static int[] ReadInt32Array(ReadOnlySpan<byte> span, int offset, int count)
	{
		var result = new int[count];
		for (int i = 0; i < count; i++)
		{
			result[i] = ReadInt32LittleEndian(span, offset + (i * 4));
		}
		return result;
	}

	/// <summary>
	/// Creates an ArcPartEntry struct from a 12-byte span at the given offset.
	/// Format: fileOffset(4), compressedSize(4), realSize(4)
	/// </summary>
	public static ArcPartEntrySpan ReadArcPartEntry(ReadOnlySpan<byte> span, int offset)
		=> new()
		{
			FileOffset = ReadInt32LittleEndian(span, offset),
			CompressedSize = ReadInt32LittleEndian(span, offset + 4),
			RealSize = ReadInt32LittleEndian(span, offset + 8)
		};

	/// <summary>
	/// Creates an ArcDirEntrySpan from a 44-byte span at the given offset.
	/// Format: storageType(4), fileOffset(4), compressedSize(4), realSize(4),
	///         crap x3 (12), numberOfParts(4), firstPart(4), filenameLength(4), filenameOffset(4)
	/// </summary>
	public static ArcDirEntrySpan ReadArcDirEntry(ReadOnlySpan<byte> span, int offset)
		=> new()
		{
			StorageType = ReadInt32LittleEndian(span, offset),
			FileOffset = ReadInt32LittleEndian(span, offset + 4),
			CompressedSize = ReadInt32LittleEndian(span, offset + 8),
			RealSize = ReadInt32LittleEndian(span, offset + 12),
			NumberOfParts = ReadInt32LittleEndian(span, offset + 28),
			FirstPart = ReadInt32LittleEndian(span, offset + 32),
			FilenameLength = ReadInt32LittleEndian(span, offset + 36),
			FilenameOffset = ReadInt32LittleEndian(span, offset + 40)
		};

	/// <summary>
	/// Determines if a record should be marked active based on storageType and numberOfParts.
	/// Matches the logic in ArcDirEntry.IsActive.
	/// </summary>
	/// <param name="storageType">The storage type (1 = uncompressed, 3 = compressed).</param>
	/// <param name="numberOfParts">Number of parts for this entry.</param>
	/// <returns>True if the entry is active.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsActiveArcEntry(int storageType, int numberOfParts)
		=> (storageType == 1) ? true : numberOfParts > 0;

	#endregion
}

/// <summary>
/// Lightweight struct representing an ARC part entry when reading from a span.
/// </summary>
public readonly struct ArcPartEntrySpan
{
	public int FileOffset { get; init; }
	public int CompressedSize { get; init; }
	public int RealSize { get; init; }
}

/// <summary>
/// Lightweight struct representing an ARC directory entry when reading from a span.
/// </summary>
public readonly struct ArcDirEntrySpan
{
	public int StorageType { get; init; }
	public int FileOffset { get; init; }
	public int CompressedSize { get; init; }
	public int RealSize { get; init; }
	public int NumberOfParts { get; init; }
	public int FirstPart { get; init; }
	public int FilenameLength { get; init; }
	public int FilenameOffset { get; init; }
}

/// <summary>
/// Represents a pooled char array that should be returned to the pool when disposed.
/// </summary>
public sealed class PooledCharArray : IDisposable
{
	private char[]? _array;
	private int _length;

	/// <summary>
	/// Gets the span of the pooled array.
	/// </summary>
	public Span<char> Span => _array is null ? Span<char>.Empty : _array.AsSpan(0, _length);

	/// <summary>
	/// Gets the length of the data in the pooled array.
	/// </summary>
	public int Length => _length;

	/// <summary>
	/// An empty pooled array instance.
	/// </summary>
	public static PooledCharArray Empty { get; } = new(Array.Empty<char>(), 0);

	private PooledCharArray(char[] array, int length)
	{
		_array = array;
		_length = length;
	}

	/// <summary>
	/// Creates a pooled array of the specified length.
	/// </summary>
	public static PooledCharArray Rent(int length)
	{
		if (length <= 0)
			return Empty;

		return new PooledCharArray(ArrayPool<char>.Shared.Rent(length), length);
	}

	/// <summary>
	/// Creates a pooled array from an existing string.
	/// </summary>
	public static PooledCharArray FromString(string? str)
	{
		if (string.IsNullOrEmpty(str))
			return Empty;

		var pooled = Rent(str.Length);
		str.CopyTo(pooled.Span);
		return pooled;
	}

	/// <summary>
	/// Returns the pooled array to the pool.
	/// </summary>
	public void Dispose()
	{
		if (_array is not null)
		{
			ArrayPool<char>.Shared.Return(_array);
			_array = null;
			_length = 0;
		}
	}
}

/// <summary>
/// Represents a pooled byte array that should be returned to the pool when disposed.
/// </summary>
public sealed class PooledByteArray : IDisposable
{
	private byte[]? _array;
	private int _length;

	/// <summary>
	/// Gets the span of the pooled array.
	/// </summary>
	public Span<byte> Span => _array is null ? Span<byte>.Empty : _array.AsSpan(0, _length);

	/// <summary>
	/// Gets the read-only span of the pooled array.
	/// </summary>
	public ReadOnlySpan<byte> ReadOnlySpan => _array is null ? ReadOnlySpan<byte>.Empty : _array.AsSpan(0, _length);

	/// <summary>
	/// Gets the length of the data in the pooled array.
	/// </summary>
	public int Length => _length;

	/// <summary>
	/// An empty pooled array instance.
	/// </summary>
	public static PooledByteArray Empty { get; } = new(Array.Empty<byte>(), 0);

	private PooledByteArray(byte[] array, int length)
	{
		_array = array;
		_length = length;
	}

	/// <summary>
	/// Creates a pooled array of the specified length.
	/// </summary>
	public static PooledByteArray Rent(int length)
	{
		if (length <= 0)
			return Empty;

		return new PooledByteArray(ArrayPool<byte>.Shared.Rent(length), length);
	}

	/// <summary>
	/// Returns the pooled array to the pool.
	/// </summary>
	public void Dispose()
	{
		if (_array is not null)
		{
			ArrayPool<byte>.Shared.Return(_array);
			_array = null;
			_length = 0;
		}
	}
}
