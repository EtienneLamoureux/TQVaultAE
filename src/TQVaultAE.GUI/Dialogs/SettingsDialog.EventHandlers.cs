//-----------------------------------------------------------------------
// <copyright file="SettingsDialog.EventHandlers.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
/// <summary>
/// Class for the Settings/Configuration Dialog
/// </summary>

using System.Globalization;
using System.Text.RegularExpressions;
using EnumsNET;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.KeyBinds;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using TQVaultAE.Services;

namespace TQVaultAE.GUI.Dialogs;

internal partial class SettingsDialog
{
	/// <summary>
	/// Handler for clicking the vault path browse button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void VaultPathBrowseButtonClick(object? sender, EventArgs e)
	{
		this.folderBrowserDialog.Description = Resources.SettingsBrowseVault;
		this.folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
		this.folderBrowserDialog.SelectedPath = this.VaultPath;
		this.folderBrowserDialog.ShowDialog();
		if (this.editor.SetVaultPath(this.folderBrowserDialog.SelectedPath))
		{
			this.vaultPathTextBox.Text = this.VaultPath;
			this.vaultPathTextBox.Invalidate();
		}
	}

	/// <summary>
	/// Opens the key binds dialog on the edited key binds, the actions turned off by the edited settings greyed; its
	/// changes are saved with the other settings by OK.
	/// </summary>
	private void KeyBindsButtonClick(object? sender, EventArgs e)
	{
		using var dialog = this.Services.Forms.Create<KeyBindsDialog>();
		dialog.Bindings = KeyBindMap.FromOverrides(this.editor.Current.KeyBinds.Overrides);
		dialog.Options = new KeyBindOptions(this.editor.Current.AllowItemCopy, this.editor.Current.AllowCharacterEdit);
		if (dialog.ShowDialog(this) != DialogResult.OK)
			return;

		var edited = new KeyBindSettings();
		edited.SetOverrides(dialog.Bindings.ToOverrides());
		this.editor.Current.KeyBinds = edited;
	}

	/// <summary>
	/// Handler for clicking the reset button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ResetButtonClick(object? sender, EventArgs e)
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
	private void SkipTitleCheckBoxCheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.SkipTitle = this.skipTitleCheckBox.Checked;

	/// <summary>
	/// Handler for clicking the OK button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void OkayButtonClick(object? sender, EventArgs e)
	{
		// Validation
		if (!this.editor.IsGitRepositoryValid)
		{
			this.UIService.ShowError(Resources.SettingsGitRepositoryUrlInvalid, Buttons: Application.Contracts.Services.ShowMessageButtons.OK);
			this.DialogResult = DialogResult.None;
			return;
		}

		if (this.ConfigurationChanged)
		{
			this.editor.ApplyTo(USettings);

			// The handlers read the new key binds at once, no restart needed.
			if (this.editor.KeyBindsChanged)
				this.Services.KeyBinds.CopyFrom(KeyBindMap.FromOverrides(USettings.KeyBinds.Overrides));

			// The windows take the new font now, no restart needed.
			if (this.editor.BaseFontChanged)
				this.FontService.Reload();
		}

		this.DialogResult = DialogResult.OK;
	}

	/// <summary>
	/// Handler for clicking the allow item editing check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void AllowItemEditCheckBoxCheckedChanged(object? sender, EventArgs e)
	{
		this.editor.SetAllowItemEdit(this.allowItemEditCheckBox.Checked);

		this.scalingCheckBoxEnableEpicLegendaryAffixes.Enabled = this.allowItemEditCheckBox.Checked;
		if (!this.allowItemEditCheckBox.Checked)
			this.scalingCheckBoxEnableEpicLegendaryAffixes.Checked = this.editor.Current.EnableEpicLegendaryAffixes;
	}

	/// <summary>
	/// Handler for clicking the allow item copy check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void AllowItemCopyCheckBoxCheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.AllowItemCopy = this.allowItemCopyCheckBox.Checked;

	/// <summary>
	/// Handler for clicking the load last character check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void LoadLastCharacterCheckBoxCheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.LoadLastCharacter = this.loadLastCharacterCheckBox.Checked;

	/// <summary>
	/// Handler for clicking the load last vault check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void LoadLastVaultCheckBoxCheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.LoadLastVault = this.loadLastVaultCheckBox.Checked;

