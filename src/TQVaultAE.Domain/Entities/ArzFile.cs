//-----------------------------------------------------------------------
// <copyright file="ArzFile.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Domain.Entities;

using System.Collections.Generic;
using System.Linq;
using TQVaultAE.GameFiles;

/// <summary>
/// A Titan Quest database (.arz) and its record table.
/// </summary>
public class ArzFile
{
	/// <summary>
	/// Name of the ARZ file.
	/// </summary>
	public readonly string FileName;

	/// <summary>
	/// Open reader for the file, set by <c>IArzFileProvider.Read</c>; records are decoded from it on demand.
	/// </summary>
	public IArzDatabase? Database { get; set; }

	/// <summary>
	/// Record table entries (class, place in the file) keyed by their ID: available without decompressing the records.
	/// </summary>
	public Dictionary<RecordId, ArzRecordInfo> RecordInfo = [];

	/// <summary>
	/// Ordered keys for the recordInfo Dictionary
	/// </summary>
	public IEnumerable<RecordId> Keys => this.RecordInfo.Keys.OrderBy(v => v);

	/// <summary>
	/// Initializes a new instance of the ArzFile class.
	/// </summary>
	/// <param name="fileName">name of the ARZ file.</param>
	public ArzFile(string fileName)
	{
		this.FileName = fileName;
	}

	/// <summary>
	/// Gets the number of DBRecords
	/// </summary>
	public int Count => this.RecordInfo?.Count ?? 0;
}
