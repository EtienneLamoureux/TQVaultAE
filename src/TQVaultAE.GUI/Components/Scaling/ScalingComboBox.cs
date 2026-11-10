﻿//-----------------------------------------------------------------------
// <copyright file="ScalingComboBox.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components.Scaling;

using System.Drawing;
using System.Windows.Forms;

/// <summary>
/// ComboBox class to support scaling of the fonts.
/// </summary>
public class ScalingComboBox : ComboBox, IScalingControl
{
	public ScalingComboBox() 
	{
		// Take care by myself of items drawing because of cyrillic having trouble to display properly with Albertus MT when DrawMode = Normal
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		this.SuspendLayout();
		// 
		// ScalingComboBox
		// 
		this.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
		this.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ScalingComboBox_DrawItem);
		this.ResumeLayout(false);
	}

	/// <summary>
	/// Text drawn in brackets after an item, from its text (the mod of a vault); null or empty for none. The items keep
	/// their own text.
	/// </summary>
	[System.ComponentModel.Browsable(false)]
	[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
	public Func<string, string?>? ItemSuffix { get; set; }

	private void ScalingComboBox_DrawItem(object? sender, DrawItemEventArgs e)
	{
		if (this.Items.Count == 0 || e.Index == -1) return;
		// Only the drawn item: converting the whole list for each item made opening the list quadratic.
		var text = this.Items[e.Index]?.ToString() ?? string.Empty;
		if (this.ItemSuffix?.Invoke(text) is { Length: > 0 } suffix)
			text = $"{text} [{suffix}]";
		using var currentBrush = new SolidBrush(this.ForeColor);

		// Draw the background of the item.
		e.DrawBackground();

		// Draw value
		e.Graphics.DrawString(text, this.Font, currentBrush, new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));

		// Draw the focus rectangle if the mouse hovers over an item.
		e.DrawFocusRectangle();
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