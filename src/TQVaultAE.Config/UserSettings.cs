using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace TQVaultAE.Config;

[XmlRoot(ElementName = "UserSettings")]
public class UserSettings
{
	#region Properties

	[XmlElement(ElementName = "SkipTitle")]
	public bool SkipTitle { get; set; } = true;

	[XmlElement(ElementName = "LoadLastVault")]
	public bool LoadLastVault { get; set; } = true;

	[XmlElement(ElementName = "LoadLastCharacter")]
	public bool LoadLastCharacter { get; set; } = true;

	[XmlElement(ElementName = "LastVaultName")]
	public string LastVaultName { get; set; } = string.Empty;

	[XmlElement(ElementName = "LastCharacterName")]
	public string LastCharacterName { get; set; } = string.Empty;

	[XmlElement(ElementName = "AutoDetectLanguage")]
	public bool AutoDetectLanguage { get; set; } = true;

	[XmlElement(ElementName = "AutoDetectGamePath")]
	public bool AutoDetectGamePath { get; set; } = true;

	[XmlElement(ElementName = "TQLanguage")]
	public string TQLanguage { get; set; } = string.Empty;

	[XmlElement(ElementName = "VaultPath")]
	public string VaultPath { get; set; } = string.Empty;

	[XmlElement(ElementName = "LoadAllFiles")]
	public bool LoadAllFiles { get; set; } = true;

	[XmlElement(ElementName = "SuppressWarnings")]
	public bool SuppressWarnings { get; set; } = false;

	[XmlElement(ElementName = "CheckForNewVersions")]
	public bool CheckForNewVersions { get; set; } = false;

	[XmlElement(ElementName = "AllowItemCopy")]
	public bool AllowItemCopy { get; set; } = false;

	[XmlElement(ElementName = "AllowItemEdit")]
	public bool AllowItemEdit { get; set; } = false;

	[XmlElement(ElementName = "TQITPath")]
	public string TQITPath { get; set; } = string.Empty;

	[XmlElement(ElementName = "TQPath")]
	public string TQPath { get; set; } = string.Empty;

	[XmlElement(ElementName = "ModEnabled")]
	public bool ModEnabled { get; set; } = false;

	[XmlElement(ElementName = "CustomMap")]
	public string CustomMap { get; set; } = string.Empty;

	[XmlElement(ElementName = "Scale")]
	public float Scale { get; set; } = 1;

	[XmlElement(ElementName = "LoadAllFilesCompleted")]
	public bool LoadAllFilesCompleted { get; set; } = true;

	[XmlElement(ElementName = "PlayerReadonly")]
	public bool PlayerReadonly { get; set; } = true;

	[XmlElement(ElementName = "AllowCharacterEdit")]
	public bool AllowCharacterEdit { get; set; } = false;

	[XmlElement(ElementName = "AllowCheats")]
	public bool AllowCheats { get; set; } = false;

	[XmlElement(ElementName = "ForceGamePath")]
	public string ForceGamePath { get; set; } = string.Empty;

	[XmlElement(ElementName = "BaseFont")]
	public string BaseFont { get; set; } = "AlbertusMT";

	[XmlElement(ElementName = "EnableDetailedTooltipView")]
	public bool EnableDetailedTooltipView { get; set; } = false;

	[XmlElement(ElementName = "ItemBGColorOpacity")]
	public int ItemBGColorOpacity { get; set; } = 15;

	[XmlElement(ElementName = "EnableItemRequirementRestriction")]
	public bool EnableItemRequirementRestriction { get; set; } = false;

	[XmlElement(ElementName = "EnableHotReload")]
	public bool EnableHotReload { get; set; } = false;

	[XmlElement(ElementName = "SearchQueries")]
	public string SearchQueries { get; set; } = string.Empty;

	[XmlElement(ElementName = "DisableTooltipEquipment")]
	public bool DisableTooltipEquipment { get; set; } = false;

	[XmlElement(ElementName = "DisableTooltipStash")]
	public bool DisableTooltipStash { get; set; } = false;

	[XmlElement(ElementName = "DisableTooltipTransfer")]
	public bool DisableTooltipTransfer { get; set; } = false;

	[XmlElement(ElementName = "DisableTooltipRelic")]
	public bool DisableTooltipRelic { get; set; } = false;

	[XmlElement(ElementName = "EnableTQVaultSounds")]
	public bool EnableTQVaultSounds { get; set; } = true;

	[XmlElement(ElementName = "CSVDelimiter")]
	public string CSVDelimiter { get; set; } = "Comma";

	[XmlElement(ElementName = "EnableEpicLegendaryAffixes")]
	public bool EnableEpicLegendaryAffixes { get; set; } = false;

	#endregion


	#region Logic

	static UserSettings _Default;
	public static UserSettings Default
	{
		get
		{
			if (_Default is null) _Default = Read();
			return _Default;
		}
	}

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


