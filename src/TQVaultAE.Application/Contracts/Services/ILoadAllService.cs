using TQVaultAE.Application.Results;
using TQVaultAE.Application.Session;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Loads every character, character stash and vault at startup, for the search.
/// </summary>
public interface ILoadAllService
{
	/// <summary>
	/// Number of progress steps of <see cref="LoadAll"/>: one per character, one per character stash, one per vault, and
	/// two per map whose shared stashes are read (transfer stash and relic vault).
	/// </summary>
	static int CountSteps(int characters, int vaults, int sharedStashMaps = 0) => (characters * 2) + vaults + (sharedStashMaps * 2);

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
