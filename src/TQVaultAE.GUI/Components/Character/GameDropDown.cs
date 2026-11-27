using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Menus;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>
/// A drop-down list drawn with the game's pictures, as the loadout bar's box: the gold box of the character window, the
/// small arrow and the selected text; a click opens the list as a menu of the TQVault look. Everything is drawn at
/// <see cref="BarScale"/>.
/// </summary>
internal sealed class GameDropDown : Control
{
	private const string ArrowUp = """InGameUI\quest\arrowdownsmallup.tex""";
	private const string ArrowOver = """InGameUI\quest\arrowdownsmallover.tex""";
	private const string ArrowDown = """InGameUI\quest\arrowdownsmalldown.tex""";

	/// <summary>Corner of the gold box picture kept unstretched.</summary>
	private const int BoxCorner = 4;

	/// <summary>The text of an entry the list cannot pick (none selected, nothing to list).</summary>
	internal static readonly Color Grey = Color.FromArgb(190, 190, 190);

	/// <summary>The arrow's look: up, under the mouse, or pressed.</summary>
	internal enum ArrowState { Up, Over, Pressed }

	private readonly IUIService _uiService;
	private readonly IFontService _fontService;
	private readonly Bitmap _box = Images.CharacterNumberBox;
	private bool _hovered;
	private bool _pressed;
	private float _scale = 1F;

	public GameDropDown(IUIService uiService, IFontService fontService)
	{
		this._uiService = uiService;
		this._fontService = fontService;
		this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
		this.BackColor = Color.Black;
		this.ForeColor = Color.White;
		this.TabStop = false;

		this.ListMenu = new WheelListMenu { ShowCheckMargin = true, ShowImageMargin = false };
		CustomProfessionalRenderer.Apply(this.ListMenu);
		this.ListMenu.ItemClicked += (_, e) =>
		{
			if (e.ClickedItem?.Tag is int index)
				this.Pick(index);
		};
		this.ListMenu.FillOnOpening(this.FillListMenu);
		this.ListMenu.Closed += (_, _) =>
		{
			this._pressed = false;
			this.Invalidate();
		};
	}

	/// <summary>Raised when the user picks an entry (its index); setting the selection with <see cref="SetItems"/> raises nothing.</summary>
	public event EventHandler<int>? ItemPicked;

	/// <summary>The list opened by a click.</summary>
	internal WheelListMenu ListMenu { get; }

	/// <summary>The entries.</summary>
	public IReadOnlyList<string> Items { get; private set; } = [];

	/// <summary>The selected entry, or -1.</summary>
	public int SelectedIndex { get; private set; } = -1;

	/// <summary>The selected entry's text, or empty.</summary>
	public string SelectedText => this.SelectedIndex >= 0 && this.SelectedIndex < this.Items.Count ? this.Items[this.SelectedIndex] : string.Empty;

	/// <summary>Whether the list is open.</summary>
	public bool DroppedDown => this.ListMenu.Visible;

	/// <summary>Shown greyed when nothing is listed (with <see cref="SetItems"/> empty).</summary>
	public string? EmptyText { get; set; }

	/// <summary>The most entries the open list shows at once (0: all); a longer list scrolls with the menu's arrows.</summary>
	public int MaxDropDownItems { get; set; }

	/// <summary>The font of the text, made by the owner at its scale (not disposed here).</summary>
	public Font? TextFont { get; set; }

	/// <summary>
	/// The text of the mark drawn in the box before the arrow (the difference mark of the loadouts), or null for no mark;
	/// the owner shows it as the box's tool tip.
	/// </summary>
	public string? MarkToolTip
	{
		get;
		set
		{
			if (field == value)
				return;

			field = value;
			this.Invalidate();
		}
	}

	/// <summary>The owner's scale: sizes the pictures.</summary>
	public float BarScale
	{
		get => this._scale;
		set
		{
			this._scale = value;
			this.Invalidate();
		}
	}

	/// <summary>Shows entries and selects one, without raising <see cref="ItemPicked"/>.</summary>
	public void SetItems(IReadOnlyList<string> items, int selected)
	{
		selected = items.Count == 0 ? -1 : Math.Clamp(selected, 0, items.Count - 1);
		if (selected == this.SelectedIndex && items.SequenceEqual(this.Items, StringComparer.Ordinal))
			return;

		this.Items = [.. items];
		this.SelectedIndex = selected;
		this.Invalidate();
	}

	/// <summary>Picks an entry as the user's click in the list does.</summary>
	internal void Pick(int index)
	{
		if (index < 0 || index >= this.Items.Count)
			return;

		this.SelectedIndex = index;
		this.Invalidate();
		this.ItemPicked?.Invoke(this, index);
	}

