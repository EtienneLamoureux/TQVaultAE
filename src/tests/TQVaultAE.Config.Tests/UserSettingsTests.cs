using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using AwesomeAssertions;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Config;

/// <summary>
/// Loading and saving of <c>UserConfig.xml</c>.
/// </summary>
public sealed class UserSettingsTests : IDisposable
{
	private readonly string _folder;
	private readonly string _xmlPath;

	public UserSettingsTests()
	{
		this._folder = Path.Combine(Path.GetTempPath(), "TQVaultAE.Tests", "UserSettings", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(this._folder);
		this._xmlPath = Path.Combine(this._folder, "UserConfig.xml");
	}

	public void Dispose()
	{
		if (Directory.Exists(this._folder))
			Directory.Delete(this._folder, true);
	}

	/// <summary>The template copied next to the application at build.</summary>
	private static string TemplatePath => Path.Combine(AppContext.BaseDirectory, "Config", "UserConfig.template.xml");

	/// <summary>A path with characters outside ASCII and Latin-1.</summary>
	private const string NonAsciiPath = """C:\Jeux\Titan Quest – Édition Anniversaire\Œuvres""";

	private static IEnumerable<PropertyInfo> SavedProperties()
		=> typeof(UserSettings)
			.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Where(p => p.CanRead && p.CanWrite && p.GetCustomAttribute<XmlIgnoreAttribute>() is null);

	/// <summary>
	/// Settings where every saved property differs from its default.
	/// </summary>
	private static UserSettings WithEveryValueChanged()
	{
		var settings = new UserSettings();
		var i = 0;
		foreach (var property in SavedProperties())
		{
			i++;
			object value = property.PropertyType switch
			{
				var t when t == typeof(bool) => !(bool)property.GetValue(settings)!,
				var t when t == typeof(string) => $"""{property.Name} {NonAsciiPath} {i}""",
				var t when t == typeof(int) => -1000 - i,
				var t when t == typeof(float) => 0.94945055F + i,
				var t when t == typeof(double) => -0.123456789012345 - i,
				var t when t == typeof(ItemCompareSettings) => new ItemCompareSettings { AlignRows = false, HideUnchanged = true, ShowSources = true },
				var t when t == typeof(SortSettings) => SortWithOwnOrders(),
				var t when t == typeof(LoadoutSettings) => new LoadoutSettings { ReplaceWorn = true },
				var t when t == typeof(KeyBindSettings) => KeyBinds(("ItemDuplicateClick", "Ctrl+Click"), ("ZoomIn", "")),
				_ => throw new NotSupportedException($"Add a test value for {property.PropertyType} ({property.Name})."),
			};
			property.SetValue(settings, value);
		}

		return settings;
	}

	/// <summary>Sort choices with a default and every button's own order changed.</summary>
	private static SortSettings SortWithOwnOrders()
	{
		var sort = new SortSettings { Algorithm = SackSortAlgorithm.Rarity, MergeRelics = true };
		sort.SetAlgorithm(SortButtonKey.Vault, SackSortAlgorithm.Level);
		sort.SetAlgorithm(SortButtonKey.SecondaryVault, SackSortAlgorithm.Type);
		sort.SetAlgorithm(SortButtonKey.Player, SackSortAlgorithm.Normal);
		sort.SetAlgorithm(SortButtonKey.PlayerBag, SackSortAlgorithm.Level);
		sort.SetAlgorithm(SortButtonKey.Stash, SackSortAlgorithm.Type);
		return sort;
	}

	private static T InCulture<T>(string cultureName, Func<T> action)
	{
		var previous = CultureInfo.CurrentCulture;
		CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
		try
		{
			return action();
		}
		finally
		{
			CultureInfo.CurrentCulture = previous;
		}
	}

	#region Missing, empty, round trip

	[Fact]
	public void Read_MissingFile_ReturnsDefaults()
	{
		var settings = UserSettings.Read(this._xmlPath);

		settings.Should().BeEquivalentTo(new UserSettings());
		File.Exists(this._xmlPath).Should().BeFalse("reading must not create the file");
	}

	[Theory]
	[InlineData("")]
	[InlineData("   \r\n")]
	[InlineData("\uFEFF")]
	public void Read_EmptyFile_ReturnsDefaults(string content)
	{
		// What an interrupted save leaves: Save truncates the file before writing it.
		File.WriteAllText(this._xmlPath, content, new UTF8Encoding(false));

		var settings = UserSettings.Read(this._xmlPath);

		settings.Should().BeEquivalentTo(new UserSettings());
	}

	[Fact]
	public void SaveThenRead_KeepsEverySetting()
	{
		var saved = WithEveryValueChanged();

		saved.Save(this._xmlPath);
		var read = UserSettings.Read(this._xmlPath);

		read.Should().BeEquivalentTo(saved);
	}

	[Fact]
	public void SaveThenRead_NullUILanguage_StaysUnset()
	{
		var saved = new UserSettings { UILanguage = null };

		saved.Save(this._xmlPath);
		var read = UserSettings.Read(this._xmlPath);

		read.UILanguage.Should().BeNullOrEmpty("StartupService treats an empty UI language as unset");
	}

	[Fact]
	public void Save_OverALongerFile_LeavesNothingOfTheOldContent()
	{
		File.WriteAllText(this._xmlPath, new string(' ', 100_000) + "<Garbage/>");

		new UserSettings { VaultPath = """D:\Vaults""" }.Save(this._xmlPath);
		var read = UserSettings.Read(this._xmlPath);

		read.VaultPath.Should().Be("""D:\Vaults""");
		File.ReadAllText(this._xmlPath).Should().NotContain("Garbage");
	}

	#endregion

	#region Loadouts

	[Fact]
	public void Loadout_Missing_WornPiecesStored()
		=> UserSettings.ParseSettings("""<UserSettings><SkipTitle>1</SkipTitle></UserSettings>""").Loadout.ReplaceWorn.Should().BeFalse();

	[Theory]
	[InlineData("ReplaceWorn='1'", true)]
	[InlineData("ReplaceWorn='0'", false)]
	[InlineData("ReplaceWorn='maybe'", false)]
	public void Loadout_AttributeParsed_UnknownKeepsTheDefault(string attributes, bool replaceWorn)
		=> UserSettings.ParseSettings($"""<UserSettings><Loadout {attributes} /></UserSettings>""").Loadout.ReplaceWorn.Should().Be(replaceWorn);

	[Fact]
	public void Loadout_SaveThenRead_OneElementWithAZeroOneAttribute()
	{
		new UserSettings { Loadout = new LoadoutSettings { ReplaceWorn = true } }.Save(this._xmlPath);

		var element = XDocument.Load(this._xmlPath).Root!.Elements("Loadout").Should().ContainSingle().Subject;
		element.Attribute("ReplaceWorn")!.Value.Should().Be("1");
		UserSettings.Read(this._xmlPath).Loadout.ReplaceWorn.Should().BeTrue();
	}

	#endregion

	#region Item comparison toolbar

	[Fact]
	public void ItemCompare_Missing_RowsAlignedAndUnchangedShown()
	{
		var settings = UserSettings.ParseSettings("""<?xml version="1.0" encoding="utf-8" ?><UserSettings><SkipTitle>1</SkipTitle></UserSettings>""");

		settings.ItemCompare.AlignRows.Should().BeTrue();
		settings.ItemCompare.HideUnchanged.Should().BeFalse();
		settings.ItemCompare.ShowSources.Should().BeFalse();
	}

	[Theory]
	[InlineData("AlignRows='0' HideUnchanged='1'", false, true, false)]
	[InlineData("AlignRows='true' HideUnchanged=' False '", true, false, false)]
	[InlineData("HideUnchanged='1'", true, true, false)]
	[InlineData("AlignRows='maybe'", true, false, false)]
	[InlineData("ShowSources='1'", true, false, true)]
	[InlineData("ShowSources='maybe'", true, false, false)]
	public void ItemCompare_AttributesParsed_UnknownOrMissingKeepTheDefault(string attributes, bool alignRows, bool hideUnchanged, bool showSources)
	{
		var settings = UserSettings.ParseSettings($"""<UserSettings><ItemCompare {attributes} /></UserSettings>""");

		settings.ItemCompare.AlignRows.Should().Be(alignRows);
		settings.ItemCompare.HideUnchanged.Should().Be(hideUnchanged);
		settings.ItemCompare.ShowSources.Should().Be(showSources);
	}

	[Fact]
	public void ItemCompare_SaveThenRead_OneElementWithZeroOneAttributes()
	{
		new UserSettings { ItemCompare = new ItemCompareSettings { AlignRows = false, HideUnchanged = true, ShowSources = true } }.Save(this._xmlPath);

		var element = XDocument.Load(this._xmlPath).Root!.Elements("ItemCompare").Should().ContainSingle().Subject;
		element.Attribute("AlignRows")!.Value.Should().Be("0");
		element.Attribute("HideUnchanged")!.Value.Should().Be("1");
		element.Attribute("ShowSources")!.Value.Should().Be("1");
		element.HasElements.Should().BeFalse();

		var read = UserSettings.Read(this._xmlPath);
		read.ItemCompare.AlignRows.Should().BeFalse();
		read.ItemCompare.HideUnchanged.Should().BeTrue();
		read.ItemCompare.ShowSources.Should().BeTrue();
	}

	#endregion

	#region Sort button

	[Fact]
	public void Sort_Missing_NormalSortWithoutMerge()
	{
		var settings = UserSettings.ParseSettings("""<?xml version="1.0" encoding="utf-8" ?><UserSettings><SkipTitle>1</SkipTitle></UserSettings>""");

		settings.Sort.Algorithm.Should().Be(SackSortAlgorithm.Normal);
		settings.Sort.MergeRelics.Should().BeFalse();
	}

	[Theory]
	[InlineData("Algorithm='Level' MergeRelics='1'", SackSortAlgorithm.Level, true)]
	[InlineData("Algorithm=' rarity ' MergeRelics='false'", SackSortAlgorithm.Rarity, false)]
	[InlineData("Algorithm='Type'", SackSortAlgorithm.Type, false)]
	[InlineData("Algorithm='Weight' MergeRelics='maybe'", SackSortAlgorithm.Normal, false)]
	[InlineData("Algorithm='2'", SackSortAlgorithm.Normal, false)]
	[InlineData("Algorithm='Level, Type'", SackSortAlgorithm.Normal, false)]
	public void Sort_AttributesParsed_UnknownOrMissingKeepTheDefault(string attributes, SackSortAlgorithm algorithm, bool mergeRelics)
	{
		var settings = UserSettings.ParseSettings($"""<UserSettings><Sort {attributes} /></UserSettings>""");

		settings.Sort.Algorithm.Should().Be(algorithm);
		settings.Sort.MergeRelics.Should().Be(mergeRelics);
	}

	[Fact]
	public void Sort_SaveThenRead_OneElementWithAttributes()
	{
		new UserSettings { Sort = new SortSettings { Algorithm = SackSortAlgorithm.Level, MergeRelics = true } }.Save(this._xmlPath);

		var element = XDocument.Load(this._xmlPath).Root!.Elements("Sort").Should().ContainSingle().Subject;
		element.Attribute("Algorithm")!.Value.Should().Be("Level");
		element.Attribute("MergeRelics")!.Value.Should().Be("1");
		element.HasElements.Should().BeFalse();

		var read = UserSettings.Read(this._xmlPath);
		read.Sort.Algorithm.Should().Be(SackSortAlgorithm.Level);
		read.Sort.MergeRelics.Should().BeTrue();
	}

	#endregion

	#region Key binds

	private static KeyBindSettings KeyBinds(params (string Name, string Value)[] overrides)
	{
		var settings = new KeyBindSettings();
		settings.SetOverrides(overrides.Select(o => KeyValuePair.Create(o.Name, o.Value)));
		return settings;
	}

	[Fact]
	public void KeyBinds_Missing_NoChange()
	{
		var settings = UserSettings.ParseSettings("""<UserSettings><SkipTitle>1</SkipTitle></UserSettings>""");

		settings.KeyBinds.Overrides.Should().BeEmpty();
	}

	[Fact]
	public void KeyBinds_SaveThenRead_OneElementWithAnAttributePerChangedAction()
	{
		new UserSettings { KeyBinds = KeyBinds(("ItemDuplicateClick", "Ctrl+Click"), ("ZoomIn", "Ctrl+Add, Ctrl+Oemplus"), ("Forge", "")) }
			.Save(this._xmlPath);

		var element = XDocument.Load(this._xmlPath).Root!.Elements("KeyBinds").Should().ContainSingle().Subject;
		element.Attribute("ItemDuplicateClick")!.Value.Should().Be("Ctrl+Click");
		element.Attribute("ZoomIn")!.Value.Should().Be("Ctrl+Add, Ctrl+Oemplus");
		element.Attribute("Forge")!.Value.Should().BeEmpty("an action without a binding is kept as such");
		element.HasElements.Should().BeFalse();

		var read = UserSettings.Read(this._xmlPath);
		read.KeyBinds.Should().Be(KeyBinds(("Forge", ""), ("ItemDuplicateClick", "Ctrl+Click"), ("ZoomIn", "Ctrl+Add, Ctrl+Oemplus")));
	}

	[Fact]
	public void KeyBinds_NameNotAnXmlName_SkippedAtSave()
	{
		new UserSettings { KeyBinds = KeyBinds(("Zoom In", "Ctrl+Add"), ("Search", "Ctrl+G")) }.Save(this._xmlPath);

		UserSettings.Read(this._xmlPath).KeyBinds.Should().Be(KeyBinds(("Search", "Ctrl+G")));
	}

	[Fact]
	public void KeyBinds_Equality_ComparesTheOverridesNotTheInstances()
	{
		KeyBinds(("A", "1"), ("B", "2")).Should().Be(KeyBinds(("B", "2"), ("A", "1")));
		KeyBinds(("A", "1")).Should().NotBe(KeyBinds(("A", "2")));
		KeyBinds(("A", "1")).Clone().Should().Be(KeyBinds(("A", "1")));
	}

	#endregion

	#region Template and older versions

	[Fact]
	public void Template_EveryElementIsASetting()
	{
		var known = SavedProperties()
			.Select(p => p.GetCustomAttribute<XmlElementAttribute>()?.ElementName ?? p.Name)
			.ToHashSet(StringComparer.Ordinal);

		var elements = XDocument.Load(TemplatePath).Root!.Elements().Select(e => e.Name.LocalName);

		elements.Should().OnlyContain(name => known.Contains(name), "an element without a property is silently ignored");
	}

	[Fact]
	public void Template_ReadsAsTheDefaults()
	{
		var template = UserSettings.Read(TemplatePath);

		template.Should().BeEquivalentTo(new UserSettings(), "a fresh install and a missing file must start alike");
		template.GitBackupEnabled.Should().BeFalse("the git backup is opt-in");
		template.VaultPath.Should().BeEmpty("an empty vault path makes the startup ask for one");
	}

	/// <summary>
	/// The template of the first <c>UserConfig.xml</c> (2022, commit 18d8adb): two-space indent,
	/// <c>SearchQueries</c> since removed, and none of the settings added later.
	/// </summary>
	private const string Version2022 = """
		<?xml version="1.0" encoding="utf-8" ?>
		<UserSettings>
		  <SkipTitle>1</SkipTitle>
		  <LoadLastVault>1</LoadLastVault>
		  <LoadLastCharacter>1</LoadLastCharacter>
		  <LastVaultName>Rings</LastVaultName>
		  <LastCharacterName />
		  <AutoDetectLanguage>1</AutoDetectLanguage>
		  <AutoDetectGamePath>1</AutoDetectGamePath>
		  <TQLanguage />
		  <VaultPath />
		  <LoadAllFiles >1</LoadAllFiles>
		  <SuppressWarnings>0</SuppressWarnings>
		  <CheckForNewVersions>0</CheckForNewVersions>
		  <AllowItemCopy>0</AllowItemCopy>
		  <AllowItemEdit>0</AllowItemEdit>
		  <TQITPath  />
		  <TQPath />
		  <ModEnabled>0</ModEnabled>
		  <CustomMap />
		  <Scale>1.25</Scale>
		  <LoadAllFilesCompleted>1</LoadAllFilesCompleted>
		  <PlayerReadonly>1</PlayerReadonly>
		  <AllowCharacterEdit>0</AllowCharacterEdit>
		  <AllowCheats>1</AllowCheats>
		  <ForceGamePath />
		  <BaseFont>AlbertusMT</BaseFont>
		  <EnableDetailedTooltipView>0</EnableDetailedTooltipView>
		  <ItemBGColorOpacity>15</ItemBGColorOpacity>
		  <EnableItemRequirementRestriction>0</EnableItemRequirementRestriction>
		  <EnableHotReload>0</EnableHotReload>
		  <SearchQueries />
		  <DisableTooltipEquipment>0</DisableTooltipEquipment>
		  <DisableTooltipStash>0</DisableTooltipStash>
		  <DisableTooltipTransfer>0</DisableTooltipTransfer>
		  <DisableTooltipRelic>0</DisableTooltipRelic>
		  <EnableTQVaultSounds>1</EnableTQVaultSounds>
		  <CSVDelimiter>Comma</CSVDelimiter>
		  <EnableEpicLegendaryAffixes>0</EnableEpicLegendaryAffixes>
		</UserSettings>
		""";

	[Fact]
	public void TooltipEnhanced_ByDefault_ShowsOnlyTheItemSkills()
	{
		var settings = new UserSettings();

		settings.EnableDetailedTooltipView.Should().BeFalse();
		settings.TooltipShowInGameRolls.Should().BeFalse();
		settings.TooltipShowValueRanges.Should().BeFalse();
		settings.TooltipShowGrantedSkills.Should().BeTrue();
		settings.TooltipShowSetPieceCounts.Should().BeFalse();
	}

	[Fact]
	public void Read_2022File_KeepsItsValuesAndDefaultsTheNewSettings()
	{
		File.WriteAllText(this._xmlPath, Version2022, new UTF8Encoding(true));

		var settings = UserSettings.Read(this._xmlPath);

		// Values of the file, including the ones that differ from today's defaults.
		settings.AllowCheats.Should().BeTrue();
		settings.DisableTooltipRelic.Should().BeFalse();
		settings.LastVaultName.Should().Be("Rings");
		settings.Scale.Should().Be(1.25F);
		// Self-closing elements are empty strings, not null.
		settings.VaultPath.Should().BeEmpty();
		settings.TQITPath.Should().BeEmpty();
		settings.LastCharacterName.Should().BeEmpty();
		// Settings added since keep their default.
		settings.ShowInGameValues.Should().BeTrue();
		settings.TooltipShowSetPieceCounts.Should().BeFalse();
		settings.GitBackupEnabled.Should().BeFalse("the git backup is opt-in, also for files older than the setting");
		settings.GitBackupRepository.Should().BeEmpty();
		settings.PasteBinExpiration.Should().Be(UserSettings.DefaultPasteBinExpiration);
		settings.GameLanguages.Should().Be("de,en,es,fr,it,pl,ru,cs,zh");
	}

	[Fact]
	public void Read_2022File_SavesWithoutTheRemovedSetting()
	{
		File.WriteAllText(this._xmlPath, Version2022);

		UserSettings.Read(this._xmlPath).Save(this._xmlPath);

		File.ReadAllText(this._xmlPath).Should().NotContain("SearchQueries");
		UserSettings.Read(this._xmlPath).AllowCheats.Should().BeTrue();
	}

	#endregion

	#region Culture

	/// <summary>A file as the app saves it, with a zoom that is not round.</summary>
	private const string SavedByTheApp = """
		<?xml version="1.0"?>
		<UserSettings xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
		  <Scale>0.94945055</Scale>
		  <ItemBGColorOpacity>-3</ItemBGColorOpacity>
		  <FadeInInterval>0.05</FadeInInterval>
		  <GitBackupRepository />
		</UserSettings>
		""";

	[Theory]
	[InlineData("fr-FR")]
	[InlineData("de-DE")]
	[InlineData("sv-SE")] // U+2212 as minus sign
	[InlineData("tr-TR")]
	public void Read_InAnotherCulture_ReadsInvariantNumbers(string cultureName)
	{
		File.WriteAllText(this._xmlPath, SavedByTheApp);

		var settings = InCulture(cultureName, () => UserSettings.Read(this._xmlPath));

		settings.Scale.Should().Be(0.94945055F);
		settings.ItemBGColorOpacity.Should().Be(-3);
		settings.FadeInInterval.Should().Be(0.05);
		settings.GitBackupRepository.Should().BeEmpty();
	}

	[Theory]
	[InlineData("fr-FR")]
	[InlineData("de-DE")]
	[InlineData("sv-SE")]
	[InlineData("tr-TR")]
	public void Save_InAnotherCulture_WritesInvariantNumbers(string cultureName)
	{
		var settings = new UserSettings { Scale = 0.94945055F, ItemBGColorOpacity = -3, FadeInInterval = 0.05, SkipTitle = true };

		InCulture(cultureName, () =>
		{
			settings.Save(this._xmlPath);
			return true;
		});

		var root = XDocument.Load(this._xmlPath).Root!;
		root.Element(nameof(UserSettings.Scale))!.Value.Should().Be("0.94945055");
		root.Element(nameof(UserSettings.ItemBGColorOpacity))!.Value.Should().Be("-3");
		root.Element(nameof(UserSettings.FadeInInterval))!.Value.Should().Be("0.05");
		root.Element(nameof(UserSettings.SkipTitle))!.Value.Should().Be("true");
		UserSettings.Read(this._xmlPath).Should().BeEquivalentTo(settings);
	}

	#endregion

	#region Encoding

	public static TheoryData<string, string> Encodings => new()
	{
		{ "utf-8 BOM", "utf-8" },
		{ "utf-8", "utf-8" },
		{ "utf-16 BOM", "utf-16" },
		{ "utf-16BE BOM", "utf-16" },
		// Notepad "Save as Unicode" keeps the declaration of the file.
		{ "utf-16 BOM", "utf-8" },
	};

	private static Encoding EncodingOf(string name)
		=> name switch
		{
			"utf-8 BOM" => new UTF8Encoding(true),
			"utf-8" => new UTF8Encoding(false),
			"utf-16 BOM" => new UnicodeEncoding(false, true),
			"utf-16BE BOM" => new UnicodeEncoding(true, true),
			_ => throw new ArgumentOutOfRangeException(nameof(name)),
		};

	[Theory]
	[MemberData(nameof(Encodings))]
	public void Read_FileInEncoding_KeepsNonAsciiText(string fileEncoding, string declaredEncoding)
	{
		var xml = $"""
			<?xml version="1.0" encoding="{declaredEncoding}"?>
			<UserSettings>
				<VaultPath>{NonAsciiPath}</VaultPath>
				<Scale>1.5</Scale>
			</UserSettings>
			""";
		File.WriteAllText(this._xmlPath, xml, EncodingOf(fileEncoding));

		var settings = UserSettings.Read(this._xmlPath);

		settings.VaultPath.Should().Be(NonAsciiPath);
		settings.Scale.Should().Be(1.5F);
	}

	[Fact]
	public void Save_WritesUtf8()
	{
		new UserSettings { VaultPath = NonAsciiPath }.Save(this._xmlPath);

		var text = File.ReadAllText(this._xmlPath, new UTF8Encoding(false, true));

		text.Should().Contain($"<VaultPath>{NonAsciiPath}</VaultPath>");
	}

	#endregion

	#region Invalid content

	[Theory]
	[InlineData("<Scale>abc</Scale>")]
	[InlineData("<Scale>0,95</Scale>")] // decimal comma typed by hand
	[InlineData("<Scale />")]
	[InlineData("<SkipTitle />")]
	[InlineData("<SkipTitle>yes</SkipTitle>")]
	[InlineData("<SkipTitle>True</SkipTitle>")] // xs:boolean is lower case
	[InlineData("<ItemBGColorOpacity>1.5</ItemBGColorOpacity>")]
	[InlineData("<ItemBGColorOpacity>99999999999</ItemBGColorOpacity>")]
	public void Read_InvalidValue_ThrowsInsteadOfReturningDefaults(string element)
	{
		// Returning the defaults would lose every setting at the next save.
		File.WriteAllText(this._xmlPath, $"""<UserSettings><VaultPath>D:\Vaults</VaultPath>{element}</UserSettings>""");

		var read = () => UserSettings.Read(this._xmlPath);

		read.Should().Throw<InvalidOperationException>();
	}

	[Theory]
	[InlineData("""<UserSettings><VaultPath>D:\Vaults</VaultPath><Scale>1</Sca""")] // cut short
	[InlineData("""<OtherRoot><VaultPath>D:\Vaults</VaultPath></OtherRoot>""")]
	public void Read_NotAUserConfig_Throws(string xml)
	{
		File.WriteAllText(this._xmlPath, xml);

		var read = () => UserSettings.Read(this._xmlPath);

		read.Should().Throw<InvalidOperationException>();
	}

	[Fact]
	public void Read_FileWithADtd_IsRejected()
	{
		// DTDs are prohibited: no entity expansion or external resolution.
		File.WriteAllText(this._xmlPath, """
			<?xml version="1.0"?>
			<!DOCTYPE UserSettings [<!ENTITY path "D:\Vaults">]>
			<UserSettings><VaultPath>&path;</VaultPath></UserSettings>
			""");

		var read = () => UserSettings.Read(this._xmlPath);

		read.Should().Throw<InvalidOperationException>().WithInnerException<System.Xml.XmlException>();
	}

	[Fact]
	public void Read_UnknownElementsAndOutOfRangeValues_AreAccepted()
	{
		// Values are not validated here: the UI clamps the zoom and the opacity.
		File.WriteAllText(this._xmlPath, """
			<UserSettings>
				<Renamed>1</Renamed>
				<Scale>-2</Scale>
				<ItemBGColorOpacity>500</ItemBGColorOpacity>
				<CSVDelimiter>Pipe</CSVDelimiter>
			</UserSettings>
			""");

		var settings = UserSettings.Read(this._xmlPath);

		settings.Scale.Should().Be(-2F);
		settings.ItemBGColorOpacity.Should().Be(500);
		settings.CSVDelimiter.Should().Be("Pipe");
	}

	#endregion
}
