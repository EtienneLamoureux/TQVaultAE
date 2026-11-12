using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// Two items side by side in four columns: the first item's differences, its picture and classic tooltip, the second
/// item's picture and classic tooltip, the second item's differences, under a toolbar ("Align rows", "Hide unchanged").
/// The content is decided by <see cref="ItemComparisonContentBuilder"/>; this dialog only draws its lines, with the rows
/// of the item tooltip, in the TQ border of the other dialogs.
/// </summary>
internal sealed partial class ItemComparisonDialog : VaultForm
{
	/// <summary>Background of the columns, as the item tooltip's.</summary>
	private static readonly Color ColumnColor = Color.FromArgb(10, 10, 10);

	/// <summary>Background of the dialog, as Item Properties'.</summary>
	private static readonly Color DialogColor = Color.FromArgb(46, 31, 21);

	/// <summary>The space the TQ border takes on each side: its pictures are drawn at their own size.</summary>
	private static readonly Lazy<Padding> BorderPadding = new(() =>
	{
		using var top = Images.BorderTop;
		using var side = Images.BorderSide;
		using var bottom = Images.BorderBottom;
		return new Padding(side.Width, top.Height, side.Width, bottom.Height);
	});

	private readonly IItemComparisonService _comparisonService;
	private readonly ItemComparisonContentBuilder _builder;
	private readonly Panel _scroll;
	private readonly TableLayoutPanel _table;
	private readonly Panel _buttons;
	private readonly FlowLayoutPanel _toolbar;
	private readonly CheckBox _alignRows;
	private readonly CheckBox _hideUnchanged;

	/// <summary>The first item, set before the dialog is shown.</summary>
	public Item First { get; set; } = null!;

	/// <summary>The second item, set before the dialog is shown.</summary>
	public Item Second { get; set; } = null!;

	/// <summary>What the dialog shows, set by <see cref="Fill"/>.</summary>
	internal ItemComparisonContent? Content { get; private set; }

	/// <summary>The toolbar choices.</summary>
	internal ItemComparisonOptions Options => new(this._alignRows.Checked, this._hideUnchanged.Checked);

	public ItemComparisonDialog(MainForm instance, IItemComparisonService comparisonService) : base(instance.Services)
	{
		this._comparisonService = comparisonService;
		this._builder = new ItemComparisonContentBuilder(this.Services.TranslationService);
		this.Owner = instance;

		// Built in code and sized with UIService.Scale: no WinForms scaling on top.
		this.AutoScaleMode = AutoScaleMode.None;
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

		float scale = this.UIService.Scale;
		int gap = (int)Math.Round(4 * scale);
		var border = BorderPadding.Value;
		this.Padding = new Padding(border.Left + gap, border.Top + gap, border.Right + gap, border.Bottom);

		this._table = new TableLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			ColumnCount = 4,
			RowCount = 2,
			BackColor = DialogColor,
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
			BackColor = DialogColor,
		};
		this._scroll.Controls.Add(this._table);

