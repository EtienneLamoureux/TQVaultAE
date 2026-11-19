//-----------------------------------------------------------------------
// <copyright file="MainForm.Automation.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.GUI.Automation;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.GUI.Models;

namespace TQVaultAE.GUI.Forms;

/// <summary>
/// Read access to the main form state for the automation endpoint.
/// </summary>
public partial class MainForm
{
	internal ItemDragInfo AutomationDragInfo => this.mainView.DragInfo;

	/// <summary>Opens a comparison window like the user's Compare (<see cref="Views.MainFormUserControl.OpenComparison"/>).</summary>
	internal Dialogs.ItemComparisonDialog AutomationOpenComparison(TQVaultAE.Domain.Entities.Item first, TQVaultAE.Domain.Entities.Item second)
		=> this.mainView.OpenComparison(first, second);

	/// <summary>The comparison windows open, oldest first.</summary>
	internal IReadOnlyList<Dialogs.ItemComparisonDialog> AutomationComparisons => this.mainView.OpenComparisons;

	/// <summary>The floating character windows open, oldest first.</summary>
	internal IReadOnlyList<Dialogs.CharacterWindow> AutomationCharacterWindows => this.mainView.CharacterWindows;

	internal UserSettings AutomationSettings => this.USettings;

	/// <summary>
	/// The view shown under the toolbar: <c>main</c> (the main content), <c>settings</c> or <c>characterEditor</c>.
	/// </summary>
	internal string AutomationShownViewName => this.shownView switch
	{
		null => "main",
		Views.SettingsUserControl => "settings",
		Views.CharacterEditorUserControl => "characterEditor",
		var other => other.Name,
	};

	/// <summary>
	/// The view shown in place of the main content (the settings or the character editor), when <paramref name="name"/>
	/// is part of its title or its type name, as the tools find a dialog; null otherwise.
	/// </summary>
	internal Control? AutomationShownView(string name)
		=> this.shownView is { } view
			&& (view.Text.Contains(name, StringComparison.OrdinalIgnoreCase) || string.Equals(view.GetType().Name, name, StringComparison.OrdinalIgnoreCase))
			? view
			: null;

	internal string? AutomationSelectedVault => this.mainView.vaultListComboBox?.SelectedItem?.ToString();

	/// <summary>Shows a search result like a click on its row in the results dialog.</summary>
	internal void AutomationSelectResult(TQVaultAE.Application.Results.SearchResult result) => this.mainView.ShowSearchResult(result);

	/// <summary>
	/// Reads the menu of the vault tools button (next to the vault list) as it opens; with <paramref name="path"/>,
	/// clicks that entry after the calling tool has returned.
	/// </summary>
	internal (IReadOnlyList<AutomationMenuEntry> Entries, string? Clicked) AutomationVaultMenu(IReadOnlyList<string>? path)
	{
		this.mainView.PrepareVaultMenu();
		var entries = AutomationMenus.Describe(this.mainView.VaultMenu.Items);
		string? clicked = path is { Count: > 0 } ? AutomationMenus.ClickLater(this, this.mainView.VaultMenu, path) : null;
		return (entries, clicked);
	}

	/// <summary>Draws the menu of the vault tools button as it opens, with the sub menus along <paramref name="open"/>.</summary>
	internal Bitmap AutomationRenderVaultMenu(IReadOnlyList<string> open)
	{
		this.mainView.PrepareVaultMenu();
		return AutomationMenus.Render(this.mainView.VaultMenu, open);
	}

	/// <summary>The mod the shown vault is linked to; null for none.</summary>
	internal string? AutomationSelectedVaultMod => this.mainView.VaultPanel?.Vault?.Mod;

	internal string? AutomationSelectedSecondaryVault
		=> this.mainView.SecondaryVaultShown ? this.mainView.secondaryVaultListComboBox?.SelectedItem?.ToString() : null;

	internal string? AutomationSelectedCharacter => this.mainView.comboBoxCharacter?.SelectedItem?.ToString();

	internal IReadOnlyList<string> AutomationCharacters
		=> this.mainView.comboBoxCharacter?.Items.Select(i => i?.ToString() ?? string.Empty).ToList() ?? [];

	internal IReadOnlyList<string> AutomationVaults
		=> this.mainView.vaultListComboBox?.Items.Cast<object>().Select(i => i?.ToString() ?? string.Empty).ToList() ?? [];

	/// <summary>
	/// Selects a character like the user would in the drop down, which loads the player and its stashes.
	/// </summary>
	/// <returns>false when no character has that name.</returns>
	internal bool AutomationSelectCharacter(string name)
	{
		var item = this.mainView.comboBoxCharacter.Items.FirstOrDefault(i => string.Equals(i?.ToString(), name, StringComparison.OrdinalIgnoreCase));
		if (item is null)
			return false;

		this.mainView.comboBoxCharacter.SelectedItem = item;
		return true;
	}

