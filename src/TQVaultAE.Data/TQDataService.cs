//-----------------------------------------------------------------------
// <copyright file="TQData.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.Application.Contracts.Services;
using Microsoft.Extensions.Logging;
using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using TQVaultAE.Logs;

namespace TQVaultAE.Data;

/// <summary>
/// TQData is used to store information about reading and writing the data files in TQ.
/// </summary>
public class TQDataService : ITQDataService
{
	public const int BEGIN_BLOCK_VALUE = -1340212530;
	public const int END_BLOCK_VALUE = -559038242;

	public int BeginBlockValue => BEGIN_BLOCK_VALUE;
	public int EndBlockValue => END_BLOCK_VALUE;

	private readonly ILogger Log;

	/// <summary>
	/// Lazy-initialized CP1252 encoding for Titan Quest file parsing.
	/// Encoding.RegisterProvider is idempotent - safe to call multiple times.
	/// </summary>
	private static readonly Lazy<Encoding> _encoding1252 = new(static () =>
	{
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		return Encoding.GetEncoding(1252);
	});

	/// <summary>
	/// Gets the CP1252 (Windows-1252) encoding used for TQ file parsing.
	/// </summary>
	public static Encoding Encoding1252 => _encoding1252.Value;

	/// <summary>
	/// Gets the Unicode UTF-16 encoding.
	/// </summary>
	public static Encoding EncodingUnicode => Encoding.Unicode;

	public TQDataService(ILogger<TQDataService> log)
	{
		this.Log = log;
	}

	/// <summary>
	/// Validates that the next string is a certain value and throws an exception if it is not.
	/// </summary>
	/// <param name="value">value to be validated</param>
	/// <param name="reader">BinaryReader instance</param>
	public void ValidateNextString(string value, BinaryReader reader)
	{
		string label = ReadCString(reader);
		if (!label.ToUpperInvariant().Equals(value.ToUpperInvariant()))
		{
			var ex = new ArgumentException(string.Format(
				CultureInfo.InvariantCulture,
				"Error reading file at position {2}.  Expecting '{0}'.  Got '{1}'",
				value,
				label,
				reader.BaseStream.Position - label.Length - 4));
			Log.ErrorException(ex);
			throw ex;
		}
	}

	public bool MatchNextString(string value, BinaryReader reader)
	{
		long readerPosition = reader.BaseStream.Position;

		string label = ReadCString(reader);
		reader.BaseStream.Position = readerPosition;

		if (!label.ToUpperInvariant().Equals(value.ToUpperInvariant()))
			return false;

		return true;
	}

	/// <summary>
	/// Validates that the next string matches the expected value using span-based parsing.
	/// Does not throw - returns true if match, false otherwise.
	/// Advances offset by the string length on match.
	/// Enables bounds-check elimination in high-frequency parsing paths.
	/// </summary>
	/// <param name="expectedValue">The expected string value</param>
	/// <param name="data">ReadOnlySpan of binary data</param>
	/// <param name="offset">Offset that will be advanced on match</param>
	/// <returns>True if the next string matches expectedValue</returns>
	public bool ValidateNextString(string expectedValue, ReadOnlySpan<byte> data, ref int offset)
	{
		var savedOffset = offset;
		string label = ReadCString(data, ref offset);
		if (label.Equals(expectedValue, StringComparison.OrdinalIgnoreCase))
			return true;

		// Restore offset if no match
		offset = savedOffset;
		return false;
	}

	/// <summary>
	/// Writes a string along with its length to the file.
	/// </summary>
	/// <param name="writer">BinaryWriter instance</param>
	/// <param name="value">string value to be written.</param>
	public void WriteCString(BinaryWriter writer, string value)
	{
		// Convert the string to ascii
		// Vorbis' fix for extended characters in the database.
		byte[] rawstring = Encoding1252.GetBytes(value);

		// Write the 4-byte length of the string
		writer.Write(rawstring.Length);

		// now write the string
		writer.Write(rawstring);
	}

	/// <summary>
	/// Reads a string from the binary stream.
	/// Expects an integer length value followed by the actual string of the stated length.
	/// </summary>
	/// <param name="reader">BinaryReader instance</param>
	/// <returns>string of data that was read</returns>
	public string ReadCString(BinaryReader reader)
	{
		// first 4 bytes is the string length, followed by the string.
		int len = reader.ReadInt32();

		// Convert the next len bytes into a string
		// Vorbis' fix for extended characters in the database.
		byte[] rawData = reader.ReadBytes(len);

		char[] chars = new char[Encoding1252.GetCharCount(rawData, 0, len)];
		Encoding1252.GetChars(rawData, 0, len, chars, 0);

		string ans = new string(chars);

		return ans;
	}

