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
using TQVaultAE.Services;
using TQVaultAE.Domain.Contracts.Services;

namespace TQVaultAE.GUI
{
	public partial class MainForm
	{
		private IPlayerService playerService = null;

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
				this.LoadPlayer(selectedSave);
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

			if (!characters?.Any() ?? false)
				this.characterComboBox.Items.Add(Resources.MainFormNoCharacters);
			else
			{
				this.characterComboBox.Items.Add(Resources.MainFormSelectCharacter);

				this.characterComboBox.Items.AddRange(characters);
			}

			this.characterComboBox.SelectedIndex = 0;
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
				this.stashPanel.CurrentBag = 0;

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
		private void LoadPlayer(PlayerSave selectedSave)
		{
			var result = this.playerService.LoadPlayer(selectedSave, true);

			// Get the player
			try
			{
				if (result.Player.ArgumentException != null)
				{
					string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, result.PlayerFile, result.Player.ArgumentException.Message);
					MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				this.playerPanel.Player = result.Player;
				this.stashPanel.Player = result.Player;
				this.stashPanel.CurrentBag = 0;
			}
			catch (IOException exception)
			{
				string msg = string.Format(CultureInfo.InvariantCulture, Resources.MainFormReadError, result.PlayerFile, exception.ToString());
				MessageBox.Show(msg, Resources.MainFormPlayerReadError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				Log.Error(msg, exception);
				this.playerPanel.Player = null;
				this.characterComboBox.SelectedIndex = 0;
			}

			// Get the player's stash
			var resultStash = this.stashService.LoadPlayerStash(selectedSave);
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

				this.stashPanel.Stash = resultStash.Stash;
			}
			catch (IOException exception)
			{
				string msg = string.Concat(Resources.MainFormReadError, resultStash.StashFile, exception.ToString());
				MessageBox.Show(msg, Resources.MainFormStashReadError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				Log.Error(msg, exception);
				this.stashPanel.Stash = null;
			}
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
				Log.Error(title, exception);
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
