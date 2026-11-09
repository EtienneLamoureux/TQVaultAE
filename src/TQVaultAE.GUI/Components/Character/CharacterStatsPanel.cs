using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TQVaultAE.Application;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>An attribute row of tab I: its label, the total shown, its colour and its tool tip.</summary>
internal sealed record CharacterAttributeView(string Label, string Value, Color Color, string ToolTip);

/// <summary>What the statistics panel shows, given by the dialog.</summary>
internal sealed record CharacterPanelView
{
	public string Name { get; init; } = string.Empty;
	public string ClassName { get; init; } = string.Empty;
	public string LevelText { get; init; } = string.Empty;
	public string XpText { get; init; } = string.Empty;
	public string AttributesTitle { get; init; } = string.Empty;

	/// <summary>The "N Points Available" line with its colour codes, empty when no point is left.</summary>
	public string PointsText { get; init; } = string.Empty;

	/// <summary>Health, Energy, Strength, Intelligence, Dexterity, in the game's order.</summary>
	public IReadOnlyList<CharacterAttributeView> Attributes { get; init; } = [];

	/// <summary>True when an attribute point can be spent: the game shows the "+" buttons then.</summary>
	public bool CanRaise { get; init; }

	/// <summary>True when attributes changed: the game shows its Undo button instead of the "Attributes" title.</summary>
	public bool CanUndo { get; init; }
	public string UndoText { get; init; } = string.Empty;

	public string LevelingTitle { get; init; } = string.Empty;

	/// <summary>The labels of the leveling rows, whose editors the dialog places with <see cref="CharacterStatsPanel.LevelingRow"/>.</summary>
	public IReadOnlyList<string> LevelingLabels { get; init; } = [];

	/// <summary>The values of the leveling rows whose editor is hidden (not editable now), written as the game writes numbers.</summary>
	public IReadOnlyList<string> LevelingValues { get; init; } = [];

	public string MasteriesTitle { get; init; } = string.Empty;
	public IReadOnlyList<(string Name, string Value)> Masteries { get; init; } = [];

	/// <summary>Tab III: the statistics lines of the save, label and value.</summary>
	public IReadOnlyList<(string Label, string Value)> Statistics { get; init; } = [];
	public string GreatestMonsterTitle { get; init; } = string.Empty;
	public string GreatestMonster { get; init; } = string.Empty;
	public string GreatestMonsterLevel { get; init; } = string.Empty;

	/// <summary>Shown where the game draws values it computes while playing (tab II, top of tab III).</summary>
	public string ComputedInGame { get; init; } = string.Empty;

	public IReadOnlyList<string> TabNames { get; init; } = ["I", "II", "III"];
	public string Money { get; init; } = string.Empty;
	public string Electrum { get; init; } = string.Empty;
}

/// <summary>
/// The statistics panel of the game's character window (tabs I, II and III, 251x390 at 394,30 of
/// <c>CharacterWindow01</c>) and the gold and Electrum bar under the character, drawn from the game textures at the
/// positions of the <c>CharStatsTab1..3</c> records. Tab I shows the name, class, level, experience and the attributes
/// with their "+" buttons (left click spends a point, right click takes it back, Shift all of them or back to the start),
/// and the editor's leveling rows where the game shows defense and offense, which only the game computes. Tab III shows
/// the statistics of the save. Sizes are in pixels of the game window times <see cref="PanelScale"/>.
/// </summary>
internal sealed partial class CharacterStatsPanel : Control
{
	private const string WindowTexture = """XPack2\UI\Character\CharacterWindow01.tex""";
	/// <summary>
	/// The panel in the window texture, with its whole gold border: the record's 251x390 box (at 394,30) stops short of
	/// the right border (x 644 to 649) and the bottom one (rows 418 to 423).
	/// </summary>
	private static readonly Rectangle PanelSource = new(394, 30, 256, 394);

	/// <summary>
	/// Rows of the panel in the window texture taken by its tab row (19), and the width of its three tabs (199): the
	/// marble of the window right of the tabs is not drawn, the page shows there.
	/// </summary>
	private const int TabRowHeight = 19;
	private const int TabRowWidth = 199;

