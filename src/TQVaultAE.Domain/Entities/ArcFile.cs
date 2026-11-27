//-----------------------------------------------------------------------
// <copyright file="ArcFile.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Domain.Entities;

using System.Collections.Generic;
using System.Linq;
using TQVaultAE.GameFiles;

/// <summary>
/// A Titan Quest ARC archive and its table of contents.
/// </summary>
public class ArcFile
{
	/// <summary>
	/// Signifies that the file has been read into memory.
	/// </summary>
	public bool FileHasBeenRead;

	/// <summary>
	/// Open reader for the archive, set by <c>IArcFileProvider.ReadARCToC</c>; entries are read from it on demand.
	/// </summary>
	public IArcArchive? Archive { get; set; }

	/// <summary>
	/// Entries of the archive's directory, keyed by their name.
	/// </summary>
	public Dictionary<RecordId, ArcEntry> DirectoryEntries = [];

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
