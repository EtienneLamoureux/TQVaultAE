using System;
using System.Collections.Generic;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;
using TQVaultAE.Logs;
using TQVaultAE.Presentation;

namespace TQVaultAE.Services
{
	public class StashService : IStashService
	{
		private readonly log4net.ILog Log = null;
		private readonly SessionContext userContext = null;
		private readonly IStashProvider StashProvider;
		private readonly IGamePathService GamePathResolver;

		public StashService(ILogger<StashService> log, SessionContext userContext, IStashProvider stashProvider, IGamePathService gamePathResolver)
		{
			this.Log = log.Logger;
			this.userContext = userContext;
			this.StashProvider = stashProvider;
			this.GamePathResolver = gamePathResolver;
		}

		/// <summary>
		/// Loads a player stash using the drop down list.
		/// </summary>
		/// <param name="selectedSave">Item from the drop down list.</param>
		/// <returns></returns>
		public LoadPlayerStashResult LoadPlayerStash(PlayerSave selectedSave)
		{
			var result = new LoadPlayerStashResult();

			if (string.IsNullOrWhiteSpace(selectedSave?.Name)) return result;

			#region Get the player's stash

			result.StashFile = GamePathResolver.GetPlayerStashFile(selectedSave.Name);

			result.Stash = this.userContext.Stashes.GetOrAddAtomic(result.StashFile, k =>
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
			});

			#endregion

			return result;
		}

		/// <summary>
		/// Loads the transfer stash for immortal throne
		/// </summary>
		public LoadTransferStashResult LoadTransferStash()
		{
			var result = new LoadTransferStashResult();

			result.TransferStashFile = GamePathResolver.TransferStashFile;

			var resultStash = this.userContext.Stashes.GetOrAddAtomic(result.TransferStashFile, k =>
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
			});
			result.Stash = resultStash;
			result.Stash.IsImmortalThrone = true;

			return result;
		}


		/// <summary>
		/// Loads the relic vault stash
		/// </summary>
		public LoadRelicVaultStashResult LoadRelicVaultStash()
		{
			var result = new LoadRelicVaultStashResult();

			result.RelicVaultStashFile = GamePathResolver.RelicVaultStashFile;

			// Get the relic vault stash
			var resultStash = this.userContext.Stashes.GetOrAddAtomic(result.RelicVaultStashFile, k =>
			{
				var stash = new Stash(Resources.GlobalRelicVaultStash, k);
				stash.CreateEmptySack();
				stash.Sack.StashType = SackType.RelicVaultStash;

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
			});
			result.Stash = resultStash;
			result.StashFound = resultStash.StashFound;
			result.StashArgumentException = resultStash.ArgumentException;

			return result;
		}

		/// <summary>
		/// Attempts to save all modified stash files.
		/// </summary>
		/// <param name="stashOnError"></param>
		/// <exception cref="IOException">can happen during file save</exception>
		public void SaveAllModifiedStashes(ref Stash stashOnError)
		{
			// Save each stash as necessary
			foreach (KeyValuePair<string, Lazy<Stash>> kvp in this.userContext.Stashes)
			{
				string stashFile = kvp.Key;
				Stash stash = kvp.Value.Value;

				if (stash == null) continue;

				if (stash.IsModified)
				{
					stashOnError = stash;
					GamePathResolver.BackupFile(stash.PlayerName, stashFile);
					StashProvider.Save(stash, stashFile);
				}
			}

		}

	}
}
