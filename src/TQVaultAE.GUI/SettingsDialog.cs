//-----------------------------------------------------------------------
// <copyright file="SettingsDialog.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using EnumsNET;
using TQVaultAE.Domain.Results;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Helpers;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;
using System.IO;
using log4net.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.Logging;

namespace TQVaultAE.GUI;


/// <summary>
/// Class for the Settings/Configuration Dialog
/// </summary>
internal partial class SettingsDialog : VaultForm, IScalingControl
{
	#region Properties & Fields

	public string BaseFont { get; private set; }

	/// <summary>
	/// Indicates whether the last opened vault will be loaded at startup
	/// </summary>
	private bool loadLastVault;

	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.EnableTQVaultSounds"/> setting has been changed
	/// </summary>
	public bool enableTQVaultSounds;

	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.EnableItemRequirementRestriction"/> setting has been changed
	/// </summary>
	public bool enableItemRequirementRestriction;

	/// <summary>
	/// Indicates whether the title screen will be skipped on startup
	/// </summary>
	private bool skipTitle;

	/// <summary>
	/// Save path for the vault files
	/// </summary>
	public string VaultPath { get; private set; }

	/// <summary>
	/// Indicates whether item copying is allowed
	/// </summary>
	private bool allowItemCopy;

	/// <summary>
	/// Indicates whether item editing is allowed
	/// </summary>
	private bool allowItemEdit;

	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.EnableEpicLegendaryAffixes"/> setting has been changed
	/// </summary>
	public bool EnableEpicLegendaryAffixes;

	/// <summary>
	/// Indicates whether character editing is allowed
	/// </summary>
	private bool allowCharacterEdit;

	/// <summary>
	/// Indicates whether the last opened character will be loaded at startup
	/// </summary>
	private bool loadLastCharacter;

	/// <summary>
	/// Indicates whether the language will be auto detected
	/// </summary>
	private bool detectLanguage;

	/// <summary>
	/// Activate the alternative Tooltip display
	/// </summary>
	private bool enableDetailedTooltipView;

	/// <summary>
	/// Value Range (0-255) for item background color opacity
	/// </summary>
	private int itemBGColorOpacity;

	/// <summary>
	/// The language we will be using.
	/// </summary>
	private string titanQuestLanguage;

	/// <summary>
	/// Indicates whether the game paths will be auto detected
	/// </summary>
	private bool detectGamePath;

	/// <summary>
	/// Titan Quest game path
	/// </summary>
	private string titanQuestPath;

	/// <summary>
	/// Immortal Throne game path
	/// </summary>
	private string immortalThronePath;

	/// <summary>
	/// Indicates whether custom maps have been enabled
	/// </summary>
	private bool enableMods;

	/// <summary>
	/// Current custom map
	/// </summary>
	private string customMap;

	/// <summary>
	/// Indicates whether we are loading all data files on startup
	/// </summary>
	private bool loadAllFiles;

	/// <summary>
	/// Indicates whether warning messages are suppressed
	/// </summary>
	private bool suppressWarnings;

	/// <summary>
	/// Indicates whether player items will be readonly.
	/// </summary>
	private bool playerReadonly;

	/// <summary>
	/// Indicates whether the vault path has been changed
	/// </summary>
	public bool VaultPathChanged { get; private set; }

	/// <summary>
	/// Indicates whether the play list filter has been changed
	/// </summary>
	public bool PlayerFilterChanged { get; private set; }

	/// <summary>
	/// Indicates that any configuration item has been changed
	/// </summary>
	public bool ConfigurationChanged { get; private set; }

	/// <summary>
	/// Indicates that the UI setting has changed.
	/// </summary>
	public bool UISettingChanged { get; private set; }

	/// <summary>
	/// Indicates that the settings have been loaded
	/// </summary>
	private bool settingsLoaded;

	/// <summary>
	/// Indicates that the ItemBGColorOpacity setting has been changed
	/// </summary>
	public bool ItemBGColorOpacityChanged { get; private set; }

	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.EnableItemRequirementRestriction"/> setting has been changed
	/// </summary>
	public bool EnableItemRequirementRestrictionChanged { get; private set; }

	/// <summary>
	/// Indicates that the allow character edit setting has been changed.
	/// </summary>
	public bool EnableCharacterEditChanged { get; private set; }

	/// <summary>
	/// Enale the hot reload feature
	/// </summary>
	private bool enableHotReload;

	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.EnableHotReload"/> setting has been changed
	/// </summary>
	public bool EnableHotReloadChanged { get; private set; }


	/// <summary>
	/// Indicates that the language setting has been changed
	/// </summary>
	public bool LanguageChanged { get; private set; }

	/// <summary>
	/// Indicates that the game language has been changed
	/// </summary>
	public bool GamePathChanged { get; private set; }

	/// <summary>
	/// Indicates that the custom map selection has changed
	/// </summary>
	public bool CustomMapsChanged { get; private set; }

	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.EnableTQVaultSounds"/> setting has been changed
	/// </summary>
	public bool EnableTQVaultSoundsChanged { get; private set; }

	/// <summary>
	/// Disable auto stacking last value
	/// </summary>
	private bool disableAutoStacking;

	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.DisableAutoStacking"/> setting has been changed
	/// </summary>
	public bool DisableAutoStackingChanged { get; private set; }

	/// <summary>
	/// AllowCheats last value
	/// </summary>
	private bool allowCheats;
	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.AllowCheats"/> setting has been changed
	/// </summary>
	public bool AllowCheatsChanged { get; private set; }

	/// <summary>
	/// enableGitBackup last value
	/// </summary>
	private bool enableGitBackup;
	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.GitBackupEnabled"/> setting has been changed
	/// </summary>
	public bool GitBackupEnabledChanged { get; private set; }

	/// <summary>
	/// gitBackupRepository last value
	/// </summary>
	private string gitBackupRepository;
	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.GitBackupRepository"/> setting has been changed
	/// </summary>
	public bool GitBackupRepositoryChanged { get; private set; }

