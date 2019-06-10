//-----------------------------------------------------------------------
// <copyright file="ScalingButton.cs" company="None">
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
	/// Button class to support skinning and scaling of the fonts.
	/// </summary>
	public class ScalingButton : Button
	{
		/// <summary>
		/// Base Font used for scaling of the text to fit within the bounds of the button.
		/// </summary>
		private Font baseFont;

		/// <summary>
		/// Used to indicate that the custom button graphic is being used.
		/// </summary>
		private bool useCustomGraphic;

		/// <summary>
		/// Used to indicate whether the button size is forced to be the same as the custom graphic.
		/// </summary>
		private bool sizeToGraphic;

		/// <summary>
		/// Initializes a new instance of the ScalingButton class.
		/// </summary>
		public ScalingButton()
			: base()
		{
			this.DownBitmap = Resources.MainButtonDown;
			this.OverBitmap = Resources.MainButtonOver;
			this.UpBitmap = Resources.MainButtonUp;

			////this.Font = GetScaledButtonTextFont(this.Font);

			TextChanged += new EventHandler(this.TextChangedCallback);
			GotFocus += new EventHandler(this.GotFocusCallback);
			LostFocus += new EventHandler(this.LostFocusCallback);
			MouseEnter += new EventHandler(this.MouseEnterCallback);
			MouseLeave += new EventHandler(this.MouseLeaveCallback);
			MouseDown += new MouseEventHandler(this.MouseDownCallback);
			MouseUp += new MouseEventHandler(this.MouseUpCallback);
		}

		/// <summary>
		/// Gets or sets a value indicating whether the custom button graphic is shown.
		/// </summary>
		public bool UseCustomGraphic
		{
			get
			{
				return this.useCustomGraphic;
			}

			set
			{
				if (value)
				{
					this.BackColor = Color.Transparent;
					this.FlatStyle = FlatStyle.Flat;
					this.FlatAppearance.BorderSize = 0;
					this.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, (int)((byte)51), (int)((byte)44), (int)((byte)28));
					this.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, (int)((byte)51), (int)((byte)44), (int)((byte)28));
					if (this.UpBitmap != null)
					{
						this.Image = this.UpBitmap;
					}
				}
				else
				{
					this.BackColor = Color.Gold;
					this.FlatStyle = FlatStyle.Standard;
					this.FlatAppearance.BorderSize = 1;
					this.FlatAppearance.MouseOverBackColor = SystemColors.ButtonHighlight;
					this.FlatAppearance.MouseDownBackColor = SystemColors.ButtonShadow;
					this.Image = null;
				}

				this.useCustomGraphic = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the button size is forced to be the same as the custom graphic.
		/// </summary>
		public bool SizeToGraphic
		{
			get
			{
				return this.sizeToGraphic;
			}

			set
			{
				if (value && this.UpBitmap != null)
				{
					this.Size = this.UpBitmap.Size;
				}

				this.sizeToGraphic = value;
			}
		}

		/// <summary>
		/// Gets or sets the Bitmap used when the button is pressed.
		/// </summary>
		public Bitmap DownBitmap { get; set; }

		/// <summary>
		/// Gets or sets the Bitmap used when the mouse hovers over the button.
		/// </summary>
		public Bitmap OverBitmap { get; set; }

		/// <summary>
		/// Gets or sets the Bitmap used when the button neither highlighted or pressed.
		/// </summary>
		public Bitmap UpBitmap { get; set; }

		/// <summary>
		/// Reverts the basic settings of a control back to the original settings.
		/// </summary>
		/// <param name="location">New Location of the control</param>
		/// <param name="size">New Size of the control</param>
		public void Revert(Point location, Size size)
		{
			this.Font = new Font("Microsoft Sans Serif", 8.25F);
			this.Location = location;
			this.Size = size;
			this.ForeColor = SystemColors.ControlText;
			this.UseCustomGraphic = false;
		}

		/// <summary>
		/// Handles the ScalingButton Paint Event.
		/// </summary>
		/// <param name="pevent">PaintEventArgs data</param>
		protected override void OnPaint(PaintEventArgs pevent)
		{
			if (this.UseCustomGraphic && this.Image != null && this.Size != this.Image.Size)
			{
				// Scaling smaller can make the outside border disappear.
				// To avoid this we scale the image and then copy a 3 pixel strip from each edge to restore the border.
				if (this.Width < this.Image.Width)
				{
					int width = 4;
					Bitmap newImage = new Bitmap(this.Width, this.Height);
					Graphics graphics = Graphics.FromImage(newImage);

					// First draw the new image scaled to the size of the button.
					graphics.DrawImage(this.Image, new Rectangle(new Point(0, 0), this.Size));

					// Grab the left border from the source image and paint it onto the new image.
					graphics.DrawImage(this.Image, new Rectangle(0, 0, width, this.Height), new Rectangle(0, 0, width, this.Image.Height), GraphicsUnit.Pixel);

					// Now grab the right border from the source image and paint it onto the new image.
					graphics.DrawImage(
						this.Image,
						new Rectangle(this.Width - width, 0, width, this.Height),
						new Rectangle(this.Image.Width - width, 0, width, this.Image.Height),
						GraphicsUnit.Pixel);

					this.Image = newImage;
				}
				else
				{
					// Just scale the image normally.
					this.Image = new Bitmap(this.Image, this.Size);
				}
			}

			base.OnPaint(pevent);
		}

		/// <summary>
		/// Override of ScaleControl which supports font scaling.
		/// </summary>
		/// <param name="factor">SizeF for the scale factor</param>
		/// <param name="specified">BoundsSpecified value.</param>
		protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
		{
			// Get a reference for scaling.
			if (this.baseFont == null)
			{
				this.baseFont = this.Font;
			}

			// We changed the font so reset the base font.
			if (this.baseFont.FontFamily != this.Font.FontFamily)
			{
				this.baseFont = this.Font;
			}

			this.baseFont = new Font(this.baseFont.FontFamily, this.baseFont.SizeInPoints * factor.Height, this.baseFont.Style);
			this.Font = this.GetScaledButtonTextFont(this.baseFont, Convert.ToInt32((float)this.Width * factor.Width));

			// Reset the image to the original size.  Assume that we are resizing and that this button is not under the mouse.
			if (this.Image != null)
			{
				this.Image = this.UpBitmap;
			}

			base.ScaleControl(factor, specified);
		}

		/// <summary>
		/// Handler for when the mouse enters the ScalingButton
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MouseEnterCallback(object sender, EventArgs e)
		{
			if (this.UseCustomGraphic)
			{
				if (this.OverBitmap != null)
				{
					this.Image = this.OverBitmap;
				}
				else
				{
					this.FlatAppearance.MouseOverBackColor = SystemColors.ButtonHighlight;
					this.Image = this.UpBitmap;
				}
			}
		}

		/// <summary>
		/// Handler for when the ScalingButton Gets Focus.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void GotFocusCallback(object sender, EventArgs e)
		{
			if (this.UseCustomGraphic)
			{
				if (this.OverBitmap != null)
				{
					this.Image = this.OverBitmap;
				}
				else
				{
					this.FlatAppearance.MouseOverBackColor = SystemColors.ButtonHighlight;
					this.Image = this.UpBitmap;
				}
			}
		}

		/// <summary>
		/// Handler for when the mouse leaves the ScalingButton
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MouseLeaveCallback(object sender, EventArgs e)
		{
			if (this.UseCustomGraphic)
			{
				this.Image = this.UpBitmap;
			}
		}

		/// <summary>
		/// Handler for when the ScalingButton Gets Focus.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void LostFocusCallback(object sender, EventArgs e)
		{
			if (this.UseCustomGraphic)
			{
				this.Image = this.UpBitmap;
			}
		}

		/// <summary>
		/// Handler for when the mouse button is released.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MouseUpCallback(object sender, EventArgs e)
		{
			if (this.UseCustomGraphic)
			{
				if (this.OverBitmap != null)
				{
					this.Image = this.OverBitmap;
				}
				else
				{
					this.Image = this.UpBitmap;
				}
			}
		}

		/// <summary>
		/// Handler for when the mouse button is pressed.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MouseDownCallback(object sender, EventArgs e)
		{
			if (this.UseCustomGraphic)
			{
				if (this.DownBitmap != null)
				{
					this.Image = this.DownBitmap;
				}
				else
				{
					this.FlatAppearance.MouseDownBackColor = SystemColors.ButtonShadow;
					this.Image = this.UpBitmap;
				}
			}
		}

		/// <summary>
		/// Handler for when the mouse button is pressed.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void TextChangedCallback(object sender, EventArgs e)
		{
			// Make sure we start somewhere.
			if (this.baseFont == null)
			{
				this.baseFont = this.Font;
			}

			// We changed the font so reset the base font.
			if (this.baseFont.FontFamily != this.Font.FontFamily)
			{
				this.baseFont = this.Font;
			}

			this.Font = this.GetScaledButtonTextFont(this.baseFont, this.Width);
		}

		/// <summary>
		/// Gets the scaled font size for the button text taking into account the size of the button.
		/// </summary>
		/// <param name="font">Font that we attempting to scale</param>
		/// <param name="newWidth">width of the new button.  Used for fit tests.</param>
		/// <returns>Scaled font</returns>
		private Font GetScaledButtonTextFont(Font font, int newWidth)
		{
			// Make sure we have something to test.
			if (font == null && newWidth > 0)
			{
				return null;
			}

			// See if the text can fit on the button and if it does we do not need to do anything.
			if (TextRenderer.MeasureText(this.Text, font).Width < newWidth - 12)
			{
				return font;
			}

			float fontSize = font.Size;

			// Measure bolded text just in case the bolding will cause the text to exceed the bounging box.
			// We do not want the font to change size as we mouse over.
			while (TextRenderer.MeasureText(this.Text, font).Width >= (newWidth - 12) && fontSize > 0.5F)
			{
				fontSize -= 0.5F;
				font = new Font(font.Name, fontSize, font.Style, font.Unit);
			}

			return font;
		}
	}
}