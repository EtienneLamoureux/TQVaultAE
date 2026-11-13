using System.Xml;
using System.Xml.Serialization;

namespace TQVaultAE.Config;

[XmlRoot(ElementName = nameof(UserSettings))]
public class UserSettings
{
	#region Properties
	
	[XmlElement(ElementName = nameof(SkipTitle))]
	public bool SkipTitle { get; set; } = true;

	[XmlElement(ElementName = nameof(LoadLastVault))]
	public bool LoadLastVault { get; set; } = true;

	[XmlElement(ElementName = nameof(LoadLastCharacter))]
	public bool LoadLastCharacter { get; set; } = true;

	[XmlElement(ElementName = nameof(LastVaultName))]
	public string LastVaultName { get; set; } = string.Empty;

	[XmlElement(ElementName = nameof(LastCharacterName))]
	public string LastCharacterName { get; set; } = string.Empty;

	[XmlElement(ElementName = nameof(AutoDetectLanguage))]
	public bool AutoDetectLanguage { get; set; } = true;

	[XmlElement(ElementName = nameof(AutoDetectGamePath))]
	public bool AutoDetectGamePath { get; set; } = true;

	[XmlElement(ElementName = nameof(TQLanguage))]
	public string TQLanguage { get; set; } = string.Empty;

	[XmlElement(ElementName = nameof(VaultPath))]
	public string VaultPath { get; set; } = string.Empty;

	[XmlElement(ElementName = nameof(LoadAllFiles))]
	public bool LoadAllFiles { get; set; } = true;

	[XmlElement(ElementName = nameof(SuppressWarnings))]
	public bool SuppressWarnings { get; set; } = false;

	[XmlElement(ElementName = nameof(CheckForNewVersions))]
	public bool CheckForNewVersions { get; set; } = false;

	[XmlElement(ElementName = nameof(AllowItemCopy))]
	public bool AllowItemCopy { get; set; } = false;

	[XmlElement(ElementName = nameof(AllowItemEdit))]
	public bool AllowItemEdit { get; set; } = false;

	[XmlElement(ElementName = nameof(TQITPath))]
	public string TQITPath { get; set; } = string.Empty;

	[XmlElement(ElementName = nameof(TQPath))]
	public string TQPath { get; set; } = string.Empty;

	[XmlElement(ElementName = nameof(Scale))]
	public float Scale { get; set; } = 1;

	[XmlElement(ElementName = nameof(LoadAllFilesCompleted))]
	public bool LoadAllFilesCompleted { get; set; } = true;

	[XmlElement(ElementName = nameof(PlayerReadonly))]
	public bool PlayerReadonly { get; set; } = true;

	[XmlElement(ElementName = nameof(AllowCharacterEdit))]
	public bool AllowCharacterEdit { get; set; } = false;

	[XmlElement(ElementName = nameof(AllowCheats))]
	public bool AllowCheats { get; set; } = false;

	[XmlElement(ElementName = nameof(ForceGamePath))]
	public string ForceGamePath { get; set; } = string.Empty;

	[XmlElement(ElementName = nameof(BaseFont))]
	public string BaseFont { get; set; } = "AlbertusMT";

	/// <summary>
	/// "Tooltip Enhanced": item tooltips split their attributes into prefix, base and suffix,
	/// with the extras chosen by the <c>Tooltip*</c> settings below.
	/// </summary>
	[XmlElement(ElementName = nameof(EnableDetailedTooltipView))]
	public bool EnableDetailedTooltipView { get; set; } = false;

	/// <summary>The enhanced tooltip lists each value rolled by the in-game jitter (database -> in-game).</summary>
	[XmlElement(ElementName = nameof(TooltipShowInGameRolls))]
	public bool TooltipShowInGameRolls { get; set; } = false;

	/// <summary>The enhanced tooltip shows the possible min/max of each rolled attribute on its right.</summary>
	[XmlElement(ElementName = nameof(TooltipShowValueRanges))]
	public bool TooltipShowValueRanges { get; set; } = false;

	/// <summary>The enhanced tooltip shows the skills granted by the item in a panel on its right.</summary>
	[XmlElement(ElementName = nameof(TooltipShowGrantedSkills))]
	public bool TooltipShowGrantedSkills { get; set; } = true;

	/// <summary>The enhanced tooltip shows, next to each piece of the item's set, how many of it are loaded.</summary>
	[XmlElement(ElementName = nameof(TooltipShowSetPieceCounts))]
	public bool TooltipShowSetPieceCounts { get; set; } = false;

