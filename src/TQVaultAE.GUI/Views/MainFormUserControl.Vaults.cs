using Microsoft.Extensions.Logging;
using System.Globalization;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.Presentation;
using TQVaultAE.Services;

namespace TQVaultAE.GUI.Views;

/// <summary>
/// The vault lists of the main content: the main vault list with its maintenance entry, and the secondary vault list
/// shown in place of the character list.
/// </summary>
public partial class MainFormUserControl
{
	/// <summary>
	/// Raised before the vault maintenance dialog opens: the modified files are saved first.
	/// </summary>
	internal event EventHandler? SaveRequested;

	/// <summary>
	/// The vault selected in the main vault list; null when none is.
	/// </summary>
	internal string? SelectedVaultName
		=> this.vaultListComboBox.SelectedItem is not null ? this.vaultListComboBox.SelectedItemText() : null;

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
	internal void GetVaultList(bool loadVault)
	{
		var vaultService = this.Services.VaultService;

		// A vault linked to a mod shows it after its name, like the characters.
		this.vaultListComboBox.ItemSuffix ??= this.LoadedVaultMod;
		this.secondaryVaultListComboBox.ItemSuffix ??= this.LoadedVaultMod;

		string[] vaults = vaultService.GetVaultList();

		// There was something already selected so we will save it.
		string? currentVault = (this.vaultListComboBox.Items.Count > 0) ? this.vaultListComboBox.SelectedItemText() : IVaultService.MainVault;

		// Added by VillageIdiot
		// Clear the list before creating since this function can be called multiple times.
		this.vaultListComboBox.Items.Clear();

		// Maintenance entry first, then Main Vault, without the vault open in the secondary panel.
		string? secondaryVault = this.showSecondaryVault && this.secondaryVaultListComboBox.SelectedItem is not null
			? this.secondaryVaultListComboBox.SelectedItemText()
			: null;
		var choices = vaultService.BuildVaultChoices(vaults, Resources.MainFormMaintainVault, secondaryVault);
		this.vaultListComboBox.Items.AddRange(choices.ToArray<object>());

		// The last loaded vault when it is set to load, else the one already selected.
		currentVault = vaultService.ResolveVaultToLoad(currentVault ?? IVaultService.MainVault);

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
		var choices = this.Services.VaultService.BuildVaultChoices(mainVaults, Resources.MainFormSelectVault, mainVault, alwaysIncludeMainVault: false);
		this.secondaryVaultListComboBox.Items.AddRange(choices.ToArray<object>());

		this.secondaryVaultListComboBox.SelectedItem = currentVault;

		// Finally load Vault
		this.LoadVault(currentVault, true);
	}

	/// <summary>
	/// Selects a vault in the main vault list and loads it.
	/// </summary>
	internal void SelectVault(string vaultName)
	{
		this.vaultListComboBox.SelectedItem = vaultName;

		// Finally load Vault
		this.LoadVault(vaultName, false);
	}

	/// <summary>
	/// Loads a vault file
	/// </summary>
	/// <param name="vaultName">Name of the vault</param>
	/// <param name="secondaryVault">flag indicating whether this selection is for the secondary panel</param>
	internal void LoadVault(string vaultName, bool secondaryVault)
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
			var result = this.Services.VaultService.LoadVault(vaultName);

			// A format error keeps the vault (partly read); any other error leaves no vault to show.
			if ((result.ArgumentException as Exception ?? result.Error) is { } error)
			{
				this.Services.MessageBoxes.Inform(LoadMessages.ReadFailure(result.Filename, error.Message), Resources.GlobalError, MessageBoxIcon.Error);
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
	/// Gets the vaults the main content knows: those of both drop downs and both vault panels,
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
	/// Moves the selection of the main vault list by one vault per wheel notch, never onto its maintenance entry.
	/// </summary>
	/// <param name="delta">The wheel's delta: up selects the vault above.</param>
	internal void ScrollVaultList(int delta)
	{
		// Force a single line regardless of the delta value.
		int numberOfTextLinesToMove = ((delta > 0) ? 1 : 0) - ((delta < 0) ? 1 : 0);
		if (numberOfTextLinesToMove != 0)
		{
			int vaultSelection = this.vaultListComboBox.SelectedIndex;
			vaultSelection -= numberOfTextLinesToMove;
			if (vaultSelection < 1)
				vaultSelection = 1;

			if (vaultSelection >= this.vaultListComboBox.Items.Count)
				vaultSelection = this.vaultListComboBox.Items.Count - 1;

			this.vaultListComboBox.SelectedIndex = vaultSelection;
		}
	}

	/// <summary>
	/// Asks before deleting a vault: its name, its items and bags, and whether the file goes to the Recycle Bin.
	/// No is the default button. "Suppress warnings" skips the question, as for the other deletions.
	/// </summary>
	/// <returns>Whether to delete the vault.</returns>
	private bool ConfirmVaultDelete(string vaultName)
		=> this.Services.Settings.SuppressWarnings
			|| this.Services.MessageBoxes.Confirm(
				VaultDeleteText.Build(vaultName, this.Services.VaultService.GetDeleteInfo(vaultName)),
				Resources.VaultDelete,
				MessageBoxIcon.Warning,
				defaultNo: true);

	/// <summary>
	/// Method for the maintain vault files dialog
	/// </summary>
	internal void MaintainVaultFilesDialog()
	{
		var vaultService = this.Services.VaultService;
		try
		{
			// Saves all modifications first: the dialog and the delete confirmation are modal, so nothing changes until
			// the vault is copied, deleted or renamed below.
			this.SaveRequested?.Invoke(this, EventArgs.Empty);

			using var dlg = this.Services.Forms.Create<VaultMaintenanceDialog>();
			using (ZoomScope.Enter())
				dlg.Scale(new SizeF(this.Services.UIService.Zoom, this.Services.UIService.Zoom));

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
					if (vaultService.CopyVault(oldName, newName))
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
					vaultService.DeleteVault(oldName);

					// Remove the deleted file from the list.
					this.vaultListComboBox.Items.Remove(oldName);

					// Select the Main Vault since we know it's still there.
					this.vaultListComboBox.SelectedIndex = 1;

					handled = true;
				}
				else if (dlg.Action == VaultMaintenance.Rename && newName is not null && oldName is not null)
				{
					if (vaultService.RenameVault(oldName, newName))
					{
						// Get rid of the old name from the list
						this.vaultListComboBox.Items.Remove(oldName);

						// If we renamed something to main vault we need to remove it,
						// since the list always contains Main Vault.
						if (newName == IVaultService.MainVault)
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
