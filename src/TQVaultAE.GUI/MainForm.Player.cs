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
using TQVaultAE.Presentation;
using TQVaultAE.Logs;

namespace TQVaultAE.GUI
{
	internal partial class MainForm
	{
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
			// Initialize the character combo-box
			this.characterComboBox.Items.Clear();

			string[] charactersIT = TQData.GetCharacterList();

			int numIT = 0;
			if (charactersIT != null)
			{
				numIT = charactersIT.Length;
			}

			if (numIT < 1)
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
					characterDesignator = string.Concat(characterDesignator, "<Custom Map>");
				}

				// Combine the 2 arrays into 1 then add them
				string[] characters = new string[numIT];
				int i;
				int j = 0;

				// Put the IT chars first since that is most likely what people want to use.
				for (i = 0; i < numIT; ++i)
				{
					characters[j++] = string.Concat(charactersIT[i], characterDesignator);
				}

				this.characterComboBox.Items.AddRange(characters);
			}
		}

		/// <summary>
		/// Creates the player panel
		/// </summary>
		private void CreatePlayerPanel()
		{
			this.playerPanel = new PlayerPanel(this.dragInfo, 4, new Size(12, 5), new Size(8, 5), this.tooltip);

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
			string customDesignator = "<Custom Map>";

			bool isCustom = selectedText.EndsWith(customDesignator, StringComparison.Ordinal);
			if (isCustom)
			{
				// strip off the end from the player name.
				selectedText = selectedText.Remove(selectedText.IndexOf(customDesignator, StringComparison.Ordinal), customDesignator.Length);
			}

			string playerFile = TQData.GetPlayerFile(selectedText);

			// Get the player
			try
			{
				PlayerCollection player;
				try
				{
					player = this.players[playerFile];
				}
				catch (KeyNotFoundException)
				{
					bool playerLoaded = false;
					player = new PlayerCollection(selectedText, playerFile);
					try
					{
						PlayerCollectionProvider.LoadFile(player);
						playerLoaded = true;
					}
					catch (ArgumentException argumentException)
					{
						string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, playerFile, argumentException.Message);
						MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
						playerLoaded = false;
					}

					// Only add the player to the list if it loaded successfully.
					if (playerLoaded)
					{
						this.players.Add(playerFile, player);
					}
				}

				this.playerPanel.Player = player;
				this.stashPanel.Player = player;
				this.stashPanel.CurrentBag = 0;
			}
			catch (IOException exception)
			{
				string msg = string.Format(CultureInfo.InvariantCulture, Resources.MainFormReadError, playerFile, exception.ToString());
				MessageBox.Show(msg, Resources.MainFormPlayerReadError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				Log.Error(msg, exception);
				this.playerPanel.Player = null;
				this.characterComboBox.SelectedIndex = 0;
			}

			string stashFile = TQData.GetPlayerStashFile(selectedText);

			// Get the player's stash
			try
			{
				Stash stash;
				try
				{
					stash = this.stashes[stashFile];
				}
				catch (KeyNotFoundException)
				{
					bool stashLoaded = false;
					stash = new Stash(selectedText, stashFile);
					try
					{
						bool stashPresent = StashProvider.LoadFile(stash);

						// Throw a message if the stash is not present.
						if (!stashPresent)
						{
							MessageBox.Show(Resources.StashNotFoundMsg, Resources.StashNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, RightToLeftOptions);
						}

						stashLoaded = stashPresent;
					}
					catch (ArgumentException argumentException)
					{
						string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, stashFile, argumentException.Message);
						MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
						stashLoaded = false;
					}

					if (stashLoaded)
					{
						this.stashes.Add(stashFile, stash);
					}
				}

				this.stashPanel.Stash = stash;
			}
			catch (IOException exception)
			{
				string msg = string.Concat(Resources.MainFormReadError, stashFile, exception.ToString());
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
			int numModified = 0;

			// Save each player as necessary
			foreach (KeyValuePair<string, PlayerCollection> kvp in this.players)
			{
				string playerFile = kvp.Key;
				PlayerCollection player = kvp.Value;

				if (player == null)
				{
					continue;
				}

				if (player.IsModified)
				{
					++numModified;
					bool done = false;

					// backup the file
					while (!done)
					{
						try
						{
							TQData.BackupFile(player.PlayerName, playerFile);
							TQData.BackupStupidPlayerBackupFolder(playerFile);
							PlayerCollectionProvider.Save(player, playerFile);
							done = true;
						}
						catch (IOException exception)
						{
							string title = string.Format(CultureInfo.InvariantCulture, Resources.MainFormSaveError, player.PlayerName);
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

			return numModified > 0;
		}
	}
}
