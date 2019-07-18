//-----------------------------------------------------------------------
// <copyright file="ArcFile.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Domain.Entities
{
	/// <summary>
	/// Holds data about a file stored in an ARC file.
	/// </summary>
	public class ArcPartEntry
	{
		/// <summary>
		/// Gets or sets the offset of this part entry within the file.
		/// </summary>
		public int FileOffset { get; set; }

		/// <summary>
		/// Gets or sets the compressed size of this part entry.
		/// </summary>
		public int CompressedSize { get; set; }

		/// <summary>
		/// Gets or sets the real size of this part entry.
		/// </summary>
		public int RealSize { get; set; }
	}


}