using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>A click on a button of the skill pane: the skill (<see langword="null"/> for the mastery) and the change asked.</summary>
/// <param name="Skill">The skill clicked, or <see langword="null"/> for the mastery button.</param>
/// <param name="Step">+1 for a left click, -1 for a right click.</param>
/// <param name="ToEnd">True with Shift: up to the highest level, or down to the lowest.</param>
internal sealed record SkillPaneClick(MasterySkill? Skill, int Step, bool ToEnd);

/// <summary>What the pane shows of a skill, given by the dialog.</summary>
/// <param name="Level">Points spent in it.</param>
/// <param name="ItemBonus">Levels the equipped items add.</param>
/// <param name="State">Its border.</param>
/// <param name="TierOpen">True when the mastery level opens its tier.</param>
internal sealed record SkillPaneSkillView(int Level, int ItemBonus, SkillButtonState State, bool TierOpen);

/// <summary>
/// A mastery pane drawn as the game's skill window draws it: background, the mastery colour through the tier lines
/// reached, mastery bar and milestones, connectors, skill buttons with their state border and level, mastery picture
/// and description. Positions come from the game records (<see cref="SkillPaneArt"/>, <see cref="SkillPaneButton"/>)
/// and <see cref="SkillPaneLayout"/>; the pane is drawn at its game size times <see cref="PaneScale"/>.
/// A left click raises a skill or the mastery, a right click lowers it, with Shift up to the highest level or down to
/// the lowest. The rules are the dialog's: this control only draws what it is given and reports clicks.
/// </summary>
internal sealed partial class SkillPaneControl : Control
{
	private static readonly Color AugmentedColor = Color.FromArgb(255, 255, 128);
	private static readonly Color NoPointColor = Color.FromArgb(255, 96, 32);

	private readonly IUIService _uiService;
	private object? _hovered;
	private float _paneScale = 1F;

	public SkillPaneControl(IUIService uiService)
	{
		this._uiService = uiService;
		this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
		this.BackColor = Color.Black;
		this.Size = new Size(SkillPaneLayout.Width, SkillPaneLayout.Height);
	}

	/// <summary>The mastery shown, or <see langword="null"/> for an empty slot.</summary>
	public MasteryTree? Tree { get; set; }

	/// <summary>
	/// The art of any mastery, for an empty slot: its background is drawn alone, as a skill window without a mastery
	/// (no picture, bar, description nor skill). <see langword="null"/> leaves an empty slot black.
	/// </summary>
	public SkillPaneArt? EmptyArt { get; set; }

	/// <summary>The mastery level.</summary>
	public int MasteryLevel { get; set; }

	/// <summary>The skill points left.</summary>
	public int AvailablePoints { get; set; }

	/// <summary>The state of each skill, asked when painting.</summary>
	public Func<MasterySkill, SkillPaneSkillView>? SkillView { get; set; }

	/// <summary>The text of a game tag.</summary>
	public Func<string, string> Translate { get; set; } = tag => tag;

	/// <summary>The tool tip text of a skill, or of the mastery for <see langword="null"/>.</summary>
	public Func<MasterySkill?, IReadOnlyList<GameTooltipLine>>? ToolTipLines { get; set; }

	/// <summary>The game-style tool tip that shows <see cref="ToolTipLines"/> over the hovered button.</summary>
	public GameTooltip? Tooltip { get; set; }

	/// <summary>The font of the level texts and milestones, in pane pixels.</summary>
	public Font? NumberFont { get; set; }

	/// <summary>The font of the points line, in pane pixels.</summary>
	public Font? TextFont { get; set; }

	/// <summary>The font of the description box, in pane pixels.</summary>
	public Font? DescriptionFont { get; set; }

	/// <summary>Raised when the user clicks a skill or the mastery button.</summary>
	public event EventHandler<SkillPaneClick>? ButtonClicked;

	/// <summary>The size of a pane pixel on screen; the control takes the pane size times this scale.</summary>
	public float PaneScale
	{
		get => this._paneScale;
		set
		{
			this._paneScale = value;
			this.Size = new Size((int)Math.Round(SkillPaneLayout.Width * value), (int)Math.Round(SkillPaneLayout.Height * value));
		}
	}

