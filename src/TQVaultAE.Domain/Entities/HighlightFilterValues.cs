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
		public List<Rarity> Rarity;
		public int MaxLvl;
		public int MinLvl;
		public List<GameDlc> Origin;
		public bool HavingPrefix;
		public bool HavingSuffix;
		public bool HavingRelic;
		public bool HavingCharm;
		public bool IsSetItem;
	}
}
