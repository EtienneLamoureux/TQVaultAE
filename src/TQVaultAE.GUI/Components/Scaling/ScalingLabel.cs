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
public class ScalingLabel : Label, IScalingControl, IZoomedFont
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
	/// Gets or sets a value indicating whether the text has a dark outline instead of the drop shadow, as the captions over
	/// a frame's gold border (<see cref="Rendering.TextShadow.DrawOutline"/>).
	/// </summary>
	[System.ComponentModel.DefaultValue(false)]
	public bool TextOutline
	{
		get => this.textOutline;
		set
		{
			this.textOutline = value;
			this.Invalidate();
		}
	}

	private bool textOutline;

	/// <summary>
	/// Gets or sets a value indicating whether a text too long for one line keeps its start and end and loses its middle
	/// ("C:\Games\...\mod"), on one line: for a path, whose last folder matters most. Set <see cref="Label.AutoEllipsis"/>
	/// too for the tool tip of the whole text and an ellipsis at the end of a last folder longer than the line.
	/// </summary>
	[System.ComponentModel.DefaultValue(false)]
	public bool PathEllipsis
	{
		get => this.pathEllipsis;
		set
		{
			this.pathEllipsis = value;
			this.Invalidate();
		}
	}

	private bool pathEllipsis;

	/// <summary>
	/// Draws disabled light text dimmed instead of the system dark gray, which is not readable on the dark UI, and the
	/// drop shadow under the text when <see cref="TextShadow"/> is set.
	/// </summary>
	protected override void OnPaint(PaintEventArgs e)
	{
		bool dimmed = !this.Enabled && DisabledTextPainter.Applies(this.ForeColor);
		if (this.Image is not null || (!dimmed && !this.TextShadow && !this.TextOutline && !this.PathEllipsis)) {
			base.OnPaint(e);
			return;
		}

		var flags = TextFormatFlags.PreserveGraphicsClipping | AlignmentFlags(this.TextAlign);
		if (!this.UseMnemonic)
			flags |= TextFormatFlags.NoPrefix;
		// Wraps like the base label: a fixed size, or an auto size whose width is capped by MaximumSize (measured on
		// several lines, so drawn on several lines too).
		if (this.PathEllipsis)
			flags |= TextFormatFlags.SingleLine | TextFormatFlags.PathEllipsis;
		else if (!this.AutoSize || this.MaximumSize.Width > 0)
			flags |= TextFormatFlags.WordBreak;
		if (this.AutoEllipsis)
			flags |= TextFormatFlags.EndEllipsis;

		var client = this.ClientRectangle;
		var bounds = new Rectangle(
			client.X + this.Padding.Left,
			client.Y + this.Padding.Top,
			client.Width - this.Padding.Horizontal,
			client.Height - this.Padding.Vertical);

		if (this.TextShadow || this.TextOutline)
			Rendering.TextShadow.Draw(e.Graphics, this.Text, this.Font, bounds, flags, this.TextOutline);

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

	/// <inheritdoc/>
	Font IZoomedFont.ZoomedFont
	{
		get => this.Font;
		set => this.Font = value;
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