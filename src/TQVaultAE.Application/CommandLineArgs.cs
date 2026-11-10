//-----------------------------------------------------------------------
// <copyright file="CommandLineArgs.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Globalization;

namespace TQVaultAE.Application;

/// <summary>
/// Class for parsing command line arguments
/// </summary>
public class CommandLineArgs
{
	/// <summary>
	/// Initializes a new instance of the CommandLineArgs class.
	/// </summary>
	/// <param name="args">The command line as <see cref="Environment.GetCommandLineArgs"/> gives it: the program first.</param>
	public CommandLineArgs(string[] args)
	{
		if (args.Length > 2)
			this.Error = UsageMessage("Too many arguments");
		else if (args.Length == 2)
		{
			// The custom map comes from the character's mod link now: the old /mod: argument is refused.
			if (args[1].Trim().StartsWith('/'))
				this.Error = UsageMessage("Unknown argument");
			else
				this.Player = args[1];
		}
	}

	/// <summary>
	/// Gets the name of the player specified on the command line.
	/// </summary>
	public string? Player { get; }

	/// <summary>
	/// Gets the usage message when the arguments could not be parsed, else null.
	/// </summary>
	public string? Error { get; }

	/// <summary>
	/// Gets a value indicating whether to bypass the starts screen.
	/// </summary>
	public bool IsAutomatic => this.Player is not null;

	/// <summary>
	/// Throws the usage message if the command line arguments are incorrect.
	/// </summary>
	/// <exception cref="ArgumentException">The arguments could not be parsed.</exception>
	public void ThrowIfInvalid()
	{
		if (this.Error is not null)
			throw new ArgumentException(this.Error);
	}

	private static string UsageMessage(string error)
		=> string.Format(CultureInfo.InvariantCulture, "Error parsing command-line arguments: {0}\n\nUsage: TQVault [playername]", error);
}
