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
using TQVaultAE.GUI.Components.Character;
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

	/// <summary>The largest client size Windows lets the window take, and the largest zoom that fits it.</summary>
	internal (Size MaxClientSize, float LargestZoom) AutomationZoomLimit => (this.MaxClientSize, this.LargestZoom());

	/// <summary>Gets the table that lays out the vault, player and stash panels.</summary>
	internal TableLayoutPanel AutomationLayoutTable => this.tableLayoutPanelMain;

	internal string? AutomationSelectedVault => this.vaultListComboBox?.SelectedItem?.ToString();

	/// <summary>Shows a search result like a click on its row in the results dialog.</summary>
	internal void AutomationSelectResult(TQVaultAE.Application.Results.SearchResult result) => this.SelectResult(this, new ResultChangedEventArgs(result));

	/// <summary>
	/// Reads the menu of the vault tools button (next to the vault list) as it opens; with <paramref name="path"/>,
	/// clicks that entry after the calling tool has returned.
	/// </summary>
	internal (IReadOnlyList<AutomationMenuEntry> Entries, string? Clicked) AutomationVaultMenu(IReadOnlyList<string>? path)
	{
		this.PrepareVaultMenu();
		var entries = AutomationMenus.Describe(this.vaultExportContextMenu.Items);
		string? clicked = path is { Count: > 0 } ? AutomationMenus.ClickLater(this, this.vaultExportContextMenu, path) : null;
		return (entries, clicked);
	}

	/// <summary>Draws the menu of the vault tools button as it opens, with the sub menus along <paramref name="open"/>.</summary>
	internal Bitmap AutomationRenderVaultMenu(IReadOnlyList<string> open)
	{
		this.PrepareVaultMenu();
		return AutomationMenus.Render(this.vaultExportContextMenu, open);
	}

	/// <summary>The mod the shown vault is linked to; null for none.</summary>
	internal string? AutomationSelectedVaultMod => this.vaultPanel?.Vault?.Mod;

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

		// The first entry opens the modal maintenance dialog: select it after the calling tool has returned.
		if (this.vaultListComboBox.Items.IndexOf(item) == 0)
			this.BeginInvoke(() => this.vaultListComboBox.SelectedItem = item);
		else
			this.vaultListComboBox.SelectedItem = item;
		return true;
	}

	/// <summary>
	/// Exports the vault shown to a JSON file, like "Export to file" without its file dialog.
	/// </summary>
	/// <returns>The name of the exported vault, or null when no vault is shown.</returns>
	internal string? AutomationExportVault(string fileName)
	{
		var vault = this.vaultPanel.Player;
		if (vault is null)
			return null;

		this.itemExchangeService.ExportVaultToFile(vault, fileName);
		return vault.PlayerName;
	}

	/// <summary>
	/// Writes the CSV export of the vault shown, or of one of its bags, to a file, like "Export CSV" without the clipboard.
	/// </summary>
	/// <returns>The name of the exported vault, or null when no vault is shown.</returns>
	internal string? AutomationExportCsv(string fileName, int? bagIndex)
	{
		var vault = this.vaultPanel.Player;
		if (vault is null)
			return null;

		File.WriteAllText(fileName, this.vaultPanel.BuildCsvExport(bagIndex));
		return vault.PlayerName;
	}

	/// <summary>
	/// Imports an exported file, like "Import from file" without its file dialog.
	/// </summary>
	internal void AutomationImportFile(string fileName) => this.ImportFile(fileName);

	/// <summary>
	/// Imports text like a paste (Ctrl+V) of the clipboard holding it, without touching the clipboard: the same parsing
	/// (exported JSON, or a PasteBin link) and the same placement as <see cref="HandleClipboardImportAsync"/>.
	/// </summary>
	/// <returns>The import's error, or null when it succeeded.</returns>
	internal string? AutomationImportText(string text)
	{
		// Off the UI thread, as the paste awaits it: a PasteBin link is downloaded.
		var result = Task.Run(() => this.itemExchangeService.ImportFromTextAsync(text)).GetAwaiter().GetResult();
		this.HandleImportResult(result);
		return result.Success ? null : result.ErrorMessage;
	}

	internal string? AutomationCurrentPlayerFile => this.userContext?.CurrentPlayer?.PlayerFile;

	/// <summary>The character shown, as loaded.</summary>
	internal TQVaultAE.Application.PlayerCollection? AutomationCurrentPlayer => this.userContext?.CurrentPlayer;

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


	/// <summary>
	/// Saves the modified characters, vaults and stashes like the Save button, file errors included
	/// (the Abort/Retry/Ignore box). Returns whether characters were saved.
	/// </summary>
	internal bool AutomationSaveFiles() => this.SaveAllModifiedFiles();


	/// <summary>The character drop down, whose context menu holds the tag boxes.</summary>
	internal ComboBoxCharacter AutomationCharacterCombo => this.comboBoxCharacter;

	/// <summary>
	/// Opens the character editor like the Edit button of the stash panel, after the calling tool has returned
	/// (the editor is modal).
	/// </summary>
	internal void AutomationOpenCharacterEditor()
	{
		var edit = typeof(StashPanel).GetMethod("EditButton_Click", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
			?? throw new InvalidOperationException("StashPanel.EditButton_Click not found");
		this.BeginInvoke(() => edit.Invoke(this.stashPanel, [this.stashPanel, EventArgs.Empty]));
	}
}
