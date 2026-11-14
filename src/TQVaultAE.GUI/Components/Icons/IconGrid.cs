using System.ComponentModel;
using System.Drawing.Drawing2D;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation.Win32.Models;

namespace TQVaultAE.GUI.Components.Icons;

/// <summary>
/// Grid of icons drawn by a single control, with a vertical scroll bar.
/// </summary>
/// <remarks>
/// Replaces one <see cref="PictureBox"/> per icon: thousands of native controls took seconds to create and lay out.
/// Only the visible rows are drawn, each icon decoded and reduced to a thumbnail the first time it is shown.
/// </remarks>
public class IconGrid : Control
{
	/// <summary>One picture of an icon: its On, Off or Over bitmap.</summary>
	public sealed record Cell(IconInfo Info, RecordId Id)
	{
		/// <summary>The picture, decoded the first time it is drawn.</summary>
		public Bitmap Bitmap => this.Info.BitmapOf(this.Id);
	}

	/// <summary>Size of a drawn icon, border included, at 96 DPI, as the former picture boxes.</summary>
	private const int DefaultIconSize = 32;

	/// <summary>Space around each icon at <see cref="DefaultIconSize"/>, as the former picture box margins.</summary>
	private const int DefaultSpacing = 3;

	private int _iconSize = DefaultIconSize;

	/// <summary>
	/// Size of a drawn icon in pixels, border included. The space around each icon follows it, in the proportion of the
	/// former picture box margins.
	/// </summary>
	[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int IconSize
	{
		get => this._iconSize;
		set
		{
			value = Math.Max(3, value);
			if (value == this._iconSize)
				return;

			this._iconSize = value;
			this.ClearThumbnails();
			this.SetHover(-1);
			this.UpdateScroll();
			this.Invalidate();
		}
	}

	/// <summary>Space around each icon.</summary>
	private int Spacing => Math.Max(1, (int)Math.Round(this._iconSize * (float)DefaultSpacing / DefaultIconSize));

	private int Pitch => this._iconSize + 2 * this.Spacing;

	private static readonly Color BorderColor = Color.FromArgb(10, 8, 6);
	private static readonly Color HoverColor = Color.Gold;

	private readonly List<Cell> _cells = [];
	private readonly List<Cell> _shown = [];
	private readonly Dictionary<Bitmap, Bitmap> _thumbnails = [];
	private readonly VScrollBar _scroll = new() { Dock = DockStyle.Right, Visible = false, TabStop = false };
	private Func<Cell, bool>? _filter;
	private int _hoverIndex = -1;

	public IconGrid()
	{
		this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint
			| ControlStyles.ResizeRedraw, true);
		this._scroll.ValueChanged += (_, _) => this.Invalidate();
		this.Controls.Add(this._scroll);
	}

