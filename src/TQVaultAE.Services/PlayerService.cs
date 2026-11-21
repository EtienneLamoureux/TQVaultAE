using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Session;
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
	, IItemTextFormatter itemText
	, IGameMapService gameMapService
	, ICharacterStatsService characterStatsService
	, ICharacterEditService characterEditService
	, ICharacterReadOnlyService characterReadOnlyService
	, ILoadoutService loadoutService
) : IPlayerService
{
	private readonly ILoadoutService LoadoutService = loadoutService;
	private readonly ICharacterReadOnlyService CharacterReadOnly = characterReadOnlyService;
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
	private readonly IItemTextFormatter ItemText = itemText;
	private readonly IGameMapService GameMapService = gameMapService;
	private readonly ICharacterStatsService CharacterStatsService = characterStatsService;
	private readonly ICharacterEditService CharacterEditService = characterEditService;

	/// <summary>
	/// Loads a player. Its stash is loaded apart (<see cref="IStashService.LoadPlayerStash"/>), see
	/// <see cref="LoadCharacterSession"/>.
	/// </summary>
	/// <param name="selectedSave">Player save information.</param>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Player load result with the player and error information.</returns>
	public PlayerLoadResult LoadPlayer(PlayerSave selectedSave, bool fromFileWatcher = false)
	{
		var result = new PlayerLoadResult();

		if (string.IsNullOrWhiteSpace(selectedSave?.Name))
			return result;

		try
		{
			#region Get the player

			// From the save's own folder: main campaign and custom map characters are listed together.
			var pf = this.GamePathResolver.PlayerFileIn(this.PathIO, selectedSave.Folder);

			var resultPC = new PlayerCollection(selectedSave.Name, pf);

			resultPC.IsImmortalThrone = selectedSave.IsImmortalThrone;

			result.PlayerFile = pf;

			PlayerCollection addFactory(string k)
			{
				try
				{
					// Read with the character's own map, whatever the active one is; its items keep that map.
					using (this.GameMapService.ReadWith(selectedSave))
						PlayerCollectionProvider.LoadFile(resultPC);

					string? characterMod = this.GameMapService.ModNameFor(selectedSave);
					foreach (var item in resultPC.SackItems().Concat(resultPC.EquipmentSack ?? []))
						item.SetContainerMod(characterMod);

					// Once the sacks exist: a read-only character marks them all.
					resultPC.IsReadOnly = this.CharacterReadOnly.IsReadOnly(selectedSave.Folder);
					selectedSave.IsReadOnly = resultPC.IsReadOnly;

					selectedSave.Info = resultPC.PlayerInfo;
					this.DescribeCharacter(selectedSave);
					this.GameMapService.MarkLoaded(selectedSave);

					// Add player and equipment items to the search database
					this.ItemDatabaseService.IndexPlayer(resultPC, k, selectedSave.Name);

					// Its loadouts, read with its map like its items.
					this.LoadoutService.Read(selectedSave.Folder, selectedSave.Name, characterMod);
				}
				catch (ArgumentException argumentException)
				{
					resultPC.ArgumentException = argumentException;
				}
				return resultPC;
			}

			result.Player = this.userContext.Players.LoadOrReload(result.PlayerFile, addFactory, fromFileWatcher, this.ItemDatabaseService);

			this.TagService.LoadTags(selectedSave);

			#endregion
		}
		catch (Exception ex)
		{
			Log.LogError(ex, "Failed to load player {PlayerName}", selectedSave?.Name);
			result.Error = ex;
		}

		return result;
	}

	/// <inheritdoc/>
	public CharacterLabels DescribeCharacter(PlayerSave save)
	{
		if (save.Info is not { } info)
			return CharacterLabels.Empty;

		// The character's own map, the one rule for any of its data: its linked mod when installed, else the base game.
		using (this.GameMapService.ReadWith(save))
		{
			var masteries = this.CharacterEditService.GetMasteries(info);
			var classTag = info.ClassTag;
			string className = classTag is { Length: > 0 } ? this.TranslationService.TranslateXTag(classTag, true, true) : string.Empty;
			save.ClassLabel = classTag is { Length: > 0 } ? (classTag, className) : null;
			return new CharacterLabels(className, CharacterLabels.JoinMasteryNames(masteries, this.TranslationService));
		}
	}

	/// <inheritdoc/>
	public CharacterSessionLoadResult LoadCharacterSession(PlayerSave selectedSave, bool loadTransferStash, bool loadRelicVaultStash)
	{
		// Read again a character read with another map's database, unless it or its stash has changes to save: the
		// stash is read again with it, and its unsaved changes would be dropped.
		string playerFile = this.GamePathResolver.PlayerFileIn(this.PathIO, selectedSave.Folder);
		string stashFile = this.GamePathResolver.StashFileIn(this.PathIO, selectedSave.Folder);
		bool unsaved = (this.userContext.Players.TryGetLoaded(playerFile, out var loaded) && loaded.IsModified)
			|| (this.userContext.Stashes.TryGetLoaded(stashFile, out var loadedStash) && loadedStash.IsModified);
		bool reload = this.GameMapService.LoadedWithOtherMap(selectedSave) && !unsaved;

		var player = this.LoadPlayer(selectedSave, reload);

		var playerStash = this.StashService.LoadCharacterStash(selectedSave, reload);
		if (playerStash is null)
			return new CharacterSessionLoadResult(player, null, null, null);

		// Relic vault first, as it always was
		var relicVaultStash = loadRelicVaultStash ? this.StashService.LoadRelicVaultStashFor(selectedSave) : null;
		var transferStash = loadTransferStash ? this.StashService.LoadTransferStashFor(selectedSave) : null;

		return new CharacterSessionLoadResult(player, playerStash, transferStash, relicVaultStash);
	}

	/// <inheritdoc/>
	public IReadOnlyList<(string File, PlayerCollection Player)> GetModifiedPlayers()
		=> [.. this.userContext.Players.LoadedEntries()
			.Select(kvp => (File: kvp.Key, Player: kvp.Value))
			.Where(p => p.Player is not null && p.Player.IsModified && !this.SkipReadOnly(p.Player.IsReadOnly, p.File))];

	/// <summary>
	/// A read-only character is never saved, even modified (nothing should modify it): its read-only attribute stays.
	/// </summary>
	private bool SkipReadOnly(bool isReadOnly, string file)
	{
		if (isReadOnly)
			this.Log.LogWarning("The read-only character file {File} has changes: it is not saved", file);

		return isReadOnly;
	}

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
				// And its read-only state, shown in the list.
				ps.IsReadOnly = this.CharacterReadOnly.IsReadOnly(ps.Folder);
				return ps;
			})
			.ToArray();
	}

	public void AlterNameInPlayerFileSave(string newname, string saveFolder)
	{
		// Alter name in Player file
		var newPlayerFile = this.GamePathResolver.PlayerFileIn(this.PathIO, saveFolder);
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

			// The copy of a read-only character starts writable: its name is written below, and it is a new character.
			this.CharacterReadOnly.ClearReadOnly(newFolder);
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
	public ArchiveChangeResult SetArchived(IEnumerable<PlayerSave> saves, bool archive, string? tag)
	{
		// Pick the saves first: moving one changes its IsArchived.
		var toMove = saves
			.Where(ps => ps.IsArchived != archive && (tag is null || ps.Tags.ContainsKey(tag)))
			.ToList();

		List<PlayerSave> changed = [];
		List<PlayerSave> failed = [];
		Exception? firstError = null;
		foreach (var ps in toMove)
		{
			try
			{
				this.MoveSave(ps, archive);
				changed.Add(ps);
			}
			catch (Exception ex)
			{
				this.Log.LogError(ex, "Failed to {Action} the character {Name}", archive ? "archive" : "unarchive", ps.Name);
				firstError ??= ex;
				failed.Add(ps);
			}
		}

		return new ArchiveChangeResult(changed, failed, firstError);
	}

	/// <summary>
	/// Archives or unarchives a save, then moves the loaded player from its old file path to the new one, so the player
	/// is found, saved and reloaded at its new place.
	/// </summary>
	/// <exception cref="IOException">The folder could not be moved: nothing changed.</exception>
	private void MoveSave(PlayerSave ps, bool archive)
	{
		var oldPath = this.GamePathResolver.PlayerFileIn(this.PathIO, ps.Folder);
		this.GameFileService.ToggleArchive(ps, archive);
		this.UpdatePlayerFilePath(oldPath, this.GamePathResolver.PlayerFileIn(this.PathIO, ps.Folder));
	}

	/// <inheritdoc/>
	public bool CurrentPlayerMeetsRequirements(Item item)
		=> this.MeetsRequirements(this.userContext.CurrentPlayer, item);

	/// <inheritdoc/>
	public bool MeetsRequirements(PlayerCollection? player, Item item, RequirementAttributes? attributes = null)
	{
		var currPlayer = player;
		if (currPlayer is null)
			return true;

		var reqs = this.ItemText.GetFriendlyNames(item, FriendlyNamesExtraScopes.Requirements).RequirementVariables;
		if (reqs is null || reqs.Count == 0)
			return true;

		return currPlayer.IsPlayerMeetRequierements(reqs, attributes, item.ItemClass);
	}

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, ReducedRequirement>? ReducedRequirements(PlayerCollection? player, Item item, RequirementAttributes? attributes = null)
	{
		if (player is null)
			return null;

		var reqs = this.ItemText.GetFriendlyNames(item, FriendlyNamesExtraScopes.Requirements).RequirementVariables;
		return reqs is null || reqs.Count == 0 ? null : player.ReducedRequirements(reqs, attributes, item.ItemClass);
	}

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, ReducedRequirement>? CurrentPlayerReducedRequirements(Item item)
		=> this.ReducedRequirements(this.userContext.CurrentPlayer, item);

	/// <summary>Passes before a loop of pieces detached and attached again (negative bonuses) stops where it is.</summary>
	private const int MaxEquipmentPasses = 8;

	/// <inheritdoc/>
	public EquipmentState ResolveEquipment(RequirementBuild build, IReadOnlyList<Item> worn, bool useAlternate, AttributeSum? buffs = null)
	{
		var counted = worn.Where(item => !item.IsInInactiveWeaponSlot(useAlternate)).ToList();
		var detached = new HashSet<Item>(ReferenceEqualityComparer.Instance);
		var requirements = new Dictionary<Item, SortedList<string, Variable>?>(ReferenceEqualityComparer.Instance);

		bool Meets(Item item, RequirementAttributes have)
		{
			if (!requirements.TryGetValue(item, out var reqs))
				requirements[item] = reqs = this.ItemText.GetFriendlyNames(item, FriendlyNamesExtraScopes.Requirements)?.RequirementVariables;

			return reqs is not { Count: > 0 } || EquipmentRequirements.Meets(reqs, item.ItemClass, have);
		}

		var state = this.SumEquipment(build, worn, detached, useAlternate, buffs);
		for (int pass = 0; pass < MaxEquipmentPasses; pass++)
		{
			// The attached pieces that fail, their own bonuses counted, are detached; when none, the detached pieces that
			// pass without them are attached again.
			var failing = counted.Where(item => !detached.Contains(item) && !Meets(item, state.Totals)).ToList();
			var passing = failing.Count > 0 ? [] : counted.Where(item => detached.Contains(item) && Meets(item, state.Totals)).ToList();
			if (failing.Count == 0 && passing.Count == 0)
				break;

			detached.UnionWith(failing);
			detached.ExceptWith(passing);
			state = this.SumEquipment(build, worn, detached, useAlternate, buffs);
		}

		return state;
	}

	/// <summary>The sums of <paramref name="build"/> wearing <paramref name="worn"/> without the <paramref name="detached"/> pieces.</summary>
	private EquipmentState SumEquipment(RequirementBuild build, IReadOnlyList<Item> worn, HashSet<Item> detached, bool useAlternate, AttributeSum? buffs)
	{
		var active = worn.Where(item => !detached.Contains(item)).ToList();
		var gear = this.CharacterStatsService.GetGearAttributes(active, useAlternate);
		var skills = this.CharacterStatsService.GetSkillAttributes(build.Skills, this.CharacterEditService.GetGearSkillAugments(active, useAlternate), build.Trees, includeBuffs: false);
		skills.Add(this.CharacterStatsService.GetItemSkillAttributes(active, useAlternate, includeBuffs: false));

		// The totals count the buffs running too; the skill bonuses kept for the save do not.
		var counted = skills;
		if (buffs is not null)
		{
			counted = new AttributeSum();
			counted.Add(skills);
			counted.Add(buffs);
		}

		var totals = new RequirementAttributes(build.Level,
			PlayerInfo.Total(build.Strength, "characterStrength", gear, counted),
			PlayerInfo.Total(build.Dexterity, "characterDexterity", gear, counted),
			PlayerInfo.Total(build.Intelligence, "characterIntelligence", gear, counted),
			gear, counted);
		return new EquipmentState(active, worn.Where(detached.Contains).ToList(), totals, gear, skills);
	}

	/// <inheritdoc/>
	public bool IsSuitableForCurrentPlayer(Item item)
		=> this.IsSuitableFor(this.userContext.CurrentPlayer, item);

	/// <inheritdoc/>
	public bool IsSuitableFor(PlayerCollection? player, Item item)
		=> player is null || ItemActionRules.IsSuitableFor(item, player.IsImmortalThrone);

	/// <inheritdoc/>
	public IReadOnlyList<Item> GetWornEquipment(IEnumerable<Item> equipment, Item? held)
		=> equipment.Where(item => !ReferenceEquals(item, held)).ToList();

	/// <inheritdoc/>
	public IReadOnlySet<RecordId>? WornSetPieces(PlayerCollection? player, Item item)
	{
		if (player is null || player.IsVault || player.EquipmentSack is not { } equipment
			|| this.ItemText.GetFriendlyNames(item, FriendlyNamesExtraScopes.ItemSet).ItemSet is not { setMembers.Count: > 0 } set)
			return null;

		var members = set.setMembers.Keys.Select(RecordId.Create).ToHashSet();
		return equipment
			.Where(piece => !piece.IsInInactiveWeaponSlot(player.UseAlternate) && members.Contains(piece.BaseItemId))
			.Select(piece => piece.BaseItemId)
			.ToHashSet();
	}

	/// <inheritdoc/>
	public IReadOnlySet<RecordId>? CurrentPlayerWornSetPieces(Item item)
		=> this.WornSetPieces(this.userContext.CurrentPlayer, item);

	/// <inheritdoc/>
	public void UpdateAttributeBonuses(PlayerCollection player, IEnumerable<Item> equipment, Item? ignored = null)
	{
		if (player.PlayerInfo is not { } info)
			return;

		var worn = this.GetWornEquipment(equipment, ignored);
		var state = this.ResolveEquipment(RequirementBuild.Of(info, this.CharacterEditService.GetMasteries(info)), worn, player.UseAlternate);
		info.GearBonus = state.Gear;
		info.SkillBonus = state.Skills;
		info.DetachedEquipment = state.Detached;
	}
}
