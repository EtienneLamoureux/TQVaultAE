//-----------------------------------------------------------------------
// <copyright file="SettingsDialog.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Globalization;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using EnumsNET;
using TQVaultAE.GUI.Infrastructure;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// Class for the Settings/Configuration Dialog
/// </summary>
internal sealed partial class SettingsDialog : VaultForm, IScalingControl
{
	#region Properties & Fields

	/// <summary>
	/// The settings being edited, with the rules between them and the changes made.
	/// </summary>
	private SettingsEditor editor = new(new SettingsValues());

	/// <summary>
	/// Save path for the vault files
	/// </summary>
	public string VaultPath => this.editor.Current.VaultPath;

	/// <summary>
	/// Indicates whether the vault path has been changed
	/// </summary>
	public bool VaultPathChanged => this.editor.VaultPathChanged;

	/// <summary>
	/// Indicates that any configuration item has been changed
	/// </summary>
	public bool ConfigurationChanged => this.editor.ConfigurationChanged;

	/// <summary>
	/// Indicates that the settings have been loaded
	/// </summary>
	private bool settingsLoaded;

	/// <summary>
	/// Indicates that the ItemBGColorOpacity setting has been changed
	/// </summary>
	public bool ItemBGColorOpacityChanged => this.editor.ItemBGColorOpacityChanged;

	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.EnableItemRequirementRestriction"/> setting has been changed
	/// </summary>
	public bool EnableItemRequirementRestrictionChanged => this.editor.EnableItemRequirementRestrictionChanged;

	/// <summary>
	/// Indicates that the allow character edit setting has been changed.
	/// </summary>
	public bool EnableCharacterEditChanged => this.editor.EnableCharacterEditChanged;

	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.EnableTQVaultSounds"/> setting has been changed
	/// </summary>
	public bool EnableTQVaultSoundsChanged => this.editor.EnableTQVaultSoundsChanged;

	/// <summary>
	/// Why the changes need a restart, if they do.
	/// </summary>
	public RestartReason RestartReason => this.editor.RestartReason;

	private static readonly Dictionary<string, string> PasteBinExpirationOptions = new()
	{
		["10M"] = "10 Minutes",
		["1H"] = "1 Hour",
		["1D"] = "1 Day",
		["1W"] = "1 Week",
		["2W"] = "2 Weeks",
		["1M"] = "1 Month",
		["6M"] = "6 Months",
		["1Y"] = "1 Year",
		["N"] = "Never"
	};

	private sealed record PasteBinExpirationComboBoxItem(string Key, string Label)
	{
		public override string ToString() => this.Label;
	}

	private readonly ILogger<SettingsDialog> Log;

	#endregion

