using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TQ.SaveFilesExplorer.Entities
{
	/// <summary>
	/// Describe Datatype per version
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	public class TQFileDataTypeAttribute : Attribute
	{
		public TQVersion Version { get; }
		public TQFileDataType DataType { get; }

		public TQFileDataTypeAttribute(TQVersion version, TQFileDataType datatype)
		{
			this.Version = version;
			this.DataType = datatype;
		}
	}
}
