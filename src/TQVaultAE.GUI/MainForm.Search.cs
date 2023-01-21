using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Search;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI;

public partial class MainForm
{
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
		var searchDialog = this.ServiceProvider.GetService<SearchDialogAdvanced>();
		//searchDialog.Scale(new SizeF(UIService.Scale, UIService.Scale));
		var result = searchDialog.ShowDialog();

		if (result == DialogResult.OK && searchDialog.QueryResults.Any())
			this.DisplayResults(null, searchDialog.QueryResults);
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

	private void DisplayResults(string searchString, IEnumerable<Result> results)
	{
		if (results is null || !results.Any())
		{
			MessageBox.Show(
				string.Format(Resources.MainFormNoItemsFound, searchString)
				, Resources.MainFormNoItemsFound2
				, MessageBoxButtons.OK
				, MessageBoxIcon.Information
				, MessageBoxDefaultButton.Button1
				, RightToLeftOptions
			);
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
		else if (selectedResult.SackType == SackType.Player || selectedResult.SackType == SackType.Equipment || selectedResult.SackType == SackType.Stash)
		{
			// Switch to the selected player
			if (this.showSecondaryVault)
			{
				this.showSecondaryVault = !this.showSecondaryVault;
				this.UpdateTopPanel();
			}

			// Update the selection list and load the character.				
			this.comboBoxCharacter.SelectedIndex = this.comboBoxCharacter.FindString(selectedResult.ContainerName);

			// Bail if we are attempting to highlight something in the stash panel and the stash does not exist.
			if ((this.stashPanel == null || this.stashPanel.SackPanel == null) && selectedResult.SackType != SackType.Player)
				return;

			if (selectedResult.SackType == SackType.Player)
			{
				// Highlight the item if it's in the player inventory.
				if (selectedResult.SackNumber == 0)
					this.playerPanel.SackPanel.SelectItem(selectedResult.FriendlyNames.Item.Location);
				else
				{
					this.playerPanel.CurrentBag = selectedResult.SackNumber - 1;
					this.playerPanel.BagSackPanel.SelectItem(selectedResult.FriendlyNames.Item.Location);
				}
			}
			else
			{
				this.stashPanel.CurrentBag = selectedResult.SackNumber;
				this.stashPanel.SackPanel.SelectItem(selectedResult.FriendlyNames.Item.Location);
			}
		}
		else if ((selectedResult.SackType == SackType.TransferStash) || (selectedResult.SackType == SackType.RelicVaultStash))
		{
			// Switch to the Stash bag
			this.stashPanel.CurrentBag = selectedResult.SackNumber;
			this.stashPanel.SackPanel.SelectItem(selectedResult.FriendlyNames.Item.Location);
		}
	}



}
