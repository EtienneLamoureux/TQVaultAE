//-----------------------------------------------------------------------
// <copyright file="AutomationRefusedException.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Automation;

/// <summary>
/// A request the app's automation code refuses, as the user could not do it (a hidden entry, a greyed button, a bag
/// out of range...). <see cref="AutomationHost.OnUi{T}(Func{Forms.MainForm, T})"/> turns it into a tool error with
/// its message; any other exception stays an error of the app.
/// </summary>
internal sealed class AutomationRefusedException : Exception
{
	/// <summary>Initializes a new instance of the <see cref="AutomationRefusedException"/> class.</summary>
	public AutomationRefusedException()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="AutomationRefusedException"/> class with its message.</summary>
	public AutomationRefusedException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="AutomationRefusedException"/> class with its message and cause.</summary>
	public AutomationRefusedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
