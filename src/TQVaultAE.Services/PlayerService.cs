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
	public class PlayerService : IPlayerService
	{
		private readonly ILogger Log = null;
		private readonly SessionContext userContext = null;
		private readonly IPlayerCollectionProvider PlayerCollectionProvider;
		private readonly IGamePathService GamePathResolver;
		private readonly ITranslationService TranslationService;
		private readonly ITQDataService TQDataService;

		public PlayerService(
			ILogger<PlayerService> log
			, SessionContext userContext
			, IPlayerCollectionProvider playerCollectionProvider
			, IStashProvider stashProvider
			, IGamePathService gamePathResolver
			, ITranslationService translationService
			, ITQDataService tQDataService
		)
		{
			this.Log = log;
			this.userContext = userContext;
			this.PlayerCollectionProvider = playerCollectionProvider;
			this.GamePathResolver = gamePathResolver;
			this.TranslationService = translationService;
			this.TQDataService = tQDataService;
		}


		/// <summary>
		/// Loads a player using the drop down list.
		/// </summary>
		/// <param name="selectedSave">Item from the drop down list.</param>
		/// <param name="isIT"></param>
		/// <param name="fromFileWatcher">When <code>true</code> called from <see cref="FileSystemWatcher.Changed"/></param>
		/// <returns></returns>
		public LoadPlayerResult LoadPlayer(PlayerSave selectedSave, bool isIT = false, bool fromFileWatcher = false)
		{
			var result = new LoadPlayerResult();

			if (string.IsNullOrWhiteSpace(selectedSave?.Name)) return result;

			#region Get the player

			result.PlayerFile = GamePathResolver.GetPlayerFile(selectedSave.Name);

			PlayerCollection addFactory(string k)
			{
				var resultPC = new PlayerCollection(selectedSave.Name, k);
				resultPC.IsImmortalThrone = isIT;
				try
				{
					PlayerCollectionProvider.LoadFile(resultPC);
					selectedSave.Info = resultPC.PlayerInfo;
				}
				catch (ArgumentException argumentException)
				{
					resultPC.ArgumentException = argumentException;
				}
				return resultPC;
			};

			PlayerCollection updateFactory(string k, PlayerCollection oldValue)
			{
				// No check on oldValue
				return addFactory(k);
			};

			var resultPlayer = fromFileWatcher 
				? this.userContext.Players.AddOrUpdateAtomic(result.PlayerFile, addFactory, updateFactory)
				: this.userContext.Players.GetOrAddAtomic(result.PlayerFile, addFactory);

			result.Player = resultPlayer;

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

		public void AlterNameInPlayerFileSave(string newname, string saveFolder)
		{
			// Alter name in Player file
			var newPlayerFile = Path.Combine(saveFolder, this.GamePathResolver.PlayerSaveFileName);
			var fileContent = File.ReadAllBytes(newPlayerFile);
			this.TQDataService.ReplaceUnicodeValueAfter(ref fileContent, "myPlayerName", newname);
			File.WriteAllBytes(newPlayerFile, fileContent);
		}
	}
}
