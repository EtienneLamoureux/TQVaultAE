using TQVaultAE.GUI.Components.Rendering;

//-----------------------------------------------------------------------
// <copyright file="ScalingCheckBox.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components.Scaling;

using System;
using System.Drawing;
using System.Windows.Forms;
using CheckBoxState = System.Windows.Forms.VisualStyles.CheckBoxState;

/// <summary>
/// CheckBox class to support scaling of the fonts.
/// </summary>
public class ScalingCheckBox : CheckBox, IScalingControl
{
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
	/// Draws disabled light text dimmed instead of the system dark gray, which is not readable on the dark UI, and the
	/// drop shadow under the text when <see cref="TextShadow"/> is set (the outline with <see cref="TextOutline"/>).
	/// Only the standard left-aligned glyph layout is handled; other layouts keep the system rendering.
	/// </summary>
	protected override void OnPaint(PaintEventArgs pevent)
	{
		bool dimmed = !this.Enabled && DisabledTextPainter.Applies(this.ForeColor);
		if ((!dimmed && !this.TextShadow && !this.TextOutline)
			|| this.Appearance != Appearance.Normal
			|| this.CheckAlign != ContentAlignment.MiddleLeft
			|| this.RightToLeft == RightToLeft.Yes
			|| this.Image is not null) {
			base.OnPaint(pevent);
			return;
		}

		var g = pevent.Graphics;
		var state = this.CheckState switch {
			CheckState.Checked => this.Enabled ? CheckBoxState.CheckedNormal : CheckBoxState.CheckedDisabled,
			CheckState.Indeterminate => this.Enabled ? CheckBoxState.MixedNormal : CheckBoxState.MixedDisabled,
			_ => this.Enabled ? CheckBoxState.UncheckedNormal : CheckBoxState.UncheckedDisabled,
		};

		// Background and glyph come from the normal rendering; the text area is then repainted with the parent
		// background, the shadow and the text drawn over it.
		base.OnPaint(pevent);

		var glyph = CheckBoxRenderer.GetGlyphSize(g, state);
		int textLeft = this.Padding.Left + glyph.Width + TextGap;
		var textBounds = new Rectangle(textLeft, 0, Math.Max(0, this.ClientSize.Width - textLeft), this.ClientSize.Height);
		if (textBounds.Width == 0)
			return;

		ButtonRenderer.DrawParentBackground(g, textBounds, this);

		var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping;
		if (!this.UseMnemonic)
			flags |= TextFormatFlags.NoPrefix;
		if (this.AutoEllipsis)
			flags |= TextFormatFlags.EndEllipsis;
		if (!this.AutoSize)
			flags |= TextFormatFlags.WordBreak;

		if (this.TextShadow || this.TextOutline)
			Rendering.TextShadow.Draw(g, this.Text, this.Font, textBounds, flags, this.TextOutline);

		var color = dimmed ? DisabledTextPainter.ColorFor(this.ForeColor) : this.ForeColor;
		TextRenderer.DrawText(g, this.Text, this.Font, textBounds, color, flags);

		// The repaint above covered the focus rectangle the system drew around the text.
		if (this.Focused && this.ShowFocusCues)
		{
			var size = TextRenderer.MeasureText(g, this.Text, this.Font, textBounds.Size, flags);
			var focus = new Rectangle(textBounds.Left, (this.ClientSize.Height - size.Height) / 2, Math.Min(size.Width, textBounds.Width), size.Height);
			ControlPaint.DrawFocusRectangle(g, focus);
		}
	}

	/// <summary>Gap between the glyph and the text, as drawn by the system checkbox.</summary>
	private const int TextGap = 3;

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