	/// <summary>
	/// Initializes a new instance of the SettingsDialog class.
	/// </summary>
	public SettingsDialog(MainForm instance) : base(instance.Services)
	{
		this.Owner = instance;
		this.Log = instance.Services.Logger<SettingsDialog>();

		this.InitializeComponent();

		// The dialog follows its inner table: the font, a translation, a group shown or a monitor of another DPI change it.
		this.bufferedTableLayoutPanelSkeleton.SizeChanged += (_, _) => this.FitToContent();
		this.FitToContent();

		foreach (var kvp in PasteBinExpirationOptions)
		{
			this.pasteBinExpirationComboBox.Items.Add(new PasteBinExpirationComboBoxItem(kvp.Key, kvp.Value));
		}

		this.SuspendLayout();

		//this.ShowResizeBorders = false;
		// As in the other dialogs: the zoom reset of this button belongs to the main window, here it reset the
		// zoom of the whole application without resizing the main window.
		this.NormalizeBox = false;
		//this.DrawCustomBorder = true;
		//this.ResizeCustomAllowed = false;
		//this.ScaleOnResize = false;

		this.linkLabelTQOriginalSupport.LinkColor = TQColor.Aqua.Color();

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
		this.baseFontLabel.Font =
		this.baseFontComboBox.Font =
		this.scalingCheckBoxTooltipDetailedView.Font =
		this.scalingCheckBoxTooltipInGameRolls.Font =
		this.scalingCheckBoxTooltipValueRanges.Font =
		this.scalingCheckBoxTooltipGrantedSkills.Font =
		this.scalingCheckBoxTooltipSetPieceCounts.Font =
		this.ShowInGameValuesCheckBox.Font =
		this.EnableItemRequirementRestrictionCheckBox.Font =
		this.ItemBGColorOpacityLabel.Font =
		this.scalingLabelCSVDelim.Font =
		this.scalingComboBoxCSVDelim.Font =
		this.scalingCheckBoxEnableEpicLegendaryAffixes.Font =
		this.scalingCheckBoxDisableAutoStacking.Font = font1125;

		this.checkGroupBoxGitBackup.ProcessAllControls((ctr) => ctr.Font = font1125);
		this.checkGroupBoxOriginalTQSupport.ProcessAllControls((ctr) => ctr.Font = font1125);

		this.Font = font1125;

		#endregion

		this.ApplyMarbleStyle();

		this.groupBoxVaultPath.Text = Resources.SettingsLabel1;
		this.languageLabel.Text = Resources.SettingsLabel2;
		this.titanQuestPathLabel.Text = Resources.SettingsLabel3;
		this.immortalThronePathLabel.Text = Resources.SettingsLabel4;
		this.detectGamePathsCheckBox.Text = Resources.SettingsDetectGamePath;
		this.detectLanguageCheckBox.Text = Resources.SettingsDetectLanguage;
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
		this.groupBoxTooltipEnhanced.Text = Resources.SettingTooltipEnhanced;
		this.scalingCheckBoxTooltipDetailedView.Text = Resources.SettingTooltipDetailedView;
		this.toolTip.SetToolTip(this.scalingCheckBoxTooltipDetailedView, Resources.SettingTooltipDetailedViewTT);
		this.scalingCheckBoxTooltipInGameRolls.Text = Resources.SettingTooltipShowInGameRolls;
		this.toolTip.SetToolTip(this.scalingCheckBoxTooltipInGameRolls, Resources.SettingTooltipShowInGameRollsTT);
		this.scalingCheckBoxTooltipValueRanges.Text = Resources.SettingTooltipShowValueRanges;
		this.toolTip.SetToolTip(this.scalingCheckBoxTooltipValueRanges, Resources.SettingTooltipShowValueRangesTT);
		this.scalingCheckBoxTooltipGrantedSkills.Text = Resources.SettingTooltipShowGrantedSkills;
		this.toolTip.SetToolTip(this.scalingCheckBoxTooltipGrantedSkills, Resources.SettingTooltipShowGrantedSkillsTT);
		this.scalingCheckBoxTooltipSetPieceCounts.Text = Resources.SettingTooltipShowSetPieceCounts;
		this.toolTip.SetToolTip(this.scalingCheckBoxTooltipSetPieceCounts, Resources.SettingTooltipShowSetPieceCountsTT);
		this.ShowInGameValuesCheckBox.Text = Resources.SettingShowInGameValues;
		this.toolTip.SetToolTip(this.ShowInGameValuesCheckBox, Resources.SettingShowInGameValuesTT);
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

		this.checkGroupBoxOriginalTQSupport.Text = Resources.SettingsOriginalTQSupport;
		this.toolTip.SetToolTip(this.checkGroupBoxOriginalTQSupport, Resources.SettingsOriginalTQSupportTT);
		this.linkLabelTQOriginalSupport.Text = Resources.SettingsHowToPlay;

		this.cancelButton.Text = Resources.GlobalCancel;
		this.okayButton.Text = Resources.GlobalOK;
		this.Text = Resources.SettingsTitle;

		this.ResumeLayout(false);
		this.PerformLayout();
	}

	/// <summary>
	/// The look of Vault Maintenance: the tiled marble behind the dialog, each group in a caravan frame with its caption
	/// drawn straight over the top edge (<see cref="CaptionBackground.Black"/> puts it back on black), and a drop shadow
	/// on the labels and check boxes left over the marble.
	/// </summary>
	private void ApplyMarbleStyle()
	{
		MarbleBackground.ApplyTo(this);
		this.bufferedTableLayoutPanelSkeleton.BackColor = Color.Transparent;

		this.ProcessAllControls(c =>
		{
			if (c is GroupBox group)
				CaravanFrame.ApplyWithCaption(group, CaptionBackground.None);
		});

		// A track bar cannot be transparent: on the black of its frame.
		this.ItemBGColorOpacityTrackBar.BackColor = Color.Black;

		this.ProcessAllControls(c =>
		{
			bool inFrame = false;
			for (var parent = c.Parent; parent is not null && !inFrame; parent = parent.Parent)
				inFrame = parent is GroupBox;
			if (inFrame)
				return;

			if (c is ScalingLabel label)
				label.TextShadow = true;
			else if (c is ScalingCheckBox box)
				box.TextShadow = true;
		});
	}