	/// <summary>The gold bar under the character in the window texture; its pieces are cut out of it.</summary>
	private static readonly Rectangle GoldSource = new(100, 436, 296, 30);

	/// <summary>The gold pile left of the gold box, cut out of the window's marble by its colour.</summary>
	private static readonly Rectangle GoldIconSource = new(100, 437, 34, 26);

	/// <summary>A sample of the window's plain marble next to the gold pile, the colour taken out around the pile.</summary>
	private static readonly Rectangle MarbleSample = new(60, 437, 35, 26);

	/// <summary>The Electrum coin left of its box, cut out round.</summary>
	private static readonly Rectangle CoinSource = new(259, 438, 24, 24);

	/// <summary>The black number boxes with their gold outline (gold, then Electrum).</summary>
	private static readonly Rectangle GoldBoxSource = new(136, 440, 106, 22);
	private static readonly Rectangle ElectrumBoxSource = new(288, 440, 106, 22);

	/// <summary>Top of the gold bar, under the panel.</summary>
	private const int GoldTop = CharacterPanelLayout.Height + 6;

	private static readonly string[] TabTextures =
	[
		"""InGameUI\CharacterScreen\PrimaryStatsBtn01.tex""",
		"""InGameUI\CharacterScreen\SecondaryStatsBtn01.tex""",
		"""InGameUI\CharacterScreen\ThirdStatsBtn01.tex""",
	];
	private static readonly int[] TabX = [-1, 68, 137];
	private static readonly int[] TabTextX = [5, 74, 142];

	private const string StatsBar = """InGameUI\CharacterScreen\StatsBar01.tex""";
	private const string PlusUp = """InGameUI\CharacterScreen\StatIncrementButtonUp01.tex""";
	private const string PlusOver = """InGameUI\CharacterScreen\StatIncrementButtonOver01.tex""";
	private const string UndoUp = """XPack\UI\Character\ButtonAttributeUndo01Up.tex""";
	private const string UndoOver = """XPack\UI\Character\ButtonAttributeUndo01Over.tex""";

	/// <summary>The Undo button of the attributes (<c>tab1AttributeChangeUndoButton</c>, 59x19 at 15,69).</summary>
	private static readonly Rectangle UndoSource = new(15, 69, 59, 19);

	/// <summary>Top of each attribute row, Health first (<c>tab1LifeText</c>...).</summary>
	private static readonly int[] AttributeY = [89, 105, 121, 137, 153];

	private static readonly Color Grey = Color.FromArgb(200, 200, 190);
	private static readonly Color Yellow = Color.FromArgb(255, 210, 60);
	private static readonly Color Orange = Color.FromArgb(240, 160, 40);

	private readonly IUIService _uiService;
	private readonly ToolTip _toolTip = new() { AutoPopDelay = 30000, InitialDelay = 300, ReshowDelay = 100 };
	private float _panelScale = 1F;
	private int _tab;
	private int _hoveredPlus = -1;
	private bool _hoveredUndo;
	private string _toolTipText = string.Empty;

	public CharacterStatsPanel(IUIService uiService)
	{
		this._uiService = uiService;
		this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);