	/// <summary>The skill (or the mastery, as the tree) under a point of the control, or <see langword="null"/>.</summary>
	internal object? HitTest(Point location)
	{
		if (this.Tree is not { } tree)
			return null;

		double x = location.X / this._paneScale, y = location.Y / this._paneScale;
		if (tree.Pane is { } art && new PaneRect(art.MasteryButtonX, art.MasteryButtonY, SkillPaneLayout.MasteryButtonSize, SkillPaneLayout.MasteryButtonSize).Contains(x, y))
			return tree;

		return tree.Skills.FirstOrDefault(s => s.Button is { } b && SkillPaneLayout.ButtonRect(b).Contains(x, y));
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		int step = e.Button switch
		{
			MouseButtons.Left => 1,
			MouseButtons.Right => -1,
			_ => 0,
		};
		if (step == 0)
			return;

		var toEnd = ModifierKeys.HasFlag(Keys.Shift);
		switch (this.HitTest(e.Location))
		{
			case MasteryTree:
				this.RaiseClick(new SkillPaneClick(null, step, toEnd));
				break;
			case MasterySkill skill:
				this.RaiseClick(new SkillPaneClick(skill, step, toEnd));
				break;
		}
	}

	private void RaiseClick(SkillPaneClick click) => this.ButtonClicked?.Invoke(this, click);

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		var hovered = this.HitTest(e.Location);
		if (Equals(hovered, this._hovered))
			return;

