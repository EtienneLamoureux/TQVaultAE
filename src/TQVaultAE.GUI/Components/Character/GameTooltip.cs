using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>A line of a game tool tip: its text (game colour tags such as <c>{^y}</c> or <c>^a</c> allowed), colour and style.</summary>
/// <param name="Text">The text; an empty text is a blank line.</param>
/// <param name="Color">The colour of the parts with no colour tag.</param>
/// <param name="Title">True for the title line, in the bigger bold font.</param>
/// <param name="Wrap">
/// True for a text that wraps at the tool tip's width (a description, a sentence); the other lines are never cut: the
/// box widens for them, as the game's does.
/// </param>
internal sealed record GameTooltipLine(string Text, Color Color, bool Title = false, bool Wrap = false)
{
	/// <summary>A blank line between two parts.</summary>
	public static GameTooltipLine Blank { get; } = new(string.Empty, Color.Empty);
}

/// <summary>
/// The eight pieces of the game's rollover border (<c>InGameUI\borderrollover*01.tex</c>): the corners, the top and
/// bottom edges and the sides, stretched along the box.
/// </summary>
internal sealed record GameTooltipFrame(Bitmap TopLeft, Bitmap Top, Bitmap TopRight, Bitmap Left, Bitmap Right, Bitmap BottomLeft, Bitmap Bottom, Bitmap BottomRight)
{
	private static readonly string[] Pieces = ["lt", "ct", "rt", "lc", "rc", "lb", "bc", "rb"];

	/// <summary>Loads the pieces from the game files; <see langword="null"/> when one is missing.</summary>
	/// <param name="load">Loads a texture by its resource id, <see langword="null"/> when missing.</param>
	public static GameTooltipFrame? Load(Func<string, Bitmap?> load)
	{
		var pieces = Pieces.Select(p => load($"""InGameUI\borderrollover{p}01.tex""")).ToArray();
		return pieces.Any(p => p is null)
			? null
			: new GameTooltipFrame(pieces[0]!, pieces[1]!, pieces[2]!, pieces[3]!, pieces[4]!, pieces[5]!, pieces[6]!, pieces[7]!);
	}

	/// <summary>Loads the pieces through the UI service; <see langword="null"/> when the game files lack one.</summary>
	public static GameTooltipFrame? Load(TQVaultAE.Presentation.Win32.Contracts.IUIService uiService)
		=> Load(id => uiService.LoadBitmap(RecordId.Create(id)) is var piece && ReferenceEquals(piece, uiService.DefaultBitmap) ? null : piece);

	/// <summary>The width of the border on each side, at <paramref name="scale"/>.</summary>
	public Padding Thickness(float scale)
		=> new(Scaled(this.TopLeft.Width, scale), Scaled(this.TopLeft.Height, scale), Scaled(this.BottomRight.Width, scale), Scaled(this.BottomRight.Height, scale));

	private static int Scaled(int pixels, float scale) => Math.Max(1, (int)Math.Round(pixels * scale));

	/// <summary>Draws the border inside <paramref name="bounds"/>: the corners at their size, the edges stretched between them.</summary>
	public void Draw(Graphics g, Rectangle bounds, float scale)
	{
		var state = g.Save();
		g.InterpolationMode = InterpolationMode.NearestNeighbor;
		g.PixelOffsetMode = PixelOffsetMode.Half;
		var t = this.Thickness(scale);
		int left = t.Left, top = t.Top, right = t.Right, bottom = t.Bottom;
		int innerWidth = Math.Max(0, bounds.Width - left - right), innerHeight = Math.Max(0, bounds.Height - top - bottom);

		g.DrawImage(this.Top, new Rectangle(bounds.X + left, bounds.Y, innerWidth, top));
		g.DrawImage(this.Bottom, new Rectangle(bounds.X + left, bounds.Bottom - bottom, innerWidth, bottom));
		g.DrawImage(this.Left, new Rectangle(bounds.X, bounds.Y + top, left, innerHeight));
		g.DrawImage(this.Right, new Rectangle(bounds.Right - right, bounds.Y + top, right, innerHeight));
		g.DrawImage(this.TopLeft, new Rectangle(bounds.X, bounds.Y, left, top));
		g.DrawImage(this.TopRight, new Rectangle(bounds.Right - right, bounds.Y, right, top));
		g.DrawImage(this.BottomLeft, new Rectangle(bounds.X, bounds.Bottom - bottom, left, bottom));
		g.DrawImage(this.BottomRight, new Rectangle(bounds.Right - right, bounds.Bottom - bottom, right, bottom));
		g.Restore(state);
	}
}

