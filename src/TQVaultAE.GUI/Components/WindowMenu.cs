//-----------------------------------------------------------------------
// <copyright file="WindowMenu.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components
{
	using Properties;
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using TQVaultAE.GUI.Models;

	/// <summary>
	/// Class for creating a Window Menu on the form.
	/// </summary>
	internal sealed class WindowMenu : ContextMenu
	{
		/// <summary>
		/// Holds the owner Form
		/// </summary>
		private Form owner;

		/// <summary>
		/// Holds the Restore menu item.
		/// </summary>
		private MenuItem menuRestore;

		/// <summary>
		/// Holds the Move menu item.
		/// </summary>
		private MenuItem menuMove;

		/// <summary>
		/// Holds the Form Size menu item.
		/// </summary>
		private MenuItem menuSize;

		/// <summary>
		/// Holds the Minimize menu item.
		/// </summary>
		private MenuItem menuMin;

		/// <summary>
		/// Holds the Maximize menu item.
		/// </summary>
		private MenuItem menuMax;

		/// <summary>
		/// Holds the menu item separator.
		/// </summary>
		private MenuItem menuSep;

		/// <summary>
		/// Holds the Close menu item.
		/// </summary>
		private MenuItem menuClose;

		/// <summary>
		/// Initializes a new instance of the WindowMenu class.
		/// </summary>
		/// <param name="owner">ownder Form</param>
		public WindowMenu(Form owner)
			: base()
		{
			this.owner = owner;

			this.menuRestore = this.CreateMenuItem(Resources.GlobalRestore);
			this.menuMove = this.CreateMenuItem(Resources.GlobalMove);
			this.menuSize = this.CreateMenuItem(Resources.GlobalSize);
			this.menuMin = this.CreateMenuItem(Resources.GlobalMinimize);
			this.menuMax = this.CreateMenuItem(Resources.GlobalMaximize);
			this.menuSep = this.CreateMenuItem("-");
			this.menuClose = this.CreateMenuItem(Resources.GlobalClose, Shortcut.AltF4);

			this.MenuItems.AddRange(new MenuItem[] { this.menuRestore, this.menuMove, this.menuSize, this.menuMin, this.menuMax, this.menuSep, this.menuClose });

			this.menuClose.DefaultItem = true;
		}

		/// <summary>
		/// Event handler for system events.
		/// </summary>
		public event EventHandler<WindowMenuEventArgs> SystemEvent;

		/// <summary>
		/// Gets or sets a value indicating whether the Maximize menu item is enabled.
		/// </summary>
		public bool MaximizeEnabled { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the Minimize menu item is enabled.
		/// </summary>
		public bool MinimizeEnabled { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the Size menu item is enabled.
		/// </summary>
		public bool SizeEnabled { get; set; }

		/// <summary>
		/// Handler for popping up the menu.
		/// </summary>
		/// <param name="e">EventArgs data</param>
		protected override void OnPopup(EventArgs e)
		{
			switch (this.owner.WindowState)
			{
				case FormWindowState.Normal:
					this.menuRestore.Enabled = false;
					this.menuMax.Enabled = this.MaximizeEnabled;
					this.menuMin.Enabled = this.MinimizeEnabled;
					this.menuMove.Enabled = true;
					this.menuSize.Enabled = this.SizeEnabled;
					break;

				case FormWindowState.Minimized:
					this.menuRestore.Enabled = true;
					this.menuMax.Enabled = this.MaximizeEnabled;
					this.menuMin.Enabled = false;
					this.menuMove.Enabled = false;
					this.menuSize.Enabled = false;
					break;

				case FormWindowState.Maximized:
					this.menuRestore.Enabled = true;
					this.menuMax.Enabled = false;
					this.menuMin.Enabled = this.MinimizeEnabled;
					this.menuMove.Enabled = false;
					this.menuSize.Enabled = false;
					break;
			}

			base.OnPopup(e);
		}

		/// <summary>
		/// Handler for clicking an item in the menu.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void OnWindowMenuClick(object sender, EventArgs e)
		{
			switch (this.MenuItems.IndexOf((MenuItem)sender))
			{
				case 0:
					this.SendSysCommand(User32.SystemMenuCommand.Restore);
					break;

				case 1:
					this.SendSysCommand(User32.SystemMenuCommand.Move);
					break;

				case 2:
					this.SendSysCommand(User32.SystemMenuCommand.Size);
					break;

				case 3:
					this.SendSysCommand(User32.SystemMenuCommand.Minimize);
					break;

				case 4:
					this.SendSysCommand(User32.SystemMenuCommand.Maximize);
					break;

				case 6:
					this.SendSysCommand(User32.SystemMenuCommand.Close);
					break;
			}
		}

		/// <summary>
		/// Creates a new menu item.
		/// </summary>
		/// <param name="text">Text tag for the menu item.</param>
		/// <returns>new menu MenuItem with the passed text tag</returns>
		private MenuItem CreateMenuItem(string text)
		{
			return this.CreateMenuItem(text, Shortcut.None);
		}

		/// <summary>
		/// Creates a new menu item.
		/// </summary>
		/// <param name="text">Text tag for the menu item.</param>
		/// <param name="shortcut">Shortcut for this menu item.</param>
		/// <returns>new menu MenuItem with the passed text tag</returns>
		private MenuItem CreateMenuItem(string text, Shortcut shortcut)
		{
			MenuItem item = new MenuItem(text, this.OnWindowMenuClick, shortcut);
			item.OwnerDraw = true;
			item.MeasureItem += new MeasureItemEventHandler(this.MenuItemMeasureItem);
			item.DrawItem += new DrawItemEventHandler(this.MenuItemDrawItem);
			return item;
		}

		/// <summary>
		/// Measures the menu item.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">MeasureItemEventArgs data</param>
		private void MenuItemMeasureItem(object sender, MeasureItemEventArgs e)
		{
			MenuItem item = this.MenuItems[e.Index];
			string itemText = item.Text;
			itemText += "/tAlt+F4";
			Size itemSize = TextRenderer.MeasureText(itemText, SystemFonts.MenuFont);
			e.ItemHeight = e.Index == 5 ? 8 : itemSize.Height + 7;
			e.ItemWidth = itemSize.Width + itemSize.Height + 23;
		}

		/// <summary>
		/// Draws a menu item on screen.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">DrawItemEventArgs data</param>
		private void MenuItemDrawItem(object sender, DrawItemEventArgs e)
		{
			MenuItem item = this.MenuItems[e.Index];

			if (item.Enabled)
			{
				e.DrawBackground();
			}
			else
			{
				e.Graphics.FillRectangle(SystemBrushes.Menu, e.Bounds);
			}

			if (e.Index == 5)
			{
				e.Graphics.DrawLine(SystemPens.GrayText, e.Bounds.Left + 2, e.Bounds.Top + 3, e.Bounds.Right - 2, e.Bounds.Top + 3);
			}
			else
			{
				TextFormatFlags format = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;

				Color textColor = item.Enabled ? SystemColors.MenuText : SystemColors.GrayText;

				using (Font marlettFont = new Font("Marlett", 10.0F))
				{
					Rectangle glyphRect = e.Bounds;
					glyphRect.Width = glyphRect.Height;

					if (item == this.menuRestore)
					{
						TextRenderer.DrawText(e.Graphics, "2", marlettFont, glyphRect, textColor, format);
					}
					else if (item == this.menuMin)
					{
						TextRenderer.DrawText(e.Graphics, "0", marlettFont, glyphRect, textColor, format);
					}
					else if (item == this.menuMax)
					{
						TextRenderer.DrawText(e.Graphics, "1", marlettFont, glyphRect, textColor, format);
					}
					else if (item == this.menuClose)
					{
						TextRenderer.DrawText(e.Graphics, "r", marlettFont, glyphRect, textColor, format);
					}
				}

				format &= TextFormatFlags.Left | ~TextFormatFlags.HorizontalCenter;

				Rectangle textRect = new Rectangle(e.Bounds.Left + e.Bounds.Height + 3, e.Bounds.Top, e.Bounds.Width - e.Bounds.Height - 3, e.Bounds.Height);

				TextRenderer.DrawText(e.Graphics, item.Text, SystemFonts.MenuFont, textRect, textColor, format);

				if (item == this.menuClose)
				{
					string shortcut = "Alt+F4";
					Size shortcutSize = TextRenderer.MeasureText(shortcut, SystemFonts.MenuFont);
					textRect.X = textRect.Right - shortcutSize.Width - 13;
					TextRenderer.DrawText(e.Graphics, shortcut, SystemFonts.MenuFont, textRect, textColor, format);
				}
			}
		}

		/// <summary>
		/// Sends a System command
		/// </summary>
		/// <param name="command">command to be sent.</param>
		private void SendSysCommand(User32.SystemMenuCommand command)
		{
			if (this.SystemEvent != null)
			{
				WindowMenuEventArgs ev = new WindowMenuEventArgs((int)command);
				this.SystemEvent(this, ev);
			}
		}
	}
}