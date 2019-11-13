using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		private readonly ITranslationService TranslationService;
		public const string CustomDesignator = "<Custom Map>";

		public PlayerService(
			ILogger<PlayerService> log
			, SessionContext userContext
			, IPlayerCollectionProvider playerCollectionProvider
			, IStashProvider stashProvider
			, IGamePathService gamePathResolver
			, ITranslationService translationService
		)
		{
			this.Log = log.Logger;
			this.userContext = userContext;
			this.PlayerCollectionProvider = playerCollectionProvider;
			this.StashProvider = stashProvider;
			this.GamePathResolver = gamePathResolver;
			this.TranslationService = translationService;
		}


		/// <summary>
		/// Loads a player using the drop down list.
		/// Assumes designators are appended to character name.
		/// </summary>
		/// <param name="selectedSave">Player string from the drop down list.</param>
		/// <param name="isIT"></param>
		/// <returns></returns>
		public LoadPlayerResult LoadPlayer(PlayerSave selectedSave, bool isIT = false)
		{
			var result = new LoadPlayerResult();

			if (string.IsNullOrWhiteSpace(selectedSave?.Name)) return result;

			#region Get the player

			result.PlayerFile = GamePathResolver.GetPlayerFile(selectedSave.Name);

			try
			{
				result.Player = this.userContext.Players[result.PlayerFile];
				if (selectedSave.Info is null && result.Player.PlayerInfo != null) selectedSave.Info = result.Player.PlayerInfo;
			}
			catch (KeyNotFoundException)
			{
				result.Player = new PlayerCollection(selectedSave.Name, result.PlayerFile);
				result.Player.IsImmortalThrone = isIT;
				try
				{
					PlayerCollectionProvider.LoadFile(result.Player);
					this.userContext.Players.Add(result.PlayerFile, result.Player);
					selectedSave.Info = result.Player.PlayerInfo;
				}
				catch (ArgumentException argumentException)
				{
					result.PlayerArgumentException = argumentException;
				}
			}


			#endregion

			#region Get the player's stash

			result.StashFile = GamePathResolver.GetPlayerStashFile(selectedSave.Name);

			try
			{
				result.Stash = this.userContext.Stashes[result.StashFile];
			}
			catch (KeyNotFoundException)
			{
				result.Stash = new Stash(selectedSave.Name, result.StashFile);
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
					GamePathResolver.BackupFile(player.PlayerName, playerFile);
					GamePathResolver.BackupStupidPlayerBackupFolder(playerFile);
					PlayerCollectionProvider.Save(player, playerFile);
				}
			}

			return numModified > 0;
		}

		/// <summary>
		/// Gets a list of all of the character files in the save folder.
		/// </summary>
		/// <returns>List of character files descriptor</returns>
		public PlayerSave[] GetPlayerSaveList()
		{
			string[] folders = this.GamePathResolver.GetCharacterList();

			return (folders is null) ? null : folders
				.Select(f => new PlayerSave(f, this.GamePathResolver.IsCustom, this.GamePathResolver.MapName, this.TranslationService))
				.OrderBy(ps => ps.Name)
				.ToArray();
		}
	}
}
