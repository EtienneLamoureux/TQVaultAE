using System;
using System.Collections.Generic;
using TQVaultAE.Data;
using TQVaultAE.Entities;
using TQVaultAE.Entities.Results;
using TQVaultAE.Logs;

namespace TQVaultAE.Services
{
	public class PlayerService
	{
		private readonly log4net.ILog Log = null;
		private readonly SessionContext userContext = null;

		public const string CustomDesignator = "<Custom Map>";

		public PlayerService(SessionContext userContext)
		{
			Log = Logger.Get(this);
			this.userContext = userContext;
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

			result.PlayerFile = TQData.GetPlayerFile(selectedText);

			try
			{
				result.Player = this.userContext.Players[result.PlayerFile];
			}
			catch (KeyNotFoundException)
			{
				result.Player = new PlayerCollection(selectedText, result.PlayerFile);
				result.Player.IsImmortalThrone = isIT;
				try
				{
					PlayerCollectionProvider.LoadFile(result.Player);
					this.userContext.Players.Add(result.PlayerFile, result.Player);
				}
				catch (ArgumentException argumentException)
				{
					result.PlayerArgumentException = argumentException;
				}
			}

			#endregion

			#region Get the player's stash

			result.StashFile = TQData.GetPlayerStashFile(selectedText);

			try
			{
				result.Stash = this.userContext.Stashes[result.StashFile];
			}
			catch (KeyNotFoundException)
			{
				result.Stash = new Stash(selectedText, result.StashFile);
				try
				{
					result.StashFound = StashProvider.LoadFile(result.Stash);
					if (result.StashFound.Value)
						this.userContext.Stashes.Add(result.StashFile, result.Stash);
				}
				catch (ArgumentException argumentException)
				{
					result.StashArgumentException = argumentException;
				}
			}

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
			foreach (KeyValuePair<string, PlayerCollection> kvp in this.userContext.Players)
			{
				string playerFile = kvp.Key;
				PlayerCollection player = kvp.Value;

				if (player == null) continue;

				if (player.IsModified)
				{
					++numModified;
					playerOnError = player;// if needed by caller
					TQData.BackupFile(player.PlayerName, playerFile);
					TQData.BackupStupidPlayerBackupFolder(playerFile);
					PlayerCollectionProvider.Save(player, playerFile);
				}
			}

			return numModified > 0;
		}
	}
}
