//-----------------------------------------------------------------------
// <copyright file="VaultForm.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using log4net;
	using Microsoft.Extensions.DependencyInjection;
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Globalization;
	using System.Security.Permissions;
	using System.Windows.Forms;
	using System.Windows.Input;
	using TQVaultAE.Domain.Contracts.Providers;
	using TQVaultAE.Domain.Contracts.Services;
	using TQVaultAE.GUI.Components;
	using TQVaultAE.GUI.Models;
	using TQVaultAE.Logs;
	using TQVaultAE.Presentation;

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
		private readonly ILog Log;

		/// <summary>
		/// WindowMenu used to display the system menu.
		/// </summary>
		private WindowMenu systemMenu;

		/// <summary>
		/// Indicates whether resizing is allowed when custom borders are enabled.
		/// </summary>
		private bool resizeCustomAllowed;

		public IFontService FontService;
		public IUIService UIService;
		public IDatabase Database;
		public IItemProvider ItemProvider;
		public IPlayerCollectionProvider PlayerCollectionProvider;
		public IGamePathService GamePathResolver;
		public IServiceProvider ServiceProvider;


#if DEBUG
		// For Design Mode
		public VaultForm() => InitForm();
#endif
		/// <summary>
		/// Initializes a new instance of the VaultForm class.
		/// </summary>
		public VaultForm(IServiceProvider serviceProvider)
		{
			if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
			{
				this.ServiceProvider = serviceProvider;
				this.FontService = this.ServiceProvider.GetService<IFontService>();
				this.UIService = this.ServiceProvider.GetService<IUIService>();
				this.Database = this.ServiceProvider.GetService<IDatabase>();
				this.ItemProvider = this.ServiceProvider.GetService<IItemProvider>();
				this.PlayerCollectionProvider = this.ServiceProvider.GetService<IPlayerCollectionProvider>();
				this.GamePathResolver = this.ServiceProvider.GetService<IGamePathService>();
				this.titleFont = FontService.GetFontAlbertusMTLight(9.5F);
				this.Log = this.ServiceProvider.GetService<ILogger<VaultForm>>().Logger;

				InitForm();
			}
		}

		private void InitForm()
		{
			this.InitializeComponent();

			this.topBorder = Resources.BorderTop;
			this.bottomBorder = Resources.BorderBottom;
			this.sideBorder = Resources.BorderSide;
			this.bottomRightCorner = Resources.BorderBottomRightCorner;
			this.bottomLeftCorner = Resources.BorderBottomLeftCorner;
			this.ShowResizeBorders = false;

			this.TitleTextColor = SystemColors.ControlText;

			this.PlaceButtons();

			#region Apply custom font & scaling

			ScaleControl(this.UIService, this.buttonMaximize);
			ScaleControl(this.UIService, this.ButtonScaleTo1);
			ScaleControl(this.UIService, this.buttonMinimize);
			ScaleControl(this.UIService, this.buttonClose);

			#endregion

			this.CreateBorderRects();

			// to avoid flickering use double buffer and to force control to use OnPaint
			this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);


			this.ResizeBegin += new System.EventHandler(this.ResizeBeginCallback);
			this.ResizeEnd += new System.EventHandler(this.ResizeEndCallback);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintCallback);
			this.Resize += new System.EventHandler(this.ResizeCallback);

			this.systemMenu = new WindowMenu(this);
			this.systemMenu.SystemEvent += new EventHandler<WindowMenuEventArgs>(this.SystemMenuSystemEvent);
			this.systemMenu.MaximizeEnabled = this.MaximizeBox;
			this.systemMenu.NormalizeEnabled = this.NormalizeBox;
			this.systemMenu.MinimizeEnabled = this.MinimizeBox;

		}

		public static void ScaleControl(IUIService uiService, System.Windows.Forms.Control ctrl, bool isAbsolutePositioning = true)
		{
			if (isAbsolutePositioning)
				ctrl.Location = ScalePoint(uiService, ctrl.Location);

			ctrl.Size = ScaleSize(uiService, ctrl.Size);
		}

		public static Size ScaleSize(IUIService uiService, Size size)
			=> new Size((int)System.Math.Round(size.Width * uiService?.Scale ?? 1.0F), (int)System.Math.Round(size.Height * uiService?.Scale ?? 1.0F));

		public static Point ScalePoint(IUIService uiService, Point point)
			=> new Point((int)System.Math.Round(point.X * uiService?.Scale ?? 1.0F), (int)System.Math.Round(point.Y * uiService?.Scale ?? 1.0F));


		/// <summary>
		/// Gets the MessageBoxOptions for right to left reading.
		/// </summary>
		public static MessageBoxOptions RightToLeftOptions
		{
			get
			{
				// Set options for Right to Left reading.
				if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
					return MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;

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
			get => this.drawCustomBorder;

			set
			{
				if (value)
				{
					this.FormBorderStyle = FormBorderStyle.None;

					this.buttonClose.Visible = true;
					this.buttonClose.Enabled = true;

					this.buttonMaximize.Visible = this.MaximizeBox;
					this.buttonMaximize.Enabled = this.MaximizeBox;

					this.ButtonScaleTo1.Visible = this.NormalizeBox;
					this.ButtonScaleTo1.Enabled = this.NormalizeBox;

					this.buttonMinimize.Visible = this.MinimizeBox;
					this.buttonMinimize.Enabled = this.MinimizeBox;
				}
				else
				{
					this.FormBorderStyle = FormBorderStyle.FixedSingle;
					this.buttonMaximize.Visible = false;
					this.buttonMaximize.Enabled = false;
					this.ButtonScaleTo1.Visible = false;
					this.ButtonScaleTo1.Enabled = false;
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
			get => this.resizeCustomAllowed;

			set
			{
				if (this.systemMenu != null)
					this.systemMenu.SizeEnabled = value;

				this.resizeCustomAllowed = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the Maximize button is displayed in the caption bar of the form.
		/// </summary>
		public new bool MaximizeBox
		{
			get => base.MaximizeBox;

			set
			{
				if (this.systemMenu != null)
					this.systemMenu.MaximizeEnabled = value;

				if (this.buttonMaximize != null)
				{
					this.buttonMaximize.Visible = value;
					this.buttonMaximize.Enabled = value;
				}

				base.MaximizeBox = value;
			}
		}

		bool _NormalizeBox = true;
		/// <summary>
		/// Gets a value indicating whether the Normalize button is displayed in the caption bar of the form.
		/// </summary>
		public virtual bool NormalizeBox
		{
			get => _NormalizeBox && this.UIService.Scale != 1.0F;
			set
			{
				_NormalizeBox = value;
				RefreshNormalizeBox();
			}
		}

		private void RefreshNormalizeBox()
		{
			var val = _NormalizeBox && this.UIService.Scale != 1.0F;
			if (this.systemMenu != null)
				this.systemMenu.NormalizeEnabled = val;

			if (this.ButtonScaleTo1 != null)
			{
				this.ButtonScaleTo1.Visible = val;
				this.ButtonScaleTo1.Enabled = val;
			}
		}


		/// <summary>
		/// Gets or sets a value indicating whether the Minimize button is displayed in the caption bar of the form.
		/// </summary>
		public new bool MinimizeBox
		{
			get => base.MinimizeBox;

			set
			{
				if (this.systemMenu != null)
					this.systemMenu.MinimizeEnabled = value;

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
				if (
					(
						(m.Msg == (int)User32.WindowMessage.SysCommand)
						&& (m.WParam.ToInt32() == (int)User32.SystemMenuCommand.Move)
					)
					||
					(
						(m.Msg == (int)User32.NonClientMouseMessage.LeftButtonDown)
						&& (m.WParam.ToInt32() == (int)User32.NonClientHitTestResult.Caption)
					)
				)
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
					case (int)User32.WindowMessage.NonClientHitTest:
						m.Result = this.OnNonClientHitTest(m.LParam);
						break;
					case (int)User32.NonClientMouseMessage.RightButtonUp:
						this.OnNonClientRButtonUp(m.LParam);
						break;
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
			=> this.SendNCWinMessage(User32.WindowMessage.SysCommand, (IntPtr)e.SystemCommand, IntPtr.Zero);

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
					this.titleFont = new Font(this.titleFont.Name, this.titleFont.SizeInPoints * factor.Width);
			}

			base.ScaleControl(factor, specified);

			if (this.DrawCustomBorder)
				this.CreateBorderRects();
		}

		internal const int NORMAL_FORMWIDTH = 1350;
		internal const int NORMAL_FORMHEIGHT = 910;

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
				float newDBScale = UIService.Scale * scaleFactor;
				if (newDBScale > 2.0F || newDBScale < 0.40F)
					return;

				UIService.Scale = newDBScale;
				this.Scale(new SizeF(scaleFactor, scaleFactor));

				Config.Settings.Default.Scale = UIService.Scale;
				Config.Settings.Default.Save();
			}
			else if (scaleFactor == 1.0F)
			{
				// Reset the border graphics to the originals.
				this.topBorder = Resources.BorderTop;
				this.bottomBorder = Resources.BorderBottom;
				this.sideBorder = Resources.BorderSide;
				this.bottomRightCorner = Resources.BorderBottomRightCorner;
				this.bottomLeftCorner = Resources.BorderBottomLeftCorner;

				UIService.Scale = 1.0F;

				// Use the width since it is usually more drastic of a change.
				// especially when coming from a small size.
				var size = new SizeF(
					(float)NORMAL_FORMWIDTH / (float)this.Width,
					(float)NORMAL_FORMWIDTH / (float)this.Width);
				this.Scale(size);

				Config.Settings.Default.Scale = 1.0F;
				Config.Settings.Default.Save();
			}
			else
			{
				float scalingWidth = (float)NORMAL_FORMWIDTH / (float)this.Width * scaleFactor;
				float scalingHeight = (float)NORMAL_FORMHEIGHT / (float)this.Height * scaleFactor;
				float scaling = scalingWidth;

				// Use the scaling factor closest to one.
				if ((scalingHeight < scalingWidth && scalingHeight > 1.0F) || (scalingHeight > scalingWidth && scalingHeight < 1.0F))
					scaling = scalingHeight;

				UIService.Scale = scaleFactor;
				this.Scale(new SizeF(scaling, scaling));

				Config.Settings.Default.Scale = UIService.Scale;
				Config.Settings.Default.Save();
			}

			RefreshNormalizeBox();

			this.Log.DebugFormat("Config.Settings.Default.Scale changed to {0} !", Config.Settings.Default.Scale);
		}

		/// <summary>
		/// Callback for resizing of the main form.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">ResizeEventArgs data</param>
		protected void ResizeFormCallback(object sender, ResizeEventArgs e)
		{
			if (e.ResizeDelta == 0.0F)
				// Nothing to do so we return.
				return;

			if (this.ScaleOnResize)
			{
				float scale;
				if (e.ResizeDelta == 1.0F)
					scale = e.ResizeDelta;
				else
					scale = UIService.Scale + e.ResizeDelta;

				if (scale < 0.39F || scale > 2.01F)
					return;

				this.ScaleForm(scale, false);
			}

			this.LastFormSize = this.Size;
		}

		protected Size InitialScaling(Rectangle workingArea)
		{
			// If screen is smaller than NORMAL_FORM sizes, adjust scaling (mostly at first run)
			if (workingArea.Width < NORMAL_FORMWIDTH || workingArea.Height < NORMAL_FORMHEIGHT)
			{
				var initialScale = Math.Min(
					Convert.ToSingle(workingArea.Width) / Convert.ToSingle(NORMAL_FORMWIDTH)
					, Convert.ToSingle(workingArea.Height) / Convert.ToSingle(NORMAL_FORMHEIGHT)
				);

				if (Config.Settings.Default.Scale > initialScale)
				{
					Config.Settings.Default.Scale = initialScale;
					Config.Settings.Default.Save();
				}
			}

			// Rescale from last saved value
			var thisClientSize = new Size(
				(int)Math.Round(NORMAL_FORMWIDTH * Config.Settings.Default.Scale)
				, (int)Math.Round(NORMAL_FORMHEIGHT * Config.Settings.Default.Scale)
			);

			return thisClientSize;
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
				// If we are coming out of a minimized or maximized state, we need to restore
				// the form the size prior to minimizing or maximizing.
				bool fromMinimizedMaximized = false;
				if (this.lastState == FormWindowState.Minimized || this.lastState == FormWindowState.Maximized)
				{
					this.LastFormSize = this.RestoreBounds.Size;
					fromMinimizedMaximized = true;
				}

				this.lastState = this.WindowState;

				this.DoResizeEndCallback(this, e, fromMinimizedMaximized);
			}
			else if (this.Size != this.LastFormSize && this.FormDesignRatio != 0.0F && this.ConstrainToDesignRatio)
			{
				Size tempSize = this.Size;

				// Keep the resizing within the form's design aspect ratio and bounds.
				// Find which was resized.
				if (tempSize.Height != this.LastFormSize.Height)
					tempSize.Width = Convert.ToInt32((float)tempSize.Height / this.FormDesignRatio);
				else
					tempSize.Height = Convert.ToInt32((float)tempSize.Width * this.FormDesignRatio);

				// Make sure we do not exceed the maximum values.
				if (tempSize.Height >= this.FormMaximumSize.Height || tempSize.Width >= this.FormMaximumSize.Width)
					tempSize = this.FormMaximumSize;

				if (tempSize.Height <= this.FormMinimumSize.Height || tempSize.Width <= this.FormMinimumSize.Width)
					tempSize = this.FormMinimumSize;

				this.Size = tempSize;
			}
		}

		/// <summary>
		/// Handler for the ResizeEnd event.  Used to scale the internal controls after the window has been resized.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		protected virtual void ResizeEndCallback(object sender, EventArgs e) => DoResizeEndCallback(sender, e);
		protected virtual void DoResizeEndCallback(object sender, EventArgs e, bool fromMinimizedMaximized = false)
		{
			// Dragging the form will trigger this event.
			// If the size did not change or if sizing is disabled we just skip it.
			if (this.Size == this.LastFormSize || !this.ResizeCustomAllowed)
				return;

			if (this.ScaleOnResize)
			{
				Size resizeTo = this.LastFormSize;
				// Scale the internal controls to the new size.
				float scalingFactor = (float)this.Size.Height / (float)resizeTo.Height;

				// Reset scaling to 1.0 if resize from minimize/maximize to normal with shift keydown
				if (fromMinimizedMaximized)
				{
					if (Keyboard.IsKeyDown(Key.LeftShift))
					{
						resizeTo = new Size(NORMAL_FORMWIDTH, NORMAL_FORMHEIGHT);
						scalingFactor = 1.0F;
					}
				}

				// Set it back to the original size since the Scale() call in ScaleForm() will also resize the form.
				this.Size = resizeTo;
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

		/// <summary>
		/// Processes a Client Right Button Up Event.
		/// </summary>
		/// <param name="parameter">Windows parameters</param>
		private void OnNonClientRButtonUp(IntPtr parameter)
		{
			if (this.topBorderRect.Contains(this.PointToClient(new Point(parameter.ToInt32()))))
				this.SendNCWinMessage(User32.WindowMessage.GetSysMenu, IntPtr.Zero, parameter);
		}


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
				result = User32.NonClientHitTestResult.Caption;

			if (this.WindowState == FormWindowState.Normal && this.ResizeCustomAllowed)
			{
				if (this.upperLeftCorner.Contains(point))
					result = User32.NonClientHitTestResult.TopLeftCorner;
				else if (this.topResizeRect.Contains(point))
					result = User32.NonClientHitTestResult.TopBorder;
				else if (this.upperRightCorner.Contains(point))
					result = User32.NonClientHitTestResult.TopRightCorner;
				else if (this.leftBorderRect.Contains(point))
					result = User32.NonClientHitTestResult.LeftBorder;
				else if (this.rightBorderRect.Contains(point))
					result = User32.NonClientHitTestResult.RightBorder;
				else if (this.lowerLeftCorner.Contains(point))
					result = User32.NonClientHitTestResult.BottomLeftCorner;
				else if (this.bottomBorderRect.Contains(point))
					result = User32.NonClientHitTestResult.BottomBorder;
				else if (this.lowerRightCorner.Contains(point))
					result = User32.NonClientHitTestResult.BottomRightCorner;
			}


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
					this.ClientRectangle.Right - (this.buttonClose.Width + this.sideBorder.Width)
					, this.ClientRectangle.Top);
			}

			if (this.buttonMaximize != null)
				this.buttonMaximize.Location = new Point(this.buttonClose.Location.X - 5 - this.buttonMaximize.Width, this.ClientRectangle.Top);

			if (this.ButtonScaleTo1 != null)
				this.ButtonScaleTo1.Location = new Point(this.buttonMaximize.Location.X - 5 - this.ButtonScaleTo1.Width, this.ClientRectangle.Top);

			if (this.buttonMinimize != null)
				this.buttonMinimize.Location = new Point(this.ButtonScaleTo1.Location.X - 5 - this.buttonMinimize.Width, this.ClientRectangle.Top);
		}

		/// <summary>
		/// Creates the bounding Rectangles for the border graphics
		/// </summary>
		private void CreateBorderRects()
		{
			// Make it a couple of pixels wider than the side graphic.
			int cornerWidth = 6;
			if (this.sideBorder != null)
				cornerWidth = this.sideBorder.Width + 2;

			int cornerHeight = 6;
			if (this.bottomBorder != null)
				cornerHeight = this.bottomBorder.Height + 2;

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
				titleBarHeight = this.topBorder.Height + 2;

			this.topBorderRect = new Rectangle(ClientRectangle.Left + cornerSize.Width, ClientRectangle.Top, ClientRectangle.Width - (2 * cornerSize.Width), titleBarHeight);

			// This should be inside of the topBorderRect
			this.topResizeRect = new Rectangle(this.topBorderRect.X, this.topBorderRect.Y, this.topBorderRect.Width, cornerSize.Height);
		}

		/// <summary>
		/// Handler for clicking the Close button.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void CloseButtonClick(object sender, EventArgs e) => this.Close();

		/// <summary>
		/// Handler for clicking the Minimize button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MaximizeButtonClick(object sender, EventArgs e)
		{
			if (!this.MaximizeBox)
				return;

			if (this.WindowState == FormWindowState.Maximized)
				this.WindowState = FormWindowState.Normal;
			else
				this.WindowState = FormWindowState.Maximized;
		}

		/// <summary>
		/// Handler for clicking the Maximize button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MinimizeButtonClick(object sender, EventArgs e)
		{
			if (!this.MinimizeBox)
				return;

			if (this.WindowState != FormWindowState.Minimized)
				this.WindowState = FormWindowState.Minimized;
		}

		private void ButtonScaleTo1_Click(object sender, EventArgs e)
		{
			if (!this.NormalizeBox)
				return;

			this.WindowState = FormWindowState.Normal;
			ScaleForm(1.0f, false);
		}
	}
}