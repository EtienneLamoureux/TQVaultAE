//-----------------------------------------------------------------------
// <copyright file="ScalingCheckBox.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components
{
	using System.Drawing;
	using System.Windows.Forms;

	/// <summary>
	/// CheckBox class to support scaling of the fonts.
	/// </summary>
	public class ScalingCheckBox : CheckBox
	{
		/// <summary>
		/// Override of ScaleControl which supports font scaling.
		/// </summary>
		/// <param name="factor">SizeF for the scale factor</param>
		/// <param name="specified">BoundsSpecified value.</param>
		protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
		{
			this.Font = new Font(this.Font.FontFamily, this.Font.SizeInPoints * factor.Height, this.Font.Style);

			base.ScaleControl(factor, specified);
		}
	}
}