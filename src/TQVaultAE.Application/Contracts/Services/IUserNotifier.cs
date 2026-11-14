using System.Drawing;
using Microsoft.Extensions.Logging;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Shows notifications and messages to the user, whatever the user interface.
/// </summary>
public interface IUserNotifier
{
	/// <summary>
	/// Display notification to user
	/// </summary>
	/// <param name="message"></param>
	/// <param name="color"></param>
	void NotifyUser(string message, TQColor color = TQColor.Turquoise);

	/// <summary>
	/// Display notification to user
	/// </summary>
	/// <param name="message"></param>
	/// <param name="color"></param>
	void NotifyUser(string message, Color color);

	/// <summary>
	/// Notification event
	/// </summary>
	event NotifyUserEventHandler? NotifyUserEvent;

	/// <summary>
	/// Notification event
	/// </summary>
	event ShowMessageUserEventHandler? ShowMessageUserEvent;

	/// <summary>
	/// Display error to user
	/// </summary>
	/// <param name="exception"></param>
	/// <param name="message"></param>
	ShowMessageUserEventHandlerEventArgs ShowError(string message, Exception? exception = null, ShowMessageButtons Buttons = ShowMessageButtons.OKCancel);
	/// <summary>
	/// Display warning to user
	/// </summary>
	/// <param name="exception"></param>
	/// <param name="message"></param>
	ShowMessageUserEventHandlerEventArgs ShowWarning(string message, Exception? exception = null, ShowMessageButtons Buttons = ShowMessageButtons.OKCancel);
}

/// <summary>
/// Notification delegate
/// </summary>
/// <param name="sender"></param>
/// <param name="message"></param>
/// <param name="color"></param>
public delegate void NotifyUserEventHandler(object sender, string message, Color color);

/// <summary>
/// Show Message delegate
/// </summary>
/// <param name="sender"></param>
/// <param name="message"></param>
public delegate void ShowMessageUserEventHandler(object sender, ShowMessageUserEventHandlerEventArgs message);

/// <summary>
/// Show Message arguments
/// </summary>
public class ShowMessageUserEventHandlerEventArgs
{
	public LogLevel Level;
	public Exception? Error;
	public string Message = string.Empty;
	public bool IsOK;
	public ShowMessageButtons Buttons;
}

public enum ShowMessageButtons
{
	/// <summary>
	/// The message box contains an OK button.
	/// </summary>
	OK,
	/// <summary>
	/// The message box contains OK and Cancel buttons.
	/// </summary>
	OKCancel,
}
