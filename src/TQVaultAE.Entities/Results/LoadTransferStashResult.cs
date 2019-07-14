using System;
using System.Collections.Generic;
using System.Text;

namespace TQVaultAE.Entities.Results
{
	public class LoadTransferStashResult
	{
		public string TransferStashFile;
		public Stash Stash;
		public bool? StashPresent;
		public ArgumentException ArgumentException;
	}
}
