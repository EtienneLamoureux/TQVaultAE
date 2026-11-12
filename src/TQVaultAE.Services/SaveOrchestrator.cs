using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

/// <summary>
/// Runs the save of the session: modified characters, vaults and stashes, then the configuration, then the git backup.
/// </summary>
public class SaveOrchestrator(
	IPlayerService playerService
	, IVaultService vaultService
	, IStashService stashService
	, ISettingsPersistenceService settingsPersistenceService
	, IGameFileService gameFileService
	, UserSettings userSettings
) : ISaveOrchestrator
{
	private readonly IPlayerService _playerService = playerService;
	private readonly IVaultService _vaultService = vaultService;
	private readonly IStashService _stashService = stashService;
	private readonly ISettingsPersistenceService _settingsPersistenceService = settingsPersistenceService;
	private readonly IGameFileService _gameFileService = gameFileService;
	private readonly UserSettings _userSettings = userSettings;

	public SaveResult Save(SaveRequest request, Func<SaveError, SaveDecision> onError)
	{
		var errors = new List<SaveError>();
		bool playersSaved = false;
		int vaultsSaved = 0;
		int stashesSaved = 0;

		if (request.Files)
		{
			playersSaved = SaveFiles(SaveStep.Players, this._playerService.GetModifiedPlayers(), p => p.Player.PlayerName
				, p => this._playerService.SaveModifiedPlayer(p.File, p.Player), onError, errors) > 0;

			vaultsSaved = SaveFiles(SaveStep.Vaults, this._vaultService.GetModifiedVaults(), v => v.Vault.PlayerName
				, v => this._vaultService.SaveModifiedVault(v.File, v.Vault), onError, errors);

			stashesSaved = SaveFiles(SaveStep.Stashes, this._stashService.GetModifiedStashes(), s => s.Stash.PlayerName
				, s => this._stashService.SaveModifiedStash(s.File, s.Stash), onError, errors);
		}

		bool? configChanged = null;
		if (request.Configuration is { } configuration)
			configChanged = this._settingsPersistenceService.SaveConfiguration(configuration.VaultName, configuration.CharacterName, configuration.Zoom);

		// After the files, so the commit holds them.
		bool gitPushed = request.GitBackup
			&& this._userSettings.GitBackupEnabled
			&& this._gameFileService.GitAddCommitTagAndPush();

		return new SaveResult
		{
			PlayersSaved = playersSaved,
			VaultsSaved = vaultsSaved,
			StashesSaved = stashesSaved,
			Errors = errors,
			ConfigChanged = configChanged,
			GitPushed = gitPushed,
		};
	}

	/// <summary>
	/// Saves the files of a step one by one. Each file is written until it succeeds, is ignored or is aborted,
	/// so a file that fails does not keep the next ones from being saved.
	/// </summary>
	/// <param name="step">The step.</param>
	/// <param name="files">The modified files of the step.</param>
	/// <param name="name">The name of the character, vault or stash of a file.</param>
	/// <param name="save">Saves one file.</param>
	/// <param name="onError">Decides after a file error.</param>
	/// <param name="errors">Receives each file error.</param>
	/// <returns>How many files were saved.</returns>
	private static int SaveFiles<T>(SaveStep step, IEnumerable<T> files, Func<T, string?> name, Action<T> save, Func<SaveError, SaveDecision> onError, List<SaveError> errors)
	{
		int saved = 0;
		foreach (var file in files)
		{
			if (SaveFile(step, file, name, save, onError, errors))
				saved++;
		}

		return saved;
	}

	/// <summary>
	/// Writes one file, asking <paramref name="onError"/> after each file or access error.
	/// </summary>
	/// <returns>Whether the file was saved; false when it was ignored after an error.</returns>
	private static bool SaveFile<T>(SaveStep step, T file, Func<T, string?> name, Action<T> save, Func<SaveError, SaveDecision> onError, List<SaveError> errors)
	{
		while (true)
		{
			try
			{
				save(file);
				return true;
			}
			catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
			{
				var error = new SaveError(step, name(file), exception);
				errors.Add(error);
				switch (onError(error))
				{
					case SaveDecision.Abort:
						throw;
					case SaveDecision.Retry:
						continue;
					default:
						return false;
				}
			}
		}
	}
}
