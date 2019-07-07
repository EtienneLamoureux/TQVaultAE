using System;

namespace TQVaultAE.Entities.Results
{
	public class LoadVaultResult
	{
		public PlayerCollection Vault;
		public string Filename;
		public bool VaultLoaded;
		public ArgumentException ArgumentException;
	}
}
