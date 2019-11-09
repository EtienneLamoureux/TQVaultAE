//-----------------------------------------------------------------------
// <copyright file="ScalingComboBox.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components
{
	using System;
	using System.Linq;
	using System.Drawing;
	using System.Windows.Forms;
	using TQVaultAE.Presentation;

	/// <summary>
	/// ComboBox class to support scaling of the fonts.
	/// </summary>
	public class ScalingComboBox : ComboBox
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

		private void ScalingComboBox_DrawItem(object sender, DrawItemEventArgs e)
		{
			if (this.Items.Count == 0 || e.Index == -1) return;
			var itemsValues = this.Items.Cast<object>().Select(o => o?.ToString() ?? string.Empty).ToArray();
			var currentBrush = new SolidBrush(this.ForeColor);

			// Draw the background of the item.
			e.DrawBackground();

			// Draw value
			e.Graphics.DrawString(itemsValues[e.Index], this.Font, currentBrush, new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));

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
			this.Font = new Font(this.Font.FontFamily, this.Font.SizeInPoints * factor.Height, this.Font.Style);
			base.ScaleControl(factor, specified);
		}
	}
}