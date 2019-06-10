using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TQ.SaveFilesExplorer.Entities.TransferStash
{
	/// <summary>
	/// List of keys in player save file with data size as [DataType]
	/// Be carreful this is case sensitive.
	/// </summary>
	public enum TQFilePlayerTransferStashKey
	{
		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		CRC,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		stashVersion,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.String1252)]
		fName,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		sackWidth,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		sackHeight,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		numItems,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		begin_block,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		end_block,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		stackCount,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.String1252)]
		baseName,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.String1252)]
		prefixName,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.String1252)]
		suffixName,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.String1252)]
		relicName,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.String1252)] // TODO TBD
		relicBonus,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		seed,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		var1,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		xOffset,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		yOffset,

		[TQFileDataType(TQVersion.TQAE, TQFileDataType.String1252)]
		relicName2,

		[TQFileDataType(TQVersion.TQAE, TQFileDataType.String1252)]
		relicBonus2,

		[TQFileDataType(TQVersion.TQAE, TQFileDataType.Int)]
		var2,


	}
}
