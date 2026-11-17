using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Menus;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>What the user asked the loadout bar for.</summary>
public enum LoadoutBarAction
{
	/// <summary>Another loadout is selected in the list.</summary>
	Select,

	/// <summary>Adds an empty loadout ([+] button).</summary>
	Add,

	/// <summary>Merges the worn equipment into the selected loadout.</summary>
	Update,

	/// <summary>Renames the selected loadout.</summary>
	Rename,

	/// <summary>Copies the selected loadout under a new name.</summary>
	Copy,

	/// <summary>Deletes the selected loadout.</summary>
	Delete,

	/// <summary>Gear swap: the selected loadout's pieces and the worn ones change places (menu).</summary>
	Swap,

	/// <summary>Empty loadout: its pieces move to the character's inventory (menu).</summary>
	EmptyToInventory,

	/// <summary>Empty loadout: its pieces move to the stash (menu).</summary>
	EmptyToStash,

	/// <summary>Empty loadout: its pieces move to the bag of the vault shown (menu).</summary>
	EmptyToVault,
}

/// <summary>An action of the loadout bar, on the loadout it names.</summary>
/// <param name="action">The action.</param>
/// <param name="name">The loadout (for <see cref="LoadoutBarAction.Add"/>, null).</param>
public sealed class LoadoutBarEventArgs(LoadoutBarAction action, string? name) : EventArgs
{
	public LoadoutBarAction Action { get; } = action;

	public string? Name { get; } = name;
}

/// <summary>
/// The loadouts of the shown character, under the gold of the statistics panel: a drop-down of their names in the gold
/// box of the character window and the [+] button of the attributes to add one. The right-click menu holds every action. Everything is drawn at the panel's scale.
/// </summary>
public sealed class LoadoutBar : Control
{
	private const string PlusUp = """InGameUI\CharacterScreen\StatIncrementButtonUp01.tex""";
	private const string PlusOver = """InGameUI\CharacterScreen\StatIncrementButtonOver01.tex""";
	private const string PlusDown = """InGameUI\CharacterScreen\StatIncrementButtonDown01.tex""";

	/// <summary>Height of the bar at scale 1: the gold box of the character window.</summary>
	public const int ReferenceHeight = 22;

	private readonly IUIService _uiService;
	private readonly IFontService _fontService;
	private readonly Bitmap _box = Images.CharacterNumberBox;
	private readonly ToolTip _toolTip = new();

	private enum Part { None, Field, Add }

	private Part _hovered;
	private Part _pressed;
	private float _scale = 1F;

	public LoadoutBar(IUIService uiService, IFontService fontService)
	{
		this._uiService = uiService;
		this._fontService = fontService;
		this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
		this.BackColor = Color.Black;
		this.ForeColor = Color.White;

		this.ListMenu = new ContextMenuStrip { ShowCheckMargin = true, ShowImageMargin = false };
		this.ActionMenu = new ContextMenuStrip { ShowImageMargin = false, ShowItemToolTips = true };
		CustomProfessionalRenderer.Apply(this.ListMenu);
		CustomProfessionalRenderer.Apply(this.ActionMenu);
		this.ListMenu.ItemClicked += (_, e) =>
		{
			if (e.ClickedItem?.Tag is string name)
				this.Raise(LoadoutBarAction.Select, name);
		};
		this.ActionMenu.ItemClicked += this.ActionClicked;
		this.EmptyMenu = new ContextMenuStrip { ShowImageMargin = false, ShowItemToolTips = true };
		CustomProfessionalRenderer.Apply(this.EmptyMenu);
		this.EmptyMenu.ItemClicked += this.ActionClicked;
		this.EmptyMenu.FillOnOpening(this.FillEmptyMenu);
		this.ActionMenu.FillOnOpening(this.FillActionMenu);
		this.ListMenu.FillOnOpening(this.FillListMenu);
		this.ContextMenuStrip = this.ActionMenu;
	}

	/// <summary>Raised for each action asked; the owner does it and shows the result with <see cref="ShowLoadouts"/>.</summary>
	public event EventHandler<LoadoutBarEventArgs>? ActionRequested;

	/// <summary>The drop-down list of the loadout names.</summary>
	internal ContextMenuStrip ListMenu { get; }

	/// <summary>The right-click menu: every action.</summary>
	internal ContextMenuStrip ActionMenu { get; }

	/// <summary>The menu of destinations of Empty loadout, opened by the loadout panel's button.</summary>
	internal ContextMenuStrip EmptyMenu { get; }

	/// <summary>Whether Empty loadout offers a destination now (the loadout panel's button is greyed otherwise).</summary>
	internal bool CanEmpty => EmptyActions.Any(this.CanAct);

	/// <summary>The destinations of Empty loadout, in menu order.</summary>
	private static readonly LoadoutBarAction[] EmptyActions = [LoadoutBarAction.EmptyToInventory, LoadoutBarAction.EmptyToStash, LoadoutBarAction.EmptyToVault];

	/// <summary>The loadout names, in the order of the file.</summary>
	public IReadOnlyList<string> Names { get; private set; } = [];

