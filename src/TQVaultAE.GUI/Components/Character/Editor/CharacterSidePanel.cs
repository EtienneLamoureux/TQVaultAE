using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Character.Editor;

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

	/// <summary>The option of the computed statistics: the difficulty played.</summary>
	public string StatisticsTitle { get; init; } = string.Empty;
	public IReadOnlyList<string> StatisticsLabels { get; init; } = [];
}

/// <summary>
/// The editor's own rows under the game's character window, drawn in its style: the option of the statistics TQVault
/// computes (difficulty), the leveling editors (where the game draws defense and offense in its window) and the
/// masteries. Sizes are in pixels of the game window times <see cref="PanelScale"/>.
/// </summary>
internal sealed partial class CharacterSidePanel : Control
{
	private const int PanelWidth = 256;

	/// <summary>
	/// Top of the titles and first rows of the three parts, rows every 20 pixels. The statistics options come first: the
	/// panel is under the statistics panel, whose values they change.
	/// </summary>
	private const int StatisticsTop = 6;
	private const int RowHeight = 20;
	private const int LevelingTop = StatisticsTop + 26 + RowHeight + 6;
	private const int MasteriesTop = LevelingTop + 128;
	/// <summary>Two lines per mastery (its name, then what it has), two masteries at most.</summary>
	private const int PanelHeight = MasteriesTop + 20 + 4 * 18 + 12;

	private static readonly Color Grey = PanelPainter.Grey;

	/// <summary>The frame of the statistics panel's picture: its gold border (5 pixels) around the black, under the tabs.</summary>
	private static readonly Rectangle FrameSource = new(0, 19, 255, 375);

	/// <summary>The corners of the frame kept as they are; the edges and the black between them are stretched.</summary>
	private const int FrameCorner = 12;

	private readonly IUIService _uiService;
	private readonly Bitmap _frame = Images.CharacterStatsPanel;
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

	/// <summary>The place of a statistics option on screen (difficulty).</summary>
	public Rectangle StatisticsRow(int row) => this.ScaledRect(new Rectangle(140, RowTop(StatisticsTop, row) - 1, 103, 18));

	private static int RowTop(int partTop, int row) => partTop + 22 + row * RowHeight;

	private int Scaled(int value) => this.Painter.Scaled(value);

	private Rectangle ScaledRect(Rectangle r) => this.Painter.ScaledRect(r);

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			this._frame.Dispose();

		base.Dispose(disposing);
	}

	/// <summary>
	/// The frame of the statistics panel stretched to this panel as nine pieces: the corners at their size, the edges
	/// stretched along, the black inside stretched to fill.
	/// </summary>
	private void DrawFrame(Graphics g)
	{
		// Tiled edges keep the bicubic filter from blending in the pixels outside each piece.
		using var attributes = new ImageAttributes();
		attributes.SetWrapMode(WrapMode.TileFlipXY);
		NineSlice.Draw(g, this._frame, FrameSource, FrameCorner, this.ClientRectangle, this.Scaled(FrameCorner), attributes: attributes);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		var g = e.Graphics;
		g.InterpolationMode = InterpolationMode.HighQualityBicubic;
		g.PixelOffsetMode = PixelOffsetMode.Half;
		g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

		// The statistics panel's frame: its textured gold border and its black.
		this.DrawFrame(g);

		if (this.View?.Invoke() is not { } view)
			return;

		this.DrawText(g, view.StatisticsTitle, this.TitleFont, Color.White, new Rectangle(15, StatisticsTop, 228, 18), StringAlignment.Near);
		for (int i = 0; i < view.StatisticsLabels.Count; i++)
			this.DrawRow(g, view.StatisticsLabels[i], string.Empty, RowTop(StatisticsTop, i));

		this.Bar(g, LevelingTop - 8);
		this.DrawText(g, view.LevelingTitle, this.TitleFont, Color.White, new Rectangle(15, LevelingTop, 228, 18), StringAlignment.Near);
		for (int i = 0; i < view.LevelingLabels.Count; i++)
			this.DrawRow(g, view.LevelingLabels[i], i < view.LevelingValues.Count ? view.LevelingValues[i] : string.Empty, RowTop(LevelingTop, i));

		this.Bar(g, MasteriesTop - 8);
		this.DrawText(g, view.MasteriesTitle, this.TitleFont, Color.White, new Rectangle(15, MasteriesTop, 228, 18), StringAlignment.Near);
		// The name on its own line, whole in any language; what the mastery has under it, indented.
		for (int i = 0; i < view.Masteries.Count; i++)
		{
			int y = MasteriesTop + 20 + i * 2 * 18;
			this.DrawText(g, view.Masteries[i].Name, this.TextFont, Grey, new Rectangle(15, y, 228, 16), StringAlignment.Near);
			this.DrawText(g, view.Masteries[i].Value, this.TextFont, Color.White, new Rectangle(30, y + 18, 213, 16), StringAlignment.Near);
		}
	}

	/// <summary>
	/// A row: its label, and its value right-aligned where the game writes numbers. The label takes the width up to the
	/// value or to the editor shown on the row, whichever comes first: a long label ("Points de compétence") was cut at
	/// the editors' column although only a short number or a check box follows it.
	/// </summary>
	/// <summary>A row of the panel; the label stops before an editor placed on the row.</summary>
	private void DrawRow(Graphics g, string label, string value, int y)
	{
		int labelLimit = int.MaxValue;
		var row = this.ScaledRect(new Rectangle(0, y, PanelWidth, 16));
		foreach (var editor in this.Editors)
			if (editor.Visible && editor.Top + editor.Height / 2 is var middle && middle >= row.Top && middle < row.Bottom)
				labelLimit = Math.Min(labelLimit, (int)(editor.Left / this._panelScale) - 4);

		this.Painter.Row(g, label, this.TextFont, value, this.NumberFont ?? this.Font, y, labelLimit);
	}

	private void Bar(Graphics g, int y) => this.Painter.Bar(g, this._uiService.LoadBitmap(PanelPainter.StatsBar.ToRecordId()), y);

	private void DrawText(Graphics g, string text, Font? font, Color color, Rectangle box, StringAlignment alignment)
		=> this.Painter.DrawText(g, text, font, color, box, alignment);

	/// <summary>The panel's text and bars at its scale.</summary>
	private PanelPainter Painter => new(this._panelScale, this.Font);
}
