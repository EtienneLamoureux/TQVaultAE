//-----------------------------------------------------------------------
// <copyright file="VaultPanel.BagButtons.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
/// <summary>
/// Represents a TQ Vault control that displays a frame around a group of TQ Vault panels with an optional caption.
/// </summary>

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Buttons;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Components.Panels;

public partial class VaultPanel
{
	private void ApplyCustomButtonIcons()
	{
		if (this.Vault is not null)
		{
			foreach (var button in this.BagButtons)
			{
				var sack = this.Vault.GetSack(button.ButtonNumber);
				button.ApplyIconInfo(sack);
			}
		}
	}

	/// <summary>
	/// Creates all of the bag buttons and adds them to the control.
	/// </summary>
	/// <param name="numberOfBags">total number of buttons to create</param>
	protected virtual void CreateBagButtons(int numberOfBags)
	{
		for (int i = 0; i < numberOfBags; ++i)
		{
			this.BagButtons.Insert(i, this.CreateBagButton(i, numberOfBags));
			this.Controls.Add(this.BagButtons[i]);
		}
	}

	/// <summary>
	/// Handler for clicking a bag button
	/// </summary>
	/// <remarks>
	/// Changed by VillageIdiot to support right and left mouse clicking.
	/// </remarks>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	protected virtual void BagButtonClick(object? sender, MouseEventArgs e)
	{
		BagButtonBase? b = (BagButtonBase)sender!;

		int bagID = b.ButtonNumber;
		if (bagID != this.CurrentBag)
		{
			// turn off the current bag and turn on the new bag
			this.BagButtons[this.CurrentBag].IsOn = false;
			this.BagButtons[bagID].IsOn = true;
			this.CurrentBag = bagID;
			this.BagSackPanel.ClearSelectedItems();
			this.BagSackPanel.Sack = this.ShownPlayer.GetSack(this.CurrentBag + this.BagPanelOffset);
			this.BagSackPanel.CurrentSack = this.CurrentBag + this.BagPanelOffset;
		}

		if (e.Button == MouseButtons.Right)
		{
			// Only process vault bags with something in them.
			if (this.BagSackPanel.SackType == SackType.Vault && this.BagButtons[this.CurrentBag].ButtonText is not null)
			{
				this.contextMenu.Items.Clear();
				this.contextMenu.ShowCheckMargin = false;

				// Add the move submenu
				this.AddSubMenu(Resources.PlayerPanelMenuMove, this.MoveBagClicked);

				// Only show Copy, Merge and Empty if something is in the bag.
				if (!this.BagSackPanel.ShownSack.IsEmpty)
				{
					if (this.USettings.AllowItemCopy)
					{
						// Add the copy submenu
						this.AddSubMenu(Resources.PlayerPanelMenuCopy, this.CopyBagClicked);
					}

					// Add the merge submenu
					this.AddSubMenu(Resources.PlayerPanelMenuMerge, this.MergeBagClicked);

					this.contextMenu.Items.Add("-");
					this.contextMenu.Items.Add(Resources.PlayerPanelMenuEmpty).Tag = BagMenuAction.Empty;

					if (this.Vault is not null)
					{
						// Add the Disable Tooltip submenu
						this.contextMenu.Items.Add("-");

						if (this.Vault.DisabledTooltipBagId.Contains(bagID))
						{
							this.AddTooltipMenuItem(Resources.PlayerPanelMenuEnableTooltip, TQVaultAE.Application.DTOs.BagTooltipChange.Enable);
						}
						else
						{
							this.AddTooltipMenuItem(Resources.PlayerPanelMenuDisableTooltip, TQVaultAE.Application.DTOs.BagTooltipChange.Disable);
						}

						if (this.Vault.DisabledTooltipBagId.Count < this.BagButtons.Count)
						{
							this.AddTooltipMenuItem(Resources.PlayerPanelMenuDisableAllTooltip, TQVaultAE.Application.DTOs.BagTooltipChange.DisableAll);
						}

						if (this.Vault.DisabledTooltipBagId.Count != 0)
						{
							this.AddTooltipMenuItem(Resources.PlayerPanelMenuEnableAllTooltip, TQVaultAE.Application.DTOs.BagTooltipChange.EnableAll);
						}
						// Export to CSV
						this.contextMenu.Items.Add("-");
						var csvBagItem = new ToolStripMenuItem(Resources.ExportBagCSVToClipboard, null, this.ExportBagCSVToClipboard)
						{
							BackColor = this.contextMenu.BackColor,
							Font = this.contextMenu.Font,
							ForeColor = this.contextMenu.ForeColor,
						};
						var csvVaultItem = new ToolStripMenuItem(Resources.ExportVaultCSVToClipboard, null, this.ExportVaultCSVToClipboard)
						{
							BackColor = this.contextMenu.BackColor,
							Font = this.contextMenu.Font,
							ForeColor = this.contextMenu.ForeColor,
						};
						var csvMenu = new ToolStripMenuItem(Resources.ExportToCSV, null, csvBagItem, csvVaultItem)
						{
							BackColor = this.contextMenu.BackColor,
							Font = this.contextMenu.Font,
							ForeColor = this.contextMenu.ForeColor,
							DisplayStyle = ToolStripItemDisplayStyle.Text,
						};
						this.contextMenu.Items.Add(csvMenu);

						// Export Tab (JSON)
						this.contextMenu.Items.Add("-");
						this.AddExportTabSubMenu();
					}

				}

				if (this.Vault is not null)
				{
					this.contextMenu.Items.Add("-");
					this.contextMenu.Items.Add(Resources.PlayerPanelChangeIcon).Tag = BagMenuAction.ChangeIcon;
					this.contextMenu.Items.Add(Resources.PlayerPanelCopyIcon).Tag = BagMenuAction.CopyIcon;

					if (this.userContext.IconInfoCopied)
						this.contextMenu.Items.Add(Resources.PlayerPanelPasteIcon).Tag = BagMenuAction.PasteIcon;

					// Import from file
					this.contextMenu.Items.Add("-");
					var importFileItem = new ToolStripMenuItem(
						Resources.PlayerPanelMenuImportTabFile, null,
						this.ImportFromFileClicked)
					{
						BackColor = this.contextMenu.BackColor,
						Font = this.contextMenu.Font,
						ForeColor = this.contextMenu.ForeColor,
					};
					this.contextMenu.Items.Add(importFileItem);
				}

				this.contextMenu.Show(this.BagButtons[this.CurrentBag], new Point(e.X, e.Y));
			}
		}
	}

