using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Forms;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation;
using TQVaultAE.Config;

namespace TQVaultAE.GUI.Tooltips;

public partial class ItemTooltip : BaseTooltip
{
	private static Dictionary<Item, ItemTooltip> ItemTooltipOpened = [];
	/// <summary>Rendered item tooltips and their text, up to 48 MB.</summary>
	private static readonly TooltipImageCache<TooltipKey, ToFriendlyNameResult> ToImage = new(48L << 20);

	/// <summary>What a cached tooltip image depends on. <paramref name="SetCounts"/> lists the set piece counts shown, empty without them.</summary>
	private readonly record struct TooltipKey(Item Item, float Scale, ItemTooltipView View, bool InGameValues, string SetCounts);

	internal ItemTooltipView View { get; set; }
	internal bool ShowInGameValues { get; set; }
	/// <summary>The loaded items, counted for <see cref="ItemTooltipView.SetPieceCounts"/>.</summary>
	internal IItemDatabaseService? ItemDatabase { get; set; }
	internal Item FocusedItem { get; set; }
	internal SackPanel? SackPanel { get; set; }
	internal ResultsDialog? ResultsDialog { get; set; }
	internal PictureBox? PictureBox { get; set; }


	/// <summary>Gives an item linked to a mod its mod's data while the tooltip is built.</summary>
	internal IItemModService? ItemModService { get; init; }

	/// <summary>Text of the item, set by <see cref="FillToolTip"/>.</summary>
	internal ToFriendlyNameResult? Data { get; private set; }

	/// <summary>Attribute rows with a right-aligned range, sized to the tooltip width once all rows exist.</summary>
	private readonly List<(Panel Row, Control Left, Control Right)> rangeRows = [];

#if DEBUG
	// For Design Mode
#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services and state.
	public ItemTooltip() => InitializeComponent();
#pragma warning restore CS8618
#endif

	private ItemTooltip(MainForm instance, IItemProvider itemProvider, IFontService fontService, IUIService uiService, ITranslationService translationService, Item focusedItem) : base(itemProvider, fontService, uiService, translationService)
	{
		InitializeComponent();

		this.FocusedItem = focusedItem;

		this.Owner = instance;

		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

		// A layered window, so that WS_EX_TRANSPARENT lets the mouse through (see CreateParams).
		this.TransparencyKey = SkillsTransparencyKey;

		var wa = Screen.FromControl(this).WorkingArea;

		// Fill it outside of screen to avoid flickering
		this.Location = new Point(0, wa.Height);
	}

	public static void InvalidateCache(params Item?[] items)
	{
		var existing = items.OfType<Item>().ToArray();
		ToImage.RemoveWhere(k => existing.Contains(k.Item));
	}

	// to avoid Mainform lost focus with this.TopMost = false
	protected override bool ShowWithoutActivation => true;

	/// <summary>
	/// The mouse goes through the tooltip. A tooltip wider than the room beside the item covers the cursor:
	/// catching the mouse made the sack panel lose it and close the tooltip at once.
	/// </summary>
	protected override CreateParams CreateParams
	{
		get
		{
			const int WS_EX_TRANSPARENT = 0x20;
			var cp = base.CreateParams;
			cp.ExStyle |= WS_EX_TRANSPARENT;
			return cp;
		}
	}

	public static void HideTooltip()
	{
		lock (ToImage)
		{
			var lst = ItemTooltipOpened.Where(f => f.Value.Visible).ToList();
			foreach (var form in lst.Select(f => f.Value))
				form.Close();
			foreach (var key in lst.Select(f => f.Key))
				ItemTooltipOpened.Remove(key);
		}
	}

	#region Factory

	public static ItemTooltip ShowTooltip(
		GuiServices services
		, Item focusedItem
		, SackPanel sackPanel
		, bool? enableDetailedTooltipView = null
	)
	{
		ItemTooltip _Current;
		lock (ToImage)
		{
			HideTooltip();
				
			var userSettings = services.Settings;

			_Current = new ItemTooltip(
				services.Forms.MainForm
				, services.ItemProvider
				, services.FontService
				, services.UIService
				, services.TranslationService
				, focusedItem)
			{
				SackPanel = sackPanel,
				View = ViewOf(services, enableDetailedTooltipView),
				ShowInGameValues = userSettings.ShowInGameValues,
				ItemDatabase = services.ItemDatabaseService,
				ItemModService = services.ItemModService,
			};
			ItemTooltipOpened.Add(focusedItem, _Current);
			_Current.Show();
		}
		return _Current;
	}

