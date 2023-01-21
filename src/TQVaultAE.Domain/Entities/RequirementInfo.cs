using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace TQVaultAE.Domain.Entities
{
	/// <summary>
	/// Item requierement info
	/// </summary>
	public class RequirementInfo
	{
		public Item Item;
		public int? Lvl;
		public int? Str;
		public int? Dex;
		public int? Int;
	}
}
