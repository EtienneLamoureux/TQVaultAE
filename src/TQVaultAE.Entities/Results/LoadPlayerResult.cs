using System;
using System.Collections.Generic;
using System.Text;

namespace TQVaultAE.Entities.Results
{
	public class LoadPlayerResult
	{
		public bool IsCustom;
		public string PlayerFile;
		public PlayerCollection Player;
		public Stash Stash;
		public bool? StashFound;
		public ArgumentException PlayerArgumentException;
		public string StashFile;
		public ArgumentException StashArgumentException;
	}
}
