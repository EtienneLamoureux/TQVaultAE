//-----------------------------------------------------------------------
// <copyright file="VaultProgressBar.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using TQVaultAE.Presentation;

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
		private ScalingLabel scalingLabelTitle;
		private int _value;

		/// <summary>
		/// Initializes a new instance of the VaultProgressBar class.
		/// </summary>
		public VaultProgressBar()
		{
			InitializeComponent();

			this.Minimum = 0;
			this.Maximum = 0;
			this.Title = string.Empty;
			this.Value = 0;
			this.backgroundImage = Resources.ProgressBar;
			this.Size = this.backgroundImage.Size;
			this.fillImage = Resources.ProgressBarFill;

			this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
		}

		private void InitializeComponent()
		{
			this.scalingLabelTitle = new TQVaultAE.GUI.Components.ScalingLabel();
			this.SuspendLayout();
			// 
			// scalingLabelTitle
			// 
			this.scalingLabelTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.scalingLabelTitle.ForeColor = System.Drawing.Color.Gold;
			this.scalingLabelTitle.Location = new System.Drawing.Point(0, 0);
			this.scalingLabelTitle.Name = "scalingLabelTitle";
			this.scalingLabelTitle.Size = new System.Drawing.Size(150, 150);
			this.scalingLabelTitle.TabIndex = 0;
			this.scalingLabelTitle.Text = "Progressing...";
			this.scalingLabelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// VaultProgressBar
			// 
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.scalingLabelTitle);
			this.Name = "VaultProgressBar";
			this.ResumeLayout(false);

		}

		/// <summary>
		/// Gets or sets the minimum value for the progress bar.
		/// </summary>
		public int Minimum
		{
			get => this.minimum;
			set => this.minimum = (value < 0) ? 0 : value;
		}

		/// <summary>
		/// Gets or sets the maximum value for the progress bar.
		/// </summary>
		public int Maximum
		{
			get => this.maximum;
			set => this.maximum = (value < this.Minimum) ? this.Minimum : value;
		}

		/// <summary>
		/// Gets or sets the current value of the progress bar.
		/// </summary>
		public int Value
		{
			get => _value;
			set
			{
				_value = value;
			}
		}

		public Color TitleBackColor { get => this.scalingLabelTitle.BackColor; internal set => this.scalingLabelTitle.BackColor = value; }
		public Color TitleForeColor { get => this.scalingLabelTitle.ForeColor; internal set => this.scalingLabelTitle.ForeColor = value; }
		public Font TitleFont { get => this.scalingLabelTitle.Font; internal set => this.scalingLabelTitle.Font = value; }
		public string Title { get => this.scalingLabelTitle.Text; internal set => this.scalingLabelTitle.Text = value; }

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