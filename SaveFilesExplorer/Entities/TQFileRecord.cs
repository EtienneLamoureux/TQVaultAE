using EnumsNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SaveFilesExplorer.Entities
{
	public class TQFileRecord
	{
		/// <summary>
		/// Sub structure opening
		/// </summary>
		public const string begin_block = "begin_block";
		/// <summary>
		/// Sub structure closing
		/// </summary>
		public const string end_block = "end_block";
		/// <summary>
		/// Unhandled data
		/// </summary>
		public const string unknown_segment = "Unknown segment";

		internal static readonly Encoding Encoding1252 = Encoding.GetEncoding(1252);
		public Match RegExMatch { get; set; }

		public string KeyLen { get; set; }
		public int KeyLenAsInt { get; set; }

		public string KeyRaw { get; }
		public string Key { get; set; }

		public TQFileDataType DataType = TQFileDataType.Unknown;

		public int ValueStart { get; set; }
		public int ValueEnd { get; set; }

		public string DataAsStr { get; set; }
		public byte[] DataAsByteArray { get; set; }
		public int DataAsInt { get; set; }

		public bool IsSubStructureOpening { get => this.Key == begin_block; }
		public bool IsStructureClosing { get => this.Key == end_block; }
		public bool IsUnknownSegment { get => this.Key == unknown_segment; }

		public TQFileRecord()
		{ }

		public TQFileRecord(Match m)
		{
			this.RegExMatch = m;

			this.KeyLen = m.Groups["Len"].Value;
			this.Key = m.Groups["Key"].Value;
			this.KeyRaw = m.Groups["Key"].Value;

			var barray = Encoding1252.GetBytes(this.KeyLen.ToArray());
			this.KeyLenAsInt = BitConverter.ToInt32(barray, 0);

			if (this.KeyLenAsInt < this.Key.Length)
			{
				// Cut remaining chars
				this.Key = this.Key.Substring(0, this.KeyLenAsInt);
			}
		}

		public virtual void ReadValue(byte[] file)
		{
			throw new NotImplementedException();
		}
	}
}
