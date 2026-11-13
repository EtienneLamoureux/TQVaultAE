using TQVaultAE.Domain.Helpers;
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

	/// <summary>Records the worn equipment as a new loadout ([+] button).</summary>
	Add,

	/// <summary>Loads the selected loadout (Load button or menu).</summary>
	Load,

	/// <summary>Records the worn equipment over the selected loadout.</summary>
	Update,

	/// <summary>Renames the selected loadout.</summary>
	Rename,

	/// <summary>Copies the selected loadout under a new name.</summary>
	Copy,

	/// <summary>Deletes the selected loadout.</summary>
	Delete,
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
/// box of the character window, the [+] button of the attributes to add one, and the game's gold "next" arrow to load
/// the chosen one. The right-click menu holds every action. Everything is drawn at the panel's scale.
/// </summary>
public sealed class LoadoutBar : Control
{
	private const string ArrowUp = """InGameUI\quest\arrowdownsmallup.tex""";
	private const string ArrowOver = """InGameUI\quest\arrowdownsmallover.tex""";
	private const string ArrowDown = """InGameUI\quest\arrowdownsmalldown.tex""";
	private const string PlusUp = """InGameUI\CharacterScreen\StatIncrementButtonUp01.tex""";
	private const string PlusOver = """InGameUI\CharacterScreen\StatIncrementButtonOver01.tex""";
	private const string PlusDown = """InGameUI\CharacterScreen\StatIncrementButtonDown01.tex""";
	private const string LoadUp = """InGameUI\tutorial\buttonnextup01.tex""";
	private const string LoadOver = """InGameUI\tutorial\buttonnextover01.tex""";
	private const string LoadDown = """InGameUI\tutorial\buttonnextdown01.tex""";
	private const string LoadDisabled = """InGameUI\tutorial\buttonnextdisable01.tex""";

	/// <summary>Width over height of the "next" arrow pictures (16 x 20).</summary>
	private const float LoadAspect = 16F / 20F;

	/// <summary>Height of the bar at scale 1: the gold box of the character window.</summary>
	public const int ReferenceHeight = 22;

	/// <summary>Corner of the gold box picture kept unstretched.</summary>
	private const int BoxCorner = 4;

	private static readonly Color Grey = Color.FromArgb(190, 190, 190);

	private readonly IUIService _uiService;
	private readonly Bitmap _box = Images.CharacterNumberBox;
	private readonly ToolTip _toolTip = new();

	private enum Part { None, Field, Add, Load }

	private Part _hovered;
	private Part _pressed;
	private float _scale = 1F;

	public LoadoutBar(IUIService uiService)
	{
		this._uiService = uiService;
		this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
		this.BackColor = Color.Black;
		this.ForeColor = Color.White;

		this.ListMenu = new ContextMenuStrip { ShowCheckMargin = true, ShowImageMargin = false };
		this.ActionMenu = new ContextMenuStrip { ShowImageMargin = false };
		CustomProfessionalRenderer.Apply(this.ListMenu);
		CustomProfessionalRenderer.Apply(this.ActionMenu);
		this.ListMenu.ItemClicked += (_, e) =>
		{
			if (e.ClickedItem?.Tag is string name)
				this.Raise(LoadoutBarAction.Select, name);
		};
		this.ActionMenu.ItemClicked += (_, e) =>
		{
			if (e.ClickedItem?.Tag is LoadoutBarAction action)
				this.Raise(action, action == LoadoutBarAction.Add ? null : this.Selected);
		};
		this.ActionMenu.Opening += (_, _) => this.FillActionMenu();
		this.ListMenu.Opening += (_, _) => this.FillListMenu();
		this.ContextMenuStrip = this.ActionMenu;
	}

	/// <summary>Raised for each action asked; the owner does it and shows the result with <see cref="ShowLoadouts"/>.</summary>
	public event EventHandler<LoadoutBarEventArgs>? ActionRequested;

	/// <summary>The drop-down list of the loadout names.</summary>
	internal ContextMenuStrip ListMenu { get; }

	/// <summary>The right-click menu: every action.</summary>
	internal ContextMenuStrip ActionMenu { get; }

	/// <summary>The loadout names, in the order of the file.</summary>
	public IReadOnlyList<string> Names { get; private set; } = [];

	/// <summary>The selected loadout, or null.</summary>
	public string? Selected { get; private set; }

