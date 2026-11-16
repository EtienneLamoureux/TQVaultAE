using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// The loadout panel in place of the statistics panel: the slot half of the equipment background and the
/// <see cref="LoadoutPanel"/> over it, both the main equipment's geometry times one factor so that the slots fall on
/// their frames at every zoom and DPI, with the game's small buttons in the gaps of the stone: Back (returns to the
/// statistics) in the upper right, previous and next loadout in the lower left and right, Gear swap under the rings.
/// </summary>
public sealed class LoadoutView : Control
{
	private readonly IUIService _uiService;
	private readonly GameButton _back;
	private readonly GameButton _previous;
	private readonly GameButton _next;
	private readonly GameButton _swap;

	/// <summary>The equipment background picture, the part of it with the slots, and where the main equipment draws that
	/// part, relative to the equipment panel and at the window's scale.</summary>
	private (Bitmap Picture, Rectangle Source, Rectangle Target)? _slotsPicture;

	/// <summary>Where the slots picture is drawn in this view.</summary>
	private Rectangle _pictureBounds;

	/// <summary>The size of the slots of the equipment panel at the window's scale, as last laid out.</summary>
	private Size _slotsPanelSize;

	/// <summary>Initializes a new instance of the LoadoutView class.</summary>
	/// <param name="dragInfo">The drag shared by every panel.</param>
	/// <param name="services">The services.</param>
	public LoadoutView(ItemDragInfo dragInfo, GuiServices services)
	{
		this._uiService = services.UIService;
		this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
		this.BackColor = Color.Black;
		this.Name = "loadoutView";

		this.Panel = new LoadoutPanel(dragInfo, services);
		this._back = new GameButton(this._uiService, """InGameUI\tutorial\buttonback{0}01.tex""", Resources.LoadoutBack) { Name = "loadoutBackButton" };
		this._back.Pressed += (_, _) => this.BackRequested?.Invoke(this, EventArgs.Empty);
		this._previous = new GameButton(this._uiService, """InGameUI\characterscreen\characterrotateleft{0}01.tex""", Resources.LoadoutPrevious) { Name = "loadoutPreviousButton" };
		this._previous.Pressed += (_, _) => this.TurnRequested?.Invoke(this, -1);
		this._next = new GameButton(this._uiService, """InGameUI\characterscreen\characterrotateright{0}01.tex""", Resources.LoadoutNext) { Name = "loadoutNextButton" };
		this._next.Pressed += (_, _) => this.TurnRequested?.Invoke(this, 1);

		// The game has no disable picture of this tile (greyed instead); its lighter over picture shows at rest, the up one under the mouse.
		this._swap = new GameButton(this._uiService, """InGameUI\characterscreen\weaponswapbtn{0}01.tex""", Resources.LoadoutSwap, hasDisable: false, restState: "over", hoverState: "up")
		{
			Name = "loadoutSwapButton",
		};
		this._swap.Pressed += (_, _) => this.SwapRequested?.Invoke(this, EventArgs.Empty);
		this.Controls.Add(this.Panel);
		foreach (var button in new[] { this._back, this._previous, this._next, this._swap })
		{
			this.Controls.Add(button);
			button.BringToFront();
		}
	}

	/// <summary>The loadout panel.</summary>
	public LoadoutPanel Panel { get; }

	/// <summary>The Back arrow, which returns to the statistics panel.</summary>
	internal Control BackButton => this._back;

	/// <summary>The arrow that shows the previous loadout.</summary>
	internal Control PreviousButton => this._previous;

	/// <summary>The arrow that shows the next loadout.</summary>
	internal Control NextButton => this._next;

	/// <summary>The Gear swap button, which exchanges the loadout's pieces and the worn ones.</summary>
	internal Control SwapButton => this._swap;

	/// <summary>Raised when the user clicks the Gear swap button.</summary>
	public event EventHandler? SwapRequested;

	/// <summary>Whether the Gear swap button can be clicked: greyed when the loadout bar's Gear swap is.</summary>
	public bool CanSwap
	{
		get => this._swap.Enabled;
		set => this._swap.Enabled = value;
	}

	/// <summary>Raised when the user clicks the Back arrow.</summary>
	public event EventHandler? BackRequested;

	/// <summary>Raised when the user clicks the previous (-1) or next (+1) arrow.</summary>
	public event EventHandler<int>? TurnRequested;

	/// <summary>Whether the previous and next arrows can be clicked: greyed with a single loadout.</summary>
	public bool CanTurn
	{
		get => this._next.Enabled;
		set
		{
			this._previous.Enabled = value;
			this._next.Enabled = value;
		}
	}

