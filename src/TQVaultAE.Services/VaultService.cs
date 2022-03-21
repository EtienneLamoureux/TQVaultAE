using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Services
{
	public class VaultService : IVaultService
	{
		private readonly ILogger Log = null;
		private readonly SessionContext userContext = null;
		private readonly IGamePathService GamePathResolver;
		private readonly IPlayerCollectionProvider PlayerCollectionProvider;
		public const string MAINVAULT = "Main Vault";

		public VaultService(ILogger<StashService> log, SessionContext userContext, IPlayerCollectionProvider playerCollectionProvider, IGamePathService gamePathResolver)
		{
			this.Log = log;
			this.userContext = userContext;
			this.GamePathResolver = gamePathResolver;
			this.PlayerCollectionProvider = playerCollectionProvider;
		}


		/// <summary>
		/// Creates a new empty vault file
		/// </summary>
		/// <param name="name">Name of the vault.</param>
		/// <param name="file">file name of the vault.</param>
		/// <returns>Player instance of the new vault.</returns>
		public PlayerCollection CreateVault(string name, string file)
		{
			// From .json to .vault
			var oldFormatfileName = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file));

			PlayerCollection vault = new PlayerCollection(name, file);
			vault.IsVault = true;

			// Convert by reading old format
			if (File.Exists(oldFormatfileName))
			{
				LoadVault(vault, oldFormatfileName);

				if (vault.sacks.Any())
					vault.sacks.First().IsModified = true;// Force Save into json on closing

				return vault;
			}

			// CreatNew
			vault.CreateEmptySacks(12); // number of bags
			return vault;
		}

		/// <summary>
		/// Attempts to save all modified vault files
		/// </summary>
		/// <param name="vaultOnError"></param>
		/// <exception cref="IOException">can happen during file save</exception>
		public void SaveAllModifiedVaults(ref PlayerCollection vaultOnError)
		{
			foreach (KeyValuePair<string, Lazy<PlayerCollection>> kvp in this.userContext.Vaults)
			{
				string vaultFile = kvp.Key;
				PlayerCollection vault = kvp.Value.Value;

				if (vault == null) continue;

				if (vault.IsModified)
				{
					// backup the file
					vaultOnError = vault;
					GamePathResolver.BackupFile(vault.PlayerName, vaultFile);
					PlayerCollectionProvider.Save(vault, vaultFile);
				}
			}
		}


		/// <summary>
		/// Loads a vault file
		/// </summary>
		/// <param name="vaultName">Name of the vault</param>
		public LoadVaultResult LoadVault(string vaultName)
		{
			var result = new LoadVaultResult();

			// Get the filename
			result.Filename = GamePathResolver.GetVaultFile(vaultName);

			// Check the cache
			var resultVault = this.userContext.Vaults.GetOrAddAtomic(result.Filename, k =>
			{
				PlayerCollection pc;
				// We need to load the vault.
				if (!File.Exists(k))
				{
					// the file does not exist so create a new vault or convert old format to Json.
					pc = this.CreateVault(vaultName, k);
					pc.VaultLoaded = true;
				}
				else
				{
					pc = new PlayerCollection(vaultName, k);
					pc.IsVault = true;
					LoadVault(pc);
				}
				return pc;
			});
			result.Vault = resultVault;
			result.VaultLoaded = resultVault.VaultLoaded;
			result.ArgumentException = resultVault.ArgumentException;

			return result;
		}

		/// <summary>
		/// Load vault
		/// </summary>
		/// <param name="pc"></param>
		/// <param name="filePathToUse">if not <code>null</code>, use this file instead of <see cref="PlayerCollection.PlayerFile"/> </param>
		private void LoadVault(PlayerCollection pc, string filePathToUse = null)
		{
			try
			{
				PlayerCollectionProvider.LoadFile(pc, filePathToUse);
				pc.VaultLoaded = true;
			}
			catch (ArgumentException argumentException)
			{
				pc.ArgumentException = argumentException;
			}
		}


		/// <summary>
		/// Updates VaultPath key from the configuration UI
		/// Needed since all vaults will need to be reloaded if this key changes.
		/// </summary>
		/// <param name="vaultPath">Path to the vault files</param>
		public void UpdateVaultPath(string vaultPath)
		{
			Config.Settings.Default.VaultPath = vaultPath;
			Config.Settings.Default.Save();
		}
	}
}
