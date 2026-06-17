using System.Reflection;
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

	[XmlElement(ElementName = nameof(ModEnabled))]
	public bool ModEnabled { get; set; } = false;

	[XmlElement(ElementName = nameof(CustomMap))]
	public string CustomMap { get; set; } = string.Empty;

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

	[XmlElement(ElementName = nameof(EnableDetailedTooltipView))]
	public bool EnableDetailedTooltipView { get; set; } = false;

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

	[XmlElement(ElementName = nameof(DisableTooltipRelic))]
	public bool DisableTooltipRelic { get; set; } = false;

	[XmlElement(ElementName = nameof(EnableTQVaultSounds))]
	public bool EnableTQVaultSounds { get; set; } = true;

	[XmlElement(ElementName = nameof(CSVDelimiter))]
	public string CSVDelimiter { get; set; } = "Comma";

	[XmlElement(ElementName = nameof(EnableEpicLegendaryAffixes))]
	public bool EnableEpicLegendaryAffixes { get; set; } = false;

	[XmlElement(ElementName = nameof(DisableAutoStacking))]
	public bool DisableAutoStacking { get; set; } = false;

	[XmlElement(ElementName = nameof(GitBackupEnabled))]
	public bool GitBackupEnabled { get; set; } = true;

	[XmlElement(ElementName = nameof(GitBackupRepository))]
	public string GitBackupRepository { get; set; } = string.Empty;

	[XmlElement(ElementName = nameof(DisableLegacyBackup))]
	public bool DisableLegacyBackup { get; set; } = false;

	[XmlElement(ElementName = nameof(GitBackupPlayerSavesEnabled))]
	public bool GitBackupPlayerSavesEnabled { get; set; } = false;

	[XmlElement(ElementName = nameof(EnableOriginalTQSupport))]
	public bool EnableOriginalTQSupport { get; set; } = false;

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
	public string UILanguage { get; set; }

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

	public void Save()
	{
		string xmlPath = ResolveUserSettingsFilePath();

		XmlSerializer serializer = new XmlSerializer(typeof(UserSettings));
		using var stream = new FileStream(xmlPath, FileMode.Create);
		serializer.Serialize(stream, this);
	}

	private static string ResolveUserSettingsFilePath()
	{
		var currentPath = new System.Uri(Assembly.GetExecutingAssembly().EscapedCodeBase).LocalPath;
		currentPath = Path.GetDirectoryName(currentPath);
		var xmlPath = Path.Combine(currentPath, "UserConfig.xml");
		return xmlPath;
	}

	public static UserSettings Read()
	{
		string xmlPath = ResolveUserSettingsFilePath();

		if (File.Exists(xmlPath))
			return ParseSettings(File.ReadAllText(xmlPath));

		return new UserSettings();// Default
	}

	public static UserSettings ParseSettings(string xmlData)
	{
		XmlSerializer serializer = new XmlSerializer(typeof(UserSettings));
		using var reader = new StringReader(xmlData);
		return (UserSettings)serializer.Deserialize(reader);
	}

	#endregion
}


