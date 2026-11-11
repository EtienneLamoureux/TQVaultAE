using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;

namespace TQVaultAE.Services;

/// <summary>
/// Stores the state of the session in the user settings.
/// </summary>
public class SettingsPersistenceService(UserSettings userSettings, IDatabase database) : ISettingsPersistenceService
{
	private readonly UserSettings _userSettings = userSettings;
	private readonly IDatabase _database = database;

	/// <inheritdoc/>
	public string? GameLanguage => this._database.GameLanguage;

	public bool SaveConfiguration(string? vaultName, string? characterName, float zoom, bool configChanged)
	{
		var changed = ApplySessionState(this._userSettings, vaultName, characterName, zoom) || configChanged;

		if (changed)
			this._userSettings.Save();

		return changed;
	}

	/// <summary>
	/// Writes the state of the session into the settings, without saving them.
	/// </summary>
	/// <returns>Whether a setting that needs a save changed.</returns>
	public static bool ApplySessionState(UserSettings settings, string? vaultName, string? characterName, float zoom)
	{
		var changed = false;

		// The Main Vault is not auto loaded, so only a different vault counts.
		if (settings.LoadLastVault
			&& vaultName is not null
			&& !vaultName.Equals(settings.LastVaultName, StringComparison.OrdinalIgnoreCase))
		{
			settings.LastVaultName = vaultName;
			changed = true;
		}

		if (settings.LoadLastCharacter
			&& characterName is not null
			&& !characterName.Equals(settings.LastCharacterName, StringComparison.OrdinalIgnoreCase))
		{
			settings.LastCharacterName = characterName;
			changed = true;
		}

		// Auto detected values are found again on each start.
		if (settings.AutoDetectLanguage)
			settings.TQLanguage = string.Empty;

		if (settings.AutoDetectGamePath)
		{
			settings.TQITPath = string.Empty;
			settings.TQPath = string.Empty;
		}

		// Compared with the saved zoom, not with 1: going back to 100% is a change to save too.
		if (zoom != settings.Scale)
		{
			settings.Scale = zoom;
			changed = true;
		}

		return changed;
	}
}
