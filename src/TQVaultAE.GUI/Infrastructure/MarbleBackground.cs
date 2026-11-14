using System.Runtime.CompilerServices;
using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Infrastructure;

/// <summary>
/// A marble as the background of a window of any size: the character editor's (<see cref="Images.CharacterEditorMarble"/>)
/// repeated without seams, or the main window's stretched over it.
/// </summary>
internal static class MarbleBackground
{
	/// <summary>The marbles a window can have as its background.</summary>
	public enum Marble
	{
		/// <summary>The character editor's marble, mirrored into a tile that repeats.</summary>
		CharacterEditor,

		/// <summary>The marble of the main window's background, without its dark band and border, stretched.</summary>
		MainWindow,
	}

	/// <summary>
	/// The part of the marble kept for the tile: the picture darkens near its left, right and bottom edges, which would
	/// show as bands where the tiles meet.
	/// </summary>
	private static readonly Rectangle Inner = Rectangle.FromLTRB(170, 0, 940, 690);

	/// <summary>
	/// The marble of the main window's background: below the dark band at its top, inside the black border around it.
	/// </summary>
	private static readonly Rectangle MainWindowMarble = Rectangle.FromLTRB(4, 58, 1074, 905);

	private static Bitmap? tile;

	/// <summary>
	/// The tile: the inner part of the marble next to its mirror images (2 x 2), so that each edge meets its own mirror.
	/// Made once and shared by every window.
	/// </summary>
	private static Bitmap Tile => tile ??= BuildTile();

	private static TextureBrush? brush;

	/// <summary>
	/// The brush that tiles the marble, made once: Windows Forms makes one from the whole tile at each background paint,
	/// which a transparent control asks of its window too, so a dialog with many controls took over a second to draw.
	/// </summary>
	private static TextureBrush Brush => brush ??= new TextureBrush(Tile, System.Drawing.Drawing2D.WrapMode.Tile);

	private static Bitmap? mainWindowMarble;

	/// <summary>The main window's marble (<see cref="MainWindowMarble"/>), made once and shared by every window.</summary>
	private static Bitmap MainWindowMarbleImage => mainWindowMarble ??= BuildMainWindowMarble();

	/// <summary>
	/// The main window's marble stretched to the size of each window having it, made again when that size changes: Windows
	/// Forms stretches the whole picture at each background paint, which a transparent control asks of its window too.
	/// </summary>
	private static readonly ConditionalWeakTable<Control, Bitmap> stretched = [];

	/// <summary>Gives <paramref name="form"/> (a window, or a view laid in one) the marble as its background.</summary>
	public static void ApplyTo(Control form, Marble marble = Marble.CharacterEditor)
	{
		if (marble == Marble.MainWindow)
		{
			form.BackgroundImage = MainWindowMarbleImage;
			form.BackgroundImageLayout = ImageLayout.Stretch;
			form.Disposed -= DropStretched;
			form.Disposed += DropStretched;
			// The marble moves under the transparent panels too when the window is resized.
			form.Resize -= RepaintAll;
			form.Resize += RepaintAll;
			return;
		}

		form.BackgroundImage = Tile;
		form.BackgroundImageLayout = ImageLayout.Tile;
	}

	/// <summary>
	/// Paints the background of a window or view given the marble by <see cref="ApplyTo"/>, as Windows Forms would (the
	/// tiles start at its client origin, the stretched marble covers its client area), on the area to paint only: a
	/// transparent control asks for the part behind it.
	/// </summary>
	/// <returns>False when the window has another background, left to Windows Forms.</returns>
	public static bool TryPaint(ScrollableControl form, PaintEventArgs e)
	{
		if (form.BackgroundImage is null || SystemInformation.HighContrast || form.RightToLeft == RightToLeft.Yes
			|| form.AutoScrollPosition != Point.Empty)
			return false;

		if (ReferenceEquals(form.BackgroundImage, tile) && form.BackgroundImageLayout == ImageLayout.Tile)
		{
			e.Graphics.FillRectangle(Brush, e.ClipRectangle);
			return true;
		}

		if (ReferenceEquals(form.BackgroundImage, mainWindowMarble) && form.BackgroundImageLayout == ImageLayout.Stretch)
		{
			var size = form.ClientSize;
			if (size.Width <= 0 || size.Height <= 0)
				return true;

			if (!stretched.TryGetValue(form, out var picture) || picture.Size != size)
			{
				var previous = picture;
				picture = Stretch(mainWindowMarble, size);
				stretched.AddOrUpdate(form, picture);
				previous?.Dispose();
			}

			e.Graphics.DrawImage(picture, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
			return true;
		}

		return false;
	}

	private static void RepaintAll(object? sender, EventArgs e) => (sender as Control)?.Invalidate(true);

	private static void DropStretched(object? sender, EventArgs e)
	{
		if (sender is Control form && stretched.TryGetValue(form, out var picture))
		{
			stretched.Remove(form);
			picture.Dispose();
		}
	}

	private static Bitmap Stretch(Bitmap source, Size size)
	{
		var result = new Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
		using var g = Graphics.FromImage(result);
		g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
		g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
		using var attributes = new System.Drawing.Imaging.ImageAttributes();
		// Without it, the edge pixels blend with the transparent outside of the picture.
		attributes.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
		g.DrawImage(source, new Rectangle(Point.Empty, size), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
		return result;
	}

	/// <summary>The main window's marble: its background without the dark band at its top and the border around it.</summary>
	private static Bitmap BuildMainWindowMarble()
	{
		using var background = (Bitmap)new System.ComponentModel.ComponentResourceManager(typeof(Forms.MainForm)).GetObject("$this.BackgroundImage", System.Globalization.CultureInfo.InvariantCulture)!;
		var part = Rectangle.Intersect(MainWindowMarble, new Rectangle(Point.Empty, background.Size));
		var result = new Bitmap(part.Width, part.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
		using var g = Graphics.FromImage(result);
		g.DrawImage(background, new Rectangle(Point.Empty, part.Size), part, GraphicsUnit.Pixel);
		return result;
	}

	private static Bitmap BuildTile()
	{
		using var marble = Images.CharacterEditorMarble;
		var inner = Rectangle.Intersect(Inner, new Rectangle(Point.Empty, marble.Size));

		using var part = marble.Clone(inner, marble.PixelFormat);
		var result = new Bitmap(inner.Width * 2, inner.Height * 2, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
		using var g = Graphics.FromImage(result);
		void Draw(RotateFlipType flip, int x, int y)
		{
			using var copy = (Bitmap)part.Clone();
			copy.RotateFlip(flip);
			g.DrawImageUnscaled(copy, x, y);
		}

		Draw(RotateFlipType.RotateNoneFlipNone, 0, 0);
		Draw(RotateFlipType.RotateNoneFlipX, inner.Width, 0);
		Draw(RotateFlipType.RotateNoneFlipY, 0, inner.Height);
		Draw(RotateFlipType.RotateNoneFlipXY, inner.Width, inner.Height);
		return result;
	}
}
