namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Stores the state of the session in the user settings.
/// </summary>
public interface ISettingsPersistenceService
{
	/// <summary>
	/// Records the last vault and character (when their auto load is on), the zoom and the custom map state,
	/// clears the language and game paths that are auto detected, then saves the settings if anything changed or they
	/// already changed in this session (<see cref="SessionContext.ConfigChanged"/>, which then stays set).
	/// </summary>
	/// <param name="vaultName">The vault shown in the main panel, or null.</param>
	/// <param name="characterName">The selected character, an empty string when none is selected, or null.</param>
	/// <param name="zoom">The current UI zoom.</param>
	/// <returns>Whether the settings were saved.</returns>
	bool SaveConfiguration(string? vaultName, string? characterName, float zoom);

	/// <summary>
	/// Gets the language of the installed game, or null when it could not be read.
	/// </summary>
	string? GameLanguage { get; }
}
