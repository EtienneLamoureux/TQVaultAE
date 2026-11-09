using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;
using TQVaultAE.Config;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Services;

public class StashService(
	ILogger<StashService> log,
	SessionContext userContext,
	IItemDatabaseService itemDatabaseService,
	IStashProvider stashProvider,
	IGamePathService gamePathResolver,
	IGameFileService iGameFileService,
	UserSettings userSettings) : IStashService
{
	private readonly ILogger Log = log;
	private readonly SessionContext userContext = userContext;
	private readonly IItemDatabaseService ItemDatabaseService = itemDatabaseService;
	private readonly IStashProvider StashProvider = stashProvider;
	private readonly IGamePathService GamePathResolver = gamePathResolver;
	private readonly IGameFileService GameFileService = iGameFileService;
	private readonly UserSettings UserSettings = userSettings;

	/// <summary>
	/// Loads a player stash.
	/// </summary>
	/// <param name="selectedSave">Player save information.</param>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Stash load result.</returns>
	public StashLoadResult LoadPlayerStash(PlayerSave? selectedSave, bool fromFileWatcher = false)
	{
		var result = new StashLoadResult();

		if (string.IsNullOrWhiteSpace(selectedSave?.Name))
			return result;

		// From the save's own folder: main campaign and custom map characters are listed together.
		result.StashFile = Path.Combine(selectedSave.Folder, GamePathResolver.PlayerStashFileNameB);

		Stash addStash(string k)
		{
			var stash = new Stash(selectedSave.Name, k);
			try
			{
				stash.StashFound = StashProvider.LoadFile(stash);
				if ((stash.StashFound ?? false) && stash.Sack is not null)
					stash.Sack.StashType = StashType.PlayerStash;

				// Add stash items to the search database
				if (stash.Sack is not null)
				{
					foreach (var item in stash.Sack)
						this.ItemDatabaseService.AddItemToDatabase(item, k, selectedSave.Name, BagIdConstants.BAGID_PLAYERSTASH, SackType.Stash, StashType.PlayerStash);
				}
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
			// The reloaded items replace those of the old file in the search.
			this.ItemDatabaseService.RemoveContainerFromDatabase(k);
			return addStash(k);
		}

		result.Stash = fromFileWatcher
			? userContext.Stashes.AddOrUpdateAtomic(result.StashFile, addStash, updateStash)
			: userContext.Stashes.GetOrAddAtomic(result.StashFile, addStash);

		return result;
	}

	/// <summary>
	/// Loads the transfer stash for immortal throne.
	/// </summary>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Stash load result.</returns>
	public StashLoadResult LoadTransferStash(bool fromFileWatcher = false)
	{
		var result = new StashLoadResult();

		result.StashFile = GamePathResolver.TransferStashFileFullPath;

		Stash addStash(string k)
		{
			var stash = new Stash(Resources.GlobalTransferStash, result.StashFile);
			try
			{
				stash.StashFound = StashProvider.LoadFile(stash);
				if ((stash.StashFound ?? false) && stash.Sack is not null)
					stash.Sack.StashType = StashType.TransferStash;

				// Add transfer stash items to the search database
				if (stash.Sack is not null)
				{
					foreach (var item in stash.Sack)
						this.ItemDatabaseService.AddItemToDatabase(item, k, Resources.GlobalTransferStash, BagIdConstants.BAGID_TRANSFERSTASH, SackType.Stash, StashType.TransferStash);
				}
			}
			catch (ArgumentException argumentException)
			{
				stash.ArgumentException = argumentException;
			}
			return stash;
		}

		Stash updateStash(string k, Stash oldValue)
		{
			// The reloaded items replace those of the old file in the search.
			this.ItemDatabaseService.RemoveContainerFromDatabase(k);
			return addStash(k);
		}

		var resultStash = fromFileWatcher
			? userContext.Stashes.AddOrUpdateAtomic(result.StashFile, addStash, updateStash)
			: userContext.Stashes.GetOrAddAtomic(result.StashFile, addStash);

		result.Stash = resultStash;
		result.Stash.IsImmortalThrone = true;

		return result;
	}

	/// <summary>
	/// A Titan Quest original character has no player, transfer or relic vault stash.
	/// </summary>
	private static bool HasStashes(PlayerSave? selectedSave)
		=> selectedSave is null || selectedSave.IsImmortalThrone;

	/// <inheritdoc/>
	public StashLoadResult? LoadCharacterStash(PlayerSave selectedSave, bool fromFileWatcher = false)
		=> HasStashes(selectedSave) ? this.LoadPlayerStash(selectedSave, fromFileWatcher) : null;

	/// <inheritdoc/>
	public StashLoadResult? LoadTransferStashFor(PlayerSave? selectedSave, bool fromFileWatcher = false)
		=> HasStashes(selectedSave) ? this.LoadTransferStash(fromFileWatcher) : null;

	/// <inheritdoc/>
	public StashLoadResult? LoadRelicVaultStashFor(PlayerSave? selectedSave, bool fromFileWatcher = false)
		=> HasStashes(selectedSave) ? this.LoadRelicVaultStash(fromFileWatcher) : null;

	/// <summary>
	/// Loads the relic vault stash.
	/// </summary>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Stash load result.</returns>
	public StashLoadResult LoadRelicVaultStash(bool fromFileWatcher = false)
	{
		var result = new StashLoadResult();

		result.StashFile = GamePathResolver.RelicVaultStashFileFullPath;

		Stash addStash(string k)
		{
			var stash = new Stash(Resources.GlobalRelicVaultStash, k);

			try
			{
				stash.StashFound = StashProvider.LoadFile(stash);
				if ((stash.StashFound ?? false) && stash.Sack is not null)
					stash.Sack.StashType = StashType.RelicVaultStash;

				// Add relic vault stash items to the search database
				if (stash.Sack is not null)
				{
					foreach (var item in stash.Sack)
						this.ItemDatabaseService.AddItemToDatabase(item, k, Resources.GlobalRelicVaultStash, BagIdConstants.BAGID_RELICVAULTSTASH, SackType.Stash, StashType.RelicVaultStash);
				}
			}
			catch (ArgumentException argumentException)
			{
				stash.ArgumentException = argumentException;
			}

			return stash;
		}

		Stash updateStash(string k, Stash oldValue)
		{
			// The reloaded items replace those of the old file in the search.
			this.ItemDatabaseService.RemoveContainerFromDatabase(k);
			return addStash(k);
		}

		// Get the relic vault stash
		var resultStash = fromFileWatcher
			? userContext.Stashes.AddOrUpdateAtomic(result.StashFile, addStash, updateStash)
			: userContext.Stashes.GetOrAddAtomic(result.StashFile, addStash);

		result.Stash = resultStash;
		result.Stash.IsImmortalThrone = true;

		return result;
	}

	/// <inheritdoc/>
	public IReadOnlyList<(string File, Stash Stash)> GetModifiedStashes()
		=> [.. this.userContext.Stashes
			.Select(kvp => (File: kvp.Key, Stash: kvp.Value.Value))
			.Where(s => s.Stash is not null && s.Stash.IsModified)];

	/// <inheritdoc/>
	public void SaveModifiedStash(string file, Stash stash)
	{
		if (!this.UserSettings.DisableLegacyBackup)
			this.GameFileService.BackupFile(stash.PlayerName, file);

		this.StashProvider.Save(stash, file);
		stash.Saved();
	}

	/// <inheritdoc />
	public int ClampBagId(int bagId)
		=> Math.Clamp(bagId, BagIdConstants.BAGID_EQUIPMENTPANEL, BagIdConstants.BAGID_RELICVAULTSTASH);

	/// <inheritdoc />
	public SackCollection? GetBagSack(int bagId, PlayerCollection? player, Stash? transferStash, Stash? playerStash, Stash? relicVaultStash)
	{
		static SackCollection? SackOf(Stash? stash)
			=> stash is { NumberOfSacks: > 0 } ? stash.Sack : null;

		return this.ClampBagId(bagId) switch
		{
			BagIdConstants.BAGID_EQUIPMENTPANEL => player?.EquipmentSack,
			BagIdConstants.BAGID_TRANSFERSTASH => SackOf(transferStash),
			BagIdConstants.BAGID_PLAYERSTASH => SackOf(playerStash),
			_ => SackOf(relicVaultStash),
		};
	}

	/// <summary>
	/// The player stash is centered as if it were at least this many cells high.
	/// </summary>
	private const int PlayerStashMinCenteringHeight = 15;

	/// <inheritdoc />
	public StashBag? GetBag(int bagId, PlayerCollection? player, Stash? transferStash, Stash? playerStash, Stash? relicVaultStash)
	{
		static StashBag? BagOf(int id, Stash? stash, int minCenteringHeight = 0)
			=> stash is null
				? null
				: new StashBag(id, stash.Sack, stash.Width, stash.Height, Math.Max(minCenteringHeight, stash.Height));

		int id = this.ClampBagId(bagId);
		return id switch
		{
			BagIdConstants.BAGID_EQUIPMENTPANEL => new StashBag(id, player?.EquipmentSack, 0, 0, 0),
			BagIdConstants.BAGID_TRANSFERSTASH => BagOf(id, transferStash),
			BagIdConstants.BAGID_PLAYERSTASH => BagOf(id, playerStash, PlayerStashMinCenteringHeight),
			_ => BagOf(id, relicVaultStash),
		};
	}
}
