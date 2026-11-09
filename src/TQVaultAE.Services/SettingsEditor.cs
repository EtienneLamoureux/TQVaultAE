using TQVaultAE.Application.Results;
using TQVaultAE.Config;

namespace TQVaultAE.Services;

/// <summary>
/// Edits a copy of the settings for the settings dialog: holds the rules between settings and tells what changed by
/// comparing the copy to the values it started from.
/// </summary>
public sealed class SettingsEditor
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SettingsEditor"/> class.
	/// </summary>
	/// <param name="original">The values when the dialog opened, left untouched.</param>
	public SettingsEditor(SettingsValues original)
	{
		this.Original = original;
		this.Current = original with { };
	}

	/// <summary>
	/// The values when the dialog opened.
	/// </summary>
	public SettingsValues Original { get; }

	/// <summary>
	/// The values being edited. The settings without a rule are set on it directly.
	/// </summary>
	public SettingsValues Current { get; }

	/// <summary>
	/// The values to save: the epic and legendary affixes need item editing.
	/// </summary>
	public SettingsValues Effective
		=> this.Current with { EnableEpicLegendaryAffixes = this.Current.AllowItemEdit && this.Current.EnableEpicLegendaryAffixes };

	#region Rules

	/// <summary>
	/// Turning off the cheats also turns off character editing, item editing and item copying.
	/// </summary>
	public void SetAllowCheats(bool value)
	{
		this.Current.AllowCheats = value;

		if (value)
			return;

		this.Current.AllowCharacterEdit = false;
		this.Current.AllowItemCopy = false;
		this.SetAllowItemEdit(false);
	}

	/// <summary>
	/// Turning off item editing also turns off the epic and legendary affixes.
	/// </summary>
	public void SetAllowItemEdit(bool value)
	{
		this.Current.AllowItemEdit = value;

		if (!value)
			this.Current.EnableEpicLegendaryAffixes = false;
	}

	/// <summary>
	/// Turning off the git backup brings back the legacy backup.
	/// </summary>
	public void SetGitBackupEnabled(bool value)
	{
		this.Current.GitBackupEnabled = value;

		if (!value)
			this.Current.DisableLegacyBackup = false;
	}

	/// <summary>
	/// Sets the vault path, trimmed; a change of case only is ignored.
	/// </summary>
	/// <returns>true when the path changed.</returns>
	public bool SetVaultPath(string? path)
		=> SetPath(path, this.Current.VaultPath, p => this.Current.VaultPath = p);

	/// <summary>
	/// Sets the Titan Quest path, trimmed; a change of case only is ignored.
	/// </summary>
	/// <returns>true when the path changed.</returns>
	public bool SetTitanQuestPath(string? path)
		=> SetPath(path, this.Current.TQPath, p => this.Current.TQPath = p);

	/// <summary>
	/// Sets the Immortal Throne path, trimmed; a change of case only is ignored.
	/// </summary>
	/// <returns>true when the path changed.</returns>
	public bool SetImmortalThronePath(string? path)
		=> SetPath(path, this.Current.TQITPath, p => this.Current.TQITPath = p);

	private static bool SetPath(string? path, string current, Action<string> set)
	{
		var trimmed = (path ?? string.Empty).Trim();
		if (trimmed.Equals(current.Trim(), StringComparison.OrdinalIgnoreCase))
			return false;

		set(trimmed);
		return true;
	}

	/// <summary>
	/// The git repository must be a valid URL when the git backup is on.
	/// </summary>
	public bool IsGitRepositoryValid
		=> !this.Current.GitBackupEnabled || GitRepositoryUrl.IsValid(this.Current.GitBackupRepository);

	#endregion

	#region Changes

	/// <summary>
	/// Any setting differs from the values the dialog opened with.
	/// </summary>
	public bool ConfigurationChanged => this.Effective != this.Original;

	public bool VaultPathChanged
		=> !string.Equals(this.Current.VaultPath, this.Original.VaultPath, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// The language changed, or its detection was turned on, which needs a restart to detect it.
	/// </summary>
	public bool LanguageChanged
		=> (this.Current.AutoDetectLanguage && !this.Original.AutoDetectLanguage)
		|| !string.Equals(this.Current.TQLanguage, this.Original.TQLanguage, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// A game path changed, or their detection was turned on, which needs a restart to detect them.
	/// </summary>
	public bool GamePathChanged
		=> (this.Current.AutoDetectGamePath && !this.Original.AutoDetectGamePath)
		|| !string.Equals(this.Current.TQPath, this.Original.TQPath, StringComparison.OrdinalIgnoreCase)
		|| !string.Equals(this.Current.TQITPath, this.Original.TQITPath, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// A setting read once at startup changed: the font, the hot reload, the backups or the original game support.
	/// </summary>
	public bool UISettingChanged
		=> this.Current.BaseFont != this.Original.BaseFont
		|| this.Current.EnableHotReload != this.Original.EnableHotReload
		|| this.Current.GitBackupEnabled != this.Original.GitBackupEnabled
		|| this.Current.GitBackupRepository != this.Original.GitBackupRepository
		|| this.Current.DisableLegacyBackup != this.Original.DisableLegacyBackup
		|| this.Current.GitBackupPlayerSavesEnabled != this.Original.GitBackupPlayerSavesEnabled
		|| this.Current.EnableOriginalTQSupport != this.Original.EnableOriginalTQSupport;

	public bool ItemBGColorOpacityChanged => this.Current.ItemBGColorOpacity != this.Original.ItemBGColorOpacity;

	public bool EnableItemRequirementRestrictionChanged
		=> this.Current.EnableItemRequirementRestriction != this.Original.EnableItemRequirementRestriction;

	public bool EnableCharacterEditChanged => this.Current.AllowCharacterEdit != this.Original.AllowCharacterEdit;

	public bool EnableTQVaultSoundsChanged => this.Current.EnableTQVaultSounds != this.Original.EnableTQVaultSounds;

	/// <summary>
	/// Why the changes need a restart, if they do.
	/// </summary>
	public RestartReason RestartReason
		=> RestartRule.Evaluate(this.LanguageChanged, this.GamePathChanged, this.UISettingChanged);

	#endregion

	/// <summary>
	/// Writes the edited values to the configuration.
	/// </summary>
	public void ApplyTo(UserSettings settings) => this.Effective.ApplyTo(settings);
}
