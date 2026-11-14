using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// Two items side by side in four columns: the first item's differences, its picture and classic tooltip, the second
/// item's picture and classic tooltip, the second item's differences, under a toolbar ("Align rows", "Hide unchanged",
/// "Show sources"); a click on a stat with sources shows or hides them for that stat.
/// The content is decided by <see cref="ItemComparisonContentBuilder"/>; this dialog only draws its lines, with the rows
/// of the item tooltip, in the look of Item Properties: the marble behind, each column on black in a caravan frame.
/// </summary>
internal sealed partial class ItemComparisonDialog : VaultForm
{
	/// <summary>Background of the columns: the black inside their caravan frame, as Item Properties' lines.</summary>
	private static readonly Color ColumnColor = Color.Black;

	/// <summary>Background of the dialog where the marble is not drawn (high contrast), as Item Properties'.</summary>
	private static readonly Color DialogColor = Color.FromArgb(46, 31, 21);

	private readonly IItemComparisonService _comparisonService;
	private readonly ItemComparisonContentBuilder _builder;
	private readonly Panel _scroll;
	private readonly TableLayoutPanel _table;
	private readonly Panel _buttons;
	private readonly FlowLayoutPanel _toolbar;
	private readonly ScalingCheckBox _alignRows;
	private readonly ScalingCheckBox _hideUnchanged;
	private readonly ScalingCheckBox _showSources;

	/// <summary>The stats whose sources were clicked open or closed against "Show sources"; emptied when it changes.</summary>
	private readonly HashSet<string> _toggled = new(StringComparer.Ordinal);

	/// <summary>The first item, set before the dialog is shown.</summary>
	public Item First { get; set; } = null!;

	/// <summary>The second item, set before the dialog is shown.</summary>
	public Item Second { get; set; } = null!;

	/// <summary>What the dialog shows, set by <see cref="Fill"/>.</summary>
	internal ItemComparisonContent? Content { get; private set; }

	/// <summary>The toolbar choices.</summary>
	internal ItemComparisonOptions Options => new(this._alignRows.Checked, this._hideUnchanged.Checked, this._showSources.Checked);

	public ItemComparisonDialog(MainForm instance, IItemComparisonService comparisonService) : base(instance.Services)
	{
		this._comparisonService = comparisonService;
		this._builder = new ItemComparisonContentBuilder(this.Services.TranslationService);
		this.Owner = instance;

		// The DPI of the dialog's monitor: the toolbar and the button are made at the dialog's DPI before it has a
		// window (the system's), fonts with the user's zoom only, and Windows Forms scales them to its monitor's DPI when it
		// opens and when it moves to a monitor of another DPI (AutoScaleMode.Dpi); the columns, made again at each change,
		// are made at the dialog's DPI (ContentScale), and again when it changes (OnDpiChanged).
		this.AutoScaleDimensions = new SizeF(96F, 96F);
		this.AutoScaleMode = AutoScaleMode.Dpi;
		this.BackColor = DialogColor;
		this.ForeColor = Color.White;
		this.MaximizeBox = false;
		this.MinimizeBox = false;
		this.NormalizeBox = false;
		this.ShowIcon = false;
		this.ShowInTaskbar = false;
		this.StartPosition = FormStartPosition.CenterParent;
		this.Text = Resources.ItemComparisonText;
		this.Name = nameof(ItemComparisonDialog);

		float scale = this.ContentScale;
		int gap = (int)Math.Round(4 * scale);
		this.SetPadding();

		this._table = new TableLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			ColumnCount = 4,
			RowCount = 2,
			Margin = new Padding(0),
			Padding = new Padding(0),
			Location = Point.Empty,
			Name = "comparisonTable",
		};
		for (int i = 0; i < this._table.ColumnCount; i++)
			this._table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		this._table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		this._table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

		this._scroll = new Panel()
		{
			AutoScroll = true,
			Dock = DockStyle.Fill,
		};
		this._scroll.Controls.Add(this._table);