	/// <summary>
	/// Handler for clicking the detect language check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void DetectLanguageCheckBoxCheckedChanged(object? sender, EventArgs e)
	{
		// Turning the detection on forces a restart to detect the language: see SettingsEditor.LanguageChanged.
		this.editor.Current.AutoDetectLanguage = this.detectLanguageCheckBox.Checked;
		this.languageComboBox.Enabled = !this.detectLanguageCheckBox.Checked;
	}

	/// <summary>
	/// Handler for clicking the detect game paths check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void DetectGamePathsCheckBoxCheckedChanged(object? sender, EventArgs e)
	{
		// Turning the detection on forces a restart to detect the game paths: see SettingsEditor.GamePathChanged.
		this.editor.Current.AutoDetectGamePath = this.detectGamePathsCheckBox.Checked;
		this.titanQuestPathTextBox.Enabled = this.immortalThronePathTextBox.Enabled
			= this.titanQuestPathBrowseButton.Enabled = this.immortalThronePathBrowseButton.Enabled = !this.detectGamePathsCheckBox.Checked;
	}

	/// <summary>
	/// Handler for changing the language selection
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void LanguageComboBoxSelectedIndexChanged(object? sender, EventArgs e)
	{
		// There was some problem getting the game language so we ignore changing it.
		if (this.Services.SettingsPersistence.GameLanguage is null)
			return;

		// No selection happens only while the list is rebuilt.
		if (this.languageComboBox.SelectedItem is ComboBoxItem language)
			this.editor.Current.TQLanguage = language.Value;
	}

	/// <summary>
	/// Handler for leaving the vault text box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void VaultPathTextBoxLeave(object? sender, EventArgs e)
	{
		if (this.editor.SetVaultPath(this.vaultPathTextBox.Text))
			this.vaultPathTextBox.Invalidate();
	}

	/// <summary>
	/// Handler for leaving the Titan Quest game path text box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void TitanQuestPathTextBoxLeave(object? sender, EventArgs e)
	{
		if (this.editor.SetTitanQuestPath(this.titanQuestPathTextBox.Text))
			this.titanQuestPathTextBox.Invalidate();
	}

	/// <summary>
	/// Handler for leaving the Immortal Throne game path text box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ImmortalThronePathTextBoxLeave(object? sender, EventArgs e)
	{
		if (this.editor.SetImmortalThronePath(this.immortalThronePathTextBox.Text))
			this.immortalThronePathTextBox.Invalidate();
	}

	/// <summary>
	/// Handler for clicking the Titan Quest game path browse button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void TitanQuestPathBrowseButtonClick(object? sender, EventArgs e)
	{
		this.folderBrowserDialog.Description = Resources.SettingsBrowseTQ;
		this.folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
		this.folderBrowserDialog.SelectedPath = this.editor.Current.TQPath;
		this.folderBrowserDialog.ShowDialog();
		if (this.editor.SetTitanQuestPath(this.folderBrowserDialog.SelectedPath))
		{
			this.titanQuestPathTextBox.Text = this.editor.Current.TQPath;
			this.titanQuestPathTextBox.Invalidate();
		}
	}

	/// <summary>
	/// Handler for clicking the Immortal Throne game path browse button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ImmortalThronePathBrowseButtonClick(object? sender, EventArgs e)
	{
		this.folderBrowserDialog.Description = Resources.SettingsBrowseIT;
		this.folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
		this.folderBrowserDialog.SelectedPath = this.editor.Current.TQITPath;
		this.folderBrowserDialog.ShowDialog();
		if (this.editor.SetImmortalThronePath(this.folderBrowserDialog.SelectedPath))
		{
			this.immortalThronePathTextBox.Text = this.editor.Current.TQITPath;
			this.immortalThronePathTextBox.Invalidate();
		}
	}

	/// <summary>
	/// Handler for clicking the load all files check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void LoadAllFilesCheckBoxCheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.LoadAllFiles = this.loadAllFilesCheckBox.Checked;

	/// <summary>
	/// Handler for clicking the suppress warnings check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void SuppressWarningsCheckBoxCheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.SuppressWarnings = this.suppressWarningsCheckBox.Checked;

	private void PlayerReadonlyCheckboxCheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.PlayerReadonly = this.playerReadonlyCheckbox.Checked;

	private void CharacterEditCheckBox_CheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.AllowCharacterEdit = this.characterEditCheckBox.Checked;

	private void FontComboBoxBase_SelectedIndexChanged(object? sender, EventArgs e)
	{
		if (this.baseFontComboBox.SelectedItem is ComboBoxItem font)
			this.editor.Current.BaseFont = font.Value;
	}

