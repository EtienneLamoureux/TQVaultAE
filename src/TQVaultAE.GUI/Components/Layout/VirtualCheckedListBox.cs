using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms.VisualStyles;
using TQVaultAE.GUI.Components.Scaling;

namespace TQVaultAE.GUI.Components.Layout;

/// <summary>
/// Multi-column checked list that only draws the visible rows. Items are plain objects held in memory, so filling it
/// with ten thousand entries is instant, where <see cref="CheckedListBox"/> creates one native entry per item
/// (about 0.2 ms each).
/// </summary>
/// <remarks>
/// Implements the part of the <see cref="CheckedListBox"/> API used by the advanced search dialog: column-major
/// layout with a horizontal scroll bar (like <c>MultiColumn = true</c>), check on click, <see cref="CheckedItems"/>,
/// <see cref="IndexFromPoint"/> and <see cref="SelectedValueChanged"/>, raised after each check change.
/// The keyboard moves a focus item drawn with a focus rectangle: Up/Down by one entry, Left/Right by one column,
/// Page Up/Page Down by the visible entries, Home/End to the ends, and Space toggles its check box.
/// </remarks>
public partial class VirtualCheckedListBox : Control, IScalingControl, IZoomedFont
{
	/// <summary>Index returned by <see cref="IndexFromPoint"/> outside any item, as <see cref="ListBox.NoMatches"/>.</summary>
	public const int NoMatches = ListBox.NoMatches;

	private readonly List<object> _items = [];
	private readonly List<bool> _checked = [];
	private readonly HScrollBar _scroll = new() { Dock = DockStyle.Bottom, Visible = false, TabStop = false };
	private int _updateDepth;
	private int _columnWidth = 120;
	private BorderStyle _borderStyle = BorderStyle.None;
	private int _hoverIndex = NoMatches;
	private int _focusIndex = NoMatches;

	/// <summary>
	/// Focus entry before <see cref="ObjectCollection.Clear"/>: the search dialog refills its lists after each check, and
	/// the focus goes back to the same entry (else to the same place) once the list is filled again.
	/// </summary>
	private (object Item, int Index)? _pendingFocus;

	public VirtualCheckedListBox()
	{
		this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint
			| ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
		this.Items = new ObjectCollection(this);
		this._scroll.ValueChanged += (_, _) => this.Invalidate();
		this.Controls.Add(this._scroll);
	}

	#region CheckedListBox compatible API

	/// <summary>Items of the list; adding or clearing them only updates the layout.</summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public ObjectCollection Items { get; }

