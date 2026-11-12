using AwesomeAssertions;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class SettingsEditorTests
{
	private static UserSettings NewSettings()
		=> new()
		{
			VaultPath = """C:\Vaults""",
			AllowCheats = true,
			AllowCharacterEdit = true,
			AllowItemEdit = true,
			AllowItemCopy = true,
			EnableEpicLegendaryAffixes = true,
			GitBackupEnabled = true,
			GitBackupRepository = "https://github.com/user/vaults.git",
			DisableLegacyBackup = true,
			AutoDetectLanguage = false,
			AutoDetectGamePath = false,
			BaseFont = "AlbertusMT",
			CSVDelimiter = "Comma",
		};

	private static SettingsEditor NewEditor(UserSettings? settings = null)
		=> new(SettingsValues.From(settings ?? NewSettings(), "English", """C:\TQ""", """C:\TQIT"""));

	[Fact]
	public void NothingEdited_ReportsNoChangeAndNoRestart()
	{
		var editor = NewEditor();

		editor.ConfigurationChanged.Should().BeFalse();
		editor.RestartReason.Should().Be(RestartReason.None);
	}

	[Fact]
	public void SettingChangedThenChangedBack_IsNoChange()
	{
		var editor = NewEditor();

		editor.Current.EnableHotReload = true;
		editor.Current.EnableHotReload = false;

		editor.ConfigurationChanged.Should().BeFalse();
		editor.RestartReason.Should().Be(RestartReason.None);
	}

	[Fact]
	public void CheatsTurnedOff_ClearsCharacterEditItemEditItemCopyAndEpicAffixes()
	{
		var editor = NewEditor();

		editor.SetAllowCheats(false);

		editor.Current.AllowCharacterEdit.Should().BeFalse();
		editor.Current.AllowItemEdit.Should().BeFalse();
		editor.Current.AllowItemCopy.Should().BeFalse();
		editor.Current.EnableEpicLegendaryAffixes.Should().BeFalse();
		editor.EnableCharacterEditChanged.Should().BeTrue();
		editor.ConfigurationChanged.Should().BeTrue();
	}

	[Fact]
	public void CheatsTurnedOn_KeepsTheEditSettings()
	{
		var settings = NewSettings();
		settings.AllowCheats = false;
		settings.AllowItemEdit = false;
		var editor = NewEditor(settings);

		editor.SetAllowCheats(true);

		editor.Current.AllowCharacterEdit.Should().BeTrue();
		editor.Current.AllowItemCopy.Should().BeTrue();
		editor.Current.AllowItemEdit.Should().BeFalse();
	}

	[Fact]
	public void ItemEditTurnedOff_ClearsEpicAffixes()
	{
		var editor = NewEditor();

		editor.SetAllowItemEdit(false);

		editor.Current.EnableEpicLegendaryAffixes.Should().BeFalse();
	}

	[Fact]
	public void EpicAffixesWithoutItemEdit_AreSavedOff()
	{
		var settings = NewSettings();
		settings.AllowItemEdit = false;
		var editor = NewEditor(settings);

		editor.Current.SkipTitle = !settings.SkipTitle;
		editor.ApplyTo(settings);

		settings.EnableEpicLegendaryAffixes.Should().BeFalse();
	}

	[Fact]
	public void GitBackupTurnedOff_BringsBackLegacyBackupAndNeedsRestart()
	{
		var editor = NewEditor();

		editor.SetGitBackupEnabled(false);

		editor.Current.DisableLegacyBackup.Should().BeFalse();
		editor.RestartReason.Should().Be(RestartReason.Settings);
	}


	[Fact]
	public void LanguageChanged_NeedsRestartForLanguage()
	{
		var editor = NewEditor();

		editor.Current.TQLanguage = "French";

		editor.RestartReason.Should().Be(RestartReason.Language);
	}

	[Fact]
	public void LanguageChangedOnlyInCase_IsNoLanguageChange()
	{
		var editor = NewEditor();

		editor.Current.TQLanguage = "ENGLISH";

		editor.LanguageChanged.Should().BeFalse();
	}

	[Fact]
	public void LanguageDetectionTurnedOn_NeedsRestart_TurnedOffDoesNot()
	{
		var on = NewEditor();
		on.Current.AutoDetectLanguage = true;

		var settings = NewSettings();
		settings.AutoDetectLanguage = true;
		var off = NewEditor(settings);
		off.Current.AutoDetectLanguage = false;

		on.RestartReason.Should().Be(RestartReason.Language);
		off.RestartReason.Should().Be(RestartReason.None);
		off.ConfigurationChanged.Should().BeTrue();
	}

	[Fact]
	public void GamePathDetectionTurnedOn_NeedsRestartForGamePath()
	{
		var editor = NewEditor();

		editor.Current.AutoDetectGamePath = true;

		editor.RestartReason.Should().Be(RestartReason.GamePath);
	}

	[Fact]
	public void GamePathAndLanguageChanged_NeedRestartForSettings()
	{
		var editor = NewEditor();

		editor.SetTitanQuestPath("""D:\Games\TQ""");
		editor.Current.TQLanguage = "German";

		editor.RestartReason.Should().Be(RestartReason.Settings);
	}

	[Theory]
	[InlineData("""  C:\TQ  """, false)]
	[InlineData("""c:\tq""", false)]
	[InlineData("""D:\TQ""", true)]
	public void SetTitanQuestPath_TrimsAndIgnoresCase(string path, bool expected)
	{
		var editor = NewEditor();

		var changed = editor.SetTitanQuestPath(path);

		changed.Should().Be(expected);
		editor.GamePathChanged.Should().Be(expected);
		editor.Current.TQPath.Should().Be(expected ? path.Trim() : """C:\TQ""");
	}

	[Fact]
	public void SetVaultPath_NewPath_IsTrimmedAndReported()
	{
		var editor = NewEditor();

		var changed = editor.SetVaultPath("""  D:\Other Vaults """);

		changed.Should().BeTrue();
		editor.VaultPathChanged.Should().BeTrue();
		editor.Current.VaultPath.Should().Be("""D:\Other Vaults""");
	}

	[Fact]
	public void SetImmortalThronePath_NewPath_ChangesGamePath()
	{
		var editor = NewEditor();

		editor.SetImmortalThronePath("""D:\TQIT""").Should().BeTrue();

		editor.GamePathChanged.Should().BeTrue();
	}

	[Fact]
	public void FontChanged_AppliesWithoutRestart()
	{
		var editor = NewEditor();

		editor.Current.BaseFont = "Arial";

		editor.BaseFontChanged.Should().BeTrue();
		editor.ConfigurationChanged.Should().BeTrue();
		editor.UISettingChanged.Should().BeFalse();
		editor.RestartReason.Should().Be(RestartReason.None);
	}

	[Fact]
	public void TooltipSettingChanged_SavesWithoutRestart()
	{
		var editor = NewEditor();

		editor.Current.TooltipShowValueRanges = !editor.Current.TooltipShowValueRanges;

		editor.ConfigurationChanged.Should().BeTrue();
		editor.RestartReason.Should().Be(RestartReason.None);
	}

	[Fact]
	public void ChangeFlags_FollowTheirSetting()
	{
		var editor = NewEditor();

		editor.Current.ItemBGColorOpacity += 10;
		editor.Current.EnableTQVaultSounds = !editor.Current.EnableTQVaultSounds;

		editor.ItemBGColorOpacityChanged.Should().BeTrue();
		editor.EnableTQVaultSoundsChanged.Should().BeTrue();
		editor.EnableItemRequirementRestrictionChanged.Should().BeFalse();
		editor.EnableCharacterEditChanged.Should().BeFalse();
		editor.VaultPathChanged.Should().BeFalse();
	}

	[Theory]
	[InlineData(true, "not a url", false)]
	[InlineData(true, "https://github.com/user/vaults.git", true)]
	[InlineData(false, "not a url", true)]
	public void IsGitRepositoryValid_ChecksTheUrlOnlyWhenGitBackupIsOn(bool gitEnabled, string url, bool expected)
	{
		var editor = NewEditor();

		editor.SetGitBackupEnabled(gitEnabled);
		editor.Current.GitBackupRepository = url;

		editor.IsGitRepositoryValid.Should().Be(expected);
	}

	[Fact]
	public void ApplyTo_WritesTheEditedValues()
	{
		var settings = NewSettings();
		var editor = NewEditor(settings);

		editor.SetVaultPath("""D:\Vaults""");
		editor.SetAllowCheats(false);
		editor.Current.TQLanguage = "French";
		editor.Current.PasteBinExpiration = "1W";
		editor.Current.CSVDelimiter = "Semicolon";
		editor.ApplyTo(settings);

		settings.VaultPath.Should().Be("""D:\Vaults""");
		settings.AllowCheats.Should().BeFalse();
		settings.AllowItemEdit.Should().BeFalse();
		settings.TQLanguage.Should().Be("French");
		settings.TQPath.Should().Be("""C:\TQ""");
		settings.PasteBinExpiration.Should().Be("1W");
		settings.CSVDelimiter.Should().Be("Semicolon");
	}

	[Fact]
	public void Original_IsNotChangedByTheEdits()
	{
		var editor = NewEditor();

		editor.SetAllowCheats(false);

		editor.Original.AllowItemEdit.Should().BeTrue();
	}
}
