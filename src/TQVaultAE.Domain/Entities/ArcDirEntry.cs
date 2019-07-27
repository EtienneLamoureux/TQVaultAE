//-----------------------------------------------------------------------
// <copyright file="ArcFile.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Domain.Entities
{
	/// <summary>
	/// Holds information about the directory entry.
	/// </summary>
	public class ArcDirEntry
	{
		/// <summary>
		/// Gets or sets the filename.
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// Gets or sets the storage type.
		/// Data is either compressed (3) or stored (1)
		/// </summary>
		public int StorageType { get; set; }

		/// <summary>
		/// Gets or sets the offset within the file.
		/// </summary>
		public int FileOffset { get; set; }

		/// <summary>
		/// Gets or sets the compressed size of this entry.
		/// </summary>
		public int CompressedSize { get; set; }

		/// <summary>
		/// Gets or sets the real size of this entry.
		/// </summary>
		public int RealSize { get; set; }

		/// <summary>
		/// Gets or sets the part data
		/// </summary>
		public ArcPartEntry[] Parts { get; set; }

		/// <summary>
		/// Gets a value indicating whether this part is active.
		/// </summary>
		public bool IsActive
			=> (this.StorageType == 1) ? true : this.Parts != null;
	}

}