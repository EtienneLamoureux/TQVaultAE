//-----------------------------------------------------------------------
// <copyright file="StashPanel.BagButtons.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
/// <summary>
/// Class for handling the stash panel ui functions
/// </summary>

using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.GUI.Components.Buttons;
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
	/// <param name="numberOfBags">The total number of bags which is used for scaling.</param>
	/// <returns>The new StashButton that is created.</returns>
	protected StashButton CreateBagButton(int bagNumber, int numberOfBags)
	{
		StashButton button = new StashButton(bagNumber, new GetToolTip(this.GetSackToolTip), this.Services);

		float buttonWidth = (float)Images.StashTabUp.Width;
		float buttonHeight = (float)Images.StashTabUp.Height;
		float pad = 2.0F;
		float slotWidth = buttonWidth + (2.0F * pad);

		// we need to scale down the button size depending on the # we have
		float scale = this.GetBagButtonScale(slotWidth, numberOfBags);
		float bagSlotWidth = scale * slotWidth;

		// We are using tabs so only scale horizontally
		button.Size = new Size((int)Math.Round(scale * buttonWidth), Convert.ToInt32(buttonHeight * UIService.Scale));
		float offset = (bagSlotWidth * bagNumber) + ((bagSlotWidth - button.Width) / 2.0F);

		button.Location = new Point(this.SackPanel.Location.X + (int)Math.Round(offset), this.SackPanel.Location.Y - button.Height);
		button.Visible = false;
		button.MouseDown += new MouseEventHandler(this.BagButtonClick);

		button.ButtonText = buttonNames[bagNumber];

		return button;
	}

	/// <summary>
	/// Creates all of the stash buttons.
	/// </summary>
	/// <param name="numberOfBags">The total number of buttons to create.</param>
	/// <remarks>We need this override since the CreateBagButton() method above has a different return type than the base class.</remarks>
	protected override void CreateBagButtons(int numberOfBags)
	{
		for (int i = 0; i < numberOfBags; ++i)
		{
			this.BagButtons.Insert(i, this.CreateBagButton(i, numberOfBags));
			this.Controls.Add(this.BagButtons[i]);
		}
	}

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
	/// Gets the tooltip for a sack.  Summarizes the items within the sack
	/// </summary>
	/// <param name="button">button the mouse is over.  Corresponds to a sack</param>
	/// <returns>string to be displayed in the tooltip</returns>
	private void GetSackToolTip(BagButtonBase button)
	{
		button.Sack = this.GetBagSack(button.ButtonNumber);
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
