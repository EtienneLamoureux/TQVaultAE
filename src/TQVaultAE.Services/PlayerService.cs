using System;
using System.Collections.Generic;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;
using TQVaultAE.Logs;

namespace TQVaultAE.Services
{
	public class PlayerService : IPlayerService
	{
		private readonly log4net.ILog Log = null;
		private readonly SessionContext userContext = null;
		private readonly IPlayerCollectionProvider PlayerCollectionProvider;
		private readonly IStashProvider StashProvider;
		private readonly IGamePathService GamePathResolver;
		public const string CustomDesignator = "<Custom Map>";

		public PlayerService(ILogger<PlayerService> log, SessionContext userContext, IPlayerCollectionProvider playerCollectionProvider, IStashProvider stashProvider, IGamePathService gamePathResolver)
		{
			this.Log = log.Logger;
			this.userContext = userContext;
			this.PlayerCollectionProvider = playerCollectionProvider;
			this.StashProvider = stashProvider;
			this.GamePathResolver = gamePathResolver;
		}


		/// <summary>
		/// Loads a player using the drop down list.
		/// Assumes designators are appended to character name.
		/// </summary>
		/// <param name="selectedText">Player string from the drop down list.</param>
		/// <param name="isIT"></param>
		/// <returns></returns>
		public LoadPlayerResult LoadPlayer(string selectedText, bool isIT = false)
		{
			var result = new LoadPlayerResult();

			if (string.IsNullOrWhiteSpace(selectedText)) return result;

			result.IsCustom = selectedText.EndsWith(PlayerService.CustomDesignator, StringComparison.Ordinal);
			if (result.IsCustom)
			{
				// strip off the end from the player name.
				selectedText = selectedText.Remove(selectedText.IndexOf(PlayerService.CustomDesignator, StringComparison.Ordinal), PlayerService.CustomDesignator.Length);
			}

			#region Get the player

			result.PlayerFile = GamePathResolver.GetPlayerFile(selectedText);

			var resultPlayer = this.userContext.Players.GetOrAddAtomic(result.PlayerFile, k =>
			{
				var resultPC = new PlayerCollection(selectedText, k);
				resultPC.IsImmortalThrone = isIT;
				try
				{
					PlayerCollectionProvider.LoadFile(resultPC);
				}
				catch (ArgumentException argumentException)
				{
					resultPC.ArgumentException = argumentException;
				}
				return resultPC;
			});
			result.Player = resultPlayer;

			#endregion

			#region Get the player's stash

			result.StashFile = GamePathResolver.GetPlayerStashFile(selectedText);

			var resultStash = this.userContext.Stashes.GetOrAddAtomic(result.StashFile, k =>
			{
				var stash = new Stash(selectedText, k);
				try
				{
					stash.StashFound = StashProvider.LoadFile(stash);
				}
				catch (ArgumentException argumentException)
				{
					stash.StashArgumentException = argumentException;
				}
				return stash;
			});
			result.Stash = resultStash;
			result.StashFound = resultStash.StashFound;
			result.StashArgumentException = resultStash.StashArgumentException;

			#endregion

			return result;
		}


		/// <summary>
		/// Attempts to save all modified player files
		/// </summary>
		/// <param name="playerOnError"></param>
		/// <returns>True if there were any modified player files.</returns>
		/// <exception cref="IOException">can happen during file save</exception>
		public bool SaveAllModifiedPlayers(ref PlayerCollection playerOnError)
		{
			int numModified = 0;

			// Save each player as necessary
			foreach (KeyValuePair<string, Lazy<PlayerCollection>> kvp in this.userContext.Players)
			{
				string playerFile = kvp.Key;
				PlayerCollection player = kvp.Value.Value;

				if (player == null) continue;

				if (player.IsModified)
				{
					++numModified;
					playerOnError = player;// if needed by caller
					GamePathResolver.BackupFile(player.PlayerName, playerFile);
					GamePathResolver.BackupStupidPlayerBackupFolder(playerFile);
					PlayerCollectionProvider.Save(player, playerFile);
				}
			}

			return numModified > 0;
		}
	}
}
