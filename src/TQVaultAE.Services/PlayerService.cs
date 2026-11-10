using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Config;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Services;

public class PlayerService(
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
	, IDirectoryIO directoryIO
	, IItemProvider itemProvider
	, IDatabase database
	, IGameMapService gameMapService
	, IItemModService? itemModService = null
) : IPlayerService
{
	private readonly IItemModService? ItemModService = itemModService;
	private readonly ILogger Log = log;
	private readonly SessionContext userContext = userContext;
	private readonly IItemDatabaseService ItemDatabaseService = itemDatabaseService;
	private readonly IPlayerCollectionProvider PlayerCollectionProvider = playerCollectionProvider;
	private readonly IStashService StashService = stashService;
	private readonly IGameFileService GameFileService = iGameFileService;
	private readonly IGamePathService GamePathResolver = gamePathResolver;
	private readonly ITranslationService TranslationService = translationService;
	private readonly ITQDataService TQDataService = tQDataService;
	private readonly ITagService TagService = tagService;
	private readonly IFileIO FileIO = fileIO;
	private readonly IPathIO PathIO = pathIO;
	private readonly UserSettings UserSettings = userSettings;
	private readonly IDirectoryIO DirectoryIO = directoryIO;
	private readonly IItemProvider ItemProvider = itemProvider;
	private readonly IDatabase Database = database;
	private readonly IGameMapService GameMapService = gameMapService;

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

			// From the save's own folder: main campaign and custom map characters are listed together.
			var pf = this.PathIO.Combine(selectedSave.Folder, this.GamePathResolver.PlayerSaveFileName);

			var resultPC = new PlayerCollection(selectedSave.Name, pf);

			resultPC.IsImmortalThrone = selectedSave.IsImmortalThrone;

			result.PlayerFile = pf;

			PlayerCollection addFactory(string k)
			{
				try
				{
					PlayerCollectionProvider.LoadFile(resultPC);
					selectedSave.Info = resultPC.PlayerInfo;
					selectedSave.ClassLabel = resultPC.PlayerInfo?.Class is { } playerClass
						? (playerClass, this.TranslationService.TranslateXTag(playerClass, true, true))
						: null;
					this.GameMapService.MarkLoaded(selectedSave);

					// Add player items to the search database
					int sackNumber = -1;
					foreach (var sack in resultPC)
					{
						sackNumber++;
						if (sack is null)
							continue;

						foreach (var item in sack)
							this.ItemDatabaseService.AddItemToDatabase(item, k, selectedSave.Name, sackNumber, SackType.Player);
					}

					// Add equipment items
					if (resultPC.EquipmentSack is not null)
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
				// The reloaded items replace those of the old file in the search.
				this.ItemDatabaseService.RemoveContainerFromDatabase(k);
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

	/// <inheritdoc/>
	public CharacterSessionLoadResult LoadCharacterSession(PlayerSave selectedSave, bool loadTransferStash, bool loadRelicVaultStash)
	{
		// Read again a character read with another map's database, unless it has changes to save.
		string playerFile = this.PathIO.Combine(selectedSave.Folder, this.GamePathResolver.PlayerSaveFileName);
		bool reload = this.GameMapService.LoadedWithOtherMap(selectedSave)
			&& !(this.userContext.Players.TryGetValue(playerFile, out var loaded) && loaded.IsValueCreated && loaded.Value.IsModified);

		var player = this.LoadPlayer(selectedSave, reload);

		var playerStash = this.StashService.LoadCharacterStash(selectedSave, reload);
		if (playerStash is null)
			return new CharacterSessionLoadResult(player, null, null, null);

		// Relic vault first, as it always was
		var relicVaultStash = loadRelicVaultStash ? this.StashService.LoadRelicVaultStashFor(selectedSave) : null;
		var transferStash = loadTransferStash ? this.StashService.LoadTransferStashFor(selectedSave) : null;

		this.AdoptCharacterMod(selectedSave, player, playerStash, transferStash, relicVaultStash);

		return new CharacterSessionLoadResult(player, playerStash, transferStash, relicVaultStash);
	}

	/// <summary>
	/// Links the mod items of a character linked to an installed mod (records the base game does not have) to that mod,
	/// so that they keep its data when moved to a vault.
	/// </summary>
	private void AdoptCharacterMod(PlayerSave save, PlayerLoadResult player, params StashLoadResult?[] stashes)
	{
		if (this.ItemModService is null || this.GameMapService.ModOf(save) is not { State: CharacterModState.Linked, Mod: { } mod })
			return;

		var items = new List<Item>();
		if (player.Player is { } pc)
		{
			items.AddRange(pc.Where(s => s is not null).SelectMany(s => s));
			if (pc.EquipmentSack is not null)
				items.AddRange(pc.EquipmentSack);
		}

		foreach (var stash in stashes)
		{
			if (stash?.Stash?.Sack is { } sack)
				items.AddRange(sack);
		}

		int linked = this.ItemModService.AdoptMod(items, mod.Name);
		if (linked != 0)
			this.Log.LogInformation("{Count} items of {Player} linked to the mod {Mod}", linked, save.Name, mod.Name);
	}

	/// <inheritdoc/>
	public IReadOnlyList<(string File, PlayerCollection Player)> GetModifiedPlayers()
		=> [.. this.userContext.Players
			.Select(kvp => (File: kvp.Key, Player: kvp.Value.Value))
			.Where(p => p.Player is not null && p.Player.IsModified)];

	/// <inheritdoc/>
	public void SaveModifiedPlayer(string file, PlayerCollection player)
	{
		if (!this.UserSettings.DisableLegacyBackup)
		{
			this.GameFileService.BackupFile(player.PlayerName, file);
			this.GameFileService.BackupStupidPlayerBackupFolder(file);
		}

		this.PlayerCollectionProvider.Save(player, file);
		player.Saved();
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
					, this.GamePathResolver.IsCustomCharacterFolder(f)
					, string.Empty
					, this.TranslationService
					, this.PathIO
				)
			)
			.OrderBy(ps => ps.Name)
			.Select(ps =>
			{
				// Tags come with the list, so displaying a save does not read the tag configuration.
				this.TagService.LoadTags(ps);
				// The mod comes with the list too: its label and the map it is loaded with.
				this.GameMapService.Refresh(ps);
				return ps;
			})
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

	/// <inheritdoc />
	public CharacterNameError ValidateNewCharacterName(PlayerSave source, string newName, IEnumerable<string> existingNames)
	{
		if (string.IsNullOrWhiteSpace(newName))
			return CharacterNameError.Empty;

		var invalidChars = this.PathIO.GetInvalidPathChars().Concat(this.PathIO.GetInvalidFileNameChars()).ToArray();
		if (newName.IndexOfAny(invalidChars) != -1)
			return CharacterNameError.InvalidCharacters;

		if (existingNames.Any(name => newName.Equals(name, StringComparison.OrdinalIgnoreCase)))
			return CharacterNameError.AlreadyUsed;

		// A folder not listed as a character (another game, a leftover) must not be overwritten.
		if (this.DirectoryIO.Exists(this.GameFileService.GetDuplicateCharacterFolder(source.Folder, newName)))
			return CharacterNameError.AlreadyUsed;

		return CharacterNameError.None;
	}

	/// <inheritdoc />
	public CharacterDuplicateResult DuplicateCharacter(PlayerSave source, string newName)
	{
		var newFolder = this.GameFileService.GetDuplicateCharacterFolder(source.Folder, newName);
		if (this.DirectoryIO.Exists(newFolder))
			return CharacterDuplicateResult.Failed(new IOException($"The save folder {newFolder} exists already."));

		try
		{
			newFolder = this.GameFileService.DuplicateCharacterFiles(source.Folder, newName);
			this.AlterNameInPlayerFileSave(newName, newFolder);
			return CharacterDuplicateResult.Succeeded(newFolder);
		}
		catch (Exception ex)
		{
			this.Log.LogError(ex, "Failed to duplicate the character {Name} as {NewName}", source.Name, newName);

			// Rollback: the folder did not exist before, so all of it is the partial copy.
			try
			{
				if (this.DirectoryIO.Exists(newFolder))
					this.DirectoryIO.Delete(newFolder, true);
			}
			catch (Exception rollbackError)
			{
				this.Log.LogError(rollbackError, "Failed to remove the partial copy {Folder}", newFolder);
			}

			return CharacterDuplicateResult.Failed(ex);
		}
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

	/// <inheritdoc/>
	public bool Archive(PlayerSave ps) => this.MoveSave(ps, archive: true);

	/// <inheritdoc/>
	public bool Unarchive(PlayerSave ps) => this.MoveSave(ps, archive: false);

	/// <inheritdoc/>
	public ArchiveChangeResult SetArchived(IEnumerable<PlayerSave> saves, bool archive, string? tag)
	{
		// Pick the saves first: moving one changes its IsArchived.
		var toMove = saves
			.Where(ps => ps.IsArchived != archive && (tag is null || ps.Tags.ContainsKey(tag)))
			.ToList();

		List<PlayerSave> changed = [];
		List<PlayerSave> failed = [];
		foreach (var ps in toMove)
		{
			bool moved;
			try
			{
				moved = this.MoveSave(ps, archive);
			}
			catch (Exception ex)
			{
				this.Log.LogError(ex, "Failed to {Action} the character {Name}", archive ? "archive" : "unarchive", ps.Name);
				moved = false;
			}

			(moved ? changed : failed).Add(ps);
		}

		return new ArchiveChangeResult(changed, failed);
	}

	/// <summary>
	/// Archives or unarchives a save, then moves the loaded player from its old file path to the new one, so the player
	/// is found, saved and reloaded at its new place.
	/// </summary>
	private bool MoveSave(PlayerSave ps, bool archive)
	{
		var oldPath = this.PathIO.Combine(ps.Folder, this.GamePathResolver.PlayerSaveFileName);
		bool moved = archive ? this.GameFileService.Archive(ps) : this.GameFileService.Unarchive(ps);
		if (moved)
			this.UpdatePlayerFilePath(oldPath, this.PathIO.Combine(ps.Folder, this.GamePathResolver.PlayerSaveFileName));

		return moved;
	}

	/// <inheritdoc/>
	public bool CurrentPlayerMeetsRequirements(Item item)
	{
		var currPlayer = this.userContext.CurrentPlayer;
		if (currPlayer is null)
			return true;

		var reqs = this.ItemProvider.GetFriendlyNames(item, FriendlyNamesExtraScopes.Requirements).RequirementVariables;
		if (reqs is null || reqs.Count == 0)
			return true;

		return currPlayer.IsPlayerMeetRequierements(reqs);
	}

	/// <inheritdoc/>
	public bool IsSuitableForCurrentPlayer(Item item)
	{
		var currPlayer = this.userContext.CurrentPlayer;
		return currPlayer is null || ItemActionRules.IsSuitableFor(item, currPlayer.IsImmortalThrone);
	}

	/// <inheritdoc/>
	public void UpdateGearStatBonuses(PlayerCollection player, IEnumerable<Item> equipment, Item? ignored = null)
	{
		player.ClearPlayerGearBonuses();

		foreach (Item item in equipment)
		{
			// Ignore the item if it is being dragged
			if (item == ignored)
				continue;

			// Skip the weapons of the set not in hand.
			if (item.IsInInactiveWeaponSlot(player.UseAlternate))
				continue;

			player.UpdatePlayerGearBonuses(this.ItemProvider.GetStatBonuses(item));
		}
	}

	/// <inheritdoc/>
	public void UpdateSkillStatBonuses(PlayerCollection player)
	{
		if (player.PlayerInfo is null)
			return;

		var statBonuses = new SortedList<string, int>();

		foreach (SkillRecord skill in player.PlayerInfo.SkillRecordList)
			this.ItemProvider.GetStatBonusesFromRecord(statBonuses, this.Database.GetRecordFromFile(skill.skillName), skill.skillLevel);

		player.ClearPlayerSkillBonuses();
		player.UpdatePlayerSkillBonuses(statBonuses);
	}
}