	public static ItemTooltip ShowTooltip(
		GuiServices services
		, Item focusedItem
		, ResultsDialog resultsDialog
		, bool? enableDetailedTooltipView = null
	)
	{
		ItemTooltip _Current;
		lock (ToImage)
		{
			HideTooltip();

			var userSettings = services.Settings;

			_Current = new ItemTooltip(
				services.Forms.MainForm
				, services.ItemProvider
				, services.FontService
				, services.UIService
				, services.TranslationService
				, focusedItem)
			{
				ResultsDialog = resultsDialog,
				View = ViewOf(services, enableDetailedTooltipView),
				ShowInGameValues = userSettings.ShowInGameValues,
				ItemDatabase = services.ItemDatabaseService,
				ItemModService = services.ItemModService,
			};
			ItemTooltipOpened.Add(focusedItem, _Current);
			_Current.Show();
		}
		return _Current;
	}

	public static ItemTooltip ShowTooltip(
		GuiServices services
		, Item focusedItem
		, PictureBox picBox
		, bool? enableDetailedTooltipView = null
	)
	{
		ItemTooltip _Current;
		lock (ToImage)
		{
			HideTooltip();

			var userSettings = services.Settings;

			_Current = new ItemTooltip(
				services.Forms.MainForm
				, services.ItemProvider
				, services.FontService
				, services.UIService
				, services.TranslationService
				, focusedItem)
			{
				PictureBox = picBox,
				View = ViewOf(services, enableDetailedTooltipView),
				ShowInGameValues = userSettings.ShowInGameValues,
				ItemDatabase = services.ItemDatabaseService,
				ItemModService = services.ItemModService,
			};
			ItemTooltipOpened.Add(focusedItem, _Current);
			_Current.Show();
		}
		return _Current;
	}

	#endregion

	/// <summary>
	/// The display chosen in the settings: the classic one, or the detailed one with the extras checked in "Tooltip Enhanced".
	/// </summary>
	/// <param name="enableDetailedTooltipView">Forces the detailed display on or off, whatever the settings.</param>
	internal static ItemTooltipView ViewOf(UserSettings settings, bool? enableDetailedTooltipView = null)
		=> ItemTooltipContentBuilder.ViewOf(settings, enableDetailedTooltipView);

	/// <summary>
	/// The display a tooltip shows now: the settings, or the detailed display while the session forces it (the forge).
	/// </summary>
	internal static ItemTooltipView ViewOf(GuiServices services, bool? enableDetailedTooltipView = null)
		=> ItemTooltipContentBuilder.ViewOf(services.Settings, enableDetailedTooltipView, services.Session.ForceDetailedTooltipView);

	/// <summary>
	/// Init the tooltip content.
	/// </summary>
	public void FillToolTip()
	{
		// An item linked to a mod is described with that mod's data.
		using var modScope = this.ItemModService?.Scope(this.FocusedItem) ?? NoScope.Instance;

		// How many of each set piece are loaded now: part of the cache key, since it changes with the loaded items.
		IReadOnlyDictionary<RecordId, int>? setCounts = null;
		if (this.View.HasFlag(ItemTooltipView.SetPieceCounts) && this.ItemDatabase is not null
			&& this.ItemProvider.GetSetItems(this.FocusedItem) is { setMembers.Count: > 0 } set)
			setCounts = this.ItemDatabase.CountItemsByRecord(set.setMembers.Keys.Select(RecordId.Create));

		var key = new TooltipKey(this.FocusedItem, UIService.Scale, this.View, this.ShowInGameValues
			, setCounts is null ? string.Empty : string.Join(",", setCounts.Values));

		// Redraw
		if (ToImage.TryGetCopy(key, out var cached, out var data))
		{
			this.Data = data;
			this.ShowCompositeImage(cached);
			return;
		}

		this.SuspendLayout();
		this.flowLayoutPanelFriendlyNames.SuspendLayout();
		this.rangeRows.Clear();

		this.Data = this.ItemProvider.GetFriendlyNames(FocusedItem, ItemTooltipContentBuilder.ScopesFor(this.View));
		var content = new ItemTooltipContentBuilder(this.TranslationService).Build(this.Data, this.View, setCounts);

		// The relic lines may move to a column: note where their rows start and end.
		int relicRowsStart = 0;
		int relicRowsEnd = 0;
		for (int i = 0; i < content.Lines.Count; i++)
		{
			if (i == content.RelicStart)
				relicRowsStart = this.flowLayoutPanelFriendlyNames.Controls.Count;
			if (i == content.RelicEnd)
				relicRowsEnd = this.flowLayoutPanelFriendlyNames.Controls.Count;
			this.AddLine(content.Lines[i]);
		}

		this.AlignRangeRows();
		this.flowLayoutPanelFriendlyNames.ResumeLayout();
		this.ResumeLayout();

		// A tooltip taller than the screen gets cut: the relics then go in a column on the right.
		FlowLayoutPanel? relicColumn = null;
		if (relicRowsEnd > relicRowsStart
			&& this.flowLayoutPanelFriendlyNames.GetPreferredSize(Size.Empty).Height > this.MaxPanelHeight())
			relicColumn = this.MoveRowsToColumn(relicRowsStart, relicRowsEnd);

#if DEBUG
		if (this.AutomationRecorder is { } recorder)
		{
			recorder.Panel(this.flowLayoutPanelFriendlyNames);
			if (relicColumn is not null)
			{
				recorder.Column("relics");
				recorder.Panel(relicColumn);
			}
		}
#endif

		// The relic column and the granted skills go on the right, shown with the item panel as one cached image.
		if (relicColumn is not null || this.Data.GrantedSkills.Length != 0)
		{
			this.ShowComposite(key, relicColumn, this.Data.GrantedSkills);
			return;
		}

		// Rasterize flowLayoutPanelFriendlyNames and cache it for fast redraw,
		// with the gray frame the form padding draws around it: the cached redraw has no padding.
		using var panel = Rasterize(this.flowLayoutPanelFriendlyNames);
		var raster = new Bitmap(panel.Width + 2, panel.Height + 2);
		using (var g = Graphics.FromImage(raster))
			DrawFramed(g, panel, 0, this.BackColor);
		ToImage.Add(key, raster, Data);
	}

