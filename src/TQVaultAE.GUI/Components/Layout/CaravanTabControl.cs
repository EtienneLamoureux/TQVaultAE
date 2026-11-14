namespace TQVaultAE.GUI.Components.Layout;

/// <summary>
/// A tab control for the marble dialogs, laid in a caravan frame: it paints itself on the frame's black instead of the
/// system's white tabs and strip. The selected tab is black with its text in gold, the others dark brown with light
/// text, each tab and the page edged in the dark gold of the frames; the pages are black.
/// </summary>
public partial class CaravanTabControl : TabControl
{
	private const int WM_SETFONT = 0x0030;

	/// <summary>The font handle given to the native control, deleted when replaced.</summary>
	private IntPtr _fontHandle;

	/// <summary>The background of a tab that is not selected: the dark brown of the game windows.</summary>
	private static readonly Color TabBack = Color.FromArgb(46, 31, 21);

	/// <summary>The edge of the tabs and of the page, the dark gold of the frames.</summary>
	private static readonly Color Edge = Color.FromArgb(120, 98, 52);

	public CaravanTabControl()
		=> this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
			| ControlStyles.ResizeRedraw, true);

	/// <summary>Makes the pages black, once the designer has set their properties (it sets them after adding them).</summary>
	protected override void OnCreateControl()
	{
		base.OnCreateControl();
		foreach (TabPage page in this.TabPages)
		{
			page.UseVisualStyleBackColor = false;
			page.BackColor = Color.Black;
		}
	}

	/// <summary>
	/// Windows Forms gives its font to the native control only when the system paints it: the tabs would be measured with
	/// the system font and drawn here with this one. The native control gets it before it lays out its pages.
	/// </summary>
	protected override void OnHandleCreated(EventArgs e)
	{
		this.SendFont();
		base.OnHandleCreated(e);
	}

	protected override void OnFontChanged(EventArgs e)
	{
		if (this.IsHandleCreated)
			this.SendFont();
		base.OnFontChanged(e);
	}

	private void SendFont()
	{
		var old = this._fontHandle;
		this._fontHandle = this.Font.ToHfont();
		var message = Message.Create(this.Handle, WM_SETFONT, this._fontHandle, 1);
		this.DefWndProc(ref message);
		if (old != IntPtr.Zero)
			DeleteObject(old);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (this._fontHandle != IntPtr.Zero)
		{
			DeleteObject(this._fontHandle);
			this._fontHandle = IntPtr.Zero;
		}
	}

	[System.Runtime.InteropServices.LibraryImport("gdi32.dll")]
	[return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
	private static partial bool DeleteObject(IntPtr handle);

	protected override void OnSelectedIndexChanged(EventArgs e)
	{
		base.OnSelectedIndexChanged(e);
		this.Invalidate();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		var g = e.Graphics;
		g.Clear(Color.Black);

		using var edge = new Pen(Edge);
		var page = this.DisplayRectangle;
		g.DrawRectangle(edge, page.X - 1, page.Y - 1, page.Width + 1, page.Height + 1);

		var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
			| TextFormatFlags.NoPrefix;
		using var back = new SolidBrush(TabBack);
		for (int i = 0; i < this.TabCount; i++)
		{
			var tab = this.GetTabRect(i);
			if (!tab.IntersectsWith(e.ClipRectangle))
				continue;

			bool selected = i == this.SelectedIndex;
			if (selected)
				g.FillRectangle(Brushes.Black, tab);
			else
				g.FillRectangle(back, tab);
			g.DrawRectangle(edge, tab.X, tab.Y, tab.Width - 1, tab.Height - 1);
			TextRenderer.DrawText(g, this.TabPages[i].Text, this.Font, tab, selected ? Color.Gold : Color.Gainsboro, flags);
		}
	}
}
