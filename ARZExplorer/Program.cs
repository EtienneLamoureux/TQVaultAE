//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="VillageIdiot">
//     Copyright (c) Village Idiot. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ArzExplorer
{
	using System;
	using System.Windows.Forms;

	/// <summary>
	/// Holds the main program.
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}
	}
}