	private void AddRow(string? friendlyName = TOOLTIPSPACER, Color? FGColor = null, float fontSize = 10F, FontStyle style = FontStyle.Regular, Color? BGColor = null)
	{
		Control? row = MakeRow(this.UIService, this.FontService, friendlyName, FGColor, fontSize, style, BGColor: this.flowLayoutPanelFriendlyNames.BackColor);

		this.flowLayoutPanelFriendlyNames.Controls.Add(row);
	}

	/// <summary>Adds the row of a line built by <see cref="ItemTooltipContentBuilder"/>.</summary>
	private void AddLine(TooltipLine line)
	{
		switch (line.Kind)
		{
			case TooltipLineKind.Spacer:
				AddRow(TOOLTIPSPACER);
				break;
			case TooltipLineKind.Delimiter:
				AddRow(TOOLTIPDELIM);
				break;
			case TooltipLineKind.Range:
				this.flowLayoutPanelFriendlyNames.Controls.Add(this.MakeRangeRow(line.Text, line.Range ?? string.Empty));
				break;
			case TooltipLineKind.Warning:
				this.flowLayoutPanelFriendlyNames.Controls.Add(this.MakeWarningRow(line.Text, line.Color?.Color()));
				break;
			default:
				AddRow(line.Text, line.Color?.Color(), style: line.Bold ? FontStyle.Bold : FontStyle.Regular);
				break;
		}
	}

	/// <summary>
	/// A row with the attribute on the left and its range on the right. Its width is set by <see cref="AlignRangeRows"/>:
	/// a flow layout does not count rows stretched with anchors when it measures its widest row.
	/// </summary>
	private Panel MakeRangeRow(string line, string range)
	{
		var bg = this.flowLayoutPanelFriendlyNames.BackColor;
		var left = MakeRow(this.UIService, this.FontService, line, BGColor: bg)
			?? MakeSingleColorLabel(this.UIService, this.FontService, line, null, 10F, FontStyle.Regular, bg);
		var right = MakeSingleColorLabel(this.UIService, this.FontService, range, ItemTooltipContentBuilder.RangeColor.Color(), 10F, FontStyle.Regular, bg);

		var row = new Panel()
		{
			AutoSize = false,
			BackColor = bg,
			Margin = new Padding(0),
			Padding = new Padding(0),
			Anchor = AnchorStyles.Left,
		};

		left.Anchor = AnchorStyles.Top | AnchorStyles.Left;
		left.Location = Point.Empty;
		right.Anchor = AnchorStyles.Top | AnchorStyles.Left;

		row.Controls.Add(left);
		row.Controls.Add(right);
		this.rangeRows.Add((row, left, right));
		return row;
	}

	/// <summary>The warning icon of an incomplete item, drawn once.</summary>
	private static readonly Lazy<Bitmap> WarningIcon = new(() => SystemIcons.GetStockIcon(StockIconId.Warning, StockIconOptions.Default).ToBitmap());