	/// <summary>
	/// Lays the view out in its bounds: the slots picture and the loadout panel take the largest factor of the main
	/// equipment's geometry that fits (whole-pixel cells), centred; the buttons sit in the gaps of the stone.
	/// </summary>
	/// <param name="picture">The equipment background picture.</param>
	/// <param name="source">The part of the picture with the slots.</param>
	/// <param name="target">Where the main equipment draws that part, relative to the equipment panel's corner.</param>
	/// <param name="slotsPanelSize">The size of the slots of the equipment panel (<see cref="LoadoutPanel.SlotCells"/>) at the window's scale.</param>
	public void LayOut(Bitmap picture, Rectangle source, Rectangle target, Size slotsPanelSize)
	{
		this._slotsPicture = (picture, source, target);
		this._slotsPanelSize = slotsPanelSize;
		if (this.Width <= 0 || this.Height <= 0)
			return;

		// The region to show, relative to the equipment panel: the slots picture and the slot cells.
		var region = Rectangle.Union(target, new Rectangle(Point.Empty, slotsPanelSize));
		int unit = this._uiService.ItemUnitSize;
		float fit = Math.Min(this.Width / (float)region.Width, this.Height / (float)region.Height);

		// Whole-pixel cells: the factor that gives the largest cell that fits.
		int cell = Math.Max(1, (int)Math.Floor(unit * Math.Min(fit, 1F)));
		float factor = cell / (float)unit;
		this.Panel.CellScaleFactor = factor;
		this.Panel.FollowScale();

		var offset = new Point(
			(this.Width - Convert.ToInt32(region.Width * factor)) / 2,
			(this.Height - Convert.ToInt32(region.Height * factor)) / 2);
		Point Scaled(Point p) => new(offset.X + Convert.ToInt32((p.X - region.X) * factor), offset.Y + Convert.ToInt32((p.Y - region.Y) * factor));

		var panelLocation = Scaled(Point.Empty);
		if (this.Panel.Location != panelLocation)
			this.Panel.Location = panelLocation;

		var pictureLocation = Scaled(target.Location);
		this._pictureBounds = new Rectangle(pictureLocation, new Size(Convert.ToInt32(target.Width * factor), Convert.ToInt32(target.Height * factor)));

		// The buttons in the gaps of the stone, at the panel's cell size.
		var (back, previous, next, swap) = LoadoutPanel.ButtonCells();
		void Place(Control button, RectangleF cells)
		{
			var bounds = this.Panel.CellsToPixels(cells);
			bounds.Offset(panelLocation);
			if (button.Bounds != bounds)
				button.Bounds = bounds;
		}

		Place(this._back, back);
		Place(this._previous, previous);
		Place(this._next, next);
		Place(this._swap, swap);

		this.Invalidate();
	}

	/// <summary>Resized by its room: laid out again in its new bounds.</summary>
	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		if (this._slotsPicture is var (picture, source, target))
			this.LayOut(picture, source, target, this._slotsPanelSize);
	}

	/// <inheritdoc/>
	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		if (this._slotsPicture is not var (picture, source, _) || this._pictureBounds.IsEmpty)
			return;

		var g = e.Graphics;
		g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
		g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
		using var attributes = new System.Drawing.Imaging.ImageAttributes();

		// No blending with the columns right of the slots.
		attributes.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
		g.DrawImage(picture, this._pictureBounds, source.X, source.Y, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
	}

	/// <summary>
	/// A small button drawn with one of the game's button pictures, in its up, over, down and disable states; its action
	/// is raised from the mouse up (a synthetic click from the automation endpoint works too).
	/// </summary>
	private sealed class GameButton : Control
	{
		private readonly IUIService _uiService;
		private readonly string _resource;
		private readonly bool _hasDisable;
		private readonly string _restState;
		private readonly string _hoverState;
		private readonly ToolTip _toolTip = new();
		private bool _hovered;
		private bool _pressed;

		/// <summary>Raised when the left button is released over the button it went down on.</summary>
		internal event EventHandler? Pressed;

		/// <param name="uiService">The UI service, which loads the pictures.</param>
		/// <param name="resource">The pictures' id, with {0} for the state: up, over, down or disable.</param>
		/// <param name="text">The tool tip and accessible name.</param>
		/// <param name="hasDisable">False when the game has no disable picture: the rest picture is greyed instead.</param>
		/// <param name="restState">The state whose picture shows at rest.</param>
		/// <param name="hoverState">The state whose picture shows under the mouse.</param>
		internal GameButton(IUIService uiService, string resource, string text, bool hasDisable = true, string restState = "up", string hoverState = "over")
		{
			this._uiService = uiService;
			this._resource = resource;
			this._hasDisable = hasDisable;
			this._restState = restState;
			this._hoverState = hoverState;
			this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
			this.BackColor = Color.Transparent;
			this.Cursor = Cursors.Hand;
			this.AccessibleName = text;
			this._toolTip.SetToolTip(this, text);
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			this.Cursor = this.Enabled ? Cursors.Hand : Cursors.Default;
			this._hovered = this._pressed = false;
			this.Invalidate();
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			this._hovered = true;
			this.Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			this._hovered = false;
			this._pressed = false;
			this.Invalidate();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button != MouseButtons.Left)
				return;

			this._pressed = true;
			this.Invalidate();
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			bool clicked = this._pressed && this.Enabled && e.Button == MouseButtons.Left && this.ClientRectangle.Contains(e.Location);
			this._pressed = false;
			this.Invalidate();
			if (clicked)
				this.Pressed?.Invoke(this, EventArgs.Empty);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			bool greyed = !this.Enabled && !this._hasDisable;
			string state = !this.Enabled ? (greyed ? this._restState : "disable") : this._pressed ? "down" : this._hovered ? this._hoverState : this._restState;
			string resource = string.Format(System.Globalization.CultureInfo.InvariantCulture, this._resource, state);
			e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			GamePicture.Draw(e.Graphics, this._uiService.LoadBitmap(resource.ToRecordId()), this.ClientRectangle, greyed);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				this._toolTip.Dispose();

			base.Dispose(disposing);
		}
	}
}
