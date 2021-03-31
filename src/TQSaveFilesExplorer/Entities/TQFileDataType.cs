using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TQ.SaveFilesExplorer.Entities
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
		/// sizeof(float) as Data
		/// </summary>
		Float,
		/// <summary>
		/// sizeof(int) as Data
		/// </summary>
		Int,
		/// <summary>
		/// sizeof(int) as Len + (?<Data>.{Len})
		/// </summary>
		String1252,
		/// <summary>
		/// sizeof(int) as Len + (?<Data>\uXXXX{Len})
		/// </summary>
		StringUTF16,
		/// <summary>
		/// sizeof(int) as Len + sizeof(byte[Len]) as Data
		/// </summary>
		ByteArrayVar,
		/// <summary>
		/// sizeof(byte[16]) as Data
		/// </summary>
		ByteArray16,
	}
}
