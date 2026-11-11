using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;

namespace TQVaultAE.Services;

/// <summary>
/// Loads every character, character stash and vault at startup, for the search.
/// </summary>
public class LoadAllService(
	ILogger<LoadAllService> log
	, UserSettings userSettings
	, IPlayerService playerService
	, IStashService stashService
	, IVaultService vaultService
) : ILoadAllService
{
	private readonly ILogger<LoadAllService> _log = log;
	private readonly UserSettings _userSettings = userSettings;
	private readonly IPlayerService _playerService = playerService;
	private readonly IStashService _stashService = stashService;
	private readonly IVaultService _vaultService = vaultService;

	/// <summary>
	/// Number of progress steps of <see cref="LoadAll"/>: one per character, one per character stash, one per vault, and
	/// two per map whose shared stashes are read (transfer stash and relic vault).
	/// </summary>
	public static int CountSteps(int characters, int vaults, int sharedStashMaps = 0) => (characters * 2) + vaults + (sharedStashMaps * 2);

	public bool LastRunFailed => !this._userSettings.LoadAllFilesCompleted;

	public void DisableLoadAll()
	{
		this._userSettings.LoadAllFilesCompleted = true;
		this._userSettings.LoadAllFiles = false;
		this.SaveSettings();
	}

	public LoadAllResult LoadAll(IReadOnlyCollection<PlayerSave> characters, IReadOnlyCollection<string> vaults, IProgress<int> progress
		, IReadOnlyCollection<string>? sharedStashMaps = null)
	{
		sharedStashMaps ??= [];
		if (CountSteps(characters.Count, vaults.Count, sharedStashMaps.Count) == 0)
			return LoadAllResult.Nothing;

		// Cleared while loading: still cleared on the next start means the application stopped during the load.
		this._userSettings.LoadAllFilesCompleted = false;
		this.SaveSettings();

		var stopWatch = Stopwatch.StartNew();

		var players = new ConcurrentBag<PlayerLoadResult>();
		var stashes = new ConcurrentBag<StashLoadResult>();
		var loadedVaults = new ConcurrentBag<VaultLoadResult>();

		var loadPlayers = characters.Select(c => (Action)(() =>
		{
			players.Add(this._playerService.LoadPlayer(c));
			progress.Report(1);
		}));

		var loadStashes = characters.Select(c => (Action)(() =>
		{
			stashes.Add(this._stashService.LoadPlayerStash(c));
			progress.Report(1);
		}));

		var loadVaults = vaults.Select(v => (Action)(() =>
		{
			loadedVaults.Add(this._vaultService.LoadVault(v));
			progress.Report(1);
		}));

		// The transfer stash and relic vault of every map in use, each read with its map.
		var loadSharedStashes = sharedStashMaps.SelectMany(map => new[]
		{
			(Action)(() =>
			{
				stashes.Add(this._stashService.LoadTransferStash(false, map));
				progress.Report(1);
			}),
			() =>
			{
				stashes.Add(this._stashService.LoadRelicVaultStash(false, map));
				progress.Report(1);
			},
		});

		Parallel.Invoke(loadPlayers.Concat(loadStashes).Concat(loadVaults).Concat(loadSharedStashes).ToArray());

		var errors = players.Select(p => (File: p.PlayerFile, Error: (Exception?)p.Player?.ArgumentException))
			.Concat(stashes.Select(s => (File: s.StashFile, Error: (Exception?)s.Stash?.ArgumentException)))
			.Concat(loadedVaults.Select(v => (File: (string?)v.Filename, Error: (Exception?)v.ArgumentException)))
			.Where(e => e.Error is not null)
			.Select(e => new LoadAllError(e.File, e.Error!))
			.ToList();

		stopWatch.Stop();
		var ts = stopWatch.Elapsed;
		this._log.LogInformation("LoadTime {Hours:00}:{Minutes:00}:{Seconds:00}.{Centiseconds:00}",
			ts.Hours, ts.Minutes, ts.Seconds,
			ts.Milliseconds / 10);

		// We made it so set the flag to indicate we were successful.
		this._userSettings.LoadAllFilesCompleted = true;
		this.SaveSettings();

		return new LoadAllResult { Ran = true, Errors = errors, Elapsed = ts };
	}

	/// <summary>
	/// Writes the settings to their file.
	/// </summary>
	protected virtual void SaveSettings() => this._userSettings.Save();
}
