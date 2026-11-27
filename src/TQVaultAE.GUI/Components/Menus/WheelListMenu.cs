namespace TQVaultAE.GUI.Components.Menus;

/// <summary>
/// A list opened as a menu that the mouse wheel scrolls a row per notch while it shows its scroll arrows (a menu
/// ignores the wheel): the entry past the visible ones is highlighted as the arrow keys do, and the menu brings it in.
/// </summary>
internal sealed class WheelListMenu : ContextMenuStrip
{
	protected override void OnMouseWheel(MouseEventArgs e)
	{
		base.OnMouseWheel(e);
		this.ScrollRows(-(e.Delta / 120 is var notches and not 0 ? notches : Math.Sign(e.Delta)));
		if (e is HandledMouseEventArgs handled)
			handled.Handled = true;
	}

	/// <summary>
	/// Opens at least <paramref name="minimumWidth"/> wide (the box it drops from), every row as wide as the list: the
	/// highlight of the row under the mouse crosses the whole list. Called once the rows are in.
	/// </summary>
	internal void WidenRows(int minimumWidth) => this.MinimumSize = new Size(minimumWidth, 0);

	private bool _widening;

	/// <summary>
	/// After the menu's layout, which gives every row the widest text's width even in a wider list, stretches the rows to
	/// the list's inner width.
	/// </summary>
	protected override void OnLayout(LayoutEventArgs e)
	{
		base.OnLayout(e);
		if (this._widening)
			return;

		int width = this.DisplayRectangle.Width;
		this._widening = true;
		try
		{
			foreach (ToolStripItem row in this.Items)
			{
				if (row.Available && row.Width < width)
					row.Width = width;
			}
		}
		finally
		{
			this._widening = false;
		}
	}

	/// <summary>Scrolls the visible entries by rows: down for a positive count, up for a negative one, within the list.</summary>
	internal void ScrollRows(int rows)
	{
		var entries = this.Items.OfType<ToolStripItem>().Where(i => i.Available).ToList();
		var view = this.DisplayRectangle;
		for (int step = 0; step < Math.Abs(rows); step++)
		{
			var visible = entries.Where(i => i.Bounds.Top >= view.Top && i.Bounds.Bottom <= view.Bottom).ToList();
			if (visible.Count == 0)
				return;

			// From the visible edge, one entry further: the menu scrolls it in, as for the arrow keys; none past the ends.
			var edge = rows > 0 ? visible[^1] : visible[0];
			if (edge == (rows > 0 ? entries[^1] : entries[0]))
				return;

			edge.Select();
			this.ProcessDialogKey(rows > 0 ? Keys.Down : Keys.Up);
		}
	}
}
