﻿using TQVaultAE.Presentation.Win32;

//-----------------------------------------------------------------------
// <copyright file="ScalingButton.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components.Scaling;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TQVaultAE.Presentation;
using TQVaultAE.GUI.Components.Rendering;

/// <summary>
/// Button class to support skinning and scaling of the fonts.
/// </summary>
public class ScalingButton : Button, IScalingControl, IZoomedFont, IFontRebasing
{
	/// <summary>
	/// Base Font used for scaling of the text to fit within the bounds of the button.
	/// </summary>
	private Font? baseFont;

	/// <summary>
	/// Set while the button changes its own font to fit its text, so that change is not taken as a new base font.
	/// </summary>
	private bool fittingFont;

	/// <summary>
	/// Used to indicate that the custom button graphic is being used.
	/// </summary>
	private bool useCustomGraphic;

	/// <summary>
	/// Used to indicate whether the button size is forced to be the same as the custom graphic.
	/// </summary>
	private bool sizeToGraphic;

	/// <summary>
	/// Initializes a new instance of the ScalingButton class.
	/// </summary>
	public ScalingButton()
	{
		InitializeComponent();

		this.DownBitmap = Images.MainButtonDown;
		this.OverBitmap = Images.MainButtonOver;
		this.UpBitmap = Images.MainButtonUp;
	}

