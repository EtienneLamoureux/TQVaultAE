using TQVaultAE.GUI.Components.Rendering;

//-----------------------------------------------------------------------
// <copyright file="ScalingLabel.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components.Scaling;

using System.Drawing;
using System.Windows.Forms;

/// <summary>
/// Label class to support scaling of the fonts.
/// </summary>
public class ScalingLabel : Label, IScalingControl
{
	/// <summary>
	/// Reverts the basic settings of a control back to the original settings.
	/// </summary>
	/// <param name="location">New Location of the control</param>
	/// <param name="size">New Size of the control</param>
	/// <param name="textColor">Color for the label</param>
	public void Revert(Point location, Size size, Color textColor)
	{
		this.Font = new Font("Microsoft Sans Serif", 8.25F);
		this.Location = location;
		this.Size = size;
		this.ForeColor = textColor;
	}

	/// <summary>
	/// Reverts the basic settings of a control back to the original settings.
	/// </summary>
	/// <param name="location">New Location of the control</param>
	/// <param name="size">New Size of the control</param>
	public void Revert(Point location, Size size)
	{
		this.Revert(location, size, SystemColors.ControlText);
	}

	/// <summary>
	/// Gets or sets a value indicating whether the text has a drop shadow: a dark copy one pixel lower right, which keeps
	/// light text readable over a textured background.
	/// </summary>
	[System.ComponentModel.DefaultValue(false)]
	public bool TextShadow
	{
		get => this.textShadow;
		set
		{
			this.textShadow = value;
			this.Invalidate();
		}
	}

	private bool textShadow;

	/// <summary>
	/// Draws disabled light text dimmed instead of the system dark gray, which is not readable on the dark UI, and the
	/// drop shadow under the text when <see cref="TextShadow"/> is set.
	/// </summary>
	protected override void OnPaint(PaintEventArgs e)
	{
		bool dimmed = !this.Enabled && DisabledTextPainter.Applies(this.ForeColor);
		if (this.Image is not null || (!dimmed && !this.TextShadow)) {
			base.OnPaint(e);
			return;
		}

		var flags = TextFormatFlags.PreserveGraphicsClipping | AlignmentFlags(this.TextAlign);
		if (!this.UseMnemonic)
			flags |= TextFormatFlags.NoPrefix;
		// Wraps like the base label: a fixed size, or an auto size whose width is capped by MaximumSize (measured on
		// several lines, so drawn on several lines too).
		if (!this.AutoSize || this.MaximumSize.Width > 0)
			flags |= TextFormatFlags.WordBreak;
		if (this.AutoEllipsis)
			flags |= TextFormatFlags.EndEllipsis;

		var client = this.ClientRectangle;
		var bounds = new Rectangle(
			client.X + this.Padding.Left,
			client.Y + this.Padding.Top,
			client.Width - this.Padding.Horizontal,
			client.Height - this.Padding.Vertical);

		if (this.TextShadow)
			Rendering.TextShadow.DrawShadow(e.Graphics, this.Text, this.Font, bounds, flags);

		var color = dimmed ? DisabledTextPainter.ColorFor(this.ForeColor) : this.ForeColor;
		TextRenderer.DrawText(e.Graphics, this.Text, this.Font, bounds, color, flags);
	}

	private static TextFormatFlags AlignmentFlags(ContentAlignment alignment)
	{
		var horizontal = alignment switch {
			ContentAlignment.TopCenter or ContentAlignment.MiddleCenter or ContentAlignment.BottomCenter => TextFormatFlags.HorizontalCenter,
			ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight => TextFormatFlags.Right,
			_ => TextFormatFlags.Left,
		};
		var vertical = alignment switch {
			ContentAlignment.MiddleLeft or ContentAlignment.MiddleCenter or ContentAlignment.MiddleRight => TextFormatFlags.VerticalCenter,
			ContentAlignment.BottomLeft or ContentAlignment.BottomCenter or ContentAlignment.BottomRight => TextFormatFlags.Bottom,
			_ => TextFormatFlags.Top,
		};
		return horizontal | vertical;
	}

	/// <summary>
	/// Override of ScaleControl which supports font scaling.
	/// </summary>
	/// <param name="factor">SizeF for the scale factor</param>
	/// <param name="specified">BoundsSpecified value.</param>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		this.Font = ZoomScope.ScaleFont(this.Font, factor.Height);

		base.ScaleControl(factor, specified);
	}
}