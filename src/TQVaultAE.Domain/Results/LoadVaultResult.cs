using System;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Results
{
	public class LoadVaultResult
	{
		public PlayerCollection Vault;
		public string Filename;
		public bool VaultLoaded;
		public ArgumentException ArgumentException;
	}
}
