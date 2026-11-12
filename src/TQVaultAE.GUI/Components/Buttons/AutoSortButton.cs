using TQVaultAE.Presentation.Win32;

//-----------------------------------------------------------------------
// <copyright file="AutoSortButton.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components.Buttons;

using System;
using System.Windows.Forms;
using TQVaultAE.Presentation;

/// <summary>
/// Displays and handles the auto sort button.
/// </summary>
public class AutoSortButton : BagButtonBase
{

	public AutoSortButton()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		this.SuspendLayout();
		// 
		// AutoSortButton
		// 
		this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MouseDownCallback);
		this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUpCallback);
		this.ResumeLayout(false);

	}

	/// <summary>
	/// Initializes a new instance of the AutoSortButton class.
	/// </summary>
	/// <param name="buttonNumber">Number for this button</param>
	/// <param name="rotateGraphic">bool to signal the button is using a rotated background graphic</param>
	/// <param name="mirrorGraphic">true for the upright picture mirrored: its edge against a frame on its left side
	/// (the stash's sort button, right of its frame), where a vault's button has it on its right side.</param>
	public AutoSortButton(int buttonNumber, bool rotateGraphic, GuiServices services, bool mirrorGraphic = false) : base(buttonNumber, null, services)
	{
		InitializeComponent();

		this.IsVault = rotateGraphic;
		this.IsFlipped = mirrorGraphic;
		this.CreateBackgroundGraphics();
	}

	/// <summary>
	/// Gets or sets a value indicating whether this button is attached to a vault panel.
	/// </summary>
	public bool IsVault { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the picture is mirrored left to right (see the constructor).
	/// </summary>
	public bool IsFlipped { get; set; }

	/// <summary>
	/// Updates the background graphics for the sort buttons.
	/// </summary>
	public override void CreateBackgroundGraphics()
	{
		this.OffBitmap = Images.autosortup01;
		this.OnBitmap = Images.autosortdown01;
		this.OverBitmap = Images.autosortover01;

		if (this.IsVault)
		{
			this.OffBitmap = Images.rotatedautosortup01;
			this.OnBitmap = Images.rotatedautosortdown01;
			this.OverBitmap = Images.rotatedautosortover01;
		}

		// The resource pictures are new bitmaps at each read: flipping them changes no other button.
		if (this.IsFlipped)
		{
			this.OffBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
			this.OnBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
			this.OverBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
		}

		// Scale the button to the size of the graphic.
		this.Height = Convert.ToInt32((float)this.OffBitmap.Height * UIService.Scale);
		this.Width = Convert.ToInt32((float)this.OffBitmap.Width * UIService.Scale);
	}

	/// <summary>
	/// Handler for clicking the mouse button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	private void MouseDownCallback(object? sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			this.IsOn = true;
		}
	}

	/// <summary>
	/// Handler for releasing the mouse button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	private void MouseUpCallback(object? sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			this.IsOn = false;
		}
	}
}