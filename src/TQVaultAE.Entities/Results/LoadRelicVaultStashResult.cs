using System;
using System.Collections.Generic;
using System.Text;

namespace TQVaultAE.Entities.Results
{
	public class LoadRelicVaultStashResult
	{
		public string RelicVaultStashFile;
		public Stash Stash;
		public bool? StashPresent;
		public ArgumentException ArgumentException;
	}
}
