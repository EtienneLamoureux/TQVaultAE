using System.Globalization;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Results;

namespace TQVaultAE.GUI;

public partial class MainForm
{
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
	private StashLoadResult LoadTransferStash(bool fromFileWatcher = false)
	{
		// Only if it's IT, TQ doesn't have one
		var selectedSave = this.comboBoxCharacter.SelectedItem as PlayerSave;
		if (selectedSave is not null && !selectedSave.IsImmortalThrone)
		{
			this.stashPanel.TransferStash = null;
			return null;
		}

		var result = this.stashService.LoadTransferStash(fromFileWatcher);

		// Get the transfer stash
		try
		{
			if (result.Stash.StashFound.HasValue && !result.Stash.StashFound.Value)
			{
				var msg = string.Concat(Resources.StashNotFoundMsg, "\n\nTransfer Stash\n\n", result.StashFile);
				MessageBox.Show(msg, Resources.StashNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, RightToLeftOptions);
			}

			if (result.Stash.ArgumentException != null)
			{
				string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, result.StashFile, result.Stash.ArgumentException.Message);
				MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
			}

			if (!fromFileWatcher)
				this.stashPanel.TransferStash = result.Stash;
		}
		catch (IOException exception)
		{
			string msg = string.Format(CultureInfo.InvariantCulture, Resources.MainFormReadError, result.StashFile, exception.ToString());
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
	private StashLoadResult LoadRelicVaultStash(bool fromFileWatcher = false)
	{
		// Only if it's IT, TQ doesn't have one
		var selectedSave = this.comboBoxCharacter.SelectedItem as PlayerSave;
		if (selectedSave is not null && !selectedSave.IsImmortalThrone)
		{
			this.stashPanel.RelicVaultStash = null;
			return null;
		}

		var result = this.stashService.LoadRelicVaultStash(fromFileWatcher);

		// Get the relic vault stash
		try
		{
			if (result.Stash.StashFound.HasValue && !result.Stash.StashFound.Value)
			{
				var msg = string.Concat(Resources.StashNotFoundMsg, "\n\nRelic Stash\n\n", result.StashFile);
				MessageBox.Show(msg, Resources.StashNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, RightToLeftOptions);
			}

			if (result.Stash.ArgumentException != null)
			{
				string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, result.StashFile, result.Stash.ArgumentException.Message);
				MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
			}

			if (!fromFileWatcher)
				this.stashPanel.RelicVaultStash = result.Stash;
		}
		catch (IOException exception)
		{
			string msg = string.Format(CultureInfo.InvariantCulture, Resources.MainFormReadError, result.StashFile, exception.ToString());
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

	retryOnLock:
		try
		{
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
		catch (IOException ioException)
		{
			Log.LogError(ioException, "Retry in 0.5 sec");
			Thread.Sleep(500);
			goto retryOnLock;
		}
	}

	private void fileSystemWatcherTransferStash_Changed(object sender, FileSystemEventArgs e)
	{
		if (e.ChangeType != WatcherChangeTypes.Changed) return;

		var fw = sender as FileSystemWatcher;
		fw.EnableRaisingEvents = false;

	retryOnLock:
		try
		{
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
		catch (IOException ioException)
		{
			Log.LogError(ioException, "Retry in 0.5 sec");
			Thread.Sleep(500);
			goto retryOnLock;
		}
	}
	/// <summary>
	/// Attempts to save all modified stash files.
	/// </summary>
	private bool SaveAllModifiedStashes()
	{
		// Use service with ref parameter
		Stash stashOnError = null;
		return this.stashService.SaveAllModifiedStashes(ref stashOnError) > 0;
	}
}