	private void FillListMenu()
	{
		this.ListMenu.Font = MenuStyle.MenuFont(this._fontService, this._uiService.FontZoom);
		this.ListMenu.Items.Clear();
		if (this.Items.Count == 0)
			this.ListMenu.Items.Add(new ToolStripMenuItem(this.EmptyText ?? string.Empty) { Enabled = false });
		else
		{
			for (int i = 0; i < this.Items.Count; i++)
				this.ListMenu.Items.Add(new ToolStripMenuItem(this.Items[i]) { Tag = i, Checked = i == this.SelectedIndex });
		}

		this.ListMenu.WidenRows(this.Width);
		int row = this.ListMenu.Items[0].GetPreferredSize(Size.Empty).Height;
		var padding = this.ListMenu.Padding.Vertical + 4;
		this.ListMenu.MaximumSize = this.MaxDropDownItems > 0 && this.Items.Count > this.MaxDropDownItems
			? new Size(0, (row * this.MaxDropDownItems) + padding)
			: Size.Empty;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (!this._hovered)
		{
			this._hovered = true;
			this.Invalidate();
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		this._hovered = false;
		this.Invalidate();
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if (e.Button != MouseButtons.Left)
			return;

		this._pressed = true;
		this.Invalidate();
		this.ListMenu.Show(this, new Point(0, this.Height));
	}

	/// <summary>The wheel over the box scrolls the open list too (the wheel goes to the window with the focus).</summary>
	protected override void OnMouseWheel(MouseEventArgs e)
	{
		base.OnMouseWheel(e);
		if (!this.DroppedDown || e.Delta == 0)
			return;

		this.ListMenu.ScrollRows(-(e.Delta / 120 is var notches and not 0 ? notches : Math.Sign(e.Delta)));
		if (e is HandledMouseEventArgs handled)
			handled.Handled = true;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		var state = this._pressed ? ArrowState.Pressed : this._hovered && this.Items.Count > 0 ? ArrowState.Over : ArrowState.Up;
		var text = this.Items.Count == 0 ? this.EmptyText ?? string.Empty : this.SelectedText;
		var mark = this.MarkToolTip is null ? Rectangle.Empty : MarkBounds(this.ClientRectangle, this._scale);
		DrawField(e.Graphics, this._uiService, this._box, this.ClientRectangle, this._scale, text, this.TextFont ?? this.Font, this.Items.Count == 0 ? Grey : this.ForeColor, state, mark.Width);
		if (!mark.IsEmpty)
			DrawMark(e.Graphics, this._uiService, mark);
	}

	/// <summary>Where a box drawn by <see cref="DrawField"/> in <paramref name="field"/> has its mark: just before the arrow, as large as it.</summary>
	internal static Rectangle MarkBounds(Rectangle field, float scale)
	{
		int size = Math.Max(1, field.Height - Convert.ToInt32(6 * scale));
		int left = field.Right - size - Convert.ToInt32(3 * scale) - Convert.ToInt32(2 * scale) - size;
		return new Rectangle(left, field.Top + ((field.Height - size) / 2), size, size);
	}

	/// <summary>Draws the mark (the loadouts' difference mark) in <paramref name="mark"/>.</summary>
	internal static void DrawMark(Graphics g, IUIService uiService, Rectangle mark)
		=> GamePicture.Draw(g, uiService.LoadBitmap(Panels.EquipmentPanel.DifferentMark.ToRecordId()), mark, greyed: false);

	/// <summary>
	/// Draws a drop-down's box: the gold box picture, its corners kept and its edges stretched to <paramref name="bounds"/>,
	/// the small arrow at its right end, pointing down, and <paramref name="text"/> before it. The loadout bar draws its
	/// box with it too.
	/// </summary>
	/// <param name="textInset">Room left free before the arrow, for a mark the owner draws there.</param>
	internal static void DrawField(Graphics g, IUIService uiService, Bitmap box, Rectangle bounds, float scale, string text, Font font, Color color, ArrowState state, int textInset = 0)
	{
		g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
		g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
		DrawBox(g, box, bounds, scale);

		int size = Math.Max(1, bounds.Height - Convert.ToInt32(6 * scale));
		var arrow = new Rectangle(bounds.Right - size - Convert.ToInt32(3 * scale), bounds.Top + ((bounds.Height - size) / 2), size, size);
		var picture = state switch { ArrowState.Pressed => ArrowDown, ArrowState.Over => ArrowOver, _ => ArrowUp };
		// The picture's triangle points up: drawn upside down, it points to the list it opens.
		GamePicture.Draw(g, uiService.LoadBitmap(picture.ToRecordId()), arrow, greyed: false, flip: true);

		var textBounds = Rectangle.FromLTRB(bounds.Left + Convert.ToInt32(6 * scale), bounds.Top, arrow.Left - Convert.ToInt32(2 * scale) - (textInset > 0 ? textInset + Convert.ToInt32(2 * scale) : 0), bounds.Bottom);
		TextRenderer.DrawText(g, text, font, textBounds, color
			, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
	}

	/// <summary>The gold box of the character window, its corners kept and its edges stretched to the bounds.</summary>
	private static void DrawBox(Graphics g, Bitmap box, Rectangle to, float scale)
		=> NineSlice.Draw(g, box, new Rectangle(Point.Empty, box.Size), BoxCorner, to, Math.Max(1, Convert.ToInt32(BoxCorner * scale)));

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			this._box.Dispose();
			this.ListMenu.Dispose();
		}

		base.Dispose(disposing);
	}
}