	/// <summary>
	/// Selects a vault in the main vault drop down, which loads it.
	/// </summary>
	/// <returns>false when no vault has that name.</returns>
	internal bool AutomationSelectVault(string name)
	{
		var item = this.mainView.vaultListComboBox.Items.Cast<object>().FirstOrDefault(i => string.Equals(i?.ToString(), name, StringComparison.OrdinalIgnoreCase));
		if (item is null)
			return false;

		// The first entry opens the modal maintenance dialog: select it after the calling tool has returned.
		if (this.mainView.vaultListComboBox.Items.IndexOf(item) == 0)
			this.BeginInvoke(() => this.mainView.vaultListComboBox.SelectedItem = item);
		else
			this.mainView.vaultListComboBox.SelectedItem = item;
		return true;
	}

	/// <summary>
	/// Exports the vault shown to a JSON file, like "Export to file" without its file dialog.
	/// </summary>
	/// <returns>The name of the exported vault, or null when no vault is shown.</returns>
	internal string? AutomationExportVault(string fileName)
	{
		var vault = this.mainView.VaultPanel.Player;
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
		var vault = this.mainView.VaultPanel.Player;
		if (vault is null)
			return null;

		File.WriteAllText(fileName, this.mainView.VaultPanel.BuildCsvExport(bagIndex));
		return vault.PlayerName;
	}

	/// <summary>
	/// Imports an exported file, like "Import from file" without its file dialog.
	/// </summary>
	internal void AutomationImportFile(string fileName) => this.mainView.ImportFile(fileName);

	/// <summary>
	/// Imports text like a paste (Ctrl+V) of the clipboard holding it, without touching the clipboard: the same parsing
	/// (exported JSON, or a PasteBin link) and the same placement as <see cref="Views.MainFormUserControl.PasteFromClipboardAsync"/>.
	/// </summary>
	/// <remarks>
	/// Called from the request thread, not the UI thread: a PasteBin link is downloaded, and the UI thread stays free
	/// meanwhile. The result is then placed on the UI thread with <see cref="AutomationHandleImportResult"/>.
	/// </remarks>
	internal Task<ImportResult> AutomationParseImportTextAsync(string text) => this.itemExchangeService.ImportFromTextAsync(text);

	/// <summary>Places an import's items like the paste does, on the UI thread.</summary>
	/// <returns>The import's error, or null when it succeeded.</returns>
	internal string? AutomationHandleImportResult(ImportResult result)
	{
		this.mainView.HandleImportResult(result);
		return result.Success ? null : result.ErrorMessage;
	}

	internal string? AutomationCurrentPlayerFile => this.userContext?.CurrentPlayer?.PlayerFile;

	/// <summary>The character shown, as loaded.</summary>
	internal TQVaultAE.Application.Session.PlayerCollection? AutomationCurrentPlayer => this.userContext?.CurrentPlayer;

	internal bool AutomationLoadingComplete => this.loadingComplete;

	internal VaultPanel? AutomationVaultPanel => this.mainView.VaultPanel;

	internal VaultPanel? AutomationSecondaryVaultPanel => this.mainView.SecondaryVaultPanel;

	internal PlayerPanel? AutomationPlayerPanel => this.mainView.PlayerPanel;

	internal StashPanel? AutomationStashPanel => this.mainView.StashPanel;

	internal ForgePanel? AutomationForgePanel => this.mainView.ForgePanel;

	internal bool AutomationForgeShown => this.mainView.ForgePanel?.Visible == true;

	/// <summary>Text of the notification bar, which shows the name of the item under the cursor.</summary>
	internal string AutomationNotificationText => this.mainView.NotificationText.Text;

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
		if (!this.mainView.highlightFilters.Visible)
			this.mainView.ToggleHighlightFilters();
		return this.mainView.highlightFilters;
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

		this.mainView.vaultListComboBox.Items.Add(name);
		this.mainView.SelectVault(name);
		return VaultMaintenanceError.None;
	}

	/// <summary>
	/// Saves the modified characters, vaults and stashes like the Save button, file errors included
	/// (the Abort/Retry/Ignore box). Returns whether characters were saved.
	/// </summary>
	internal bool AutomationSaveFiles() => this.SaveAllModifiedFiles();

	/// <summary>The character drop down, whose context menu holds the tag boxes.</summary>
	internal ComboBoxCharacter AutomationCharacterCombo => this.mainView.comboBoxCharacter;

	/// <summary>
	/// Shows the character editor in place of the main content like the Edit button of the stash panel (its click
	/// handler), before the calling tool returns.
	/// </summary>
	internal void AutomationOpenCharacterEditor() => this.mainView.StashPanel.AutomationClickEdit();
}
