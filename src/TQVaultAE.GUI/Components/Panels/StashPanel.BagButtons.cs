//-----------------------------------------------------------------------
// <copyright file="StashPanel.BagButtons.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.GUI.Components.Buttons;
using TQVaultAE.GUI.Components.Menus;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Components.Panels;

using System;
using System.Drawing;
using System.Windows.Forms;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;
using TQVaultAE.Presentation;

public partial class StashPanel
{
	/// <summary>
	/// Creates a new stash button
	/// </summary>
	/// <param name="bagNumber">The bag number for button that is being created.</param>
	/// <returns>The new StashButton that is created, placed by <see cref="LayOut"/>.</returns>
	protected override StashButton CreateBagButton(int bagNumber)
	{
		StashButton button = new StashButton(bagNumber, new GetToolTip(this.GetSackToolTip), this.Services);
		button.Visible = false;
		button.MouseDown += new MouseEventHandler(this.BagButtonClick);

		button.ButtonText = buttonNames[bagNumber];

		return button;
	}

	/// <inheritdoc/>
	protected override Size BagButtonPicture => TabPictureSize;

	/// <summary>The size of the tab picture, read once: the resources make a new bitmap at each read.</summary>
	private static readonly Size TabPictureSize = PictureSize(Images.StashTabUp);

	/// <summary>Tabs are only made narrower as there are more of them: their height follows the scale alone.</summary>
	protected override Size BagButtonSize(float scale, Size picture)
		=> new((int)Math.Round(scale * (float)picture.Width), Convert.ToInt32((float)picture.Height * UIService.Scale));

	/// <summary>
	/// Handler for clicking on a bag button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	protected override void BagButtonClick(object? sender, MouseEventArgs e)
	{
		BagButtonBase? button = (BagButtonBase)sender!;

		int bagID = this.Services.StashService.ClampBagId(button.ButtonNumber);

		if (bagID != this.currentBag)
		{
			// The setter turns the buttons off and on.
			this.CurrentBag = bagID;
			this.SackPanel.ClearSelectedItems();
		}

		DisplayPlayerInfo();

		//this.BagButtons[bagID].ContextMenuStrip.Show();
	}

	/// <summary>
	/// Gives the button the stash bag it summarizes.
	/// </summary>
	/// <param name="button">button the mouse is over.  Corresponds to a sack</param>
	/// <returns>Whether the summary of this stash bag is shown: the user can turn it off per bag.</returns>
	private bool GetSackToolTip(BagButtonBase button)
	{
		button.Sack = this.GetBagSack(button.ButtonNumber);
		return !this.USettings.IsStashTooltipDisabled(button.ButtonNumber);
	}

	private void contextMenuStrip_ItemClicked(object? sender, ToolStripItemClickedEventArgs e)
	{
		if (e.ClickedItem == this.toolStripMenuItemDisableTooltip)
			this.USettings.SetStashTooltipDisabled(this.CurrentBag, true);

		if (e.ClickedItem == this.toolStripMenuItemEnableTooltip)
			this.USettings.SetStashTooltipDisabled(this.CurrentBag, false);

		this.USettings.Save();
	}

	private void buttonContextMenuStrip_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
	{
		// The font at the zoom of now: the menu is built once.
		this.buttonContextMenuStrip.Font = MenuStyle.MenuFont(this.FontService, UIService.FontZoom);

		void SetMenuItemDisableTooltip(bool disabled)
		{
			if (disabled)
			{
				this.toolStripMenuItemDisableTooltip.Visible = false;
				this.toolStripMenuItemEnableTooltip.Visible = true;
				return;
			}
			this.toolStripMenuItemDisableTooltip.Visible = true;
			this.toolStripMenuItemEnableTooltip.Visible = false;
		}

		if (((ContextMenuStrip)sender!).SourceControl is not StashButton sourceButton)
			return;

		// Adjust menu items display
		SetMenuItemDisableTooltip(this.USettings.IsStashTooltipDisabled(sourceButton.ButtonNumber));
	}
}
