//-----------------------------------------------------------------------
// <copyright file="ArcFile.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Domain.Entities
{
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Reads and decodes a Titan Quest ARC file.
	/// </summary>
	public class ArcFile
	{
		/// <summary>
		/// Signifies that the file has been read into memory.
		/// </summary>
		public bool FileHasBeenRead;

		/// <summary>
		/// Dictionary of the directory entries.
		/// </summary>
		public Dictionary<RecordId, ArcDirEntry> DirectoryEntries = new();

		/// <summary>
		/// Ordered keys for the directoryEntries dictionary.
		/// </summary>
		public IEnumerable<RecordId> Keys => this.DirectoryEntries.Keys.OrderBy(v => v);

		/// <summary>
		/// Initializes a new instance of the ArcFile class.
		/// </summary>
		/// <param name="fileName">File Name of the ARC file to be read.</param>
		public ArcFile(string fileName)
			=> this.FileName = fileName;

		/// <summary>
		/// Gets the ARC file name.
		/// </summary>
		public string FileName { get; private set; }

		/// <summary>
		/// Gets the number of Directory entries
		/// </summary>
		public int Count => this.DirectoryEntries.Count;

	}
}