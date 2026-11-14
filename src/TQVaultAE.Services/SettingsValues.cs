using TQVaultAE.Config;

namespace TQVaultAE.Services;

/// <summary>
/// The values of <see cref="UserSettings"/> the settings dialog edits, copied so the dialog can change them without
/// touching the configuration until OK.
/// </summary>
public sealed record SettingsValues
{
	public bool SkipTitle { get; set; }
	public string VaultPath { get; set; } = string.Empty;
	public bool AllowItemCopy { get; set; }
	public bool AllowItemEdit { get; set; }
	public bool AllowCharacterEdit { get; set; }
	public bool LoadLastCharacter { get; set; }
	public bool LoadLastVault { get; set; }
	public bool AutoDetectLanguage { get; set; }

	/// <summary>
	/// The game language, as the English name of the culture.
	/// </summary>
	public string TQLanguage { get; set; } = string.Empty;

	public bool AutoDetectGamePath { get; set; }
	public string TQPath { get; set; } = string.Empty;
	public string TQITPath { get; set; } = string.Empty;
	public bool LoadAllFiles { get; set; }
	public bool SuppressWarnings { get; set; }
	public bool PlayerReadonly { get; set; }
	public string BaseFont { get; set; } = string.Empty;
	public bool EnableDetailedTooltipView { get; set; }
	public bool ShowInGameValues { get; set; }
	public bool TooltipShowInGameRolls { get; set; }
	public bool TooltipShowValueRanges { get; set; }
	public bool TooltipShowGrantedSkills { get; set; }
	public bool TooltipShowSetPieceCounts { get; set; }
	public int ItemBGColorOpacity { get; set; }
	public bool EnableItemRequirementRestriction { get; set; }
	public bool EnableHotReload { get; set; }
	public bool EnableTQVaultSounds { get; set; }
	public bool EnableEpicLegendaryAffixes { get; set; }
	public bool DisableAutoStacking { get; set; }

	/// <summary>
	/// Sorting a bag also stacks the shards of the same relic or charm (<c>&lt;Sort MergeRelics="..." /&gt;</c>).
	/// </summary>
	public bool MergeRelicsOnSort { get; set; }
	public bool LoadoutReplaceWorn { get; set; }

	public bool AllowCheats { get; set; }
	public bool GitBackupEnabled { get; set; }
	public string GitBackupRepository { get; set; } = string.Empty;
	public bool DisableLegacyBackup { get; set; }
	public bool GitBackupPlayerSavesEnabled { get; set; }
	public bool EnableOriginalTQSupport { get; set; }
	public string PasteBinApiKey { get; set; } = string.Empty;
	public string PasteBinExpiration { get; set; } = string.Empty;

	/// <summary>
	/// The name of the <c>CsvDelimiter</c> member.
	/// </summary>
	public string CSVDelimiter { get; set; } = string.Empty;

	/// <summary>
	/// The key and mouse bindings changed by the user, a copy edited by the key binds dialog.
	/// </summary>
	public KeyBindSettings KeyBinds { get; set; } = new();