		// OK closes, as Escape and the close button of the border do.
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
			Font = this.FontService.GetFontLight(12F, this.UIService.FontZoom),
		};
		ok.FlatAppearance.BorderSize = 0;
		ok.FlatAppearance.MouseDownBackColor = Theme.TransparentButton;
		ok.FlatAppearance.MouseOverBackColor = Theme.TransparentButton;

		this._buttons = new Panel()
		{
			Dock = DockStyle.Bottom,
			Height = ok.Height + (2 * gap),
			BackColor = DialogColor,
		};
		this._buttons.Controls.Add(ok);
		this._buttons.Resize += (_, _) => ok.Location = new Point((this._buttons.Width - ok.Width) / 2, (this._buttons.Height - ok.Height) / 2);

		// The toolbar, first row of the dialog: changing a choice lays the columns out again.
		// The choices saved in UserConfig.xml (<ItemCompare ... />).
		var saved = ItemComparisonOptions.From(this.USettings.ItemCompare);
		this._alignRows = this.MakeToolbarCheckBox("alignRowsCheckBox", Resources.ItemComparisonAlignRows, saved.AlignRows);
		this._hideUnchanged = this.MakeToolbarCheckBox("hideUnchangedCheckBox", Resources.ItemComparisonHideUnchanged, saved.HideUnchanged);
		this._toolbar = new FlowLayoutPanel()
		{
			Dock = DockStyle.Top,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = true,
			BackColor = DialogColor,
			Padding = new Padding(0, 0, 0, gap),
			Name = "comparisonToolbar",
		};
		this._toolbar.Controls.Add(this._alignRows);
		this._toolbar.Controls.Add(this._hideUnchanged);

		this.Controls.Add(this._scroll);
		this.Controls.Add(this._toolbar);
		this.Controls.Add(this._buttons);
		this.AcceptButton = ok;
		this.CancelButton = ok;

		this.ResizeCustomAllowed = true;
		this.DrawCustomBorder = true;
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		this.Fill();
		this.FitToContent();
		this._alignRows.CheckedChanged += this.OptionChanged;
		this._hideUnchanged.CheckedChanged += this.OptionChanged;
	}

	/// <summary>A check box of the toolbar, in the dialog's font and colors.</summary>
	private CheckBox MakeToolbarCheckBox(string name, string text, bool isChecked)
		=> new()
		{
			AutoSize = true,
			Checked = isChecked,
			// Light text on the dark brown background (the button text color is meant for the gold buttons).
			ForeColor = TQColor.Silver.Color(),
			BackColor = DialogColor,
			Font = this.FontService.GetFontLight(11F, this.UIService.FontZoom),
			Margin = new Padding((int)Math.Round(4 * this.UIService.Scale), 0, (int)Math.Round(12 * this.UIService.Scale), 0),
			Name = name,
			Text = text,
		};

	private void OptionChanged(object? sender, EventArgs e)
	{
		if (this.Options.ApplyTo(this.USettings))
			this.USettings.Save();
		this.Fill();
		this.FitToContent();
	}

	/// <summary>Compares <see cref="First"/> and <see cref="Second"/> and lays out the four columns.</summary>
	internal ItemComparisonContent Fill()
	{
		var content = this._builder.Build(this._comparisonService.Compare(this.First, this.Second), this.Options);
		this.Content = content;

		this._table.SuspendLayout();
		foreach (Control old in this._table.Controls.Cast<Control>().ToArray())
			old.Dispose();

		// Row 0: the pictures above the item columns, so both items' names start on the same line; the difference
		// columns start there too.
		Item?[] pictures = [null, this.First, this.Second, null];
		IReadOnlyList<TooltipLine>[] columns = [content.FirstDifferences, content.First, content.Second, content.SecondDifferences];
		var panels = new FlowLayoutPanel[columns.Length];
		for (int i = 0; i < columns.Length; i++)
		{
			panels[i] = this.MakeColumn(columns[i], ColumnNames[i]);
			this._table.Controls.Add(this.MakePicture(pictures[i], PictureNames[i]), i, 0);
			this._table.Controls.Add(panels[i], i, 1);
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

	/// <summary>Space between two columns, at 96 DPI and zoom 1.</summary>
	private const float ColumnGap = 2F;

	/// <summary>
	/// The cell above a column: the item's picture, centered, at the size the sacks draw it; an empty cell on the column
	/// background for no item.
	/// </summary>
	private Panel MakePicture(Item? item, string name)
	{
		float scale = this.UIService.Scale;
		int gap = (int)Math.Round(ColumnGap * scale);
		int padding = (int)Math.Round(6 * scale);
		var bitmap = item is null ? null : this.UIService.GetBitmap(item);
		var size = bitmap is null
			? Size.Empty
			: new Size((int)Math.Round(bitmap.Width * scale), (int)Math.Round(bitmap.Height * scale));

		var cell = new Panel()
		{
			BackColor = ColumnColor,
			Dock = DockStyle.Fill,
			Margin = new Padding(gap, 0, gap, 0),
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
		float scale = this.UIService.Scale;
		int padding = (int)Math.Round(6 * scale);
		int gap = (int)Math.Round(ColumnGap * scale);
		var column = new FlowLayoutPanel()
		{
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			BackColor = ColumnColor,
			Padding = new Padding(padding),
			Margin = new Padding(gap, 0, gap, 0),
			Dock = DockStyle.Fill,
			Name = name,
		};

		column.SuspendLayout();
		foreach (var line in lines)
			column.Controls.Add(BaseTooltip.MakeLineRow(this.UIService, this.FontService, line, ColumnColor));
		column.ResumeLayout();
		return column;
	}

	/// <summary>Sizes the dialog to its columns and border, within the working area of its screen; the rest scrolls.</summary>
	private void FitToContent()
	{
		var area = Screen.FromControl(this.Owner ?? this).WorkingArea;
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
		if (this.Owner is not null)
		{
			var owner = this.Owner.Bounds;
			this.StartPosition = FormStartPosition.Manual;
			this.Location = new Point(
				Math.Clamp(owner.Left + ((owner.Width - this.Width) / 2), area.Left, Math.Max(area.Left, area.Right - this.Width)),
				Math.Clamp(owner.Top + ((owner.Height - this.Height) / 2), area.Top, Math.Max(area.Top, area.Bottom - this.Height)));
		}
	}
}
