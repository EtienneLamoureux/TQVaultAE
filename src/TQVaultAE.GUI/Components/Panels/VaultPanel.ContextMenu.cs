//-----------------------------------------------------------------------
// <copyright file="VaultPanel.ContextMenu.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
/// <summary>
/// Represents a TQ Vault control that displays a frame around a group of TQ Vault panels with an optional caption.
/// </summary>

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Panels;

public partial class VaultPanel
{
	private (char charDelimiter, CsvDelimiter csvDelimiter) GetCSVDelimiter()
	{
		var delimStr = this.USettings.CSVDelimiter;

		char delim = ',';

		if (!EnumsNET.Enums.TryParse<CsvDelimiter>(delimStr, out var delimEnum))
			delimEnum = CsvDelimiter.Comma;

		delim = (EnumsNET.Enums.AsString(delimEnum, EnumsNET.EnumFormat.Description) ?? ",").First();

		return (delim, delimEnum);
	}

	private void ExportVaultCSVToClipboard(object? sender, EventArgs e)
		=> this.ExportCSVToClipboard(null);

	private void ExportBagCSVToClipboard(object? sender, EventArgs e)
		=> this.ExportCSVToClipboard(this.BagSackPanel.CurrentSack);

	/// <summary>
	/// Copies the CSV export of the shown vault, or of one of its bags, to the clipboard.
	/// </summary>
	/// <param name="bagIndex">Index of the bag to export; the whole vault when null.</param>
	private void ExportCSVToClipboard(int? bagIndex)
	{
		var (delim, _) = GetCSVDelimiter();

		var export = this.Services.VaultService.BuildCsvExport(this.ShownVault, bagIndex, delim);

		Clipboard.SetText(export, TextDataFormat.UnicodeText);

		this.UIService.NotifyUser(string.Format(Resources.CSVExportedToClipboardNotification, DateTime.Now));
	}

	private void DisableTooltipClicked(object? sender, EventArgs e)
	{
		// The entry carries its action (AddTooltipMenuItem), not compared by its translated text.
		if ((sender as ToolStripItem)?.Tag is TQVaultAE.Application.DTOs.BagTooltipChange change)
			this.Services.VaultService.ChangeBagTooltip(this.ShownVault, change, this.CurrentBag, this.BagButtons.Count);
	}

	/// <summary>Adds a bag tooltip entry that does <paramref name="change"/>.</summary>
	private void AddTooltipMenuItem(string menuText, TQVaultAE.Application.DTOs.BagTooltipChange change)
	{
		this.AddMenuItem(menuText, this.DisableTooltipClicked);
		this.contextMenu.Items[^1].Tag = change;
	}

	private void AddExportTabSubMenu()
	{
		var hasApiKey = this.ExchangeService?.HasPasteBinApiKey == true;

		var clipboardItem = new ToolStripMenuItem(
			Resources.PlayerPanelMenuExportTabClipboard, null,
			this.ExportTabToClipboardClicked)
		{
			BackColor = this.contextMenu.BackColor,
			Font = this.contextMenu.Font,
			ForeColor = this.contextMenu.ForeColor,
		};

		var fileItem = new ToolStripMenuItem(
			Resources.PlayerPanelMenuExportTabFile, null,
			this.ExportTabToFileClicked)
		{
			BackColor = this.contextMenu.BackColor,
			Font = this.contextMenu.Font,
			ForeColor = this.contextMenu.ForeColor,
		};

		var pasteBinItem = new ToolStripMenuItem(
			Resources.PlayerPanelMenuExportTabPasteBin, null,
			this.ExportTabToPasteBinClicked)
		{
			BackColor = this.contextMenu.BackColor,
			Font = this.contextMenu.Font,
			ForeColor = this.contextMenu.ForeColor,
			Enabled = hasApiKey
		};

		var exportTabMenu = new ToolStripMenuItem(
			Resources.PlayerPanelMenuExportTab, null,
			clipboardItem, fileItem, pasteBinItem)
		{
			BackColor = this.contextMenu.BackColor,
			Font = this.contextMenu.Font,
			ForeColor = this.contextMenu.ForeColor,
			DisplayStyle = ToolStripItemDisplayStyle.Text,
		};

		this.contextMenu.Items.Add(exportTabMenu);
	}

	private void ExportTabToClipboardClicked(object? sender, EventArgs e)
	{
		var exchangeService = this.ExchangeService;
		if (exchangeService is not null && this.BagSackPanel?.Sack is not null)
		{
			var json = exchangeService.SerializeSackCollection(this.BagSackPanel.Sack, this.CurrentBag);
			Clipboard.SetText(json);
		}
	}

