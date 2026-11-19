namespace TQVaultAE.GUI.Components.Character;

/// <summary>
/// Text and bars of the game's character window panels (the statistics panel, the character editor's side panel), placed
/// in pixels of the game window times <see cref="Scale"/>.
/// </summary>
/// <param name="Scale">The size of a game pixel on screen.</param>
/// <param name="Fallback">The font used when a panel font is not set yet.</param>
internal readonly record struct PanelPainter(float Scale, Font Fallback)
{
	/// <summary>The thin bar between the parts of a panel.</summary>
	public const string StatsBar = """InGameUI\CharacterScreen\StatsBar01.tex""";

	/// <summary>The color of a row's label.</summary>
	public static readonly Color Grey = Color.FromArgb(200, 200, 190);

	/// <summary>The right end of a row's value, in game pixels.</summary>
	private const int RowRight = 243;

	/// <summary>The left end of a row's label, in game pixels.</summary>
	private const int RowLeft = 15;

	public int Scaled(int value) => (int)Math.Round(value * this.Scale);

	public Rectangle ScaledRect(Rectangle r) => new(this.Scaled(r.X), this.Scaled(r.Y), this.Scaled(r.Width), this.Scaled(r.Height));

	/// <summary>Draws <paramref name="text"/> in <paramref name="box"/> (game pixels), cut with an ellipsis, on one line unless <paramref name="wrap"/>.</summary>
	public void DrawText(Graphics g, string text, Font? font, Color color, Rectangle box, StringAlignment alignment, bool wrap = false)
	{
		if (string.IsNullOrEmpty(text))
			return;

		using var format = new StringFormat
		{
			Alignment = alignment,
			LineAlignment = wrap ? StringAlignment.Center : StringAlignment.Near,
			Trimming = StringTrimming.EllipsisCharacter,
			FormatFlags = wrap ? 0 : StringFormatFlags.NoWrap,
		};
		using var brush = new SolidBrush(color);
		g.DrawString(text, font ?? this.Fallback, brush, this.ScaledRect(box), format);
	}

	/// <summary>
	/// A row 16 game pixels high at <paramref name="y"/>: the value in white at the right, the label in grey from the left up
	/// to the value, and not past <paramref name="labelLimit"/>.
	/// </summary>
	public void Row(Graphics g, string label, Font? labelFont, string value, Font valueFont, int y, int labelLimit = int.MaxValue)
	{
		int valueWidth = string.IsNullOrEmpty(value) ? 0 : (int)Math.Ceiling(g.MeasureString(value, valueFont).Width / this.Scale) + 4;
		this.DrawText(g, value, valueFont, Color.White, new Rectangle(RowRight - valueWidth, y, valueWidth, 16), StringAlignment.Far);

		int labelRight = Math.Min(RowRight - valueWidth, labelLimit);
		this.DrawText(g, label, labelFont, Grey, new Rectangle(RowLeft, y, Math.Max(0, labelRight - RowLeft), 16), StringAlignment.Near);
	}

	/// <summary>Draws the bar picture at <paramref name="y"/>, at its size in game pixels.</summary>
	public void Bar(Graphics g, Bitmap bar, int y) => g.DrawImage(bar, this.ScaledRect(new Rectangle(9, y, bar.Width, bar.Height)));
}
