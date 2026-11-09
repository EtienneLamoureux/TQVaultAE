using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>What the side panel shows, given by the dialog.</summary>
internal sealed record CharacterSideView
{
	public string LevelingTitle { get; init; } = string.Empty;

	/// <summary>The labels of the leveling rows, whose editors the dialog places with <see cref="CharacterSidePanel.EditorRow"/>.</summary>
	public IReadOnlyList<string> LevelingLabels { get; init; } = [];

	/// <summary>The values of the leveling rows whose editor is hidden (not editable now), written as the game writes numbers.</summary>
	public IReadOnlyList<string> LevelingValues { get; init; } = [];

	public string MasteriesTitle { get; init; } = string.Empty;
	public IReadOnlyList<(string Name, string Value)> Masteries { get; init; } = [];

	/// <summary>The options of the computed statistics: the difficulty played and whether the buffs count.</summary>
	public string StatisticsTitle { get; init; } = string.Empty;
	public IReadOnlyList<string> StatisticsLabels { get; init; } = [];
}

/// <summary>
/// The editor's own rows beside the game's character window, drawn in its style: the leveling editors (where the game
/// draws defense and offense in its window), the masteries, and the options of the statistics TQVault computes
/// (difficulty, buffs). Sizes are in pixels of the game window times <see cref="PanelScale"/>.
/// </summary>
internal sealed partial class CharacterSidePanel : Control
{
	private const int PanelWidth = 256;
	private const string StatsBar = """InGameUI\CharacterScreen\StatsBar01.tex""";

	/// <summary>Top of the titles and first rows of the three parts, rows every 20 pixels.</summary>
	private const int LevelingTop = 6;
	private const int MasteriesTop = 134;
	private const int StatisticsTop = 200;
	private const int RowHeight = 20;
	private const int PanelHeight = StatisticsTop + 26 + 2 * RowHeight + 6;

	private static readonly Color Grey = Color.FromArgb(200, 200, 190);

	private readonly IUIService _uiService;
	private float _panelScale = 1F;

	public CharacterSidePanel(IUIService uiService)
	{
		this._uiService = uiService;
		this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
		this.BackColor = Color.Transparent;
		this.PanelScale = 1F;
	}

	/// <summary>The state shown, asked when painting.</summary>
	public Func<CharacterSideView>? View { get; set; }

	/// <summary>The editors placed on the panel (leveling, then statistics options).</summary>
	public List<Control> Editors { get; } = [];

	public Font? TextFont { get; set; }
	public Font? TitleFont { get; set; }
	public Font? NumberFont { get; set; }

	/// <summary>The size of a game pixel on screen.</summary>
	public float PanelScale
	{
		get => this._panelScale;
		set
		{
			this._panelScale = value;
			this.Size = new Size(this.Scaled(PanelWidth), this.Scaled(PanelHeight));
		}
	}

	/// <summary>The place of a leveling editor on screen: right part of its row, where the game writes numbers.</summary>
	public Rectangle EditorRow(int row) => this.ScaledRect(new Rectangle(140, RowTop(LevelingTop, row) - 1, 103, 18));

	/// <summary>The place of a statistics option on screen (difficulty, buffs).</summary>
	public Rectangle StatisticsRow(int row) => this.ScaledRect(new Rectangle(140, RowTop(StatisticsTop, row) - 1, 103, 18));

	private static int RowTop(int partTop, int row) => partTop + 22 + row * RowHeight;

	private int Scaled(int value) => (int)Math.Round(value * this._panelScale);

	private Rectangle ScaledRect(Rectangle r) => new(this.Scaled(r.X), this.Scaled(r.Y), this.Scaled(r.Width), this.Scaled(r.Height));

	protected override void OnPaint(PaintEventArgs e)
	{
		var g = e.Graphics;
		g.InterpolationMode = InterpolationMode.HighQualityBicubic;
		g.PixelOffsetMode = PixelOffsetMode.Half;
		g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

		// A dark box like the game window's black, on the page's marble.
		using (var back = new SolidBrush(Color.FromArgb(200, 0, 0, 0)))
			g.FillRectangle(back, this.ClientRectangle);
		using (var border = new Pen(Color.FromArgb(150, 120, 80), Math.Max(1F, this._panelScale)))
			g.DrawRectangle(border, new Rectangle(0, 0, this.Width - 1, this.Height - 1));

		if (this.View?.Invoke() is not { } view)
			return;

		this.DrawText(g, view.LevelingTitle, this.TitleFont, Color.White, new Rectangle(15, LevelingTop, 228, 18), StringAlignment.Near);
		for (int i = 0; i < view.LevelingLabels.Count; i++)
		{
			int y = RowTop(LevelingTop, i);
			this.DrawText(g, view.LevelingLabels[i], this.TextFont, Grey, new Rectangle(15, y, 125, 16), StringAlignment.Near);
			if (i < view.LevelingValues.Count)
				this.DrawText(g, view.LevelingValues[i], this.NumberFont, Color.White, new Rectangle(140, y, 103, 16), StringAlignment.Far);
		}

		this.Bar(g, MasteriesTop - 8);
		this.DrawText(g, view.MasteriesTitle, this.TitleFont, Color.White, new Rectangle(15, MasteriesTop, 228, 18), StringAlignment.Near);
		for (int i = 0; i < view.Masteries.Count; i++)
		{
			int y = MasteriesTop + 20 + i * 18;
			var valueWidth = string.IsNullOrEmpty(view.Masteries[i].Value) ? 0
				: (int)Math.Ceiling(g.MeasureString(view.Masteries[i].Value, this.TextFont ?? this.Font).Width / this._panelScale) + 4;
			this.DrawText(g, view.Masteries[i].Value, this.TextFont, Color.White, new Rectangle(243 - valueWidth, y, valueWidth, 16), StringAlignment.Far);
			this.DrawText(g, view.Masteries[i].Name, this.TextFont, Grey, new Rectangle(15, y, 228 - valueWidth, 16), StringAlignment.Near);
		}

		this.Bar(g, StatisticsTop - 8);
		this.DrawText(g, view.StatisticsTitle, this.TitleFont, Color.White, new Rectangle(15, StatisticsTop, 228, 18), StringAlignment.Near);
		for (int i = 0; i < view.StatisticsLabels.Count; i++)
			this.DrawText(g, view.StatisticsLabels[i], this.TextFont, Grey, new Rectangle(15, RowTop(StatisticsTop, i), 125, 16), StringAlignment.Near);
	}

	private void Bar(Graphics g, int y)
	{
		var bar = this._uiService.LoadBitmap(StatsBar.ToRecordId());
		g.DrawImage(bar, this.ScaledRect(new Rectangle(9, y, bar.Width, bar.Height)));
	}

	private void DrawText(Graphics g, string text, Font? font, Color color, Rectangle box, StringAlignment alignment)
	{
		if (string.IsNullOrEmpty(text))
			return;

		using var format = new StringFormat
		{
			Alignment = alignment,
			Trimming = StringTrimming.EllipsisCharacter,
			FormatFlags = StringFormatFlags.NoWrap,
		};
		using var brush = new SolidBrush(color);
		g.DrawString(text, font ?? this.Font, brush, this.ScaledRect(box), format);
	}
}
