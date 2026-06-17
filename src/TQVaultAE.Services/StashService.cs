using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;
using TQVaultAE.Config;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Services;

public class StashService : IStashService
{
	private readonly ILogger Log;
	private readonly SessionContext userContext;
	private readonly IItemDatabaseService ItemDatabaseService;
	private readonly IStashProvider StashProvider;
	private readonly IGamePathService GamePathResolver;
	private readonly IGameFileService GameFileService;
	private readonly UserSettings UserSettings;

	public StashService(
		ILogger<StashService> log,
		SessionContext userContext,
		IItemDatabaseService itemDatabaseService,
		IStashProvider stashProvider,
		IGamePathService gamePathResolver,
		IGameFileService iGameFileService,
		UserSettings userSettings)
	{
		Log = log;
		this.userContext = userContext;
		this.ItemDatabaseService = itemDatabaseService;
		StashProvider = stashProvider;
		GamePathResolver = gamePathResolver;
		GameFileService = iGameFileService;
		UserSettings = userSettings;
	}

	/// <summary>
	/// Loads a player stash.
	/// </summary>
	/// <param name="selectedSave">Player save information.</param>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Stash load result.</returns>
	public StashLoadResult LoadPlayerStash(PlayerSave selectedSave, bool fromFileWatcher = false)
	{
		var result = new StashLoadResult();

		if (string.IsNullOrWhiteSpace(selectedSave?.Name))
			return result;

		result.StashFile = GamePathResolver.GetPlayerStashFile(selectedSave.Name, selectedSave.IsArchived);

		Stash addStash(string k)
		{
			var stash = new Stash(selectedSave.Name, k);
			try
			{
				stash.StashFound = StashProvider.LoadFile(stash);
				if (stash.StashFound ?? false)
					stash.Sack.StashType = StashType.PlayerStash;

				// Add stash items to the search database
				if (stash.Sack != null)
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
				if (stash.StashFound ?? false)
					stash.Sack.StashType = StashType.TransferStash;

				// Add transfer stash items to the search database
				if (stash.Sack != null)
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
				if (stash.StashFound ?? false)
					stash.Sack.StashType = StashType.RelicVaultStash;

				// Add relic vault stash items to the search database
				if (stash.Sack != null)
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

	/// <summary>
	/// Attempts to save all modified stash files.
	/// </summary>
	/// <param name="stashOnError">If save fails, this will contain the stash that failed.</param>
	/// <returns>Number of stash saved.</returns>
	/// <exception cref="IOException">can happen during file save</exception>
	public int SaveAllModifiedStashes(ref Stash stashOnError)
	{
		// Save each stash as necessary
		int saved = 0;
		foreach (KeyValuePair<string, Lazy<Stash>> kvp in userContext.Stashes)
		{
			string stashFile = kvp.Key;
			Stash stash = kvp.Value.Value;

			if (stash == null) continue;

			if (stash.IsModified)
			{
				stashOnError = stash;

				if (!UserSettings.DisableLegacyBackup)
					GameFileService.BackupFile(stash.PlayerName, stashFile);

				StashProvider.Save(stash, stashFile);
				stash.Saved();
				saved++;
			}
		}
		return saved;
	}

	/// <summary>
	/// Creates a file watcher for the relic vault stash file.
	/// </summary>
	/// <param name="onChanged">Action to call when the file changes.</param>
	/// <returns>A FileSystemWatcher instance or null if the file path is invalid.</returns>
	public FileSystemWatcher CreateRelicStashFileWatcher(Action onChanged)
	{
		var stashFile = GamePathResolver.RelicVaultStashFileFullPath;

		if (string.IsNullOrEmpty(stashFile))
		{
			Log.LogWarning("Relic vault stash file path is not configured");
			return null;
		}

		var directory = Path.GetDirectoryName(stashFile);
		var fileName = Path.GetFileName(stashFile);

		if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
		{
			Log.LogWarning("Invalid relic vault stash file path: {Path}", stashFile);
			return null;
		}

		var watcher = new FileSystemWatcher(directory, fileName)
		{
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
			EnableRaisingEvents = true
		};

		watcher.Changed += (sender, e) =>
		{
			Log.LogInformation("Relic vault stash file changed: {FileName}", e.FullPath);
			onChanged?.Invoke();
		};

		return watcher;
	}

	/// <summary>
	/// Creates a file watcher for the transfer stash file.
	/// </summary>
	/// <param name="onChanged">Action to call when the file changes.</param>
	/// <returns>A FileSystemWatcher instance or null if the file path is invalid.</returns>
	public FileSystemWatcher CreateTransferStashFileWatcher(Action onChanged)
	{
		var stashFile = GamePathResolver.TransferStashFileFullPath;

		if (string.IsNullOrEmpty(stashFile))
		{
			Log.LogWarning("Transfer stash file path is not configured");
			return null;
		}

		var directory = Path.GetDirectoryName(stashFile);
		var fileName = Path.GetFileName(stashFile);

		if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
		{
			Log.LogWarning("Invalid transfer stash file path: {Path}", stashFile);
			return null;
		}

		var watcher = new FileSystemWatcher(directory, fileName)
		{
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
			EnableRaisingEvents = true
		};

		watcher.Changed += (sender, e) =>
		{
			Log.LogInformation("Transfer stash file changed: {FileName}", e.FullPath);
			onChanged?.Invoke();
		};

		return watcher;
	}
}
