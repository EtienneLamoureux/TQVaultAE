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
using System.Linq;
using TQVaultAE.Domain.Contracts.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TQVaultAE.Domain.Results;
using System.Collections.Generic;
using System.Threading;

namespace TQVaultAE.GUI
{
	public partial class MainForm
	{
		private IPlayerService playerService = null;

		private IReadOnlyCollection<string> _watchedPlayerFiles = null;
		private IEnumerable<string> WatchedPlayerFiles
		{
			get
			{
				if (_watchedPlayerFiles is null)
					_watchedPlayerFiles = new List<string> { this.GamePathResolver.PlayerSaveFileName, this.GamePathResolver.PlayerStashFileNameB }.AsReadOnly();

				return _watchedPlayerFiles;
			}
		}

		/// <summary>
		/// Handler for changing the Character drop down selection.
		/// </summary>
		/// <param name="sender">Sender object</param>
		/// <param name="e">EventArgs data</param>
		private void CharacterComboBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			// Hmm. We can load a character now!
			var selected = this.characterComboBox.SelectedItem;
			var selectedSave = selected as PlayerSave;
			var selectedText = selected.ToString();

			// See if they actually changed their selection and ignore "No TQ characters detected"
			if (selectedText.Equals(Resources.MainFormSelectCharacter) || selectedText.Equals(Resources.MainFormNoCharacters)
				|| selectedText.Equals(Resources.MainFormNoCustomChars))
			{
				// no char selected
				this.ClearPlayer();
			}
			else
			{
				this.LoadPlayerAndStash(selectedSave);
			}
			this.Refresh();
		}

		/// <summary>
		/// Gets a list of available player files and populates the drop down list.
		/// </summary>
		private void GetPlayerList()
		{
			// Initialize the character combo-box
			this.characterComboBox.Items.Clear();

			var characters = this.playerService.GetPlayerSaveList();

			// Init FileWatcher
			if (Config.Settings.Default.EnableHotReload)
			{
				foreach (var ps in characters)
				{
					ps.PlayerSaveWatcher = CreateFileWatcher(ps.Folder, this.GamePathResolver.PlayerSaveFileName);
					ps.PlayerStashWatcher = CreateFileWatcher(ps.Folder, this.GamePathResolver.PlayerStashFileNameB);
				}
			}

			if (!(characters?.Any() ?? false))
				this.characterComboBox.Items.Add(Resources.MainFormNoCharacters);
			else
			{
				this.characterComboBox.Items.Add(Resources.MainFormSelectCharacter);

				this.characterComboBox.Items.AddRange(characters);
			}

			this.characterComboBox.SelectedIndex = 0;
		}

