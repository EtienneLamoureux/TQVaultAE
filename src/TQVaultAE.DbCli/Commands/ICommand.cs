//-----------------------------------------------------------------------
// <copyright file="ICommand.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TQVaultAE.DbCli.Commands;

/// <summary>A sub-command.</summary>
internal interface ICommand
{
	/// <summary>Declaration (name, options, help).</summary>
	CommandSpec Spec { get; }

	/// <summary>Runs the command; returns the process exit code.</summary>
	int Execute(ParsedArgs args, CliContext context);
}

/// <summary>Process exit codes.</summary>
internal static class ExitCodes
{
	public const int Ok = 0;
	public const int Failure = 1;
	public const int Usage = 2;
	public const int NotFound = 3;
	public const int Cancelled = 130;
}