	private void scalingCheckBoxTooltipDetailedView_CheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.EnableDetailedTooltipView = this.scalingCheckBoxTooltipDetailedView.Checked;

	private void scalingCheckBoxTooltipInGameRolls_CheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.TooltipShowInGameRolls = this.scalingCheckBoxTooltipInGameRolls.Checked;

	private void scalingCheckBoxTooltipValueRanges_CheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.TooltipShowValueRanges = this.scalingCheckBoxTooltipValueRanges.Checked;

	private void scalingCheckBoxTooltipGrantedSkills_CheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.TooltipShowGrantedSkills = this.scalingCheckBoxTooltipGrantedSkills.Checked;

	private void scalingCheckBoxTooltipSetPieceCounts_CheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.TooltipShowSetPieceCounts = this.scalingCheckBoxTooltipSetPieceCounts.Checked;

	private void ShowInGameValuesCheckBox_CheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.ShowInGameValues = this.ShowInGameValuesCheckBox.Checked;

	private void EnableItemRequirementRestrictionCheckBox_CheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.EnableItemRequirementRestriction = this.EnableItemRequirementRestrictionCheckBox.Checked;

	private void hotReloadCheckBox_CheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.EnableHotReload = this.hotReloadCheckBox.Checked;// Force restart

	private void scalingCheckBoxEnableEpicLegendaryAffixes_CheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.EnableEpicLegendaryAffixes = this.scalingCheckBoxEnableEpicLegendaryAffixes.Checked;

	private void scalingCheckBoxEnableSounds_CheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.EnableTQVaultSounds = this.scalingCheckBoxEnableSounds.Checked;

	private void scalingComboBoxCSVDelim_SelectedIndexChanged(object? sender, EventArgs e)
	{
		if (this.scalingComboBoxCSVDelim.SelectedItem is ComboBoxItem<CsvDelimiter, char> delim)
			this.editor.Current.CSVDelimiter = delim.ComboValue.ToString();
	}

	private void scalingCheckBoxDisableAutoStacking_CheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.DisableAutoStacking = this.scalingCheckBoxDisableAutoStacking.Checked;

	private void scalingCheckBoxMergeRelicsOnSort_CheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.MergeRelicsOnSort = this.scalingCheckBoxMergeRelicsOnSort.Checked;

	private void checkGroupBoxCheats_CheckedChanged(object? sender, EventArgs e)
	{
		// Turning the cheats off turns off character editing, item editing and item copying.
		this.editor.SetAllowCheats(this.checkGroupBoxAllowCheats.Checked);

		if (this.checkGroupBoxAllowCheats.Checked)
			return;

		// Adjust content
		this.characterEditCheckBox.Checked = this.editor.Current.AllowCharacterEdit;
		this.allowItemEditCheckBox.Checked = this.editor.Current.AllowItemEdit;
		this.allowItemCopyCheckBox.Checked = this.editor.Current.AllowItemCopy;
		//this.scalingCheckBoxEnableEpicLegendaryAffixes.Checked = false;// Triggered by allowItemEditCheckBox
	}

	private void checkGroupBoxGitBackup_CheckedChanged(object? sender, EventArgs e)
	{
		// Turning the git backup off brings back the legacy backup.
		this.editor.SetGitBackupEnabled(this.checkGroupBoxGitBackup.Checked);

		if (!this.checkGroupBoxGitBackup.Checked)
			this.scalingCheckBoxDisableLegacyBackup.Checked = this.editor.Current.DisableLegacyBackup;
	}

	private void scalingCheckBoxDisableLegacyBackup_CheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.DisableLegacyBackup = this.scalingCheckBoxDisableLegacyBackup.Checked;

	private void scalingCheckBoxBackupPlayerSaves_CheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.GitBackupPlayerSavesEnabled = this.scalingCheckBoxBackupPlayerSaves.Checked;

	private void checkGroupBoxOriginalTQSupport_CheckedChanged(object? sender, EventArgs e)
		=> this.editor.Current.EnableOriginalTQSupport = this.checkGroupBoxOriginalTQSupport.Checked;

	private void PasteBinExpirationComboBoxSelectedIndexChanged(object? sender, EventArgs e)
	{
		if (this.pasteBinExpirationComboBox.SelectedItem is PasteBinExpirationComboBoxItem item)
			this.editor.Current.PasteBinExpiration = item.Key;
	}
}