	/// <summary>
	/// Reads a string from the binary stream.
	/// Expects an integer length value followed by the actual string of the stated length.
	/// </summary>
	/// <param name="reader">BinaryReader instance</param>
	/// <returns>string of data that was read</returns>
	public string ReadUTF16String(BinaryReader reader)
	{
		// first 4 bytes is the string length, followed by the string.
		int len = reader.ReadInt32();
		len *= 2;// 2 byte chars
		var rawData = reader.ReadBytes(len);

		//convert bytes string
		return EncodingUnicode.GetString(rawData);
	}

	/// <summary>
	/// Reads a length-prefixed string from a span using CP1252 encoding.
	/// Enables bounds-check elimination in high-frequency parsing paths.
	/// </summary>
	/// <param name="data">ReadOnlySpan of binary data</param>
	/// <param name="offset">Offset that will be advanced by the method</param>
	/// <returns>The decoded string</returns>
	public string ReadCString(ReadOnlySpan<byte> data, ref int offset)
	{
		int len = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(offset));
		offset += sizeof(int);
		var stringData = data.Slice(offset, len);
		offset += len;
		return Encoding1252.GetString(stringData);
	}

	/// <summary>
	/// Reads a length-prefixed UTF-16 string from a span.
	/// Enables bounds-check elimination in high-frequency parsing paths.
	/// </summary>
	/// <param name="data">ReadOnlySpan of binary data</param>
	/// <param name="offset">Offset that will be advanced by the method</param>
	/// <returns>The decoded string</returns>
	public string ReadUTF16String(ReadOnlySpan<byte> data, ref int offset)
	{
		int charCount = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(offset));
		offset += sizeof(int);
		int byteCount = charCount * 2;
		var stringData = data.Slice(offset, byteCount);
		offset += byteCount;
		return Encoding.Unicode.GetString(stringData);
	}

	public (int indexOf, int valueOffset, int nextOffset, byte[] valueAsByteArray, int valueAsInt) WriteIntAfter(byte[] playerFileContent, string keyToLookFor, int newValue, int offset = 0)
	{
		var found = ReadIntAfter(playerFileContent, keyToLookFor, offset);
		if (found.indexOf != -1)
		{
			var newValueBytes = BitConverter.GetBytes(newValue);
			Array.ConstrainedCopy(newValueBytes, 0, playerFileContent, found.valueOffset, sizeof(int));
		}
		return found;
	}

	public (int indexOf, int valueOffset, int nextOffset, byte[] valueAsByteArray, float valueAsFloat) WriteFloatAfter(byte[] playerFileContent, string keyToLookFor, float newValue, int offset = 0)
	{
		var found = ReadFloatAfter(playerFileContent, keyToLookFor, offset);
		if (found.indexOf != -1)
		{
			var newValueBytes = BitConverter.GetBytes(newValue);
			Array.ConstrainedCopy(newValueBytes, 0, playerFileContent, found.valueOffset, sizeof(float));
		}
		return found;
	}

	static byte[] Empty = [];

	public (int indexOf, int valueOffset, int nextOffset, byte[] valueAsByteArray, float valueAsFloat) ReadFloatAfter(byte[] playerFileContent, string keyToLookFor, int offset = 0)
	{
		var idx = BinaryFindKey(playerFileContent, keyToLookFor, offset);
		byte[] value = Empty;

		if (idx.indexOf == -1)// Not found
			return (idx.indexOf, idx.nextOffset, idx.nextOffset, value, 0);

		var span = playerFileContent.AsSpan(idx.nextOffset, sizeof(float));
		value = span.ToArray();
		return (idx.indexOf, idx.nextOffset, idx.nextOffset + sizeof(float), value, BitConverter.ToSingle(value, 0));
	}

	public (int indexOf, int valueOffset, int nextOffset, byte[] valueAsByteArray, int valueAsInt) ReadIntAfter(byte[] playerFileContent, string keyToLookFor, int offset = 0)
	{
		var idx = BinaryFindKey(playerFileContent, keyToLookFor, offset);
		byte[] value = Empty;

		if (idx.indexOf == -1)// Not found
			return (idx.indexOf, idx.nextOffset, idx.nextOffset, value, 0);

		var span = playerFileContent.AsSpan(idx.nextOffset, sizeof(int));
		value = span.ToArray();
		int intValue = BinaryPrimitives.ReadInt32LittleEndian(span);
		return (idx.indexOf, idx.nextOffset, idx.nextOffset + sizeof(int), value, intValue);
	}

	public (int indexOf, int valueOffset, int nextOffset, int valueLen, byte[] valueAsByteArray, string valueAsString) ReadCStringAfter(byte[] playerFileContent, string keyToLookFor, int offset = 0)
	{
		var idx = BinaryFindKey(playerFileContent, keyToLookFor, offset);
		byte[] value = Empty;

		if (idx.indexOf == -1)// Not found
			return (idx.indexOf, idx.nextOffset, idx.nextOffset, 0, Empty, "");

		// Read the string length (4 bytes)
		var lenSpan = playerFileContent.AsSpan(idx.nextOffset, sizeof(int));
		int len = BinaryPrimitives.ReadInt32LittleEndian(lenSpan);
		// Read the string data
		var stringSpan = playerFileContent.AsSpan(idx.nextOffset + sizeof(int), len);
		value = stringSpan.ToArray();
		return (idx.indexOf, idx.nextOffset, idx.nextOffset + sizeof(int) + len, len, value, Encoding1252.GetString(value));
	}

	public (int indexOf, int valueOffset, int nextOffset, int valueLen, byte[] valueAsByteArray, string valueAsString) ReadUnicodeStringAfter(byte[] playerFileContent, string keyToLookFor, int offset = 0)
	{
		var idx = BinaryFindKey(playerFileContent, keyToLookFor, offset);
		byte[] value = Empty;

		if (idx.indexOf == -1)// Not found
			return (idx.indexOf, idx.nextOffset, idx.nextOffset, 0, Empty, "");

		// Read the string length (4 bytes) - multiply by 2 for 2-byte chars
		var lenSpan = playerFileContent.AsSpan(idx.nextOffset, sizeof(int));
		int len = BinaryPrimitives.ReadInt32LittleEndian(lenSpan);
		// Read the unicode string data (2 bytes per character)
		var stringSpan = playerFileContent.AsSpan(idx.nextOffset + sizeof(int), len * 2);
		value = stringSpan.ToArray();
		return (idx.indexOf, idx.nextOffset, idx.nextOffset + sizeof(int) + len * 2, len, value, EncodingUnicode.GetString(value));
	}

	public (int indexOf, int nextOffset) BinaryFindKey(byte[] dataSource, string key, int offset = 0)
	{
		// Add the length of the key to help filter out unwanted hits.
		byte[] keyWithLen = BitConverter.GetBytes(key.Length).Concat(Encoding1252.GetBytes(key)).ToArray();
		var result = BinaryFindKey(dataSource.AsSpan(), keyWithLen.AsSpan(), offset);
		// compensate the added length before returning the value
		return result.indexOf == -1 // not found
			? (result.indexOf, offset)
			: (result.indexOf + sizeof(int), result.nextOffset);
	}

	public (int indexOf, int nextOffset) BinaryFindKey(byte[] dataSource, byte[] key, int offset = 0)
	{
		return BinaryFindKey(dataSource.AsSpan(), key.AsSpan(), offset);
	}

	private static (int indexOf, int nextOffset) BinaryFindKey(ReadOnlySpan<byte> dataSource, ReadOnlySpan<byte> key, int offset = 0)
	{
		// Use SIMD-optimized SequenceEqual for comparison when first byte matches
		// adapted From https://www.codeproject.com/Questions/479424/C-23plusbinaryplusfilesplusfindingplusstrings
		int i = offset;
		var keyLength = key.Length;
		var firstByte = key[0];

		for (; i <= (dataSource.Length - keyLength); i++)
		{
			if (dataSource[i] == firstByte)
			{
				// Use Span.SequenceEqual which is SIMD-optimized in .NET
				if (dataSource.Slice(i, keyLength).SequenceEqual(key))
					return (i, i + keyLength);
			}
		}
		// Not found
		return (-1, 0);
	}

	/// <summary>
	/// Find the "end_block" of <paramref name="keyToLookFor"/>
	/// </summary>
	/// <param name="playerFileContent"></param>
	/// <param name="keyToLookFor"></param>
	/// <param name="offset"></param>
	/// <returns></returns>
	public (int indexOf, int valueOffset, int nextOffset, byte[] valueAsByteArray, int valueAsInt) BinaryFindEndBlockOf(byte[] playerFileContent, string keyToLookFor, int offset = 0)
	{
		int level = 0;
		var keybegin_block = "begin_block";
		var keyend_block = "end_block";
		var noMatch = (-1, 0, 0, new byte[] { }, 0);

		var startPoint = BinaryFindKey(playerFileContent, keyToLookFor, offset);
		if (startPoint.indexOf == -1)
			return noMatch;

		offset = startPoint.nextOffset;
		recurse:
		// Try to find next "end_block"
		var nextend_block = ReadIntAfter(playerFileContent, keyend_block, offset);
		// No more end_block left
		if (nextend_block.indexOf == -1)
			return noMatch;

		// Try to find next "begin_block"
		var nextbegin_block = ReadIntAfter(playerFileContent, keybegin_block, offset);
		// No more begin_block left
		if (nextbegin_block.indexOf == -1)
			return nextend_block; // found

		// next end_block is closer => found it
		if (nextend_block.indexOf < nextbegin_block.indexOf && level == 0)
			return nextend_block;
		else if (nextend_block.indexOf < nextbegin_block.indexOf && level > 0)
		{
			level--;
			offset = nextend_block.nextOffset;
			goto recurse;
		}
		else
		{
			level++;
			offset = nextbegin_block.nextOffset;
			goto recurse;
		}

	}

	public bool RemoveCStringValueAfter(ref byte[] playerFileContent, string keyToLookFor, int offset = 0)
	{
		var found = ReadCStringAfter(playerFileContent, keyToLookFor, offset);
		if (found.indexOf == -1) return false;

		// Remove CString value using Span-based slicing
		var keyBytes = Encoding1252.GetBytes(keyToLookFor);
		int startLen = found.indexOf - sizeof(int);
		int endLen = playerFileContent.Length - found.nextOffset;
		int totalLen = startLen + sizeof(int) + keyBytes.Length + sizeof(int) + endLen;
		var result = new byte[totalLen];
		var resultSpan = result.AsSpan();

		// start content
		playerFileContent.AsSpan(0, startLen).CopyTo(resultSpan);
		resultSpan = resultSpan.Slice(startLen);
		// KeyLen
		BitConverter.GetBytes(keyToLookFor.Length).AsSpan().CopyTo(resultSpan);
		resultSpan = resultSpan.Slice(sizeof(int));
		// Key
		keyBytes.AsSpan().CopyTo(resultSpan);
		resultSpan = resultSpan.Slice(keyBytes.Length);
		// ValueLen (set to 0 to remove)
		BitConverter.GetBytes(0).AsSpan().CopyTo(resultSpan);
		resultSpan = resultSpan.Slice(sizeof(int));
		// end content
		playerFileContent.AsSpan(found.nextOffset).CopyTo(resultSpan);

		playerFileContent = result;
		return true;
	}

	public bool ReplaceUnicodeValueAfter(ref byte[] playerFileContent, string keyToLookFor, string replacement, int offset = 0)
	{
		var found = ReadUnicodeStringAfter(playerFileContent, keyToLookFor, offset);
		if (found.indexOf == -1) return false;

		// Replace Unicode String value using Span-based slicing
		var keyBytes = Encoding1252.GetBytes(keyToLookFor);
		var valueBytes = EncodingUnicode.GetBytes(replacement);
		int startLen = found.indexOf - sizeof(int);
		int endLen = playerFileContent.Length - found.nextOffset;
		int totalLen = startLen + sizeof(int) + keyBytes.Length + sizeof(int) + valueBytes.Length + endLen;
		var result = new byte[totalLen];
		var resultSpan = result.AsSpan();

		// start content
		playerFileContent.AsSpan(0, startLen).CopyTo(resultSpan);
		resultSpan = resultSpan.Slice(startLen);
		// KeyLen
		BitConverter.GetBytes(keyToLookFor.Length).AsSpan().CopyTo(resultSpan);
		resultSpan = resultSpan.Slice(sizeof(int));
		// Key
		keyBytes.AsSpan().CopyTo(resultSpan);
		resultSpan = resultSpan.Slice(keyBytes.Length);
		// ValueLen
		BitConverter.GetBytes(replacement.Length).AsSpan().CopyTo(resultSpan);
		resultSpan = resultSpan.Slice(sizeof(int));
		// Value
		valueBytes.AsSpan().CopyTo(resultSpan);
		resultSpan = resultSpan.Slice(valueBytes.Length);
		// end content
		playerFileContent.AsSpan(found.nextOffset).CopyTo(resultSpan);

		playerFileContent = result;
		return true;
	}
}