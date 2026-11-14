namespace TQVaultAE.GUI.Infrastructure;

/// <summary>
/// A dialog, or a view of the main window, built at its first opening and kept for the next ones, since building it is
/// the slow part; it is disposed with its owner. A modal dialog closed by its buttons or its close button is only hidden
/// (a view is taken out of the window), so the kept one is shown again as is: the dialog or view itself starts each
/// opening from the current state.
/// </summary>
/// <remarks>
/// The dialog is scaled once to the user's zoom when it is built. After a zoom change it is built again at the new zoom:
/// scaling it back and forth would drift by rounding (margins and positions).
/// </remarks>
/// <typeparam name="T">The dialog or view type.</typeparam>
internal sealed class KeptDialog<T> where T : Control
{
	private T? dialog;

	/// <summary>The user's zoom <see cref="dialog"/> was built at.</summary>
	private float zoom;

	/// <summary>The DPI <see cref="dialog"/> was built at, 0 when the caller does not key it by the DPI.</summary>
	private int dpi;

	/// <summary>Whether the owner disposes the kept dialog when it closes.</summary>
	private bool disposedWithOwner;

	/// <summary>Gets the kept dialog, null before its first opening and once disposed.</summary>
	public T? Current => this.dialog is { IsDisposed: false } kept ? kept : null;

	/// <summary>
	/// Gets the dialog for an opening at the user's <paramref name="zoom"/>: the kept one, or a new one from
	/// <paramref name="create"/>, at the first opening and when the kept one was built at another zoom.
	/// </summary>
	/// <param name="owner">The window that disposes the dialog when it closes.</param>
	/// <param name="zoom">The user's zoom in use.</param>
	/// <param name="create">Builds the dialog, scaled to <paramref name="zoom"/>.</param>
	/// <param name="built">Whether the dialog was built by this call.</param>
	/// <param name="replacing">Called with the dialog built at another zoom, before it is disposed.</param>
	/// <param name="dpi">
	/// The DPI in use, for a view taken out of the window: put back in a window of another DPI, Windows Forms scales it by
	/// the ratio of the DPIs on top of its zoom, so it is built again. 0 for a dialog, which follows its own monitor.
	/// </param>
	public T Get(Form owner, float zoom, Func<T> create, out bool built, Action<T>? replacing = null, int dpi = 0)
	{
		if (this.Current is { } stale && (this.zoom != zoom || this.dpi != dpi))
		{
			replacing?.Invoke(stale);
			stale.Dispose();
			this.dialog = null;
		}

		built = this.Current is null;
		if (built)
		{
			this.dialog = create();
			this.zoom = zoom;
			this.dpi = dpi;

			if (!this.disposedWithOwner)
			{
				this.disposedWithOwner = true;
				owner.FormClosed += (_, _) => this.dialog?.Dispose();
			}
		}

		return this.dialog!;
	}
}
