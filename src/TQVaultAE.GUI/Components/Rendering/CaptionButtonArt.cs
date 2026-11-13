using System.Drawing.Drawing2D;

namespace TQVaultAE.GUI.Components.Rendering;

/// <summary>
/// The custom border's caption buttons (close, maximize, normalize, minimize), drawn at the size they are shown instead
/// of small pictures stretched by the zoom and the screen DPI, in the look of the game's buttons.
/// </summary>
internal static class CaptionButtonArt
{
	/// <summary>The state a button is drawn in.</summary>
	public enum State
	{
		Up,
		Over,
		Down,
	}

	/// <summary>What a button shows.</summary>
	public enum Glyph
	{
		/// <summary>A white cross on a red square, in a gold frame.</summary>
		Close,

		/// <summary>A window (a frame with a thick top) on a dark square, in a gold frame; also the normalize button.</summary>
		Window,

		/// <summary>A bar at the bottom of a dark square, in a gold frame.</summary>
		Minimize,
	}

	/// <summary>Draws a button in a square bitmap of <paramref name="size"/> pixels.</summary>
	public static Bitmap Render(Glyph glyph, int size, State state)
		=> glyph == Glyph.Close ? RenderClose(size, state) : RenderSmall(glyph, size, state);

	#region Close

	/// <summary>The colours of one state of the close button: gold frame, red square and cross.</summary>
	private readonly record struct ClosePalette(
		Color GoldLight, Color Gold, Color GoldDark, Color Gap,
		Color RedTop, Color RedBottom, Color RedLight, Color RedShadow,
		Color Cross, Color CrossHalo);

	private static ClosePalette ClosePaletteOf(State state) => state switch
	{
		State.Over => new(
			Color.FromArgb(0xE6, 0xC2, 0x5E), Color.FromArgb(0xD8, 0xB0, 0x47), Color.FromArgb(0xB0, 0x8D, 0x36), Color.FromArgb(0x5E, 0x47, 0x1D),
			Color.FromArgb(0xD3, 0x0C, 0x0C), Color.FromArgb(0xE8, 0x40, 0x3C), Color.FromArgb(0xF7, 0xB7, 0xB7), Color.FromArgb(0x8A, 0x10, 0x10),
			Color.FromArgb(0xFE, 0xFE, 0xFE), Color.FromArgb(0xF8, 0xC2, 0xC2)),
		State.Down => new(
			Color.FromArgb(0xC4, 0x99, 0x3E), Color.FromArgb(0xB8, 0x92, 0x38), Color.FromArgb(0x8E, 0x70, 0x2A), Color.FromArgb(0x1F, 0x16, 0x0A),
			Color.FromArgb(0x46, 0x04, 0x04), Color.FromArgb(0x6A, 0x17, 0x17), Color.FromArgb(0x84, 0x69, 0x69), Color.FromArgb(0x2A, 0x02, 0x02),
			Color.FromArgb(0x8E, 0x8D, 0x8D), Color.FromArgb(0x70, 0x42, 0x42)),
		_ => new(
			Color.FromArgb(0xDB, 0xB4, 0x50), Color.FromArgb(0xD2, 0xA6, 0x3A), Color.FromArgb(0xA5, 0x82, 0x31), Color.FromArgb(0x3A, 0x2C, 0x10),
			Color.FromArgb(0x76, 0x07, 0x07), Color.FromArgb(0xB1, 0x26, 0x25), Color.FromArgb(0xB9, 0x6F, 0x6F), Color.FromArgb(0x4A, 0x04, 0x04),
			Color.FromArgb(0xED, 0xED, 0xED), Color.FromArgb(0xD3, 0xBD, 0xBD)),
	};

