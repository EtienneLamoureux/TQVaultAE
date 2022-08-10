//-----------------------------------------------------------------------
// <copyright file="BagButtonBase.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.Windows.Forms;
using TQVaultAE.GUI.Tooltip;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Helpers;
using System.Linq;

namespace TQVaultAE.GUI.Components
{
	/// <summary>
	/// Delegate for displaying a tooltip with the bag's contents.
	/// </summary>
	/// <param name="button">instance of this BagButton</param>
	/// <returns>String containing the bag's contents</returns>
	public delegate void GetToolTip(BagButtonBase button);

	/// <summary>
	/// Provides base class for creating and managing sack bag buttons.
	/// </summary>
	public class BagButtonBase : Panel
	{
		protected readonly IServiceProvider ServiceProvider;
		protected readonly IFontService FontService;
		protected readonly IUIService UIService;
		private readonly SessionContext userContext;
		internal SackCollection Sack;

		private BagButtonIconInfo _DefaultIconInfo = new BagButtonIconInfo()
		{
			DisplayMode = BagButtonDisplayMode.Default,
		};

		internal BagButtonIconInfo CurrentIconInfo
		{
			get
			{
				var info = this.Sack?.BagButtonIconInfo;
				if (info is null) return _DefaultIconInfo;
				return info;
			}
		}

		/// <summary>
		/// Apply custom bitmap from config file
		/// </summary>
		internal void ApplyIconInfo(SackCollection sack)
		{
			this.Sack = sack;
			var info = this.CurrentIconInfo;

			ApplyDefaultIcon();

			// custom config detected
			if (info.DisplayMode != BagButtonDisplayMode.Default)
			{
				// Load custom
				this.OnBitmap = RecordId.IsNullOrEmpty(info.On) ? null : UIService.LoadBitmap(info.On);
				this.OffBitmap = RecordId.IsNullOrEmpty(info.Off) ? null : UIService.LoadBitmap(info.Off);
				this.OverBitmap = RecordId.IsNullOrEmpty(info.Over) ? null : UIService.LoadBitmap(info.Over);

				if (this.OnBitmap is not null && this.OffBitmap is not null) return;// Done

				// Bitmap not found ?? Should not happen except missing DLC and Vault file sharing
				ApplyDefaultIcon();
			}

			this.Refresh();
		}

		internal void ApplyDefaultIcon()
		{
			// Apply default config
			this.OnBitmap = this.OffBitmap = this.OverBitmap = null;// Reset
			this.CreateBackgroundGraphics();
		}

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
		/// Gets or sets the border for item highlight.
		/// </summary>
		public Pen HighlightSearchItemBorder { get; protected set; }