	/// <summary>The selected loadout, or null.</summary>
	public string? Selected { get; private set; }

	/// <summary>False for a read-only character (or a file that could not be read): the loadouts are only listed.</summary>
	public bool CanChange { get; private set; }

	/// <summary>
	/// Why the equipment cannot change (the PlayerReadonly setting), or null: Gear swap and Empty to inventory are then
	/// greyed and their tool tip says why; the loadouts can still be added, updated, renamed, copied and deleted.
	/// </summary>
	public string? EquipmentLocked { get; private set; }

	/// <summary>
	/// Why the pieces cannot be copied now (the Allow Item Copying setting off), or null: Update and Copy write copies,
	/// so they are greyed and their tool tip says why. Read at each menu opening: a setting change counts at once.
	/// </summary>
	public Func<string?>? CopyLocked { get; set; }

	/// <summary>The font of the name, made by the owner at the panel's scale (not disposed here).</summary>
	public Font? TextFont { get; set; }

	/// <summary>The panel's scale: sizes the pictures.</summary>
	public float BarScale
	{
		get => this._scale;
		set
		{
			this._scale = value;
			this.Invalidate();
		}
	}

	/// <summary>Shows the loadouts of a character.</summary>
	/// <param name="names">Their names.</param>
	/// <param name="selected">The one to select; the first one when null or unknown.</param>
	/// <param name="canChange">False for a character whose loadouts are only listed.</param>
	/// <param name="equipmentLocked">Why the equipment cannot change, or null when Gear swap can change it.</param>
	public void ShowLoadouts(IReadOnlyList<string> names, string? selected, bool canChange, string? equipmentLocked = null)
	{
		selected = names.FirstOrDefault(n => string.Equals(n, selected, StringComparison.OrdinalIgnoreCase)) ?? (names.Count > 0 ? names[0] : null);
		if (canChange == this.CanChange && equipmentLocked == this.EquipmentLocked && selected == this.Selected && names.SequenceEqual(this.Names, StringComparer.Ordinal))
			return;

		this.Names = names;
		this.Selected = selected;
		this.CanChange = canChange;
		this.EquipmentLocked = equipmentLocked;
		this._toolTip.SetToolTip(this, canChange ? null : Resources.LoadoutReadOnly);
		this.Invalidate();
	}

	/// <summary>Height of the bar at a scale.</summary>
	public static int HeightAt(float scale) => Convert.ToInt32(ReferenceHeight * scale);

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

	/// <summary>Whether the bar offers an action now (its menu entry enabled, its button raising it).</summary>
	internal bool CanAct(LoadoutBarAction action) => action switch
	{
		LoadoutBarAction.Select => this.Names.Count > 0,
		LoadoutBarAction.Add => this.CanChange,
		LoadoutBarAction.Swap or LoadoutBarAction.EmptyToInventory => this.CanChange && this.Selected is not null && this.EquipmentLocked is null,
		LoadoutBarAction.Update or LoadoutBarAction.Copy => this.CanChange && this.Selected is not null && this.CopyLocked?.Invoke() is null,
		_ => this.CanChange && this.Selected is not null,
	};

	/// <summary>Raises an action on the selected loadout, as its menu entry does (nothing while it is greyed).</summary>
	/// <param name="action">The action.</param>
	internal void Request(LoadoutBarAction action) => this.Raise(action, this.Selected);

	/// <summary>
	/// Why an action is greyed, for its tool tip, or null when the bar offers it: a read-only character, then the
	/// Allow Item Copying setting for Update and Copy, the PlayerReadonly setting for Gear swap and Empty to inventory.
	/// </summary>
	/// <param name="action">The action.</param>
	internal string? LockedReason(LoadoutBarAction action)
		=> this.CanAct(action) ? null
		: !this.CanChange ? Resources.LoadoutReadOnly
		: action is LoadoutBarAction.Update or LoadoutBarAction.Copy ? this.CopyLocked?.Invoke()
		: this.EquipmentLocked is null ? null
		: action is LoadoutBarAction.EmptyToInventory ? string.Format(System.Globalization.CultureInfo.CurrentCulture, Resources.LoadoutInventoryLocked, Resources.SettingsPlayerReadonly)
		: action is LoadoutBarAction.Swap ? this.EquipmentLocked
		: null;

	/// <summary>An entry of the action menu or of its Empty loadout submenu: the action it carries.</summary>
	private void ActionClicked(object? sender, ToolStripItemClickedEventArgs e)
	{
		if (e.ClickedItem?.Tag is LoadoutBarAction action)
			this.Raise(action, action == LoadoutBarAction.Add ? null : this.Selected);
	}

	private void Raise(LoadoutBarAction action, string? name)
	{
		if (!this.CanAct(action))
			return;

		if (action == LoadoutBarAction.Select)
		{
			this.Selected = name;
			this.Invalidate();
		}

		this.ActionRequested?.Invoke(this, new LoadoutBarEventArgs(action, name));
	}