	private void ExportTabToFileClicked(object? sender, EventArgs e)
	{
		var exchangeService = this.ExchangeService;
		if (exchangeService is not null && this.BagSackPanel?.Sack is not null)
		{
			var json = exchangeService.SerializeSackCollection(this.BagSackPanel.Sack, this.CurrentBag);
			using var dialog = new SaveFileDialog
			{
				Filter = "JSON files (*.json)|*.json",
				DefaultExt = "json",
				FileName = "tab_export.json"
			};

			if (dialog.ShowDialog() == DialogResult.OK)
				exchangeService.ExportToFile(json, dialog.FileName);
		}
	}

	private async void ExportTabToPasteBinClicked(object? sender, EventArgs e)
	{
		var exchangeService = this.ExchangeService;
		if (exchangeService is not null && this.BagSackPanel?.Sack is not null)
		{
			try
			{
				var url = await exchangeService.ExportTabToPasteBinAsync(this.ShownVault.PlayerName, this.BagSackPanel.Sack, this.CurrentBag);
				Clipboard.SetText(url);
				this.UIService.NotifyUser($"Tab exported to PasteBin: {url}");
			}
			catch (Exception ex)
			{
				this.Services.Logger<VaultPanel>().LogError(ex, "Failed to export tab to PasteBin");
				this.UIService.ShowError($"Failed to export to PasteBin: {ex.Message}");
			}
		}
	}

	/// <summary>
	/// Gets the destination bag index carried by a bag submenu entry.
	/// </summary>
	/// <param name="sender">The clicked menu entry.</param>
	/// <returns>The bag index, -1 when the entry carries none.</returns>
	private static int GetDestinationSackIndex(object? sender)
		=> (sender as ToolStripItem)?.Tag is MenuItemTagBagIndex tag ? tag.Index : -1;

	/// <summary>
	/// Creates a context menu submenu with choices for each bag in the panel
	/// except for the currently selected bag.
	/// </summary>
	/// <param name="menuText">Text label for the sub menu</param>
	/// <param name="menuCallback">handler for the submenu</param>
	private void AddSubMenu(string menuText, EventHandler menuCallback)
	{
		var menuChoices = new ToolStripItem[this.BagButtons.Count - 1];
		int menuIndex = 0;
		for (int i = 0; i < this.BagButtons.Count; i++)
		{
			if (i != this.CurrentBag)
			{
				int val = i + 1;
				menuChoices[menuIndex] = new ToolStripMenuItem(string.Format(CultureInfo.CurrentCulture, Resources.GlobalMenuBag, val)
					, null, menuCallback, string.Format(CultureInfo.CurrentCulture, Resources.GlobalMenuBag, val))
				{
					BackColor = this.contextMenu.BackColor,
					Font = this.contextMenu.Font,
					ForeColor = this.contextMenu.ForeColor,
					Tag = new MenuItemTagBagIndex(i),
				};
				menuIndex++;
			}
		}

		var subMenu = new ToolStripMenuItem(menuText, null, menuChoices)
		{
			BackColor = this.contextMenu.BackColor,
			Font = this.contextMenu.Font,
			ForeColor = this.contextMenu.ForeColor,
			DisplayStyle = ToolStripItemDisplayStyle.Text
		};

		this.contextMenu.Items.Add(subMenu);
	}

	private void AddMenuItem(string menuText, EventHandler menuCallback)
	{
		var subMenu = new ToolStripMenuItem(menuText, null, menuCallback)
		{
			BackColor = this.contextMenu.BackColor,
			Font = this.contextMenu.Font,
			ForeColor = this.contextMenu.ForeColor,
			DisplayStyle = ToolStripItemDisplayStyle.Text,
		};

		this.contextMenu.Items.Add(subMenu);
	}

	/// <summary>
	/// Handler for clicking on the context menu
	/// </summary>
	/// <param name="sender">semder object</param>
	/// <param name="e">ToolStripItemClickedEventArgs data</param>
	private void ContextMenuItemClicked(object? sender, ToolStripItemClickedEventArgs e)
	{
		string? selectedItem = e.ClickedItem?.Text;
		if (selectedItem == Resources.PlayerPanelMenuEmpty)
		{
			if (this.USettings.SuppressWarnings || this.Services.MessageBoxes.Confirm(Resources.PlayerPanelEmptyMsg, Resources.PlayerPanelEmpty))
			{
				this.Services.VaultService.EmptyBag(this.BagSackPanel.ShownSack);
				this.BagSackPanel.Refresh();
				BagButtonTooltip.InvalidateCache(this.BagSackPanel.Sack);
			}
		}

		if (selectedItem == Resources.PlayerPanelChangeIcon)
		{
			var button = this.BagButtons[this.CurrentBag];

			var bbs = this.Services.Forms.Create<BagButtonSettings>();
			bbs.CurrentBagButton = button;

			if (bbs.ShowDialog() == DialogResult.OK)
			{
				button.Refresh();
			}
		}

		if (selectedItem == Resources.PlayerPanelCopyIcon)
		{
			var button = this.BagButtons[this.CurrentBag];
			// Keep a copy, not the bag's own instance, so later edits of the bag do not change it.
			this.userContext.IconInfoCopy = button.Sack?.BagButtonIconInfo?.Clone();

			if (this.FindForm() is MainForm frm)
				frm.NotificationText.Text = Resources.GlobalCopied;
		}

		if (selectedItem == Resources.PlayerPanelPasteIcon)
		{
			var button = this.BagButtons[this.CurrentBag];

			// Paste a copy, so pasting on several bags does not share one instance.
			var sourceInfo = this.userContext.IconInfoCopy;
			if (sourceInfo is not null && button.Sack is null)
				return;

			button.Sack?.ChangeBagButtonIconInfo(sourceInfo?.Clone());
			if (this.FindForm() is MainForm frm)
				frm.NotificationText.Text = Resources.GlobalPasted;
			button.ApplyIconInfo(button.Sack);
			button.Refresh();
		}

	}

