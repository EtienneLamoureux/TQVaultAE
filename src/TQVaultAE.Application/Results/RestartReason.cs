namespace TQVaultAE.Application.Results;

/// <summary>
/// Why a settings change asks to restart the application, which picks the message shown.
/// </summary>
public enum RestartReason
{
	/// <summary>No change needs a restart.</summary>
	None,

	/// <summary>Several settings changed: the game path and the language together, or a UI setting.</summary>
	Settings,

	/// <summary>The game path changed.</summary>
	GamePath,

	/// <summary>The language changed.</summary>
	Language,
}

/// <summary>
/// Decides whether the changes made in the settings dialog need a restart.
/// </summary>
public static class RestartRule
{
	/// <summary>
	/// Gets the reason to restart for the changed settings.
	/// </summary>
	public static RestartReason Evaluate(bool languageChanged, bool gamePathChanged, bool uiSettingChanged)
	{
		if (!languageChanged && !gamePathChanged && !uiSettingChanged)
			return RestartReason.None;

		if ((gamePathChanged && languageChanged) || uiSettingChanged)
			return RestartReason.Settings;

		if (gamePathChanged)
			return RestartReason.GamePath;

		return RestartReason.Language;
	}
}
