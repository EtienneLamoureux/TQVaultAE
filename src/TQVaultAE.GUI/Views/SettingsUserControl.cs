//-----------------------------------------------------------------------
// <copyright file="SettingsUserControl.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Diagnostics;
using EnumsNET;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Services;

namespace TQVaultAE.GUI.Views;

/// <summary>
/// The settings: every option with its rules, the key binds button and the OK, Cancel and reset buttons, with the
/// caravan frames over the main window's own background. The main window shows it in place of its main content: it
/// shows the settings in use with <see cref="ShowSettingsInUse"/> at each opening, and leaves it on <see cref="Applied"/> (OK, the edits saved in
/// <see cref="UserSettings"/>) or <see cref="Cancelled"/> (Cancel or Escape, the edits dropped).
/// </summary>
/// <remarks>
/// Sized to its content (<see cref="Control.AutoSize"/>) by default; given an area by its host instead (AutoSize off), it
/// lays its content in the middle, with scroll bars when the area is smaller (<see cref="ScrollableControl.AutoScroll"/>).
/// </remarks>
internal sealed partial class SettingsUserControl : UserControl, IMainWindowView
{
	#region Properties & Fields

	private readonly GuiServices Services;
	private readonly UserSettings USettings;
	private readonly IUIService UIService;
	private readonly IFontService FontService;
	private readonly IGamePathService GamePathResolver;
	private readonly ILogger<SettingsUserControl> Log;

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

	/// <inheritdoc/>
	bool IMainWindowView.HasChanges => this.ConfigurationChanged;

	/// <inheritdoc/>
	string IMainWindowView.DiscardChangesQuestion => Resources.SettingsDiscardChanges;

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

	/// <summary>
	/// OK was pressed and the settings checked: the edits, if any, are in <see cref="UserSettings"/> (not saved to the
	/// file yet) and the change flags tell what changed.
	/// </summary>
	public event EventHandler? Applied;

	/// <summary>
	/// Cancel or Escape was pressed: the edits are dropped at the next <see cref="ShowSettingsInUse"/>.
	/// </summary>
	public event EventHandler? Cancelled;

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

	/// <summary>
	/// Opens the key binds dialog, in the free first cell of the button row. Made here rather than in the designer file,
	/// with the look of the other buttons.
	/// </summary>
	private readonly ScalingButton keyBindsButton;

	#endregion

