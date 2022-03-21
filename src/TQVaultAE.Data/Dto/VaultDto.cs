using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TQVaultAE.Data.Dto
{
	public class VaultDto
	{
		public int currentlyFocusedSackNumber { get; set; }
		public int currentlySelectedSackNumber { get; set; }
		public List<SackDto> sacks { get; set; }
	}
}