	/// <summary>Checked items, in list order.</summary>
	[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public IReadOnlyList<object> CheckedItems => this._items.Where((_, i) => this._checked[i]).ToList();

	/// <summary>Indices of the checked items, in list order.</summary>
	[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public IReadOnlyList<int> CheckedIndices => Enumerable.Range(0, this._items.Count).Where(i => this._checked[i]).ToList();

	/// <summary>Raised after an item is checked or unchecked by the user.</summary>
	public event EventHandler? SelectedValueChanged;

	/// <summary>Toggles the check box when an item is clicked (always on; kept for designer compatibility).</summary>
	[DefaultValue(true)]
	public bool CheckOnClick { get; set; } = true;

	/// <summary>Kept for designer compatibility; the list is always laid out in columns.</summary>
	[DefaultValue(true)]
	public bool MultiColumn { get; set; } = true;

	/// <summary>Kept for designer compatibility.</summary>
	[DefaultValue(false)]
	public bool IntegralHeight { get; set; }

	/// <summary>Kept for designer compatibility.</summary>
	[DefaultValue(false)]
	public bool FormattingEnabled { get; set; }

	/// <summary>Kept for designer compatibility; the scroll bar appears when the columns do not fit.</summary>
	[DefaultValue(false)]
	public bool HorizontalScrollbar { get; set; }

	[DefaultValue(BorderStyle.None)]
	public BorderStyle BorderStyle
	{
		get => this._borderStyle;
		set { this._borderStyle = value; this.Invalidate(); }
	}

	/// <summary>Width of one column, check box included.</summary>
	[DefaultValue(120)]
	public int ColumnWidth
	{
		get => this._columnWidth;
		set { this._columnWidth = Math.Max(1, value); this.UpdateLayout(); }
	}

	/// <summary>Height of one row.</summary>
	[Browsable(false)]
	public int ItemHeight => Math.Max(this.Font.Height, CheckBoxGlyph.Height) + 2;

	public int GetItemHeight(int index) => this.ItemHeight;

	public bool GetItemChecked(int index) => this._checked[index];

	public void SetItemChecked(int index, bool value)
	{
		if (this._checked[index] == value)
			return;
		this._checked[index] = value;
		this.InvalidateItem(index);
	}

	/// <summary>Entry that has the keyboard focus (drawn with a focus rectangle), or <see cref="NoMatches"/>.</summary>
	[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int FocusedIndex
	{
		get => this._focusIndex;
		set
		{
			int index = this._items.Count == 0 ? NoMatches : Math.Clamp(value, 0, this._items.Count - 1);
			if (index == this._focusIndex)
				return;
			this.InvalidateItem(this._focusIndex);
			this._focusIndex = index;
			this.EnsureVisible(index);
			this.InvalidateItem(index);
		}
	}

	/// <summary>Scrolls the columns so that the entry at <paramref name="index"/> is fully shown.</summary>
	public void EnsureVisible(int index)
	{
		if (!this._scroll.Visible || index < 0 || index >= this._items.Count)
			return;

		int column = index / this.RowsPerColumn;
		int first = this.FirstColumn;
		int value = column < first ? column
			: column >= first + this.VisibleColumns ? column - this.VisibleColumns + 1
			: first;
		this.FirstColumn = value;
	}

	public void BeginUpdate() => this._updateDepth++;

	public void EndUpdate()
	{
		if (this._updateDepth > 0 && --this._updateDepth == 0)
			this.UpdateLayout();
	}

	/// <summary>Index of the item under <paramref name="point"/> (client coordinates), or <see cref="NoMatches"/>.</summary>
	public int IndexFromPoint(Point point)
	{
		int rows = this.RowsPerColumn;
		if (point.Y < 0 || point.Y >= rows * this.ItemHeight || point.X < 0)
			return NoMatches;
		int column = this.FirstColumn + point.X / this.ColumnWidth;
		int index = column * rows + point.Y / this.ItemHeight;
		return index < this._items.Count ? index : NoMatches;
	}

	public int IndexFromPoint(int x, int y) => this.IndexFromPoint(new Point(x, y));

	/// <summary>Up to this many texts, <see cref="GetMaxTextWidth"/> measures each one.</summary>
	private const int MeasureAllUpTo = 256;

	/// <summary>Flags of the item text, as painted.</summary>
	private const TextFormatFlags MeasureFlags = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;

	/// <summary>Width of the widest item text, without the check box.</summary>
	/// <remarks>
	/// Measuring the 10,000 entries of the attribute lists one by one takes about a second. So each text first gets an
	/// estimate, the sum of the widths of its characters (measured once each), and the texts are measured in decreasing
	/// estimate order until the estimate falls below the widest measured width by more than the estimate can be off
	/// (see <see cref="EstimateMargin"/>): no text left can then be wider.
	/// </remarks>
	public int GetMaxTextWidth() => this.MeasureMaxTextWidth().Width;

	/// <summary>How far below a text's real width its estimate may be: kerning, ligatures and rounding.</summary>
	private int EstimateMargin(int width) => this.Font.Height + width / 10;

	/// <summary><see cref="GetMaxTextWidth"/>, with the number of texts it measured for real.</summary>
	private (int Width, int Measured) MeasureMaxTextWidth()
	{
		var texts = this._items.Select(i => i?.ToString() ?? string.Empty).Distinct(StringComparer.Ordinal).ToList();
		if (texts.Count == 0)
			return (0, 0);

		using var g = this.CreateGraphics();
		int Measure(string text) => TextRenderer.MeasureText(g, text, this.Font, Size.Empty, MeasureFlags).Width;

		if (texts.Count <= MeasureAllUpTo)
			return (texts.Max(Measure), texts.Count);

		var runeWidths = new Dictionary<Rune, int>();
		int Estimate(string text)
		{
			int sum = 0;
			foreach (var rune in text.EnumerateRunes())
			{
				if (!runeWidths.TryGetValue(rune, out int width))
					runeWidths[rune] = width = Measure(rune.ToString());
				sum += width;
			}
			return sum;
		}

		int max = 0, measured = 0;
		foreach (var (text, estimate) in texts.Select(t => (t, Estimate(t))).OrderByDescending(e => e.Item2))
		{
			if (estimate < max - this.EstimateMargin(max))
				break;
			max = Math.Max(max, Measure(text));
			measured++;
		}
		return (max, measured);
	}

	#endregion

	#region Layout

	private static Size CheckBoxGlyph => new(13, 13);

	/// <summary>Space between the check box and the text: a third of the font height, so it follows the zoom.</summary>
	private int GlyphTextGap => Math.Max(4, this.Font.Height / 3);

	/// <summary>Where the item text starts, from the left of its column: the check box and the space after it.</summary>
	public int TextLeft => 1 + CheckBoxGlyph.Width + this.GlyphTextGap;

	private int ScrollBarHeight => this._scroll.Visible ? this._scroll.Height : 0;

	private int RowsPerColumn => Math.Max(1, (this.ClientSize.Height - this.ScrollBarHeight) / this.ItemHeight);

	private int ColumnCount => (this._items.Count + this.RowsPerColumn - 1) / this.RowsPerColumn;

	private int VisibleColumns => Math.Max(1, this.ClientSize.Width / this.ColumnWidth);

	/// <summary>
	/// Whether the list itself is set visible: <see cref="Control.Visible"/> also reads <see langword="false"/> while a
	/// parent is hidden (a hidden category, a closed dialog).
	/// </summary>
	[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool VisibleSelf { get; private set; } = true;

	protected override void SetVisibleCore(bool value)
	{
		this.VisibleSelf = value;
		base.SetVisibleCore(value);
	}

	/// <summary>First column shown, where the horizontal scroll bar stands; set, it scrolls (within the columns).</summary>
	[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int FirstColumn
	{
		get => this._scroll.Visible ? this._scroll.Value : 0;
		set
		{
			if (!this._scroll.Visible)
				return;
			int max = this._scroll.Maximum - this._scroll.LargeChange + 1;
			this._scroll.Value = Math.Clamp(value, 0, Math.Max(0, max));
		}
	}

	private void UpdateLayout()
	{
		if (this._updateDepth > 0)
			return;

		// The height for this monitor: created in code, the bar keeps the system DPI height (21 pixels at 120 DPI) on a
		// monitor of another DPI, and the dialog sizes the list for this height (AdjustCheckBoxesHeight).
		this._scroll.Height = SystemInformation.GetHorizontalScrollBarHeightForDpi(this.DeviceDpi);

		// Rows depend on whether the scroll bar takes space: decide with it hidden first.
		this._scroll.Visible = false;
		bool overflow = this.ColumnCount > this.VisibleColumns;
		if (overflow)
		{
			this._scroll.Visible = true;
			int max = Math.Max(0, this.ColumnCount - this.VisibleColumns);
			this._scroll.Minimum = 0;
			this._scroll.LargeChange = this.VisibleColumns;
			this._scroll.SmallChange = 1;
			this._scroll.Maximum = max + this.VisibleColumns - 1;
			if (this._scroll.Value > max)
				this._scroll.Value = max;
		}

		if (this._pendingFocus is { } pending && this._items.Count > 0)
		{
			this._pendingFocus = null;
			int index = this._items.IndexOf(pending.Item);
			this._focusIndex = index >= 0 ? index : Math.Min(pending.Index, this._items.Count - 1);
			this.EnsureVisible(this._focusIndex);
		}
		this.Invalidate();
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		this.UpdateLayout();
	}

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		this.UpdateLayout();
	}

	private Rectangle ItemBounds(int index)
	{
		int rows = this.RowsPerColumn;
		int column = index / rows - this.FirstColumn;
		return new Rectangle(column * this.ColumnWidth, index % rows * this.ItemHeight, this.ColumnWidth, this.ItemHeight);
	}

	private void InvalidateItem(int index)
	{
		if (this._updateDepth == 0 && index >= 0 && index < this._items.Count)
			this.Invalidate(this.ItemBounds(index));
	}

	#endregion

	#region Painting

	protected override void OnPaint(PaintEventArgs e)
	{
		var g = e.Graphics;
		using (var back = new SolidBrush(this.BackColor))
			g.FillRectangle(back, this.ClientRectangle);

		int rows = this.RowsPerColumn;
		int first = this.FirstColumn * rows;
		int last = Math.Min(this._items.Count, (this.FirstColumn + this.VisibleColumns + 1) * rows);
		var glyph = CheckBoxGlyph;
		var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding;

		for (int i = first; i < last; i++)
		{
			var bounds = this.ItemBounds(i);
			if (!bounds.IntersectsWith(e.ClipRectangle))
				continue;

			if (i == this._hoverIndex)
				using (var hover = new SolidBrush(ControlPaint.Light(this.BackColor, 0.3f)))
					g.FillRectangle(hover, bounds);

			var glyphPoint = new Point(bounds.X + 1, bounds.Y + (bounds.Height - glyph.Height) / 2);
			CheckBoxRenderer.DrawCheckBox(g, glyphPoint, this._checked[i] ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal);

			var textBounds = Rectangle.FromLTRB(glyphPoint.X + glyph.Width + this.GlyphTextGap, bounds.Top, bounds.Right, bounds.Bottom);
			TextRenderer.DrawText(g, this._items[i]?.ToString() ?? string.Empty, this.Font, textBounds, this.ForeColor, flags);

			if (i == this._focusIndex && this.Focused)
				ControlPaint.DrawFocusRectangle(g, bounds, this.ForeColor, this.BackColor);
		}

		if (this.BorderStyle != BorderStyle.None)
			ControlPaint.DrawBorder(g, this.ClientRectangle, ControlPaint.Dark(this.BackColor), ButtonBorderStyle.Solid);
	}

	#endregion

	#region Input

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);
		if (e.Button != MouseButtons.Left)
			return;

		int index = this.IndexFromPoint(e.Location);
		if (index == NoMatches)
			return;

		this.Focus();
		this.FocusedIndex = index;
		this.ToggleItem(index);
	}

	/// <summary>Toggles the check box of an entry like a user, so <see cref="SelectedValueChanged"/> follows.</summary>
	private void ToggleItem(int index)
	{
		this.SetItemChecked(index, !this._checked[index]);
		this.SelectedValueChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>The keys that move the focus entry stay in the list instead of moving to the next control.</summary>
	protected override bool IsInputKey(Keys keyData)
	{
		if ((keyData & Keys.Modifiers) == Keys.None)
		{
			switch (keyData & Keys.KeyCode)
			{
				case Keys.Up or Keys.Down or Keys.Left or Keys.Right or Keys.PageUp or Keys.PageDown or Keys.Home or Keys.End:
					return true;
			}
		}
		return base.IsInputKey(keyData);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (e.Handled || e.Modifiers != Keys.None || this._items.Count == 0)
			return;

		int rows = this.RowsPerColumn;
		int current = this._focusIndex;
		if (current == NoMatches)
		{
			// No focus entry yet: the first key shows it on the first visible entry.
			if (e.KeyCode is Keys.Up or Keys.Down or Keys.Left or Keys.Right or Keys.PageUp or Keys.PageDown or Keys.Space)
			{
				this.FocusedIndex = this.FirstColumn * rows;
				e.Handled = true;
				return;
			}
			current = 0;
		}

		int page = rows * this.VisibleColumns;
		int? target = e.KeyCode switch
		{
			Keys.Up => current - 1,
			Keys.Down => current + 1,
			Keys.Left => current - rows,
			Keys.Right => current + rows,
			Keys.PageUp => current - page,
			Keys.PageDown => current + page,
			Keys.Home => 0,
			Keys.End => this._items.Count - 1,
			_ => null,
		};

		if (target is int index)
		{
			this.FocusedIndex = index;
			this.EnsureVisible(this._focusIndex);
			e.Handled = true;
		}
		else if (e.KeyCode == Keys.Space)
		{
			this.ToggleItem(current);
			e.Handled = true;
		}
	}

	protected override void OnGotFocus(EventArgs e)
	{
		base.OnGotFocus(e);
		this.InvalidateItem(this._focusIndex);
	}

	protected override void OnLostFocus(EventArgs e)
	{
		base.OnLostFocus(e);
		this.InvalidateItem(this._focusIndex);
	}

	protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
		// A double click is two clicks for a check box list: treat the second one as a click too.
		base.OnMouseDoubleClick(e);
		this.OnMouseClick(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		int index = this.IndexFromPoint(e.Location);
		if (index == this._hoverIndex)
			return;
		this.InvalidateItem(this._hoverIndex);
		this._hoverIndex = index;
		this.InvalidateItem(index);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		this.InvalidateItem(this._hoverIndex);
		this._hoverIndex = NoMatches;
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		base.OnMouseWheel(e);
		if (!this._scroll.Visible)
			return;
		this.FirstColumn = this._scroll.Value - Math.Sign(e.Delta);
		if (e is HandledMouseEventArgs handled)
			handled.Handled = true;
	}

	#endregion

	/// <inheritdoc/>
	Font IZoomedFont.ZoomedFont
	{
		get => this.Font;
		set => this.Font = value;
	}

	/// <summary>Scales the font like the other scaling controls.</summary>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		this.Font = ZoomScope.ScaleFont(this.Font, factor.Height);
		base.ScaleControl(factor, specified);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			this._scroll.Dispose();
		base.Dispose(disposing);
	}

	/// <summary>Items of a <see cref="VirtualCheckedListBox"/>, with the <see cref="CheckedListBox.ObjectCollection"/> members the dialog uses.</summary>
	public sealed class ObjectCollection : IReadOnlyList<object>, IList
	{
		private readonly VirtualCheckedListBox _owner;

		internal ObjectCollection(VirtualCheckedListBox owner) => this._owner = owner;

		public int Count => this._owner._items.Count;

		public object this[int index]
		{
			get => this._owner._items[index];
			set { this._owner._items[index] = value; this._owner.InvalidateItem(index); }
		}

		public int Add(object item) => this.Add(item, false);

		public int Add(object item, bool isChecked)
		{
			this._owner._items.Add(item);
			this._owner._checked.Add(isChecked);
			this._owner.UpdateLayout();
			return this._owner._items.Count - 1;
		}

		public void AddRange(object[] items) => this.AddRange((IEnumerable<object>)items);

		public void AddRange(IEnumerable<object> items)
		{
			int before = this._owner._items.Count;
			this._owner._items.AddRange(items);
			this._owner._checked.AddRange(Enumerable.Repeat(false, this._owner._items.Count - before));
			this._owner.UpdateLayout();
		}

		public void Clear()
		{
			this._owner._hoverIndex = NoMatches;
			if (this._owner._focusIndex != NoMatches)
				this._owner._pendingFocus = (this._owner._items[this._owner._focusIndex], this._owner._focusIndex);
			this._owner._items.Clear();
			this._owner._checked.Clear();
			this._owner._focusIndex = NoMatches;
			this._owner.UpdateLayout();
		}

		public IEnumerator<object> GetEnumerator() => this._owner._items.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

		#region IList (designer serialization)

		bool IList.IsFixedSize => false;
		bool IList.IsReadOnly => false;
		bool ICollection.IsSynchronized => false;
		object ICollection.SyncRoot => this;
		object? IList.this[int index] { get => this[index]; set => this[index] = value!; }
		int IList.Add(object? value) => this.Add(value!);
		bool IList.Contains(object? value) => this._owner._items.Contains(value!);
		int IList.IndexOf(object? value) => this._owner._items.IndexOf(value!);
		void IList.Insert(int index, object? value) => throw new NotSupportedException();
		void IList.Remove(object? value) => throw new NotSupportedException();
		void IList.RemoveAt(int index) => throw new NotSupportedException();
		void ICollection.CopyTo(Array array, int index) => ((ICollection)this._owner._items).CopyTo(array, index);

		#endregion
	}
}
