using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Menus;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>An action of a list bar, on the entry it names.</summary>
/// <param name="action">The action.</param>
/// <param name="name">The entry (for Add, null).</param>
public sealed class ListBarEventArgs<TAction>(TAction action, string? name) : EventArgs
	where TAction : struct, Enum
{
	public TAction Action { get; } = action;

	public string? Name { get; } = name;
}

/// <summary>The size of the list bars (<see cref="GameListBar{TAction}"/>).</summary>
public static class GameListBar
{
	/// <summary>Height of a bar at scale 1: the gold box of the character window.</summary>
	public const int ReferenceHeight = 22;

	/// <summary>Height of a bar at a scale.</summary>
	public static int HeightAt(float scale) => Convert.ToInt32(ReferenceHeight * scale);
}

/// <summary>
/// A bar of named sets drawn with the game's pictures, as the loadout bar and the class set bar: a drop-down of the
/// names in the gold box of the character window, the [+] button of the attributes to add one, and a right-click menu
/// with every action. The owner does each action asked (<see cref="ActionRequested"/>) and shows the result with
/// <see cref="SetList"/>. Everything is drawn at <see cref="BarScale"/>.
/// </summary>
public abstract class GameListBar<TAction> : Control
	where TAction : struct, Enum
{
	private const string PlusUp = """InGameUI\CharacterScreen\StatIncrementButtonUp01.tex""";
	private const string PlusOver = """InGameUI\CharacterScreen\StatIncrementButtonOver01.tex""";
	private const string PlusDown = """InGameUI\CharacterScreen\StatIncrementButtonDown01.tex""";

	private readonly IUIService _uiService;
	private readonly IFontService _fontService;
	private readonly Bitmap _box = Images.CharacterNumberBox;
	private readonly ToolTip _toolTip = new();

	private enum Part { None, Field, Mark, Add }

	private Part _hovered;
	private Part _pressed;
	private float _scale = 1F;

	protected GameListBar(IUIService uiService, IFontService fontService)
	{
		this._uiService = uiService;
		this._fontService = fontService;
		this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
		this.BackColor = Color.Black;
		this.ForeColor = Color.White;

		this.ListMenu = new WheelListMenu { ShowCheckMargin = true, ShowImageMargin = false };
		this.ActionMenu = new ContextMenuStrip { ShowImageMargin = false, ShowItemToolTips = true };
		CustomProfessionalRenderer.Apply(this.ListMenu);
		CustomProfessionalRenderer.Apply(this.ActionMenu);
		this.ListMenu.ItemClicked += (_, e) =>
		{
			if (e.ClickedItem?.Tag is string name)
				this.Raise(this.SelectAction, name);
		};
		this.ActionMenu.ItemClicked += this.ActionClicked;
		this.ActionMenu.FillOnOpening(this.FillActionMenu);
		this.ListMenu.FillOnOpening(this.FillListMenu);
		this.ContextMenuStrip = this.ActionMenu;
	}

	/// <summary>Raised for each action asked; the owner does it and shows the result with <see cref="SetList"/>.</summary>
	public event EventHandler<ListBarEventArgs<TAction>>? ActionRequested;

	/// <summary>The drop-down list of the names.</summary>
	internal ContextMenuStrip ListMenu { get; }

	/// <summary>The right-click menu: every action.</summary>
	internal ContextMenuStrip ActionMenu { get; }

	/// <summary>The names, in the owner's order.</summary>
	public IReadOnlyList<string> Names { get; private set; } = [];

	/// <summary>The selected entry, or null.</summary>
	public string? Selected { get; private set; }

	/// <summary>False when the entries are only listed (a read-only character, a file that could not be read).</summary>
	public bool CanChange { get; private set; }

	/// <summary>The font of the name, made by the owner at its scale (not disposed here).</summary>
	public Font? TextFont { get; set; }

	/// <summary>
	/// The tool tip of the mark drawn in the box before the arrow (the difference mark of the loadout comparison), or
	/// null for no mark: the selected entry no longer holds what is shown.
	/// </summary>
	internal string? MarkToolTip { get; private set; }

	/// <summary>Shows the mark in the box with its tool tip, or hides it (null).</summary>
	protected void SetMark(string? toolTip)
	{
		if (toolTip == this.MarkToolTip)
			return;

		this.MarkToolTip = toolTip;
		if (this._hovered == Part.Mark)
			this._toolTip.SetToolTip(this, toolTip);
		this.Invalidate();
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

	/// <summary>The action of a pick in the list.</summary>
	protected internal abstract TAction SelectAction { get; }

	/// <summary>The action of the [+] button.</summary>
	protected internal abstract TAction AddAction { get; }

	/// <summary>The text of the box with nothing to list.</summary>
	protected abstract string NoneText { get; }

	/// <summary>The text of the box with entries but none selected.</summary>
	protected virtual string PlaceholderText => string.Empty;

	/// <summary>The tool tip of the [+] button.</summary>
	protected abstract string AddToolTip { get; }

	/// <summary>Why nothing can change, for the tool tips (<see cref="CanChange"/> false).</summary>
	protected abstract string ReadOnlyText { get; }

	/// <summary>True when the first entry is selected when none is asked for (the loadouts); false to allow none selected.</summary>
	protected abstract bool SelectsFirst { get; }

	/// <summary>Whether the bar offers an action now (its menu entry enabled, its button raising it).</summary>
	public abstract bool CanAct(TAction action);

	/// <summary>Why an action is greyed, for its tool tip, or null when the bar offers it.</summary>
	public virtual string? LockedReason(TAction action)
		=> this.CanAct(action) || this.CanChange ? null : this.ReadOnlyText;

	/// <summary>Fills the right-click menu, at each opening.</summary>
	protected abstract void FillActions(ToolStripItemCollection items);

	/// <summary>
	/// Shows the entries; <paramref name="selected"/> is selected, or the first one when it is unknown and
	/// <see cref="SelectsFirst"/>, or none.
	/// </summary>
	/// <returns>False when nothing changed.</returns>
	protected bool SetList(IReadOnlyList<string> names, string? selected, bool canChange)
	{
		selected = names.FirstOrDefault(n => string.Equals(n, selected, StringComparison.OrdinalIgnoreCase))
			?? (this.SelectsFirst && names.Count > 0 ? names[0] : null);
		if (canChange == this.CanChange && selected == this.Selected && names.SequenceEqual(this.Names, StringComparer.Ordinal))
			return false;

		this.Names = names;
		this.Selected = selected;
		this.CanChange = canChange;
		this._toolTip.SetToolTip(this, canChange ? null : this.ReadOnlyText);
		this.Invalidate();
		return true;
	}

	private int Gap => Math.Max(1, Convert.ToInt32(4 * this._scale));

	/// <summary>Height of the [+] button: the bar's height less a margin.</summary>
	private int IconHeight => Math.Max(1, this.Height - Convert.ToInt32(4 * this._scale));

	/// <summary>The [+] button, at the right end.</summary>
	internal Rectangle AddBounds
	{
		get
		{
			int size = this.IconHeight;
			return new Rectangle(this.Width - size, (this.Height - size) / 2, size, size);
		}
	}

	/// <summary>The drop-down box: the rest of the width.</summary>
	internal Rectangle FieldBounds => new(0, 0, Math.Max(this.Height, this.AddBounds.Left - this.Gap), this.Height);

	/// <summary>The mark in the box, just before the arrow, as large as the arrow; empty without a mark.</summary>
	internal Rectangle MarkBounds
	{
		get
		{
			if (this.MarkToolTip is null)
				return Rectangle.Empty;

			var box = this.FieldBounds;
			int size = Math.Max(1, box.Height - Convert.ToInt32(6 * this._scale));
			int left = box.Right - size - Convert.ToInt32(3 * this._scale) - Convert.ToInt32(2 * this._scale) - size;
			return new Rectangle(left, box.Top + ((box.Height - size) / 2), size, size);
		}
	}

	/// <summary>Raises an action on the selected entry, as its menu entry does (nothing while it is greyed).</summary>
	internal void Request(TAction action) => this.Raise(action, this.Selected);

	/// <summary>A menu entry raising an action on the selected entry: greyed with its reason as tool tip when the bar does not offer it.</summary>
	protected ToolStripMenuItem Entry(string text, TAction action)
		=> new(text) { Tag = action, Enabled = this.CanAct(action), ToolTipText = this.LockedReason(action) };

	/// <summary>An entry of a menu or submenu: the action it carries, on the selected entry.</summary>
	protected void ActionClicked(object? sender, ToolStripItemClickedEventArgs e)
	{
		if (e.ClickedItem?.Tag is TAction action)
			this.Raise(action, EqualityComparer<TAction>.Default.Equals(action, this.AddAction) ? null : this.Selected);
	}

	private void Raise(TAction action, string? name)
	{
		if (!this.CanAct(action))
			return;

		if (EqualityComparer<TAction>.Default.Equals(action, this.SelectAction))
		{
			this.Selected = name;
			this.Invalidate();
		}

		this.ActionRequested?.Invoke(this, new ListBarEventArgs<TAction>(action, name));
	}

	private void FillListMenu()
	{
		this.ListMenu.Font = this.MenuFont();
		this.ListMenu.Items.Clear();
		if (this.Names.Count == 0)
		{
			this.ListMenu.Items.Add(new ToolStripMenuItem(this.NoneText) { Enabled = false });
			return;
		}

		foreach (var name in this.Names)
			this.ListMenu.Items.Add(new ToolStripMenuItem(name) { Tag = name, Checked = name == this.Selected });
	}

	private void FillActionMenu()
	{
		this.ActionMenu.Font = this.MenuFont();
		this.ActionMenu.Items.Clear();
		this.FillActions(this.ActionMenu.Items);
	}

	/// <summary>A submenu of the right-click menu in its look, whose entries raise their action.</summary>
	protected ToolStripMenuItem SubMenu(string text, bool enabled, IEnumerable<ToolStripItem> entries, string? toolTip = null)
	{
		var sub = new ToolStripMenuItem(text) { Enabled = enabled, ToolTipText = toolTip };
		sub.DropDown.Font = this.ActionMenu.Font;
		CustomProfessionalRenderer.Apply(sub.DropDown);
		sub.DropDownItems.AddRange([.. entries]);
		sub.DropDownItemClicked += this.ActionClicked;
		return sub;
	}

	/// <summary>The menu font of every menu, at the zoom of now: the menus are filled at each opening.</summary>
	protected Font MenuFont() => MenuStyle.MenuFont(this._fontService, this._uiService.FontZoom);

	private Part PartAt(Point point)
		=> this.AddBounds.Contains(point) ? Part.Add
		: this.MarkBounds.Contains(point) ? Part.Mark
		: this.FieldBounds.Contains(point) ? Part.Field
		: Part.None;

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		var part = this.PartAt(e.Location);
		if (part == this._hovered)
			return;

		this._hovered = part;
		this._toolTip.SetToolTip(this, part switch
		{
			Part.Add => this.AddToolTip,
			Part.Mark => this.MarkToolTip,
			_ => this.CanChange ? this.Selected : this.ReadOnlyText,
		});
		this.Invalidate();
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		this._hovered = Part.None;
		this._pressed = Part.None;
		this.Invalidate();
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if (e.Button != MouseButtons.Left)
			return;

		// The mark is part of the box: a click on it opens the list too.
		this._pressed = this.PartAt(e.Location) is Part.Mark ? Part.Field : this.PartAt(e.Location);
		this.Invalidate();
		if (this._pressed == Part.Field)
			this.ListMenu.Show(this, new Point(0, this.Height));
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if (e.Button != MouseButtons.Left)
			return;

		var pressed = this._pressed;
		this._pressed = Part.None;
		this.Invalidate();
		if (pressed != (this.PartAt(e.Location) is Part.Mark ? Part.Field : this.PartAt(e.Location)))
			return;

		if (pressed == Part.Add)
			this.Raise(this.AddAction, null);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		var g = e.Graphics;
		var arrow = this._pressed == Part.Field ? GameDropDown.ArrowState.Pressed
			: this._hovered is Part.Field or Part.Mark && this.CanAct(this.SelectAction) ? GameDropDown.ArrowState.Over
			: GameDropDown.ArrowState.Up;
		string text = this.Selected ?? (this.Names.Count == 0 ? this.NoneText : this.PlaceholderText);
		var mark = this.MarkBounds;
		GameDropDown.DrawField(g, this._uiService, this._box, this.FieldBounds, this._scale, text, this.TextFont ?? this.Font, this.Selected is null ? GameDropDown.Grey : Color.White, arrow, mark.Width);
		if (!mark.IsEmpty)
			GamePicture.Draw(g, this._uiService.LoadBitmap(Panels.EquipmentPanel.DifferentMark.ToRecordId()), mark, greyed: false);

		this.DrawAddButton(g, this.CanAct(this.AddAction));
	}

	/// <summary>The [+] button, with the game picture of its state: pressed, under the mouse, or up; dimmed when it cannot be used.</summary>
	private void DrawAddButton(Graphics g, bool enabled)
	{
		var picture = !enabled ? PlusUp : this._pressed == Part.Add ? PlusDown : this._hovered == Part.Add ? PlusOver : PlusUp;
		GamePicture.Draw(g, this._uiService.LoadBitmap(picture.ToRecordId()), this.AddBounds, greyed: !enabled);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			this._box.Dispose();
			this._toolTip.Dispose();
			this.ListMenu.Dispose();
			this.ActionMenu.Dispose();
		}

		base.Dispose(disposing);
	}
}