	/// <summary>
	/// Show item attributes with the in-game jitter applied (issue #552) instead of the database values.
	/// </summary>
	[XmlElement(ElementName = nameof(ShowInGameValues))]
	public bool ShowInGameValues { get; set; } = true;

	[XmlElement(ElementName = nameof(ItemBGColorOpacity))]
	public int ItemBGColorOpacity { get; set; } = 15;

	[XmlElement(ElementName = nameof(EnableItemRequirementRestriction))]
	public bool EnableItemRequirementRestriction { get; set; } = false;

	[XmlElement(ElementName = nameof(EnableHotReload))]
	public bool EnableHotReload { get; set; } = false;

	[XmlElement(ElementName = nameof(DisableTooltipEquipment))]
	public bool DisableTooltipEquipment { get; set; } = false;

	[XmlElement(ElementName = nameof(DisableTooltipStash))]
	public bool DisableTooltipStash { get; set; } = false;

	[XmlElement(ElementName = nameof(DisableTooltipTransfer))]
	public bool DisableTooltipTransfer { get; set; } = false;

	/// <summary>
	/// Hides the bag tooltips of the relic vault ("Enable Tooltip" unchecked on the Relic tab). Hidden by default.
	/// </summary>
	[XmlElement(ElementName = nameof(DisableTooltipRelic))]
	public bool DisableTooltipRelic { get; set; } = true;

	[XmlElement(ElementName = nameof(EnableTQVaultSounds))]
	public bool EnableTQVaultSounds { get; set; } = true;

	[XmlElement(ElementName = nameof(CSVDelimiter))]
	public string CSVDelimiter { get; set; } = "Comma";

	/// <summary>
	/// The character of <see cref="CSVDelimiter"/>, a comma when the saved value is unknown.
	/// </summary>
	[XmlIgnore]
	public char CsvDelimiterChar => CsvDelimiters.Parse(this.CSVDelimiter).ToChar();

	[XmlElement(ElementName = nameof(EnableEpicLegendaryAffixes))]
	public bool EnableEpicLegendaryAffixes { get; set; } = false;

	[XmlElement(ElementName = nameof(DisableAutoStacking))]
	public bool DisableAutoStacking { get; set; } = false;

	/// <summary>
	/// Backs the saves up to a git repository. Opt-in: off by default, as in the shipped template.
	/// </summary>
	[XmlElement(ElementName = nameof(GitBackupEnabled))]
	public bool GitBackupEnabled { get; set; } = false;

	[XmlElement(ElementName = nameof(GitBackupRepository))]
	public string GitBackupRepository { get; set; } = string.Empty;

	[XmlElement(ElementName = nameof(DisableLegacyBackup))]
	public bool DisableLegacyBackup { get; set; } = false;

	[XmlElement(ElementName = nameof(GitBackupPlayerSavesEnabled))]
	public bool GitBackupPlayerSavesEnabled { get; set; } = false;

	[XmlElement(ElementName = nameof(EnableOriginalTQSupport))]
	public bool EnableOriginalTQSupport { get; set; } = false;

	[XmlElement(ElementName = nameof(PasteBinApiKey))]
	public string PasteBinApiKey { get; set; } = string.Empty;

	/// <summary>
	/// PasteBin expiration used when none is set: one month.
	/// </summary>
	public const string DefaultPasteBinExpiration = "1M";

	[XmlElement(ElementName = nameof(PasteBinExpiration))]
	public string PasteBinExpiration { get; set; } = DefaultPasteBinExpiration;

	/// <summary>
	/// The toolbar choices of the item comparison dialog: <c>&lt;ItemCompare AlignRows="1" HideUnchanged="0" ShowSources="0" /&gt;</c>.
	/// Missing, the defaults apply (rows aligned, unchanged stats shown, sources collapsed).
	/// </summary>
	[XmlElement(ElementName = nameof(ItemCompare))]
	public ItemCompareSettings ItemCompare { get; set; } = new();

	/// <summary>
	/// The choices of the bag sort button: <c>&lt;Sort Algorithm="Normal" MergeRelics="0" /&gt;</c>.
	/// Missing, the defaults apply (the normal sort, shards left as they are).
	/// </summary>
	[XmlElement(ElementName = nameof(Sort))]
	public SortSettings Sort { get; set; } = new();

	/// <summary>
	/// The key and mouse bindings changed by the user: <c>&lt;KeyBinds ItemDuplicateClick="Ctrl+Click" /&gt;</c>.
	/// Missing or empty, every action has its default.
	/// </summary>
	[XmlElement(ElementName = nameof(KeyBinds))]
	public KeyBindSettings KeyBinds { get; set; } = new();

