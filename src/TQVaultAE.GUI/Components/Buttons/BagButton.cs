using TQVaultAE.GUI.Components.Rendering;

//-----------------------------------------------------------------------
// <copyright file="BagButton.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TQVaultAE.GUI.Components.Buttons;

using System;
using TQVaultAE.Presentation;

/// <summary>
/// Used to create sack bag buttons.
/// </summary>
public class BagButton : BagButtonBase
{
	/// <summary>
	/// Initializes a new instance of the BagButton class.
	/// </summary>
	/// <param name="bagNumber">number of the bag for display</param>
	/// <param name="getToolTip">Tooltip delegate</param>
	/// <param name="services"></param>
	public BagButton(int bagNumber, GetToolTip getToolTip, GuiServices services)
		: base(bagNumber, getToolTip, services)
	{ }

	/// <summary>
	/// Sets the background bitmaps for the BagButton
	/// </summary>
	public override void CreateBackgroundGraphics()
	{
		this.OnBitmap ??= UIPictures.BagUp;
		this.OffBitmap ??= UIPictures.BagDown;
		this.OverBitmap ??= UIPictures.BagOver;
	}
}