using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveFilesExplorer.Entities
{
	public class PlayerRecordKeyDescriptor
	{
		public TQFilePlayerRecordKey Enum { get; set; }
		public string Name { get; set; }
		public TQFileDataType DataType { get; set; }
		public TQVersion Version { get; set; }
	}
}
