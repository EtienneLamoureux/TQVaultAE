using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TQVaultAE.Data.Dto
{
	/// <summary>
	/// Vault data structure
	/// </summary>
	public class VaultDto
	{
		/// <summary>
		/// Persisted disabled tooltip ids
		/// </summary>
		public List<int> disabledtooltip { get; set; }
		/// <summary>
		/// Holds the currently focused sack
		/// </summary>
		/// <remarks>used to preseve right vault selected tab (Type = Vault only)</remarks>
		public int currentlyFocusedSackNumber { get; set; } = -1;

		/// <summary>
		/// Holds the currently selected sack
		/// </summary>
		/// <remarks>used to preseve left vault selected tab (Type = Vault only)</remarks>
		public int currentlySelectedSackNumber { get; set; } = -1;

		/// <summary>
		/// List of vault tabs
		/// </summary>
		public List<SackDto> sacks { get; set; }
	}
}