	#endregion

	#region AppSettings & DebugLevels

	/// <summary>
	/// Is loot table debug enabled?
	/// </summary>
	[XmlElement(ElementName = nameof(LootTableDebugEnabled))]
	public bool LootTableDebugEnabled { get; set; }
	
	/// <summary>
	/// Gets or sets the arc file debug level
	/// </summary>
	[XmlElement(ElementName = nameof(ARCFileDebugLevel))]
	public int ARCFileDebugLevel { get; set; }
	
	/// <summary>
	/// Gets or sets the database debug level
	/// </summary>
	[XmlElement(ElementName = nameof(DatabaseDebugLevel))]
	public int DatabaseDebugLevel { get; set; }
	
	/// <summary>
	/// Gets or sets the item debug level
	/// </summary>
	[XmlElement(ElementName = nameof(ItemDebugLevel))]
	public int ItemDebugLevel { get; set; }

	/// <summary>
	/// Gets or sets the item attributes debug level
	/// </summary>
	[XmlElement(ElementName = nameof(ItemAttributesDebugLevel))]
	public int ItemAttributesDebugLevel { get; set; }

	/// <summary>
	/// de,en,es,fr,it,pl,ru,cs,zh
	/// </summary>
	[XmlElement(ElementName = nameof(GameLanguages))]
	public string GameLanguages { get; set; } = "de,en,es,fr,it,pl,ru,cs,zh";

	[XmlElement(ElementName = nameof(UILanguage))]
	public string? UILanguage { get; set; }

	/// <summary>
	/// https://github.com/EtienneLamoureux/TQVaultAE
	/// </summary>
	[XmlElement(ElementName = nameof(UpdateURL))]
	public string UpdateURL { get; set; } = "https://github.com/EtienneLamoureux/TQVaultAE";
	
	[XmlElement(ElementName = nameof(ShowSkillLevel))]
	public bool ShowSkillLevel { get; set; }
	
	[XmlElement(ElementName = nameof(FadeInInterval))]
	public double FadeInInterval { get; set; } = 0.1;
	
	[XmlElement(ElementName = nameof(FadeOutInterval))]
	public double FadeOutInterval { get; set; } = 0.2;
	
	[XmlElement(ElementName = nameof(DebugEnabled))]
 	public bool DebugEnabled { get; set; }

	[XmlElement(ElementName = nameof(TQOriginalHowtoUrl))]
	public string TQOriginalHowtoUrl { get; set; } = "https://github.com/EtienneLamoureux/TQVaultAE/blob/master/documentation/TQORIGINAL.md";

	#endregion

	#region Logic

	/// <summary>
	/// Saves the settings to the <c>UserConfig.xml</c> next to the application.
	/// </summary>
	public void Save() => this.Save(ResolveUserSettingsFilePath());

	/// <summary>
	/// Saves the settings to <paramref name="xmlPath"/>, replacing the file.
	/// </summary>
	public void Save(string xmlPath)
	{
		XmlSerializer serializer = new XmlSerializer(typeof(UserSettings));
		using var stream = new FileStream(xmlPath, FileMode.Create);
		serializer.Serialize(stream, this);
	}

	private static string ResolveUserSettingsFilePath()
	{
		return Path.Combine(AppContext.BaseDirectory, "UserConfig.xml");
	}

	/// <summary>
	/// Reads the <c>UserConfig.xml</c> next to the application, or the defaults when there is none.
	/// </summary>
	public static UserSettings Read() => Read(ResolveUserSettingsFilePath());

	/// <summary>
	/// Reads the settings from <paramref name="xmlPath"/>, or the defaults when the file does not exist.
	/// Settings missing from the file keep their default. An empty file, as an interrupted save leaves it, gives the defaults too.
	/// </summary>
	public static UserSettings Read(string xmlPath)
	{
		if (!File.Exists(xmlPath))
			return new UserSettings();

		var xmlData = File.ReadAllText(xmlPath);
		if (string.IsNullOrWhiteSpace(xmlData))
			return new UserSettings();

		return ParseSettings(xmlData);
	}

	public static UserSettings ParseSettings(string xmlData)
	{
		XmlSerializer serializer = new XmlSerializer(typeof(UserSettings));
		using var reader = XmlReader.Create(new StringReader(xmlData), new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
		return (UserSettings?)serializer.Deserialize(reader) ?? new UserSettings();
	}

	#endregion
}