	private void InitializeComponent()
	{
		this.SuspendLayout();
		// 
		// ScalingButton
		// 
		this.TextChanged += new System.EventHandler(this.TextChangedCallback);
		this.GotFocus += new System.EventHandler(this.GotFocusCallback);
		this.LostFocus += new System.EventHandler(this.LostFocusCallback);
		this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MouseDownCallback);
		this.MouseEnter += new System.EventHandler(this.MouseEnterCallback);
		this.MouseLeave += new System.EventHandler(this.MouseLeaveCallback);
		this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUpCallback);
		this.ResumeLayout(false);
	}
		
	/// <summary>
	/// Gets or sets a value indicating whether the custom button graphic is shown.
	/// </summary>
	public bool UseCustomGraphic
	{
		get => this.useCustomGraphic;

		set
		{
			if (value)
			{
				this.BackColor = Color.Transparent;
				this.FlatStyle = FlatStyle.Flat;
				this.FlatAppearance.BorderSize = 0;
				this.FlatAppearance.MouseOverBackColor = Theme.TransparentButton;
				this.FlatAppearance.MouseDownBackColor = Theme.TransparentButton;
				if (this.UpBitmap is not null)
					this.Image = this.UpBitmap;
			}
			else
			{
				this.BackColor = Color.Gold;
				this.FlatStyle = FlatStyle.Standard;
				this.FlatAppearance.BorderSize = 1;
				this.FlatAppearance.MouseOverBackColor = SystemColors.ButtonHighlight;
				this.FlatAppearance.MouseDownBackColor = SystemColors.ButtonShadow;
				this.Image = null;
			}

			this.useCustomGraphic = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the button size is forced to be the same as the custom graphic.
	/// </summary>
	/// <summary>
	/// Gets or sets a value indicating whether only the picture is drawn, not the text. The text stays the button's name
	/// for accessibility and automation (window caption buttons: Close, Maximize...).
	/// </summary>
	public bool ImageOnly { get; set; }

	public bool SizeToGraphic
	{
		get => this.sizeToGraphic;

		set
		{
			if (value && this.UpBitmap is not null)
				this.Size = this.UpBitmap.Size;

			this.sizeToGraphic = value;
		}
	}

	/// <summary>
	/// Gets or sets the Bitmap used when the button is pressed.
	/// </summary>
	public Bitmap DownBitmap { get; set; }

	/// <summary>
	/// Gets or sets the Bitmap used when the mouse hovers over the button.
	/// </summary>
	public Bitmap OverBitmap { get; set; }

	/// <summary>
	/// Gets or sets the Bitmap used when the button neither highlighted or pressed.
	/// </summary>
	public Bitmap UpBitmap { get; set; }

	/// <summary>
	/// Reverts the basic settings of a control back to the original settings.
	/// </summary>
	/// <param name="location">New Location of the control</param>
	/// <param name="size">New Size of the control</param>
	public void Revert(Point location, Size size)
	{
		this.Font = new Font("Microsoft Sans Serif", 8.25F);
		this.Location = location;
		this.Size = size;
		this.ForeColor = SystemColors.ControlText;
		this.UseCustomGraphic = false;
	}

	/// <summary>
	/// Handles the ScalingButton Paint Event.
	/// </summary>
	/// <param name="pevent">PaintEventArgs data</param>
	protected override void OnPaint(PaintEventArgs pevent)
	{
		if (this.UseCustomGraphic && this.Image is not null && this.Size != this.Image.Size)
		{
			// The copies were made for another size: drop them (this can put the source image back).
			if (this.scaledSize != this.Size)
				this.ReleaseScaledImages();

			if (this.Image is { } source && this.Size != source.Size)
				this.Image = this.ScaledImage(source);
		}

		if (this.ImageOnly && this.Image is { } picture)
		{
			// The background (and the parent behind a transparent one) is painted by OnPaintBackground.
			pevent.Graphics.DrawImage(picture, this.ClientRectangle);
			return;
		}

		base.OnPaint(pevent);
	}

	/// <summary>
	/// Copies of the up, over and down pictures at the button size, by source picture. The mouse and focus handlers put the
	/// source back on each change, so without them every hover made a new bitmap.
	/// </summary>
	private readonly Dictionary<Image, Bitmap> scaledImages = [];

	/// <summary>Size the <see cref="scaledImages"/> were made for.</summary>
	private Size scaledSize;

	private Bitmap ScaledImage(Image source)
	{
		if (this.scaledImages.TryGetValue(source, out var scaled))
			return scaled;

		// Scaling smaller can make the outside border disappear.
		// To avoid this we scale the image and then copy a 4 pixel strip from each edge to restore the border.
		if (this.Width < source.Width)
		{
			int width = 4;
			scaled = new Bitmap(this.Width, this.Height);
			using Graphics graphics = Graphics.FromImage(scaled);

			// First draw the new image scaled to the size of the button.
			graphics.DrawImage(source, new Rectangle(new Point(0, 0), this.Size));

			// Grab the left border from the source image and paint it onto the new image.
			graphics.DrawImage(source, new Rectangle(0, 0, width, this.Height), new Rectangle(0, 0, width, source.Height), GraphicsUnit.Pixel);

			// Now grab the right border from the source image and paint it onto the new image.
			graphics.DrawImage(
				source,
				new Rectangle(this.Width - width, 0, width, this.Height),
				new Rectangle(source.Width - width, 0, width, source.Height),
				GraphicsUnit.Pixel);
		}
		else
			// Just scale the image normally.
			scaled = new Bitmap(source, this.Size);

		this.scaledImages[source] = scaled;
		return scaled;
	}

	/// <summary>Disposes the scaled copies, giving the button its source picture back if it shows one of them.</summary>
	private void ReleaseScaledImages()
	{
		foreach (var (source, scaled) in this.scaledImages)
		{
			if (ReferenceEquals(this.Image, scaled))
				this.Image = source;

			scaled.Dispose();
		}

		this.scaledImages.Clear();
		this.scaledSize = this.Size;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			this.ReleaseScaledImages();

		base.Dispose(disposing);
	}

	/// <inheritdoc/>
	Font IZoomedFont.ZoomedFont
	{
		get => this.baseFont ?? this.Font;
		set
		{
			this.baseFont = value;
			this.Font = value;
		}
	}

	/// <summary>
	/// Override of ScaleControl which supports font scaling.
	/// </summary>
	/// <param name="factor">SizeF for the scale factor</param>
	/// <param name="specified">BoundsSpecified value.</param>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		// A DPI scaling (at load, or on a monitor of another DPI): Windows Forms scales the font itself and disposes the
		// fonts it made for the previous DPI, so no reference to them is kept; the text only fits the new width.
		if (!ZoomScope.IsZooming)
		{
			this.baseFont = null;
			this.SetFittedFont(this.GetScaledButtonTextFont(this.Font, Convert.ToInt32((float)this.Width * factor.Width)));
			if (this.Image is not null)
				this.Image = this.UpBitmap;

			base.ScaleControl(factor, specified);
			return;
		}

		// Get a reference for scaling.
		if (this.baseFont is null)
			this.baseFont = this.Font;

		// We changed the font so reset the base font.
		if (this.baseFont.FontFamily != this.Font.FontFamily)
			this.baseFont = this.Font;

		this.baseFont = ZoomScope.ScaleFont(this.baseFont, factor.Height);
		this.SetFittedFont(this.GetScaledButtonTextFont(this.baseFont, Convert.ToInt32((float)this.Width * factor.Width)));

		// Reset the image to the original size.  Assume that we are resizing and that this button is not under the mouse.
		if (this.Image is not null)
			this.Image = this.UpBitmap;

		base.ScaleControl(factor, specified);
	}

	/// <summary>
	/// Handler for when the mouse enters the ScalingButton
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void MouseEnterCallback(object? sender, EventArgs e)
	{
		if (this.UseCustomGraphic)
		{
			if (this.OverBitmap is not null)
				this.Image = this.OverBitmap;
			else
			{
				this.FlatAppearance.MouseOverBackColor = SystemColors.ButtonHighlight;
				this.Image = this.UpBitmap;
			}
		}
	}

	/// <summary>
	/// Handler for when the ScalingButton Gets Focus.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void GotFocusCallback(object? sender, EventArgs e)
	{
		if (this.UseCustomGraphic)
		{
			if (this.OverBitmap is not null)
				this.Image = this.OverBitmap;
			else
			{
				this.FlatAppearance.MouseOverBackColor = SystemColors.ButtonHighlight;
				this.Image = this.UpBitmap;
			}
		}
	}

	/// <summary>
	/// Handler for when the mouse leaves the ScalingButton
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void MouseLeaveCallback(object? sender, EventArgs e)
	{
		if (this.UseCustomGraphic)
			this.Image = this.UpBitmap;
	}

	/// <summary>
	/// Handler for when the ScalingButton Gets Focus.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void LostFocusCallback(object? sender, EventArgs e)
	{
		if (this.UseCustomGraphic)
			this.Image = this.UpBitmap;
	}

	/// <summary>
	/// Handler for when the mouse button is released.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void MouseUpCallback(object? sender, EventArgs e)
	{
		if (this.UseCustomGraphic)
		{
			if (this.OverBitmap is not null)
				this.Image = this.OverBitmap;
			else
				this.Image = this.UpBitmap;
		}
	}

	/// <summary>
	/// Handler for when the mouse button is pressed.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void MouseDownCallback(object? sender, EventArgs e)
	{
		if (this.UseCustomGraphic)
		{
			if (this.DownBitmap is not null)
				this.Image = this.DownBitmap;
			else
			{
				this.FlatAppearance.MouseDownBackColor = SystemColors.ButtonShadow;
				this.Image = this.UpBitmap;
			}
		}
	}

	/// <summary>
	/// Handler for when the mouse button is pressed.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void TextChangedCallback(object? sender, EventArgs e)
	{
		// Make sure we start somewhere.
		if (this.baseFont is null)
			this.baseFont = this.Font;

		// We changed the font so reset the base font.
		if (this.baseFont.FontFamily != this.Font.FontFamily)
			this.baseFont = this.Font;

		this.SetFittedFont(this.GetScaledButtonTextFont(this.baseFont, this.Width));
	}

	/// <summary>
	/// A font set from outside (a form applying the game font after the text, DPI scaling) is the new base font, and
	/// the text has to fit again: a translation longer than the English text was otherwise drawn too big and cut.
	/// </summary>
	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		if (this.fittingFont)
			return;

		this.baseFont = this.Font;
		this.SetFittedFont(this.GetScaledButtonTextFont(this.baseFont, this.Width));
	}

	/// <summary>
	/// The base font changed: the base font to fit moves to it, not the font fitted from it, which may be smaller.
	/// </summary>
	void IFontRebasing.RebaseFonts(TQVaultAE.Presentation.Win32.Contracts.IFontService fontService)
	{
		var baseFont = fontService.Rebase(this.baseFont ?? this.Font);
		if (ReferenceEquals(baseFont, this.baseFont ?? this.Font))
			return;

		this.baseFont = baseFont;
		this.SetFittedFont(this.GetScaledButtonTextFont(this.baseFont, this.Width));
	}

	/// <summary>
	/// Fits the text again when the button is resized (layout, DPI scaling).
	/// </summary>
	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		if (this.fittingFont || this.baseFont is null)
			return;

		this.SetFittedFont(this.GetScaledButtonTextFont(this.baseFont, this.Width));
	}

	/// <summary>
	/// Sets a font made to fit the text, without taking it as the base font.
	/// </summary>
	private void SetFittedFont(Font? font)
	{
		if (font is null || font.Equals(this.Font))
			return;

		this.fittingFont = true;
		try
		{
			this.Font = font;
		}
		finally
		{
			this.fittingFont = false;
		}
	}

	/// <summary>
	/// Gets the scaled font size for the button text taking into account the size of the button.
	/// </summary>
	/// <param name="font">Font that we attempting to scale</param>
	/// <param name="newWidth">width of the new button.  Used for fit tests.</param>
	/// <returns>Scaled font</returns>
	private Font? GetScaledButtonTextFont(Font? font, int newWidth)
	{
		// Make sure we have something to test.
		if (font is null)
			return null;

		// See if the text can fit on the button and if it does we do not need to do anything.
		if (TextRenderer.MeasureText(this.Text, font).Width < newWidth - 12)
			return font;

		float fontSize = font.Size;

		// Measure bolded text just in case the bolding will cause the text to exceed the bounging box.
		// We do not want the font to change size as we mouse over.
		while (TextRenderer.MeasureText(this.Text, font).Width >= (newWidth - 12) && fontSize > 0.5F)
		{
			fontSize -= 0.5F;
			// By its family: the game fonts are private, a font made from their name was Microsoft Sans Serif.
			font = new Font(font.FontFamily, fontSize, font.Style, font.Unit);
		}

		return font;
	}
}