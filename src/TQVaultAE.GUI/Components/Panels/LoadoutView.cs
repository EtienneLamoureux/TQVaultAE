using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// The loadout panel in place of the statistics panel: the slot half of the equipment background and the
/// <see cref="LoadoutPanel"/> over it, both the main equipment's geometry times one factor so that the slots fall on
/// their frames at every zoom and DPI, and a small Back arrow in the upper right corner that returns to the statistics.
/// </summary>
public sealed class LoadoutView : Control
{
	private readonly IUIService _uiService;
	private readonly BackArrow _back;

	/// <summary>The equipment background picture, the part of it with the slots, and where the main equipment draws that
	/// part, relative to the equipment panel and at the window's scale.</summary>
	private (Bitmap Picture, Rectangle Source, Rectangle Target)? _slotsPicture;

	/// <summary>Where the slots picture is drawn in this view.</summary>
	private Rectangle _pictureBounds;

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
		this._back = new BackArrow(this._uiService) { Name = "loadoutBackButton" };
		this._back.Pressed += (_, _) => this.BackRequested?.Invoke(this, EventArgs.Empty);
		this.Controls.Add(this.Panel);
		this.Controls.Add(this._back);
		this._back.BringToFront();
	}

	/// <summary>The loadout panel.</summary>
	public LoadoutPanel Panel { get; }

	/// <summary>The Back arrow, which returns to the statistics panel.</summary>
	internal Control BackButton => this._back;

	/// <summary>Raised when the user clicks the Back arrow.</summary>
	public event EventHandler? BackRequested;

	/// <summary>
	/// Lays the view out in its bounds: the slots picture and the loadout panel take the largest factor of the main
	/// equipment's geometry that fits (whole-pixel cells), centred; the Back arrow sits in the upper right corner.
	/// </summary>
	/// <param name="picture">The equipment background picture.</param>
	/// <param name="source">The part of the picture with the slots.</param>
	/// <param name="target">Where the main equipment draws that part, relative to the equipment panel's corner.</param>
	/// <param name="slotsPanelSize">The size of the slots of the equipment panel (<see cref="LoadoutPanel.SlotCells"/>) at the window's scale.</param>
	public void LayOut(Bitmap picture, Rectangle source, Rectangle target, Size slotsPanelSize)
	{
		this._slotsPicture = (picture, source, target);

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

		// The game's "back" arrow, at the height of the loadout bar's Load arrow.
		float scale = this._uiService.Scale;
		var arrow = new Size(Convert.ToInt32(BackArrow.DesignSize.Width * scale), Convert.ToInt32(BackArrow.DesignSize.Height * scale));
		int margin = Convert.ToInt32(4 * scale);
		var backBounds = new Rectangle(new Point(this.Width - arrow.Width - margin, margin), arrow);
		if (this._back.Bounds != backBounds)
			this._back.Bounds = backBounds;

		this.Invalidate();
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

	/// <summary>The Back arrow: the game's gold "back" arrow, with its hover and pressed pictures.</summary>
	private sealed class BackArrow : Control
	{
		private const string Up = """InGameUI\tutorial\buttonbackup01.tex""";
		private const string Over = """InGameUI\tutorial\buttonbackover01.tex""";
		private const string Down = """InGameUI\tutorial\buttonbackdown01.tex""";

		/// <summary>The size at scale 1: the 16 x 20 picture, a little larger than the loadout bar's Load arrow.</summary>
		internal static readonly Size DesignSize = new(20, 25);

		private readonly IUIService _uiService;
		private readonly ToolTip _toolTip = new();
		private bool _hovered;
		private bool _pressed;

		/// <summary>Raised when the left button is released over the arrow it went down on.</summary>
		internal event EventHandler? Pressed;

		internal BackArrow(IUIService uiService)
		{
			this._uiService = uiService;
			this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
			this.BackColor = Color.Transparent;
			this.Cursor = Cursors.Hand;
			this.AccessibleName = Resources.LoadoutBack;
			this._toolTip.SetToolTip(this, Resources.LoadoutBack);
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
			bool clicked = this._pressed && e.Button == MouseButtons.Left && this.ClientRectangle.Contains(e.Location);
			this._pressed = false;
			this.Invalidate();
			if (clicked)
				this.Pressed?.Invoke(this, EventArgs.Empty);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			string resource = this._pressed ? Down : this._hovered ? Over : Up;
			GamePicture.Draw(e.Graphics, this._uiService.LoadBitmap(resource.ToRecordId()), this.ClientRectangle);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				this._toolTip.Dispose();

			base.Dispose(disposing);
		}
	}
}
