using System.Drawing;
using System.Windows.Forms;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Interop;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// Holds a sack panel in a room of fixed size: a sack larger than the room (a stash file larger than the game's size)
/// scrolls inside it with dark scroll bars and the wheel (one cell per notch, Shift for across); a sack that fits is shown
/// whole, the viewport exactly its size, so nothing changes for it.
/// </summary>
public sealed class SackViewport : Panel
{
	/// <summary>The wheel turned and not yet scrolled: less than a notch.</summary>
	private int wheelRest;

	/// <summary>The cell size of the sack when last placed, to keep the scroll position in cells across a zoom.</summary>
	private int placedUnit;

	/// <summary>Puts <paramref name="sack"/> in a new viewport.</summary>
	public SackViewport(SackPanel sack)
	{
		this.Sack = sack;
		// Placed by the viewport only: a sack anchored otherwise would move when the viewport is resized.
		sack.Anchor = AnchorStyles.Top | AnchorStyles.Left;
		this.AutoScroll = true;
		this.BackColor = Theme.Background;
		this.DoubleBuffered = true;
		this.Controls.Add(sack);
	}

	/// <summary>The sack panel shown.</summary>
	public SackPanel Sack { get; }

	/// <summary>Whether the sack is larger than the room: a scroll bar is shown.</summary>
	public bool Scrolls => this.HorizontalScroll.Visible || this.VerticalScroll.Visible;

	/// <summary>How far the sack is scrolled, in pixels from its top-left corner.</summary>
	public Point ScrollOffset => new(-this.AutoScrollPosition.X, -this.AutoScrollPosition.Y);

	/// <summary>
	/// The size of a viewport showing <paramref name="content"/> in <paramref name="room"/>: the content's when it fits,
	/// else the whole room, where the scroll bars take their part (a bar on one axis may make the other axis scroll too).
	/// </summary>
	public static Size Fit(Size content, Size room)
		=> content.Width > room.Width || content.Height > room.Height ? room : content;

	/// <summary>
	/// Places the viewport in <paramref name="room"/> (client coordinates of its parent): a sack that fits at
	/// <paramref name="centered"/>, kept inside the room; a larger one fills the room. Keeps the scroll position in cells,
	/// unless <paramref name="toTopLeft"/> (another sack shown).
	/// </summary>
	public void Place(Rectangle room, Point centered, bool toTopLeft)
	{
		var unit = this.Sack.UnitSize;
		var cells = toTopLeft || this.placedUnit <= 0
			? PointF.Empty
			: new PointF(this.ScrollOffset.X / (float)this.placedUnit, this.ScrollOffset.Y / (float)this.placedUnit);
		this.placedUnit = unit;

		// From the top-left corner, so that the sack's location is its place in the scrolled area.
		this.AutoScrollPosition = Point.Empty;
		this.Sack.Location = Point.Empty;

		var size = Fit(this.Sack.Size, room.Size);
		this.Bounds = new Rectangle(
			Math.Clamp(centered.X, room.Left, room.Right - size.Width),
			Math.Clamp(centered.Y, room.Top, room.Bottom - size.Height),
			size.Width,
			size.Height);

		// The scroll range, set and laid out even while hidden (WinForms lays out a hidden panel's scroll bars only later).
		this.AutoScrollMinSize = this.Sack.Size;
		this.PerformLayout();

		if (cells != PointF.Empty)
			this.AutoScrollPosition = new Point(Convert.ToInt32(cells.X * unit), Convert.ToInt32(cells.Y * unit));
	}

	/// <summary>Scrolls as little as needed to show <paramref name="area"/> (sack client coordinates) whole, or its top-left part.</summary>
	public void ShowArea(Rectangle area)
	{
		if (!this.Scrolls)
			return;

		var client = this.ClientSize;
		var offset = this.ScrollOffset;
		static int Axis(int position, int start, int length, int visible)
			=> start < position || length > visible ? start
				: start + length > position + visible ? start + length - visible
				: position;

		var target = new Point(Axis(offset.X, area.X, area.Width, client.Width), Axis(offset.Y, area.Y, area.Height, client.Height));
		if (target != offset)
		{
			this.AutoScrollPosition = target;
			this.AfterScroll();
		}
	}

	/// <summary>The sack keeps its scroll position when it takes the focus (WinForms would scroll to its top-left corner).</summary>
	protected override Point ScrollToControl(Control activeControl) => this.DisplayRectangle.Location;

	/// <inheritdoc/>
	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		_ = UxTheme.SetWindowTheme(this.Handle, "DarkMode_Explorer", null);
	}

	/// <summary>
	/// One cell per notch, down the sack (across with Shift, or when only that bar is shown). A sack that fits passes the
	/// wheel on to the window, as before.
	/// </summary>
	protected override void OnMouseWheel(MouseEventArgs e)
	{
		bool across = this.HorizontalScroll.Visible && (!this.VerticalScroll.Visible || (ModifierKeys & Keys.Shift) != 0);
		if (!across && !this.VerticalScroll.Visible)
		{
			base.OnMouseWheel(e);
			return;
		}

		this.wheelRest += e.Delta;
		int notches = this.wheelRest / SystemInformation.MouseWheelScrollDelta;
		this.wheelRest %= SystemInformation.MouseWheelScrollDelta;
		if (notches != 0)
		{
			var offset = this.ScrollOffset;
			int step = notches * this.Sack.UnitSize;
			this.AutoScrollPosition = across ? new Point(offset.X - step, offset.Y) : new Point(offset.X, offset.Y - step);
			this.AfterScroll();
		}

		if (e is HandledMouseEventArgs handled)
			handled.Handled = true;
	}

	/// <summary>Shown again: its scroll bars follow the sack placed while hidden.</summary>
	protected override void OnVisibleChanged(EventArgs e)
	{
		base.OnVisibleChanged(e);
		if (this.Visible)
			this.PerformLayout();
	}

	/// <inheritdoc/>
	protected override void OnScroll(ScrollEventArgs se)
	{
		base.OnScroll(se);
		this.AfterScroll();
	}

	/// <summary>The sack moved under a still cursor: it repaints and highlights, or drags over, the cell now under the cursor.</summary>
	private void AfterScroll()
	{
		this.Sack.Invalidate();
		var cursor = Automation.VirtualCursor.GetClientPosition(this);
		if (this.ClientRectangle.Contains(cursor))
			this.Sack.FollowCursor();
	}
}
