using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TQVaultAE.Application;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>An attribute row of tab I: its label, the total shown, its colour and its tool tip.</summary>
internal sealed record CharacterAttributeView(string Label, string Value, Color Color, string ToolTip);

/// <summary>An icon of the panel (a resistance, a damage kind, a monster race) with the number drawn on it and its tool tip.</summary>
internal sealed record StatIconView(string Icon, string Value, string ToolTip);

/// <summary>A row of the panel: grey label, white value, tool tip.</summary>
internal sealed record StatRowView(string Label, string Value, string ToolTip);

/// <summary>The values TQVault computes like the game (tab I defense, tab II, top of tab III), ready to draw.</summary>
internal sealed record CharacterStatsView
{
	public string DefenseTitle { get; init; } = string.Empty;
	public StatRowView Armor { get; init; } = new(string.Empty, string.Empty, string.Empty);

	/// <summary>Tab I fire, cold, lightning, pierce, vitality, poison, bleeding, physical; tab II slow, trap, energy, disruption, stun, freeze, sleep, petrify.</summary>
	public IReadOnlyList<StatIconView> Resistances { get; init; } = [];

	public string OffenseTitle { get; init; } = string.Empty;

	/// <summary>Average damage, attack speed, DPS: only the game computes them.</summary>
	public IReadOnlyList<StatRowView> Offense { get; init; } = [];

	public string SecondaryTitle { get; init; } = string.Empty;

	/// <summary>Health and energy regeneration, energy cost and recharge reduction, cast and run speed, offensive and defensive ability, dodge, deflect, block.</summary>
	public IReadOnlyList<StatRowView> Tab2Rows { get; init; } = [];

	public string DamageTitle { get; init; } = string.Empty;

	/// <summary>Physical, pierce, bleeding, poison, pets, fire, cold, lightning, vitality, life leech: icons with their bonuses as tool tips.</summary>
	public IReadOnlyList<StatIconView> Damages { get; init; } = [];

	/// <summary>Weapon damage as health, weapon base as elemental.</summary>
	public IReadOnlyList<StatRowView> Tab3Rows { get; init; } = [];

	public string RaceTitle { get; init; } = string.Empty;

	/// <summary>The 12 monster races, icons with their bonuses as tool tips.</summary>
	public IReadOnlyList<StatIconView> Races { get; init; } = [];
}

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

	/// <summary>The values computed like the game.</summary>
	public CharacterStatsView Stats { get; init; } = new();

	/// <summary>Tab III: the statistics lines of the save, label and value.</summary>
	public IReadOnlyList<(string Label, string Value)> Statistics { get; init; } = [];
	public string GreatestMonsterTitle { get; init; } = string.Empty;
	public string GreatestMonster { get; init; } = string.Empty;
	public string GreatestMonsterLevel { get; init; } = string.Empty;

	public IReadOnlyList<string> TabNames { get; init; } = ["I", "II", "III"];
	public string Money { get; init; } = string.Empty;
	public string Electrum { get; init; } = string.Empty;
}

/// <summary>
/// The statistics panel of the game's character window (tabs I, II and III, <see cref="Images.CharacterStatsPanel"/>
/// cut from <c>CharacterWindow01</c>) and the gold and Electrum bar under the character (its pieces cut from the same
/// window), at the positions of the <c>CharStatsTab1..3</c> records. Tab I shows the name, class, level, experience and the attributes
/// with their "+" buttons (left click spends a point, right click takes it back, Shift all of them or back to the start),
/// the combined armor and the resistances; tab II the secondary resistances and the other values; tab III the damage
/// and race bonuses (icons with tool tips, as the game's rollovers) and the statistics of the save. Damage, attack speed
/// and DPS stay computed in game. Sizes are in pixels of the game window times <see cref="PanelScale"/>.
/// </summary>
internal sealed partial class CharacterStatsPanel : Control
{
	/// <summary>
	/// The panel with its whole gold border (256x394; the record's 251x390 box stops short of the right and bottom
	/// border), transparent right of its three tabs.
	/// </summary>
	private readonly Bitmap _panel = Images.CharacterStatsPanel;

