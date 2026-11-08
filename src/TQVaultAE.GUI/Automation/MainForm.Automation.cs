//-----------------------------------------------------------------------
// <copyright file="MainForm.Automation.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.Application;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Config;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Models;

namespace TQVaultAE.GUI.Forms;

/// <summary>
/// Debug-only read access to the main form state for the automation endpoint.
/// </summary>
public partial class MainForm
{
	internal ItemDragInfo AutomationDragInfo => this.DragInfo;

	/// <summary>Zooms like Ctrl + / Ctrl - (delta 0.1 / -0.1) or Ctrl Home (delta 1, back to 100 %).</summary>
	internal void AutomationZoom(float delta) => this.ResizeFormCallback(this, new ResizeEventArgs(delta));

	internal UserSettings AutomationSettings => this.USettings;

	internal string? AutomationSelectedVault => this.vaultListComboBox?.SelectedItem?.ToString();

	internal string? AutomationSelectedSecondaryVault
		=> this.showSecondaryVault ? this.secondaryVaultListComboBox?.SelectedItem?.ToString() : null;

	internal string? AutomationSelectedCharacter => this.comboBoxCharacter?.SelectedItem?.ToString();

	internal IReadOnlyList<string> AutomationCharacters
		=> this.comboBoxCharacter?.Items.Select(i => i?.ToString() ?? string.Empty).ToList() ?? [];

	internal IReadOnlyList<string> AutomationVaults
		=> this.vaultListComboBox?.Items.Cast<object>().Select(i => i?.ToString() ?? string.Empty).ToList() ?? [];

	/// <summary>
	/// Selects a character like the user would in the drop down, which loads the player and its stashes.
	/// </summary>
	/// <returns>false when no character has that name.</returns>
	internal bool AutomationSelectCharacter(string name)
	{
		var item = this.comboBoxCharacter.Items.FirstOrDefault(i => string.Equals(i?.ToString(), name, StringComparison.OrdinalIgnoreCase));
		if (item is null)
			return false;

		this.comboBoxCharacter.SelectedItem = item;
		return true;
	}

	/// <summary>
	/// Selects a vault in the main vault drop down, which loads it.
	/// </summary>
	/// <returns>false when no vault has that name.</returns>
	internal bool AutomationSelectVault(string name)
	{
		var item = this.vaultListComboBox.Items.Cast<object>().FirstOrDefault(i => string.Equals(i?.ToString(), name, StringComparison.OrdinalIgnoreCase));
		if (item is null)
			return false;

		this.vaultListComboBox.SelectedItem = item;
		return true;
	}

	internal string? AutomationCurrentPlayerFile => this.userContext?.CurrentPlayer?.PlayerFile;

	internal bool AutomationLoadingComplete => this.loadingComplete;

	internal VaultPanel? AutomationVaultPanel => this.vaultPanel;

	internal VaultPanel? AutomationSecondaryVaultPanel => this.secondaryVaultPanel;

	internal PlayerPanel? AutomationPlayerPanel => this.playerPanel;

	internal StashPanel? AutomationStashPanel => this.stashPanel;

	internal ForgePanel? AutomationForgePanel => this.forgePanel;

	internal bool AutomationForgeShown => this.forgePanel?.Visible == true;

	/// <summary>Text of the notification bar, which shows the name of the item under the cursor.</summary>
	internal string AutomationNotificationText => this.NotificationText.Text;

	/// <summary>
	/// Shows or hides the forge like the Forge button does, which also hides or restores the other panels.
	/// </summary>
	internal void AutomationShowForge(bool show)
	{
		if (show != this.AutomationForgeShown)
			this.scalingButtonForge_Click(this, EventArgs.Empty);
	}

	/// <summary>Shows the highlight filters panel like its link, and returns it.</summary>
	internal HighlightFilters AutomationShowHighlightFilters()
	{
		if (!this.highlightFilters.Visible)
			this.scalingLabelHighlight_LinkClicked(this.scalingLabelHighlight, new LinkLabelLinkClickedEventArgs(this.scalingLabelHighlight.Links[0]));
		return this.highlightFilters;
	}


	/// <summary>
	/// Creates a vault like the New action of the vault maintenance dialog: adds it to the list, selects and loads it.
	/// </summary>
	/// <returns>Why the name is refused, or <see cref="VaultMaintenanceError.None"/>.</returns>
	internal VaultMaintenanceError AutomationCreateVault(string name)
	{
		var error = this.vaultService.ValidateMaintenance(VaultMaintenance.New, null, name, this.AutomationVaults);
		if (error != VaultMaintenanceError.None)
			return error;

		this.vaultListComboBox.Items.Add(name);
		this.vaultListComboBox.SelectedItem = name;
		this.LoadVault(name, false);
		return VaultMaintenanceError.None;
	}

	/// <summary>
	/// Duplicates the selected character like the Duplicate Character menu entry, with <paramref name="newName"/>
	/// instead of the name dialog. On success, TQVault restarts shortly after, as it does for a user.
	/// </summary>
	/// <returns>An error message, or null on success.</returns>
	internal string? AutomationDuplicateCharacter(string newName)
	{
		if (this.comboBoxCharacter.SelectedItem is not PlayerSave ps)
			return "No character is selected.";

		var existingNames = this.comboBoxCharacter.Items.OfType<PlayerSave>().Select(save => save.Name).ToList();
		var nameError = this.playerService.ValidateNewCharacterName(ps, newName, existingNames);
		if (nameError != CharacterNameError.None)
			return $"Name refused: {nameError}.";

		var result = this.playerService.DuplicateCharacter(ps, newName);
		if (!result.IsSuccess)
			return result.Error?.Message ?? "The duplication failed.";

		// Restart after the tool has answered.
		_ = Task.Delay(500).ContinueWith(_ => this.BeginInvoke(System.Windows.Forms.Application.Restart), TaskScheduler.Default);
		return null;
	}
}