		this._hovered = hovered;
		this.Cursor = hovered is null ? Cursors.Default : Cursors.Hand;
		var lines = hovered switch
		{
			MasteryTree => this.ToolTipLines?.Invoke(null),
			MasterySkill skill => this.ToolTipLines?.Invoke(skill),
			_ => null,
		};
		if (lines is { Count: > 0 })
			this.Tooltip?.Show(this, lines);
		else
			this.Tooltip?.Hide();
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		this._hovered = null;
		this.Tooltip?.Hide();
	}

	/// <summary>Shows the tool tip again after the dialog changed what it says.</summary>
	public void RefreshToolTip()
	{
		var hovered = this._hovered;
		this._hovered = null;
		if (hovered is not null)
			this.OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, this.PointToClient(Cursor.Position).X, this.PointToClient(Cursor.Position).Y, 0));
	}

	private Bitmap? Picture(string resource)
		=> string.IsNullOrEmpty(resource) ? null : this._uiService.LoadBitmap(resource.ToRecordId());

	protected override void OnPaint(PaintEventArgs e)
	{
		var g = e.Graphics;
		g.Clear(this.BackColor);
		var tree = this.Tree;
		if ((tree?.Pane ?? this.EmptyArt) is not { } art)
			return;

		g.ScaleTransform(this._paneScale, this._paneScale);
		g.InterpolationMode = this._paneScale == 1F ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
		g.PixelOffsetMode = PixelOffsetMode.Half;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

		// An empty slot: the skill window with no mastery in it.
		if (tree?.Pane is null)
		{
			this.DrawPicture(g, art.Background, 0, 0);
			this.DrawTexts(g, null);
			return;
		}

		// The mastery colour, which only shows through the tier lines of the background.
		var peek = SkillPaneLayout.PeekThrough(art, this.MasteryLevel);
		using (var brush = new SolidBrush(Color.FromArgb(art.PeekThroughRed, art.PeekThroughGreen, art.PeekThroughBlue)))
			g.FillRectangle(brush, peek.X, peek.Y, peek.Width, peek.Height);

		this.DrawPicture(g, art.Background, 0, 0);
		this.DrawPicture(g, art.Picture, art.PictureX, art.PictureY);
		this.DrawBar(g, art);
		this.DrawTexts(g, art);

		foreach (var skill in tree.Skills)
			if (skill.Button is { } button)
				this.DrawSkill(g, skill, button);

		this.DrawMasteryButton(g, tree, art);
	}

	private void DrawPicture(Graphics g, string resource, int x, int y)
	{
		if (this.Picture(resource) is { } picture)
			g.DrawImage(picture, x, y, picture.Width, picture.Height);
	}

	/// <summary>The bar fills from the bottom: the bottom part of its texture, as high as the fill.</summary>
	private void DrawBar(Graphics g, SkillPaneArt art)
	{
		var fill = SkillPaneLayout.BarFill(this.MasteryLevel);
		if (fill <= 0 || this.Picture(art.Bar) is not { } bar)
			return;

		var height = Math.Min(fill, bar.Height);
		var target = new Rectangle(art.BarX, art.BarY + SkillPaneLayout.BarHeight - height, bar.Width, height);
		g.DrawImage(bar, target, 0, bar.Height - height, bar.Width, height, GraphicsUnit.Pixel);
	}

	/// <summary>The milestones and the points line; with <paramref name="art"/>, the mastery description too.</summary>
	private void DrawTexts(Graphics g, SkillPaneArt? art)
	{
		using var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
		var number = this.NumberFont ?? this.Font;
		var milestones = SkillTiers.RequiredLevels;
		for (int i = 0; i < SkillPaneLayout.Milestones.Count && i < milestones.Count; i++)
		{
			var box = SkillPaneLayout.Milestones[i];
			g.DrawString(milestones[i].ToString(System.Globalization.CultureInfo.CurrentCulture), number, Brushes.White, new RectangleF(box.X - 4, box.Y, box.Width + 8, box.Height), center);
		}

		var points = this.AvailablePoints;
		var line = points <= 0
			? this.Translate("NoSkillPointsAvailable")
			: CharacterPanelLayout.FormatGameText(this.Translate(points == 1 ? "SkillPointsAvailableSingular" : "SkillPointsAvailable"), points);
		var rect = SkillPaneLayout.PointsRemaining;
		using (var brush = new SolidBrush(points <= 0 ? NoPointColor : Color.White))
			g.DrawString(line, this.TextFont ?? this.Font, brush, new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), center);

		if (art is null)
			return;

		var description = SkillPaneLayout.Description;
		g.DrawString(this.Translate(art.DescriptionTag), this.DescriptionFont ?? this.Font, Brushes.White,
			new RectangleF(description.X + 4, description.Y, description.Width - 8, description.Height), center);
	}

	private void DrawSkill(Graphics g, MasterySkill skill, SkillPaneButton button)
	{
		var view = this.SkillView?.Invoke(skill) ?? new SkillPaneSkillView(0, 0, SkillButtonState.TierLocked, false);

		foreach (var piece in SkillPaneLayout.Connectors(button, view.Level > 0))
			this.DrawPicture(g, piece.Texture, piece.X, piece.Y);

		var icon = SkillPaneLayout.IconRect(button);
		var iconResource = view.TierOpen || string.IsNullOrEmpty(button.LockedIcon) ? skill.Icon : button.LockedIcon;
		if (this.Picture(iconResource) is { } picture)
			g.DrawImage(picture, icon.X, icon.Y, icon.Width, icon.Height);

		this.DrawPicture(g, button.IsCircular ? SkillPaneLayout.RoundBorder : SkillPaneLayout.SquareBorder, button.X, button.Y);
		this.DrawPicture(g, SkillPaneLayout.StateBorder(view.State, button.IsCircular), button.X, button.Y);

		if (SkillPaneLayout.LevelText(view.Level, view.ItemBonus, skill.MaxLevel, view.TierOpen) is { } text)
		{
			var (dx, dy) = SkillPaneLayout.LevelTextOffset;
			this.DrawCenteredText(g, text, button.X + dx, button.Y + dy, view.ItemBonus > 0 ? AugmentedColor : Color.White);
		}
	}

	private void DrawMasteryButton(Graphics g, MasteryTree tree, SkillPaneArt art)
	{
		this.DrawPicture(g, tree.Icon, art.MasteryButtonX, art.MasteryButtonY);
		var (dx, dy) = SkillPaneLayout.MasteryLevelTextOffset;
		this.DrawCenteredText(g, $"{this.MasteryLevel} / {tree.MaxLevel}", art.MasteryButtonX + dx, art.MasteryButtonY + dy, Color.White);
	}

	/// <summary>A text centred on x with its top at y, with a dark outline so it reads on the background.</summary>
	private void DrawCenteredText(Graphics g, string text, int x, int y, Color color)
	{
		var font = this.NumberFont ?? this.Font;
		var size = g.MeasureString(text, font);
		var left = x - size.Width / 2;
		using var shadow = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
		g.DrawString(text, font, shadow, left + 1, y + 1);
		using var brush = new SolidBrush(color);
		g.DrawString(text, font, brush, left, y);
	}
}
