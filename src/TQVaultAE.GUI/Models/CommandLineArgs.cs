//-----------------------------------------------------------------------
// <copyright file="CommandLineArgs.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Models
{
	using System;
	using System.Globalization;

	/// <summary>
	/// Class for parsing command line arguments
	/// </summary>
	public class CommandLineArgs
	{
		/// <summary>
		/// Initializes a new instance of the CommandLineArgs class.
		/// </summary>
		public CommandLineArgs()
		{
			string[] args = Environment.GetCommandLineArgs();

			// Modified by VillageIdiot
			// to accept mapname from the command line
			if (args.Length == 2)
			{
				if (args[1].Trim().ToUpperInvariant().StartsWith("/MOD:", StringComparison.OrdinalIgnoreCase))
				{
					this.MapName = args[1].Trim().Substring(5);
				}
				else
				{
					this.Player = args[1];
				}
			}
			else if (args.Length == 3)
			{
				if (args[2].Trim().ToUpperInvariant().StartsWith("/MOD:", StringComparison.OrdinalIgnoreCase))
				{
					this.Player = args[1];
					this.MapName = args[2].Trim().Substring(5);
				}
				else if (args[1].Trim().ToUpperInvariant().StartsWith("/MOD:", StringComparison.OrdinalIgnoreCase))
				{
					// Check to see if the arguments are swapped.
					this.Player = args[2];
					this.MapName = args[1].Trim().Substring(5);
				}
				else
				{
					// Neither one started with /mod: so we fail.
					CommandLineArgs.Usage("Unknown arguments");
				}
			}
			else if (args.Length > 3)
			{
				CommandLineArgs.Usage("Too many arguments");
			}
		}

		/// <summary>
		/// Gets the name of the player specified on the command line.
		/// </summary>
		public string Player { get; private set; }

		/// <summary>
		/// Gets the name of the custom map specified on the command line.
		/// </summary>
		public string MapName { get; private set; }

		/// <summary>
		/// Gets a value indicating whether to bypass the starts screen.
		/// </summary>
		public bool IsAutomatic
		{
			get
			{
				return this.Player != null || this.MapName != null;
			}
		}

		/// <summary>
		/// Gets a value indicating whether a custom map has been specified.
		/// </summary>
		public bool HasMapName
		{
			get
			{
				return this.MapName != null;
			}
		}

		/// <summary>
		/// Displays a usage message if the command line arguments are incorrect or there is an error.
		/// </summary>
		/// <param name="error">error text containing passed arguments.</param>
		private static void Usage(string error)
		{
			string message = string.Format(CultureInfo.InvariantCulture, "Error parsing command-line arguments: {0}\n\nUsage: TQVault [playername] [/mod:<mod name>]", error);
			throw new ArgumentException(message);
		}
	}
}