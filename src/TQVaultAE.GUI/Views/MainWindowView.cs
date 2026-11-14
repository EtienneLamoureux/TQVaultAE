namespace TQVaultAE.GUI.Views;

/// <summary>
/// The contract shared by the views the main window shows in place of its main content (<see cref="IMainWindowView"/>):
/// they are left by their OK (<see cref="Applied"/>) or their Cancel (<see cref="Cancelled"/>), and Escape presses that
/// Cancel, as in a dialog.
/// </summary>
internal class MainWindowView : UserControl
{
	/// <summary>OK was pressed and the view's checks passed: the view tells what its OK applied.</summary>
	public event EventHandler? Applied;

	/// <summary>Cancel or Escape was pressed: the view's edits are dropped.</summary>
	public event EventHandler? Cancelled;

	/// <summary>The button Escape presses: the view's Cancel. None, Escape goes on as usual.</summary>
	protected virtual IButtonControl? EscapeButton => null;

	/// <summary>Raises <see cref="Applied"/>.</summary>
	protected void RaiseApplied() => this.Applied?.Invoke(this, EventArgs.Empty);

	/// <summary>Raises <see cref="Cancelled"/>.</summary>
	protected void RaiseCancelled() => this.Cancelled?.Invoke(this, EventArgs.Empty);

	/// <summary>
	/// Escape presses <see cref="EscapeButton"/>, as in a dialog: once the focused control left it (an open drop-down list
	/// closes first).
	/// </summary>
	protected override bool ProcessDialogKey(Keys keyData)
	{
		if (keyData == Keys.Escape && this.EscapeButton is { } cancel)
		{
			cancel.PerformClick();
			return true;
		}

		return base.ProcessDialogKey(keyData);
	}
}