	/// <summary>
	/// Handler for clicking on Copy Bag from the context menu
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void CopyBagClicked(object? sender, EventArgs e)
	{
		ToolStripMenuItem? item = (ToolStripMenuItem)sender!;
		if (item is not null)
		{
			int destinationIndex = GetDestinationSackIndex(item);

			var check = this.Services.VaultService.CheckBagTransfer(this.ShownPlayer, this.CurrentBag, destinationIndex);
			if (check == BagTransferCheck.Invalid)
			{
				return;
			}

			if (check == BagTransferCheck.DestinationNotEmpty)
			{
				// Suppressed warnings mean overwrite without asking, as for the other confirmations.
				if (!this.USettings.SuppressWarnings && !this.Services.MessageBoxes.Confirm(Resources.PlayerOverwriteSackMsg, Resources.PlayerOverwriteSack))
				{
					return;
				}

				// The overwritten items leave the search too.
				if (this.ShownPlayer.GetSack(destinationIndex) is { } overwritten)
					this.Services.VaultService.EmptyBag(overwritten);
			}

			if (this.ShownPlayer.CopySack(this.CurrentBag, destinationIndex))
			{
				if (this.CurrentBag != destinationIndex)
				{
					// turn off the current bag and turn on the new bag
					this.BagButtons[this.CurrentBag].IsOn = false;
					this.BagButtons[destinationIndex].IsOn = true;
					this.CurrentBag = destinationIndex;
					this.BagSackPanel.Sack = this.ShownPlayer.GetSack(this.CurrentBag + this.BagPanelOffset);
					this.BagSackPanel.CurrentSack = this.CurrentBag + this.BagPanelOffset;
				}
			}
		}
	}

	/// <summary>
	/// Handler for clicking move bag from the context menu
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void MoveBagClicked(object? sender, EventArgs e)
	{
		ToolStripMenuItem? item = (ToolStripMenuItem)sender!;
		if (item is not null)
		{
			int destinationIndex = GetDestinationSackIndex(item);
			if (this.ShownPlayer.MoveSack(this.CurrentBag, destinationIndex))
			{
				if (this.CurrentBag != destinationIndex)
				{
					// turn off the current bag and turn on the new bag
					this.BagButtons[this.CurrentBag].IsOn = false;
					this.BagButtons[destinationIndex].IsOn = true;
					this.CurrentBag = destinationIndex;
					this.BagSackPanel.Sack = this.ShownPlayer.GetSack(this.CurrentBag + this.BagPanelOffset);
					this.BagSackPanel.CurrentSack = this.CurrentBag + this.BagPanelOffset;
				}

				ApplyCustomButtonIcons();
				Refresh();
			}
		}
	}

	/// <summary>
	/// Handler for clicking Merge Bag on the context menu
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void MergeBagClicked(object? sender, EventArgs e)
	{
		ToolStripMenuItem? item = (ToolStripMenuItem)sender!;
		if (item is not null)
		{
			int destinationIndex = GetDestinationSackIndex(item);

			if (this.Services.VaultService.CheckBagTransfer(this.ShownPlayer, this.CurrentBag, destinationIndex) == BagTransferCheck.Invalid)
			{
				return;
			}

			SackPanel dstSackPanel = this.BagSackPanel;
			if (dstSackPanel.MergeSack(destinationIndex))
			{
				// turn off the current bag and turn on the new bag
				this.BagButtons[this.CurrentBag].IsOn = false;
				this.BagButtons[destinationIndex].IsOn = true;
				this.CurrentBag = destinationIndex;
				this.BagSackPanel.Sack = this.ShownPlayer.GetSack(this.CurrentBag + this.BagPanelOffset);
				this.BagSackPanel.CurrentSack = this.CurrentBag + this.BagPanelOffset;
			}
		}
	}
}