		// The marble shows through the scrolled area: Windows moves the drawn pixels when it scrolls, the marble with the
		// columns, so it is drawn again at its place.
		this._scroll.Scroll += (_, _) => this._scroll.Invalidate(true);
		this._scroll.MouseWheel += (_, _) => this._scroll.Invalidate(true);

		// OK closes, as Escape and the close button of the border do (shown modeless too); at the bottom right, as in Item
		// Properties.
		var ok = new ScalingButton()
		{
			Anchor = AnchorStyles.None,
			BackColor = Color.Transparent,
			DialogResult = DialogResult.OK,
			FlatStyle = FlatStyle.Flat,
			ForeColor = Theme.ButtonText,
			Name = "okButton",
			Size = new Size((int)(137 * scale), (int)(30 * scale)),
			SizeToGraphic = false,
			Text = Resources.GlobalOK,
			UseCustomGraphic = true,
			UseVisualStyleBackColor = false,
			Font = this.FontService.GetFontLight(CaravanLook.ButtonTextSize, this.UIService.Zoom),
		};
		ok.Click += (_, _) => this.Close();
		ok.FlatAppearance.BorderSize = 0;
		ok.FlatAppearance.MouseDownBackColor = Theme.TransparentButton;
		ok.FlatAppearance.MouseOverBackColor = Theme.TransparentButton;

		// Right-aligned with the last column's frame.
		int frameGap = (int)Math.Round(ColumnGap / 2 * scale);
		this._buttons = new Panel()
		{
			Dock = DockStyle.Bottom,
			Height = ok.Height + (2 * gap),
		};
		this._buttons.Controls.Add(ok);
		this._buttons.Resize += (_, _) => ok.Location = new Point(
			Math.Max(0, this._buttons.Width - ok.Width - frameGap), (this._buttons.Height - ok.Height) / 2);

		// The toolbar, first row of the dialog: changing a choice lays the columns out again.
		// The choices saved in UserConfig.xml (<ItemCompare ... />).
		var saved = ItemComparisonOptions.From(this.USettings.ItemCompare);
		this._alignRows = this.MakeToolbarCheckBox("alignRowsCheckBox", Resources.ItemComparisonAlignRows, saved.AlignRows);
		this._hideUnchanged = this.MakeToolbarCheckBox("hideUnchangedCheckBox", Resources.ItemComparisonHideUnchanged, saved.HideUnchanged);
		this._showSources = this.MakeToolbarCheckBox("showSourcesCheckBox", Resources.ItemComparisonShowSources, saved.ShowSources);
		this._toolbar = new FlowLayoutPanel()
		{
			Dock = DockStyle.Top,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = true,
			Padding = new Padding(0, 0, 0, gap),
			Name = "comparisonToolbar",
		};
		this._toolbar.Controls.Add(this._alignRows);
		this._toolbar.Controls.Add(this._hideUnchanged);
		this._toolbar.Controls.Add(this._showSources);

		this.Controls.Add(this._scroll);
		this.Controls.Add(this._toolbar);
		this.Controls.Add(this._buttons);
		this.AcceptButton = ok;
		this.CancelButton = ok;

		// Item Properties' look: the marble behind the toolbar, the columns and the button, the toolbar's text shadowed.
		CaravanLook.Apply(this, this._scroll, this._table, this._toolbar, this._buttons);
		// The toolbar check boxes over the marble in gold, outlined like the group titles over the frames.
		CaravanLook.OutlineText(this);