	/// <summary>False for a read-only character (or a file that could not be read): the loadouts are only listed.</summary>
	public bool CanChange { get; private set; }

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
	public void ShowLoadouts(IReadOnlyList<string> names, string? selected, bool canChange)
	{
		selected = names.FirstOrDefault(n => string.Equals(n, selected, StringComparison.OrdinalIgnoreCase)) ?? (names.Count > 0 ? names[0] : null);
		if (canChange == this.CanChange && selected == this.Selected && names.SequenceEqual(this.Names, StringComparer.Ordinal))
			return;

		this.Names = names;
		this.Selected = selected;
		this.CanChange = canChange;
		this._toolTip.SetToolTip(this, canChange ? null : Resources.LoadoutReadOnly);
		this.Invalidate();
	}

	/// <summary>Height of the bar at a scale.</summary>
	public static int HeightAt(float scale) => Convert.ToInt32(ReferenceHeight * scale);

	private int Gap => Math.Max(1, Convert.ToInt32(4 * this._scale));

	/// <summary>Height of the picture buttons ([+] and Load): the bar's height less a margin.</summary>
	private int IconHeight => Math.Max(1, this.Height - Convert.ToInt32(4 * this._scale));

	/// <summary>The Load button, the "next" arrow at the height of the [+] button, at the right end.</summary>
	internal Rectangle LoadBounds
	{
		get
		{
			int height = this.IconHeight;
			int width = Math.Max(1, Convert.ToInt32(height * LoadAspect));
			return new Rectangle(this.Width - width, (this.Height - height) / 2, width, height);
		}
	}

	/// <summary>The [+] button, left of the Load button.</summary>
	internal Rectangle AddBounds
	{
		get
		{
			int size = this.IconHeight;
			return new Rectangle(this.LoadBounds.Left - this.Gap - size, (this.Height - size) / 2, size, size);
		}
	}

	/// <summary>The drop-down box: the rest of the width.</summary>
	internal Rectangle FieldBounds => new(0, 0, Math.Max(this.Height, this.AddBounds.Left - this.Gap), this.Height);

	private Rectangle ArrowBounds
	{
		get
		{
			var box = this.FieldBounds;
			int size = Math.Max(1, box.Height - Convert.ToInt32(6 * this._scale));
			return new Rectangle(box.Right - size - Convert.ToInt32(3 * this._scale), (box.Height - size) / 2, size, size);
		}
	}

	private bool CanAct(LoadoutBarAction action) => action switch
	{
		LoadoutBarAction.Select => this.Names.Count > 0,
		LoadoutBarAction.Add => this.CanChange,
		_ => this.CanChange && this.Selected is not null,
	};

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

	/// <summary>Asks for an action as the user would (for the automation): refused when the bar does not offer it.</summary>
	internal bool Perform(LoadoutBarAction action, string? name = null)
	{
		if (!this.CanAct(action))
			return false;

		this.Raise(action, action == LoadoutBarAction.Select ? name : action == LoadoutBarAction.Add ? null : this.Selected);
		return true;
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
			=> new(text) { Tag = action, Enabled = this.CanAct(action) };

		this.ActionMenu.Items.AddRange(
		[
			Entry(Resources.LoadoutLoad, LoadoutBarAction.Load),
			Entry(Resources.LoadoutUpdate, LoadoutBarAction.Update),
			Entry(Resources.LoadoutRename, LoadoutBarAction.Rename),
			Entry(Resources.LoadoutCopy, LoadoutBarAction.Copy),
			Entry(Resources.LoadoutDelete, LoadoutBarAction.Delete),
			new ToolStripSeparator(),
			Entry(Resources.LoadoutAddMenu, LoadoutBarAction.Add),
		]);
	}

	private Font MenuFont() => this.Parent?.Font ?? this.Font;

