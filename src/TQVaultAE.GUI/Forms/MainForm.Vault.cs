using System.Globalization;
using Microsoft.Extensions.Logging;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.Domain.Actions;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using TQVaultAE.Services;
using TQVaultAE.Application;
using TQVaultAE.Application.DTOs;
using TQVaultAE.GUI.Components.Scaling;

namespace TQVaultAE.GUI.Forms;

public partial class MainForm
{
	// Uses vaultService from MainForm.cs

	/// <summary>
	/// Creates the vault panel
	/// </summary>
	/// <param name="numBags">Number of bags in the vault panel.</param>
	private void CreateVaultPanel(int numBags)
	{
		this.vaultPanel = new VaultPanel(this.DragInfo, numBags, new Size(18, 20), 1, AutoMoveLocation.Vault, this.Services);

		this.vaultPanel.DrawAsGroupBox = false;

		this.vaultPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
		this.vaultPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
		this.vaultPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
		this.vaultPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
		this.vaultPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
		this.vaultPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);

		this.tableLayoutPanelMain.Controls.Add(this.vaultPanel, 0, 1);
		this.vaultPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
		this.vaultPanel.Margin = new Padding(0);
	}

	/// <summary>
	/// Creates the secondary vault panel.  Player panel needs to be created before this.
	/// </summary>
	/// <param name="numBags">Number of bags in the secondary vault panel.</param>
	private void CreateSecondaryVaultPanel(int numBags)
	{
		this.secondaryVaultPanel = new VaultPanel(this.DragInfo, numBags, new Size(18, 20), 1, AutoMoveLocation.SecondaryVault, this.Services);
		this.secondaryVaultPanel.DrawAsGroupBox = false;

		this.secondaryVaultPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
		this.secondaryVaultPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
		this.secondaryVaultPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
		this.secondaryVaultPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
		this.secondaryVaultPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
		this.secondaryVaultPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);

		this.flowLayoutPanelRightPanels.Controls.Add(this.secondaryVaultPanel);
		this.secondaryVaultPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
		this.secondaryVaultPanel.Margin = new Padding(0);
	}


	/// <summary>
	/// The mod a vault is linked to, once the vault is loaded; null otherwise.
	/// </summary>
	private string? LoadedVaultMod(string vaultName)
		=> this.Services.Session.Vaults.TryGetValue(this.Services.GamePathService.GetVaultFile(vaultName), out var vault)
			&& vault.IsValueCreated
				? vault.Value?.Mod
				: null;

	/// <summary>
	/// Gets a list of all available vault files and populates the drop down list.
	/// </summary>
	/// <param name="loadVault">Indicates whether the list will also load the last vault selected.</param>
	private void GetVaultList(bool loadVault)
	{
		// A vault linked to a mod shows it after its name, like the characters.
		this.vaultListComboBox.ItemSuffix ??= this.LoadedVaultMod;
		this.secondaryVaultListComboBox.ItemSuffix ??= this.LoadedVaultMod;

		string[] vaults = this.vaultService.GetVaultList();

		// There was something already selected so we will save it.
		string? currentVault = (this.vaultListComboBox.Items.Count > 0) ? this.vaultListComboBox.SelectedItemText() : VaultService.MAINVAULT;

		// Added by VillageIdiot
		// Clear the list before creating since this function can be called multiple times.
		this.vaultListComboBox.Items.Clear();

		// Maintenance entry first, then Main Vault, without the vault open in the secondary panel.
		string? secondaryVault = this.showSecondaryVault && this.secondaryVaultListComboBox.SelectedItem is not null
			? this.secondaryVaultListComboBox.SelectedItemText()
			: null;
		var choices = this.vaultService.BuildVaultChoices(vaults, Resources.MainFormMaintainVault, secondaryVault);
		this.vaultListComboBox.Items.AddRange(choices.ToArray<object>());

		// The last loaded vault when it is set to load, else the one already selected.
		currentVault = this.vaultService.ResolveVaultToLoad(currentVault ?? VaultService.MAINVAULT);

		if (loadVault)
		{
			this.vaultListComboBox.SelectedItem = currentVault;

			// Finally load Vault
			this.LoadVault(currentVault, false);
		}
	}

	/// <summary>
	/// Reads the list from the main vault combo box.
	/// To support adding another vault panel to the screen.
	/// </summary>
	private void GetSecondaryVaultList()
	{

		// There was something already selected so we will save it.
		string? currentVault = (this.secondaryVaultListComboBox.Items.Count > 0) ? this.secondaryVaultListComboBox.SelectedItemText() : Resources.MainFormSelectVault;

		if (currentVault == this.vaultListComboBox.SelectedItemText())
			// Clear the selection if it is already loaded on the main panel.
			currentVault = Resources.MainFormSelectVault;

		// Clear the list before creating since this function can be called multiple times.
		this.secondaryVaultListComboBox.Items.Clear();

		// The main list without its maintenance entry and without the vault open in the main panel.
		var mainVaults = this.vaultListComboBox.Items.Cast<object>().Skip(1).Select(item => item.ToString() ?? string.Empty);
		string? mainVault = this.vaultListComboBox.SelectedIndex > 0 ? this.vaultListComboBox.SelectedItemText() : null;
		var choices = this.vaultService.BuildVaultChoices(mainVaults, Resources.MainFormSelectVault, mainVault, alwaysIncludeMainVault: false);
		this.secondaryVaultListComboBox.Items.AddRange(choices.ToArray<object>());

		this.secondaryVaultListComboBox.SelectedItem = currentVault;

		// Finally load Vault
		this.LoadVault(currentVault, true);
	}

	/// <summary>
	/// Loads a vault file
	/// </summary>
	/// <param name="vaultName">Name of the vault</param>
	/// <param name="secondaryVault">flag indicating whether this selection is for the secondary panel</param>
	private void LoadVault(string vaultName, bool secondaryVault)
	{
		PlayerCollection? vault = null;
		if (secondaryVault && vaultName == Resources.MainFormSelectVault)
		{
			if (this.secondaryVaultPanel.Player is not null)
				this.secondaryVaultPanel.Player = null;
		}
		else
		{
			// Use service
			var result = this.vaultService.LoadVault(vaultName);

			if (result.ArgumentException is not null)
			{
				string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, result.Filename, result.ArgumentException.Message);
				this.Services.MessageBoxes.Inform(msg, Resources.GlobalError, MessageBoxIcon.Error);
			}

			vault = result.Vault;
		}

		// Now assign the vault to the vaultpanel
		if (secondaryVault)
			this.secondaryVaultPanel.Player = vault;
		else
			this.vaultPanel.Player = vault;
	}

	/// <summary>
	/// Gets the vaults the main window knows: those of both drop downs and both vault panels,
	/// new vaults not saved yet included (they have no file).
	/// </summary>
	internal IEnumerable<string> KnownVaultNames
		=> this.vaultListComboBox.Items.Cast<object>().Skip(1)
			.Concat(this.secondaryVaultListComboBox.Items.Cast<object>().Skip(1))
			.Select(item => item.ToString() ?? string.Empty)
			.Append(this.vaultPanel.Player?.PlayerName ?? string.Empty)
			.Append(this.secondaryVaultPanel.Player?.PlayerName ?? string.Empty)
			.Where(name => name.Length > 0)
			.Distinct(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Asks before deleting a vault: its name, its items and bags, and whether the file goes to the Recycle Bin.
	/// No is the default button. "Suppress warnings" skips the question, as for the other deletions.
	/// </summary>
	/// <returns>Whether to delete the vault.</returns>
	private bool ConfirmVaultDelete(string vaultName)
		=> this.USettings.SuppressWarnings
			|| this.Services.MessageBoxes.Confirm(
				VaultDeleteText.Build(vaultName, this.vaultService.GetDeleteInfo(vaultName)),
				Resources.VaultDelete,
				MessageBoxIcon.Warning,
				defaultNo: true);

	/// <summary>
	/// Method for the maintain vault files dialog
	/// </summary>
	private void MaintainVaultFilesDialog()
	{
		try
		{
			// Saves all modifications first: the dialog and the delete confirmation are modal, so nothing changes until
			// the vault is copied, deleted or renamed below.
			this.SaveAllModifiedFiles();

			var dlg = this.Services.Forms.Create<VaultMaintenanceDialog>();
			using (ZoomScope.Enter())
				dlg.Scale(new SizeF(UIService.Zoom, UIService.Zoom));

			if (dlg.ShowDialog() == DialogResult.OK)
			{
				string? newName = dlg.Target;
				string? oldName = dlg.Source;
				bool handled = false;

				// Deleting a vault asks first, with what it holds and whether the file can be restored.
				bool deleteConfirmed = dlg.Action != VaultMaintenance.Delete || oldName is null || this.ConfirmVaultDelete(oldName);

				// A vault deleted or renamed while the secondary panel shows it: clear that panel first, else
				// it would load the vault again under its old name, as a new empty vault saved on the next change.
				if (dlg.Action is VaultMaintenance.Delete or VaultMaintenance.Rename
					&& deleteConfirmed
					&& oldName is not null
					&& oldName.Equals(this.secondaryVaultPanel.Player?.PlayerName, StringComparison.OrdinalIgnoreCase)
					&& this.secondaryVaultListComboBox.Items.Count > 0)
				{
					this.secondaryVaultListComboBox.SelectedIndex = 0;
				}

				// Create a new vault?
				if (dlg.Action == VaultMaintenance.New && newName is not null)
				{
					// Add the name to the list
					this.vaultListComboBox.Items.Add(newName);

					// Select it
					this.vaultListComboBox.SelectedItem = newName;

					// Load it
					this.LoadVault(newName, false);
					handled = true;
				}
				else if (dlg.Action == VaultMaintenance.Copy && newName is not null && oldName is not null)
				{
					if (this.vaultService.CopyVault(oldName, newName))
					{
						// Add the new name to the list
						this.vaultListComboBox.Items.Add(newName);

						// Select the new name
						this.vaultListComboBox.SelectedItem = newName;

						// Load the new file.
						this.LoadVault(newName, false);
						handled = true;
					}
				}
				else if (dlg.Action == VaultMaintenance.Delete && oldName is not null && deleteConfirmed)
				{
					this.vaultService.DeleteVault(oldName);

					// Remove the deleted file from the list.
					this.vaultListComboBox.Items.Remove(oldName);

					// Select the Main Vault since we know it's still there.
					this.vaultListComboBox.SelectedIndex = 1;

					handled = true;
				}
				else if (dlg.Action == VaultMaintenance.Rename && newName is not null && oldName is not null)
				{
					if (this.vaultService.RenameVault(oldName, newName))
					{
						// Get rid of the old name from the list
						this.vaultListComboBox.Items.Remove(oldName);

						// If we renamed something to main vault we need to remove it,
						// since the list always contains Main Vault.
						if (newName == VaultService.MAINVAULT)
							this.vaultListComboBox.Items.Remove(newName);

						// Add the new name to the list
						this.vaultListComboBox.Items.Add(newName);

						// Select the new name
						this.vaultListComboBox.SelectedItem = newName;

						// Load the new file.
						this.LoadVault(newName, false);
						handled = true;
					}
				}

				if ((newName is null && oldName is null) || !handled)
				{
					// put the vault back to what it was
					if (this.vaultPanel.Player is not null)
					{
						this.vaultListComboBox.SelectedItem = this.vaultPanel.Player.PlayerName;
					}
				}
			}
			else
			{
				// put the vault back to what it was
				if (this.vaultPanel.Player is not null)
				{
					this.vaultListComboBox.SelectedItem = this.vaultPanel.Player.PlayerName;
				}
			}
		}
		catch (IOException exception)
		{
			Log.LogError(exception, "Vault maintenance failed (shown vault {Vault})", this.vaultPanel.Player?.PlayerName);
			this.Services.MessageBoxes.ShowException(exception, icon: MessageBoxIcon.None);
		}
	}

	/// <summary>
	/// Handler for changing the vault list drop down selection
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void VaultListComboBoxSelectedIndexChanged(object? sender, EventArgs e)
	{
		try
		{
			if (this.vaultListComboBox.SelectedIndex == 0)
			{
				this.MaintainVaultFilesDialog();
			}
			else
			{
				string? vaultName = this.vaultListComboBox.SelectedItemText();
				if (this.vaultPanel.Player is null || !vaultName.Equals(this.vaultPanel.Player.PlayerName, StringComparison.Ordinal))
				{
					this.LoadVault(vaultName, false);
				}
			}

			if (this.showSecondaryVault)
			{
				this.GetSecondaryVaultList();
			}
		}
		catch (IOException exception)
		{
			Log.LogError(exception, "Failed to load the vault {Vault}", this.vaultListComboBox.SelectedItemText());
			this.Services.MessageBoxes.ShowException(exception, icon: MessageBoxIcon.None);

			// put the vault back to what it was
			if (this.vaultPanel.Player is not null)
			{
				this.vaultListComboBox.SelectedItem = this.vaultPanel.Player.PlayerName;
			}
		}
	}


	/// <summary>
	/// Handler for changing the secondary vault list drop down selection.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void SecondaryVaultListComboBoxSelectedIndexChanged(object? sender, EventArgs e)
	{
		try
		{
			if (this.secondaryVaultListComboBox.SelectedIndex == 0)
			{
				// Clear the vault panel.
				if (this.secondaryVaultPanel.Player is not null)
				{
					this.secondaryVaultPanel.Player = null;
				}
			}
			else
			{
				string? vaultName = this.secondaryVaultListComboBox.SelectedItemText();
				if (this.secondaryVaultPanel.Player is null || !vaultName.Equals(this.secondaryVaultPanel.Player.PlayerName, StringComparison.Ordinal))
				{
					this.LoadVault(vaultName, true);
				}
			}
		}
		catch (IOException exception)
		{
			Log.LogError(exception, "Failed to load the secondary vault {Vault}", this.secondaryVaultListComboBox.SelectedItemText());
			this.Services.MessageBoxes.ShowException(exception, icon: MessageBoxIcon.None);

			// put the vault back to what it was
			if (this.secondaryVaultPanel.Player is not null)
			{
				this.secondaryVaultListComboBox.SelectedItem = this.secondaryVaultPanel.Player.PlayerName;
			}
		}
	}
}
