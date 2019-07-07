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
using System.Linq;
using TQVaultAE.Services;

namespace TQVaultAE.GUI
{
	public partial class MainForm
	{
		private PlayerService playerService = null;

		/// <summary>
		/// Handler for changing the Character drop down selection.
		/// </summary>
		/// <param name="sender">Sender object</param>
		/// <param name="e">EventArgs data</param>
		private void CharacterComboBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			// Hmm. We can load a character now!
			string selectedText = this.characterComboBox.SelectedItem.ToString();

			// See if they actually changed their selection and ignore "No TQ characters detected"
			if (selectedText.Equals(Resources.MainFormSelectCharacter) || selectedText.Equals(Resources.MainFormNoCharacters)
				|| selectedText.Equals(Resources.MainFormNoCustomChars))
			{
				// no char selected
				this.ClearPlayer();
			}
			else
			{
				this.LoadPlayer(selectedText);
			}
		}

		/// <summary>
		/// Gets a list of available player files and populates the drop down list.
		/// </summary>
		private void GetPlayerList()
		{
			if (this.playerService is null) this.playerService = new PlayerService(userContext);

			// Initialize the character combo-box
			this.characterComboBox.Items.Clear();

			string[] charactersIT = TQData.GetCharacterList();

			int numIT = 0;
			if (charactersIT != null)
			{
				numIT = charactersIT.Length;
			}

			if (numIT == 0)
			{
				this.characterComboBox.Items.Add(Resources.MainFormNoCharacters);
				this.characterComboBox.SelectedIndex = 0;
			}
			else
			{
				this.characterComboBox.Items.Add(Resources.MainFormSelectCharacter);
				this.characterComboBox.SelectedIndex = 0;

				string characterDesignator = string.Empty;

				// Modified by VillageIdiot
				// Added to support custom Maps
				if (TQData.IsCustom)
				{
					characterDesignator = string.Concat(characterDesignator, PlayerService.CustomDesignator);
				}

				string[] characters = charactersIT.Select(c => string.Concat(c, characterDesignator)).ToArray();
				this.characterComboBox.Items.AddRange(characters);
			}
		}

		/// <summary>
		/// Creates the player panel
		/// </summary>
		private void CreatePlayerPanel()
		{
			this.playerPanel = new PlayerPanel(this.DragInfo, 4, new Size(12, 5), new Size(8, 5));

			this.playerPanel.Location = new Point(
				this.ClientSize.Width - (this.playerPanel.Width + Convert.ToInt32(22.0F * UIService.UI.Scale)),
				this.characterComboBox.Location.Y + Convert.ToInt32(28.0F * UIService.UI.Scale));

			this.playerPanel.DrawAsGroupBox = false;

			this.playerPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
			this.playerPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
			this.playerPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
			this.playerPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
			this.playerPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
			this.playerPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);
			Controls.Add(this.playerPanel);
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
				{
					this.stashPanel.Stash = null;
				}
			}
		}

		/// <summary>
		/// Loads a player using the drop down list.
		/// Assumes designators are appended to character name.
		/// Changed by VillageIdiot to a separate function.
		/// </summary>
		/// <param name="selectedText">Player string from the drop down list.</param>
		private void LoadPlayer(string selectedText)
		{
			var result = this.playerService.LoadPlayer(selectedText,true);

			// Get the player
			try
			{
				if (result.PlayerArgumentException != null)
				{
					string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, result.PlayerFile, result.PlayerArgumentException.Message);
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
			try
			{
				// Throw a message if the stash is not present.
				if (result.StashFound.HasValue && !result.StashFound.Value)
				{
					MessageBox.Show(Resources.StashNotFoundMsg, Resources.StashNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				if (result.StashArgumentException != null)
				{
					string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, result.StashFile, result.StashArgumentException.Message);
					MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				this.stashPanel.Stash = result.Stash;
			}
			catch (IOException exception)
			{
				string msg = string.Concat(Resources.MainFormReadError, result.StashFile, exception.ToString());
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
