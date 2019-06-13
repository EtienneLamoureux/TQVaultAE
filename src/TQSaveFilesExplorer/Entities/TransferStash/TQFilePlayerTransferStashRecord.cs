using EnumsNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TQ.SaveFilesExplorer.Entities.TransferStash
{

	public class TQFilePlayerTransferStashRecord : TQFileRecord
	{
		#region Align versions and known keys for player records

		private static readonly PlayerTransferStashRecordKeyDescriptor[] playerEnumDescriptor = (
			from descriptor in (
				from e in Enums.GetMembers<TQFilePlayerTransferStashKey>()
					.SelectMany(
						k => k.Attributes.GetAll<TQFileDataTypeAttribute>()
						, (Key, Attr) => new { Key, Attr.Version, Attr.DataType })
				from v in new TQVersion[] { TQVersion.TQ, TQVersion.TQIT, TQVersion.TQAE }// Detailed versions
				where e.Version.HasFlag(v)
				select new PlayerTransferStashRecordKeyDescriptor
				{
					Enum = e.Key.Value,
					Name = e.Key.AsString(EnumFormat.Description, EnumFormat.Name),
					DataType = e.DataType,
					Version = v
				}
			)
			group descriptor by new { descriptor.DataType, descriptor.Enum, descriptor.Version } into grp
			select grp.First()
		).ToArray();

		#endregion

		public TQFilePlayerTransferStashKey KeyAsEnum { get; set; }

		public override void DefineDataType()
		{
			// Find the corresponding datatype according to the enum & version
			var found = playerEnumDescriptor.FirstOrDefault(d => d.Name == this.KeyName && d.Version == this.File.Version);
			if (found != null)
			{
				this.DataType = found.DataType;
				this.KeyAsEnum = found.Enum;
			}
		}

		/// <summary>
		/// Try to read value from the file
		/// </summary>
		public override void ReadValue()
		{
			DefineDataType();
			base.ReadValue();
		}

	}
}
