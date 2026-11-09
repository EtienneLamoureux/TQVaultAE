using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>A line of a game tool tip: its text (game colour tags such as <c>{^y}</c> or <c>^a</c> allowed), colour and style.</summary>
/// <param name="Text">The text; an empty text is a blank line.</param>
/// <param name="Color">The colour of the parts with no colour tag.</param>
/// <param name="Title">True for the title line, in the bigger bold font.</param>
internal sealed record GameTooltipLine(string Text, Color Color, bool Title = false)
{
	/// <summary>A blank line between two parts.</summary>
	public static GameTooltipLine Blank { get; } = new(string.Empty, Color.Empty);
}

/// <summary>
/// A tool tip drawn as the game's rollovers: a black box with a gold border and coloured lines (the skill window's skill
/// tool tip: name, description, levels and their effects, requirements in red). It opens under the pointer and stays
/// while the pointer rests on what it describes, unlike a standard tool tip which closes after a while.
/// </summary>
internal sealed partial class GameTooltip : IDisposable
{
	private static readonly Color Background = Color.FromArgb(8, 8, 6);
	private static readonly Color OuterBorder = Color.FromArgb(196, 160, 82);
	private static readonly Color InnerBorder = Color.FromArgb(92, 74, 38);

	private const TextFormatFlags Flags = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

	private readonly ToolTip _toolTip = new() { OwnerDraw = true, UseAnimation = false, UseFading = false };
	private readonly Font _titleFont;
	private readonly Font _textFont;
	private readonly int _padding;
	private readonly int _maxWidth;
	private IReadOnlyList<GameTooltipLine> _lines = [];
	private Control? _shownOn;

	public GameTooltip(Font titleFont, Font textFont, float scale)
	{
		this._titleFont = titleFont;
		this._textFont = textFont;
		this._padding = (int)Math.Round(8 * scale);
		this._maxWidth = (int)Math.Round(360 * scale);
		this._toolTip.Popup += this.OnPopup;
		this._toolTip.Draw += this.OnDraw;
	}

	/// <summary>True while the tool tip is shown.</summary>
	public bool Visible => this._shownOn is not null;

	/// <summary>The lines shown, for the automation endpoint.</summary>
	public IReadOnlyList<GameTooltipLine> Lines => this._lines;

	/// <summary>Shows <paramref name="lines"/> under the pointer over <paramref name="control"/>, until <see cref="Hide"/>.</summary>
	public void Show(Control control, IReadOnlyList<GameTooltipLine> lines)
	{
		this._lines = lines;
		if (lines.Count == 0)
		{
			this.Hide();
			return;
		}

		var position = control.PointToClient(Cursor.Position);
		int below = (Cursor.Current?.Size.Height ?? 32) / 2 + 4;
		this._shownOn = control;

		// The text is not drawn as is: the size comes from Popup and the drawing from Draw.
		this._toolTip.Show(" ", control, new Point(position.X + 12, position.Y + below));
	}

	/// <summary>Closes the tool tip.</summary>
	public void Hide()
	{
		if (this._shownOn is { IsDisposed: false } control)
			this._toolTip.Hide(control);

		this._shownOn = null;
	}

	public void Dispose()
	{
		this._toolTip.Dispose();
	}

	private void OnPopup(object? sender, PopupEventArgs e)
		=> e.ToolTipSize = this.Layout(null);

	private void OnDraw(object? sender, DrawToolTipEventArgs e)
		=> this.Paint(e.Graphics, e.Bounds);

	/// <summary>The tool tip of <paramref name="lines"/> as a picture, as it is drawn on screen.</summary>
	internal Bitmap Render(IReadOnlyList<GameTooltipLine> lines)
	{
		var shown = this._lines;
		this._lines = lines;
		try
		{
			var size = this.Layout(null);
			var bitmap = new Bitmap(size.Width, size.Height);
			using var g = Graphics.FromImage(bitmap);
			this.Paint(g, new Rectangle(Point.Empty, size));
			return bitmap;
		}
		finally
		{
			this._lines = shown;
		}
	}

	private void Paint(Graphics g, Rectangle bounds)
	{
		using (var back = new SolidBrush(Background))
			g.FillRectangle(back, bounds);
		using (var outer = new Pen(OuterBorder))
			g.DrawRectangle(outer, new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1));
		using (var inner = new Pen(InnerBorder))
			g.DrawRectangle(inner, new Rectangle(bounds.X + 2, bounds.Y + 2, bounds.Width - 5, bounds.Height - 5));

		this.Layout(g);
	}

	/// <summary>Measures the lines, and draws them when <paramref name="g"/> is given; long lines wrap at word ends.</summary>
	private Size Layout(Graphics? g)
	{
		int y = this._padding;
		int width = 0;
		foreach (var line in this._lines)
		{
			var font = line.Title ? this._titleFont : this._textFont;
			if (line.Text.Length == 0)
			{
				y += TextRenderer.MeasureText("X", font, Size.Empty, Flags).Height / 2;
				continue;
			}

			foreach (var row in this.Wrap(Runs(line.Text, line.Color), font))
			{
				int x = this._padding;
				int height = 0;
				foreach (var (text, color) in row)
				{
					var size = TextRenderer.MeasureText(text, font, Size.Empty, Flags);
					if (g is not null)
						TextRenderer.DrawText(g, text, font, new Point(x, y), color, Flags);
					x += size.Width;
					height = Math.Max(height, size.Height);
				}

				width = Math.Max(width, x - this._padding);
				y += height;
			}
		}

		return new Size(width + 2 * this._padding, y + this._padding);
	}

	/// <summary>Splits coloured runs into rows no wider than the tool tip, breaking at spaces.</summary>
	private List<List<(string Text, Color Color)>> Wrap(IEnumerable<(string Text, Color Color)> runs, Font font)
	{
		var rows = new List<List<(string, Color)>> { new() };
		int x = 0;
		foreach (var (text, color) in runs)
		{
			foreach (var word in WordRegex().Matches(text).Select(m => m.Value))
			{
				var width = TextRenderer.MeasureText(word, font, Size.Empty, Flags).Width;
				if (x + width > this._maxWidth && x > 0 && word.Trim().Length != 0)
				{
					rows.Add([]);
					x = 0;
				}

				var piece = x == 0 ? word.TrimStart() : word;
				rows[^1].Add((piece, color));
				x += TextRenderer.MeasureText(piece, font, Size.Empty, Flags).Width;
			}
		}

		return rows;
	}

	/// <summary>Splits a game text at its colour tags (<c>{^x}</c> and <c>^x</c>), each part with its colour.</summary>
	internal static IEnumerable<(string Text, Color Color)> Runs(string text, Color color)
	{
		int start = 0;
		var current = color;
		foreach (Match tag in ColorTagRegex().Matches(text))
		{
			if (tag.Index > start)
				yield return (text[start..tag.Index], current);

			var code = char.ToUpperInvariant((tag.Groups["a"].Success ? tag.Groups["a"] : tag.Groups["b"]).Value[0]);
			current = TQColorHelper.GetColorFromTagIdentifier(code).Color();
			start = tag.Index + tag.Length;
		}

		if (start < text.Length)
			yield return (text[start..], current);
	}

	[GeneratedRegex("""\{\^(?<a>[A-Za-z])\}|\^(?<b>[A-Za-z])""")]
	private static partial Regex ColorTagRegex();

	[GeneratedRegex("""\s*\S+""")]
	private static partial Regex WordRegex();
}
