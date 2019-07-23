﻿using System;
using System.Collections.Generic;
using System.IO;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;
using TQVaultAE.Logs;

namespace TQVaultAE.Services
{
	public class VaultService : IVaultService
	{
		private readonly log4net.ILog Log = null;
		private readonly SessionContext userContext = null;
		private readonly IGamePathService GamePathResolver;
		private readonly IPlayerCollectionProvider PlayerCollectionProvider;
		public const string MAINVAULT = "Main Vault";

		public VaultService(ILogger<StashService> log, SessionContext userContext, IPlayerCollectionProvider playerCollectionProvider, IGamePathService gamePathResolver)
		{
			this.Log = log.Logger;
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
			PlayerCollection vault = new PlayerCollection(name, file);
			vault.IsVault = true;
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
			foreach (KeyValuePair<string, PlayerCollection> kvp in this.userContext.Vaults)
			{
				string vaultFile = kvp.Key;
				PlayerCollection vault = kvp.Value;

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
			try
			{
				result.Vault = this.userContext.Vaults[result.Filename];
			}
			catch (KeyNotFoundException)
			{
				// We need to load the vault.
				if (!File.Exists(result.Filename))
				{
					// the file does not exist so create a new vault.
					result.Vault = this.CreateVault(vaultName, result.Filename);
					result.VaultLoaded = true;
				}
				else
				{
					result.Vault = new PlayerCollection(vaultName, result.Filename);
					result.Vault.IsVault = true;
					try
					{
						PlayerCollectionProvider.LoadFile(result.Vault);
						result.VaultLoaded = true;
					}
					catch (ArgumentException argumentException)
					{
						result.ArgumentException = argumentException;
					}
				}

				// Add the vault to the cache, but only if we create it or it successfully loads.
				if (result.VaultLoaded)
				{
					this.userContext.Vaults.Add(result.Filename, result.Vault);
				}
			}

			return result;
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
