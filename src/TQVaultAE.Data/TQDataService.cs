//-----------------------------------------------------------------------
// <copyright file="TQDataService.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.Application.Contracts.Services;
using Microsoft.Extensions.Logging;
using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using TQVaultAE.GameFiles;

namespace TQVaultAE.Data;

/// <summary>
/// TQData is used to store information about reading and writing the data files in TQ.
/// </summary>
public class TQDataService(ILogger<TQDataService> log) : ITQDataService
{
	public const int BEGIN_BLOCK_VALUE = -1340212530;
	public const int END_BLOCK_VALUE = -559038242;

	public int BeginBlockValue => BEGIN_BLOCK_VALUE;
	public int EndBlockValue => END_BLOCK_VALUE;

	private readonly ILogger Log = log;

	/// <summary>
	/// Gets the CP1252 (Windows-1252) encoding used for TQ file parsing.
	/// </summary>
	public static Encoding Encoding1252 => TextEncodings.Cp1252;

	/// <summary>
	/// Gets the Unicode UTF-16 encoding.
	/// </summary>
	public static Encoding EncodingUnicode => Encoding.Unicode;

	/// <summary>
	/// Validates that the next string is a certain value and throws an exception if it is not.
	/// </summary>
	/// <param name="value">value to be validated</param>
	/// <param name="reader">BinaryReader instance</param>
	public void ValidateNextString(string value, BinaryReader reader)
	{
		string label = ReadCString(reader);
		if (!string.Equals(label, value, StringComparison.OrdinalIgnoreCase))
		{
			long position = reader.BaseStream.Position - label.Length - 4;
			var ex = new ArgumentException(string.Format(
				CultureInfo.InvariantCulture,
				"Error reading file at position {2}.  Expecting '{0}'.  Got '{1}'",
				value,
				label,
				position));
			this.Log.LogError(ex, "Unexpected string at position {Position}: expected '{Expected}', got '{Actual}'", position, value, label);
			throw ex;
		}
	}

