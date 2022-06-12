using System;
using System.Collections.Generic;
using System.Text;

namespace TQVaultAE.Domain.Entities
{
	public class HighlightFilterValues
	{
		public int MaxStr;
		public int MaxDex;
		public int MaxInt;
		public int MinStr;
		public int MinDex;
		public int MinInt;
		public bool MinRequierement;
		public bool MaxRequierement;
		public List<string> ClassItem;
		public int MaxLvl;
		public int MinLvl;
	}
}
