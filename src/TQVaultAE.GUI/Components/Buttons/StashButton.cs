//-----------------------------------------------------------------------
// <copyright file="StashButton.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components.Buttons;

/// <summary>
/// Extends the BagButton functionality to the stash panel.
/// </summary>
public class StashButton : BagButtonBase
{
	/// <summary>
	/// Initializes a new instance of the StashButton class.
	/// </summary>
	/// <param name="bagNumber">number of the bag for display</param>
	/// <param name="getToolTip">Tooltip delegate</param>
	public StashButton(int bagNumber, GetToolTip getToolTip, GuiServices services) : base(bagNumber, getToolTip, services)
	{
		// Override the default background graphics.
		this.CreateBackgroundGraphics();
	}

	/// <summary>
	/// Updates the background graphics for the stash buttons.
	/// </summary>
	public override void CreateBackgroundGraphics()
	{
		this.OnBitmap = UIPictures.StashTabUp;
		this.OffBitmap = UIPictures.StashTabDown;
		this.OverBitmap = UIPictures.StashTabOver;
	}
}