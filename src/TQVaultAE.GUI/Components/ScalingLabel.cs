//-----------------------------------------------------------------------
// <copyright file="ScalingLabel.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components
{
	using System.Drawing;
	using System.Windows.Forms;

	/// <summary>
	/// Label class to support scaling of the fonts.
	/// </summary>
	public class ScalingLabel : Label
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