using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// An equipment panel's slots on their stone, apart from the stash panel: the slot half of the equipment background and
/// the panel over it, both the main equipment's geometry times one factor so that the slots fall on their frames at
/// every zoom and DPI. The loadout panel (<see cref="LoadoutView"/>) and the floating character window show their slots so,
/// with the previous and next loadout arrows in the gaps of the stone (<see cref="LoadoutPanel.ButtonCells"/>).
/// </summary>
public class EquipmentSlotsView : Control
{
	private readonly IUIService _uiService;

	/// <summary>The equipment background picture, the part of it with the slots, and where the main equipment draws that
	/// part, relative to the equipment panel and at the window's scale.</summary>
	private (Bitmap Picture, Rectangle Source, Rectangle Target)? _slotsPicture;

	/// <summary>Where the slots picture is drawn in this view.</summary>
	private Rectangle _pictureBounds;

	/// <summary>The size of the slots of the equipment panel at the window's scale, as last laid out.</summary>
	private Size _slotsPanelSize;

	/// <summary>The largest factor of the main equipment's geometry, as last laid out.</summary>
	private float _maxFactor = 1F;

	/// <summary>The previous loadout arrow's pictures, with {0} for the state.</summary>
	private const string PreviousPicture = """InGameUI\characterscreen\characterrotateleft{0}01.tex""";

	/// <summary>The next loadout arrow's pictures, with {0} for the state.</summary>
	private const string NextPicture = """InGameUI\characterscreen\characterrotateright{0}01.tex""";

	/// <summary>The size of the previous and next arrows' pictures, in pixels at scale 1.</summary>
	internal static readonly Size TurnPictureSize = new(20, 14);

	private readonly GameButton _previous;
	private readonly GameButton _next;

	/// <summary>Initializes a new instance of the EquipmentSlotsView class.</summary>
	/// <param name="panel">The equipment panel shown on the stone.</param>
	/// <param name="uiService">The UI service.</param>
	public EquipmentSlotsView(EquipmentPanel panel, IUIService uiService)
	{
		this._uiService = uiService;
		this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
		this.BackColor = Color.Black;
		this.SlotsPanel = panel;
		this.Controls.Add(panel);

		this._previous = new GameButton(uiService, PreviousPicture, Resources.LoadoutPrevious) { Name = "loadoutPreviousButton" };
		this._previous.Pressed += (_, _) => this.TurnRequested?.Invoke(this, -1);
		this._next = new GameButton(uiService, NextPicture, Resources.LoadoutNext) { Name = "loadoutNextButton" };
		this._next.Pressed += (_, _) => this.TurnRequested?.Invoke(this, 1);
		foreach (var button in new[] { this._previous, this._next })
		{
			this.Controls.Add(button);
			button.BringToFront();
		}
	}

	/// <summary>The arrow that shows the previous loadout.</summary>
	internal Control PreviousButton => this._previous;

	/// <summary>The arrow that shows the next loadout.</summary>
	internal Control NextButton => this._next;

	/// <summary>Raised when the user clicks the previous (-1) or next (+1) arrow.</summary>
	public event EventHandler<int>? TurnRequested;

	/// <summary>Whether the previous and next arrows can be clicked: greyed with nothing to turn to.</summary>
	public bool CanTurn
	{
		get => this._next.Available;
		set
		{
			this._previous.Available = value;
			this._next.Available = value;
		}
	}

	/// <summary>The equipment panel on the stone.</summary>
	public EquipmentPanel SlotsPanel { get; }

	/// <summary>
	/// The size the view shows the slots picture and the slot cells in at a factor of 1 (the main equipment's size).
	/// </summary>
	/// <param name="target">Where the main equipment draws the slots picture, relative to the equipment panel's corner.</param>
	/// <param name="slotsPanelSize">The size of the slot cells at the window's scale.</param>
	public static Size RegionSize(Rectangle target, Size slotsPanelSize)
		=> Rectangle.Union(target, new Rectangle(Point.Empty, slotsPanelSize)).Size;