		// Transparent: the gold bar is wider than the panel, and the page shows beside the panel.
		this.BackColor = Color.Transparent;
		this.PanelScale = 1F;
	}

	/// <summary>The state shown, asked when painting.</summary>
	public Func<CharacterPanelView>? View { get; set; }

	/// <summary>Raised by a click on a "+" button: the attribute index (Health first), +1 or -1, and Shift.</summary>
	public event EventHandler<(int Attribute, int Step, bool ToEnd)>? AttributeClicked;

	/// <summary>Raised by a click on the Undo button: the attributes go back to their values at the start.</summary>
	public event EventHandler? UndoClicked;

	/// <summary>Raised when another tab (I, II, III) is shown.</summary>
	public event EventHandler? TabChanged;

	/// <summary>Controls shown on tab I only (the leveling editors).</summary>
	public List<Control> Tab1Controls { get; } = [];

	public Font? NameFont { get; set; }
	public Font? ClassFont { get; set; }
	public Font? TextFont { get; set; }
	public Font? TitleFont { get; set; }
	public Font? NumberFont { get; set; }
	public Font? TabFont { get; set; }

	/// <summary>The tab shown: 0 for I, 1 for II, 2 for III.</summary>
	public int Tab
	{
		get => this._tab;
		set
		{
			if (value is < 0 or > 2 || value == this._tab)
				return;

			this._tab = value;
			// An editor shows on tab I while it can be changed; else the panel writes its value.
			foreach (var control in this.Tab1Controls)
				control.Visible = value == 0 && control.Enabled;

			this.Invalidate();
			this.TabChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	/// <summary>The size of a game pixel on screen.</summary>
	public float PanelScale
	{
		get => this._panelScale;
		set
		{
			this._panelScale = value;
			// The gold bar is wider than the panel above it.
			this.Size = new Size(this.Scaled(Math.Max(CharacterPanelLayout.Width, GoldSource.Width)), this.Scaled(GoldTop + GoldSource.Height));
		}
	}

	/// <summary>
	/// The place of the editor of a leveling row on screen: right part of the row, where the game writes the numbers
	/// (x 150 to 243), rows every 20 pixels under the "Leveling" title.
	/// </summary>
	public Rectangle LevelingRow(int row)
		=> this.ScaledRect(new Rectangle(140, 204 + row * 20, 103, 18));

	/// <summary>The "+" button of an attribute on screen.</summary>
	public Rectangle PlusBounds(int attribute) => this.ScaledRect(new Rectangle(15, AttributeY[attribute] + 3, 12, 12));

	/// <summary>A tab (I, II, III) on screen.</summary>
	public Rectangle TabBounds(int tab) => this.ScaledRect(new Rectangle(TabX[tab] + 1, 0, 60, 22));

	private int Scaled(int value) => (int)Math.Round(value * this._panelScale);

	private Rectangle ScaledRect(Rectangle r) => new(this.Scaled(r.X), this.Scaled(r.Y), this.Scaled(r.Width), this.Scaled(r.Height));

	/// <summary>The gold pile and the Electrum coin cut out of the window texture, made once.</summary>
	private Bitmap? _goldIcon;
	private Bitmap? _coin;

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			this._toolTip.Dispose();
			this._goldIcon?.Dispose();
			this._coin?.Dispose();
		}

		base.Dispose(disposing);
	}

	/// <summary>The "+" button under a point, or -1; none while no point can be spent or off tab I.</summary>
	internal int HitPlus(Point location)
	{
		if (this._tab != 0 || this.View?.Invoke() is not { CanRaise: true } view)
			return -1;

		for (int i = 0; i < view.Attributes.Count && i < AttributeY.Length; i++)
			if (this.PlusBounds(i).Contains(location))
				return i;

		return -1;
	}

	internal void RaiseAttributeClick(int attribute, int step, bool toEnd)
		=> this.AttributeClicked?.Invoke(this, (attribute, step, toEnd));

	/// <summary>The Undo button on screen.</summary>
	public Rectangle UndoBounds => this.ScaledRect(UndoSource);

	/// <summary>True when the Undo button shows under a point.</summary>
	internal bool HitUndo(Point location)
		=> this._tab == 0 && this.View?.Invoke() is { CanUndo: true } && this.UndoBounds.Contains(location);

	internal void RaiseUndo() => this.UndoClicked?.Invoke(this, EventArgs.Empty);

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		for (int tab = 0; tab < 3; tab++)
			if (e.Button == MouseButtons.Left && this.TabBounds(tab).Contains(e.Location))
			{
				this.Tab = tab;
				return;
			}

		var step = e.Button switch
		{
			MouseButtons.Left => 1,
			MouseButtons.Right => -1,
			_ => 0,
		};
		if (step != 0 && this.HitPlus(e.Location) is var attribute and >= 0)
			this.RaiseAttributeClick(attribute, step, ModifierKeys.HasFlag(Keys.Shift));
		else if (e.Button == MouseButtons.Left && this.HitUndo(e.Location))
			this.RaiseUndo();
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		var plus = this.HitPlus(e.Location);
		var undo = this.HitUndo(e.Location);
		if (plus != this._hoveredPlus || undo != this._hoveredUndo)
		{
			this._hoveredPlus = plus;
			this._hoveredUndo = undo;
			this.Invalidate();
		}

		this.Cursor = plus >= 0 || undo || Enumerable.Range(0, 3).Any(t => this.TabBounds(t).Contains(e.Location)) ? Cursors.Hand : Cursors.Default;

		// The attribute rows tell their base value and bonuses.
		var text = string.Empty;
		if (this._tab == 0 && this.View?.Invoke() is { } view)
			for (int i = 0; i < view.Attributes.Count && i < AttributeY.Length; i++)
				if (this.ScaledRect(new Rectangle(15, AttributeY[i], 228, 16)).Contains(e.Location))
					text = view.Attributes[i].ToolTip;

		if (text != this._toolTipText)
		{
			this._toolTipText = text;
			this._toolTip.SetToolTip(this, text);
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		this._hoveredPlus = -1;
		this._hoveredUndo = false;
		this._toolTipText = string.Empty;
		this._toolTip.SetToolTip(this, string.Empty);
		this.Invalidate();
	}

	private Bitmap Picture(string resource) => this._uiService.LoadBitmap(resource.ToRecordId());

	protected override void OnPaint(PaintEventArgs e)
	{
		var g = e.Graphics;
		g.InterpolationMode = InterpolationMode.HighQualityBicubic;
		g.PixelOffsetMode = PixelOffsetMode.Half;
		g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

		var window = this.Picture(WindowTexture);
		// The panel: its three tabs, then its body under the tab row.
		g.DrawImage(window, this.ScaledRect(new Rectangle(0, 0, TabRowWidth, TabRowHeight)),
			new Rectangle(PanelSource.X, PanelSource.Y, TabRowWidth, TabRowHeight), GraphicsUnit.Pixel);
		g.DrawImage(window, this.ScaledRect(new Rectangle(0, TabRowHeight, PanelSource.Width, PanelSource.Height - TabRowHeight)),
			new Rectangle(PanelSource.X, PanelSource.Y + TabRowHeight, PanelSource.Width, PanelSource.Height - TabRowHeight), GraphicsUnit.Pixel);

		// The gold bar, piece by piece at their places in the window, without the window's marble between them.
		this._goldIcon ??= CutByColor(window, GoldIconSource, MarbleSample);
		this._coin ??= CutRound(window, CoinSource);
		g.DrawImage(this._goldIcon, this.GoldPiece(GoldIconSource));
		g.DrawImage(window, this.GoldPiece(GoldBoxSource), GoldBoxSource, GraphicsUnit.Pixel);
		g.DrawImage(this._coin, this.GoldPiece(CoinSource));
		g.DrawImage(window, this.GoldPiece(ElectrumBoxSource), ElectrumBoxSource, GraphicsUnit.Pixel);

		if (this.View?.Invoke() is not { } view)
			return;

		// The tab shown gets its lit button; the names are white on it, grey on the others.
		var tab = this.Picture(TabTextures[this._tab]);
		g.DrawImage(tab, this.ScaledRect(new Rectangle(TabX[this._tab], -1, tab.Width, tab.Height)));
		for (int i = 0; i < 3 && i < view.TabNames.Count; i++)
			this.DrawText(g, view.TabNames[i], this.TabFont, i == this._tab ? Color.White : Grey, new Rectangle(TabTextX[i], 4, 50, 14), StringAlignment.Center);

		switch (this._tab)
		{
			case 0:
				this.PaintTab1(g, view);
				break;
			case 1:
				this.DrawText(g, view.ComputedInGame, this.TextFont, Grey, new Rectangle(15, 150, 228, 60), StringAlignment.Center, wrap: true);
				break;
			default:
				this.PaintTab3(g, view);
				break;
		}

		// The gold bar: gold in yellow, Electrum in grey, centred in their boxes (characterGoldNumber...).
		this.DrawText(g, view.Money, this.NumberFont, Yellow, new Rectangle(38, GoldTop + 6, 102, 20), StringAlignment.Center);
		this.DrawText(g, view.Electrum, this.NumberFont, Grey, new Rectangle(190, GoldTop + 6, 102, 20), StringAlignment.Center);
	}

	/// <summary>Where a piece of the window's gold bar goes on screen: at its place in the bar, under the panel.</summary>
	private Rectangle GoldPiece(Rectangle source)
		=> this.ScaledRect(new Rectangle(source.X - GoldSource.X, GoldTop + source.Y - GoldSource.Y, source.Width, source.Height));

	/// <summary>
	/// A piece of a texture with the colour of a sample area taken out: close colours become transparent, a band of
	/// colours further away fades in, the rest stays (the gold pile keeps its shape, the marble around it goes).
	/// </summary>
	private static Bitmap CutByColor(Bitmap texture, Rectangle piece, Rectangle sample)
	{
		long r = 0, gr = 0, b = 0;
		for (int y = sample.Top; y < sample.Bottom; y++)
			for (int x = sample.Left; x < sample.Right; x++)
			{
				var c = texture.GetPixel(x, y);
				r += c.R;
				gr += c.G;
				b += c.B;
			}

		var count = sample.Width * sample.Height;
		var reference = (R: r / (double)count, G: gr / (double)count, B: b / (double)count);
		var result = new Bitmap(piece.Width, piece.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		for (int y = 0; y < piece.Height; y++)
			for (int x = 0; x < piece.Width; x++)
			{
				var c = texture.GetPixel(piece.X + x, piece.Y + y);
				var distance = Math.Sqrt(Math.Pow(c.R - reference.R, 2) + Math.Pow(c.G - reference.G, 2) + Math.Pow(c.B - reference.B, 2));
				var alpha = (int)Math.Clamp((distance - 38) / 32 * 255, 0, 255);
				result.SetPixel(x, y, Color.FromArgb(alpha * c.A / 255, c));
			}

		return result;
	}

	/// <summary>A round piece of a texture: the disc inscribed in the area, transparent outside it.</summary>
	private static Bitmap CutRound(Bitmap texture, Rectangle piece)
	{
		var result = new Bitmap(piece.Width, piece.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		using var g = Graphics.FromImage(result);
		using var disc = new GraphicsPath();
		disc.AddEllipse(1, 1, piece.Width - 2, piece.Height - 2);
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.SetClip(disc);
		g.DrawImage(texture, new Rectangle(0, 0, piece.Width, piece.Height), piece, GraphicsUnit.Pixel);
		return result;
	}

	private void PaintTab1(Graphics g, CharacterPanelView view)
	{
		this.DrawText(g, view.Name, this.NameFont, Color.White, new Rectangle(14, 24, 167, 20), StringAlignment.Near);
		this.DrawText(g, view.ClassName, this.ClassFont, Orange, new Rectangle(14, 45, 130, 18), StringAlignment.Near);
		this.DrawText(g, view.LevelText, this.TextFont, Grey, new Rectangle(143, 28, 100, 16), StringAlignment.Far);
		this.DrawText(g, view.XpText, this.TextFont, Grey, new Rectangle(105, 47, 138, 16), StringAlignment.Far);

		if (view.CanUndo)
		{
			g.DrawImage(this.Picture(this._hoveredUndo ? UndoOver : UndoUp), this.UndoBounds);
			using var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
			g.DrawString(view.UndoText, this.TabFont ?? this.Font, Brushes.White, this.UndoBounds, center);
		}
		else
			this.DrawText(g, view.AttributesTitle, this.TitleFont, Color.White, new Rectangle(15, 67, 120, 18), StringAlignment.Near);
		this.ColoredText(g, view.PointsText, this.TextFont, Grey, new Rectangle(102, 68, 141, 16));

		for (int i = 0; i < view.Attributes.Count && i < AttributeY.Length; i++)
		{
			var row = view.Attributes[i];
			if (view.CanRaise)
				g.DrawImage(this.Picture(i == this._hoveredPlus ? PlusOver : PlusUp), this.PlusBounds(i));

			this.DrawText(g, row.Label, this.TextFont, Grey, new Rectangle(33, AttributeY[i], 120, 16), StringAlignment.Near);
			this.DrawText(g, row.Value, this.NumberFont, row.Color, new Rectangle(150, AttributeY[i], 93, 16), StringAlignment.Far);
		}

		this.Bar(g, 178);
		this.DrawText(g, view.LevelingTitle, this.TitleFont, Color.White, new Rectangle(15, 185, 228, 18), StringAlignment.Near);
		for (int i = 0; i < view.LevelingLabels.Count; i++)
		{
			this.DrawText(g, view.LevelingLabels[i], this.TextFont, Grey, new Rectangle(15, 205 + i * 20, 125, 16), StringAlignment.Near);
			if (i < view.LevelingValues.Count)
				this.DrawText(g, view.LevelingValues[i], this.NumberFont, Color.White, new Rectangle(140, 205 + i * 20, 103, 16), StringAlignment.Far);
		}

		this.Bar(g, 312);
		this.DrawText(g, view.MasteriesTitle, this.TitleFont, Color.White, new Rectangle(15, 319, 228, 18), StringAlignment.Near);
		for (int i = 0; i < view.Masteries.Count; i++)
		{
			this.Row(g, view.Masteries[i].Name, view.Masteries[i].Value, 340 + i * 18, this.TextFont);
		}
	}

	private void PaintTab3(Graphics g, CharacterPanelView view)
	{
		this.DrawText(g, view.ComputedInGame, this.TextFont, Grey, new Rectangle(15, 110, 228, 60), StringAlignment.Center, wrap: true);
		this.Bar(g, 268);
		// The monster's name keeps its colour codes (a hero's name is coloured); its level goes on the title row.
		this.Row(g, view.GreatestMonsterTitle, view.GreatestMonsterLevel, 277);
		this.ColoredText(g, view.GreatestMonster, this.NumberFont, Color.White, new Rectangle(16, 294, 227, 16), alignRight: false);

		// Monsters killed, greatest damage, elapsed time, deaths (tab3MonstersKilledText...).
		int[] rows = [315, 333, 351, 369];
		for (int i = 0; i < view.Statistics.Count && i < rows.Length; i++)
			this.Row(g, view.Statistics[i].Label, view.Statistics[i].Value, rows[i]);
	}

	/// <summary>A grey label and a white number right-aligned on the same row; the label gives way to the number.</summary>
	private void Row(Graphics g, string label, string value, int y, Font? valueFont = null)
	{
		var number = valueFont ?? this.NumberFont ?? this.Font;
		var valueWidth = string.IsNullOrEmpty(value) ? 0 : (int)Math.Ceiling(g.MeasureString(value, number).Width / this._panelScale) + 4;
		this.DrawText(g, value, number, Color.White, new Rectangle(243 - valueWidth, y, valueWidth, 16), StringAlignment.Far);
		this.DrawText(g, label, this.TextFont, Grey, new Rectangle(15, y, 228 - valueWidth, 16), StringAlignment.Near);
	}

	private void Bar(Graphics g, int y)
	{
		var bar = this.Picture(StatsBar);
		g.DrawImage(bar, this.ScaledRect(new Rectangle(9, y, bar.Width, bar.Height)));
	}

	private void DrawText(Graphics g, string text, Font? font, Color color, Rectangle box, StringAlignment alignment, bool wrap = false)
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
		g.DrawString(text, font ?? this.Font, brush, this.ScaledRect(box), format);
	}

	/// <summary>A text with game colour codes (<c>{^y}</c> yellow, <c>{^w}</c> white), right-aligned by default.</summary>
	private void ColoredText(Graphics g, string text, Font? font, Color color, Rectangle box, bool alignRight = true)
	{
		var runs = CharacterPanelLayout.ColorRuns(text);
		if (runs.Count == 0)
			return;

		font ??= this.Font;
		using var format = (StringFormat)StringFormat.GenericTypographic.Clone();
		format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
		var widths = runs.Select(r => g.MeasureString(r.Text, font, PointF.Empty, format).Width).ToList();
		var target = this.ScaledRect(box);
		var x = alignRight ? target.Right - widths.Sum() : target.X;
		var state = g.Save();
		g.SetClip(target);
		for (int i = 0; i < runs.Count; i++)
		{
			using var brush = new SolidBrush(runs[i].Color switch
			{
				'y' => Yellow,
				'w' => Color.White,
				'g' => Color.LimeGreen,
				'r' => Color.Red,
				_ => color,
			});
			g.DrawString(runs[i].Text, font, brush, x, target.Y, format);
			x += widths[i];
		}

		g.Restore(state);
	}
}
