using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using TQVaultAE.Logs;
using TQVaultAE.Domain.Contracts.Services;
using Microsoft.Extensions.Logging;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.GUI
{
	public partial class MainForm
	{
		private IStashService stashService = null;

		/// <summary>
		/// Creates the stash panel
		/// </summary>
		private void CreateStashPanel()
		{
			// size params are width, height
			Size panelSize = new Size(17, 16);

			this.stashPanel = new StashPanel(this.DragInfo, panelSize, this.ServiceProvider);

			// New location in bottom right of the Main Form.
			// Align to playerPanel

			this.stashPanel.DrawAsGroupBox = false;

			this.stashPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
			this.stashPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
			this.stashPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
			this.stashPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
			this.stashPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
			this.stashPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);

			this.flowLayoutPanelRightPanels.Controls.Add(this.stashPanel);
			this.stashPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
			this.stashPanel.Margin = new Padding(0);
		}

		/// <summary>
		/// Loads the transfer stash for immortal throne
		/// </summary>
		/// <param name="fromFileWatcher">When <code>true</code> called from <see cref="FileSystemWatcher.Changed"/></param>
		private LoadTransferStashResult LoadTransferStash(bool fromFileWatcher = false)
		{
			var result = this.stashService.LoadTransferStash(fromFileWatcher);

			// Get the transfer stash
			try
			{
				if (result.Stash.StashFound.HasValue && !result.Stash.StashFound.Value)
				{
					var msg = string.Concat(Resources.StashNotFoundMsg, "\n\nTransfer Stash\n\n", result.TransferStashFile);
					MessageBox.Show(msg, Resources.StashNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				if (result.Stash.ArgumentException != null)
				{
					string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, result.TransferStashFile, result.Stash.ArgumentException.Message);
					MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				if (!fromFileWatcher)
					this.stashPanel.TransferStash = result.Stash;
			}
			catch (IOException exception)
			{
				string msg = string.Format(CultureInfo.InvariantCulture, Resources.MainFormReadError, result.TransferStashFile, exception.ToString());
				MessageBox.Show(msg, Resources.MainFormStashReadError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				Log.LogError(exception, msg);
				this.stashPanel.TransferStash = null;
			}

			return result;
		}

		/// <summary>
		/// Loads the relic vault stash
		/// </summary>
		/// <param name="fromFileWatcher">When <code>true</code> called from <see cref="FileSystemWatcher.Changed"/></param>
		private LoadRelicVaultStashResult LoadRelicVaultStash(bool fromFileWatcher = false)
		{
			var result = this.stashService.LoadRelicVaultStash(fromFileWatcher);

			// Get the relic vault stash
			try
			{
				if (result.Stash.StashFound.HasValue && !result.Stash.StashFound.Value)
				{
					var msg = string.Concat(Resources.StashNotFoundMsg, "\n\nRelic Stash\n\n", result.RelicVaultStashFile);
					MessageBox.Show(msg, Resources.StashNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				if (result.Stash.ArgumentException != null)
				{
					string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, result.RelicVaultStashFile, result.Stash.ArgumentException.Message);
					MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				if (!fromFileWatcher)
					this.stashPanel.RelicVaultStash = result.Stash;
			}
			catch (IOException exception)
			{
				string msg = string.Format(CultureInfo.InvariantCulture, Resources.MainFormReadError, result.RelicVaultStashFile, exception.ToString());
				MessageBox.Show(msg, Resources.MainFormStashReadError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);

				this.stashPanel.RelicVaultStash = null;
			}

			return result;
		}

		private void fileSystemWatcherRelicStash_Changed(object sender, FileSystemEventArgs e)
		{
			if (e.ChangeType != WatcherChangeTypes.Changed) return;

			var fw = sender as FileSystemWatcher;
			fw.EnableRaisingEvents = false;

			// Reload
			var stashResult = LoadRelicVaultStash(true);

			// Refresh
			this.Invoke((MethodInvoker)delegate
			{
				if (stashResult is not null)
					this.stashPanel.RelicVaultStash = stashResult.Stash;

				fw.EnableRaisingEvents = true;
			});
		}

		private void fileSystemWatcherTransferStash_Changed(object sender, FileSystemEventArgs e)
		{
			if (e.ChangeType != WatcherChangeTypes.Changed) return;

			var fw = sender as FileSystemWatcher;
			fw.EnableRaisingEvents = false;

			// Reload
			var stashResult = LoadTransferStash(true);

			// Refresh
			this.Invoke((MethodInvoker)delegate
			{
				if (stashResult is not null)
					this.stashPanel.TransferStash = stashResult.Stash;

				fw.EnableRaisingEvents = true;
			});
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
				Log.LogError(exception, title);

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