	/// <summary>
	/// Override of ScaleControl which supports font scaling.
	/// </summary>
	/// <param name="factor">SizeF for the scale factor</param>
	/// <param name="specified">BoundsSpecified value.</param>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		this.Font = ZoomScope.ScaleFont(this.Font, factor.Height);
		base.ScaleControl(factor, specified);
	}

	/// <summary>
	/// Handler for loading this dialog box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void SettingsDialogLoad(object? sender, EventArgs e)
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
		// Force English since there was some issue with getting the proper language setting.
		var gl = this.Services.SettingsPersistence.GameLanguage;
		var language = gl is null ? "English" : gl;

		this.editor = new SettingsEditor(SettingsValues.From(USettings, language, GamePathResolver.GamePathTQ, GamePathResolver.GamePathTQIT));

		this.settingsLoaded = true;
	}

	/// <summary>
	/// Updates the dialog settings display
	/// </summary>
	private void UpdateDialogSettings()
	{
		// Check to see that we can update things
		if (!this.settingsLoaded)
			return;

		var values = this.editor.Current;

		// Read before the language combo box is cleared, which can raise its handler with no selection.
		var language = values.TQLanguage;

		this.SuspendLayout();

		// Build language combo box
		this.languageComboBox.Items.Clear();

		// The languages of the config file, English names as values; English alone when none is valid.
		ComboBoxItem[] languages = [.. SettingsChoices.GameLanguages(USettings.GameLanguages)
			.Select(choice => new ComboBoxItem() { Value = choice.Value, DisplayName = choice.DisplayName })];

		this.languageComboBox.Items.AddRange(languages);

		// The controls raise their handlers, which write back to the editor and apply its rules, as the fields did.
		this.vaultPathTextBox.Text = values.VaultPath;
		this.skipTitleCheckBox.Checked = values.SkipTitle;
		this.allowItemEditCheckBox.Checked = values.AllowItemEdit;
		this.allowItemCopyCheckBox.Checked = values.AllowItemCopy;
		this.characterEditCheckBox.Checked = values.AllowCharacterEdit;
		this.loadLastCharacterCheckBox.Checked = values.LoadLastCharacter;
		this.loadLastVaultCheckBox.Checked = values.LoadLastVault;
		this.detectLanguageCheckBox.Checked = values.AutoDetectLanguage;
		this.detectGamePathsCheckBox.Checked = values.AutoDetectGamePath;
		this.immortalThronePathTextBox.Text = values.TQITPath;
		this.immortalThronePathTextBox.Enabled = !values.AutoDetectGamePath;
		this.immortalThronePathBrowseButton.Enabled = !values.AutoDetectGamePath;
		this.titanQuestPathTextBox.Text = values.TQPath;
		this.titanQuestPathTextBox.Enabled = !values.AutoDetectGamePath;
		this.titanQuestPathBrowseButton.Enabled = !values.AutoDetectGamePath;
		this.loadAllFilesCheckBox.Checked = values.LoadAllFiles;
		this.suppressWarningsCheckBox.Checked = values.SuppressWarnings;
		this.playerReadonlyCheckbox.Checked = values.PlayerReadonly;
		this.scalingCheckBoxTooltipDetailedView.Checked = values.EnableDetailedTooltipView;
		this.scalingCheckBoxTooltipInGameRolls.Checked = values.TooltipShowInGameRolls;
		this.scalingCheckBoxTooltipValueRanges.Checked = values.TooltipShowValueRanges;
		this.scalingCheckBoxTooltipGrantedSkills.Checked = values.TooltipShowGrantedSkills;
		this.scalingCheckBoxTooltipSetPieceCounts.Checked = values.TooltipShowSetPieceCounts;
		this.ShowInGameValuesCheckBox.Checked = values.ShowInGameValues;
		this.ItemBGColorOpacityTrackBar.Value = values.ItemBGColorOpacity;
		this.EnableItemRequirementRestrictionCheckBox.Checked = values.EnableItemRequirementRestriction;
		this.hotReloadCheckBox.Checked = values.EnableHotReload;
		this.scalingCheckBoxEnableSounds.Checked = values.EnableTQVaultSounds;
		this.scalingCheckBoxEnableEpicLegendaryAffixes.Checked = values.EnableEpicLegendaryAffixes;
		this.scalingCheckBoxDisableAutoStacking.Checked = values.DisableAutoStacking;
		this.checkGroupBoxAllowCheats.Checked = values.AllowCheats;
		this.checkGroupBoxGitBackup.Checked = values.GitBackupEnabled;
		this.scalingCheckBoxDisableLegacyBackup.Checked = values.DisableLegacyBackup;
		this.scalingTextBoxGitRepository.Text = values.GitBackupRepository;
		this.scalingCheckBoxBackupPlayerSaves.Checked = values.GitBackupPlayerSavesEnabled;
		this.checkGroupBoxOriginalTQSupport.Checked = values.EnableOriginalTQSupport;

		this.pasteBinApiKeyTextBox.Text = values.PasteBinApiKey;

		PasteBinExpirationOptions.TryGetValue(values.PasteBinExpiration, out var expLabel);
		if (expLabel is not null)
		{
			this.pasteBinExpirationComboBox.SelectedItem = this.pasteBinExpirationComboBox.Items
				.OfType<PasteBinExpirationComboBoxItem>()
				.FirstOrDefault(i => i.Key == values.PasteBinExpiration);
		}

		this.languageComboBox.SelectedItem = languages.FirstOrDefault(cb => cb.Value.Equals(language,StringComparison.OrdinalIgnoreCase));

		this.languageComboBox.Enabled = !values.AutoDetectLanguage;

		// Build Font combo box
		this.baseFontComboBox.Items.Clear();
		var listItem = Enums.GetMembers<FontFamilyList>()
			.Select(m => new ComboBoxItem()
			{
				Value = m.AsString(EnumFormat.Name) ?? string.Empty,
				DisplayName = m.AsString(EnumFormat.Description, EnumFormat.Name) ?? string.Empty
			}).ToArray();
		this.baseFontComboBox.Items.AddRange(listItem);
		this.baseFontComboBox.SelectedItem = listItem.Where(i => i.Value == values.BaseFont).FirstOrDefault() ?? listItem.First();

		// Build CsvDelim combo box
		this.scalingComboBoxCSVDelim.Items.Clear();
		ComboBoxItem<CsvDelimiter, char>[] listItemDelim = [.. SettingsChoices.CsvDelimiters()
			.Select(choice => new ComboBoxItem<CsvDelimiter, char>()
			{
				ComboValue = choice.Value,
				Value = choice.Value.ToChar(),
				DisplayName = choice.DisplayName
			})];
		this.scalingComboBoxCSVDelim.Items.AddRange(listItemDelim);
		// The edited value, read the way the export reads it (an unknown value is a comma).
		var delimiter = CsvDelimiters.Parse(values.CSVDelimiter);
		this.scalingComboBoxCSVDelim.SelectedItem = listItemDelim.First(i => i.ComboValue == delimiter);

		this.ResumeLayout(false);
		this.PerformLayout();
	}

	private void ItemBGColorOpacityTrackBar_Scroll(object? sender, EventArgs e)
	{
		this.editor.Current.ItemBGColorOpacity = this.ItemBGColorOpacityTrackBar.Value;
	}

	/// <summary>
	/// On a monitor of another DPI, Windows gives the window its suggested size after the layout was scaled, over the
	/// fit made while the table changed: fit again once it is applied.
	/// </summary>
	protected override void OnDpiChanged(DpiChangedEventArgs e)
	{
		base.OnDpiChanged(e);
		this.FitToContent();
	}

	/// <summary>
	/// Adjusts the size to the inner table, so a taller column does not push the buttons out. Not Form.AutoSize: the
	/// caption buttons of the custom border sit on the right edge and would keep the form from shrinking.
	/// </summary>
	private void FitToContent()
	{
		this.Width = this.bufferedTableLayoutPanelSkeleton.Location.X + this.bufferedTableLayoutPanelSkeleton.Width + this.Padding.Right + 5;
		this.Height = this.bufferedTableLayoutPanelSkeleton.Location.Y + this.bufferedTableLayoutPanelSkeleton.Height + this.Padding.Bottom + 5;
	}

	private void scalingTextBoxGitRepository_TextChanged(object? sender, EventArgs e)
	{
		this.editor.Current.GitBackupRepository = this.scalingTextBoxGitRepository.Text;
	}

	private void linkLabelTQOriginalSupport_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
	{
		ProcessStartInfo psInfo = new ProcessStartInfo
		{
			FileName = this.USettings.TQOriginalHowtoUrl,
			UseShellExecute = true
		};
		Process.Start(psInfo);
	}

	private void PasteBinApiKeyTextBoxTextChanged(object? sender, EventArgs e)
	{
		this.editor.Current.PasteBinApiKey = this.pasteBinApiKeyTextBox.Text ?? string.Empty;
	}
}