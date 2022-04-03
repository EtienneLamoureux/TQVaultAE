using System;
using System.Collections.Generic;
using System.Text;

namespace TQVaultAE.Domain.Entities
{
	[Flags]
	public enum BagButtonDisplayMode
	{
		Default = 0,
		CustomIcon = 1 << 0,
		Number = 1 << 1,
		Label = 1 << 2,
	}
}
