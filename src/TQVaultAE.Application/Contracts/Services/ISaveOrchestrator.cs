using TQVaultAE.Application.Results;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Runs the save of the session: modified characters, vaults and stashes, then the configuration, then the git backup.
/// </summary>
public interface ISaveOrchestrator
{
	/// <summary>
	/// Saves what <paramref name="request"/> asks, in this order: characters, vaults, stashes, configuration,
	/// git commit and push (only when the git backup is enabled).
	/// Each modified file is saved on its own. When a character, vault or stash file cannot be written
	/// (<see cref="IOException"/> or <see cref="UnauthorizedAccessException"/>), <paramref name="onError"/> decides:
	/// <see cref="SaveDecision.Retry"/> writes that file again, <see cref="SaveDecision.Ignore"/> leaves it unsaved and
	/// goes on with the next files, <see cref="SaveDecision.Abort"/> rethrows the error, so nothing after it runs.
	/// </summary>
	/// <param name="request">What to save.</param>
	/// <param name="onError">Asked after each file error.</param>
	/// <exception cref="IOException">The user aborted after a file error.</exception>
	/// <exception cref="UnauthorizedAccessException">The user aborted after an access error.</exception>
	SaveResult Save(SaveRequest request, Func<SaveError, SaveDecision> onError);
}
