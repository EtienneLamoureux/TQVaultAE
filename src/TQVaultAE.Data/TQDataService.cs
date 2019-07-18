//-----------------------------------------------------------------------
// <copyright file="TQData.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Data
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Text;
	using TQVaultAE.Domain.Contracts.Services;
	using TQVaultAE.Logs;

	/// <summary>
	/// TQData is used to store information about reading and writing the data files in TQ.
	/// </summary>
	public class TQDataService : ITQDataService
	{
		private readonly log4net.ILog Log;

		public TQDataService(ILogger<TQDataService> log)
		{
			this.Log = log.Logger;
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

	}
}