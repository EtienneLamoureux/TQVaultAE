//-----------------------------------------------------------------------
// <copyright file="Headless.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Microsoft.Extensions.Logging;
using TQVaultAE.Automation;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// TQVault's part of the Debug-only <c>--headless</c> mode (<see cref="HeadlessDesktop"/>): the name of its desktop and
/// of the variable that hands it to the started instance. Message boxes never reach the hidden desktop, they are
/// answered without being shown (see <see cref="AutomationHost.TryTakeDialogAnswer"/>); the native dialogs that do
/// (common dialogs, task dialogs) are cancelled, recorded by title and logged.
/// </summary>
internal static class Headless
{
	/// <summary>Environment variable that gives the started instance the name of its hidden desktop.</summary>
	private const string DesktopVariable = "TQVAULT_HEADLESS_DESKTOP";

	/// <inheritdoc cref="HeadlessDesktop.TryAttach"/>
	public static bool TryAttach()
		=> HeadlessDesktop.TryAttach(
			DesktopVariable,
			onCancelled: title => AutomationHost.Log?.LogWarning("Headless: native dialog \"{Title}\" cancelled", title));

	/// <inheritdoc cref="HeadlessDesktop.StartOnHiddenDesktop"/>
	public static string? StartOnHiddenDesktop() => HeadlessDesktop.StartOnHiddenDesktop(DesktopVariable, "TQVaultAE");
}
