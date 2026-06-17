using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Config;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Services;

public class PlayerService : IPlayerService
{
	private readonly ILogger Log;
	private readonly SessionContext userContext;
	private readonly IItemDatabaseService ItemDatabaseService;
	private readonly IPlayerCollectionProvider PlayerCollectionProvider;
	private readonly IStashService StashService;
	private readonly IGameFileService GameFileService;
	private readonly IGamePathService GamePathResolver;
	private readonly ITranslationService TranslationService;
	private readonly ITQDataService TQDataService;
	private readonly ITagService TagService;
	private readonly IFileIO FileIO;
	private readonly IPathIO PathIO;
	private readonly UserSettings UserSettings;

	public PlayerService(
		ILogger<PlayerService> log
		, SessionContext userContext
		, IItemDatabaseService itemDatabaseService
		, IPlayerCollectionProvider playerCollectionProvider
		, IStashService stashService
		, IGameFileService iGameFileService
		, IGamePathService gamePathResolver
		, ITranslationService translationService
		, ITQDataService tQDataService
		, ITagService tagService
		, IFileIO fileIO
		, IPathIO pathIO
		, UserSettings userSettings
	)
	{
		this.Log = log;
		this.userContext = userContext;
		this.ItemDatabaseService = itemDatabaseService;
		this.PlayerCollectionProvider = playerCollectionProvider;
		this.StashService = stashService;
		this.GameFileService = iGameFileService;
		this.GamePathResolver = gamePathResolver;
		this.TranslationService = translationService;
		this.TQDataService = tQDataService;
		this.TagService = tagService;
		this.FileIO = fileIO;
		this.PathIO = pathIO;
		this.UserSettings = userSettings;
	}

	/// <summary>
	/// Loads a player and their stash.
	/// </summary>
	/// <param name="selectedSave">Player save information.</param>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Player load result with player, stash, and error information.</returns>
	public PlayerLoadResult LoadPlayer(PlayerSave selectedSave, bool fromFileWatcher = false)
	{
		var result = new PlayerLoadResult();

		if (string.IsNullOrWhiteSpace(selectedSave?.Name))
			return result;

		try
		{
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

					// Add player items to the search database
					int sackNumber = -1;
					foreach (var sack in resultPC)
					{
						sackNumber++;
						if (sack == null)
							continue;

						foreach (var item in sack)
							this.ItemDatabaseService.AddItemToDatabase(item, k, selectedSave.Name, sackNumber, SackType.Player);
					}

					// Add equipment items
					if (resultPC.EquipmentSack != null)
					{
						foreach (var item in resultPC.EquipmentSack)
							this.ItemDatabaseService.AddItemToDatabase(item, k, selectedSave.Name, 0, SackType.Equipment);
					}
				}
				catch (ArgumentException argumentException)
				{
					resultPC.ArgumentException = argumentException;
				}
				return resultPC;
			}

			PlayerCollection updateFactory(string k, PlayerCollection oldValue)
			{
				// No check on oldValue
				return addFactory(k);
			}

			var resultPlayer = fromFileWatcher
				? this.userContext.Players.AddOrUpdateAtomic(result.PlayerFile, addFactory, updateFactory)
				: this.userContext.Players.GetOrAddAtomic(result.PlayerFile, addFactory);

			result.Player = resultPlayer;

			this.TagService.LoadTags(selectedSave);

			#endregion

			// Load stash if not from file watcher
			if (!fromFileWatcher)
			{
				var stashResult = StashService.LoadPlayerStash(selectedSave, fromFileWatcher);
				result.PlayerStash = stashResult.Stash;
				result.PlayerStashFile = stashResult.StashFile;
			}
		}
		catch (Exception ex)
		{
			Log.LogError(ex, "Failed to load player {PlayerName}", selectedSave?.Name);
			result.Error = ex;
		}

		return result;
	}

	/// <summary>
	/// Loads a player stash.
	/// </summary>
	/// <param name="playerSave">Player save information.</param>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Stash load result.</returns>
	public StashLoadResult LoadPlayerStash(PlayerSave playerSave, bool fromFileWatcher = false)
		=> StashService.LoadPlayerStash(playerSave, fromFileWatcher);

	/// <summary>
	/// Attempts to save all modified player files.
	/// </summary>
	/// <param name="playerOnError">If save fails, this will contain the player that failed.</param>
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
				if (!this.UserSettings.DisableLegacyBackup)
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
					, this.PathIO
				)
			)
			.OrderBy(ps => ps.Name)
			.ToArray();
	}

	/// <summary>
	/// Gets a list of all of the character files in the save folder.
	/// </summary>
	/// <returns>Read-only list of character files descriptor</returns>
	public IReadOnlyList<PlayerSave> GetPlayerSaveListReadOnly()
		=> GetPlayerSaveList();

	public void AlterNameInPlayerFileSave(string newname, string saveFolder)
	{
		// Alter name in Player file
		var newPlayerFile = this.PathIO.Combine(saveFolder, this.GamePathResolver.PlayerSaveFileName);
		var fileContent = this.FileIO.ReadAllBytes(newPlayerFile);
		this.TQDataService.ReplaceUnicodeValueAfter(ref fileContent, "myPlayerName", newname);
		this.FileIO.WriteAllBytes(newPlayerFile, fileContent);
	}

	/// <summary>
	/// Creates a file watcher for the player file.
	/// </summary>
	/// <param name="playerSave">Player save information.</param>
	/// <param name="onChanged">Action to call when the file changes.</param>
	/// <returns>A FileSystemWatcher instance or null if the file path is invalid.</returns>
	public FileSystemWatcher CreatePlayerFileWatcher(PlayerSave playerSave, Action<PlayerSave, bool> onChanged)
	{
		var playerFile = GamePathResolver.GetPlayerFile(playerSave.Name, playerSave.IsImmortalThrone, playerSave.IsArchived);
		var directory = Path.GetDirectoryName(playerFile);
		var fileName = Path.GetFileName(playerFile);

		if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
			return null;

		var watcher = new FileSystemWatcher(directory, fileName)
		{
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
			EnableRaisingEvents = true
		};

		watcher.Changed += (sender, e) =>
		{
			Log.LogInformation("Player file changed: {FileName}", e.FullPath);
			onChanged?.Invoke(playerSave, true);
		};

		return watcher;
	}

	/// <summary>
	/// Updates the player file path in the player collection cache after archiving/unarchiving.
	/// </summary>
	/// <param name="oldPath">The old file path of the player.</param>
	/// <param name="newPath">The new file path of the player.</param>
	public void UpdatePlayerFilePath(string oldPath, string newPath)
	{
		if (string.IsNullOrWhiteSpace(oldPath) || string.IsNullOrWhiteSpace(newPath))
			return;

		if (this.userContext.Players.TryGetValue(oldPath, out var lazyPlayer))
		{
			var player = lazyPlayer.Value;
			if (player is not null)
			{
				// Remove from old key
				this.userContext.Players.TryRemove(oldPath, out _);
				// Update the player's file path
				player.PlayerFile = newPath;
				// Add with new key
				this.userContext.Players.GetOrAddAtomic(newPath, _ => player);
			}
		}
	}
}
