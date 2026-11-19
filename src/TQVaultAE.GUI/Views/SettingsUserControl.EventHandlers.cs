//-----------------------------------------------------------------------
// <copyright file="SettingsUserControl.EventHandlers.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
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

namespace TQVaultAE.GUI.Views;

/// <summary>
/// The handlers of the settings controls: each writes its value to the editor, which applies the rules between settings.
/// </summary>
internal partial class SettingsUserControl
{
	/// <summary>
	/// Handler for clicking the vault path browse button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void VaultPathBrowseButtonClick(object? sender, EventArgs e)
		=> this.BrowseFolder(Resources.SettingsBrowseVault, Environment.SpecialFolder.Desktop, this.VaultPath
			, this.editor.SetVaultPath, this.vaultPathTextBox, () => this.VaultPath);

	/// <summary>
	/// Lets the user pick a folder from <paramref name="current"/>, gives it to <paramref name="set"/>, and when taken shows
	/// the folder kept (<paramref name="shown"/>) in <paramref name="box"/>.
	/// </summary>
	private void BrowseFolder(string description, Environment.SpecialFolder root, string current, Func<string, bool> set, TextBox box, Func<string> shown)
	{
		this.folderBrowserDialog.Description = description;
		this.folderBrowserDialog.RootFolder = root;
		this.folderBrowserDialog.SelectedPath = current;
		this.folderBrowserDialog.ShowDialog();
		if (set(this.folderBrowserDialog.SelectedPath))
		{
			box.Text = shown();
			box.Invalidate();
		}
	}

	/// <summary>
	/// Opens the key binds dialog on the edited key binds, the actions turned off by the edited settings greyed; its
	/// changes are saved with the other settings by OK.
	/// </summary>
	private void KeyBindsButtonClick(object? sender, EventArgs e)
	{
		using var dialog = this.Services.Forms.Create<Dialogs.KeyBindsDialog>();
		dialog.Bindings = KeyBindMap.FromOverrides(this.editor.Current.KeyBinds.Overrides);
		dialog.Options = new KeyBindOptions(this.editor.Current.AllowItemCopy, this.editor.Current.AllowCharacterEdit);
		if (dialog.ShowDialog(this.FindForm()) != DialogResult.OK)
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
			this.ShowSettingsInUse();
			this.Invalidate();
		}
	}

	/// <summary>
	/// Handler for clicking the OK button: checks the settings, applies the edits and raises <see cref="Applied"/>; an
	/// invalid setting is told and the settings stay open.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void OkayButtonClick(object? sender, EventArgs e)
	{
		// Validation
		if (!this.editor.IsGitRepositoryValid)
		{
			this.Services.MessageBoxes.Inform(Resources.SettingsGitRepositoryUrlInvalid, Resources.GlobalError, MessageBoxIcon.Error);
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

		this.RaiseApplied();
	}

	/// <summary>
	/// Handler for clicking the Cancel button: the host leaves the settings, the edits are dropped.
	/// </summary>
	private void CancelButtonClick(object? sender, EventArgs e) => this.RaiseCancelled();

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

		// No selection happens only while the list is rebuilt. The game may name the language in another case than the
		// list ("ENGLISH"): the same language, left as it was, else opening the settings would count as a change.
		if (this.languageComboBox.SelectedItem is ComboBoxItem language
			&& !string.Equals(language.Value, this.editor.Current.TQLanguage, StringComparison.OrdinalIgnoreCase))
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
		=> this.BrowseFolder(Resources.SettingsBrowseTQ, Environment.SpecialFolder.MyComputer, this.editor.Current.TQPath
			, this.editor.SetTitanQuestPath, this.titanQuestPathTextBox, () => this.editor.Current.TQPath);

	/// <summary>
	/// Handler for clicking the Immortal Throne game path browse button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ImmortalThronePathBrowseButtonClick(object? sender, EventArgs e)
		=> this.BrowseFolder(Resources.SettingsBrowseIT, Environment.SpecialFolder.MyComputer, this.editor.Current.TQITPath
			, this.editor.SetImmortalThronePath, this.immortalThronePathTextBox, () => this.editor.Current.TQITPath);

	private void FontComboBoxBase_SelectedIndexChanged(object? sender, EventArgs e)
	{
		if (this.baseFontComboBox.SelectedItem is ComboBoxItem font)
			this.editor.Current.BaseFont = font.Value;
	}

	private void scalingComboBoxCSVDelim_SelectedIndexChanged(object? sender, EventArgs e)
	{
		if (this.scalingComboBoxCSVDelim.SelectedItem is ComboBoxItem<CsvDelimiter, char> delim)
			this.editor.Current.CSVDelimiter = delim.ComboValue.ToString();
	}

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

	private void numericUpDownMcpServerPort_ValueChanged(object? sender, EventArgs e)
		=> this.editor.Current.McpServerPort = (int)this.numericUpDownMcpServerPort.Value;

	private void PasteBinExpirationComboBoxSelectedIndexChanged(object? sender, EventArgs e)
	{
		if (this.pasteBinExpirationComboBox.SelectedItem is PasteBinExpirationComboBoxItem item)
			this.editor.Current.PasteBinExpiration = item.Key;
	}
}