		public BagButtonBase()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// BagButtonBase
			// 
			this.BackColor = System.Drawing.Color.Transparent;
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintCallback);
			this.MouseEnter += new System.EventHandler(this.MouseEnterCallback);
			this.MouseLeave += new System.EventHandler(this.MouseLeaveCallback);
			this.ResumeLayout(false);

		}

		/// <summary>
		/// Initializes a new instance of the BagButtonBase class.
		/// </summary>
		/// <param name="bagNumber">number of the bag for display</param>
		/// <param name="getToolTip">Tooltip delegate</param>
		public BagButtonBase(int bagNumber, GetToolTip getToolTip, IServiceProvider serviceProvider)
		{
			InitializeComponent();

			this.ServiceProvider = serviceProvider;
			this.FontService = this.ServiceProvider.GetService<IFontService>();
			this.UIService = this.ServiceProvider.GetService<IUIService>();
			this.userContext = this.ServiceProvider.GetService<SessionContext>();

			this.getToolTip = getToolTip;
			this.ButtonNumber = bagNumber;

			this.HighlightSearchItemBorder = new Pen(this.userContext.HighlightSearchItemBorderColor)
			{
				Width = 4,
			};

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
			get => this.isOn;

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
		public void ToolTipCallback(int windowHandle)
		{
			// see if this is us
			if (this.Handle.ToInt32() == windowHandle)
			{
				// yep.
				var temp = this.getToolTip;
				if (temp != null)
					temp(this);
			}
		}

		/// <summary>
		/// Sets the background bitmaps for the BagButton
		/// </summary>
		public virtual void CreateBackgroundGraphics()
			=> throw new NotImplementedException();

		#region BagButton Private Methods

		/// <summary>
		/// Callback for when the mouse enters the BagButton
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MouseEnterCallback(object sender, EventArgs e)
		{
			BagButtonLabelTooltip.ShowTooltip(this.ServiceProvider, this);

			if (this.getToolTip != null)
			{
				// Disable Tooltip bag
				var panel = this.getToolTip.Target as Panel;
				this.getToolTip(this);

				if (panel is StashPanel sp)
				{
					switch (this.ButtonNumber)
					{
						case StashPanel.BAGID_EQUIPMENTPANEL when !Config.Settings.Default.DisableTooltipEquipment:
						case StashPanel.BAGID_PLAYERSTASH when !Config.Settings.Default.DisableTooltipStash:
						case StashPanel.BAGID_RELICVAULTSTASH when !Config.Settings.Default.DisableTooltipRelic:
						case StashPanel.BAGID_TRANSFERSTASH when !Config.Settings.Default.DisableTooltipTransfer:
							BagButtonTooltip.ShowTooltip(this.ServiceProvider, this);
							break;
					}
				}
				else if (
					(panel is VaultPanel vp && vp.Vault is not null && !vp.Vault.DisabledTooltipBagId.Contains(this.ButtonNumber)) // Vault Only
					|| panel is PlayerPanel || panel is SackPanel || panel is StashPanel // Others
				)
				{
					BagButtonTooltip.ShowTooltip(this.ServiceProvider, this);
				}
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
			BagButtonTooltip.HideTooltip();
			BagButtonLabelTooltip.HideTooltip();
		}

		/// <summary>
		/// Draws the background of the items in the panel during a Paint call.
		/// </summary>
		/// <param name="e">PaintEventArgs data</param>
		protected virtual void PaintHighlightAreaUnderButton(PaintEventArgs e)
		{
			// Highlight search
			using (SolidBrush brush = new SolidBrush(Color.FromArgb(127, this.userContext.HighlightSearchItemColor)))
			{
				e.Graphics.FillRectangle(brush, 0, 0, this.Width, this.Height);
			}
		}

		/// <summary>
		/// Paint callback
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">PaintEventArgs data</param>
		private void PaintCallback(object sender, PaintEventArgs e)
		{
			if (this.OffBitmap is null)
				this.CreateBackgroundGraphics();

			Bitmap bitmap = this.OffBitmap;

			if (this.IsOn)
			{
				bitmap = this.OnBitmap;
			}
			else if (this.IsOver)
			{
				if (this.OverBitmap is null)
					bitmap = this.OffBitmap;// use off state
				else
					bitmap = this.OverBitmap;
			}

			bool highlight = false;
			if (this is not AutoSortButton && this.Sack is not null)
			{
				highlight = this.userContext.HighlightedItems.Count > 0
					&& this.userContext.HighlightedItems.Intersect(this.Sack.ToList()).Any();
			}

			if (highlight) PaintHighlightAreaUnderButton(e);

			// Draw the icon
			Image bmp;
			if (this.Parent is VaultPanel vp && vp.Vault is not null)
				bmp = bitmap.ResizeImage(this.Width, this.Height, maintainAspectRatio: true);
			else
				bmp = bitmap;

			e.Graphics.DrawImage(bmp, 0, 0, this.Width, this.Height);

			if (// If it's not a vault with a number restriction
				!(this.CurrentIconInfo.DisplayMode != BagButtonDisplayMode.Default // Vault Only
					&& !this.CurrentIconInfo.DisplayMode.HasFlag(BagButtonDisplayMode.Number)// No Number
				)
			)
			{
				// Display the text overlay if we have one.
				if (!string.IsNullOrEmpty(this.ButtonText))// ButtonText is a number for Vault
				{
					Font font = this.GetScaledButtonTextFont(e.Graphics, FontService.GetFontLight(20.0F * UIService.Scale, GraphicsUnit.Pixel));

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

			if (highlight) PaintHighlightAreaOnTopOfButton(e);
		}

		private void PaintHighlightAreaOnTopOfButton(PaintEventArgs e)
		{
			// Add highlight borders
			e.Graphics.DrawRectangle(this.HighlightSearchItemBorder, 0, 0, this.Width, this.Height);
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
			Font testFont = FontService.GetFont(font.Size, FontStyle.Bold, GraphicsUnit.Pixel, font.GdiCharSet);

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
			while (TextRenderer.MeasureText(teststring, testFont).Width >= this.Width)
			{
				--fontSize;
				font = FontService.GetFont(fontSize, font.Style, GraphicsUnit.Pixel, font.GdiCharSet);

				// Update comparison font.
				testFont = FontService.GetFont(fontSize, testFont.Style, GraphicsUnit.Pixel, testFont.GdiCharSet);
			}

			// We might need to resize vertically if the line was split.
			if (teststring != this.ButtonText)
			{
				// Use 2xHeight since there are 2 lines of text.
				while ((TextRenderer.MeasureText(teststring, testFont).Height * 2) >= this.Height)
				{
					--fontSize;
					font = FontService.GetFont(fontSize, font.Style, GraphicsUnit.Pixel, font.GdiCharSet);

					// Update comparison font.
					testFont = FontService.GetFont(fontSize, testFont.Style, GraphicsUnit.Pixel, testFont.GdiCharSet);

					// Check to see if the whole string will fit inside the button on a single line.
					if (TextRenderer.MeasureText(this.ButtonText, testFont).Width < this.Width)
					{
						break;
					}
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
