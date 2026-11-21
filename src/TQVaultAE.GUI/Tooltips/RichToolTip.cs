namespace TQVaultAE.GUI.Tooltips;

/// <summary>
/// Tool tip drawn as a card: a coloured title, a summary, then sections of lines marked as allowed (✔) or restricted (✖).
/// </summary>
/// <remarks>Used by the Forge to spell out what each mode allows.</remarks>
internal sealed partial class RichToolTip : IDisposable
{
	/// <summary>One line of a section: something allowed, or a restriction.</summary>
	public sealed record Line(string Text, bool Allowed);

	public sealed record Section(string Title, IReadOnlyList<Line> Lines);

	public sealed record Card(string Title, Color TitleColor, string Summary, IReadOnlyList<Section> Sections, string? Footer = null);

	private static readonly Color Background = Color.FromArgb(24, 19, 13);
	private static readonly Color Border = Color.FromArgb(140, 112, 60);
	private static readonly Color SummaryColor = Color.FromArgb(205, 198, 180);
	private static readonly Color SectionColor = Color.FromArgb(230, 190, 90);
	private static readonly Color TextColor = Color.FromArgb(235, 235, 235);
	private static readonly Color AllowedColor = Color.FromArgb(110, 200, 90);
	private static readonly Color RestrictedColor = Color.FromArgb(230, 140, 60);
	private const string AllowedMark = "✔";
	private const string RestrictedMark = "✖";

	private const TextFormatFlags Flags = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;

	/// <summary>Delay before the card opens, like a standard tool tip.</summary>
	private const int InitialDelay = 400;

	private readonly ToolTip _toolTip = new() { OwnerDraw = true };
	private readonly Dictionary<Control, Card> _cards = [];

	/// <summary>Opens the card once the pointer has rested on <see cref="_pending"/>.</summary>
	private readonly System.Windows.Forms.Timer _delay = new() { Interval = InitialDelay };
	private Control? _pending;
	private readonly Font _titleFont;
	private readonly Font _textFont;
	private readonly Font _sectionFont;
	private readonly int _padding;
	private readonly int _maxTextWidth;
	private readonly int _markWidth;
	private readonly int _gap;

	public RichToolTip(Font titleFont, Font textFont, Font sectionFont, float scale)
	{
		this._titleFont = titleFont;
		this._textFont = textFont;
		this._sectionFont = sectionFont;
		this._padding = (int)(10 * scale);
		this._gap = (int)(6 * scale);
		this._maxTextWidth = (int)(380 * scale);
		this._markWidth = TextRenderer.MeasureText(AllowedMark, textFont).Width + (int)(4 * scale);

		this._toolTip.Popup += this.OnPopup;
		this._toolTip.Draw += this.OnDraw;
		this._delay.Tick += this.OnDelayElapsed;
	}

	/// <summary>
	/// Shows <paramref name="card"/> when the mouse rests on <paramref name="control"/>, for as long as it stays there.
	/// </summary>
	/// <remarks>
	/// Opened and closed by hand: an automatic tool tip closes after at most about 32 seconds (<see cref="ToolTip.AutoPopDelay"/>).
	/// </remarks>
	public void Set(Control control, Card card)
	{
		if (this._cards.TryAdd(control, card))
		{
			control.MouseEnter += this.OnMouseEnter;
			control.MouseLeave += this.OnMouseLeave;
		}
		else
			this._cards[control] = card;
	}

	private void OnMouseEnter(object? sender, EventArgs e)
	{
		this._pending = sender as Control;
		this._delay.Stop();
		this._delay.Start();
	}

	private void OnMouseLeave(object? sender, EventArgs e)
	{
		this._delay.Stop();
		this._pending = null;
		if (sender is Control control)
			this._toolTip.Hide(control);
	}

	private void OnDelayElapsed(object? sender, EventArgs e)
	{
		this._delay.Stop();
		if (this._pending is not { IsDisposed: false } control || !this._cards.TryGetValue(control, out var card))
			return;

		// Below the pointer, like a standard tool tip; with no duration it stays until the pointer leaves.
		var position = control.PointToClient(Cursor.Position);
		int below = (Cursor.Current?.Size.Height ?? 32) / 2 + 4;
		this._toolTip.Show(card.Title, control, new Point(position.X, position.Y + below));
	}

	private void OnPopup(object? sender, PopupEventArgs e)
	{
		if (e.AssociatedControl is not null && this._cards.TryGetValue(e.AssociatedControl, out var card))
			e.ToolTipSize = this.Layout(card, null).Size;
	}

	private void OnDraw(object? sender, DrawToolTipEventArgs e)
	{
		if (e.AssociatedControl is null || !this._cards.TryGetValue(e.AssociatedControl, out var card))
		{
			e.DrawBackground();
			e.DrawText();
			return;
		}

		using (var back = new SolidBrush(Background))
			e.Graphics.FillRectangle(back, e.Bounds);
		using (var border = new Pen(Border))
			e.Graphics.DrawRectangle(border, new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1));

		this.Layout(card, e.Graphics);
	}

	/// <summary>Measures the card, and draws it when <paramref name="g"/> is given.</summary>
	private Rectangle Layout(Card card, Graphics? g)
	{
		int x = this._padding;
		int y = this._padding;
		int width = 0;

		void Text(string text, Font font, Color color, int left)
		{
			var size = TextRenderer.MeasureText(text, font, new Size(this._maxTextWidth - (left - this._padding), int.MaxValue), Flags);
			if (g is not null)
				TextRenderer.DrawText(g, text, font, new Rectangle(left, y, size.Width, size.Height), color, Flags);
			width = Math.Max(width, left - this._padding + size.Width);
			y += size.Height;
		}

		Text(card.Title, this._titleFont, card.TitleColor, x);
		y += this._gap / 2;
		Text(card.Summary, this._textFont, SummaryColor, x);

		foreach (var section in card.Sections)
		{
			y += this._gap;
			Text(section.Title, this._sectionFont, SectionColor, x);
			foreach (var line in section.Lines)
			{
				if (g is not null)
					TextRenderer.DrawText(g, line.Allowed ? AllowedMark : RestrictedMark, this._textFont, new Point(x, y),
						line.Allowed ? AllowedColor : RestrictedColor, TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
				Text(line.Text, this._textFont, TextColor, x + this._markWidth);
			}
		}

		if (!string.IsNullOrEmpty(card.Footer))
		{
			y += this._gap;
			Text(card.Footer, this._textFont, SummaryColor, x);
		}

		return new Rectangle(0, 0, width + 2 * this._padding, y + this._padding);
	}

	public void Dispose()
	{
		this._delay.Dispose();
		foreach (var control in this._cards.Keys)
		{
			control.MouseEnter -= this.OnMouseEnter;
			control.MouseLeave -= this.OnMouseLeave;
		}
		this._toolTip.Dispose();
	}
}