	private void FillListMenu()
	{
		this.ListMenu.Font = this.MenuFont();
		this.ListMenu.Items.Clear();
		if (this.Names.Count == 0)
		{
			this.ListMenu.Items.Add(new ToolStripMenuItem(Resources.LoadoutNone) { Enabled = false });
			return;
		}

		foreach (var name in this.Names)
			this.ListMenu.Items.Add(new ToolStripMenuItem(name) { Tag = name, Checked = name == this.Selected });
	}

	private void FillActionMenu()
	{
		this.ActionMenu.Font = this.MenuFont();
		this.ActionMenu.Items.Clear();
		ToolStripMenuItem Entry(string text, LoadoutBarAction action)
			=> new(text) { Tag = action, Enabled = this.CanAct(action), ToolTipText = this.LockedReason(action) };

		var empty = new ToolStripMenuItem(Resources.LoadoutEmpty) { Enabled = this.CanEmpty };
		empty.DropDown.Font = this.ActionMenu.Font;
		CustomProfessionalRenderer.Apply(empty.DropDown);
		empty.DropDownItems.AddRange(this.EmptyEntries());
		empty.DropDownItemClicked += this.ActionClicked;

		this.ActionMenu.Items.AddRange(
		[
			Entry(Resources.LoadoutSwap, LoadoutBarAction.Swap),
			empty,
			Entry(Resources.LoadoutUpdate, LoadoutBarAction.Update),
			Entry(Resources.LoadoutRename, LoadoutBarAction.Rename),
			Entry(Resources.LoadoutCopy, LoadoutBarAction.Copy),
			Entry(Resources.LoadoutDelete, LoadoutBarAction.Delete),
			new ToolStripSeparator(),
			Entry(Resources.LoadoutAddMenu, LoadoutBarAction.Add),
		]);
	}

	private void FillEmptyMenu()
	{
		this.EmptyMenu.Font = this.MenuFont();
		this.EmptyMenu.Items.Clear();
		this.EmptyMenu.Items.AddRange(this.EmptyEntries());
	}

	/// <summary>
	/// The destinations of Empty loadout, for the right-click menu's submenu and the loadout panel's button menu; a greyed
	/// one says why (the PlayerReadonly setting for the inventory).
	/// </summary>
	private ToolStripItem[] EmptyEntries()
	{
		ToolStripMenuItem Target(string text, LoadoutBarAction action)
			=> new(text) { Tag = action, Enabled = this.CanAct(action), ToolTipText = this.LockedReason(action) };

		return
		[
			Target(Resources.LoadoutEmptyToInventory, LoadoutBarAction.EmptyToInventory),
			Target(Resources.LoadoutEmptyToStash, LoadoutBarAction.EmptyToStash),
			Target(Resources.LoadoutEmptyToVault, LoadoutBarAction.EmptyToVault),
		];
	}

	/// <summary>The menu font of every menu, at the zoom of now: the menus are filled at each opening.</summary>
	private Font MenuFont() => MenuStyle.MenuFont(this._fontService, this._uiService.FontZoom);

	private Part PartAt(Point point)
		=> this.AddBounds.Contains(point) ? Part.Add
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
			Part.Add => Resources.LoadoutAdd,
			_ => this.CanChange ? this.Selected : Resources.LoadoutReadOnly,
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

		this._pressed = this.PartAt(e.Location);
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
		if (pressed != this.PartAt(e.Location))
			return;

		if (pressed == Part.Add)
			this.Raise(LoadoutBarAction.Add, null);
	}

	private Bitmap? Picture(string resource) => this._uiService.LoadBitmap(resource.ToRecordId());

	protected override void OnPaint(PaintEventArgs e)
	{
		var g = e.Graphics;
		var arrow = this._pressed == Part.Field ? GameDropDown.ArrowState.Pressed
			: this._hovered == Part.Field && this.CanAct(LoadoutBarAction.Select) ? GameDropDown.ArrowState.Over
			: GameDropDown.ArrowState.Up;
		string text = this.Selected ?? (this.Names.Count == 0 ? Resources.LoadoutNone : string.Empty);
		GameDropDown.DrawField(g, this._uiService, this._box, this.FieldBounds, this._scale, text, this.TextFont ?? this.Font, this.Selected is null ? GameDropDown.Grey : Color.White, arrow);

		this.DrawIconButton(g, Part.Add, this.AddBounds, this.CanAct(LoadoutBarAction.Add), PlusUp, PlusOver, PlusDown);
	}

	/// <summary>
	/// A picture button of the bar, with the game picture of its state: pressed, under the mouse, or up; dimmed when it
	/// cannot be used.
	/// </summary>
	private void DrawIconButton(Graphics g, Part part, Rectangle bounds, bool enabled, string up, string over, string down)
	{
		if (!enabled)
		{
			this.DrawPicture(g, up, bounds, greyed: true);
			return;
		}

		this.DrawPicture(g, this._pressed == part ? down : this._hovered == part ? over : up, bounds);
	}

	/// <summary>A game picture stretched to the bounds; a grey frame without the game files.</summary>
	private void DrawPicture(Graphics g, string resource, Rectangle to, bool greyed = false)
		=> GamePicture.Draw(g, this.Picture(resource), to, greyed);

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
