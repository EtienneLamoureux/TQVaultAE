namespace TQVaultAE.Application.Contracts.Services;

public interface ITQDataService
{
	bool MatchNextString(string value, BinaryReader reader);
	/// <summary>
	/// Reads a string from the binary stream.
	/// Expects an integer length value followed by the actual string of the stated length.
	/// </summary>
	/// <param name="reader">BinaryReader instance</param>
	/// <returns>string of data that was read</returns>
	string ReadCString(BinaryReader reader);
	/// <summary>
	/// Reads a string from the binary stream.
	/// Expects an integer length value followed by the actual string of the stated length.
	/// </summary>
	/// <param name="reader">BinaryReader instance</param>
	/// <returns>string of data that was read</returns>
	string ReadUTF16String(BinaryReader reader);
	/// <summary>
	/// Validates that the next string is a certain value and throws an exception if it is not.
	/// </summary>
	/// <param name="value">value to be validated</param>
	/// <param name="reader">BinaryReader instance</param>
	void ValidateNextString(string value, BinaryReader reader);
	/// <summary>
	/// Writes a string along with its length to the file.
	/// </summary>
	/// <param name="writer">BinaryWriter instance</param>
	/// <param name="value">string value to be written.</param>
	void WriteCString(BinaryWriter writer, string value);

	bool ReplaceUnicodeValueAfter(ref byte[] playerFileContent, string keyToLookFor, string replacement, int offset = 0);
	bool RemoveCStringValueAfter(ref byte[] playerFileContent, string keyToLookFor, int offset = 0);
	(int indexOf, int valueOffset, int nextOffset, byte[] valueAsByteArray, float valueAsFloat) ReadFloatAfter(byte[] playerFileContent, string keyToLookFor, int offset = 0);

	(int indexOf, int valueOffset, int nextOffset, byte[] valueAsByteArray, int valueAsInt) ReadIntAfter(byte[] playerFileContent, string keyToLookFor, int offset = 0);

	(int indexOf, int valueOffset, int nextOffset, int valueLen, byte[] valueAsByteArray, string valueAsString) ReadCStringAfter(byte[] playerFileContent, string keyToLookFor, int offset = 0);

	(int indexOf, int valueOffset, int nextOffset, int valueLen, byte[] valueAsByteArray, string valueAsString) ReadUnicodeStringAfter(byte[] playerFileContent, string keyToLookFor, int offset = 0);

	(int indexOf, int nextOffset) BinaryFindKey(byte[] dataSource, string key, int offset = 0);

	(int indexOf, int nextOffset) BinaryFindKey(byte[] dataSource, byte[] key, int offset = 0);

	(int indexOf, int valueOffset, int nextOffset, byte[] valueAsByteArray, int valueAsInt) WriteIntAfter(byte[] playerFileContent, string keyToLookFor, int newValue, int offset = 0);
	(int indexOf, int valueOffset, int nextOffset, byte[] valueAsByteArray, float valueAsFloat) WriteFloatAfter(byte[] playerFileContent, string keyToLookFor, float newValue, int offset = 0);

	/// <summary>
	/// Find the "end_block" of <paramref name="keyToLookFor"/>
	/// </summary>
	/// <param name="playerFileContent"></param>
	/// <param name="keyToLookFor"></param>
	/// <param name="offset"></param>
	/// <returns></returns>
	(int indexOf, int valueOffset, int nextOffset, byte[] valueAsByteArray, int valueAsInt) BinaryFindEndBlockOf(byte[] playerFileContent, string keyToLookFor, int offset = 0);

	/// <summary>
	/// Return value for "beginBlock" tag
	/// </summary>
	public int BeginBlockValue { get; }
	/// <summary>
	/// Return value for "endBlock" tag
	/// </summary>
	public int EndBlockValue { get; }

	/// <summary>
	/// Reads a length-prefixed string from a span using CP1252 encoding.
	/// Enables bounds-check elimination in high-frequency parsing paths.
	/// </summary>
	/// <param name="data">ReadOnlySpan of binary data</param>
	/// <param name="offset">Offset that will be advanced by the method</param>
	/// <returns>The decoded string</returns>
	string ReadCString(ReadOnlySpan<byte> data, ref int offset);

	/// <summary>
	/// Reads a length-prefixed UTF-16 string from a span.
	/// Enables bounds-check elimination in high-frequency parsing paths.
	/// </summary>
	/// <param name="data">ReadOnlySpan of binary data</param>
	/// <param name="offset">Offset that will be advanced by the method</param>
	/// <returns>The decoded string</returns>
	string ReadUTF16String(ReadOnlySpan<byte> data, ref int offset);

	/// <summary>
	/// Validates that the next string matches the expected value using span-based parsing.
	/// Does not throw - returns true if match, false otherwise.
	/// Advances offset by the string length on match.
	/// </summary>
	/// <param name="expectedValue">The expected string value</param>
	/// <param name="data">ReadOnlySpan of binary data</param>
	/// <param name="offset">Offset that will be advanced on match</param>
	/// <returns>True if the next string matches expectedValue</returns>
	bool ValidateNextString(string expectedValue, ReadOnlySpan<byte> data, ref int offset);
}