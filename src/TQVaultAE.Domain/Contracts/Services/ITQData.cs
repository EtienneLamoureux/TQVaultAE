using System.IO;

namespace TQVaultAE.Domain.Contracts.Services
{
	public interface ITQDataService
	{
		bool MatchNextString(string value, BinaryReader reader);
		/// <summary>
		/// Normalizes the record path to Upper Case Invariant Culture and replace backslashes with slashes.
		/// </summary>
		/// <param name="recordId">record path to be normalized</param>
		/// <returns>normalized record path</returns>
		string NormalizeRecordPath(string recordId);
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
	}
}