		// Called on FileSystemWatcher thread
		private void PlayerSaveFile_Changed(object sender, FileSystemEventArgs e)
		{
			if (e.ChangeType != WatcherChangeTypes.Changed
				|| !this.WatchedPlayerFiles.Any(f => f.Equals(e.Name, StringComparison.OrdinalIgnoreCase))
			) return;

			var fw = sender as FileSystemWatcher;
			fw.EnableRaisingEvents = false;

			// retrieve PlayerSave
			var playerSave = this.characterComboBox.Items.OfType<PlayerSave>().FirstOrDefault(ps => ps.Folder == fw.Path);

		retryOnLock:
			try
			{
				// Reload player file
				LoadPlayerResult playerResult = null;
				if (e.Name.Equals(this.GamePathResolver.PlayerSaveFileName, StringComparison.OrdinalIgnoreCase))
					playerResult = this.LoadPlayer(playerSave, true);

				LoadPlayerStashResult stashResult = null;
				if (e.Name.Equals(this.GamePathResolver.PlayerStashFileNameB, StringComparison.OrdinalIgnoreCase))
					stashResult = this.LoadPlayerStash(playerSave, true);

				// Refresh
				this.Invoke((MethodInvoker)delegate
				{
					// if is current displayed character
					if (this.characterComboBox.SelectedItem == playerSave)
					{
						if (playerResult is not null)
						{
							this.playerPanel.Player = playerResult.Player;
							this.stashPanel.Player = playerResult.Player;
						}

						if (stashResult is not null)
							this.stashPanel.Stash = stashResult.Stash;
					}

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


		private FileSystemWatcher CreateFileWatcher(string folder, string filename)
		{
			// the game write save file when inventory is opened and again when it's closed. We can't distinguish between both events.
			var fw = new FileSystemWatcher();
			fw.BeginInit();
			fw.Path = folder;
			fw.Filter = filename;
			fw.EnableRaisingEvents = true;
			fw.IncludeSubdirectories = false;
			fw.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;// You need "CreationTime" in order to trigger "LastWrite"
			fw.SynchronizingObject = this;
			fw.Changed += new FileSystemEventHandler(this.PlayerSaveFile_Changed);
			fw.EndInit();
			return fw;
		}

		/// <summary>
		/// Creates the player panel
		/// </summary>
		private void CreatePlayerPanel()
		{
			this.playerPanel = new PlayerPanel(this.DragInfo, 4, new Size(12, 5), new Size(8, 5), this.ServiceProvider);

			this.playerPanel.DrawAsGroupBox = false;

			this.playerPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
			this.playerPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
			this.playerPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
			this.playerPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
			this.playerPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
			this.playerPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);

			this.flowLayoutPanelRightPanels.Controls.Add(this.playerPanel);
			this.playerPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
			this.playerPanel.Margin = new Padding(0);
		}



		/// <summary>
		/// Clears out the selected player
		/// Changed by VillageIdiot to a separate function.
		/// </summary>
		private void ClearPlayer()
		{
			if (this.playerPanel.Player != null)
			{
				this.playerPanel.Player = null;
				this.stashPanel.Player = null;
				this.stashPanel.CurrentBag = StashPanel.BAGID_EQUIPMENTPANEL;

				if (this.stashPanel.Stash != null)
					this.stashPanel.Stash = null;
			}
		}

		/// <summary>
		/// Loads a player using the drop down list.
		/// Assumes designators are appended to character name.
		/// Changed by VillageIdiot to a separate function.
		/// </summary>
		/// <param name="selectedSave">Player string from the drop down list.</param>
		/// <param name="fromFileWatcher">When <code>true</code> called from <see cref="FileSystemWatcher.Changed"/></param>
		/// <returns></returns>
		private (LoadPlayerResult PlayerResult, LoadPlayerStashResult StashResult) LoadPlayerAndStash(PlayerSave selectedSave, bool fromFileWatcher = false)
		{
			var result = LoadPlayer(selectedSave, fromFileWatcher);

			var resultStash = this.LoadPlayerStash(selectedSave, fromFileWatcher);

			return (result, resultStash);
		}

		private LoadPlayerResult LoadPlayer(PlayerSave selectedSave, bool fromFileWatcher)
		{
			var result = this.playerService.LoadPlayer(selectedSave, true, fromFileWatcher);

			// Get the player
			try
			{
				if (result.Player.ArgumentException != null)
				{
					string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, result.PlayerFile, result.Player.ArgumentException.Message);
					MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				if (!fromFileWatcher)
				{
					this.playerPanel.Player = result.Player;
					this.stashPanel.Player = result.Player;
					this.stashPanel.CurrentBag = StashPanel.BAGID_EQUIPMENTPANEL;
				}
			}
			catch (IOException exception)
			{
				string msg = string.Format(CultureInfo.InvariantCulture, Resources.MainFormReadError, result.PlayerFile, exception.ToString());
				MessageBox.Show(msg, Resources.MainFormPlayerReadError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				Log.LogError(exception, msg);
				this.playerPanel.Player = null;
				this.characterComboBox.SelectedIndex = 0;
			}

			return result;
		}

		private LoadPlayerStashResult LoadPlayerStash(PlayerSave selectedSave, bool fromFileWatcher = false)
		{
			// Get the player's stash
			var resultStash = this.stashService.LoadPlayerStash(selectedSave, fromFileWatcher);
			try
			{
				// Throw a message if the stash is not present.
				if (resultStash.Stash.StashFound.HasValue && !resultStash.Stash.StashFound.Value)
				{
					var msg = string.Concat(Resources.StashNotFoundMsg, "\n\nCharacter : ", selectedSave);
					MessageBox.Show(msg, Resources.StashNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				if (resultStash.Stash.ArgumentException != null)
				{
					string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, resultStash.StashFile, resultStash.Stash.ArgumentException.Message);
					MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				if (!fromFileWatcher) this.stashPanel.Stash = resultStash.Stash;
			}
			catch (IOException exception)
			{
				string msg = string.Concat(Resources.MainFormReadError, resultStash.StashFile, exception.ToString());
				MessageBox.Show(msg, Resources.MainFormStashReadError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				Log.LogError(exception, msg);
				this.stashPanel.Stash = null;
			}

			return resultStash;
		}

		/// <summary>
		/// Attempts to save all modified player files
		/// </summary>
		/// <returns>True if there were any modified player files.</returns>
		private bool SaveAllModifiedPlayers()
		{
		retry:
			bool saved = false;
			PlayerCollection playerIfError = null;
			try
			{
				saved = this.playerService.SaveAllModifiedPlayers(ref playerIfError);
			}
			catch (IOException exception)
			{
				string title = string.Format(CultureInfo.InvariantCulture, Resources.MainFormSaveError, playerIfError.PlayerName);
				Log.LogError(exception, title);
				switch (MessageBox.Show(Log.FormatException(exception), title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, RightToLeftOptions))
				{
					case DialogResult.Abort:
						// rethrow the exception
						throw;
					case DialogResult.Retry:
						goto retry;
						//default: break; // DialogResult.Ignore
				}
			}
			return saved;
		}
	}
}
