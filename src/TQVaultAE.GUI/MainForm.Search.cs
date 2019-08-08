using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Search;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using TQVaultAE.Services;

namespace TQVaultAE.GUI
{
	public partial class MainForm
	{
		private ISearchService searchService = null;

		/// <summary>
		/// Handler for clicking the search button on the form.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void SearchButtonClick(object sender, EventArgs e)
		{
			this.OpenSearchDialog();
		}

		/// <summary>
		/// Opens a scaled SearchDialog box and calls Search().
		/// </summary>
		private void OpenSearchDialog()
		{
			var searchDialog = this.ServiceProvider.GetService<SearchDialog>();
			searchDialog.Scale(new SizeF(UIService.Scale, UIService.Scale));

			if (searchDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(searchDialog.SearchText))
			{
				this.Search(searchDialog.SearchText);
			}
		}

		/// <summary>
		/// Handler for keypresses within the search text box.
		/// Used to handle the resizing hot keys.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">KeyEventArgs data</param>
		private void SearchTextBoxKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == (Keys.Control | Keys.Add) || e.KeyData == (Keys.Control | Keys.Oemplus))
			{
				this.ResizeFormCallback(this, new ResizeEventArgs(0.1F));
			}

			if (e.KeyData == (Keys.Control | Keys.Subtract) || e.KeyData == (Keys.Control | Keys.OemMinus))
			{
				this.ResizeFormCallback(this, new ResizeEventArgs(-0.1F));
			}

			if (e.KeyData == (Keys.Control | Keys.Home))
			{
				this.ResizeFormCallback(this, new ResizeEventArgs(1.0F));
			}
		}


		/// <summary>
		/// Searches loaded files based on the specified search string.  Internally normalized to UpperInvariant
		/// </summary>
		/// <param name="searchString">string that we are searching for</param>
		private void Search(string searchString)
		{
			if (string.IsNullOrWhiteSpace(searchString)) return;

			var results = this.searchService.Search(searchString);

			if (results is null || !results.Any())
			{
				MessageBox.Show(
					string.Format(CultureInfo.CurrentCulture, Resources.MainFormNoItemsFound, searchString),
					Resources.MainFormNoItemsFound2,
					MessageBoxButtons.OK,
					MessageBoxIcon.Information,
					MessageBoxDefaultButton.Button1,
					RightToLeftOptions);

				return;
			}

			// Display a dialog with the results.
			ResultsDialog dlg = this.ServiceProvider.GetService<ResultsDialog>();
			dlg.ResultChanged += new ResultsDialog.EventHandler<ResultChangedEventArgs>(this.SelectResult);
			dlg.ResultsList.Clear();
			dlg.ResultsList.AddRange(results);
			dlg.SearchString = searchString;
			dlg.Show();
		}

		/// <summary>
		/// Selects the item highlighted in the results list.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">ResultChangedEventArgs data</param>
		private void SelectResult(object sender, ResultChangedEventArgs e)
		{
			Result selectedResult = e.Result;
			if (selectedResult == null || selectedResult.FriendlyNames == null) return;

			this.ClearAllItemsSelectedCallback(this, new SackPanelEventArgs(null, null));

			if (selectedResult.SackType == SackType.Vault)
			{
				// Switch to the selected vault
				this.vaultListComboBox.SelectedItem = selectedResult.ContainerName;
				this.vaultPanel.CurrentBag = selectedResult.SackNumber;
				this.vaultPanel.SackPanel.SelectItem(selectedResult.FriendlyNames.Item.Location);
			}
			else if (selectedResult.SackType == SackType.Player || selectedResult.SackType == SackType.Equipment)
			{
				// Switch to the selected player
				if (this.showSecondaryVault)
				{
					this.showSecondaryVault = !this.showSecondaryVault;
					this.UpdateTopPanel();
				}

				string myName = selectedResult.ContainerName;

				if (GamePathResolver.IsCustom)
				{
					myName = string.Concat(myName, PlayerService.CustomDesignator);
				}

				// Update the selection list and load the character.
				this.characterComboBox.SelectedItem = myName;
				if (selectedResult.SackNumber > 0)
				{
					this.playerPanel.CurrentBag = selectedResult.SackNumber - 1;
				}

				if (selectedResult.SackType != SackType.Equipment)
				{
					// Highlight the item if it's in the player inventory.
					if (selectedResult.SackNumber == 0)
					{
						this.playerPanel.SackPanel.SelectItem(selectedResult.FriendlyNames.Item.Location);
					}
					else
					{
						this.playerPanel.BagSackPanel.SelectItem(selectedResult.FriendlyNames.Item.Location);
					}
				}
			}
			else if (selectedResult.SackType == SackType.Stash)
			{
				// Switch to the selected player
				if (this.showSecondaryVault)
				{
					this.showSecondaryVault = !this.showSecondaryVault;
					this.UpdateTopPanel();
				}

				// Assume that only IT characters can have a stash.
				string myName = string.Concat(selectedResult.ContainerName, "<Immortal Throne>");

				if (GamePathResolver.IsCustom)
				{
					myName = string.Concat(myName, PlayerService.CustomDesignator);
				}

				// Update the selection list and load the character.
				this.characterComboBox.SelectedItem = myName;

				// Switch to the Stash bag
				this.stashPanel.CurrentBag = selectedResult.SackNumber;
				this.stashPanel.SackPanel.SelectItem(selectedResult.FriendlyNames.Item.Location);
			}
			else if ((selectedResult.SackType == SackType.TransferStash) || (selectedResult.SackType == SackType.RelicVaultStash))
			{
				// Switch to the Stash bag
				this.stashPanel.CurrentBag = selectedResult.SackNumber;
				this.stashPanel.SackPanel.SelectItem(selectedResult.FriendlyNames.Item.Location);
			}
		}



	}
}
