using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
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
		private StashService stashService = null;

		/// <summary>
		/// Creates the stash panel
		/// </summary>
		private void CreateStashPanel()
		{
			// size params are width, height
			Size panelSize = new Size(17, 16);

			this.stashPanel = new StashPanel(this.DragInfo, panelSize);

			// New location in bottom right of the Main Form.
			//Align to playerPanel
			this.stashPanel.Location = new Point(
				this.playerPanel.Location.X,
				this.ClientSize.Height - (this.stashPanel.Height + Convert.ToInt32(16.0F * UIService.UI.Scale)));
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
			InitStashService();

			var result = this.stashService.LoadTransferStash();

			// Get the transfer stash
			try
			{
				if (result.StashPresent.HasValue && !result.StashPresent.Value)
				{
					MessageBox.Show(Resources.StashNotFoundMsg, Resources.StashNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				if (result.ArgumentException != null)
				{
					string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, result.TransferStashFile, result.ArgumentException.Message);
					MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				this.stashPanel.TransferStash = result.Stash;
			}
			catch (IOException exception)
			{
				string msg = string.Format(CultureInfo.InvariantCulture, Resources.MainFormReadError, result.TransferStashFile, exception.ToString());
				MessageBox.Show(msg, Resources.MainFormStashReadError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				Log.Error(msg, exception);
				this.stashPanel.TransferStash = null;
			}
		}

		private void InitStashService()
		{
			if (this.stashService is null) this.stashService = new StashService(userContext);
		}


		/// <summary>
		/// Loads the relic vault stash
		/// </summary>
		private void LoadRelicVaultStash()
		{
			InitStashService();

			var result = this.stashService.LoadRelicVaultStash();

			// Get the relic vault stash
			try
			{
				if (result.StashPresent.HasValue && !result.StashPresent.Value)
				{
					MessageBox.Show(Resources.StashNotFoundMsg, Resources.StashNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				if (result.ArgumentException != null)
				{
					string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, result.RelicVaultStashFile, result.ArgumentException.Message);
					MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				this.stashPanel.RelicVaultStash = result.Stash;
			}
			catch (IOException exception)
			{
				string msg = string.Format(CultureInfo.InvariantCulture, Resources.MainFormReadError, result.RelicVaultStashFile, exception.ToString());
				MessageBox.Show(msg, Resources.MainFormStashReadError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);

				this.stashPanel.RelicVaultStash = null;
			}
		}

		/// <summary>
		/// Attempts to save all modified stash files.
		/// </summary>
		private void SaveAllModifiedStashes()
		{
		retry:
			Stash stashOnError = null;
			try
			{
				this.stashService.SaveAllModifiedStashes(ref stashOnError);
			}
			catch (IOException exception)
			{
				string title = string.Format(CultureInfo.InvariantCulture, Resources.MainFormSaveError, stashOnError.PlayerName);
				Log.Error(title, exception);

				switch (MessageBox.Show(Log.FormatException(exception), title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, RightToLeftOptions))
				{
					case DialogResult.Abort:
						// rethrow the exception
						throw;
					case DialogResult.Retry:
						goto retry;
				}
			}
		}
	}
}
