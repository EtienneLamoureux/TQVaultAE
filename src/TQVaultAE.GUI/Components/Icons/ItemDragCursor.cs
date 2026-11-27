using TQVaultAE.GUI.Interop;

namespace TQVaultAE.GUI.Components.Icons;

/// <summary>
/// Mouse cursor showing an item bitmap, with its hot spot at the centre: Windows draws and moves it, so dragging over
/// a control costs no repaint, and the bitmap keeps its transparency.
/// </summary>
internal sealed class ItemDragCursor : IDisposable
{
	private IntPtr _handle;

	private ItemDragCursor(IntPtr handle)
	{
		this._handle = handle;
		this.Cursor = new Cursor(handle);
	}

	public Cursor Cursor { get; }

	/// <summary>Builds a cursor from <paramref name="bitmap"/>, drawn at <paramref name="scale"/>.</summary>
	/// <returns><c>null</c> when Windows cannot create it.</returns>
	public static ItemDragCursor? Create(Bitmap bitmap, float scale)
	{
		int width = Math.Max(1, (int)Math.Round(bitmap.Width * scale));
		int height = Math.Max(1, (int)Math.Round(bitmap.Height * scale));
		using var sized = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		using (var g = Graphics.FromImage(sized))
		{
			g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			g.DrawImage(bitmap, 0, 0, width, height);
		}

		IntPtr icon = sized.GetHicon();
		try
		{
			if (!User32.GetIconInfo(icon, out var info))
				return null;
			try
			{
				info.fIcon = 0; // FALSE: a cursor
				info.xHotspot = width / 2;
				info.yHotspot = height / 2;
				IntPtr cursor = User32.CreateIconIndirect(ref info);
				return cursor == IntPtr.Zero ? null : new ItemDragCursor(cursor);
			}
			finally
			{
				Gdi32.DeleteObject(info.hbmColor);
				Gdi32.DeleteObject(info.hbmMask);
			}
		}
		finally
		{
			User32.DestroyIcon(icon);
		}
	}

	public void Dispose()
	{
		if (this._handle == IntPtr.Zero)
			return;
		this.Cursor.Dispose();
		User32.DestroyCursor(this._handle);
		this._handle = IntPtr.Zero;
	}
}
