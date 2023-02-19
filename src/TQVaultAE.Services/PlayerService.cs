using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;
using TQVaultAE.Domain.Search;

namespace TQVaultAE.Services
{
	public class PlayerService : IPlayerService
	{
		private readonly ILogger Log = null;
		private readonly SessionContext userContext = null;
		private readonly IPlayerCollectionProvider PlayerCollectionProvider;
		private readonly IGameFileService GameFileService;
		private readonly IGamePathService GamePathResolver;
		private readonly ITranslationService TranslationService;
		private readonly ITQDataService TQDataService;
		private readonly ITagService TagService;

		public PlayerService(
			ILogger<PlayerService> log
			, SessionContext userContext
			, IPlayerCollectionProvider playerCollectionProvider
			, IStashProvider stashProvider
			, IGameFileService iGameFileService
			, IGamePathService gamePathResolver
			, ITranslationService translationService
			, ITQDataService tQDataService
			, ITagService tagService
		)
		{
			this.Log = log;
			this.userContext = userContext;
			this.PlayerCollectionProvider = playerCollectionProvider;
			this.GameFileService = iGameFileService;
			this.GamePathResolver = gamePathResolver;
			this.TranslationService = translationService;
			this.TQDataService = tQDataService;
			this.TagService = tagService;
		}


		/// <summary>
		/// Loads a player using the drop down list.
		/// </summary>
		/// <param name="selectedSave">Item from the drop down list.</param>
		/// <param name="fromFileWatcher">When <code>true</code> called from <see cref="FileSystemWatcher.Changed"/></param>
		/// <returns></returns>
		public LoadPlayerResult LoadPlayer(PlayerSave selectedSave, bool fromFileWatcher = false)
		{
			var result = new LoadPlayerResult();

			if (string.IsNullOrWhiteSpace(selectedSave?.Name)) return result;

			#region Get the player

			var pf = GamePathResolver.GetPlayerFile(selectedSave.Name, selectedSave.IsImmortalThrone, selectedSave.IsArchived);

			var resultPC = new PlayerCollection(selectedSave.Name, pf);

			resultPC.IsImmortalThrone = selectedSave.IsImmortalThrone;

			result.PlayerFile = pf;

			PlayerCollection addFactory(string k)
			{
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

			this.TagService.LoadTags(selectedSave);

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
					if (!Config.UserSettings.Default.DisableLegacyBackup)
					{
						GameFileService.BackupFile(player.PlayerName, playerFile);
						GameFileService.BackupStupidPlayerBackupFolder(playerFile);
					}
					PlayerCollectionProvider.Save(player, playerFile);
					player.Saved();
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

			return folders
				.Select(f =>
					new PlayerSave(f
						, f.ContainsIgnoreCase(GamePathResolver.SaveDirNameTQIT)  // Is TQIT
						, f.ContainsIgnoreCase(GamePathResolver.ArchiveDirName) // Is Archived
						, this.GamePathResolver.IsCustom
						, this.GamePathResolver.MapName
						, this.TranslationService
					)
				)
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
