//-----------------------------------------------------------------------
// <copyright file="VaultProgressBar.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components
{
	using Properties;
	using System;
	using System.Drawing;
	using System.Windows.Forms;

	/// <summary>
	/// Class which encapsulates the cusom TQ Vault progress bar.
	/// </summary>
	public class VaultProgressBar : UserControl
	{
		/// <summary>
		/// Holds the main image for the progress bar.
		/// </summary>
		private Bitmap backgroundImage;

		/// <summary>
		/// Holds the filler image for the progress bar.
		/// </summary>
		private Bitmap fillImage;

		/// <summary>
		/// Holds the minimum value of the progress bar.
		/// </summary>
		private int minimum;

		/// <summary>
		/// Holds the maximum value of the progress bar.
		/// </summary>
		private int maximum;

		/// <summary>
		/// Initializes a new instance of the VaultProgressBar class.
		/// </summary>
		public VaultProgressBar()
			: base()
		{
			this.Minimum = 0;
			this.Maximum = 0;
			this.Value = 0;
			this.backgroundImage = Resources.ProgressBar;
			this.Size = this.backgroundImage.Size;
			this.fillImage = Resources.ProgressBarFill;

			this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
			this.BackColor = Color.Transparent;
		}

		/// <summary>
		/// Gets or sets the minimum value for the progress bar.
		/// </summary>
		public int Minimum
		{
			get
			{
				return this.minimum;
			}

			set
			{
				if (value < 0)
				{
					this.minimum = 0;
				}
				else
				{
					this.minimum = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the maximum value for the progress bar.
		/// </summary>
		public int Maximum
		{
			get
			{
				return this.maximum;
			}

			set
			{
				if (value < this.Minimum)
				{
					this.maximum = this.Minimum;
				}
				else
				{
					this.maximum = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the current value of the progress bar.
		/// </summary>
		public int Value { get; set; }

		/// <summary>
		/// Increments the progress bar by a specified value.
		/// </summary>
		/// <param name="value">value to increment the progress bar.</param>
		public void Increment(int value)
		{
			this.Value += value;
			this.Invalidate();
		}

		/// <summary>
		/// Handler for the Paint event.
		/// </summary>
		/// <param name="e">PaintEventArgs data</param>
		protected override void OnPaint(PaintEventArgs e)
		{
			if (this.backgroundImage != null)
			{
				Rectangle destRect = new Rectangle(0, 0, this.Width, this.Height);
				e.Graphics.DrawImage(this.backgroundImage, destRect, 0, 0, this.backgroundImage.Width, this.backgroundImage.Height, GraphicsUnit.Pixel);
			}

			if (this.Value > 0 && this.fillImage != null)
			{
				float factor = (float)this.Value / Math.Max(1.0F, (float)this.Maximum);
				Rectangle destRect = new Rectangle(0, 0, Convert.ToInt32((float)this.Width * factor), this.Height);
				e.Graphics.DrawImage(this.fillImage, destRect, 0, 0, Convert.ToInt32((float)this.fillImage.Width * factor), this.fillImage.Height, GraphicsUnit.Pixel);
			}

			base.OnPaint(e);
		}
	}
}