	public bool MatchNextString(string value, BinaryReader reader)
	{
		long readerPosition = reader.BaseStream.Position;

		string label = ReadCString(reader);
		reader.BaseStream.Position = readerPosition;

		if (!string.Equals(label, value, StringComparison.OrdinalIgnoreCase))
			return false;

		return true;
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

	public (int indexOf, int valueOffset, int nextOffset, int valueAsInt) WriteIntAfter(byte[] playerFileContent, string keyToLookFor, int newValue, int offset = 0)
	{
		var found = ReadIntAfter(playerFileContent, keyToLookFor, offset);
		if (found.indexOf != -1)
		{
			var newValueBytes = BitConverter.GetBytes(newValue);
			Array.ConstrainedCopy(newValueBytes, 0, playerFileContent, found.valueOffset, sizeof(int));
		}
		return found;
	}

	public (int indexOf, int valueOffset, int nextOffset, float valueAsFloat) WriteFloatAfter(byte[] playerFileContent, string keyToLookFor, float newValue, int offset = 0)
	{
		var found = ReadFloatAfter(playerFileContent, keyToLookFor, offset);
		if (found.indexOf != -1)
		{
			var newValueBytes = BitConverter.GetBytes(newValue);
			Array.ConstrainedCopy(newValueBytes, 0, playerFileContent, found.valueOffset, sizeof(float));
		}
		return found;
	}

	public (int indexOf, int valueOffset, int nextOffset, float valueAsFloat) ReadFloatAfter(byte[] playerFileContent, string keyToLookFor, int offset = 0)
	{
		var idx = BinaryFindKey(playerFileContent, keyToLookFor, offset);

		if (idx.indexOf == -1)// Not found
			return (idx.indexOf, idx.nextOffset, idx.nextOffset, 0);

		float value = BinaryPrimitives.ReadSingleLittleEndian(playerFileContent.AsSpan(idx.nextOffset, sizeof(float)));
		return (idx.indexOf, idx.nextOffset, idx.nextOffset + sizeof(float), value);
	}

	public (int indexOf, int valueOffset, int nextOffset, int valueAsInt) ReadIntAfter(byte[] playerFileContent, string keyToLookFor, int offset = 0)
	{
		var idx = BinaryFindKey(playerFileContent, keyToLookFor, offset);

		if (idx.indexOf == -1)// Not found
			return (idx.indexOf, idx.nextOffset, idx.nextOffset, 0);

		int intValue = BinaryPrimitives.ReadInt32LittleEndian(playerFileContent.AsSpan(idx.nextOffset, sizeof(int)));
		return (idx.indexOf, idx.nextOffset, idx.nextOffset + sizeof(int), intValue);
	}

	public (int indexOf, int valueOffset, int nextOffset, int valueLen, string valueAsString) ReadCStringAfter(byte[] playerFileContent, string keyToLookFor, int offset = 0)
	{
		var idx = BinaryFindKey(playerFileContent, keyToLookFor, offset);

		if (idx.indexOf == -1)// Not found
			return (idx.indexOf, idx.nextOffset, idx.nextOffset, 0, "");

		// Read the string length (4 bytes)
		int len = BinaryPrimitives.ReadInt32LittleEndian(playerFileContent.AsSpan(idx.nextOffset, sizeof(int)));
		// Read the string data
		var text = Encoding1252.GetString(playerFileContent.AsSpan(idx.nextOffset + sizeof(int), len));
		return (idx.indexOf, idx.nextOffset, idx.nextOffset + sizeof(int) + len, len, text);
	}

	public (int indexOf, int valueOffset, int nextOffset, int valueLen, string valueAsString) ReadUnicodeStringAfter(byte[] playerFileContent, string keyToLookFor, int offset = 0)
	{
		var idx = BinaryFindKey(playerFileContent, keyToLookFor, offset);

		if (idx.indexOf == -1)// Not found
			return (idx.indexOf, idx.nextOffset, idx.nextOffset, 0, "");

		// Read the string length (4 bytes) - multiply by 2 for 2-byte chars
		int len = BinaryPrimitives.ReadInt32LittleEndian(playerFileContent.AsSpan(idx.nextOffset, sizeof(int)));
		// Read the unicode string data (2 bytes per character)
		var text = EncodingUnicode.GetString(playerFileContent.AsSpan(idx.nextOffset + sizeof(int), len * 2));
		return (idx.indexOf, idx.nextOffset, idx.nextOffset + sizeof(int) + len * 2, len, text);
	}

	public (int indexOf, int nextOffset) BinaryFindKey(byte[] dataSource, string key, int offset = 0)
	{
		// Add the length of the key to help filter out unwanted hits.
		int byteCount = sizeof(int) + Encoding1252.GetByteCount(key);
		Span<byte> keyWithLen = byteCount <= 256 ? stackalloc byte[byteCount] : new byte[byteCount];
		BinaryPrimitives.WriteInt32LittleEndian(keyWithLen, key.Length);
		Encoding1252.GetBytes(key, keyWithLen[sizeof(int)..]);
		var result = BinaryFindKey(dataSource.AsSpan(), keyWithLen, offset);
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
		int found = offset < 0 || offset > dataSource.Length ? -1 : dataSource[offset..].IndexOf(key);
		return found == -1 ? (-1, 0) : (offset + found, offset + found + key.Length);
	}

	/// <summary>
	/// Find the "end_block" of <paramref name="keyToLookFor"/>
	/// </summary>
	/// <param name="playerFileContent"></param>
	/// <param name="keyToLookFor"></param>
	/// <param name="offset"></param>
	/// <returns></returns>
	public (int indexOf, int valueOffset, int nextOffset, int valueAsInt) BinaryFindEndBlockOf(byte[] playerFileContent, string keyToLookFor, int offset = 0)
	{
		int level = 0;
		var keybegin_block = "begin_block";
		var keyend_block = "end_block";
		var noMatch = (-1, 0, 0, 0);

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

		// The key, then an empty value (length 0)
		var keyAndValue = SaveBytes.Encode(writer =>
		{
			WriteKey(writer, keyToLookFor);
			writer.Write(0);
		}, 64);
		playerFileContent = SaveBytes.Splice(playerFileContent, found.indexOf - sizeof(int), found.nextOffset, keyAndValue);
		return true;
	}

	public bool ReplaceUnicodeValueAfter(ref byte[] playerFileContent, string keyToLookFor, string replacement, int offset = 0)
	{
		var found = ReadUnicodeStringAfter(playerFileContent, keyToLookFor, offset);
		if (found.indexOf == -1) return false;

		// The key, then the value: its length in characters and its UTF-16 bytes
		var keyAndValue = SaveBytes.Encode(writer =>
		{
			WriteKey(writer, keyToLookFor);
			writer.Write(replacement.Length);
			writer.Write(EncodingUnicode.GetBytes(replacement));
		}, 64);
		playerFileContent = SaveBytes.Splice(playerFileContent, found.indexOf - sizeof(int), found.nextOffset, keyAndValue);
		return true;
	}

	/// <summary>
	/// A key as the value splices write it: its length in characters, then its CP1252 bytes.
	/// </summary>
	private static void WriteKey(BinaryWriter writer, string key)
	{
		writer.Write(key.Length);
		writer.Write(Encoding1252.GetBytes(key));
	}
}