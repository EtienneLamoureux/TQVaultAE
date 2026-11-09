using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>
/// Tabs drawn as the game's skill window: its marble frame (<c>SelectedSkillBackground01</c>, 1001x637), a leather tab
/// per page along the top (192x34, one every 225 pixels from x 25, as the window's two mastery tabs) and the page in
/// the black area of the frame, where the game draws a mastery pane (919x540 at 41,65). Each page can also show a few
/// controls on the marble strip right of the tabs. Sizes are in pixels of the game window, times <see cref="FrameScale"/>.
/// </summary>
internal sealed class GameTabControl : Control
{
	/// <summary>Width of the frame texture.</summary>
	public const int FrameWidth = 1001;

	/// <summary>
	/// Rows of the frame texture left out at the top: its title strip and close button, which the dialog has already.
	/// </summary>
	public const int TopCrop = 20;

	/// <summary>Height of the frame drawn: the texture (637) without its title strip.</summary>
	public const int FrameHeight = 637 - TopCrop;

	/// <summary>The black area of the frame, where a page goes: the place of the mastery pane in the game (41,65).</summary>
	public static readonly Rectangle PageArea = new(41, 65 - TopCrop, 919, 540);

	/// <summary>The marble strip right of the tabs, where a page may show its controls.</summary>
	public static readonly Rectangle HeaderArea = new(700, 24 - TopCrop, 255, 34);

	private const string FrameTexture = """InGameUI\Skills\SelectedSkillBackground01.tex""";
	private const string TabTexture = """InGameUI\Skills\SkillTab01.tex""";
	private const int TabLeft = 25;
	private const int TabTop = 26 - TopCrop;
	private const int TabWidth = 192;
	private const int TabHeight = 34;
	private const int TabStep = 225;

	private static readonly Color ActiveText = Color.White;
	private static readonly Color InactiveText = Color.FromArgb(150, 150, 140);

	private readonly IUIService _uiService;
	private readonly List<GamePage> _pages = [];
	private float _frameScale = 1F;
	private int _selectedIndex = -1;

	/// <summary>A page: its tab text, its content and the controls of the marble strip.</summary>
	internal sealed class GamePage
	{
		public required Control Content { get; init; }
		public Control? Header { get; init; }
		public string Text { get; set; } = string.Empty;
	}

	public GameTabControl(IUIService uiService)
	{
		this._uiService = uiService;
		this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
		this.BackColor = Color.Black;
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

	/// <summary>Adds a page: its content fills the black area of the frame, its header goes on the marble strip.</summary>
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

	private void LayoutPages()
	{
		var area = new Rectangle(this.Scaled(PageArea.X), this.Scaled(PageArea.Y), this.Scaled(PageArea.Width), this.Scaled(PageArea.Height));
		foreach (var page in this._pages)
		{
			page.Content.Bounds = area;
			if (page.Header is { } header)
			{
				// Right-aligned on the marble strip, centred on its height.
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

		var frame = this._uiService.LoadBitmap(FrameTexture.ToRecordId());
		g.DrawImage(frame, new Rectangle(0, 0, this.Width, this.Height), new Rectangle(0, TopCrop, FrameWidth, FrameHeight), GraphicsUnit.Pixel);

		// The bottom of the frame's close button reaches below the title strip: marble from its left covers it.
		var corner = new Rectangle(FrameWidth - 30, TopCrop, 30, 10);
		g.DrawImage(frame, new Rectangle(this.Scaled(corner.X), 0, this.Width - this.Scaled(corner.X), this.Scaled(corner.Height)),
			new Rectangle(corner.X - corner.Width, corner.Y, corner.Width, corner.Height), GraphicsUnit.Pixel);

		var tab = this._uiService.LoadBitmap(TabTexture.ToRecordId());
		using var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
		for (int i = 0; i < this._pages.Count; i++)
		{
			var bounds = this.TabBounds(i);
			var active = i == this._selectedIndex;

			// The frame has two tabs of its own; the leather is drawn again so that every tab looks the same, and the
			// tabs of the hidden pages are darkened as the game greys their text.
			g.DrawImage(tab, bounds);
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