	/// <summary>The gold bar's pieces: the gold pile, the Electrum coin and the black number box with its gold outline.</summary>
	private readonly Bitmap _goldPile = Images.CharacterGoldPile;
	private readonly Bitmap _coin = Images.CharacterElectrumCoin;
	private readonly Bitmap _numberBox = Images.CharacterNumberBox;

	/// <summary>Width of the gold bar, which is wider than the panel above it.</summary>
	private const int GoldBarWidth = 296;

	/// <summary>Height of the gold bar.</summary>
	private const int GoldBarHeight = 30;

	/// <summary>Places of the gold bar's pieces, from its top-left, as in the character window.</summary>
	private static readonly Point GoldPileAt = new(0, 1);
	private static readonly Point GoldBoxAt = new(36, 4);
	private static readonly Point CoinAt = new(159, 2);
	private static readonly Point ElectrumBoxAt = new(188, 4);

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

	/// <summary>Resistance icons: left of each column, top of each row of tab I (fire...) then of tab II (slow...).</summary>
	private static readonly int[] ResistanceX = [15, 75, 135, 195];
	private static readonly int[] Tab1ResistanceY = [232, 267];
	private static readonly int[] Tab2ResistanceY = [55, 90];

	/// <summary>The game draws the number in a 40x16 box 9 pixels right and 11 down from the 24x24 icon, right-aligned (<c>resistanceFireNumber</c>).</summary>
	private static readonly Size ResistanceNumberOffset = new(9, 11);

	/// <summary>Damage icons of tab III: 5 per row every 50 pixels; race icons: 6 per row every 40 pixels.</summary>
	private static readonly int[] DamageY = [55, 90];
	private static readonly int[] RaceY = [198, 234];

	/// <summary>The rows of tab II (<c>lifeRegenText</c>... <c>blockPercentText</c>).</summary>
	private static readonly int[] Tab2RowY = [138, 157, 182, 200, 226, 244, 281, 299, 325, 343, 369];

	private static readonly int[] OffenseY = [333, 348, 363];
	private static readonly int[] Tab3RowY = [126, 144];

	/// <summary>NumberStandard_Green: pure green.</summary>
	private static readonly Color Green = Color.FromArgb(0, 255, 0);
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

	public Font? NameFont { get; set; }
	public Font? ClassFont { get; set; }
	public Font? TextFont { get; set; }
	public Font? TitleFont { get; set; }
	public Font? NumberFont { get; set; }
	public Font? TabFont { get; set; }

	/// <summary>The resistance numbers: Arial 14, not bold (<c>NumberStandard_Green</c>).</summary>
	public Font? ResistanceFont { get; set; }

	/// <summary>The tab shown: 0 for I, 1 for II, 2 for III.</summary>
	public int Tab
	{
		get => this._tab;
		set
		{
			if (value is < 0 or > 2 || value == this._tab)
				return;

			this._tab = value;
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
			this.Size = new Size(this.Scaled(Math.Max(CharacterPanelLayout.Width, GoldBarWidth)), this.Scaled(GoldTop + GoldBarHeight));
		}
	}

	/// <summary>The "+" button of an attribute on screen.</summary>
	public Rectangle PlusBounds(int attribute) => this.ScaledRect(new Rectangle(15, AttributeY[attribute] + 3, 12, 12));

	/// <summary>A tab (I, II, III) on screen.</summary>
	public Rectangle TabBounds(int tab) => this.ScaledRect(new Rectangle(TabX[tab] + 1, 0, 60, 22));

	private int Scaled(int value) => (int)Math.Round(value * this._panelScale);

	private Rectangle ScaledRect(Rectangle r) => new(this.Scaled(r.X), this.Scaled(r.Y), this.Scaled(r.Width), this.Scaled(r.Height));

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			this._toolTip.Dispose();
			this._panel.Dispose();
			this._goldPile.Dispose();
			this._coin.Dispose();
			this._numberBox.Dispose();
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

