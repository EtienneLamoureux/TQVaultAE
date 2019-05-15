using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveFilesExplorer.Entities
{
	[Flags]
	public enum TQVersion
	{
		TQ = 1 << 0,
		TQIT = 1 << 1,
		TQITAE = 1 << 2,
		TQITAE_Ragnarok = 1 << 3,
		TQITAE_Atlantis = 1 << 4,
		TQIT_All = TQ | TQIT,
		TQITAE_All = TQITAE | TQITAE_Ragnarok | TQITAE_Atlantis,
		All = TQIT_All | TQITAE_All,
	}
}