	/// <summary>
	/// Lays the view out in its bounds: the slots picture and the panel take the largest factor of the main equipment's
	/// geometry that fits (whole-pixel cells), at most <paramref name="maxFactor"/>, centred; then <see cref="PlaceExtras"/>.
	/// </summary>
	/// <param name="picture">The equipment background picture.</param>
	/// <param name="source">The part of the picture with the slots.</param>
	/// <param name="target">Where the main equipment draws that part, relative to the equipment panel's corner.</param>
	/// <param name="slotsPanelSize">The size of the slots of the equipment panel (<see cref="LoadoutPanel.SlotCells"/>) at the window's scale.</param>
	/// <param name="maxFactor">The largest factor: 1 in the main window, more in a window on a monitor of a higher DPI.</param>
	public void LayOut(Bitmap picture, Rectangle source, Rectangle target, Size slotsPanelSize, float maxFactor = 1F)
	{
		this._slotsPicture = (picture, source, target);
		this._slotsPanelSize = slotsPanelSize;
		this._maxFactor = maxFactor;
		if (this.Width <= 0 || this.Height <= 0)
			return;

		// The region to show, relative to the equipment panel: the slots picture and the slot cells.
		var region = Rectangle.Union(target, new Rectangle(Point.Empty, slotsPanelSize));
		int unit = this._uiService.ItemUnitSize;
		float fit = Math.Min(this.Width / (float)region.Width, this.Height / (float)region.Height);

		// Whole-pixel cells: the factor that gives the largest cell that fits.
		int cell = Math.Max(1, (int)Math.Floor(unit * Math.Min(fit, maxFactor) + 0.001F));
		float factor = cell / (float)unit;
		this.SlotsPanel.CellScaleFactor = factor;
		this.SlotsPanel.FollowScale();

		var offset = new Point(
			(this.Width - Convert.ToInt32(region.Width * factor)) / 2,
			(this.Height - Convert.ToInt32(region.Height * factor)) / 2);
		Point Scaled(Point p) => new(offset.X + Convert.ToInt32((p.X - region.X) * factor), offset.Y + Convert.ToInt32((p.Y - region.Y) * factor));

		var panelLocation = Scaled(Point.Empty);
		if (this.SlotsPanel.Location != panelLocation)
			this.SlotsPanel.Location = panelLocation;

		var pictureLocation = Scaled(target.Location);
		this._pictureBounds = new Rectangle(pictureLocation, new Size(Convert.ToInt32(target.Width * factor), Convert.ToInt32(target.Height * factor)));

		this.PlaceExtras(panelLocation);
		this.Invalidate();
	}

	/// <summary>
	/// Places what the view adds over the stone, once the panel is at <paramref name="panelLocation"/>: the previous and
	/// next arrows left of the left column and right of the right column, level with their bottom.
	/// </summary>
	protected virtual void PlaceExtras(Point panelLocation)
	{
		var (_, previous, next, _, _) = LoadoutPanel.ButtonCells();
		this.PlaceOnStone(this._previous, previous, panelLocation);
		this.PlaceOnStone(this._next, next, panelLocation);
	}

	/// <summary>Puts <paramref name="control"/> on the cells <paramref name="cells"/> of the panel at <paramref name="panelLocation"/>.</summary>
	protected void PlaceOnStone(Control control, RectangleF cells, Point panelLocation)
	{
		var bounds = this.SlotsPanel.CellsToPixels(cells);
		bounds.Offset(panelLocation);
		if (control.Bounds != bounds)
			control.Bounds = bounds;
	}

	/// <summary>Resized by its owner: laid out again in its new bounds.</summary>
	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		if (this._slotsPicture is var (picture, source, target))
			this.LayOut(picture, source, target, this._slotsPanelSize, this._maxFactor);
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
}
