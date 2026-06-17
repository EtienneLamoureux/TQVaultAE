//-----------------------------------------------------------------------
// <copyright file="WindowMenu.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components;

using System;
using System.Drawing;
using System.Windows.Forms;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;

/// <summary>
/// Class for creating a Window Menu on the form.
/// Migrated from deprecated Menu/MenuItem to ToolStrip for .NET 10+ compatibility.
/// </summary>
internal sealed class WindowMenu : ContextMenuStrip
{
	/// <summary>
	/// Holds the owner Form
	/// </summary>
	private readonly Form owner;

	/// <summary>
	/// Holds the Restore menu item.
	/// </summary>
	private ToolStripMenuItem menuRestore;

	/// <summary>
	/// Holds the Move menu item.
	/// </summary>
	private ToolStripMenuItem menuMove;

	/// <summary>
	/// Holds the Form Size menu item.
	/// </summary>
	private ToolStripMenuItem menuSize;

	/// <summary>
	/// Holds the Minimize menu item.
	/// </summary>
	private ToolStripMenuItem menuMin;

	/// <summary>
	/// Holds the Maximize menu item.
	/// </summary>
	private ToolStripMenuItem menuMax;

	/// <summary>
	/// Holds the menu item separator.
	/// </summary>
	private ToolStripSeparator menuSep;

	/// <summary>
	/// Holds the Close menu item.
	/// </summary>
	private ToolStripMenuItem menuClose;

	/// <summary>
	/// Holds the Normalize menu item.
	/// </summary>
	private ToolStripMenuItem menuNorm;

	/// <summary>
	/// Initializes a new instance of the WindowMenu class.
	/// </summary>
	/// <param name="owner">owner Form</param>
	public WindowMenu(Form owner)
	{
		this.owner = owner;

		// Create menu items
		this.menuRestore = this.CreateMenuItem(Resources.GlobalRestore);
		this.menuMove = this.CreateMenuItem(Resources.GlobalMove);
		this.menuSize = this.CreateMenuItem(Resources.GlobalSize);
		this.menuMin = this.CreateMenuItem(Resources.GlobalMinimize);
		this.menuNorm = this.CreateMenuItem(Resources.GlobalNormalize);
		this.menuMax = this.CreateMenuItem(Resources.GlobalMaximize);
		this.menuSep = new ToolStripSeparator();
		this.menuClose = this.CreateMenuItem(Resources.GlobalClose, Keys.Alt | Keys.F4);
		this.menuClose.ShortcutKeyDisplayString = "Alt+F4";

		// Add items to menu
		this.Items.AddRange(new ToolStripItem[] {
			this.menuRestore,
			this.menuMove,
			this.menuSize,
			this.menuMin,
			this.menuNorm,
			this.menuMax,
			this.menuSep,
			this.menuClose
		});

		this.menuClose.BackColor = SystemColors.MenuHighlight;
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
	/// Gets or sets a value indicating whether the Normalize menu item is enabled.
	/// </summary>
	public bool NormalizeEnabled { get; set; }

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
	protected override void OnOpening(System.ComponentModel.CancelEventArgs e)
	{
		switch (this.owner.WindowState)
		{
			case FormWindowState.Normal:
				this.menuRestore.Enabled = false;
				this.menuMax.Enabled = this.MaximizeEnabled;
				this.menuNorm.Enabled = this.NormalizeEnabled;
				this.menuMin.Enabled = this.MinimizeEnabled;
				this.menuMove.Enabled = true;
				this.menuSize.Enabled = this.SizeEnabled;
				break;

			case FormWindowState.Minimized:
				this.menuRestore.Enabled = true;
				this.menuMax.Enabled = this.MaximizeEnabled;
				this.menuNorm.Enabled = this.NormalizeEnabled;
				this.menuMin.Enabled = false;
				this.menuMove.Enabled = false;
				this.menuSize.Enabled = false;
				break;

			case FormWindowState.Maximized:
				this.menuRestore.Enabled = true;
				this.menuMax.Enabled = false;
				this.menuMin.Enabled = this.MinimizeEnabled;
				this.menuNorm.Enabled = this.NormalizeEnabled;
				this.menuMove.Enabled = false;
				this.menuSize.Enabled = false;
				break;
		}

		base.OnOpening(e);
	}

	/// <summary>
	/// Handler for clicking an item in the menu.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void OnWindowMenuClick(object sender, EventArgs e)
	{
		int index = this.Items.IndexOf((ToolStripItem)sender);
		switch (index)
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
	/// <returns>new menu ToolStripMenuItem with the passed text tag</returns>
	private ToolStripMenuItem CreateMenuItem(string text)
		=> this.CreateMenuItem(text, Keys.None);

	/// <summary>
	/// Creates a new menu item.
	/// </summary>
	/// <param name="text">Text tag for the menu item.</param>
	/// <param name="shortcut">Shortcut for this menu item.</param>
	/// <returns>new menu ToolStripMenuItem with the passed text tag</returns>
	private ToolStripMenuItem CreateMenuItem(string text, Keys shortcut)
	{
		ToolStripMenuItem item = new ToolStripMenuItem(text)
		{
			ShortcutKeys = shortcut
		};
		item.Click += this.OnWindowMenuClick;
		return item;
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
