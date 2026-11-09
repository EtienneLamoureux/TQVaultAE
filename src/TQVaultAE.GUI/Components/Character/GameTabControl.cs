using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>
/// Tabs drawn as the game's skill window: a leather tab per page (<see cref="Images.CharacterEditorTab"/>, 192x34, one
/// every 225 pixels as the window's two mastery tabs) on a transparent strip, where the dialog's background shows, and
/// under it the window's frame cut down to its inner border (<see cref="Images.CharacterEditorFrame"/>, 951x560, from
/// <c>SelectedSkillBackground01</c>); the page goes in the black area of the frame, where the game draws a mastery pane.
/// Each page can also show a few controls on the strip right of the tabs. Sizes are in pixels of the game texture,
/// times <see cref="FrameScale"/>.
/// </summary>
internal sealed class GameTabControl : Control
{
	/// <summary>Height of the strip of the tabs above the frame.</summary>
	private const int StripHeight = 35;

	/// <summary>Width of the frame.</summary>
	public const int FrameWidth = 951;

	/// <summary>Height of the strip and the frame.</summary>
	public const int FrameHeight = StripHeight + 560;

	/// <summary>The black area of the frame, where a page goes: the place of the mastery pane in the game window.</summary>
	public static readonly Rectangle PageArea = new(16, StripHeight + 10, 919, 540);

	/// <summary>The strip right of the tabs, where a page may show its controls (up to the frame's right edge).</summary>
	public static readonly Rectangle HeaderArea = new(675, 4, 275, 30);

	private const int TabLeft = 0;
	private const int TabTop = 6;
	private const int TabWidth = 192;
	private const int TabHeight = 34;
	private const int TabStep = 225;

	private static readonly Color ActiveText = Color.White;
	private static readonly Color InactiveText = Color.FromArgb(150, 150, 140);

	private readonly Bitmap _frame = Images.CharacterEditorFrame;
	private readonly Bitmap _tab = Images.CharacterEditorTab;
	private readonly List<GamePage> _pages = [];
	private float _frameScale = 1F;
	private int _selectedIndex = -1;

	/// <summary>A page: its tab text, its content and the controls of the strip.</summary>
	internal sealed class GamePage
	{
		public required Control Content { get; init; }
		public Control? Header { get; init; }
		public string Text { get; set; } = string.Empty;
	}

	public GameTabControl()
	{
		this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);

		// Transparent: the strip of the tabs shows the dialog's background.
		this.BackColor = Color.Transparent;
		this.FrameScale = 1F;
	}

	/// <summary>Raised when another page is shown.</summary>
	public event EventHandler? SelectedIndexChanged;

	/// <summary>The font of the tab texts, in screen pixels.</summary>
	public Font? TabFont { get; set; }

	/// <summary>The pages, in tab order.</summary>
	public IReadOnlyList<GamePage> Pages => this._pages;

	/// <summary>The size of a game pixel on screen; the control takes the frame size times this scale.</summary>
	public float FrameScale
	{
		get => this._frameScale;
		set
		{
			this._frameScale = value;
			this.Size = new Size(this.Scaled(FrameWidth), this.Scaled(FrameHeight));
			this.LayoutPages();
		}
	}

	/// <summary>The page shown.</summary>
	public int SelectedIndex
	{
		get => this._selectedIndex;
		set
		{
			if (value < 0 || value >= this._pages.Count || value == this._selectedIndex)
				return;

			this._selectedIndex = value;
			for (int i = 0; i < this._pages.Count; i++)
			{
				this._pages[i].Content.Visible = i == value;
				if (this._pages[i].Header is { } header)
					header.Visible = i == value;
			}

			this.Invalidate();
			this.SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	/// <summary>The size of the page area on screen.</summary>
	public Size PageSize => new(this.Scaled(PageArea.Width), this.Scaled(PageArea.Height));

	/// <summary>Adds a page: its content fills the black area of the frame, its header goes on the strip.</summary>
	public GamePage AddPage(string text, Control content, Control? header = null)
	{
		var page = new GamePage { Text = text, Content = content, Header = header };
		this._pages.Add(page);
		this.Controls.Add(content);
		if (header is not null)
			this.Controls.Add(header);

		this.LayoutPages();
		if (this._selectedIndex < 0)
			this.SelectedIndex = 0;
		else
		{
			content.Visible = false;
			if (header is not null)
				header.Visible = false;
		}

		return page;
	}

	/// <summary>Changes the text of a tab.</summary>
	public void SetPageText(int index, string text)
	{
		if (this._pages[index].Text == text)
			return;

		this._pages[index].Text = text;
		this.Invalidate(this.TabBounds(index));
	}

	/// <summary>The tab of a page on screen.</summary>
	public Rectangle TabBounds(int index)
		=> new(this.Scaled(TabLeft + index * TabStep), this.Scaled(TabTop), this.Scaled(TabWidth), this.Scaled(TabHeight));

	private int Scaled(int value) => (int)Math.Round(value * this._frameScale);

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			this._frame.Dispose();
			this._tab.Dispose();
		}

		base.Dispose(disposing);
	}

	private void LayoutPages()
	{
		var area = new Rectangle(this.Scaled(PageArea.X), this.Scaled(PageArea.Y), this.Scaled(PageArea.Width), this.Scaled(PageArea.Height));
		foreach (var page in this._pages)
		{
			page.Content.Bounds = area;
			if (page.Header is { } header)
			{
				// Right-aligned on the strip, centred on its height.
				var strip = new Rectangle(this.Scaled(HeaderArea.X), this.Scaled(HeaderArea.Y), this.Scaled(HeaderArea.Width), this.Scaled(HeaderArea.Height));
				var size = header.PreferredSize;
				header.Size = size;
				header.Location = new Point(strip.Right - size.Width, strip.Top + (strip.Height - size.Height) / 2);
			}
		}
	}

	protected override void OnLayout(LayoutEventArgs levent)
	{
		base.OnLayout(levent);
		this.LayoutPages();
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if (e.Button != MouseButtons.Left)
			return;

		for (int i = 0; i < this._pages.Count; i++)
			if (this.TabBounds(i).Contains(e.Location))
				this.SelectedIndex = i;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		var g = e.Graphics;
		g.InterpolationMode = this._frameScale == 1F ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
		g.PixelOffsetMode = PixelOffsetMode.Half;
		g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

		var strip = this.Scaled(StripHeight);
		g.DrawImage(this._frame, new Rectangle(0, strip, this.Width, this.Height - strip));

		using var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
		for (int i = 0; i < this._pages.Count; i++)
		{
			var bounds = this.TabBounds(i);
			var active = i == this._selectedIndex;

			// The tabs of the hidden pages are darkened, as the game greys their text.
			g.DrawImage(this._tab, bounds);
			if (!active)
			{
				using var shade = new SolidBrush(Color.FromArgb(90, 0, 0, 0));
				g.FillRectangle(shade, bounds);
			}

			using var brush = new SolidBrush(active ? ActiveText : InactiveText);
			g.DrawString(this._pages[i].Text, this.TabFont ?? this.Font, brush, bounds, center);
		}
	}
}