	/// <summary>
	/// disableLegacyBackup last value
	/// </summary>
	private bool disableLegacyBackup;
	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.DisableLegacyBackup"/> setting has been changed
	/// </summary>
	public bool DisableLegacyBackupChanged { get; private set; }

	/// <summary>
	/// enableBackupPlayerSaves last value
	/// </summary>
	private bool enableBackupPlayerSaves;
	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.GitBackupPlayerSavesEnabled"/> setting has been changed
	/// </summary>
	public bool EnableBackupPlayerSavesChanged { get; private set; }

	private readonly ILogger<SettingsDialog> Log;

	#endregion

	/// <summary>
	/// Initializes a new instance of the SettingsDialog class.
	/// </summary>
	public SettingsDialog(MainForm instance) : base(instance.ServiceProvider)
	{
		this.Owner = instance;

		this.Log = instance.ServiceProvider.GetService<ILogger<SettingsDialog>>();

		this.InitializeComponent();

		this.SuspendLayout();

		//this.ShowResizeBorders = false;
		//this.NormalizeBox = false;
		//this.DrawCustomBorder = true;
		//this.ResizeCustomAllowed = false;
		//this.ScaleOnResize = false;

		#region Apply custom font

		var font12 = FontService.GetFontLight(12F);
		var font1125 = FontService.GetFontLight(11.25F);

		this.cancelButton.Font =
		this.okayButton.Font =
		this.resetButton.Font =
		this.vaultPathBrowseButton.Font =
		this.titanQuestPathBrowseButton.Font =
		this.immortalThronePathBrowseButton.Font = font12;


		this.hotReloadCheckBox.Font =
		this.scalingCheckBoxEnableSounds.Font =
		this.characterEditCheckBox.Font =
		this.allowItemEditCheckBox.Font =
		this.allowItemCopyCheckBox.Font =
		this.skipTitleCheckBox.Font =
		this.loadLastCharacterCheckBox.Font =
		this.loadLastVaultCheckBox.Font =
		this.vaultPathTextBox.Font =
		this.vaultPathLabel.Font =
		this.enableCustomMapsCheckBox.Font =
		this.loadAllFilesCheckBox.Font =
		this.suppressWarningsCheckBox.Font =
		this.playerReadonlyCheckbox.Font =
		this.languageComboBox.Font =
		this.languageLabel.Font =
		this.detectLanguageCheckBox.Font =
		this.titanQuestPathTextBox.Font =
		this.titanQuestPathLabel.Font =
		this.immortalThronePathLabel.Font =
		this.immortalThronePathTextBox.Font =
		this.detectGamePathsCheckBox.Font =
		this.customMapLabel.Font =
		this.mapListComboBox.Font =
		this.baseFontLabel.Font =
		this.baseFontComboBox.Font =
		this.EnableDetailedTooltipViewCheckBox.Font =
		this.EnableItemRequirementRestrictionCheckBox.Font =
		this.ItemBGColorOpacityLabel.Font =
		this.scalingLabelCSVDelim.Font =
		this.scalingComboBoxCSVDelim.Font =
		this.scalingCheckBoxEnableEpicLegendaryAffixes.Font =
		this.scalingCheckBoxDisableAutoStacking.Font = font1125;

		this.checkGroupBoxGitBackup.ProcessAllControls((ctr) => ctr.Font = font1125);

		this.Font = font1125;

		#endregion

		this.vaultPathLabel.Text = Resources.SettingsLabel1;
		this.languageLabel.Text = Resources.SettingsLabel2;
		this.titanQuestPathLabel.Text = Resources.SettingsLabel3;
		this.immortalThronePathLabel.Text = Resources.SettingsLabel4;
		this.customMapLabel.Text = Resources.SettingsLabel5;
		this.detectGamePathsCheckBox.Text = Resources.SettingsDetectGamePath;
		this.detectLanguageCheckBox.Text = Resources.SettingsDetectLanguage;
		this.enableCustomMapsCheckBox.Text = Resources.SettingsEnableMod;
		this.toolTip.SetToolTip(this.enableCustomMapsCheckBox, Resources.SettingsEnableModTT);
		this.skipTitleCheckBox.Text = Resources.SettingsSkipTitle;
		this.toolTip.SetToolTip(this.skipTitleCheckBox, Resources.SettingsSkipTitleTT);
		this.allowItemCopyCheckBox.Text = Resources.SettingsAllowCopy;
		this.toolTip.SetToolTip(this.allowItemCopyCheckBox, Resources.SettingsAllowCopyTT);
		this.allowItemEditCheckBox.Text = Resources.SettingsAllowEdit;
		this.toolTip.SetToolTip(this.allowItemEditCheckBox, Resources.SettingsAllowEdit);
		this.characterEditCheckBox.Text = Resources.SettingsAllowEditCE;
		this.toolTip.SetToolTip(this.characterEditCheckBox, Resources.SettingsAllowEditCE);
		this.loadLastCharacterCheckBox.Text = Resources.SettingsLoadChar;
		this.toolTip.SetToolTip(this.loadLastCharacterCheckBox, Resources.SettingsLoadCharTT);
		this.loadLastVaultCheckBox.Text = Resources.SettingsLoadVault;
		this.toolTip.SetToolTip(this.loadLastVaultCheckBox, Resources.SettingsLoadVaultTT);
		this.loadAllFilesCheckBox.Text = Resources.SettingsPreLoad;
		this.toolTip.SetToolTip(this.loadAllFilesCheckBox, Resources.SettingsPreLoadTT);
		this.suppressWarningsCheckBox.Text = Resources.SettingsNoWarning;
		this.toolTip.SetToolTip(this.suppressWarningsCheckBox, Resources.SettingsNoWarningTT);
		this.playerReadonlyCheckbox.Text = Resources.SettingsPlayerReadonly;
		this.toolTip.SetToolTip(this.playerReadonlyCheckbox, Resources.SettingsPlayerReadonlyTT);
		this.resetButton.Text = Resources.SettingsReset;
		this.toolTip.SetToolTip(this.resetButton, Resources.SettingsResetTT);
		this.EnableDetailedTooltipViewCheckBox.Text = Resources.SettingEnableDetailedTooltipView;
		this.toolTip.SetToolTip(this.EnableDetailedTooltipViewCheckBox, Resources.SettingEnableDetailedTooltipViewTT);
		this.ItemBGColorOpacityLabel.Text = Resources.SettingsItemBGColorOpacityLabel;
		this.toolTip.SetToolTip(this.ItemBGColorOpacityLabel, Resources.SettingsItemBGColorOpacityLabelTT);
		this.EnableItemRequirementRestrictionCheckBox.Text = Resources.SettingsEnableItemRequirementRestriction;
		this.toolTip.SetToolTip(this.EnableItemRequirementRestrictionCheckBox, Resources.SettingsEnableItemRequirementRestrictionTT);
		this.hotReloadCheckBox.Text = Resources.SettingsEnableHotReload;
		this.toolTip.SetToolTip(this.hotReloadCheckBox, Resources.SettingsEnableHotReloadTT);

		this.scalingLabelCSVDelim.Text = Resources.SettingsCsvDelimiterLabel;

		this.scalingCheckBoxEnableEpicLegendaryAffixes.Text = Resources.SettingsEnableEpicLegendaryAffixes;
		this.toolTip.SetToolTip(this.scalingCheckBoxEnableEpicLegendaryAffixes, Resources.SettingsEnableEpicLegendaryAffixesTT);

		this.scalingCheckBoxDisableAutoStacking.Text = Resources.SettingsDisableAutoStacking;
		this.toolTip.SetToolTip(this.scalingCheckBoxDisableAutoStacking, Resources.SettingsDisableAutoStackingTT);

		this.checkGroupBoxAllowCheats.Text = Resources.SettingsAllowCheats;
		this.scalingCheckBoxDisableLegacyBackup.Text = Resources.SettingsDisableLegacyBackup;
		this.toolTip.SetToolTip(this.scalingCheckBoxDisableLegacyBackup, Resources.SettingsDisableLegacyBackupTT);
		this.scalingLabelGitRepository.Text = Resources.SettingsGitRepositoryUrl;
		this.scalingCheckBoxBackupPlayerSaves.Text = Resources.SettingsBackupPlayerSaves;
		this.toolTip.SetToolTip(this.scalingCheckBoxBackupPlayerSaves, Resources.SettingsBackupPlayerSavesTT);

		this.cancelButton.Text = Resources.GlobalCancel;
		this.okayButton.Text = Resources.GlobalOK;
		this.Text = Resources.SettingsTitle;

		this.mapListComboBox.Items.Clear();

		var maps = GamePathResolver.GetCustomMapList();

		if (maps?.Any() ?? false)
			this.mapListComboBox.Items.AddRange(maps);

		this.ResumeLayout(false);
		this.PerformLayout();
	}

