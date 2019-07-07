using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using TQVaultAE.Data;
using TQVaultAE.Entities;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using TQVaultAE.Logs;
using TQVaultAE.Services;

namespace TQVaultAE.GUI
{
	public partial class MainForm
	{
		private VaultService vaultService = null;

		/// <summary>
		/// Creates the vault panel
		/// </summary>
		/// <param name="numBags">Number of bags in the vault panel.</param>
		private void CreateVaultPanel(int numBags)
		{
			this.vaultPanel = new VaultPanel(this.DragInfo, numBags, new Size(18, 20), 1, AutoMoveLocation.Vault);

			int locationY = this.vaultListComboBox.Location.Y + Convert.ToInt32(28.0F * UIService.UI.Scale);
			this.vaultPanel.DrawAsGroupBox = false;

			this.vaultPanel.Location = new Point(Convert.ToInt32(22.0F * UIService.UI.Scale), locationY);
			this.vaultPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
			this.vaultPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
			this.vaultPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
			this.vaultPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
			this.vaultPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
			this.vaultPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);
			Controls.Add(this.vaultPanel);
		}

		/// <summary>
		/// Creates the secondary vault panel.  Player panel needs to be created before this.
		/// </summary>
		/// <param name="numBags">Number of bags in the secondary vault panel.</param>
		private void CreateSecondaryVaultPanel(int numBags)
		{
			this.secondaryVaultPanel = new VaultPanel(this.DragInfo, numBags, new Size(18, 20), 1, AutoMoveLocation.SecondaryVault);
			this.secondaryVaultPanel.DrawAsGroupBox = false;

			// Place it with the same Y value as the character panel and X value of the vault panel.
			this.secondaryVaultPanel.Location = new Point(
				this.ClientSize.Width - (this.secondaryVaultPanel.Width + Convert.ToInt32(49.0F * UIService.UI.Scale)),
				this.vaultPanel.Location.Y);

			this.secondaryVaultPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
			this.secondaryVaultPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
			this.secondaryVaultPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
			this.secondaryVaultPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
			this.secondaryVaultPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
			this.secondaryVaultPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);
			Controls.Add(this.secondaryVaultPanel);
		}


		/// <summary>
		/// Gets a list of all available vault files and populates the drop down list.
		/// </summary>
		/// <param name="loadVault">Indicates whether the list will also load the last vault selected.</param>
		private void GetVaultList(bool loadVault)
		{
			if (this.vaultService is null) this.vaultService = new VaultService(userContext);

			string[] vaults = TQData.GetVaultList();

			// Added by VillageIdiot
			// See if the Vault path was set during GetVaultList and update the key accordingly
			if (TQData.VaultFolderChanged)
			{
				this.vaultService.UpdateVaultPath(TQData.TQVaultSaveFolder);
			}

			string currentVault;

			// There was something already selected so we will save it.
			if (this.vaultListComboBox.Items.Count > 0)
			{
				currentVault = this.vaultListComboBox.SelectedItem.ToString();
			}
			else
			{
				currentVault = VaultService.MAINVAULT;
			}

			// Added by VillageIdiot
			// Clear the list before creating since this function can be called multiple times.
			this.vaultListComboBox.Items.Clear();

			this.vaultListComboBox.Items.Add(Resources.MainFormMaintainVault);

			// Add Main Vault first
			if (this.secondaryVaultListComboBox.SelectedItem == null || this.secondaryVaultListComboBox.SelectedItem.ToString() != VaultService.MAINVAULT)
			{
				this.vaultListComboBox.Items.Add(VaultService.MAINVAULT);
			}

			if ((vaults?.Length ?? 0) > 0)
			{
				// now add everything EXCEPT for main vault
				foreach (string vault in vaults)
				{
					if (!vault.Equals(VaultService.MAINVAULT))
					{
						// we already added main vault
						if (this.secondaryVaultListComboBox.SelectedItem != null && vault.Equals(this.secondaryVaultListComboBox.SelectedItem.ToString()) && this.showSecondaryVault)
						{
							break;
						}

						this.vaultListComboBox.Items.Add(vault);
					}
				}
			}

			// See if we should load the last loaded vault
			if (Config.Settings.Default.LoadLastVault)
			{
				currentVault = Config.Settings.Default.LastVaultName;

				// Make sure there is something in the config file to load else load the Main Vault
				// We do not want to create new here.
				if (string.IsNullOrEmpty(currentVault) || !File.Exists(TQData.GetVaultFile(currentVault)))
				{
					currentVault = VaultService.MAINVAULT;
				}
			}

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
			string currentVault;

			// There was something already selected so we will save it.
			if (this.secondaryVaultListComboBox.Items.Count > 0)
			{
				currentVault = this.secondaryVaultListComboBox.SelectedItem.ToString();
			}
			else
			{
				currentVault = Resources.MainFormSelectVault;
			}

			if (currentVault == this.vaultListComboBox.SelectedItem.ToString())
			{
				// Clear the selection if it is already loaded on the main panel.
				currentVault = Resources.MainFormSelectVault;
			}

			// Clear the list before creating since this function can be called multiple times.
			this.secondaryVaultListComboBox.Items.Clear();
			this.secondaryVaultListComboBox.Items.Add(Resources.MainFormSelectVault);

			if (this.vaultListComboBox.Items.Count > 1)
			{
				// Now add everything EXCEPT for the selected vault in the other panel.
				for (int i = 1; i < this.vaultListComboBox.Items.Count; ++i)
				{ // Skip over the maintenance selection.
					if (i != this.vaultListComboBox.SelectedIndex)
					{ // Skip over the selected item.
						this.secondaryVaultListComboBox.Items.Add(this.vaultListComboBox.Items[i]);
					}
				}
			}

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
			PlayerCollection vault = null;
			if (secondaryVault && vaultName == Resources.MainFormSelectVault)
			{
				if (this.secondaryVaultPanel.Player != null)
				{
					this.secondaryVaultPanel.Player = null;
				}
			}
			else
			{
				var result = this.vaultService.LoadVault(vaultName);

				if (result.ArgumentException != null)
				{
					string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, result.Filename, result.ArgumentException.Message);
					MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				vault = result.Vault;
			}

			// Now assign the vault to the vaultpanel
			if (secondaryVault)
			{
				this.secondaryVaultPanel.Player = vault;
			}
			else
			{
				this.vaultPanel.Player = vault;
			}
		}

		/// <summary>
		/// Method for the maintain vault files dialog
		/// </summary>
		private void MaintainVaultFilesDialog()
		{
			try
			{
				this.SaveAllModifiedFiles();
				VaultMaintenanceDialog dlg = new VaultMaintenanceDialog();
				dlg.Scale(new SizeF(UIService.UI.Scale, UIService.UI.Scale));

				if (dlg.ShowDialog() == DialogResult.OK)
				{
					string newName = dlg.Target;
					string oldName = dlg.Source;
					bool handled = false;

					// Create a new vault?
					if (dlg.Action == VaultMaintenanceDialog.VaultMaintenance.New && newName != null)
					{
						// Add the name to the list
						this.vaultListComboBox.Items.Add(newName);

						// Select it
						this.vaultListComboBox.SelectedItem = newName;

						// Load it
						this.LoadVault(newName, false);
						handled = true;
					}
					else if (dlg.Action == VaultMaintenanceDialog.VaultMaintenance.Copy && newName != null && oldName != null)
					{
						string oldFilename = TQData.GetVaultFile(oldName);
						string newFilename = TQData.GetVaultFile(newName);

						// Make sure we save all modifications first.
						this.SaveAllModifiedFiles();

						// Make sure the vault file to copy exists and the new name does not.
						if (File.Exists(oldFilename) && !File.Exists(newFilename))
						{
							File.Copy(oldFilename, newFilename);

							// Add the new name to the list
							this.vaultListComboBox.Items.Add(newName);

							// Select the new name
							this.vaultListComboBox.SelectedItem = newName;

							// Load the new file.
							this.LoadVault(newName, false);
							handled = true;
						}
					}
					else if (dlg.Action == VaultMaintenanceDialog.VaultMaintenance.Delete && oldName != null)
					{
						string filename = TQData.GetVaultFile(oldName);

						// Make sure we save all modifications first.
						this.SaveAllModifiedFiles();

						// Make sure the vault file to delete exists.
						if (File.Exists(filename))
						{
							File.Delete(filename);
						}

						// Remove the file from the cache.
						userContext.Vaults.Remove(filename);

						// Remove the deleted file from the list.
						this.vaultListComboBox.Items.Remove(oldName);

						// Select the Main Vault since we know it's still there.
						this.vaultListComboBox.SelectedIndex = 1;

						handled = true;
					}
					else if (dlg.Action == VaultMaintenanceDialog.VaultMaintenance.Rename && newName != null && oldName != null)
					{
						string oldFilename = TQData.GetVaultFile(oldName);
						string newFilename = TQData.GetVaultFile(newName);

						// Make sure we save all modifications first.
						this.SaveAllModifiedFiles();

						// Make sure the vault file to rename exists and the new name does not.
						if (File.Exists(oldFilename) && !File.Exists(newFilename))
						{
							File.Move(oldFilename, newFilename);

							// Remove the old vault from the cache.
							userContext.Vaults.Remove(oldFilename);

							// Get rid of the old name from the list
							this.vaultListComboBox.Items.Remove(oldName);

							// If we renamed something to main vault we need to remove it,
							// since the list always contains Main Vault.
							if (newName == VaultService.MAINVAULT)
							{
								userContext.Vaults.Remove(newFilename);
								this.vaultListComboBox.Items.Remove(newName);
							}

							// Add the new name to the list
							this.vaultListComboBox.Items.Add(newName);

							// Select the new name
							this.vaultListComboBox.SelectedItem = newName;

							// Load the new file.
							this.LoadVault(newName, false);
							handled = true;
						}
					}

					if ((newName == null && oldName == null) || !handled)
					{
						// put the vault back to what it was
						if (this.vaultPanel.Player != null)
						{
							this.vaultListComboBox.SelectedItem = this.vaultPanel.Player.PlayerName;
						}
					}
				}
				else
				{
					// put the vault back to what it was
					if (this.vaultPanel.Player != null)
					{
						this.vaultListComboBox.SelectedItem = this.vaultPanel.Player.PlayerName;
					}
				}
			}
			catch (IOException exception)
			{
				Log.ErrorException(exception);
				MessageBox.Show(Log.FormatException(exception), Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, RightToLeftOptions);
			}
		}


		/// <summary>
		/// Attempts to save all modified vault files
		/// </summary>
		private void SaveAllModifiedVaults()
		{
		retry:
			PlayerCollection vaultOnError = null;
			try
			{
				this.vaultService.SaveAllModifiedVaults(ref vaultOnError);
			}
			catch (IOException exception)
			{
				string title = string.Format(CultureInfo.InvariantCulture, Resources.MainFormSaveError, vaultOnError.PlayerName);
				Log.Error(title, exception);
				switch (MessageBox.Show(Log.FormatException(exception), title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, RightToLeftOptions))
				{
					case DialogResult.Abort:
						// rethrow the exception
						throw;
					case DialogResult.Retry:
						// retry
						goto retry;
				}
			}
		}

		/// <summary>
		/// Handler for changing the vault list drop down selection
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void VaultListComboBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (this.vaultListComboBox.SelectedIndex == 0)
				{
					this.MaintainVaultFilesDialog();
				}
				else
				{
					string vaultName = this.vaultListComboBox.SelectedItem.ToString();
					if (this.vaultPanel.Player == null || !vaultName.Equals(this.vaultPanel.Player.PlayerName))
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
				Log.ErrorException(exception);
				MessageBox.Show(Log.FormatException(exception), Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, RightToLeftOptions);

				// put the vault back to what it was
				if (this.vaultPanel.Player != null)
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
		private void SecondaryVaultListComboBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (this.secondaryVaultListComboBox.SelectedIndex == 0)
				{
					// Clear the vault panel.
					if (this.secondaryVaultPanel.Player != null)
					{
						this.secondaryVaultPanel.Player = null;
					}
				}
				else
				{
					string vaultName = this.secondaryVaultListComboBox.SelectedItem.ToString();
					if (this.secondaryVaultPanel.Player == null || !vaultName.Equals(this.secondaryVaultPanel.Player.PlayerName))
					{
						this.LoadVault(vaultName, true);
					}
				}
			}
			catch (IOException exception)
			{
				Log.ErrorException(exception);
				MessageBox.Show(Log.FormatException(exception), Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, RightToLeftOptions);

				// put the vault back to what it was
				if (this.secondaryVaultPanel.Player != null)
				{
					this.secondaryVaultListComboBox.SelectedItem = this.secondaryVaultPanel.Player.PlayerName;
				}
			}
		}
	}
}
