//-----------------------------------------------------------------------
// <copyright file="TQData.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Data
{
	using Microsoft.Extensions.Logging;
	using System;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;
	using TQVaultAE.Domain.Contracts.Services;
	using TQVaultAE.Logs;

	/// <summary>
	/// TQData is used to store information about reading and writing the data files in TQ.
	/// </summary>
	public class TQDataService : ITQDataService
	{
		private readonly ILogger Log;
		internal static readonly Encoding Encoding1252 = Encoding.GetEncoding(1252);
		internal static readonly Encoding EncodingUnicode = Encoding.Unicode;

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
		/// Writes a string along with its length to the file.
		/// </summary>
		/// <param name="writer">BinaryWriter instance</param>
		/// <param name="value">string value to be written.</param>
		public void WriteCString(BinaryWriter writer, string value)
		{
			// Convert the string to ascii
			// Vorbis' fix for extended characters in the database.
			Encoding ascii = Encoding.GetEncoding(1252);

			byte[] rawstring = ascii.GetBytes(value);

			// Write the 4-byte length of the string
			writer.Write(rawstring.Length);

			// now write the string
			writer.Write(rawstring);
		}

		/// <summary>
		/// Normalizes the record path to Upper Case Invariant Culture and replace backslashes with slashes.
		/// </summary>
		/// <param name="recordId">record path to be normalized</param>
		/// <returns>normalized record path</returns>
		public string NormalizeRecordPath(string recordId)
		{
			// uppercase it
			string normalizedRecordId = recordId.ToUpperInvariant();

			// replace any '/' with '\\'
			normalizedRecordId = normalizedRecordId.Replace('/', '\\');
			return normalizedRecordId;
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
			Encoding ascii = Encoding.GetEncoding(1252);

			byte[] rawData = reader.ReadBytes(len);

			char[] chars = new char[ascii.GetCharCount(rawData, 0, len)];
			ascii.GetChars(rawData, 0, len, chars, 0);

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
			return (UnicodeEncoding.Unicode.GetString(rawData));
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

		public (int indexOf, int valueOffset, int nextOffset, byte[] valueAsByteArray, float valueAsFloat) ReadFloatAfter(byte[] playerFileContent, string keyToLookFor, int offset = 0)
		{
			var idx = BinaryFindKey(playerFileContent, keyToLookFor, offset);
			var value = new ArraySegment<byte>(playerFileContent, idx.nextOffset, sizeof(float)).ToArray();
			return (idx.indexOf, idx.nextOffset, idx.nextOffset + sizeof(float), value, BitConverter.ToSingle(value, 0));
		}

		public (int indexOf, int valueOffset, int nextOffset, byte[] valueAsByteArray, int valueAsInt) ReadIntAfter(byte[] playerFileContent, string keyToLookFor, int offset = 0)
		{
			var idx = BinaryFindKey(playerFileContent, keyToLookFor, offset);
			var value = new ArraySegment<byte>(playerFileContent, idx.nextOffset, sizeof(int)).ToArray();
			return (idx.indexOf, idx.nextOffset, idx.nextOffset + sizeof(int), value, BitConverter.ToInt32(value, 0));
		}

		public (int indexOf, int valueOffset, int nextOffset, int valueLen, byte[] valueAsByteArray, string valueAsString) ReadCStringAfter(byte[] playerFileContent, string keyToLookFor, int offset = 0)
		{
			var idx = BinaryFindKey(playerFileContent, keyToLookFor, offset);
			var len = BitConverter.ToInt32(new ArraySegment<byte>(playerFileContent, idx.nextOffset, sizeof(int)).ToArray(), 0);
			var stringArray = new ArraySegment<byte>(playerFileContent, idx.nextOffset + sizeof(int), len).ToArray();
			return (idx.indexOf, idx.nextOffset, idx.nextOffset + sizeof(int) + len, len, stringArray, Encoding1252.GetString(stringArray));
		}

		public (int indexOf, int valueOffset, int nextOffset, int valueLen, byte[] valueAsByteArray, string valueAsString) ReadUnicodeStringAfter(byte[] playerFileContent, string keyToLookFor, int offset = 0)
		{
			var idx = BinaryFindKey(playerFileContent, keyToLookFor, offset);
			var len = BitConverter.ToInt32(new ArraySegment<byte>(playerFileContent, idx.nextOffset, sizeof(int)).ToArray(), 0);
			var stringArray = new ArraySegment<byte>(playerFileContent, idx.nextOffset + sizeof(int), len * 2).ToArray();
			return (idx.indexOf, idx.nextOffset, idx.nextOffset + sizeof(int) + len * 2, len, stringArray, EncodingUnicode.GetString(stringArray));
		}

		public (int indexOf, int nextOffset) BinaryFindKey(byte[] dataSource, string key, int offset = 0)
		{
			// Add the length of the key to help filter out unwanted hits.
			byte[] keyWithLen = BitConverter.GetBytes(key.Length).Concat(Encoding1252.GetBytes(key)).ToArray();
			var result = BinaryFindKey(dataSource, keyWithLen, offset);
			// compensate the added length before returning the value
			return (result.indexOf + sizeof(int), result.nextOffset);
		}

		public (int indexOf, int nextOffset) BinaryFindKey(byte[] dataSource, byte[] key, int offset = 0)
		{
			// adapted From https://www.codeproject.com/Questions/479424/C-23plusbinaryplusfilesplusfindingplusstrings
			int i = offset, j = 0;
			for (; i <= (dataSource.Length - key.Length); i++)
			{
				if (dataSource[i] == key[0])
				{
					j = 1;
					for (; j < key.Length && dataSource[i + j] == key[j]; j++) ;
					if (j == key.Length)
						goto found;

				}
			}
			i = -1;// Not found
		found:
			return (i, i + key.Length);
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

			// Remove CString value
			var keyBytes = Encoding1252.GetBytes(keyToLookFor);
			playerFileContent = new[] {
					playerFileContent.Take(found.indexOf - sizeof(int)).ToArray(),// start content
					BitConverter.GetBytes(keyToLookFor.Length),// KeyLen
					keyBytes,// Key
					BitConverter.GetBytes(0),// ValueLen
					playerFileContent.Skip(found.nextOffset).ToArray(),// end content
				}.SelectMany(a => a).ToArray();

			return true;
		}
	}
}