	/// <summary>
	/// Copies the editable values of the configuration.
	/// </summary>
	/// <param name="settings">The configuration.</param>
	/// <param name="language">The game language in use, which may differ from the configured one when it is detected.</param>
	/// <param name="titanQuestPath">The Titan Quest path in use.</param>
	/// <param name="immortalThronePath">The Immortal Throne path in use.</param>
	public static SettingsValues From(UserSettings settings, string language, string titanQuestPath, string immortalThronePath)
		=> new()
		{
			SkipTitle = settings.SkipTitle,
			VaultPath = settings.VaultPath ?? string.Empty,
			AllowItemCopy = settings.AllowItemCopy,
			AllowItemEdit = settings.AllowItemEdit,
			AllowCharacterEdit = settings.AllowCharacterEdit,
			LoadLastCharacter = settings.LoadLastCharacter,
			LoadLastVault = settings.LoadLastVault,
			AutoDetectLanguage = settings.AutoDetectLanguage,
			TQLanguage = language,
			AutoDetectGamePath = settings.AutoDetectGamePath,
			TQPath = titanQuestPath,
			TQITPath = immortalThronePath,
			LoadAllFiles = settings.LoadAllFiles,
			SuppressWarnings = settings.SuppressWarnings,
			PlayerReadonly = settings.PlayerReadonly,
			BaseFont = settings.BaseFont ?? string.Empty,
			EnableDetailedTooltipView = settings.EnableDetailedTooltipView,
			ShowInGameValues = settings.ShowInGameValues,
			TooltipShowInGameRolls = settings.TooltipShowInGameRolls,
			TooltipShowValueRanges = settings.TooltipShowValueRanges,
			TooltipShowGrantedSkills = settings.TooltipShowGrantedSkills,
			TooltipShowSetPieceCounts = settings.TooltipShowSetPieceCounts,
			ItemBGColorOpacity = settings.ItemBGColorOpacity,
			EnableItemRequirementRestriction = settings.EnableItemRequirementRestriction,
			EnableHotReload = settings.EnableHotReload,
			EnableTQVaultSounds = settings.EnableTQVaultSounds,
			EnableEpicLegendaryAffixes = settings.EnableEpicLegendaryAffixes,
			DisableAutoStacking = settings.DisableAutoStacking,
			MergeRelicsOnSort = settings.Sort?.MergeRelics ?? false,
			LoadoutReplaceWorn = settings.Loadout?.ReplaceWorn ?? false,
			AllowCheats = settings.AllowCheats,
			GitBackupEnabled = settings.GitBackupEnabled,
			GitBackupRepository = settings.GitBackupRepository ?? string.Empty,
			DisableLegacyBackup = settings.DisableLegacyBackup,
			GitBackupPlayerSavesEnabled = settings.GitBackupPlayerSavesEnabled,
			EnableOriginalTQSupport = settings.EnableOriginalTQSupport,
			PasteBinApiKey = settings.PasteBinApiKey ?? string.Empty,
			PasteBinExpiration = settings.PasteBinExpiration ?? UserSettings.DefaultPasteBinExpiration,
			CSVDelimiter = settings.CSVDelimiter ?? string.Empty,
			KeyBinds = settings.KeyBinds.Clone(),
		};

	/// <summary>
	/// Writes the values to the configuration.
	/// </summary>
	public void ApplyTo(UserSettings settings)
	{
		settings.SkipTitle = this.SkipTitle;
		settings.VaultPath = this.VaultPath;
		settings.AllowItemCopy = this.AllowItemCopy;
		settings.AllowItemEdit = this.AllowItemEdit;
		settings.AllowCharacterEdit = this.AllowCharacterEdit;
		settings.LoadLastCharacter = this.LoadLastCharacter;
		settings.LoadLastVault = this.LoadLastVault;
		settings.AutoDetectLanguage = this.AutoDetectLanguage;
		settings.TQLanguage = this.TQLanguage;
		settings.AutoDetectGamePath = this.AutoDetectGamePath;
		settings.TQITPath = this.TQITPath;
		settings.TQPath = this.TQPath;
		settings.LoadAllFiles = this.LoadAllFiles;
		settings.SuppressWarnings = this.SuppressWarnings;
		settings.PlayerReadonly = this.PlayerReadonly;
		settings.BaseFont = this.BaseFont;
		settings.EnableDetailedTooltipView = this.EnableDetailedTooltipView;
		settings.ShowInGameValues = this.ShowInGameValues;
		settings.TooltipShowInGameRolls = this.TooltipShowInGameRolls;
		settings.TooltipShowValueRanges = this.TooltipShowValueRanges;
		settings.TooltipShowGrantedSkills = this.TooltipShowGrantedSkills;
		settings.TooltipShowSetPieceCounts = this.TooltipShowSetPieceCounts;
		settings.ItemBGColorOpacity = this.ItemBGColorOpacity;
		settings.EnableItemRequirementRestriction = this.EnableItemRequirementRestriction;
		settings.EnableHotReload = this.EnableHotReload;
		settings.EnableTQVaultSounds = this.EnableTQVaultSounds;
		settings.DisableAutoStacking = this.DisableAutoStacking;
		settings.Sort ??= new SortSettings();
		settings.Sort.MergeRelics = this.MergeRelicsOnSort;
		settings.Loadout ??= new LoadoutSettings();
		settings.Loadout.ReplaceWorn = this.LoadoutReplaceWorn;
		settings.AllowCheats = this.AllowCheats;
		settings.GitBackupEnabled = this.GitBackupEnabled;
		settings.DisableLegacyBackup = this.DisableLegacyBackup;
		settings.GitBackupRepository = this.GitBackupRepository;
		settings.GitBackupPlayerSavesEnabled = this.GitBackupPlayerSavesEnabled;
		settings.EnableOriginalTQSupport = this.EnableOriginalTQSupport;
		settings.PasteBinApiKey = this.PasteBinApiKey;
		settings.PasteBinExpiration = this.PasteBinExpiration;
		settings.EnableEpicLegendaryAffixes = this.EnableEpicLegendaryAffixes;
		settings.CSVDelimiter = this.CSVDelimiter;
		settings.KeyBinds = this.KeyBinds.Clone();
	}
}
