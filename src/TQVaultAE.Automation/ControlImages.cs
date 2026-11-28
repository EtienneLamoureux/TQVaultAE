using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TQVaultAE.Automation;

/// <summary>
/// Pictures of controls for the automation tools of TQVaultAE and ARZExplorer.
/// </summary>
public static partial class ControlImages
{
	/// <summary>Encodes <paramref name="image"/> as PNG.</summary>
	public static byte[] Png(Image image)
	{
		ArgumentNullException.ThrowIfNull(image);
		using var stream = new MemoryStream();
		image.Save(stream, ImageFormat.Png);
		return stream.ToArray();
	}

	/// <summary>Draws <paramref name="control"/> to a bitmap of <paramref name="size"/> (its own size by default) with <see cref="Control.DrawToBitmap"/>, which works while it is hidden.</summary>
	public static Bitmap Render(Control control, Size? size = null)
	{
		ArgumentNullException.ThrowIfNull(control);
		var area = size ?? control.Size;
		var bitmap = new Bitmap(area.Width, area.Height);
		control.DrawToBitmap(bitmap, new Rectangle(Point.Empty, area));
		return bitmap;
	}

	/// <summary>PNG of <see cref="Render"/>.</summary>
	public static byte[] RenderPng(Control control)
	{
		using var bitmap = Render(control);
		return Png(bitmap);
	}

	/// <summary>PrintWindow: the window's content as the desktop composes it, not a WM_PRINT repaint.</summary>
	private const uint PW_RENDERFULLCONTENT = 0x2;

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint flags);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool GetWindowRect(IntPtr hWnd, out Win32Rect rect);

	[StructLayout(LayoutKind.Sequential)]
	private struct Win32Rect
	{
		public int Left, Top, Right, Bottom;
	}

	/// <summary>
	/// The pixels of a top-level window as its desktop composes it (PrintWindow), with its screen bounds: for windows
	/// that are no <see cref="Control"/>, such as a native tool tip.
	/// </summary>
	public static (Bitmap Image, Rectangle Bounds) RenderWindow(IntPtr hwnd)
	{
		if (!GetWindowRect(hwnd, out var r))
			throw new InvalidOperationException("GetWindowRect failed.");
		var bounds = Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom);
		return (Print(hwnd, bounds.Size), bounds);
	}

	private static Bitmap Print(IntPtr hwnd, Size size)
	{
		var bitmap = new Bitmap(size.Width, size.Height);
		using var g = Graphics.FromImage(bitmap);
		var hdc = g.GetHdc();
		try
		{
			if (!PrintWindow(hwnd, hdc, PW_RENDERFULLCONTENT))
				throw new InvalidOperationException("PrintWindow failed.");
		}
		finally
		{
			g.ReleaseHdc(hdc);
		}
		return bitmap;
	}

	/// <summary>
	/// The pixels of <paramref name="control"/> as the window shows them on its desktop (the hidden one in headless
	/// mode): its top-level window is copied from the desktop's composition, after its pending paints, then cropped to
	/// the control. Unlike <see cref="Control.DrawToBitmap"/> it keeps what the window really drew (title bar, overlaps,
	/// transparent children).
	/// </summary>
	public static Bitmap RenderAsShown(Control control)
	{
		ArgumentNullException.ThrowIfNull(control);
		var top = control.TopLevelControl ?? control;
		top.Update();
		var window = top is Form form ? form.Bounds : top.RectangleToScreen(top.ClientRectangle);
		using var whole = Print(top.Handle, window.Size);

		var area = control.RectangleToScreen(control.ClientRectangle);
		if (control == top && top is Form)
			area = window;
		area.Offset(-window.X, -window.Y);
		return whole.Clone(Rectangle.Intersect(area, new Rectangle(Point.Empty, whole.Size)), whole.PixelFormat);
	}
}
