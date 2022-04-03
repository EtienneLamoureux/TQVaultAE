using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Data.Dto
{
	/// <summary>
	/// Vault tab content
	/// </summary>
	public class SackDto
	{
		/// <summary>
		/// Persisted icon info
		/// </summary>
		public BagButtonIconInfo iconinfo { get; set; }

		/// <summary>
		/// List of items
		/// </summary>
		public List<ItemDto> items { get; set; }
	}

}
