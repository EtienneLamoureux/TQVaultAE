using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TQVaultAE.Data;
using TQVaultAE.Entities;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Properties;
using TQVaultAE.Logs;

namespace TQVaultAE.GUI
{
	internal partial class MainForm
	{

		/// <summary>
		/// Creates the stash panel
		/// </summary>
		private void CreateStashPanel()
		{
			// size params are width, height
			Size panelSize = new Size(17, 16);

			this.stashPanel = new StashPanel(this.dragInfo, panelSize, this.tooltip);

			// New location in bottom right of the Main Form.
			//Align to playerPanel
			this.stashPanel.Location = new Point(
				this.playerPanel.Location.X,
				this.ClientSize.Height - (this.stashPanel.Height + Convert.ToInt32(16.0F * Database.DB.Scale)));
			this.stashPanel.DrawAsGroupBox = false;

			this.stashPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
			this.stashPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
			this.stashPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
			this.stashPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
			this.stashPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
			this.stashPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);
			Controls.Add(this.stashPanel);
		}

		/// <summary>
		/// Loads the transfer stash for immortal throne
		/// </summary>
		private void LoadTransferStash()
		{
			string transferStashFile = TQData.TransferStashFile;

			// Get the transfer stash
			try
			{
				Stash stash;
				try
				{
					stash = this.stashes[transferStashFile];
				}
				catch (KeyNotFoundException)
				{
					bool stashLoaded = false;
					stash = new Stash(Resources.GlobalTransferStash, transferStashFile);
					stash.IsImmortalThrone = true;

					try
					{
						// Throw a message if the stash does not exist.
						bool stashPresent = StashProvider.LoadFile(stash);
						if (!stashPresent)
						{
							MessageBox.Show(Resources.StashNotFoundMsg, Resources.StashNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, RightToLeftOptions);
						}

						stashLoaded = stashPresent;
					}
					catch (ArgumentException argumentException)
					{
						string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, transferStashFile, argumentException.Message);
						MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
						stashLoaded = false;
					}

					if (stashLoaded)
					{
						this.stashes.Add(transferStashFile, stash);
					}
				}

				this.stashPanel.TransferStash = stash;
			}
			catch (IOException exception)
			{
				string msg = string.Format(CultureInfo.InvariantCulture, Resources.MainFormReadError, transferStashFile, exception.ToString());
				MessageBox.Show(msg, Resources.MainFormStashReadError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				Log.Error(msg, exception);
				this.stashPanel.TransferStash = null;
			}
		}


		/// <summary>
		/// Loads the relic vault stash
		/// </summary>
		private void LoadRelicVaultStash()
		{
			string relicVaultStashFile = TQData.RelicVaultStashFile;

			// Get the relic vault stash
			try
			{
				Stash stash;
				try
				{
					stash = this.stashes[relicVaultStashFile];
				}
				catch (KeyNotFoundException)
				{
					bool stashLoaded = false;
					stash = new Stash(Resources.GlobalRelicVaultStash, relicVaultStashFile);
					stash.IsImmortalThrone = true;

					try
					{
						// Throw a message if the stash does not exist.
						bool stashPresent = StashProvider.LoadFile(stash);
						if (!stashPresent)
						{
							MessageBox.Show(Resources.StashNotFoundMsg, Resources.StashNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, RightToLeftOptions);
						}

						stashLoaded = stashPresent;
					}
					catch (ArgumentException argumentException)
					{
						string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, relicVaultStashFile, argumentException.Message);
						MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
						stashLoaded = false;
					}

					if (stashLoaded)
					{
						stash.Sack.StashType = SackType.RelicVaultStash;
						this.stashes.Add(relicVaultStashFile, stash);
					}
				}

				this.stashPanel.RelicVaultStash = stash;
			}
			catch (IOException exception)
			{
				string msg = string.Format(CultureInfo.InvariantCulture, Resources.MainFormReadError, relicVaultStashFile, exception.ToString());
				MessageBox.Show(msg, Resources.MainFormStashReadError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);

				this.stashPanel.RelicVaultStash = null;
			}
		}



		/// <summary>
		/// Attempts to save all modified stash files.
		/// </summary>
		private void SaveAllModifiedStashes()
		{
			// Save each stash as necessary
			foreach (KeyValuePair<string, Stash> kvp in this.stashes)
			{
				string stashFile = kvp.Key;
				Stash stash = kvp.Value;

				if (stash == null)
				{
					continue;
				}

				if (stash.IsModified)
				{
					bool done = false;

					// backup the file
					while (!done)
					{
						try
						{
							TQData.BackupFile(stash.PlayerName, stashFile);
							StashProvider.Save(stash, stashFile);
							done = true;
						}
						catch (IOException exception)
						{
							string title = string.Format(CultureInfo.InvariantCulture, Resources.MainFormSaveError, stash.PlayerName);
							Log.Error(title, exception);
							switch (MessageBox.Show(Log.FormatException(exception), title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, RightToLeftOptions))
							{
								case DialogResult.Abort:
									{
										// rethrow the exception
										throw;
									}

								case DialogResult.Retry:
									{
										// retry
										break;
									}

								case DialogResult.Ignore:
									{
										done = true;
										break;
									}
							}
						}
					}
				}
			}

			return;
		}
	}
}