	/// <summary>
	/// Handler for clicking the autosort button: a left click sorts with the order picked in its menu, a right click
	/// opens that menu.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	protected void AutoSortButtonClick(object? sender, MouseEventArgs e)
	{
		if (sender is not AutoSortButton button)
			return;

		if (e.Button == MouseButtons.Left)
			this.SortTarget(button).Autosort();
		else if (e.Button == MouseButtons.Right)
			this.ShowSortMenu(button, e.Location);
	}

	/// <summary>
	/// The sack a sort button sorts: the bag panel here, the main inventory or the bags for the player panel.
	/// </summary>
	protected virtual SackPanel SortTarget(AutoSortButton button) => this.BagSackPanel;

	/// <summary>The sack the open sort menu sorts.</summary>
	private SackPanel? sortMenuTarget;

	/// <summary>
	/// Opens the sort menu of a sort button: one entry per order, the one a left click uses checked. Picking an entry
	/// keeps it for the next clicks and sorts at once (<see cref="ContextMenuItemClicked"/>).
	/// </summary>
	private void ShowSortMenu(AutoSortButton button, Point location)
	{
		this.sortMenuTarget = this.SortTarget(button);
		var current = this.USettings.Sort?.Algorithm ?? SackSortAlgorithm.Normal;

		this.contextMenu.Items.Clear();
		this.contextMenu.ShowCheckMargin = true;
		foreach (var (algorithm, text) in SortMenuEntries)
		{
			var entry = new ToolStripMenuItem(text) { Tag = algorithm, Checked = algorithm == current };
			this.contextMenu.Items.Add(entry);
		}

		this.contextMenu.Show(button, location);
	}

	/// <summary>The entries of the sort menu, in their order.</summary>
	private static IEnumerable<(SackSortAlgorithm Algorithm, string Text)> SortMenuEntries =>
	[
		(SackSortAlgorithm.Normal, Resources.SortMenuNormal),
		(SackSortAlgorithm.Level, Resources.SortMenuLevel),
		(SackSortAlgorithm.Type, Resources.SortMenuType),
		(SackSortAlgorithm.Rarity, Resources.SortMenuRarity),
	];

