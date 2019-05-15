using EnumsNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SaveFilesExplorer.Entities
{

	public class TQFilePlayerRecord : TQFileRecord
	{
		#region Align versions and keys

		private static readonly PlayerRecordKeyDescriptor[] playerEnumDescriptor = (
			from descriptor in (
				from e in Enums.GetMembers<TQFilePlayerRecordKey>()
					.SelectMany(
						k => k.Attributes.GetAll<TQFileDataTypeAttribute>()
						, (Key, Attr) => new { Key, Attr.Version, Attr.DataType })
				from v in new TQVersion[] { TQVersion.TQ, TQVersion.TQIT, TQVersion.TQITAE, TQVersion.TQITAE_Ragnarok, TQVersion.TQITAE_Atlantis }// Detailed versions
				where e.Version.HasFlag(v)
				select new PlayerRecordKeyDescriptor
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

		public TQFilePlayerRecordKey KeyAsEnum { get; }

		public TQFilePlayerRecord(Match m, TQVersion version) : base(m)
		{
			// Find the corresponding datatype according to the enum
			var found = playerEnumDescriptor.FirstOrDefault(d => d.Name == this.Key && d.Version == version);
			if (found != null)
			{
				this.DataType = found.DataType;
				this.KeyAsEnum = found.Enum;
			}
		}

		/// <summary>
		/// Try to read value from the file
		/// </summary>
		/// <param name="file"></param>
		public override void ReadValue(byte[] file)
		{
			var idx = this.RegExMatch.Index;
			var sizeOfKeyLen = sizeof(int); // It always is
			int len = 0;
			this.ValueStart = idx + sizeOfKeyLen + this.KeyLenAsInt;
			this.ValueEnd = 0;
			byte[] val;
			switch (this.DataType)
			{
				case TQFileDataType.Int:
					val = new ArraySegment<byte>(file, ValueStart, sizeof(int)).ToArray();
					DataAsInt = BitConverter.ToInt32(val, 0);
					ValueEnd = ValueStart + sizeof(int) - 1; // -1 because ValueStart is first relevant byte
					DataAsByteArray = val;
					break;
				case TQFileDataType.TQ_AnsiString:
					// Read StrLen
					len = BitConverter.ToInt32(new ArraySegment<byte>(file, ValueStart, sizeof(int)).ToArray(), 0);
					// Read Str
					val = new ArraySegment<byte>(file, ValueStart + sizeof(int), len).ToArray();
					this.DataAsStr = TQFileRecord.Encoding1252.GetString(val);
					ValueEnd = ValueStart + sizeof(int) - 1 + len;
					DataAsByteArray = new ArraySegment<byte>(file, ValueStart, ValueEnd - ValueStart).ToArray();
					break;
				case TQFileDataType.TQ_UTF16String:
					// Read StrLen
					len = BitConverter.ToInt32(new ArraySegment<byte>(file, ValueStart, sizeof(int)).ToArray(), 0);
					// Read Str
					val = new ArraySegment<byte>(file, ValueStart + sizeof(int), len * 2).ToArray();// * 2 because UTF16 has 2 byte encoding
					this.DataAsStr = ReadUTF16String(val);
					ValueEnd = ValueStart + sizeof(int) - 1 + (len * 2);
					DataAsByteArray = new ArraySegment<byte>(file, ValueStart, ValueEnd - ValueStart).ToArray();
					break;
				case TQFileDataType.TQ_SizedByteArray:
					// Read Len
					len = BitConverter.ToInt32(new ArraySegment<byte>(file, ValueStart, sizeof(int)).ToArray(), 0);
					// Read bytes
					ValueEnd = ValueStart + sizeof(int) - 1 + len;
					DataAsByteArray = new ArraySegment<byte>(file, ValueStart + sizeof(int), len).ToArray();
					break;
				case TQFileDataType.ByteArrayFixedSize16:
					// Read bytes
					ValueEnd = ValueStart + 16 - 1;
					DataAsByteArray = new ArraySegment<byte>(file, ValueStart, 16).ToArray();
					break;
			}
		}

		private static string ReadUTF16String(byte[] value)
		{
			List<char> chars = new List<char>();
			for (int i = 0; i < value.Length; i += 2)
			{
				var c = BitConverter.ToChar(new ArraySegment<byte>(value, i, 2).ToArray(), 0);
				chars.Add(c);
			}
			return new string(chars.ToArray());
		}
	}
}
