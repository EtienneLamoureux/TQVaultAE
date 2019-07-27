using System;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Results
{
	public class LoadRelicVaultStashResult
	{
		public string RelicVaultStashFile;
		public Stash Stash;
		public bool? StashPresent;
		public ArgumentException ArgumentException;
	}
}
