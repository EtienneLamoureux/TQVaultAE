//-----------------------------------------------------------------------
// <copyright file="ArzFile.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Domain.Entities
{
	using System.Collections.Generic;

	/// <summary>
	/// Class for decoding Titan Quest ARZ files.
	/// </summary>
	public class ArzFile
	{
		/// <summary>
		/// Name of the ARZ file.
		/// </summary>
		public readonly string FileName;

		/// <summary>
		/// String table
		/// </summary>
		public string[] Strings;

		/// <summary>
		/// RecordInfo keyed by their ID
		/// </summary>
		public Dictionary<string, RecordInfo> RecordInfo;

		/// <summary>
		/// DBRecords cached as they get requested
		/// </summary>
		public Dictionary<string, DBRecordCollection> Cache;

		/// <summary>
		/// Holds the keys for the recordInfo Dictionary
		/// </summary>
		public string[] Keys;

		/// <summary>
		/// Initializes a new instance of the ArzFile class.
		/// </summary>
		/// <param name="fileName">name of the ARZ file.</param>
		public ArzFile(string fileName)
		{
			this.FileName = fileName;
			this.Cache = new Dictionary<string, DBRecordCollection>();
		}

		/// <summary>
		/// Gets the number of DBRecords
		/// </summary>
		public int Count => this.RecordInfo?.Count ?? 0;

		/// <summary>
		/// Retrieves a string from the string table.
		/// </summary>
		/// <param name="index">Offset in the string table.</param>
		/// <returns>string from the string table</returns>
		public string Getstring(int index) => this.Strings[index];

	}
}