	/// <summary>
	/// A row with a warning icon, as high as the text, then the text.
	/// </summary>
	private FlowLayoutPanel MakeWarningRow(string text, Color? color)
	{
		var bg = this.flowLayoutPanelFriendlyNames.BackColor;
		var label = MakeRow(this.UIService, this.FontService, text, color, BGColor: bg)
			?? MakeSingleColorLabel(this.UIService, this.FontService, text, color, 10F, FontStyle.Bold, bg);
		var size = Math.Max(label.PreferredSize.Height, (int)(16 * this.UIService.Scale));

		var row = new FlowLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			BackColor = bg,
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = false,
			Margin = new Padding(0),
			Padding = new Padding(0),
		};
		row.Controls.Add(new PictureBox()
		{
			Image = WarningIcon.Value,
			SizeMode = PictureBoxSizeMode.Zoom,
			Size = new Size(size, size),
			BackColor = bg,
			Margin = new Padding(0, 0, (int)(4 * this.UIService.Scale), 0),
		});
		label.Margin = new Padding(0);
		row.Controls.Add(label);
		return row;
	}

	/// <summary>
	/// Gives every range row the width of the widest tooltip row (or its own natural width if larger),
	/// so ranges line up on the right edge without truncating the attribute text.
	/// </summary>
	private void AlignRangeRows()
	{
		if (this.rangeRows.Count == 0)
			return;

		var gap = (int)(16 * this.UIService.Scale);
		int width = 0;
		foreach (Control control in this.flowLayoutPanelFriendlyNames.Controls)
		{
			if (control is Panel panel && this.rangeRows.Exists(r => r.Row == panel))
				continue;
			if ((control.Anchor & AnchorStyles.Right) == AnchorStyles.Right)
				continue; // stretched delimiters follow the others
			width = Math.Max(width, control.PreferredSize.Width + control.Margin.Horizontal);
		}

		foreach (var (_, left, right) in this.rangeRows)
			width = Math.Max(width, left.PreferredSize.Width + gap + right.PreferredSize.Width);

		foreach (var (row, left, right) in this.rangeRows)
		{
			var leftSize = left.PreferredSize;
			var rightSize = right.PreferredSize;
			row.Size = new Size(width, Math.Max(leftSize.Height, rightSize.Height));
			left.Size = leftSize;
			right.Size = rightSize;
			right.Location = new Point(width - rightSize.Width, 0);
		}
	}

	private void ItemTooltip_Load(object? sender, EventArgs e)
	{
		var usize = UIService.ItemUnitSize;
		var wa = Screen.FromControl(this).WorkingArea;

		this.FillToolTip();

		// Move it to view area from Mainform
		if (this.SackPanel is not null)
		{
			var loc = this.SackPanel.PointToScreen(Point.Empty);
			var x = loc.X + ((this.FocusedItem.PositionX + this.FocusedItem.Size.Width) * usize);
			var y = loc.Y + (this.FocusedItem.PositionY * usize);

			// Adjust for Equipment panel weapon slot
			if (this.SackPanel.SackType == SackType.Equipment)
			{
				if (this.FocusedItem.IsInWeaponSlot)
				{
					var itemX = SackCollection.GetWeaponLocationOffset(this.FocusedItem.PositionY).X;
					var itemY = SackCollection.GetWeaponLocationOffset(this.FocusedItem.PositionY).Y;
					x = loc.X + ((itemX + 2) * usize);
					y = loc.Y + (itemY * usize);
				}

				if (this.FocusedItem.IsAmulet)
					// Adjust for the whole size of the amulet box.
					x += (SackCollection.GetEquipmentLocationSize(1).Width - this.FocusedItem.Size.Width) * usize;
			}

			// Adjust position to avoid flickering if the tooltip appears on top of the cursor.
			var itemLeft = x - (this.FocusedItem.Size.Width * usize);
			x += Cursor.Size.Width;

			// The skills panel makes the tooltip wide: keep it on the screen of the panel,
			// on the left of the item when only that side has room, else as far right as it fits.
			var screen = Screen.FromPoint(loc).WorkingArea;
			if (x + this.Width > screen.Right)
			{
				var left = itemLeft - Cursor.Size.Width - this.Width;
				x = left >= screen.Left ? left : Math.Max(screen.Left, screen.Right - this.Width);
			}

			// Ajust position if tooltip size goes offscreen
			var bottom = y + this.Height;
			if (bottom > wa.Height)
			{
				// Maximize vertical view
				var offScreenHeight = bottom - wa.Height;
				if (y - offScreenHeight < 0)
					y = 0;// Do your best
				else
					y -= offScreenHeight;
			}

			this.Location = new Point(x, y);
			return;
		}

		// Move it to view area from Search form
		if (this.ResultsDialog is not null)
		{
			var loc = this.ResultsDialog.PointToScreen(Point.Empty);
			var x = loc.X + this.ResultsDialog.Width;
			var y = loc.Y;

			this.Location = new Point(x, y);
		}

		// Move it to view area from PictureBox (Forge)
		if (this.PictureBox is not null)
		{
			var loc = this.PictureBox.PointToScreen(Point.Empty);
			var x = loc.X + this.PictureBox.Width;
			var y = loc.Y;

			this.Location = new Point(x, y);
		}
	}
}