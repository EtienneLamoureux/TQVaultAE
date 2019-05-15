using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveFilesExplorer.Entities
{
	/// <summary>
	/// Save file Datatype
	/// </summary>
	public enum TQFileDataType
	{
		/// <summary>
		/// New key or not supposed to be here according to the file version.
		/// </summary>
		Unknown,
		/// <summary>
		/// sizeof(int) as Data
		/// </summary>
		Int,
		/// <summary>
		/// sizeof(int) as Len + (?<Data>.{Len})
		/// </summary>
		TQ_AnsiString,
		/// <summary>
		/// sizeof(int) as Len + (?<Data>\uXXXX{Len})
		/// </summary>
		TQ_UTF16String,
		/// <summary>
		/// sizeof(int) as Len + sizeof(byte[Len]) as Data
		/// </summary>
		TQ_SizedByteArray,
		/// <summary>
		/// sizeof(byte[16]) as Data
		/// </summary>
		ByteArrayFixedSize16,
	}
}
