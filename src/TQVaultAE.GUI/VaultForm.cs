//-----------------------------------------------------------------------
// <copyright file="VaultForm.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using Properties;
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.Security.Permissions;
	using System.Windows.Forms;
	using TQVaultData;

	/// <summary>
	/// Abstract class used for constructing TQVault themed forms.
	/// </summary>
	public partial class VaultForm : Form
	{
		/// <summary>
		/// Holds the last state of the form.  Used to support maximizing.
		/// </summary>
		private FormWindowState lastState;

		/// <summary>
		/// Bitmap graphic for the top border
		/// </summary>
		private Bitmap topBorder;

		/// <summary>
		/// Bitmap graphic for the bottom border
		/// </summary>
		private Bitmap bottomBorder;

		/// <summary>
		/// Bitmap graphic for the side borders
		/// </summary>
		private Bitmap sideBorder;

		/// <summary>
		/// Bitmap graphic for the bottom right corner
		/// </summary>
		private Bitmap bottomRightCorner;

		/// <summary>
		/// Bitmap graphic for the bottom left
		/// </summary>
		private Bitmap bottomLeftCorner;

		/// <summary>
		/// Flag to indicate whether custom border will be drawn
		/// </summary>
		private bool drawCustomBorder;

		/// <summary>
		/// Bounding Rectangle for the upper left corner of the form.
		/// </summary>
		private Rectangle upperLeftCorner;

		/// <summary>
		/// Bounding Rectangle for the upper right corner of the form
		/// </summary>
		private Rectangle upperRightCorner;

		/// <summary>
		/// Bounding Rectangle for the lower left corner of the form.
		/// </summary>
		private Rectangle lowerLeftCorner;

		/// <summary>
		/// Bounding Rectangle for the lower right corner of the form.
		/// </summary>
		private Rectangle lowerRightCorner;

		/// <summary>
		/// Bounding Rectangle for the top border of the form.
		/// </summary>
		private Rectangle topBorderRect;

		/// <summary>
		/// Bounding Rectangle for the bottom border of the form.
		/// </summary>
		private Rectangle bottomBorderRect;

		/// <summary>
		/// Bounding Rectangle for the right border of the form.
		/// </summary>
		private Rectangle rightBorderRect;

		/// <summary>
		/// Bounding Rectangle for the left border of the form.
		/// </summary>
		private Rectangle leftBorderRect;

		/// <summary>
		/// Bounding Rectangle for the top of the form used for resizing.
		/// </summary>
		private Rectangle topResizeRect;

		/// <summary>
		/// Font used to draw the title.
		/// </summary>
		private Font titleFont;

		/// <summary>
		/// WindowMenu used to display the system menu.
		/// </summary>
		private WindowMenu systemMenu;

		/// <summary>
		/// Indicates whether resizing is allowed when custom borders are enabled.
		/// </summary>
		private bool resizeCustomAllowed;

		/// <summary>
		/// Initializes a new instance of the VaultForm class.
		/// </summary>
		protected VaultForm()
		{
			this.topBorder = Resources.BorderTop;
			this.bottomBorder = Resources.BorderBottom;
			this.sideBorder = Resources.BorderSide;
			this.bottomRightCorner = Resources.BorderBottomRightCorner;
			this.bottomLeftCorner = Resources.BorderBottomLeftCorner;
			this.ShowResizeBorders = false;
			this.titleFont = Program.GetFontAlbertusMTLight(9.5F);
			this.TitleTextColor = SystemColors.ControlText;

			this.InitializeComponent();

			this.PlaceButtons();

			this.CreateBorderRects();

			#region Apply custom font & scaling

			ScaleControl(this.buttonMaximize);
			ScaleControl(this.buttonMinimize);
			ScaleControl(this.buttonClose);

			#endregion

			this.systemMenu = new WindowMenu(this);
			this.systemMenu.SystemEvent += new EventHandler<WindowMenuEventArgs>(this.SystemMenuSystemEvent);
			this.systemMenu.MaximizeEnabled = this.MaximizeBox;
			this.systemMenu.MinimizeEnabled = this.MinimizeBox;

			// to avoid flickering use double buffer and to force control to use OnPaint
			this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
		}

		protected void ScaleControl(System.Windows.Forms.Control ctrl)
		{
			ctrl.Location = this.ScalePoint(ctrl.Location);
			ctrl.Size = this.ScaleSize(ctrl.Size);
		}

		protected Size ScaleSize(Size size)
		{
			if (TQVaultData.Database.DB == null) return size;
			return new Size((int)System.Math.Round(size.Width * TQVaultData.Database.DB.Scale), (int)System.Math.Round(size.Height * TQVaultData.Database.DB.Scale));
		}

		protected Point ScalePoint(Point point)
		{
			if (TQVaultData.Database.DB == null) return point;
			return new Point((int)System.Math.Round(point.X * TQVaultData.Database.DB.Scale), (int)System.Math.Round(point.Y * TQVaultData.Database.DB.Scale));
		}


		/// <summary>
		/// Gets the MessageBoxOptions for right to left reading.
		/// </summary>
		public static MessageBoxOptions RightToLeftOptions
		{
			get
			{
				// Set options for Right to Left reading.
				if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
				{
					return MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
				}

				return (MessageBoxOptions)0;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether or not the resize border boxes are shown.
		/// </summary>
		public bool ShowResizeBorders { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the form resizing will be constrained to the original form's width and height ratio.
		/// </summary>
		public bool ConstrainToDesignRatio { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the form will be scaled when it is resized.
		/// </summary>
		public bool ScaleOnResize { get; set; }

		/// <summary>
		/// Gets or sets the original scale for resetting the form scaling.
		/// </summary>
		public float OriginalFormScale { get; set; }

		/// <summary>
		/// Gets or sets the the ratio of height / width before any scaling.
		/// </summary>
		public float FormDesignRatio { get; set; }

		/// <summary>
		/// Gets or sets the size of the form after the initial scaling.
		/// </summary>
		public Size OriginalFormSize { get; set; }

		/// <summary>
		/// Gets or sets the size of the form prior to the current scaling operation.
		/// </summary>
		public Size LastFormSize { get; set; }

		/// <summary>
		/// Gets or sets the upper bounds for the form size.
		/// </summary>
		public Size FormMaximumSize { get; set; }

		/// <summary>
		/// Gets or sets the lower bounds for the form size
		/// </summary>
		public Size FormMinimumSize { get; set; }

		/// <summary>
		/// Gets or sets the color for the text in the title bar
		/// </summary>
		public Color TitleTextColor { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the custom borders will be drawn.
		/// </summary>
		public bool DrawCustomBorder
		{
			get
			{
				return this.drawCustomBorder;
			}

			set
			{
				if (value)
				{
					this.FormBorderStyle = FormBorderStyle.None;

					this.buttonClose.Visible = true;
					this.buttonClose.Enabled = true;

					this.buttonMaximize.Visible = this.MaximizeBox;
					this.buttonMaximize.Enabled = this.MaximizeBox;

					this.buttonMinimize.Visible = this.MinimizeBox;
					this.buttonMinimize.Enabled = this.MinimizeBox;
				}
				else
				{
					this.FormBorderStyle = FormBorderStyle.FixedSingle;
					this.buttonMaximize.Visible = false;
					this.buttonMaximize.Enabled = false;
					this.buttonMinimize.Visible = false;
					this.buttonMinimize.Enabled = false;
					this.buttonClose.Visible = false;
					this.buttonClose.Enabled = false;
				}

				this.drawCustomBorder = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether resizing is allowed when Custom Borders are enabled.
		/// </summary>
		public bool ResizeCustomAllowed
		{
			get
			{
				return this.resizeCustomAllowed;
			}

			set
			{
				if (this.systemMenu != null)
				{
					this.systemMenu.SizeEnabled = value;
				}

				this.resizeCustomAllowed = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the Maximize button is displayed in the caption bar of the form.
		/// </summary>
		public new bool MaximizeBox
		{
			get
			{
				return base.MaximizeBox;
			}

			set
			{
				if (this.systemMenu != null)
				{
					this.systemMenu.MaximizeEnabled = value;
				}

				if (this.buttonMaximize != null)
				{
					this.buttonMaximize.Visible = value;
					this.buttonMaximize.Enabled = value;
				}

				base.MaximizeBox = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the Minimize button is displayed in the caption bar of the form.
		/// </summary>
		public new bool MinimizeBox
		{
			get
			{
				return base.MinimizeBox;
			}

			set
			{
				if (this.systemMenu != null)
				{
					this.systemMenu.MinimizeEnabled = value;
				}

				if (this.buttonMinimize != null)
				{
					this.buttonMinimize.Visible = value;
					this.buttonMinimize.Enabled = value;
				}

				base.MinimizeBox = value;
			}
		}

		/// <summary>
		/// Processes a Windows Message
		/// </summary>
		/// <param name="m">Message which needs to be processed</param>
		[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
		protected override void WndProc(ref Message m)
		{
			if (this.WindowState == FormWindowState.Maximized)
			{
				if (((m.Msg == (int)User32.WindowMessage.SysCommand) &&
					(m.WParam.ToInt32() == (int)User32.SystemMenuCommand.Move)) ||
					((m.Msg == (int)User32.NonClientMouseMessage.LeftButtonDown) &&
					(m.WParam.ToInt32() == (int)User32.NonClientHitTestResult.Caption)))
				{
					m.Msg = (int)User32.WindowMessage.Null;
				}
			}

			base.WndProc(ref m);

			if (this.DrawCustomBorder)
			{
				switch (m.Msg)
				{
					case (int)User32.WindowMessage.GetSysMenu:
						this.systemMenu.Show(this, this.PointToClient(new Point(m.LParam.ToInt32())));
						break;
					/*case USER32.WM_NCACTIVATE:
                        this.formActive = m.WParam.ToInt32() != 0;
                        this.Invalidate();
                        break;*/
					case (int)User32.WindowMessage.NonClientHitTest:
						m.Result = this.OnNonClientHitTest(m.LParam);
						break;
					/*case (int)USER32.NCMouseMessage.WM_NCLBUTTONUP:
                        OnNonClientLButtonUp(m.LParam);
                        break;*/
					case (int)User32.NonClientMouseMessage.RightButtonUp:
						this.OnNonClientRButtonUp(m.LParam);
						break;
					/*case (int)USER32.NCMouseMessage.WM_NCMOUSEMOVE:
                        OnNonClientMouseMove(m.LParam);
                        break;*/
					default:
						break;
				}
			}
		}

		/// <summary>
		/// Processes a system event on the system menu.
		/// </summary>
		/// <param name="sender">Sender object</param>
		/// <param name="e">Window menu event args</param>
		protected void SystemMenuSystemEvent(object sender, WindowMenuEventArgs e)
		{
			this.SendNCWinMessage(User32.WindowMessage.SysCommand, (IntPtr)e.SystemCommand, IntPtr.Zero);
		}

		/// <summary>
		/// Scales the form.
		/// </summary>
		/// <param name="factor">factor which the form will be scaled.</param>
		/// <param name="specified">BoundsSpecified enum.</param>
		protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
		{
			if (this.DrawCustomBorder)
			{
				if (this.titleFont != null)
				{
					this.titleFont = new Font(this.titleFont.Name, this.titleFont.SizeInPoints * factor.Width);
				}
			}

			base.ScaleControl(factor, specified);

			if (this.DrawCustomBorder)
			{
				this.CreateBorderRects();
			}
		}

		/// <summary>
		/// Scales the form according to the scale factor.
		/// </summary>
		/// <param name="scaleFactor">Float which signifies the scale factor of the form.  This is an absolute from the original size unless useRelativeScaling is set to true.</param>
		/// <param name="useRelativeScaling">Indicates whether the scale factor is relative.  Used to support a resize operation.</param>
		protected virtual void ScaleForm(float scaleFactor, bool useRelativeScaling)
		{
			// Support relative resizing of the DB value.
			if (useRelativeScaling && scaleFactor != 1.0F)
			{
				float newDBScale = Database.DB.Scale * scaleFactor;
				if (newDBScale > 2.0F || newDBScale < 0.40F)
				{
					return;
				}

				Database.DB.Scale = newDBScale;
				this.Scale(new SizeF(scaleFactor, scaleFactor));
			}
			else if (scaleFactor == 1.0F)
			{
				// Check if we are resetting the size.
				if (Database.DB.Scale == 1.0F)
				{
					return;
				}

				// Reset the border graphics to the originals.
				this.topBorder = Resources.BorderTop;
				this.bottomBorder = Resources.BorderBottom;
				this.sideBorder = Resources.BorderSide;
				this.bottomRightCorner = Resources.BorderBottomRightCorner;
				this.bottomLeftCorner = Resources.BorderBottomLeftCorner;

				Database.DB.Scale = this.OriginalFormScale;

				// Use the width since it is usually more drastic of a change.
				// especially when coming from a small size.
				this.Scale(new SizeF(
					(float)this.OriginalFormSize.Width / (float)this.Width,
					(float)this.OriginalFormSize.Width / (float)this.Width));

				Settings.Default.Scale = 1.0F;
				Settings.Default.Save();
			}
			else
			{
				float scalingWidth = (float)this.OriginalFormSize.Width / (float)this.Width * scaleFactor;
				float scalingHeight = (float)this.OriginalFormSize.Height / (float)this.Height * scaleFactor;
				float scaling = scalingWidth;

				// Use the scaling factor closest to one.
				if ((scalingHeight < scalingWidth && scalingHeight > 1.0F) || (scalingHeight > scalingWidth && scalingHeight < 1.0F))
				{
					scaling = scalingHeight;
				}

				Database.DB.Scale = scaleFactor;
				this.Scale(new SizeF(scaling, scaling));
			}
		}

		/// <summary>
		/// Callback for resizing of the main form.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">ResizeEventArgs data</param>
		protected void ResizeFormCallback(object sender, ResizeEventArgs e)
		{
			if (e.ResizeDelta == 0.0F)
			{
				// Nothing to do so we return.
				return;
			}

			if (this.ScaleOnResize)
			{
				float scale;
				if (e.ResizeDelta == 1.0F)
				{
					scale = e.ResizeDelta;
				}
				else
				{
					scale = Database.DB.Scale + e.ResizeDelta;
				}

				if (scale < 0.39F || scale > 2.01F)
				{
					return;
				}

				this.ScaleForm(scale, false);
			}

			this.LastFormSize = this.Size;
		}

		/// <summary>
		/// Handler for the ResizeBegin event.  Used for handling the maximize and minimize functions.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		protected virtual void ResizeBeginCallback(object sender, EventArgs e)
		{
			if (this.WindowState != this.lastState)
			{
				this.lastState = this.WindowState;

				// If we are coming out of a minimized or maximized state, we need to restore
				// the form the size prior to minimizing or maximizing.
				if (this.lastState == FormWindowState.Minimized || this.lastState == FormWindowState.Maximized)
				{
					this.LastFormSize = this.RestoreBounds.Size;
				}

				this.ResizeEndCallback(this, new EventArgs());
			}
			else if (this.Size != this.LastFormSize && this.FormDesignRatio != 0.0F && this.ConstrainToDesignRatio)
			{
				Size tempSize = this.Size;

				// Keep the resizing within the form's design aspect ratio and bounds.
				// Find which was resized.
				if (tempSize.Height != this.LastFormSize.Height)
				{
					tempSize.Width = Convert.ToInt32((float)tempSize.Height / this.FormDesignRatio);
				}
				else
				{
					tempSize.Height = Convert.ToInt32((float)tempSize.Width * this.FormDesignRatio);
				}

				// Make sure we do not exceed the maximum values.
				if (tempSize.Height >= this.FormMaximumSize.Height || tempSize.Width >= this.FormMaximumSize.Width)
				{
					tempSize = this.FormMaximumSize;
				}

				if (tempSize.Height <= this.FormMinimumSize.Height || tempSize.Width <= this.FormMinimumSize.Width)
				{
					tempSize = this.FormMinimumSize;
				}

				this.Size = tempSize;
			}
		}

		/// <summary>
		/// Handler for the ResizeEnd event.  Used to scale the internal controls after the window has been resized.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		protected virtual void ResizeEndCallback(object sender, EventArgs e)
		{
			// Dragging the form will trigger this event.
			// If the size did not change or if sizing is disabled we just skip it.
			if (this.Size == this.LastFormSize || !this.ResizeCustomAllowed)
			{
				return;
			}

			if (this.ScaleOnResize)
			{
				// Scale the internal controls to the new size.
				float scalingFactor = (float)this.Size.Height / (float)this.LastFormSize.Height;

				// Set it back to the original size since the Scale() call in ScaleForm() will also resize the form.
				this.Size = this.LastFormSize;
				this.ScaleForm(scalingFactor, true);
			}

			this.LastFormSize = this.Size;
		}

		/// <summary>
		/// Handler for the Resize event.  Used to update the borders after the window has been resized.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		protected virtual void ResizeCallback(object sender, EventArgs e)
		{
			if (!this.ScaleOnResize)
			{
				this.PlaceButtons();
				this.CreateBorderRects();
				this.Invalidate();
			}
		}

		/// <summary>
		/// Handler for the form Paint callback
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">PaintEventArgs data</param>
		protected virtual void PaintCallback(object sender, PaintEventArgs e)
		{
			if (this.DrawCustomBorder)
			{
				if (this.topBorder != null)
				{
					// Draw the Top
					Rectangle topBorderRect = new Rectangle(ClientRectangle.Left, ClientRectangle.Top, this.ClientRectangle.Width, this.topBorder.Height);
					e.Graphics.DrawImage(this.topBorder, topBorderRect);

					// Show the title text if we are drawing the top border.
					if (!string.IsNullOrEmpty(this.Text) && this.titleFont != null)
					{
						Size textSize = TextRenderer.MeasureText(this.Text, this.titleFont);
						Rectangle textRect = new Rectangle(new Point((this.ClientSize.Width - textSize.Width) / 2, (this.topBorder.Height - textSize.Height) / 2), textSize);

						// Check if the background for the title text needs to be resized.
						float factor = (float)topBorderRect.Width / (float)Resources.BorderTop.Width;
						int textBackroundWidth = Convert.ToInt32(114.0F * factor);
						if (textRect.Width > textBackroundWidth)
						{
							// Copy the center 126 pixels from the top border which make up the background to the title text.
							Rectangle srcRect = new Rectangle((Resources.BorderTop.Width / 2) - 61, 0, 126, Resources.BorderTop.Height);
							Rectangle destRect = new Rectangle(
								textRect.Left - Convert.ToInt32(6.0F * factor),
								0,
								textRect.Width + Convert.ToInt32(12.0F * factor),
								topBorderRect.Height);
							e.Graphics.DrawImage(Resources.BorderTop, destRect, srcRect, GraphicsUnit.Pixel);
						}

						TextRenderer.DrawText(
							e.Graphics,
							this.Text,
							this.titleFont,
							textRect,
							this.TitleTextColor);
					}
				}

				int topBorderOffset = Convert.ToInt32(3.0F * ((float)this.topBorder.Height) / (float)Resources.BorderTop.Height);

				// Draw the Left side
				e.Graphics.DrawImage(
					this.sideBorder,
					new Rectangle(ClientRectangle.Left, ClientRectangle.Top + topBorderOffset, this.sideBorder.Width, this.ClientRectangle.Height - topBorderOffset));

				// Draw the Right side
				e.Graphics.DrawImage(
					this.sideBorder,
					new Rectangle(ClientRectangle.Right - this.sideBorder.Width, ClientRectangle.Top + topBorderOffset, this.sideBorder.Width, this.ClientRectangle.Height - topBorderOffset));

				// Draw the Bottom
				e.Graphics.DrawImage(
					this.bottomBorder,
					new Rectangle(ClientRectangle.Left, ClientRectangle.Bottom - this.bottomBorder.Height, this.ClientRectangle.Width, this.bottomBorder.Height));

				// Draw the Bottom Right Corner Graphics
				e.Graphics.DrawImage(
					this.bottomRightCorner,
					new Rectangle(ClientRectangle.Right - this.bottomRightCorner.Width, ClientRectangle.Bottom - this.bottomRightCorner.Height, this.bottomRightCorner.Width, this.bottomRightCorner.Height));

				// Draw the Bottom Left Corner Graphics
				e.Graphics.DrawImage(
					this.bottomLeftCorner,
					new Rectangle(ClientRectangle.Left, ClientRectangle.Bottom - this.bottomLeftCorner.Height, this.bottomLeftCorner.Width, this.bottomLeftCorner.Height));

				// Show the resize bounding boxes for debugging.
				if (this.ShowResizeBorders)
				{
					// Show Corners
					e.Graphics.FillRectangle(Brushes.Red, this.upperLeftCorner);
					e.Graphics.FillRectangle(Brushes.Red, this.upperRightCorner);
					e.Graphics.FillRectangle(Brushes.Red, this.lowerLeftCorner);
					e.Graphics.FillRectangle(Brushes.Red, this.lowerRightCorner);

					// Show Sides
					e.Graphics.FillRectangle(Brushes.Yellow, this.topBorderRect);
					e.Graphics.FillRectangle(Brushes.Green, this.topResizeRect);
					e.Graphics.FillRectangle(Brushes.Green, this.bottomBorderRect);
					e.Graphics.FillRectangle(Brushes.Green, this.leftBorderRect);
					e.Graphics.FillRectangle(Brushes.Green, this.rightBorderRect);
				}
			}
		}

		/*private void OnNonClientLButtonUp(IntPtr lParam)
        {
            USER32.SysCommand code = USER32.SysCommand.SC_DEFAULT;
            Point point = this.PointToClient(new Point(lParam.ToInt32()));

            if (this.iconButton.IsVisible(point))
            {
                this.TopMost = !this.TopMost;
                this.Invalidate();
            }
            else
            {
                if (this.closeButton.IsVisible(point))
                    code = USER32.SysCommand.SC_CLOSE;
                else if (this.maxButton.IsVisible(point))
                    code = this.WindowState == FormWindowState.Normal ? USER32.SysCommand.SC_MAXIMIZE : USER32.SysCommand.SC_RESTORE;
                else if (this.minButton.IsVisible(point))
                    code = USER32.SysCommand.SC_MINIMIZE;

                SendNCWinMessage(USER32.WM_SYSCOMMAND, (IntPtr)code, IntPtr.Zero);
            }
        }*/

		/// <summary>
		/// Processes a Client Right Button Up Event.
		/// </summary>
		/// <param name="parameter">Windows parameters</param>
		private void OnNonClientRButtonUp(IntPtr parameter)
		{
			if (this.topBorderRect.Contains(this.PointToClient(new Point(parameter.ToInt32()))))
			{
				this.SendNCWinMessage(User32.WindowMessage.GetSysMenu, IntPtr.Zero, parameter);
			}
		}

		/*private void OnNonClientMouseMove(IntPtr lParam)
        {
            Point point = this.PointToClient(new Point(lParam.ToInt32()));
            String tooltip;
            if (this.buttonClose.ClientRectangle.Contains(point))
                tooltip = "Close";
            else if (this.maxButton.IsVisible(point))
                tooltip = this.WindowState == FormWindowState.Normal ? "Maximize" : "Restore";
            else if (this.minButton.IsVisible(point))
                tooltip = "Minimize";
            else if (this.iconButton.IsVisible(point))
                tooltip = this.TopMost ? "Un-Pin" : "Pin";
            else
                tooltip = string.Empty;

            if (ButtonTip.GetToolTip(this) != tooltip)
                ButtonTip.SetToolTip(this, tooltip);
        }*/

		/// <summary>
		/// Perfoms a hit test on the non client area
		/// </summary>
		/// <param name="parameter">Windows parameters</param>
		/// <returns>IntPte containing the area which was hit.</returns>
		private IntPtr OnNonClientHitTest(IntPtr parameter)
		{
			User32.NonClientHitTestResult result = User32.NonClientHitTestResult.Client;

			Point point = this.PointToClient(new Point(parameter.ToInt32()));

			if (this.topBorderRect.Contains(point))
			{
				result = User32.NonClientHitTestResult.Caption;
			}

			if (this.WindowState == FormWindowState.Normal && this.ResizeCustomAllowed)
			{
				if (this.upperLeftCorner.Contains(point))
				{
					result = User32.NonClientHitTestResult.TopLeftCorner;
				}
				else if (this.topResizeRect.Contains(point))
				{
					result = User32.NonClientHitTestResult.TopBorder;
				}
				else if (this.upperRightCorner.Contains(point))
				{
					result = User32.NonClientHitTestResult.TopRightCorner;
				}
				else if (this.leftBorderRect.Contains(point))
				{
					result = User32.NonClientHitTestResult.LeftBorder;
				}
				else if (this.rightBorderRect.Contains(point))
				{
					result = User32.NonClientHitTestResult.RightBorder;
				}
				else if (this.lowerLeftCorner.Contains(point))
				{
					result = User32.NonClientHitTestResult.BottomLeftCorner;
				}
				else if (this.bottomBorderRect.Contains(point))
				{
					result = User32.NonClientHitTestResult.BottomBorder;
				}
				else if (this.lowerRightCorner.Contains(point))
				{
					result = User32.NonClientHitTestResult.BottomRightCorner;
				}
			}

			/*if (this.buttonClose.ClientRectangle.Contains(point))
                result = USER32.NCHitTestResult.HTBORDER;
            else if (this.maxButton.IsVisible(point))
                result = USER32.NCHitTestResult.HTBORDER;
            else if (this.minButton.IsVisible(point))
                result = USER32.NCHitTestResult.HTBORDER;
            else if (this.iconButton.IsVisible(point))
                result = USER32.NCHitTestResult.HTBORDER;*/

			return (IntPtr)result;
		}

		/// <summary>
		/// Sends a Non Client Window Message
		/// </summary>
		/// <param name="msg">Message which is sent</param>
		/// <param name="parameter1">Word Window paramters</param>
		/// <param name="parameter2">Long Window paramets</param>
		[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
		private void SendNCWinMessage(User32.WindowMessage msg, IntPtr parameter1, IntPtr parameter2)
		{
			Message message = Message.Create(this.Handle, (int)msg, parameter1, parameter2);
			this.WndProc(ref message);
		}

		/// <summary>
		/// Sets the location for the minimize, maximize and close buttons for the custom border.
		/// </summary>
		private void PlaceButtons()
		{
			if (this.buttonClose != null)
			{
				this.buttonClose.Location = new Point(
											this.ClientRectangle.Right - (this.buttonClose.Width + this.sideBorder.Width),
											this.ClientRectangle.Top);
			}

			if (this.buttonMaximize != null)
			{
				this.buttonMaximize.Location = new Point(this.buttonClose.Location.X - this.buttonMaximize.Width, this.ClientRectangle.Top);
			}

			if (this.buttonMinimize != null)
			{
				this.buttonMinimize.Location = new Point(this.buttonMaximize.Location.X - this.buttonMinimize.Width, this.ClientRectangle.Top);
			}
		}

		/// <summary>
		/// Creates the bounding Rectangles for the border graphics
		/// </summary>
		private void CreateBorderRects()
		{
			// Make it a couple of pixels wider than the side graphic.
			int cornerWidth = 6;
			if (this.sideBorder != null)
			{
				cornerWidth = this.sideBorder.Width + 2;
			}

			int cornerHeight = 6;
			if (this.bottomBorder != null)
			{
				cornerHeight = this.bottomBorder.Height + 2;
			}

			Size cornerSize = new Size(cornerWidth, cornerHeight);
			this.upperLeftCorner = new Rectangle(new Point(ClientRectangle.Left, ClientRectangle.Top), cornerSize);
			this.upperRightCorner = new Rectangle(new Point(ClientRectangle.Right - cornerSize.Width, ClientRectangle.Top), cornerSize);
			this.lowerLeftCorner = new Rectangle(new Point(ClientRectangle.Left, ClientRectangle.Bottom - cornerSize.Height), cornerSize);
			this.lowerRightCorner = new Rectangle(new Point(ClientRectangle.Right - cornerSize.Width, ClientRectangle.Bottom - cornerSize.Height), cornerSize);
			this.bottomBorderRect = new Rectangle(
				ClientRectangle.Left + cornerSize.Width,
				ClientRectangle.Bottom - cornerSize.Height,
				ClientRectangle.Width - (2 * cornerSize.Width),
				cornerSize.Height);
			this.rightBorderRect = new Rectangle(
				ClientRectangle.Right - cornerSize.Width,
				ClientRectangle.Top + cornerSize.Height,
				cornerSize.Width,
				ClientRectangle.Height - (2 * cornerSize.Height));
			this.leftBorderRect = new Rectangle(
				ClientRectangle.Left,
				ClientRectangle.Top + cornerSize.Height,
				cornerSize.Width,
				ClientRectangle.Height - (2 * cornerSize.Height));

			int titleBarHeight = 21;
			if (this.topBorder != null)
			{
				titleBarHeight = this.topBorder.Height + 2;
			}

			this.topBorderRect = new Rectangle(ClientRectangle.Left + cornerSize.Width, ClientRectangle.Top, ClientRectangle.Width - (2 * cornerSize.Width), titleBarHeight);

			// This should be inside of the topBorderRect
			this.topResizeRect = new Rectangle(this.topBorderRect.X, this.topBorderRect.Y, this.topBorderRect.Width, cornerSize.Height);
		}

		/// <summary>
		/// Handler for clicking the Close button.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void CloseButtonClick(object sender, EventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// Handler for clicking the Minimize button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MaximizeButtonClick(object sender, EventArgs e)
		{
			if (!this.MaximizeBox)
			{
				return;
			}

			if (this.WindowState == FormWindowState.Maximized)
			{
				this.WindowState = FormWindowState.Normal;
			}
			else
			{
				this.WindowState = FormWindowState.Maximized;
			}
		}

		/// <summary>
		/// Handler for clicking the Maximize button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MinimizeButtonClick(object sender, EventArgs e)
		{
			if (!this.MinimizeBox)
			{
				return;
			}

			if (this.WindowState != FormWindowState.Minimized)
			{
				this.WindowState = FormWindowState.Minimized;
			}
		}
	}
}