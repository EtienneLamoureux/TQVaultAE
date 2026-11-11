using TQVaultAE.Application.Results;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Loads every character, character stash and vault at startup, for the search.
/// </summary>
public interface ILoadAllService
{
	/// <summary>
	/// Gets whether the last load of all the files did not complete, the application having stopped during it.
	/// </summary>
	bool LastRunFailed { get; }

	/// <summary>
	/// Turns the load of all the files off, after the user agreed to it following a failed run, and saves the settings.
	/// </summary>
	void DisableLoadAll();

	/// <summary>
	/// Loads the characters, their stashes and the vaults in parallel.
	/// The run is recorded in the settings, so a crash during it is found on the next start.
	/// </summary>
	/// <param name="characters">The characters to load, with their stashes.</param>
	/// <param name="vaults">The names of the vaults to load.</param>
	/// <param name="progress">Receives one step per character, per character stash, per vault and per shared stash.</param>
	/// <param name="sharedStashMaps">The custom maps (empty for the base game) whose transfer stash and relic vault to
	/// read; none by default.</param>
	LoadAllResult LoadAll(IReadOnlyCollection<PlayerSave> characters, IReadOnlyCollection<string> vaults, IProgress<int> progress
		, IReadOnlyCollection<string>? sharedStashMaps = null);
}