		this.ResizeCustomAllowed = true;
		this.DrawCustomBorder = true;
	}

	/// <summary>
	/// The scale of the columns, made at each change of the content, in pixels of the dialog's monitor: the user's zoom
	/// and that monitor's DPI.
	/// </summary>
	private float ContentScale => this.UIService.Zoom * this.DeviceDpi / 96F;

	/// <summary>
	/// The space between the border and the content, at the dialog's DPI: Windows Forms does not scale a window's own
	/// padding when its DPI changes.
	/// </summary>
	private void SetPadding()
	{
		int gap = (int)Math.Round(4 * this.ContentScale);
		var border = CustomBorderThickness;
		this.Padding = new Padding(border.Left + gap, border.Top + gap, border.Right + gap, border.Bottom + gap);
	}

	/// <summary>
	/// The dialog moved to a monitor of another DPI: Windows Forms scaled the toolbar and the button; the columns are made
	/// again at the new scale and the dialog fits them where it is.
	/// </summary>
	protected override void OnDpiChanged(DpiChangedEventArgs e)
	{
		base.OnDpiChanged(e);
		if (e.Cancel || this.Content is null)
			return;

		this.SetPadding();

		this.BeginInvoke(() =>
		{
			if (this.IsDisposed)
				return;

			var scroll = this._scroll.AutoScrollPosition;
			this.Fill();
			this.FitToContent(keepLocation: true);
			this._scroll.AutoScrollPosition = new Point(-scroll.X, -scroll.Y);
		});
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		// The window now has its monitor's DPI, which the padding made before it may not have had.
		this.SetPadding();
		this.Fill();
		this.FitToContent();
		this._alignRows.CheckedChanged += this.OptionChanged;
		this._hideUnchanged.CheckedChanged += this.OptionChanged;
		this._showSources.CheckedChanged += this.OptionChanged;
	}

	/// <summary>A check box of the toolbar, as Item Properties' "Filter Extra Info": white text over the marble.</summary>
	private ScalingCheckBox MakeToolbarCheckBox(string name, string text, bool isChecked)
		=> new()
		{
			AutoSize = true,
			Checked = isChecked,
			ForeColor = Color.Gold,
			BackColor = Color.Transparent,
			Font = this.FontService.GetFontLight(CaravanLook.TextSize, this.UIService.Zoom),
			Margin = new Padding((int)Math.Round(4 * this.ContentScale), 0, (int)Math.Round(12 * this.ContentScale), 0),
			Name = name,
			Text = text,
		};

	private void OptionChanged(object? sender, EventArgs e)
	{
		// "Show sources" opens or closes them all: the stats clicked one by one start again from it.
		if (sender == this._showSources)
			this._toggled.Clear();

		if (this.Options.ApplyTo(this.USettings))
			this.USettings.Save();

		// The dialog stays where it is, on the monitor the user may have moved it to.
		this.Fill();
		this.FitToContent(keepLocation: true);
	}

	/// <summary>
	/// A click on a stat with sources (its marker, its difference or its item line): shows or hides them, once the click is
	/// over (the rows are made again, the clicked one included).
	/// </summary>
	private void SourcesRowClicked(object? sender, EventArgs e)
	{
		if ((sender as Control)?.Tag is string key)
			this.BeginInvoke(() => this.ToggleSources(key));
	}

	/// <summary>Shows or hides the sources of the stat <paramref name="key"/>, against "Show sources".</summary>
	private void ToggleSources(string key)
	{
		if (!this._toggled.Remove(key))
			this._toggled.Add(key);

		// The rows under the click stay where they are: same scroll, the window grows from its top.
		var scroll = this._scroll.AutoScrollPosition;
		this.Fill();
		this.FitToContent(keepLocation: true);
		this._scroll.AutoScrollPosition = new Point(-scroll.X, -scroll.Y);
	}

	/// <summary>Compares <see cref="First"/> and <see cref="Second"/> and lays out the four columns.</summary>
	internal ItemComparisonContent Fill()
	{
		var content = this._builder.Build(this._comparisonService.Compare(this.First, this.Second), this.Options, this._toggled);
		this.Content = content;

		this._table.SuspendLayout();
		foreach (Control old in this._table.Controls.Cast<Control>().ToArray())
			old.Dispose();

		// Row 0: the pictures above the item columns, so both items' names start on the same line; the difference
		// columns start there too.
		Item?[] pictures = [null, this.First, this.Second, null];
		IReadOnlyList<TooltipLine>[] columns = [content.FirstDifferences, content.First, content.Second, content.SecondDifferences];
		// Each column under its picture in one caravan frame, drawn by the table in their margins.
		var panels = new FlowLayoutPanel[columns.Length];
		for (int i = 0; i < columns.Length; i++)
		{
			panels[i] = this.MakeColumn(columns[i], ColumnNames[i]);
			var picture = this.MakePicture(pictures[i], PictureNames[i]);
			this._table.Controls.Add(picture, i, 0);
			this._table.Controls.Add(panels[i], i, 1);
			CaravanFrame.FrameAround(picture, panels[i]);
		}

		// Aligned: each difference row as tall as the item row it faces, so that they stay level.
		if (content.Aligned)
		{
			AlignRows(panels[0], panels[1]);
			AlignRows(panels[3], panels[2]);
		}

		this._table.ResumeLayout();
		return content;
	}

	/// <summary>
	/// Gives each row of <paramref name="differences"/> and the row of <paramref name="item"/> it faces the height of the
	/// taller of the two (a bold name, a line with a line break).
	/// </summary>
	private static void AlignRows(FlowLayoutPanel differences, FlowLayoutPanel item)
	{
		int count = Math.Min(differences.Controls.Count, item.Controls.Count);
		for (int i = 0; i < count; i++)
		{
			var a = differences.Controls[i];
			var b = item.Controls[i];
			int height = Math.Max(a.PreferredSize.Height + a.Margin.Vertical, b.PreferredSize.Height + b.Margin.Vertical);
			SetRowHeight(a, height);
			SetRowHeight(b, height);
		}
	}

	/// <summary>Makes a row take <paramref name="height"/> with its margins; a delimiter keeps its line and grows its margins.</summary>
	private static void SetRowHeight(Control row, int height)
	{
		int own = row.PreferredSize.Height + row.Margin.Vertical;
		if (own >= height)
			return;

		if (row.AutoSize)
			row.MinimumSize = new Size(0, height - row.Margin.Vertical);
		else
		{
			int extra = height - own;
			row.Margin = new Padding(row.Margin.Left, row.Margin.Top + (extra / 2), row.Margin.Right, row.Margin.Bottom + extra - (extra / 2));
		}
	}

	/// <summary>The names of the four column panels, left to right.</summary>
	internal static readonly string[] ColumnNames = ["firstDifferences", "first", "second", "secondDifferences"];

	/// <summary>The names of the cells above the four columns, left to right: the item pictures above the item columns.</summary>
	internal static readonly string[] PictureNames = ["firstDifferencesPicture", "firstPicture", "secondPicture", "secondDifferencesPicture"];

	/// <summary>Space between two column frames, at 96 DPI and zoom 1: half on each side.</summary>
	private const float ColumnGap = 8F;

	/// <summary>
	/// The margin of a column's cells: the frame's thickness and half the gap on the sides, the frame's thickness on the
	/// outer side of the column (top for the picture, bottom for the lines).
	/// </summary>
	private Padding CellMargin(bool top)
	{
		int side = CaravanFrame.Border + (int)Math.Round(ColumnGap / 2 * this.ContentScale);
		return new Padding(side, top ? CaravanFrame.Border : 0, side, top ? 0 : CaravanFrame.Border);
	}

	/// <summary>
	/// The cell above a column: the item's picture, centered, at the size the sacks draw it; an empty cell on the column
	/// background for no item.
	/// </summary>
	private Panel MakePicture(Item? item, string name)
	{
		float scale = this.ContentScale;
		int padding = (int)Math.Round(6 * scale);
		var bitmap = item is null ? null : this.UIService.GetBitmap(item);
		var size = bitmap is null
			? Size.Empty
			: new Size((int)Math.Round(bitmap.Width * scale), (int)Math.Round(bitmap.Height * scale));

		var cell = new Panel()
		{
			BackColor = ColumnColor,
			Dock = DockStyle.Fill,
			Margin = this.CellMargin(top: true),
			MinimumSize = new Size(size.Width + (2 * padding), size.IsEmpty ? 0 : size.Height + (2 * padding)),
			Size = new Size(size.Width + (2 * padding), size.IsEmpty ? 0 : size.Height + (2 * padding)),
			Name = name,
		};

		// The bitmap belongs to the UI service's cache: drawn, never disposed here.
		if (bitmap is not null)
			cell.Paint += (_, e) => e.Graphics.DrawImage(bitmap, new Rectangle(
				(cell.ClientSize.Width - size.Width) / 2, (cell.ClientSize.Height - size.Height) / 2, size.Width, size.Height));

		return cell;
	}

	/// <summary>A column: the rows of its lines, top to bottom, on the tooltip background.</summary>
	private FlowLayoutPanel MakeColumn(IReadOnlyList<TooltipLine> lines, string name)
	{
		float scale = this.ContentScale;
		int padding = (int)Math.Round(6 * scale);
		var column = new FlowLayoutPanel()
		{
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			BackColor = ColumnColor,
			Padding = new Padding(padding),
			Margin = this.CellMargin(top: false),
			Dock = DockStyle.Fill,
			Name = name,
		};

		column.SuspendLayout();
		foreach (var line in lines)
		{
			var row = BaseTooltip.MakeLineRow(this.UIService, this.FontService, line, ColumnColor, this.UIService.Zoom);
			if (row is not null && line.ToggleKey is { } key)
				this.MakeToggle(row, key);
			column.Controls.Add(row);
		}

		column.ResumeLayout();
		return column;
	}

	/// <summary>A row a click on shows or hides the sources of the stat <paramref name="key"/>, its colored pieces included.</summary>
	private void MakeToggle(Control row, string key)
	{
		row.Tag = key;
		row.Cursor = Cursors.Hand;
		row.Click += this.SourcesRowClicked;
		foreach (Control piece in row.Controls)
			this.MakeToggle(piece, key);
	}

	/// <summary>
	/// Sizes the dialog to its columns and border, within the working area of its screen; the rest scrolls. Centered on
	/// its owner, on the owner's screen, or where it is, kept within the working area of the screen it is on, when
	/// <paramref name="keepLocation"/> (the user may have moved it to another monitor).
	/// </summary>
	private void FitToContent(bool keepLocation = false)
	{
		var area = Screen.FromControl(keepLocation ? this : this.Owner ?? this).WorkingArea;
		var table = this._table.PreferredSize;
		var content = new Size(Math.Max(table.Width, this._toolbar.PreferredSize.Width), table.Height);
		var frame = new Size(this.Padding.Horizontal, this.Padding.Vertical + this._buttons.Height + this._toolbar.PreferredSize.Height);
		int maxWidth = (int)(area.Width * 0.95);
		int maxHeight = (int)(area.Height * 0.9);

		int width = Math.Min(content.Width + frame.Width, maxWidth);
		int height = Math.Min(content.Height + frame.Height, maxHeight);

		// Room for the scroll bar that the other direction needs.
		if (content.Height + frame.Height > maxHeight)
			width = Math.Min(width + SystemInformation.GetVerticalScrollBarWidthForDpi(this.DeviceDpi), maxWidth);
		if (content.Width + frame.Width > maxWidth)
			height = Math.Min(height + SystemInformation.GetHorizontalScrollBarHeightForDpi(this.DeviceDpi), maxHeight);

		this.ClientSize = new Size(width, height);
		if (keepLocation)
		{
			this.Location = new Point(
				Math.Clamp(this.Left, area.Left, Math.Max(area.Left, area.Right - this.Width)),
				Math.Clamp(this.Top, area.Top, Math.Max(area.Top, area.Bottom - this.Height)));
		}
		else if (this.Owner is not null)
		{
			var owner = this.Owner.Bounds;
			this.StartPosition = FormStartPosition.Manual;
			this.Location = new Point(
				Math.Clamp(owner.Left + ((owner.Width - this.Width) / 2), area.Left, Math.Max(area.Left, area.Right - this.Width)),
				Math.Clamp(owner.Top + ((owner.Height - this.Height) / 2), area.Top, Math.Max(area.Top, area.Bottom - this.Height)));
		}
	}
}