	private Part PartAt(Point point)
		=> this.LoadBounds.Contains(point) ? Part.Load
		: this.AddBounds.Contains(point) ? Part.Add
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
			Part.Load => Resources.LoadoutLoadTip,
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
		else if (pressed == Part.Load)
			this.Raise(LoadoutBarAction.Load, this.Selected);
	}

	private Bitmap? Picture(string resource) => this._uiService.LoadBitmap(resource.ToRecordId());

	protected override void OnPaint(PaintEventArgs e)
	{
		var g = e.Graphics;
		g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
		g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

		var box = this.FieldBounds;
		this.DrawBox(g, box);

		var arrow = this.ArrowBounds;
		bool canSelect = this.CanAct(LoadoutBarAction.Select);
		// The picture's triangle points up: drawn upside down, it points to the list it opens.
		this.DrawPicture(g, this._pressed == Part.Field ? ArrowDown : this._hovered == Part.Field && canSelect ? ArrowOver : ArrowUp, arrow, flip: true);

		var font = this.TextFont ?? this.Font;
		var textBounds = Rectangle.FromLTRB(box.Left + Convert.ToInt32(6 * this._scale), box.Top, arrow.Left - Convert.ToInt32(2 * this._scale), box.Bottom);
		string text = this.Selected ?? (this.Names.Count == 0 ? Resources.LoadoutNone : string.Empty);
		TextRenderer.DrawText(g, text, font, textBounds, this.Selected is null ? Grey : Color.White
			, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);

		this.DrawIconButton(g, Part.Add, this.AddBounds, this.CanAct(LoadoutBarAction.Add), PlusUp, PlusOver, PlusDown);
		this.DrawIconButton(g, Part.Load, this.LoadBounds, this.CanAct(LoadoutBarAction.Load), LoadUp, LoadOver, LoadDown, LoadDisabled);
	}

	/// <summary>
	/// One of the bar's picture buttons, with the game picture of its state: pressed, under the mouse, or up; when it
	/// cannot be used, its disabled picture, or the up picture dimmed for a button that has none.
	/// </summary>
	private void DrawIconButton(Graphics g, Part part, Rectangle bounds, bool enabled, string up, string over, string down, string? disabled = null)
	{
		if (!enabled)
		{
			this.DrawPicture(g, disabled ?? up, bounds, greyed: disabled is null);
			return;
		}

		this.DrawPicture(g, this._pressed == part ? down : this._hovered == part ? over : up, bounds);
	}

	/// <summary>The gold box of the character window, its corners kept and its edges stretched to the bounds.</summary>
	private void DrawBox(Graphics g, Rectangle to)
	{
		var box = this._box;
		int c = BoxCorner;
		int cs = Math.Max(1, Convert.ToInt32(c * this._scale));
		void Part(Rectangle dest, int x, int y, int w, int h) => g.DrawImage(box, dest, x, y, w, h, GraphicsUnit.Pixel);

		int midW = box.Width - (2 * c), midH = box.Height - (2 * c);
		int left = to.Left, top = to.Top, right = to.Right - cs, bottom = to.Bottom - cs;
		int innerW = Math.Max(0, to.Width - (2 * cs)), innerH = Math.Max(0, to.Height - (2 * cs));

		Part(new Rectangle(left, top, cs, cs), 0, 0, c, c);
		Part(new Rectangle(left + cs, top, innerW, cs), c, 0, midW, c);
		Part(new Rectangle(right, top, cs, cs), box.Width - c, 0, c, c);
		Part(new Rectangle(left, top + cs, cs, innerH), 0, c, c, midH);
		Part(new Rectangle(left + cs, top + cs, innerW, innerH), c, c, midW, midH);
		Part(new Rectangle(right, top + cs, cs, innerH), box.Width - c, c, c, midH);
		Part(new Rectangle(left, bottom, cs, cs), 0, box.Height - c, c, c);
		Part(new Rectangle(left + cs, bottom, innerW, cs), c, box.Height - c, midW, c);
		Part(new Rectangle(right, bottom, cs, cs), box.Width - c, box.Height - c, c, c);
	}

	/// <summary>A game picture stretched to the bounds; a grey frame without the game files.</summary>
	private void DrawPicture(Graphics g, string resource, Rectangle to, bool greyed = false, bool flip = false)
	{
		if (this.Picture(resource) is not { } picture)
		{
			using var pen = new Pen(Grey);
			g.DrawRectangle(pen, to.X, to.Y, to.Width - 1, to.Height - 1);
			return;
		}

		if (flip)
		{
			g.DrawImage(picture, [new Point(to.Left, to.Bottom), new Point(to.Right, to.Bottom), new Point(to.Left, to.Top)]);
			return;
		}

		if (!greyed)
		{
			g.DrawImage(picture, to);
			return;
		}

		using var attributes = new System.Drawing.Imaging.ImageAttributes();
		var dim = new System.Drawing.Imaging.ColorMatrix { Matrix33 = 0.4F };
		attributes.SetColorMatrix(dim);
		g.DrawImage(picture, to, 0, 0, picture.Width, picture.Height, GraphicsUnit.Pixel, attributes);
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
