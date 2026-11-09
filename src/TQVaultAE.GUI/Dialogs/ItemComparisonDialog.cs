using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// Two items side by side in four columns: the first item's differences, its classic tooltip, the second item's classic
/// tooltip, the second item's differences. The content is decided by <see cref="ItemComparisonContentBuilder"/>; this
/// dialog only draws its lines, with the rows of the item tooltip.
/// </summary>
internal sealed partial class ItemComparisonDialog : Form
{
	/// <summary>Background of the columns, as the item tooltip's.</summary>
	private static readonly Color ColumnColor = Color.FromArgb(10, 10, 10);

	private readonly IItemComparisonService _comparisonService;
	private readonly ItemComparisonContentBuilder _builder;
	private readonly IUIService _uiService;
	private readonly IFontService _fontService;
	private readonly Panel _scroll;
	private readonly TableLayoutPanel _table;
	private readonly Panel _buttons;

	/// <summary>The first item, set before the dialog is shown.</summary>
	public Item First { get; set; } = null!;

	/// <summary>The second item, set before the dialog is shown.</summary>
	public Item Second { get; set; } = null!;

	/// <summary>What the dialog shows, set by <see cref="Fill"/>.</summary>
	internal ItemComparisonContent? Content { get; private set; }

	public ItemComparisonDialog(MainForm instance, IItemComparisonService comparisonService)
	{
		var services = instance.Services;
		this._comparisonService = comparisonService;
		this._builder = new ItemComparisonContentBuilder(services.TranslationService);
		this._uiService = services.UIService;
		this._fontService = services.FontService;
		this.Owner = instance;

		// Built in code and sized with UIService.Scale: no WinForms scaling on top.
		this.AutoScaleMode = AutoScaleMode.None;
		this.BackColor = TQColor.DarkGray.Color();
		this.FormBorderStyle = FormBorderStyle.Sizable;
		this.MinimizeBox = false;
		this.ShowInTaskbar = false;
		this.StartPosition = FormStartPosition.CenterParent;
				this.Text = Resources.ItemComparisonText;
		this.Name = nameof(ItemComparisonDialog);

		this._table = new TableLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			ColumnCount = 4,
			RowCount = 1,
			BackColor = TQColor.DarkGray.Color(),
			Margin = new Padding(0),
			Padding = new Padding(1),
			Location = Point.Empty,
			Name = "comparisonTable",
		};
		for (int i = 0; i < this._table.ColumnCount; i++)
			this._table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		this._table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

		this._scroll = new Panel()
		{
			AutoScroll = true,
			Dock = DockStyle.Fill,
			BackColor = TQColor.DarkGray.Color(),
		};
		this._scroll.Controls.Add(this._table);

		// OK closes, as Escape does.
		float scale = this._uiService.Scale;
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
			Font = this._fontService.GetFontLight(12F, this._uiService.FontZoom),
		};
		ok.FlatAppearance.BorderSize = 0;
		ok.FlatAppearance.MouseDownBackColor = Theme.TransparentButton;
		ok.FlatAppearance.MouseOverBackColor = Theme.TransparentButton;

		this._buttons = new Panel()
		{
			Dock = DockStyle.Bottom,
			Height = ok.Height + (int)(12 * scale),
			BackColor = ColumnColor,
		};
		this._buttons.Controls.Add(ok);
		this._buttons.Resize += (_, _) => ok.Location = new Point((this._buttons.Width - ok.Width) / 2, (this._buttons.Height - ok.Height) / 2);

		this.Controls.Add(this._scroll);
		this.Controls.Add(this._buttons);
		this.AcceptButton = ok;
		this.CancelButton = ok;
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		this.Fill();
		this.FitToContent();
	}

	/// <summary>Compares <see cref="First"/> and <see cref="Second"/> and lays out the four columns.</summary>
	internal ItemComparisonContent Fill()
	{
		var content = this._builder.Build(this._comparisonService.Compare(this.First, this.Second));
		this.Content = content;

		this._table.SuspendLayout();
		foreach (Control old in this._table.Controls.Cast<Control>().ToArray())
			old.Dispose();

		IReadOnlyList<TooltipLine>[] columns = [content.FirstDifferences, content.First, content.Second, content.SecondDifferences];
		for (int i = 0; i < columns.Length; i++)
			this._table.Controls.Add(this.MakeColumn(columns[i], ColumnNames[i]), i, 0);

		this._table.ResumeLayout();
		return content;
	}

	/// <summary>The names of the four column panels, left to right.</summary>
	internal static readonly string[] ColumnNames = ["firstDifferences", "first", "second", "secondDifferences"];

	/// <summary>A column: the rows of its lines, top to bottom, on the tooltip background.</summary>
	private FlowLayoutPanel MakeColumn(IReadOnlyList<TooltipLine> lines, string name)
	{
		int padding = (int)Math.Round(6 * this._uiService.Scale);
		var column = new FlowLayoutPanel()
		{
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			BackColor = ColumnColor,
			Padding = new Padding(padding),
			Margin = new Padding(1),
			Dock = DockStyle.Fill,
			Name = name,
		};

		column.SuspendLayout();
		foreach (var line in lines)
			column.Controls.Add(BaseTooltip.MakeLineRow(this._uiService, this._fontService, line, ColumnColor));
		column.ResumeLayout();
		return column;
	}

	/// <summary>Sizes the dialog to its columns, within the working area of its screen; the rest scrolls.</summary>
	private void FitToContent()
	{
		var area = Screen.FromControl(this.Owner ?? this).WorkingArea;
		var content = this._table.PreferredSize;
		int maxWidth = (int)(area.Width * 0.95);
		int maxHeight = (int)(area.Height * 0.9);

		int width = Math.Min(content.Width, maxWidth);
		int height = Math.Min(content.Height + this._buttons.Height, maxHeight);

		// Room for the scroll bar that the other direction needs.
		if (content.Height + this._buttons.Height > maxHeight)
			width = Math.Min(width + SystemInformation.GetVerticalScrollBarWidthForDpi(this.DeviceDpi), maxWidth);
		if (content.Width > maxWidth)
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
