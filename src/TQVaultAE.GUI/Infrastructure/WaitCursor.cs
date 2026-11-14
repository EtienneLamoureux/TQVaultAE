namespace TQVaultAE.GUI.Infrastructure;

/// <summary>
/// Shows the wait cursor over a control during a blocking operation, and gives the control its cursor back when
/// disposed, whatever the operation ends with: <c>using (new WaitCursor(this)) { ... }</c>.
/// </summary>
internal readonly struct WaitCursor : IDisposable
{
	private readonly Control control;
	private readonly Cursor previous;

	/// <param name="control">The window or control the cursor waits over.</param>
	public WaitCursor(Control control)
	{
		this.control = control;
		this.previous = control.Cursor;
		control.Cursor = Cursors.WaitCursor;
	}

	public void Dispose()
	{
		if (this.control is { IsDisposed: false })
			this.control.Cursor = this.previous;
	}
}
