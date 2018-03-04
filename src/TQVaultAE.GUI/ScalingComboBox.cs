//-----------------------------------------------------------------------
// <copyright file="ScalingComboBox.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;

	/// <summary>
	/// ComboBox class to support scaling of the fonts.
	/// </summary>
	public class ScalingComboBox : ComboBox
	{
		private static int WM_PAINT = 0x000F;

		public ScalingComboBox()
		{
			this.FlatStyle = FlatStyle.Popup;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
			this.ForeColor = System.Drawing.Color.White;
			this.DrawMode = DrawMode.OwnerDrawFixed;
			this.DrawItem += ScalingComboboxDrawItem;
			this.Font = Program.GetFontAlbertusMTLight(8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
		}

		/// <summary>
		/// Reverts the basic settings of a control back to the original settings.
		/// </summary>
		/// <param name="location">New Location of the control</param>
		/// <param name="size">New Size of the control</param>
		public void Revert(Point location, Size size)
		{
			this.Location = location;
			this.Size = size;
			this.FlatStyle = FlatStyle.Popup;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
			this.ForeColor = System.Drawing.Color.White;
			this.Font = Program.GetFontAlbertusMTLight(8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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

		private void ScalingComboboxDrawItem(object sender, DrawItemEventArgs e)
		{
			var combo = sender as ComboBox;

			if (e.Index < 0) return;
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
			{
				e.Graphics.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(195)))), ((int)(((byte)(112)))))), e.Bounds);
			}
			else
			{
				e.Graphics.FillRectangle(new SolidBrush(this.BackColor), e.Bounds);
			}
			e.Graphics.DrawString(combo.Items[e.Index].ToString(),
										  e.Font,
										  new SolidBrush(Color.White),
										  new Point(e.Bounds.X, e.Bounds.Y));

		}

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);

			Graphics g = Graphics.FromHwnd(Handle);

			if (m.Msg == WM_PAINT)
			{
				Rectangle bounds = new Rectangle(0, 0, Width, Height);
				ControlPaint.DrawBorder(g, bounds, Color.FromArgb(223, 188, 97), ButtonBorderStyle.Solid);
			}
		}
	}
}