	/// <summary>
	/// The close button. Proportions are those of the 26 pixel design: a gold frame of 3, a dark gap of 2, then the red
	/// square with its cross.
	/// </summary>
	private static Bitmap RenderClose(int size, State state)
	{
		size = Math.Max(size, 8);
		var p = ClosePaletteOf(state);
		var bitmap = new Bitmap(size, size);
		using var g = Graphics.FromImage(bitmap);
		g.PixelOffsetMode = PixelOffsetMode.HighQuality;

		float unit = size / 26F;
		int frame = Math.Max(2, (int)Math.Round(3 * unit));
		int gap = Math.Max(1, (int)Math.Round(2 * unit));
		int bevel = Math.Max(1, (int)Math.Round(unit));

		var gapRect = DrawGoldFrame(g, size, frame, bevel, p.GoldLight, p.GoldDark, p.Gap);

		// Red square: darker at the top, lighter at the bottom, with a light bevel on the top and left edges and a
		// shadow on the others.
		var red = Rectangle.Inflate(gapRect, -gap, -gap);
		if (red.Width <= 0 || red.Height <= 0)
			return bitmap;

		using (var light = new SolidBrush(p.RedLight))
			g.FillRectangle(light, red);
		using (var shadow = new SolidBrush(p.RedShadow))
			g.FillRectangle(shadow, red.X + bevel, red.Y + bevel, red.Width - bevel, red.Height - bevel);
		var body = new Rectangle(red.X + bevel, red.Y + bevel, red.Width - 2 * bevel, red.Height - 2 * bevel);
		if (body.Width > 0 && body.Height > 0)
		{
			using var fill = new LinearGradientBrush(body, p.RedTop, p.RedBottom, LinearGradientMode.Vertical);
			g.FillRectangle(fill, body);
		}

		// The cross, a soft halo under a white stroke, inset from the red square.
		g.SmoothingMode = SmoothingMode.AntiAlias;
		float inset = red.Width * 0.22F;
		var a = new PointF(red.Left + inset, red.Top + inset);
		var b = new PointF(red.Right - inset, red.Bottom - inset);
		var c = new PointF(red.Right - inset, red.Top + inset);
		var d = new PointF(red.Left + inset, red.Bottom - inset);
		float stroke = Math.Max(1.5F, 2.6F * unit);
		using (var halo = new Pen(p.CrossHalo, stroke + Math.Max(1F, unit)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
		{
			g.DrawLine(halo, a, b);
			g.DrawLine(halo, c, d);
		}
		using (var cross = new Pen(p.Cross, stroke) { StartCap = LineCap.Round, EndCap = LineCap.Round })
		{
			g.DrawLine(cross, a, b);
			g.DrawLine(cross, c, d);
		}

		return bitmap;
	}

	#endregion

	#region Small buttons

	/// <summary>The colours of one state of a small button: gold frame, dark square and glyph.</summary>
	private readonly record struct SmallPalette(
		Color GoldLight, Color GoldDark, Color Ring, Color Inside, Color GlyphLight, Color GlyphShade);

	private static SmallPalette SmallPaletteOf(State state) => state switch
	{
		State.Over => new(
			Color.FromArgb(0xFA, 0xEE, 0xA0), Color.FromArgb(0xDA, 0xC6, 0x62), Color.FromArgb(0x84, 0x67, 0x33),
			Color.FromArgb(0x67, 0x47, 0x1A), Color.FromArgb(0xFE, 0xFD, 0xFD), Color.FromArgb(0xC3, 0xB6, 0xA5)),
		State.Down => new(
			Color.FromArgb(0x82, 0x68, 0x35), Color.FromArgb(0x66, 0x50, 0x21), Color.FromArgb(0x33, 0x26, 0x10),
			Color.FromArgb(0x21, 0x17, 0x08), Color.FromArgb(0x99, 0x98, 0x98), Color.FromArgb(0x6A, 0x66, 0x60)),
		_ => new(
			Color.FromArgb(0xD8, 0xAD, 0x59), Color.FromArgb(0xAB, 0x86, 0x37), Color.FromArgb(0x55, 0x3E, 0x1C),
			Color.FromArgb(0x37, 0x26, 0x0E), Color.FromArgb(0xFD, 0xFD, 0xFD), Color.FromArgb(0xB1, 0xAA, 0xA0)),
	};

	/// <summary>
	/// A small button. Proportions are those of the 15 pixel design: a gold frame of 2, a dark ring of 1, then the dark
	/// square with its glyph (a window from 4 to 11 with a thick top, or a bar from 9 to 11).
	/// </summary>
	private static Bitmap RenderSmall(Glyph glyph, int size, State state)
	{
		size = Math.Max(size, 7);
		var p = SmallPaletteOf(state);
		var bitmap = new Bitmap(size, size);
		using var g = Graphics.FromImage(bitmap);
		g.PixelOffsetMode = PixelOffsetMode.HighQuality;

		float unit = size / 15F;
		int frame = Math.Max(1, (int)Math.Round(2 * unit));
		int ring = Math.Max(1, (int)Math.Round(unit));

		var ringRect = DrawGoldFrame(g, size, frame, Math.Max(1, (int)Math.Round(unit * 0.7F)), p.GoldLight, p.GoldDark, p.Ring);
		var inside = Rectangle.Inflate(ringRect, -ring, -ring);
		if (inside.Width <= 0 || inside.Height <= 0)
			return bitmap;
		using (var dark = new SolidBrush(p.Inside))
			g.FillRectangle(dark, inside);

		// The glyph, from the design's cells: left 4, right 11, top 4, bottom 11 (exclusive), lines one cell thick.
		int X(float design) => (int)Math.Round(design * unit);
		int line = Math.Max(1, X(1));
		var glyphRect = Rectangle.FromLTRB(X(4), X(4), X(11), X(11));
		using var light = new SolidBrush(p.GlyphLight);
		using var shade = new SolidBrush(p.GlyphShade);
		if (glyph == Glyph.Minimize)
		{
			var bar = Rectangle.FromLTRB(glyphRect.Left, X(9), glyphRect.Right, X(11));
			g.FillRectangle(light, bar.X, bar.Y, bar.Width, Math.Max(1, bar.Height / 2));
			g.FillRectangle(shade, bar.X, bar.Y + Math.Max(1, bar.Height / 2), bar.Width, bar.Height - Math.Max(1, bar.Height / 2));
		}
		else
		{
			// A window: light top bar two lines thick and right side, shaded left side and bottom.
			int top = Math.Max(line, X(2));
			g.FillRectangle(light, glyphRect.Left, glyphRect.Top, glyphRect.Width, top);
			g.FillRectangle(light, glyphRect.Right - line, glyphRect.Top, line, glyphRect.Height);
			g.FillRectangle(shade, glyphRect.Left, glyphRect.Top + top, line, glyphRect.Height - top);
			g.FillRectangle(shade, glyphRect.Left, glyphRect.Bottom - line, glyphRect.Width - line, line);
		}

		return bitmap;
	}

	#endregion

	/// <summary>
	/// Draws the gold frame shared by every caption button: a diagonal gradient, light at the top left, with a darker
	/// inner edge, then fills the inside with <paramref name="inside"/>.
	/// </summary>
	/// <returns>The rectangle inside the frame.</returns>
	private static Rectangle DrawGoldFrame(Graphics g, int size, int frame, int bevel, Color light, Color dark, Color inside)
	{
		var outer = new Rectangle(0, 0, size, size);
		using (var gold = new LinearGradientBrush(outer, light, dark, LinearGradientMode.ForwardDiagonal))
		{
			gold.SetBlendTriangularShape(0.5F, 0.35F);
			g.FillRectangle(gold, outer);
		}
		using (var edge = new SolidBrush(dark))
			g.FillRectangle(edge, Rectangle.Inflate(outer, -(frame - bevel), -(frame - bevel)));

		var rect = Rectangle.Inflate(outer, -frame, -frame);
		using (var fill = new SolidBrush(inside))
			g.FillRectangle(fill, rect);
		return rect;
	}
}