	/// <summary>
	/// Override of ScaleControl which supports font scaling.
	/// </summary>
	/// <param name="factor">SizeF for the scale factor</param>
	/// <param name="specified">BoundsSpecified value.</param>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		this.Font = new Font(this.Font.FontFamily, this.Font.SizeInPoints * factor.Height, this.Font.Style);
		base.ScaleControl(factor, specified);
	}

	/// <summary>
	/// Handler for clicking the vault path browse button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void VaultPathBrowseButtonClick(object sender, EventArgs e)
	{
		this.folderBrowserDialog.Description = Resources.SettingsBrowseVault;
		this.folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
		this.folderBrowserDialog.SelectedPath = this.VaultPath;
		this.folderBrowserDialog.ShowDialog();
		if (this.folderBrowserDialog.SelectedPath.Trim().ToUpperInvariant() != this.VaultPath.Trim().ToUpperInvariant())
		{
			this.VaultPath = this.folderBrowserDialog.SelectedPath.Trim().ToUpperInvariant();
			this.vaultPathTextBox.Text = this.VaultPath.Trim().ToUpperInvariant();
			this.vaultPathTextBox.Invalidate();
			this.ConfigurationChanged = true;
			this.VaultPathChanged = true;
		}
	}

	/// <summary>
	/// Handler for clicking the reset button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ResetButtonClick(object sender, EventArgs e)
	{
		if (this.ConfigurationChanged)
		{
			this.LoadSettings();
			this.UpdateDialogSettings();
			this.Invalidate();
		}
	}

	/// <summary>
	/// Handler for clicking the skip title check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void SkipTitleCheckBoxCheckedChanged(object sender, EventArgs e)
	{
		if (this.skipTitleCheckBox.Checked)
		{
			if (!this.skipTitle)
			{
				this.ConfigurationChanged = this.skipTitle = true;
			}
		}
		else
		{
			if (this.skipTitle)
			{
				this.skipTitle = false;
				this.ConfigurationChanged = true;
			}
		}
	}

	/// <summary>
	/// Handler for loading this dialog box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void SettingsDialogLoad(object sender, EventArgs e)
	{
		this.settingsLoaded = false;

		this.LoadSettings();
		this.UpdateDialogSettings();
	}



	/// <summary>
	/// Loads the settings from the config file
	/// </summary>
	private void LoadSettings()
	{
		this.BaseFont = Config.UserSettings.Default.BaseFont;

		this.skipTitle = Config.UserSettings.Default.SkipTitle;
		this.VaultPath = Config.UserSettings.Default.VaultPath;
		this.allowItemCopy = Config.UserSettings.Default.AllowItemCopy;
		this.allowItemEdit = Config.UserSettings.Default.AllowItemEdit;
		this.allowCharacterEdit = Config.UserSettings.Default.AllowCharacterEdit;
		this.loadLastCharacter = Config.UserSettings.Default.LoadLastCharacter;
		this.loadLastVault = Config.UserSettings.Default.LoadLastVault;
		this.detectLanguage = Config.UserSettings.Default.AutoDetectLanguage;
		this.enableDetailedTooltipView = Config.UserSettings.Default.EnableDetailedTooltipView;
		this.itemBGColorOpacity = Config.UserSettings.Default.ItemBGColorOpacity;
		this.enableItemRequirementRestriction = Config.UserSettings.Default.EnableItemRequirementRestriction;
		this.enableHotReload = Config.UserSettings.Default.EnableHotReload;
		this.enableTQVaultSounds = Config.UserSettings.Default.EnableTQVaultSounds;
		this.EnableEpicLegendaryAffixes = Config.UserSettings.Default.EnableEpicLegendaryAffixes;
		this.disableAutoStacking = Config.UserSettings.Default.DisableAutoStacking;
		this.allowCheats = Config.UserSettings.Default.AllowCheats;
		this.enableGitBackup = Config.UserSettings.Default.GitBackupEnabled;
		this.disableLegacyBackup = Config.UserSettings.Default.DisableLegacyBackup;
		this.gitBackupRepository = Config.UserSettings.Default.GitBackupRepository;
		this.enableBackupPlayerSaves = Config.UserSettings.Default.GitBackupPlayerSavesEnabled;

		// Force English since there was some issue with getting the proper language setting.
		var gl = Database.GameLanguage;
		this.titanQuestLanguage = gl == null ? "English" : gl;

		this.detectGamePath = Config.UserSettings.Default.AutoDetectGamePath;
		this.titanQuestPath = GamePathResolver.GamePathTQ;
		this.immortalThronePath = GamePathResolver.GamePathTQIT;
		this.enableMods = Config.UserSettings.Default.ModEnabled;
		this.customMap = Config.UserSettings.Default.CustomMap;
		this.loadAllFiles = Config.UserSettings.Default.LoadAllFiles;
		this.suppressWarnings = Config.UserSettings.Default.SuppressWarnings;
		this.playerReadonly = Config.UserSettings.Default.PlayerReadonly;

		this.settingsLoaded = true;
		this.ConfigurationChanged = false;
		this.VaultPathChanged = false;
		this.PlayerFilterChanged = false;
		this.LanguageChanged = false;
		this.GamePathChanged = false;
		this.UISettingChanged = false;
	}

	/// <summary>
	/// Updates the dialog settings display
	/// </summary>
	private void UpdateDialogSettings()
	{
		// Check to see that we can update things
		if (!this.settingsLoaded)
			return;

		this.SuspendLayout();

		// Build language combo box
		this.languageComboBox.Items.Clear();

		// Read the languages from the config file
		ComboBoxItem[] languages = Config.Settings.Default.GameLanguages.Split(',').Select(iso =>
		{
			CultureInfo ci = new CultureInfo(iso.ToUpperInvariant(), true);
			return new ComboBoxItem() { Value = ci.EnglishName, DisplayName = ci.DisplayName };// to keep EnglishName as baseline value
		}).OrderBy(cb => cb.DisplayName).ToArray();

		// Reading failed so we default to English
		if (!languages.Any()) languages = new ComboBoxItem[] { new ComboBoxItem() { Value = "English", DisplayName = "English" } };

		this.languageComboBox.Items.AddRange(languages);

		this.vaultPathTextBox.Text = this.VaultPath;
		this.skipTitleCheckBox.Checked = this.skipTitle;
		this.allowItemEditCheckBox.Checked = this.allowItemEdit;
		this.allowItemCopyCheckBox.Checked = this.allowItemCopy;
		this.characterEditCheckBox.Checked = this.allowCharacterEdit;
		this.loadLastCharacterCheckBox.Checked = this.loadLastCharacter;
		this.loadLastVaultCheckBox.Checked = this.loadLastVault;
		this.detectLanguageCheckBox.Checked = this.detectLanguage;
		this.detectGamePathsCheckBox.Checked = this.detectGamePath;
		this.immortalThronePathTextBox.Text = this.immortalThronePath;
		this.immortalThronePathTextBox.Enabled = !this.detectGamePath;
		this.immortalThronePathBrowseButton.Enabled = !this.detectGamePath;
		this.titanQuestPathTextBox.Text = this.titanQuestPath;
		this.titanQuestPathTextBox.Enabled = !this.detectGamePath;
		this.titanQuestPathBrowseButton.Enabled = !this.detectGamePath;
		this.loadAllFilesCheckBox.Checked = this.loadAllFiles;
		this.suppressWarningsCheckBox.Checked = this.suppressWarnings;
		this.playerReadonlyCheckbox.Checked = this.playerReadonly;
		this.EnableDetailedTooltipViewCheckBox.Checked = this.enableDetailedTooltipView;
		this.ItemBGColorOpacityTrackBar.Value = this.itemBGColorOpacity;
		this.EnableItemRequirementRestrictionCheckBox.Checked = this.enableItemRequirementRestriction;
		this.hotReloadCheckBox.Checked = this.enableHotReload;
		this.scalingCheckBoxEnableSounds.Checked = this.enableTQVaultSounds;
		this.scalingCheckBoxEnableEpicLegendaryAffixes.Checked = this.EnableEpicLegendaryAffixes;
		this.scalingCheckBoxDisableAutoStacking.Checked = this.disableAutoStacking;
		this.checkGroupBoxAllowCheats.Checked = this.allowCheats;
		this.checkGroupBoxGitBackup.Checked = this.enableGitBackup;
		this.scalingCheckBoxDisableLegacyBackup.Checked = this.disableLegacyBackup;
		this.scalingTextBoxGitRepository.Text = this.gitBackupRepository;
		this.scalingCheckBoxBackupPlayerSaves.Checked = this.enableBackupPlayerSaves;


		this.enableCustomMapsCheckBox.Checked = this.enableMods;

		var lst = this.mapListComboBox.Items.Cast<GamePathEntry>();
		var found = lst.Where(m => m.Path == this.customMap).FirstOrDefault();
		this.mapListComboBox.SelectedItem = found;

		this.mapListComboBox.Enabled = this.enableMods;

		this.languageComboBox.SelectedItem = languages.FirstOrDefault(cb => cb.Value.Equals(this.titanQuestLanguage, StringComparison.InvariantCultureIgnoreCase));

		this.languageComboBox.Enabled = !this.detectLanguage;

		// Build Font combo box
		this.baseFontComboBox.Items.Clear();
		var listItem = Enums.GetMembers<FontFamilyList>()
			.Select(m => new ComboBoxItem()
			{
				Value = m.AsString(EnumFormat.Name),
				DisplayName = m.AsString(EnumFormat.Description, EnumFormat.Name)
			}).ToArray();
		this.baseFontComboBox.Items.AddRange(listItem);
		this.baseFontComboBox.SelectedItem = listItem.Where(i => i.Value == this.BaseFont).FirstOrDefault() ?? listItem.First();

		// Build CsvDelim combo box
		this.scalingComboBoxCSVDelim.Items.Clear();
		var listItemDelim = Enums.GetMembers<CsvDelimiter>()
			.Select(m =>
			{
				var name = m.AsString(EnumFormat.Name);
				var desc = m.AsString(EnumFormat.Description);
				return new ComboBoxItem<CsvDelimiter, char>()
				{
					ComboValue = m.Value,
					Value = desc.First(),
					DisplayName = $"{name} ({desc})"
				};
			}).ToArray();
		this.scalingComboBoxCSVDelim.Items.AddRange(listItemDelim);
		this.scalingComboBoxCSVDelim.SelectedItem = listItemDelim
			.Where(i => i.ComboValue.ToString() == Config.UserSettings.Default.CSVDelimiter)
			.FirstOrDefault() ?? listItemDelim.First();

		this.ResumeLayout(false);
		this.PerformLayout();
	}

	const string gitUrlRegex = @"((git|ssh|http(s)?)|(git@[\w\.]+))(:(//)?)([\w\.@\:/\-~]+)(\.git)(/)?";

	/// <summary>
	/// Handler for clicking the OK button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void OkayButtonClick(object sender, EventArgs e)
	{
		// Validation
		if (enableGitBackup)
		{
			if (string.IsNullOrEmpty(gitBackupRepository) || !Regex.IsMatch(gitBackupRepository, gitUrlRegex))
			{
				this.UIService.ShowError("You must enter a valid Git Url.");
				return;
			}
		}

		if (this.ConfigurationChanged)
		{
			Config.UserSettings.Default.SkipTitle = this.skipTitle;
			Config.UserSettings.Default.VaultPath = this.VaultPath;
			Config.UserSettings.Default.AllowItemCopy = this.allowItemCopy;
			Config.UserSettings.Default.AllowItemEdit = this.allowItemEdit;
			Config.UserSettings.Default.AllowCharacterEdit = this.allowCharacterEdit;
			Config.UserSettings.Default.LoadLastCharacter = this.loadLastCharacter;
			Config.UserSettings.Default.LoadLastVault = this.loadLastVault;
			Config.UserSettings.Default.AutoDetectLanguage = this.detectLanguage;
			Config.UserSettings.Default.TQLanguage = this.titanQuestLanguage;
			Config.UserSettings.Default.AutoDetectGamePath = this.detectGamePath;
			Config.UserSettings.Default.TQITPath = this.immortalThronePath;
			Config.UserSettings.Default.TQPath = this.titanQuestPath;
			Config.UserSettings.Default.ModEnabled = this.enableMods;
			Config.UserSettings.Default.CustomMap = this.customMap;
			Config.UserSettings.Default.LoadAllFiles = this.loadAllFiles;
			Config.UserSettings.Default.SuppressWarnings = this.suppressWarnings;
			Config.UserSettings.Default.PlayerReadonly = this.playerReadonly;
			Config.UserSettings.Default.BaseFont = this.BaseFont;
			Config.UserSettings.Default.EnableDetailedTooltipView = this.enableDetailedTooltipView;
			Config.UserSettings.Default.ItemBGColorOpacity = this.itemBGColorOpacity;
			Config.UserSettings.Default.EnableItemRequirementRestriction = this.enableItemRequirementRestriction;
			Config.UserSettings.Default.EnableHotReload = this.enableHotReload;
			Config.UserSettings.Default.EnableTQVaultSounds = this.enableTQVaultSounds;
			Config.UserSettings.Default.DisableAutoStacking = this.disableAutoStacking;
			Config.UserSettings.Default.AllowCheats = this.allowCheats;
			Config.UserSettings.Default.GitBackupEnabled = this.enableGitBackup;
			Config.UserSettings.Default.DisableLegacyBackup = this.disableLegacyBackup;
			Config.UserSettings.Default.GitBackupRepository = this.gitBackupRepository;
			Config.UserSettings.Default.GitBackupPlayerSavesEnabled = this.enableBackupPlayerSaves;

			Config.UserSettings.Default.EnableEpicLegendaryAffixes =
				this.scalingCheckBoxEnableEpicLegendaryAffixes.Enabled && this.scalingCheckBoxEnableEpicLegendaryAffixes.Checked;

			var delim = (ComboBoxItem<CsvDelimiter, char>)this.scalingComboBoxCSVDelim.SelectedItem;
			Config.UserSettings.Default.CSVDelimiter = delim.ComboValue.ToString();
		}
	}

	/// <summary>
	/// Handler for clicking the allow item editing check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void AllowItemEditCheckBoxCheckedChanged(object sender, EventArgs e)
	{
		if (this.allowItemEditCheckBox.Checked)
		{
			if (!this.allowItemEdit)
			{
				this.ConfigurationChanged = this.allowItemEdit = true;
			}

			this.scalingCheckBoxEnableEpicLegendaryAffixes.Enabled = true;
		}
		else
		{
			if (this.allowItemEdit)
			{
				this.allowItemEdit = false;
				this.ConfigurationChanged = true;
			}

			this.scalingCheckBoxEnableEpicLegendaryAffixes.Checked =
			this.scalingCheckBoxEnableEpicLegendaryAffixes.Enabled = false;
		}

	}

	/// <summary>
	/// Handler for clicking the allow item copy check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void AllowItemCopyCheckBoxCheckedChanged(object sender, EventArgs e)
	{
		if (this.allowItemCopyCheckBox.Checked)
		{
			if (!this.allowItemCopy)
			{
				this.ConfigurationChanged = this.allowItemCopy = true;
			}
		}
		else
		{
			if (this.allowItemCopy)
			{
				this.allowItemCopy = false;
				this.ConfigurationChanged = true;
			}
		}
	}

	/// <summary>
	/// Handler for clicking the load last character check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void LoadLastCharacterCheckBoxCheckedChanged(object sender, EventArgs e)
	{
		if (this.loadLastCharacterCheckBox.Checked)
		{
			if (!this.loadLastCharacter)
			{
				this.ConfigurationChanged = this.loadLastCharacter = true;
			}
		}
		else
		{
			if (this.loadLastCharacter)
			{
				this.loadLastCharacter = false;
				this.ConfigurationChanged = true;
			}
		}
	}

	/// <summary>
	/// Handler for clicking the load last vault check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void LoadLastVaultCheckBoxCheckedChanged(object sender, EventArgs e)
	{
		if (this.loadLastVaultCheckBox.Checked)
		{
			if (!this.loadLastVault)
			{
				this.ConfigurationChanged = this.loadLastVault = true;
			}
		}
		else
		{
			if (this.loadLastVault)
			{
				this.loadLastVault = false;
				this.ConfigurationChanged = true;
			}
		}
	}

	/// <summary>
	/// Handler for clicking the detect language check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void DetectLanguageCheckBoxCheckedChanged(object sender, EventArgs e)
	{
		if (this.detectLanguageCheckBox.Checked)
		{
			if (!this.detectLanguage)
			{
				this.languageComboBox.Enabled = false;
				this.ConfigurationChanged = this.detectLanguage = true;

				// Force TQVault to restart to autodetect the language
				this.LanguageChanged = true;
			}
		}
		else
		{
			if (this.detectLanguage)
			{
				this.detectLanguage = false;
				this.ConfigurationChanged = this.languageComboBox.Enabled = true;
			}
		}
	}

	/// <summary>
	/// Handler for clicking the detect game paths check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void DetectGamePathsCheckBoxCheckedChanged(object sender, EventArgs e)
	{
		if (this.detectGamePathsCheckBox.Checked)
		{
			if (!this.detectGamePath)
			{
				this.titanQuestPathTextBox.Enabled = this.immortalThronePathTextBox.Enabled
					= this.titanQuestPathBrowseButton.Enabled = this.immortalThronePathBrowseButton.Enabled = false;

				// Force TQVault to restart to autodetect the game path
				this.GamePathChanged = this.ConfigurationChanged = this.detectGamePath = true;
			}
		}
		else
		{
			if (this.detectGamePath)
			{
				this.detectGamePath = false;
				this.ConfigurationChanged = this.immortalThronePathTextBox.Enabled = this.titanQuestPathTextBox.Enabled
					= this.titanQuestPathBrowseButton.Enabled = this.immortalThronePathBrowseButton.Enabled = true;
			}
		}
	}

	/// <summary>
	/// Handler for changing the language selection
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void LanguageComboBoxSelectedIndexChanged(object sender, EventArgs e)
	{
		// There was some problem getting the game language so we ignore changing it.
		var gl = Database.GameLanguage;
		if (gl == null)
			return;

		this.titanQuestLanguage = ((ComboBoxItem)this.languageComboBox.SelectedItem).Value;
		if (this.titanQuestLanguage.ToUpperInvariant() != gl.ToUpperInvariant())
			this.LanguageChanged = true;

		this.ConfigurationChanged = true;
	}

	/// <summary>
	/// Handler for leaving the vault text box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void VaultPathTextBoxLeave(object sender, EventArgs e)
	{
		if (this.VaultPath.ToUpperInvariant() != this.vaultPathTextBox.Text.Trim().ToUpperInvariant())
		{
			this.VaultPath = this.vaultPathTextBox.Text.Trim();
			this.vaultPathTextBox.Invalidate();
			this.ConfigurationChanged = this.VaultPathChanged = true;
		}
	}

	/// <summary>
	/// Handler for leaving the Titan Quest game path text box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void TitanQuestPathTextBoxLeave(object sender, EventArgs e)
	{
		if (this.titanQuestPath.ToUpperInvariant() != this.titanQuestPathTextBox.Text.Trim().ToUpperInvariant())
		{
			this.titanQuestPath = this.titanQuestPathTextBox.Text.Trim();
			this.titanQuestPathTextBox.Invalidate();
			this.ConfigurationChanged = this.GamePathChanged = true;
		}
	}

	/// <summary>
	/// Handler for leaving the Immortal Throne game path text box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ImmortalThronePathTextBoxLeave(object sender, EventArgs e)
	{
		if (this.immortalThronePath.ToUpperInvariant() != this.immortalThronePathTextBox.Text.Trim().ToUpperInvariant())
		{
			this.immortalThronePath = this.immortalThronePathTextBox.Text.Trim();
			this.immortalThronePathTextBox.Invalidate();
			this.ConfigurationChanged = this.GamePathChanged = true;
		}
	}

	/// <summary>
	/// Handler for clicking the Titan Quest game path browse button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void TitanQuestPathBrowseButtonClick(object sender, EventArgs e)
	{
		this.folderBrowserDialog.Description = Resources.SettingsBrowseTQ;
		this.folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
		this.folderBrowserDialog.SelectedPath = this.titanQuestPath;
		this.folderBrowserDialog.ShowDialog();
		if (this.folderBrowserDialog.SelectedPath.Trim().ToUpperInvariant() != this.titanQuestPath.Trim().ToUpperInvariant())
		{
			this.titanQuestPath = this.folderBrowserDialog.SelectedPath.Trim().ToUpperInvariant();
			this.titanQuestPathTextBox.Text = this.titanQuestPath.Trim().ToUpperInvariant();
			this.titanQuestPathTextBox.Invalidate();
			this.ConfigurationChanged = this.GamePathChanged = true;
		}
	}

	/// <summary>
	/// Handler for clicking the Immortal Throne game path browse button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ImmortalThronePathBrowseButtonClick(object sender, EventArgs e)
	{
		this.folderBrowserDialog.Description = Resources.SettingsBrowseIT;
		this.folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
		this.folderBrowserDialog.SelectedPath = this.immortalThronePath;
		this.folderBrowserDialog.ShowDialog();
		if (this.folderBrowserDialog.SelectedPath.Trim().ToUpperInvariant() != this.immortalThronePath.Trim().ToUpperInvariant())
		{
			this.immortalThronePath = this.folderBrowserDialog.SelectedPath.Trim().ToUpperInvariant();
			this.immortalThronePathTextBox.Text = this.immortalThronePath.Trim().ToUpperInvariant();
			this.immortalThronePathTextBox.Invalidate();
			this.ConfigurationChanged = this.GamePathChanged = true;
		}
	}

	/// <summary>
	/// Handler for clicking enable custom maps
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void EnableCustomMapsCheckBoxCheckedChanged(object sender, EventArgs e)
	{
		if (this.enableCustomMapsCheckBox.Checked)
		{
			if (!this.enableMods)
			{
				this.mapListComboBox.Enabled = this.CustomMapsChanged = this.enableMods = this.ConfigurationChanged = true;
			}
		}
		else
		{
			if (this.enableMods)
			{
				this.enableMods = this.mapListComboBox.Enabled = false;
				this.ConfigurationChanged = this.CustomMapsChanged = true;
				this.customMap = string.Empty;// Reset value
			}
		}
	}

	/// <summary>
	/// Handler for changing the selected custom map
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void MapListComboBoxSelectedIndexChanged(object sender, EventArgs e)
	{
		if (!this.settingsLoaded)
			return;

		var custommap = (this.mapListComboBox.SelectedItem as GamePathEntry)?.Path ?? string.Empty;
		if (custommap != Config.UserSettings.Default.CustomMap)
		{
			this.customMap = custommap;
			this.ConfigurationChanged = this.CustomMapsChanged = true;
		}
	}

	/// <summary>
	/// Handler for clicking the load all files check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void LoadAllFilesCheckBoxCheckedChanged(object sender, EventArgs e)
	{
		if (this.loadAllFilesCheckBox.Checked)
		{
			if (!this.loadAllFiles)
			{
				this.loadAllFiles = this.ConfigurationChanged = true;
			}
		}
		else
		{
			if (this.loadAllFiles)
			{
				this.loadAllFiles = false;
				this.ConfigurationChanged = true;
			}
		}
	}

	/// <summary>
	/// Handler for clicking the suppress warnings check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void SuppressWarningsCheckBoxCheckedChanged(object sender, EventArgs e)
	{
		if (this.suppressWarningsCheckBox.Checked)
		{
			if (!this.suppressWarnings)
			{
				this.suppressWarnings = this.ConfigurationChanged = true;
			}
		}
		else
		{
			if (this.suppressWarnings)
			{
				this.suppressWarnings = false;
				this.ConfigurationChanged = true;
			}
		}
	}

	private void PlayerReadonlyCheckboxCheckedChanged(object sender, EventArgs e)
	{
		if (this.playerReadonlyCheckbox.Checked)
		{
			if (!this.playerReadonly)
			{
				this.playerReadonly = this.ConfigurationChanged = true;
			}
		}
		else
		{
			if (this.playerReadonly)
			{
				this.playerReadonly = false;
				this.ConfigurationChanged = true;
			}
		}

	}

	private void CharacterEditCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (this.characterEditCheckBox.Checked)
		{
			if (!this.allowCharacterEdit)
			{
				this.allowCharacterEdit = this.ConfigurationChanged = this.EnableCharacterEditChanged = true;
			}
		}
		else
		{
			if (this.allowCharacterEdit)
			{
				this.allowCharacterEdit = false;
				this.ConfigurationChanged = this.EnableCharacterEditChanged = true;
			}
		}
	}

	private void FontComboBoxBase_SelectedIndexChanged(object sender, EventArgs e)
	{
		var font = this.baseFontComboBox.SelectedItem as ComboBoxItem;
		if (font == null)
			return;

		if (font.Value != Config.UserSettings.Default.BaseFont)
		{
			this.BaseFont = font.Value;
			this.ConfigurationChanged = this.UISettingChanged = true;// Force restart
		}
	}

	private void EnableDetailedTooltipViewCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (this.EnableDetailedTooltipViewCheckBox.Checked)
		{
			if (!this.enableDetailedTooltipView)
				this.enableDetailedTooltipView = this.ConfigurationChanged = true;
		}
		else
		{
			if (this.enableDetailedTooltipView)
			{
				this.enableDetailedTooltipView = false;
				this.ConfigurationChanged = true;
			}
		}
	}

	private void ItemBGColorOpacityTrackBar_Scroll(object sender, EventArgs e)
	{
		this.itemBGColorOpacity = this.ItemBGColorOpacityTrackBar.Value;
		this.ConfigurationChanged = this.ItemBGColorOpacityChanged = true;
	}

	private void EnableItemRequirementRestrictionCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (this.EnableItemRequirementRestrictionCheckBox.Checked)
		{
			if (!this.enableItemRequirementRestriction)
				this.enableItemRequirementRestriction = this.ConfigurationChanged = this.EnableItemRequirementRestrictionChanged = true;
		}
		else
		{
			if (this.enableItemRequirementRestriction)
			{
				this.enableItemRequirementRestriction = false;
				this.ConfigurationChanged = this.EnableItemRequirementRestrictionChanged = true;
			}
		}
	}

	private void hotReloadCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (this.hotReloadCheckBox.Checked && !this.enableHotReload)
		{
			this.enableHotReload = this.ConfigurationChanged = this.EnableHotReloadChanged = this.UISettingChanged = true;// Force restart
			return;
		}

		if (!this.hotReloadCheckBox.Checked && this.enableHotReload)
		{
			this.enableHotReload = false;
			this.ConfigurationChanged = this.EnableHotReloadChanged = this.UISettingChanged = true;// Force restart
		}
	}
	private void scalingCheckBoxEnableEpicLegendaryAffixes_CheckedChanged(object sender, EventArgs e)
	{
		if (this.scalingCheckBoxEnableEpicLegendaryAffixes.Checked && !this.EnableEpicLegendaryAffixes)
		{
			this.EnableEpicLegendaryAffixes = this.ConfigurationChanged = true;
			return;
		}

		if (!this.scalingCheckBoxEnableEpicLegendaryAffixes.Checked && this.EnableEpicLegendaryAffixes)
		{
			this.EnableEpicLegendaryAffixes = false;
			this.ConfigurationChanged = true;
		}
	}

	private void scalingCheckBoxEnableSounds_CheckedChanged(object sender, EventArgs e)
	{
		if (this.scalingCheckBoxEnableSounds.Checked)
		{
			if (!this.enableTQVaultSounds)
				this.enableTQVaultSounds = this.ConfigurationChanged = this.EnableTQVaultSoundsChanged = true;
		}
		else
		{
			if (this.enableTQVaultSounds)
			{
				this.enableTQVaultSounds = false;
				this.ConfigurationChanged = this.EnableTQVaultSoundsChanged = true;
			}
		}
	}

	private void scalingComboBoxCSVDelim_SelectedIndexChanged(object sender, EventArgs e)
	{
		this.ConfigurationChanged = true;
	}

	private void scalingCheckBoxDisableAutoStacking_CheckedChanged(object sender, EventArgs e)
	{

		if (this.scalingCheckBoxDisableAutoStacking.Checked)
		{
			if (!this.disableAutoStacking)
				this.disableAutoStacking = this.ConfigurationChanged = this.DisableAutoStackingChanged = true;
			return;
		}

		if (this.disableAutoStacking)
		{
			this.disableAutoStacking = false;
			this.ConfigurationChanged = this.DisableAutoStackingChanged = true;
		}
	}

	private void SettingsDialog_Shown(object sender, EventArgs e)
	{
		//Adjust width to inner table
		this.Width = this.bufferedTableLayoutPanelSkeleton.Location.X + this.bufferedTableLayoutPanelSkeleton.Width + this.Padding.Right + 5;
	}

	private void checkGroupBoxCheats_CheckedChanged(object sender, EventArgs e)
	{

		if (this.checkGroupBoxAllowCheats.Checked)
		{
			if (!this.allowCheats)
				this.allowCheats = this.ConfigurationChanged = this.AllowCheatsChanged = true;

			return;
		}

		// Unchecked
		if (this.allowCheats)
		{
			this.allowCheats = false;
			this.ConfigurationChanged = this.AllowCheatsChanged = true;
		}

		// Adjust content
		this.characterEditCheckBox.Checked =
		this.allowItemEditCheckBox.Checked =
		this.allowItemCopyCheckBox.Checked = false;
		//this.scalingCheckBoxEnableEpicLegendaryAffixes.Checked = false;// Triggered by allowItemEditCheckBox
	}

	private void checkGroupBoxGitBackup_CheckedChanged(object sender, EventArgs e)
	{
		if (checkGroupBoxGitBackup.Checked)
		{
			if (!this.enableGitBackup)
				this.enableGitBackup = this.ConfigurationChanged = this.UISettingChanged = this.GitBackupEnabledChanged = true;
			return;
		}

		// Unchecked
		if (this.enableGitBackup)
		{
			this.enableGitBackup = false;
			this.ConfigurationChanged = this.UISettingChanged = this.GitBackupEnabledChanged = true;
		}

		this.scalingCheckBoxDisableLegacyBackup.Checked = false;// Restore legacy backup
	}

	private void scalingTextBoxGitRepository_TextChanged(object sender, EventArgs e)
	{
		// Changed
		if (this.gitBackupRepository != scalingTextBoxGitRepository.Text)
		{
			this.gitBackupRepository = scalingTextBoxGitRepository.Text;
			this.ConfigurationChanged = this.UISettingChanged = this.GitBackupRepositoryChanged = true;
		}
	}

	private void scalingCheckBoxDisableLegacyBackup_CheckedChanged(object sender, EventArgs e)
	{

		if (scalingCheckBoxDisableLegacyBackup.Checked)
		{
			if (!this.disableLegacyBackup)
				this.disableLegacyBackup = this.ConfigurationChanged = this.UISettingChanged = this.DisableLegacyBackupChanged = true;
			return;
		}

		// Unchecked
		if (this.disableLegacyBackup)
		{
			this.disableLegacyBackup = false;
			this.ConfigurationChanged = this.UISettingChanged = this.DisableLegacyBackupChanged = true;
		}
	}

	private void scalingCheckBoxBackupPlayerSaves_CheckedChanged(object sender, EventArgs e)
	{
		if (scalingCheckBoxBackupPlayerSaves.Checked)
		{
			if (!this.enableBackupPlayerSaves)
				this.enableBackupPlayerSaves = this.ConfigurationChanged = this.UISettingChanged = this.EnableBackupPlayerSavesChanged = true;
			return;
		}

		// Unchecked
		if (this.enableBackupPlayerSaves)
		{
			this.enableBackupPlayerSaves = false;
			this.ConfigurationChanged = this.UISettingChanged = this.EnableBackupPlayerSavesChanged = true;
		}
	}
}