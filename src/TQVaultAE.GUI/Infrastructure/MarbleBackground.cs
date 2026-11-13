using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Infrastructure;

/// <summary>
/// The character editor's marble (<see cref="Images.CharacterEditorMarble"/>) as a background that repeats without
/// seams, for a window of any size.
/// </summary>
internal static class MarbleBackground
{
	/// <summary>
	/// The part of the marble kept for the tile: the picture darkens near its left, right and bottom edges, which would
	/// show as bands where the tiles meet.
	/// </summary>
	private static readonly Rectangle Inner = Rectangle.FromLTRB(170, 0, 940, 690);

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

	/// <summary>Gives <paramref name="form"/> the tiled marble as its background.</summary>
	public static void ApplyTo(Form form)
	{
		form.BackgroundImage = Tile;
		form.BackgroundImageLayout = ImageLayout.Tile;
	}

	/// <summary>
	/// Paints the background of a window given the marble by <see cref="ApplyTo"/>, as Windows Forms would (the tiles
	/// start at its client origin), on the area to paint only: a transparent control asks for the part behind it.
	/// </summary>
	/// <returns>False when the window has another background, left to Windows Forms.</returns>
	public static bool TryPaint(Form form, PaintEventArgs e)
	{
		if (tile is null || !ReferenceEquals(form.BackgroundImage, tile) || form.BackgroundImageLayout != ImageLayout.Tile
			|| SystemInformation.HighContrast || form.RightToLeft == RightToLeft.Yes || form.AutoScrollPosition != Point.Empty)
			return false;

		e.Graphics.FillRectangle(Brush, e.ClipRectangle);
		return true;
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