	/// <summary>
	/// Initializes a new instance of the SettingsUserControl class.
	/// </summary>
	public SettingsUserControl(GuiServices services)
	{
		this.Services = services;
		this.USettings = services.Settings;
		this.UIService = services.UIService;
		this.FontService = services.FontService;
		this.GamePathResolver = services.GamePathService;
		this.Log = services.Logger<SettingsUserControl>();

		this.InitializeComponent();

		// The control follows its inner table: the font, a translation, a group shown or a monitor of another DPI change it.
		this.AutoSize = true;
		this.AutoSizeMode = AutoSizeMode.GrowAndShrink;

		foreach (var kvp in PasteBinExpirationOptions)
		{
			this.pasteBinExpirationComboBox.Items.Add(new PasteBinExpirationComboBoxItem(kvp.Key, kvp.Value));
		}

		this.SuspendLayout();

		this.linkLabelTQOriginalSupport.LinkColor = TQColor.Aqua.Color();

		#region Apply custom font

		var font12 = FontService.GetFontLight(12F);
		var font1125 = FontService.GetFontLight(11.25F);

		// One rule for every control, the PasteBin group's included: the buttons in the larger size, the rest in the base
		// size. The group boxes keep no font of their own (CaravanFrame draws their caption).
		this.Font = font1125;
		this.ProcessAllControls(ctr =>
		{
			if (ctr is Button)
				ctr.Font = font12;
			else if (ctr is not GroupBox)
				ctr.Font = font1125;
		});

		#endregion

		// Shown in the main window: over the window's own background, which the main content shows too.
		CaravanLook.ApplyFrames(this, this.bufferedTableLayoutPanelSkeleton);

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
		this.groupBoxStackable.Text = Resources.SettingsStackable;
		this.groupBoxLoadout.Text = Resources.LoadoutTitle;
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
		this.scalingCheckBoxMergeRelicsOnSort.Text = Resources.SettingsMergeRelicsOnSort;
		this.toolTip.SetToolTip(this.scalingCheckBoxMergeRelicsOnSort, Resources.SettingsMergeRelicsOnSortTT);
		this.scalingCheckBoxLoadoutReplaceWorn.Text = Resources.SettingsLoadoutReplaceWorn;
		this.toolTip.SetToolTip(this.scalingCheckBoxLoadoutReplaceWorn, Resources.SettingsLoadoutReplaceWornTT);

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

		this.keyBindsButton = new ScalingButton()
		{
			Name = nameof(this.keyBindsButton),
			Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
			BackColor = Color.Transparent,
			FlatStyle = FlatStyle.Flat,
			Font = font12,
			ForeColor = this.resetButton.ForeColor,
			Size = this.resetButton.Size,
			SizeToGraphic = false,
			UseCustomGraphic = true,
			UseVisualStyleBackColor = false,
			TabIndex = this.okayButton.TabIndex,
			Text = Resources.SettingsKeyBinds,
		};
		this.keyBindsButton.FlatAppearance.BorderSize = 0;
		this.keyBindsButton.FlatAppearance.MouseDownBackColor = this.resetButton.FlatAppearance.MouseDownBackColor;
		this.keyBindsButton.FlatAppearance.MouseOverBackColor = this.resetButton.FlatAppearance.MouseOverBackColor;
		this.keyBindsButton.Click += this.KeyBindsButtonClick;
		this.toolTip.SetToolTip(this.keyBindsButton, Resources.SettingsKeyBindsTT);
		this.tableLayoutPanelButtons.Controls.Add(this.keyBindsButton, 0, 0);

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
		this.Font = ZoomScope.ScaleFont(this.Font, factor.Height);
		base.ScaleControl(factor, specified);
	}

	/// <summary>
	/// The settings and their many transparent controls are painted in one pass (<c>WS_EX_COMPOSITED</c>): shown in
	/// place of the main content, they appeared control by control over its panels for a few frames.
	/// </summary>
	protected override CreateParams CreateParams
	{
		get
		{
			const int WS_EX_COMPOSITED = 0x02000000;
			var parameters = base.CreateParams;
			parameters.ExStyle |= WS_EX_COMPOSITED;
			return parameters;
		}
	}

	/// <summary>
	/// Escape presses Cancel, as in a dialog: once the focused control left it (an open drop-down list closes first).
	/// </summary>
	protected override bool ProcessDialogKey(Keys keyData)
	{
		if (keyData == Keys.Escape)
		{
			this.cancelButton.PerformClick();
			return true;
		}

		return base.ProcessDialogKey(keyData);
	}

	/// <summary>
	/// In an area given by the host, the settings sit in its middle; at its top left when it is smaller, scrolled.
	/// </summary>
	protected override void OnLayout(LayoutEventArgs e)
	{
		base.OnLayout(e);

		if (this.AutoSize)
			return;

		var content = this.bufferedTableLayoutPanelSkeleton;
		var location = new Point(
			Math.Max(0, (this.ClientSize.Width - content.Width) / 2) + this.AutoScrollPosition.X,
			Math.Max(0, (this.ClientSize.Height - content.Height) / 2) + this.AutoScrollPosition.Y);
		if (content.Location != location)
			content.Location = location;
	}


	/// <summary>
	/// Shows the settings in use, dropping the edits: at each opening and by the reset button. The settings changed
	/// elsewhere since the last opening show too.
	/// </summary>
	public void ShowSettingsInUse()
	{
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
		this.scalingCheckBoxMergeRelicsOnSort.Checked = values.MergeRelicsOnSort;
		this.scalingCheckBoxLoadoutReplaceWorn.Checked = values.LoadoutReplaceWorn;
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