	/// <summary>Kept for the designer, which sets it on the former flow layout panels: the grid always scrolls.</summary>
	[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool AutoScroll { get; set; }

	/// <summary>Raised when the pointer moves onto another icon, or off every icon (<c>null</c>).</summary>
	public event EventHandler<Cell?>? HoverChanged;

	/// <summary>Icons shown, in order.</summary>
	public void SetCells(IEnumerable<Cell> cells)
	{
		this._cells.Clear();
		this._cells.AddRange(cells);
		this.Refilter();
	}

	/// <summary>Icons kept by the grid; all of them when <c>null</c>.</summary>
	[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Func<Cell, bool>? Filter
	{
		get => this._filter;
		set
		{
			this._filter = value;
			this.Refilter();
		}
	}

	/// <summary>Icon under <paramref name="point"/>, in client coordinates.</summary>
	public Cell? CellAt(Point point)
	{
		int index = this.IndexAt(point);
		return index < 0 ? null : this._shown[index];
	}

	/// <summary>Bounds of the icon <paramref name="cell"/>, in client coordinates.</summary>
	public Rectangle BoundsOf(Cell cell)
	{
		int index = this._shown.IndexOf(cell);
		return index < 0 ? Rectangle.Empty : this.BoundsOf(index);
	}

	private int Columns => Math.Max(1, (this.ClientSize.Width - (this._scroll.Visible ? this._scroll.Width : 0)) / Pitch);

	private int Rows => (this._shown.Count + this.Columns - 1) / this.Columns;

	private Rectangle BoundsOf(int index)
	{
		int columns = this.Columns;
		return new Rectangle(
			index % columns * Pitch + Spacing,
			index / columns * Pitch + Spacing - this._scroll.Value,
			IconSize, IconSize);
	}

	private int IndexAt(Point point)
	{
		if (point.X < 0 || point.Y < 0 || point.X >= this.ClientSize.Width - (this._scroll.Visible ? this._scroll.Width : 0))
			return -1;

		int column = point.X / Pitch;
		int row = (point.Y + this._scroll.Value) / Pitch;
		if (column >= this.Columns)
			return -1;

		int index = row * this.Columns + column;
		if (index >= this._shown.Count || !this.BoundsOf(index).Contains(point))
			return -1;

		return index;
	}

	private void Refilter()
	{
		this._shown.Clear();
		this._shown.AddRange(this._filter is null ? this._cells : this._cells.Where(this._filter));
		this.SetHover(-1);
		this.UpdateScroll();
		this.Invalidate();
	}

	private void UpdateScroll()
	{
		// The bar takes room from the columns, so check with it shown when the rows do not fit without it.
		this._scroll.Visible = false;
		if (this.Rows * Pitch > this.ClientSize.Height)
			this._scroll.Visible = true;

		int content = this.Rows * Pitch;
		int page = Math.Max(1, this.ClientSize.Height);
		this._scroll.Minimum = 0;
		this._scroll.SmallChange = Pitch;
		this._scroll.LargeChange = page;
		this._scroll.Maximum = Math.Max(0, content - 1);
		// The highest value a scroll bar reaches is Maximum - LargeChange + 1.
		this._scroll.Value = Math.Min(this._scroll.Value, Math.Max(0, content - page));
	}

	private void SetHover(int index)
	{
		if (index == this._hoverIndex)
			return;

		if (this._hoverIndex >= 0 && this._hoverIndex < this._shown.Count)
			this.Invalidate(Rectangle.Inflate(this.BoundsOf(this._hoverIndex), 1, 1));
		this._hoverIndex = index;
		if (index >= 0)
			this.Invalidate(Rectangle.Inflate(this.BoundsOf(index), 1, 1));

		this.HoverChanged?.Invoke(this, index < 0 ? null : this._shown[index]);
	}

	/// <summary>The icon reduced to <see cref="IconSize"/>, keeping its aspect ratio, as a zoomed picture box did.</summary>
	private Bitmap ThumbnailOf(Bitmap bitmap)
	{
		if (this._thumbnails.TryGetValue(bitmap, out var thumbnail))
			return thumbnail;

		int inner = this._iconSize - 2;// inside the border
		float ratio = Math.Min((float)inner / bitmap.Width, (float)inner / bitmap.Height);
		int width = Math.Max(1, (int)Math.Round(bitmap.Width * ratio));
		int height = Math.Max(1, (int)Math.Round(bitmap.Height * ratio));

		thumbnail = new Bitmap(inner, inner, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
		using (var g = Graphics.FromImage(thumbnail))
		{
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			g.DrawImage(bitmap, (inner - width) / 2, (inner - height) / 2, width, height);
		}

		this._thumbnails[bitmap] = thumbnail;
		return thumbnail;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		if (this._shown.Count == 0)
			return;

		int columns = this.Columns;
		int firstRow = Math.Max(0, (e.ClipRectangle.Top + this._scroll.Value) / Pitch);
		int lastRow = Math.Min(this.Rows - 1, (e.ClipRectangle.Bottom + this._scroll.Value) / Pitch);

		using var border = new Pen(BorderColor);
		using var hover = new Pen(HoverColor);
		for (int row = firstRow; row <= lastRow; row++)
		{
			for (int column = 0; column < columns; column++)
			{
				int index = row * columns + column;
				if (index >= this._shown.Count)
					return;

				var bounds = this.BoundsOf(index);
				e.Graphics.DrawImageUnscaled(this.ThumbnailOf(this._shown[index].Bitmap), bounds.X + 1, bounds.Y + 1);
				e.Graphics.DrawRectangle(index == this._hoverIndex ? hover : border, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
			}
		}
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		this.UpdateScroll();
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		// While a button is held the grid has the mouse capture: no hover, the drag is shown by the cursor.
		this.SetHover(e.Button == MouseButtons.None ? this.IndexAt(e.Location) : -1);
		base.OnMouseMove(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		this.SetHover(-1);
		base.OnMouseDown(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		this.SetHover(-1);
		base.OnMouseLeave(e);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		base.OnMouseWheel(e);
		if (!this._scroll.Visible)
			return;

		int max = Math.Max(0, this._scroll.Maximum - this._scroll.LargeChange + 1);
		this._scroll.Value = Math.Clamp(this._scroll.Value - Math.Sign(e.Delta) * Pitch * SystemInformation.MouseWheelScrollLines, 0, max);
		this.SetHover(this.IndexAt(e.Location));
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			this.ClearThumbnails();
		base.Dispose(disposing);
	}

	/// <summary>Drops the thumbnails made for the previous <see cref="IconSize"/>.</summary>
	private void ClearThumbnails()
	{
		foreach (var thumbnail in this._thumbnails.Values)
			thumbnail.Dispose();
		this._thumbnails.Clear();
	}
}
