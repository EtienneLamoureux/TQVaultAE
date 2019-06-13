//-----------------------------------------------------------------------
// <copyright file="WindowMenuEventArgs.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Models
{
	using System;

	/// <summary>
	/// Class for the WindowMenuEventArgs
	/// </summary>
	public class WindowMenuEventArgs : EventArgs
	{
		/// <summary>
		/// Holds the system command for this menu item
		/// </summary>
		private int systemCommand;

		/// <summary>
		/// Initializes a new instance of the WindowMenuEventArgs class.
		/// </summary>
		/// <param name="command">Command for the menu item</param>
		public WindowMenuEventArgs(int command)
			: base()
		{
			this.systemCommand = command;
		}

		/// <summary>
		/// Gets the system command.
		/// </summary>
		public int SystemCommand
		{
			get
			{
				return (int)this.systemCommand;
			}
		}
	}
}