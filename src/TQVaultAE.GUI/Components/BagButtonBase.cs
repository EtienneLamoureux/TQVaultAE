//-----------------------------------------------------------------------
// <copyright file="BagButtonBase.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Tooltip;
	using TQVaultData;

	/// <summary>
	/// Delegate for displaying a tooltip with the bag's contents.
	/// </summary>
	/// <param name="button">instance of this BagButton</param>
	/// <returns>String containing the bag's contents</returns>
	public delegate string GetToolTip(BagButtonBase button);

	/// <summary>
	/// Provides base class for creating and managing sack bag buttons.
	/// </summary>
	public abstract class BagButtonBase : Panel
	{
		/// <summary>
		/// Flag to signal when the mouse is clicked on the button.
		/// Used to display alternate graphic.
		/// </summary>
		private bool isOn;

		/// <summary>
		/// Tooltip delegate used to display summary of bag contents.
		/// </summary>
		private GetToolTip getToolTip;

		/// <summary>
		/// Tooltip instance
		/// </summary>
		private TTLib toolTip;

		/// <summary>
		/// Initializes a new instance of the BagButtonBase class.
		/// </summary>
		/// <param name="bagNumber">number of the bag for display</param>
		/// <param name="getToolTip">Tooltip delegate</param>
		/// <param name="tooltip">Tooltip instance</param>
		protected BagButtonBase(int bagNumber, GetToolTip getToolTip, TTLib tooltip)
		{
			this.getToolTip = getToolTip;
			this.ButtonNumber = bagNumber;
			this.toolTip = tooltip;

			BackColor = Color.Black;

			MouseEnter += new EventHandler(this.MouseEnterCallback);
			MouseLeave += new EventHandler(this.MouseLeaveCallback);
			Paint += new PaintEventHandler(this.PaintCallback);

			// Da_FileServer: Some small paint optimizations.
			this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
		}

		#region BagButton Properties

		/// <summary>
		/// Gets the number assigned to this bag.
		/// </summary>
		public int ButtonNumber { get; private set; }

		/// <summary>
		/// Gets or sets the text displayed on the BagButton.
		/// </summary>
		public string ButtonText { get; set; }

		/// <summary>
		/// Gets or sets the background bitmap when this is the active bag button.
		/// </summary>
		public Bitmap OnBitmap { get; set; }

		/// <summary>
		/// Gets or sets the background bitmap when this is not the active bag button.
		/// </summary>
		public Bitmap OffBitmap { get; set; }

		/// <summary>
		/// Gets or sets the background bitmap when this is the bag button under the mouse.
		/// </summary>
		public Bitmap OverBitmap { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the mouse is hovering over this button.
		/// </summary>
		public bool IsOver { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the bag is being clicked.
		/// </summary>
		public bool IsOn
		{
			get
			{
				return this.isOn;
			}

			set
			{
				if (this.isOn ^ value)
				{
					this.isOn = value;
					Refresh();
				}
			}
		}

		#endregion BagButton Properties

		/// <summary>
		/// Tooltip callback that is used to display a tooltip with the bag's contents.
		/// </summary>
		/// <param name="windowHandle">window handle for parent</param>
		/// <returns>string with the bag's contents</returns>
		public string ToolTipCallback(int windowHandle)
		{
			// see if this is us
			if (this.Handle.ToInt32() == windowHandle)
			{
				// yep.
				GetToolTip temp = this.getToolTip;
				if (temp != null)
				{
					return temp(this);
				}
			}

			return null;
		}

		/// <summary>
		/// Sets the background bitmaps for the BagButton
		/// </summary>
		public abstract void CreateBackgroundGraphics();

		#region BagButton Private Methods

		/// <summary>
		/// Callback for when the mouse enters the BagButton
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MouseEnterCallback(object sender, EventArgs e)
		{
			string tooltip = null;

			if (this.getToolTip != null)
			{
				tooltip = this.getToolTip(this);
			}

			if (this.toolTip != null)
			{
				this.toolTip.ChangeText(tooltip);
			}

			this.IsOver = true;
			Refresh();
		}

		/// <summary>
		/// Handler for when the mouse leaves the BagButton
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MouseLeaveCallback(object sender, EventArgs e)
		{
			this.IsOver = false;
			Refresh();

			// Clear out the tooltip if it is being displayed.
			if (this.toolTip != null)
			{
				this.toolTip.ChangeText(null);
			}
		}

		/// <summary>
		/// Paint callback
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">PaintEventArgs data</param>
		private void PaintCallback(object sender, PaintEventArgs e)
		{
			if (this.OffBitmap == null)
			{
				this.CreateBackgroundGraphics();
			}

			Bitmap bitmap = this.OffBitmap;

			if (this.IsOn)
			{
				bitmap = this.OnBitmap;
			}
			else if (this.IsOver)
			{
				bitmap = this.OverBitmap;
			}

			// Draw the background graphic.
			e.Graphics.DrawImage(bitmap, 0, 0, this.Width, this.Height);

			// Display the text overlay if we have one.
			if (!string.IsNullOrEmpty(this.ButtonText))
			{
				Font font = this.GetScaledButtonTextFont(e.Graphics, Program.GetFontAlbertusMTLight(20.0F * Database.DB.Scale, GraphicsUnit.Pixel));

				if (font != null)
				{
					// If we are mousing over then display the bolded font.
					if (this.IsOver)
					{
						font = new Font(font, FontStyle.Bold);
					}

					StringFormat textFormat = new StringFormat(StringFormatFlags.NoClip);
					textFormat.LineAlignment = StringAlignment.Center;
					textFormat.Alignment = StringAlignment.Center;

					e.Graphics.DrawString(this.ButtonText, font, new SolidBrush(Color.White), new RectangleF(0.0F, 0.0F, (float)this.Width, (float)this.Height), textFormat);
				}
			}
		}

		/// <summary>
		/// Gets the scaled font size for the button text taking into account the size of the button.
		/// </summary>
		/// <param name="graphics">Current graphics instance</param>
		/// <param name="font">Font that we attempting to scale</param>
		/// <returns>Scaled font</returns>
		private Font GetScaledButtonTextFont(Graphics graphics, Font font)
		{
			// Make sure we have something to test.
			if (graphics == null || font == null)
			{
				return null;
			}

			// Make sure we use the bolded font for testing since the passed font may not be bolded.
			Font testFont = new Font(font, FontStyle.Bold);

			// See if the text can fit on the button and if it does we do not need to do anything.
			if (TextRenderer.MeasureText(this.ButtonText, testFont).Width < this.Width)
			{
				return font;
			}

			// Try to get a substring of the button text to find the best size.
			string teststring = this.GetTestString(this.ButtonText);
			float fontSize = font.Size;

			// Measure bolded text just in case the bolding will cause the text to exceed the bounging box.
			// We do not want the font to change size as we mouse over.
			while (TextRenderer.MeasureText(teststring, testFont).Width > this.Width)
			{
				--fontSize;
				font = new Font(font.Name, fontSize, font.Style, font.Unit);

				// Update comparison font.
				testFont = new Font(font.Name, fontSize, testFont.Style, font.Unit);
			}

			// We might need to resize vertically if the line was split.
			if (teststring != this.ButtonText)
			{
				// Use 2xHeight since there are 2 lines of text.
				while ((TextRenderer.MeasureText(teststring, testFont).Height * 2) > this.Height)
				{
					--fontSize;
					font = new Font(font.Name, fontSize, font.Style, font.Unit);

					// Update comparison font.
					testFont = new Font(font.Name, fontSize, testFont.Style, font.Unit);
				}
			}

			return font;
		}

		/// <summary>
		/// Gets a substring of the button text which can be used for font sizing.
		/// Attempts to find the space closest to the middle of the string.
		/// </summary>
		/// <param name="teststring">string that we are testing</param>
		/// <returns>Substring that was split at a space.  Returns the whole string if there was no space.</returns>
		private string GetTestString(string teststring)
		{
			// Make sure we have something to test.
			if (string.IsNullOrEmpty(teststring))
			{
				return string.Empty;
			}

			// Make sure the string has a space before attempting to split it.
			if (!this.ButtonText.Contains(" "))
			{
				return teststring;
			}

			string[] text = this.ButtonText.Split(' ');

			// See if we only had 1 space which means 2 lines
			// and just pick the longer string.
			if (text.Length == 2)
			{
				if (text[0].Length > text[1].Length)
				{
					teststring = text[0];
				}
				else
				{
					teststring = text[1];
				}
			}
			else
			{
				// Otherwise we need to figure out where the break will occur and find the longer string of the two.
				int middle = (this.ButtonText.Length / 2) + (this.ButtonText.Length - ((this.ButtonText.Length / 2) * 2)) - 1;
				int left = this.ButtonText.LastIndexOf(" ", middle, StringComparison.Ordinal);
				int right = this.ButtonText.IndexOf(" ", middle, StringComparison.Ordinal);

				// Use the whole string just in case we don't find a space.
				string testLeft = this.ButtonText;

				// Find the longer string when split at the space to the left of the middle.
				if (left != -1)
				{
					// Skip over the space
					testLeft = this.ButtonText.Substring(left + 1);

					// Make sure we have the longer of the two strings.
					if (left > this.ButtonText.Length - left)
					{
						testLeft = this.ButtonText.Substring(0, left);
					}
				}

				// Use the whole string just in case we don't find a space.
				string testRight = this.ButtonText;

				// Find the longer string when split at the space to the right of the middle.
				if (right != -1)
				{
					// Skip over the space
					testRight = this.ButtonText.Substring(right + 1);

					// Make sure we have the longer of the two strings.
					if (right > this.ButtonText.Length - right)
					{
						testRight = this.ButtonText.Substring(0, right);
					}
				}

				teststring = testLeft;

				// Make sure we take the one closest to the middle.
				// Both values should not be set to whole string.
				if (testRight.Length < testLeft.Length)
				{
					teststring = testRight;
				}
			}

			return teststring;
		}

		#endregion BagButton Private Methods
	}
}