	/// <summary>
	/// A sort menu entry was picked: it becomes the order of the left click (saved in the settings), and the sack is
	/// sorted with it.
	/// </summary>
	private void SortMenuPicked(SackSortAlgorithm algorithm)
	{
		var target = this.sortMenuTarget;
		this.sortMenuTarget = null;

		this.USettings.Sort ??= new SortSettings();
		if (this.USettings.Sort.Algorithm != algorithm)
		{
			this.USettings.Sort.Algorithm = algorithm;
			this.USettings.Save();
		}

		target?.Autosort();
	}

	/// <summary>
	/// Sets all of the autosort buttons to invisible.
	/// </summary>
	protected void HideAutoSortButtons()
	{
		if (this.autoSortButtons is not null && this.autoSortButtons.Count > 0)
		{
			foreach (AutoSortButton autoSortButton in this.autoSortButtons)
			{
				autoSortButton.Visible = false;
			}
		}
	}

	/// <summary>
	/// Sets all of the autosort buttons to visible.
	/// </summary>
	protected void UnhideAutoSortButtons()
	{
		if (this.autoSortButtons is not null && this.autoSortButtons.Count > 0)
		{
			this.autoSortButtons[0].Visible = true;

			if (this.autoSortButtons.Count == 2)
			{
				this.autoSortButtons[1].Visible = this.ShownPlayer.NumberOfSacks - this.BagPanelOffset > 0;
			}
		}
	}

	/// <summary>
	/// Creates a new Autosort button
	/// </summary>
	/// <param name="buttonNumber">Number of the autosort button we are creating</param>
	/// <returns>A new AutoSortButton instance</returns>
	protected virtual AutoSortButton CreateAutoSortButton(int buttonNumber)
	{
		AutoSortButton button = new AutoSortButton(buttonNumber, true, this.Services);
		button.Location = new Point(this.BagSackPanel.Location.X - button.Width, this.BagSackPanel.Location.Y);
		button.Visible = false;
		button.MouseDown += new MouseEventHandler(this.AutoSortButtonClick);

		return button;
	}

	/// <summary>
	/// Gets the scale factor for a particular button
	/// </summary>
	/// <param name="maxWidth">unscaled size of the button</param>
	/// <param name="numberOfBags">total number of bags to scale</param>
	/// <returns>scale factor float</returns>
	protected virtual float GetBagButtonScale(float maxWidth, int numberOfBags)
		=> ((float)this.BagSackPanel.Width / maxWidth) / (float)numberOfBags;

	/// <summary>
	/// Creates a new bag button
	/// </summary>
	/// <param name="bagNumber">Bag number that we are creating</param>
	/// <param name="numberOfBags">total number of buttons to create</param>
	/// <returns>Instance of the new BagButton</returns>
	private BagButton CreateBagButton(int bagNumber, int numberOfBags)
	{
		BagButton button = new BagButton(bagNumber, new GetToolTip(this.GetSackToolTip), this.Services);

		float buttonWidth = (float)Images.inventorybagup01.Width;
		float buttonHeight = (float)Images.inventorybagup01.Height;
		float pad = 2.0F;
		float slotWidth = buttonWidth + (2.0F * pad);

		// we need to scale down the bag size depending on the # we have
		float scale = this.GetBagButtonScale(slotWidth, numberOfBags);
		float bagSlotWidth = scale * slotWidth;

		button.Size = new Size((int)Math.Round(scale * buttonWidth), (int)Math.Round(scale * buttonHeight));
		float offset = (bagSlotWidth * bagNumber) + ((bagSlotWidth - button.Width) / 2.0F);

		button.Location = new Point(this.BagSackPanel.Location.X + (int)Math.Round(offset), this.BagSackPanel.Location.Y - button.Height);

		int bagLabelNumber = bagNumber - this.BagPanelOffset + 1;
		button.ButtonText = bagLabelNumber.ToString(CultureInfo.CurrentCulture);
		button.Visible = false;

		// Changed by VillageIdiot to support right and left mouse clicking.
		button.MouseDown += new MouseEventHandler(this.BagButtonClick);

		return button;
	}

	/// <summary>
	/// Gets the tooltip for the sack contents.
	/// </summary>
	/// <param name="button">Button number of the sack</param>
	/// <returns>string listing the sack's contents.</returns>
	private void GetSackToolTip(BagButtonBase button)
	{
		// Get the list of items 
		SackCollection? sack = this.ShownPlayer.GetSack(button.ButtonNumber + this.BagPanelOffset);

		if (sack is null)
		{
			return;
		}

		button.Sack = sack;// Init prop for Tooltip
	}
}
