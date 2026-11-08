using System.Globalization;
using Microsoft.Extensions.Logging;
using TQVaultAE.Logs;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI;

/// <summary>
/// Messages and questions put to the user. Every box gets the reading direction of the UI culture and its first button
/// as default, and is logged, so the automation endpoint can tell what was shown.
/// </summary>
/// <remarks>
/// For the GUI code. The services reach the user through <see cref="TQVaultAE.Application.Contracts.Services.IUserNotifier"/>,
/// whose messages the main form shows with this service.
/// </remarks>
public interface IMessageBoxService
{
	/// <summary>Shows <paramref name="message"/> with an OK button.</summary>
	void Inform(string message, string caption, MessageBoxIcon icon = MessageBoxIcon.None, IWin32Window? owner = null);

	/// <summary>Shows <paramref name="exception"/>, formatted with its inner exceptions, with an OK button.</summary>
	void ShowException(Exception exception, string? caption = null, MessageBoxIcon icon = MessageBoxIcon.Warning);

	/// <summary>Asks a Yes / No question.</summary>
	/// <param name="defaultNo">No is the default button, for a question whose Yes cannot easily be undone.</param>
	/// <returns><see langword="true"/> when the user answered Yes.</returns>
	bool Confirm(string message, string caption, MessageBoxIcon icon = MessageBoxIcon.Question, bool defaultNo = false);

	/// <summary>Shows <paramref name="message"/> with any <paramref name="buttons"/>.</summary>
	/// <returns>The button the user pressed.</returns>
	DialogResult Ask(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);
}

/// <summary>
/// <see cref="IMessageBoxService"/> over <see cref="MessageBox"/>.
/// </summary>
public sealed class MessageBoxService : IMessageBoxService
{
	private readonly ILogger<MessageBoxService> _log;

	public MessageBoxService(ILogger<MessageBoxService> log)
		=> this._log = log;

	/// <summary>Right-to-left reading for the UI cultures that read that way.</summary>
	public static MessageBoxOptions ReadingOptions
		=> CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft
			? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading
			: default;

	public void Inform(string message, string caption, MessageBoxIcon icon = MessageBoxIcon.None, IWin32Window? owner = null)
		=> this.Show(owner, message, caption, MessageBoxButtons.OK, icon);

	public void ShowException(Exception exception, string? caption = null, MessageBoxIcon icon = MessageBoxIcon.Warning)
		=> this.Show(null, this._log.FormatException(exception), caption ?? Resources.GlobalError, MessageBoxButtons.OK, icon);

	public bool Confirm(string message, string caption, MessageBoxIcon icon = MessageBoxIcon.Question, bool defaultNo = false)
		=> this.Show(null, message, caption, MessageBoxButtons.YesNo, icon, defaultNo ? MessageBoxDefaultButton.Button2 : MessageBoxDefaultButton.Button1) == DialogResult.Yes;

	public DialogResult Ask(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		=> this.Show(null, message, caption, buttons, icon);

	private DialogResult Show(IWin32Window? owner, string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
	{
		this._log.LogInformation("Message box \"{Caption}\" ({Buttons}): {Message}", caption, buttons, message);

#if DEBUG
		// Automation can queue the answers, so the box is not shown and nothing waits for a click.
		if (Automation.AutomationHost.TryTakeDialogAnswer(caption, message, buttons, out var queued))
		{
			this._log.LogInformation("Message box \"{Caption}\" answered {Answer} by automation", caption, queued);
			return queued;
		}
#endif

		var answer = owner is null
			? MessageBox.Show(message, caption, buttons, icon, defaultButton, ReadingOptions)
			: MessageBox.Show(owner, message, caption, buttons, icon, defaultButton, ReadingOptions);

		if (buttons != MessageBoxButtons.OK)
			this._log.LogInformation("Message box \"{Caption}\" answered {Answer}", caption, answer);

		return answer;
	}
}
