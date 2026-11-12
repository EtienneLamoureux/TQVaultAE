using System.Runtime.InteropServices;

namespace TQVaultAE.GUI.Infrastructure;

/// <summary>
/// Many changes to a window's controls made as one: its drawing is frozen and the layouts of the given containers
/// suspended until the batch is disposed, then each container is laid out once and the window drawn once. Without it,
/// each change (a list shown or hidden) lays out and repaints the whole flow of panels, which shows as clipping.
/// </summary>
internal sealed partial class LayoutBatch : IDisposable
{
	private const int WM_SETREDRAW = 0x000B;

	private readonly Control window;
	private readonly Control[] containers;

	/// <summary>Freezes the drawing of <paramref name="window"/> and suspends the layout of <paramref name="containers"/>.</summary>
	/// <param name="window">The control whose drawing is frozen: the dialog, or the panel that holds the changes.</param>
	/// <param name="containers">The containers whose layout waits, outermost first.</param>
	public LayoutBatch(Control window, params Control[] containers)
	{
		this.window = window;
		this.containers = containers;
		if (window.IsHandleCreated)
			SendMessage(window.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
		foreach (var container in containers)
			container.SuspendLayout();
	}

	/// <summary>Lays the containers out once, innermost first, then draws the window again.</summary>
	public void Dispose()
	{
		for (int i = this.containers.Length - 1; i >= 0; i--)
			this.containers[i].ResumeLayout(true);

		if (this.window.IsHandleCreated)
		{
			SendMessage(this.window.Handle, WM_SETREDRAW, 1, IntPtr.Zero);
			this.window.Invalidate(true);
		}
	}

	[LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
	private static partial IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
}