		// The attribute rows tell their base value and bonuses; icons and rows their meaning, as the game's rollovers.
		var text = string.Empty;
		if (this.View?.Invoke() is { } view)
			foreach (var (area, tip) in this.ToolTipAreas(view))
				if (this.ScaledRect(area).Contains(e.Location))
					text = tip;

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

		g.DrawImage(this._panel, this.ScaledRect(new Rectangle(0, 0, this._panel.Width, this._panel.Height)));

		// The gold bar, piece by piece at their places in the window, on the page.
		this.DrawGoldPiece(g, this._goldPile, GoldPileAt);
		this.DrawGoldPiece(g, this._numberBox, GoldBoxAt);
		this.DrawGoldPiece(g, this._coin, CoinAt);
		this.DrawGoldPiece(g, this._numberBox, ElectrumBoxAt);

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
				this.PaintTab2(g, view.Stats);
				break;
			default:
				this.PaintTab3(g, view);
				break;
		}

		// The gold bar: gold in yellow, Electrum in grey, centred in their boxes (characterGoldNumber...).
		this.DrawText(g, view.Money, this.NumberFont, Yellow, new Rectangle(38, GoldTop + 6, 102, 20), StringAlignment.Center);
		this.DrawText(g, view.Electrum, this.NumberFont, Grey, new Rectangle(190, GoldTop + 6, 102, 20), StringAlignment.Center);
	}

	/// <summary>Draws a piece of the gold bar at its place in the bar, under the panel.</summary>
	private void DrawGoldPiece(Graphics g, Bitmap piece, Point at)
		=> g.DrawImage(piece, this.ScaledRect(new Rectangle(at.X, GoldTop + at.Y, piece.Width, piece.Height)));

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

		// Defense (tagCDefense at 15,187): combined armor, then the resistance icons with their numbers.
		var stats = view.Stats;
		this.Bar(g, 178);
		this.DrawText(g, stats.DefenseTitle, this.TitleFont, Color.White, new Rectangle(15, 187, 228, 18), StringAlignment.Near);
		this.Row(g, stats.Armor.Label, stats.Armor.Value, 206);
		this.ResistanceIcons(g, stats.Resistances.Take(8), Tab1ResistanceY);

		// Offense (tagCOffense at 15,314): only the game computes damage, attack speed and DPS.
		this.Bar(g, 303);
		this.DrawText(g, stats.OffenseTitle, this.TitleFont, Color.White, new Rectangle(15, 314, 228, 18), StringAlignment.Near);
		for (int i = 0; i < stats.Offense.Count && i < OffenseY.Length; i++)
			this.Row(g, stats.Offense[i].Label, stats.Offense[i].Value, OffenseY[i]);
	}

	private void PaintTab2(Graphics g, CharacterStatsView stats)
	{
		this.DrawText(g, stats.SecondaryTitle, this.TitleFont, Color.White, new Rectangle(15, 31, 228, 18), StringAlignment.Near);
		this.ResistanceIcons(g, stats.Resistances.Skip(8), Tab2ResistanceY);
		this.Bar(g, 126);
		this.Bar(g, 270);
		for (int i = 0; i < stats.Tab2Rows.Count && i < Tab2RowY.Length; i++)
			this.Row(g, stats.Tab2Rows[i].Label, stats.Tab2Rows[i].Value, Tab2RowY[i]);
	}

	/// <summary>Four resistance icons per row, the green number on each (<c>NumberStandard_Green</c>).</summary>
	private void ResistanceIcons(Graphics g, IEnumerable<StatIconView> icons, int[] rows)
	{
		foreach (var (at, icon) in IconPlaces(rows.Length * ResistanceX.Length, ResistanceX.Length, 60, rows).Zip(icons))
		{
			this.Icon(g, icon.Icon, at);
			this.DrawText(g, icon.Value, this.ResistanceFont ?? this.NumberFont, Green, new Rectangle(at.X + ResistanceNumberOffset.Width, at.Y + ResistanceNumberOffset.Height, 40, 16), StringAlignment.Far);
		}
	}

	/// <summary>An icon at its size in the game window.</summary>
	private void Icon(Graphics g, string texture, Point at)
	{
		if (string.IsNullOrEmpty(texture))
			return;

		var icon = this.Picture(texture);
		g.DrawImage(icon, this.ScaledRect(new Rectangle(at.X, at.Y, icon.Width, icon.Height)));
	}

	/// <summary>The places of the icons of a list: <paramref name="perRow"/> per row every <paramref name="step"/> pixels from x 15.</summary>
	private static IEnumerable<Point> IconPlaces(int count, int perRow, int step, int[] rows)
		=> Enumerable.Range(0, Math.Min(count, perRow * rows.Length)).Select(i => new Point(15 + i % perRow * step, rows[i / perRow]));

	/// <summary>The areas with a tool tip on the tab shown, in game pixels.</summary>
	private IEnumerable<(Rectangle Area, string ToolTip)> ToolTipAreas(CharacterPanelView view)
	{
		var stats = view.Stats;
		switch (this._tab)
		{
			case 0:
				for (int i = 0; i < view.Attributes.Count && i < AttributeY.Length; i++)
					yield return (new Rectangle(15, AttributeY[i], 228, 16), view.Attributes[i].ToolTip);
				yield return (new Rectangle(15, 206, 228, 16), stats.Armor.ToolTip);
				foreach (var (place, icon) in IconPlaces(8, 4, 60, Tab1ResistanceY).Zip(stats.Resistances))
					yield return (new Rectangle(place, new Size(50, 32)), icon.ToolTip);
				for (int i = 0; i < stats.Offense.Count && i < OffenseY.Length; i++)
					yield return (new Rectangle(15, OffenseY[i], 228, 15), stats.Offense[i].ToolTip);
				break;
			case 1:
				foreach (var (place, icon) in IconPlaces(8, 4, 60, Tab2ResistanceY).Zip(stats.Resistances.Skip(8)))
					yield return (new Rectangle(place, new Size(50, 32)), icon.ToolTip);
				for (int i = 0; i < stats.Tab2Rows.Count && i < Tab2RowY.Length; i++)
					yield return (new Rectangle(15, Tab2RowY[i], 228, 16), stats.Tab2Rows[i].ToolTip);
				break;
			default:
				foreach (var (place, icon) in IconPlaces(stats.Damages.Count, 5, 50, DamageY).Zip(stats.Damages))
					yield return (new Rectangle(place, new Size(40, 32)), icon.ToolTip);
				for (int i = 0; i < stats.Tab3Rows.Count && i < Tab3RowY.Length; i++)
					yield return (new Rectangle(15, Tab3RowY[i], 228, 16), stats.Tab3Rows[i].ToolTip);
				foreach (var (place, icon) in IconPlaces(stats.Races.Count, 6, 40, RaceY).Zip(stats.Races))
					yield return (new Rectangle(place, new Size(36, 34)), icon.ToolTip);
				break;
		}
	}

	private void PaintTab3(Graphics g, CharacterPanelView view)
	{
		// Damage & Duration Bonuses and Monster Race Bonuses: icons, their values in tool tips as the game's rollovers.
		var stats = view.Stats;
		this.DrawText(g, stats.DamageTitle, this.TitleFont, Color.White, new Rectangle(15, 31, 228, 18), StringAlignment.Near);
		foreach (var (place, icon) in IconPlaces(stats.Damages.Count, 5, 50, DamageY).Zip(stats.Damages))
			this.Icon(g, icon.Icon, place);
		for (int i = 0; i < stats.Tab3Rows.Count && i < Tab3RowY.Length; i++)
			this.Row(g, stats.Tab3Rows[i].Label, stats.Tab3Rows[i].Value, Tab3RowY[i]);

		this.Bar(g, 165);
		this.DrawText(g, stats.RaceTitle, this.TitleFont, Color.White, new Rectangle(15, 174, 228, 18), StringAlignment.Near);
		foreach (var (place, icon) in IconPlaces(stats.Races.Count, 6, 40, RaceY).Zip(stats.Races))
			this.Icon(g, icon.Icon, place);

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
