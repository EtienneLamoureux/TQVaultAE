using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Collections.Generic;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;
using TQVaultAE.Presentation;

namespace TQVaultAE.Services
{
	public class StashService : IStashService
	{
		private readonly ILogger Log = null;
		private readonly SessionContext userContext = null;
		private readonly IStashProvider StashProvider;
		private readonly IGamePathService GamePathResolver;
		private readonly IGameFileService GameFileService;

		public StashService(ILogger<StashService> log, SessionContext userContext, IStashProvider stashProvider, IGamePathService gamePathResolver, IGameFileService iGameFileService)
		{
			this.Log = log;
			this.userContext = userContext;
			this.StashProvider = stashProvider;
			this.GamePathResolver = gamePathResolver;
			this.GameFileService = iGameFileService;
		}

		/// <summary>
		/// Loads a player stash using the drop down list.
		/// </summary>
		/// <param name="selectedSave">Item from the drop down list.</param>
		/// <param name="fromFileWatcher">When <code>true</code> called from <see cref="FileSystemWatcher.Changed"/></param>
		/// <returns></returns>
		public LoadPlayerStashResult LoadPlayerStash(PlayerSave selectedSave, bool fromFileWatcher = false)
		{
			var result = new LoadPlayerStashResult();

			if (string.IsNullOrWhiteSpace(selectedSave?.Name)) return result;

			#region Get the player's stash

			result.StashFile = GamePathResolver.GetPlayerStashFile(selectedSave.Name, selectedSave.IsArchived);

			Stash addStash(string k)
			{
				var stash = new Stash(selectedSave.Name, k);
				try
				{
					stash.StashFound = StashProvider.LoadFile(stash);
				}
				catch (ArgumentException argumentException)
				{
					stash.ArgumentException = argumentException;
				}
				return stash;
			}

			Stash updateStash(string k, Stash oldValue)
			{
				// no logic with oldValue
				return addStash(k);
			}

			result.Stash = fromFileWatcher
				? this.userContext.Stashes.AddOrUpdateAtomic(result.StashFile, addStash, updateStash)
				: this.userContext.Stashes.GetOrAddAtomic(result.StashFile, addStash);

			#endregion

			return result;
		}

		/// <summary>
		/// Loads the transfer stash for immortal throne
		/// </summary>
		/// <param name="fromFileWatcher">When <code>true</code> called from <see cref="FileSystemWatcher.Changed"/></param>
		/// <returns></returns>
		public LoadTransferStashResult LoadTransferStash(bool fromFileWatcher = false)
		{
			var result = new LoadTransferStashResult();

			result.TransferStashFile = GamePathResolver.TransferStashFileFullPath;

			Stash addStash(string k)
			{
				var stash = new Stash(Resources.GlobalTransferStash, result.TransferStashFile);
				try
				{
					stash.StashFound = StashProvider.LoadFile(stash);
				}
				catch (ArgumentException argumentException)
				{
					stash.ArgumentException = argumentException;
				}
				return stash;
			}

			Stash updateStash(string k, Stash oldValue)
			{
				return addStash(k);
			}

			var resultStash = fromFileWatcher
				? this.userContext.Stashes.AddOrUpdateAtomic(result.TransferStashFile, addStash, updateStash)
				: this.userContext.Stashes.GetOrAddAtomic(result.TransferStashFile, addStash);

			result.Stash = resultStash;
			result.Stash.IsImmortalThrone = true;

			return result;
		}


		/// <summary>
		/// Loads the relic vault stash
		/// </summary>
		/// <param name="fromFileWatcher">When <code>true</code> called from <see cref="FileSystemWatcher.Changed"/></param>
		/// <returns></returns>
		public LoadRelicVaultStashResult LoadRelicVaultStash(bool fromFileWatcher = false)
		{
			var result = new LoadRelicVaultStashResult();

			result.RelicVaultStashFile = GamePathResolver.RelicVaultStashFileFullPath;

			Stash addStash(string k)
			{
				var stash = new Stash(Resources.GlobalRelicVaultStash, k);

				try
				{
					stash.StashFound = StashProvider.LoadFile(stash);
					if (stash.StashFound.Value)
						stash.Sack.StashType = SackType.RelicVaultStash;
				}
				catch (ArgumentException argumentException)
				{
					stash.ArgumentException = argumentException;
				}

				return stash;
			}

			Stash updateStash(string k, Stash oldValue)
			{
				return addStash(k);
			}

			// Get the relic vault stash
			var resultStash = fromFileWatcher
				? this.userContext.Stashes.AddOrUpdateAtomic(result.RelicVaultStashFile, addStash, updateStash)
				: this.userContext.Stashes.GetOrAddAtomic(result.RelicVaultStashFile, addStash);

			result.Stash = resultStash;
			result.Stash.IsImmortalThrone = true;

			return result;
		}

		/// <summary>
		/// Attempts to save all modified stash files.
		/// </summary>
		/// <param name="stashOnError"></param>
		/// <exception cref="IOException">can happen during file save</exception>
		/// <returns>Number of stash saved</returns>
		public int SaveAllModifiedStashes(ref Stash stashOnError)
		{
			// Save each stash as necessary
			int saved = 0;
			foreach (KeyValuePair<string, Lazy<Stash>> kvp in this.userContext.Stashes)
			{
				string stashFile = kvp.Key;
				Stash stash = kvp.Value.Value;

				if (stash == null) continue;

				if (stash.IsModified)
				{
					stashOnError = stash;
					
					if(!Config.UserSettings.Default.DisableLegacyBackup)
						GameFileService.BackupFile(stash.PlayerName, stashFile);

					StashProvider.Save(stash, stashFile);
					stash.Saved();
					saved++;
				}
			}
			return saved;
		}

	}
}
