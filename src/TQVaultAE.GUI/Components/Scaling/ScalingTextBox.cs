//-----------------------------------------------------------------------
// <copyright file="ScalingTextBox.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components.Scaling;

using System.Drawing;
using System.Windows.Forms;

/// <summary>
/// TextBox class to support scaling of the fonts.
/// </summary>
public class ScalingTextBox : TextBox, IScalingControl, IZoomedFont
{
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