/// <summary>
/// A tool tip drawn as the game's rollovers: a black box with a gold border and coloured lines (the skill window's skill
/// tool tip: name, description, levels and their effects, requirements in red). It opens under the pointer and stays
/// while the pointer rests on what it describes, unlike a standard tool tip which closes after a while.
/// </summary>
internal sealed partial class GameTooltip : IDisposable
{
	// Read on the game's skill window: a light gold line with a dark gold one inside, on a very dark brown (the game's box
	// is a little translucent; this one is opaque, with the colour the box takes over the marble). The two lines are only
	// drawn when the game's border pieces are missing.
	private static readonly Color Background = Color.FromArgb(28, 26, 20);
	private static readonly Color OuterBorder = Color.FromArgb(205, 185, 140);
	private static readonly Color InnerBorder = Color.FromArgb(80, 66, 28);

	// The aqua of the recharge line as the skill window shows it, softer than the item tool tips' pure cyan.
	private static readonly Color SkillWindowAqua = Color.FromArgb(170, 235, 215);

	private const TextFormatFlags Flags = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

	private readonly ToolTip _toolTip = new() { OwnerDraw = true, UseAnimation = false, UseFading = false };
	private readonly Font _titleFont;
	private readonly Font _textFont;
	private readonly int _padding;
	private readonly int _maxWidth;
	private readonly float _scale;
	private readonly GameTooltipFrame? _frame;
	private IReadOnlyList<GameTooltipLine> _lines = [];
	private Control? _shownOn;

	/// <param name="frame">The game's rollover border pieces; without them the border is two plain lines.</param>
	public GameTooltip(Font titleFont, Font textFont, float scale, GameTooltipFrame? frame = null)
	{
		this._scale = scale;
		this._frame = frame;
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

#if DEBUG
		// Automation rests a virtual cursor on the control instead of moving the real one.
		var position = TQVaultAE.GUI.Automation.VirtualCursor.IsActive
			? TQVaultAE.GUI.Automation.VirtualCursor.GetClientPosition(control)
			: control.PointToClient(Cursor.Position);
#else
		var position = control.PointToClient(Cursor.Position);
#endif
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
		if (this._frame is { } frame)
		{
			frame.Draw(g, bounds, this._scale);
		}
		else
		{
			using var outer = new Pen(OuterBorder);
			g.DrawRectangle(outer, new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1));
			using var inner = new Pen(InnerBorder);
			g.DrawRectangle(inner, new Rectangle(bounds.X + 1, bounds.Y + 1, bounds.Width - 3, bounds.Height - 3));
		}

		this.Layout(g);
	}

	/// <summary>
	/// Measures the lines, and draws them when <paramref name="g"/> is given; the wrapping lines break at word ends, the
	/// others widen the box.
	/// </summary>
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

			foreach (var row in Wrap(Runs(line.Text, line.Color), font, line.Wrap ? this._maxWidth : int.MaxValue))
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

	/// <summary>Splits coloured runs into rows no wider than <paramref name="maxWidth"/>, breaking at spaces.</summary>
	private static List<List<(string Text, Color Color)>> Wrap(IEnumerable<(string Text, Color Color)> runs, Font font, int maxWidth)
	{
		var rows = new List<List<(string, Color)>> { new() };
		int x = 0;
		bool pendingSpace = false;
		foreach (var (text, color) in runs)
		{
			foreach (var match in WordRegex().Matches(text).Select(m => m.Value))
			{
				// A space before a colour change ("enemies. {^y}Assign") belongs to the next word.
				var word = pendingSpace && !char.IsWhiteSpace(match[0]) ? " " + match : match;
				pendingSpace = false;
				var width = TextRenderer.MeasureText(word, font, Size.Empty, Flags).Width;
				if (x + width > maxWidth && x > 0 && word.Trim().Length != 0)
				{
					rows.Add([]);
					x = 0;
				}

				var piece = x == 0 ? word.TrimStart() : word;
				rows[^1].Add((piece, color));
				x += TextRenderer.MeasureText(piece, font, Size.Empty, Flags).Width;
			}

			pendingSpace = text.Length != 0 && char.IsWhiteSpace(text[^1]);
		}

		return rows;
	}

	/// <summary>Splits a game text at its colour tags (<c>{^x}</c> and <c>^x</c>), each part with its colour.</summary>
	internal static IEnumerable<(string Text, Color Color)> Runs(string text, Color color)
		=> text.SplitColorRuns(bareTags: true).Select(run => (run.Text, run.Code switch
		{
			null => color,
			'A' => SkillWindowAqua,
			{ } code => TQColorHelper.GetColorFromTagIdentifier(code).Color(),
		}));

	[GeneratedRegex("""\s*\S+""")]
	private static partial Regex WordRegex();
}
