﻿﻿using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Buttons;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Forms;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.GUI.Tooltips;

public partial class BagButtonTooltip : BaseTooltip
{
	/// <summary>The tool tip shown, if any: one at a time.</summary>
	private static BagButtonTooltip? _Current;
	/// <summary>Rendered sack summaries, up to 16 MB.</summary>
	private static readonly TooltipImageCache<(SackCollection Sack, float Scale), object?> ToImage = new(16L << 20);

	public required BagButtonBase ButtonSack;


#if DEBUG
	// For Design Mode
#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services and state.
	public BagButtonTooltip() => InitializeComponent();
#pragma warning restore CS8618
#endif

	private BagButtonTooltip(GuiServices services) : base(services)
	{
		InitializeComponent();

		this.Owner = services.Forms.MainForm;

		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

		// Fill it outside of screen to avoid flickering
		this.Location = new Point(0, Screen.FromControl(this).WorkingArea.Height);
	}

	public static void InvalidateCache(params SackCollection?[] sack)
	{
		var sacks = sack.OfType<SackCollection>().ToArray();
		ToImage.RemoveWhere(k => sacks.Contains(k.Sack));
	}

	public static void InvalidateCache(params Item?[] items)
	{
		var existing = items.OfType<Item>().ToArray();
		ToImage.RemoveWhere(k => k.Sack.Intersect(existing).Any());
	}

	public static void HideTooltip()
	{
		lock (ToImage)
			_Current = CloseAndDispose(_Current);
	}

	public static BagButtonTooltip? ShowTooltip(GuiServices services, BagButtonBase button)
	{
		if (button?.Sack is null) return null;
		lock (ToImage)
		{
			HideTooltip();
			_Current = new BagButtonTooltip(services)
			{
				ButtonSack = button
			};
			_Current.Show();
			return _Current;
		}
	}


	/// <summary>
	/// Init the tooltip content for a sack.  Summarizes the items within the sack
	/// </summary>
	/// <param name="button">button the mouse is over.  Corresponds to a sack</param>
	/// <returns>string to be displayed in the tooltip</returns>
	public void FillSackToolTip()
	{
		if (this.ButtonSack.Sack is not { } sack)
			return;

		var key = (sack, UIService.Scale);

		// Redraw
		if (ToImage.TryGetCopy(key, out var cached, out _))
		{
			this.ShowCachedImage(cached);
			return;
		}

		this.SuspendLayout();

		this.flowLayoutPanelFriendlyNames.SuspendLayout();

		if (sack.IsEmpty)
			AddRow(Resources.VaultGroupBoxEmpty, ItemStyle.Broken.Color());
		else
		{

			var itemlist = ButtonSack.Sack
				.Where(i => !RecordId.IsNullOrEmpty(i.BaseItemId))// skip empty items
				.ToArray();
			var friendlylist = ItemSummaryText.SummarizeByName(itemlist
				.Select(i => ItemProvider.GetFriendlyNames(i)));

			foreach (var line in friendlylist)
				AddRow(line.Text, line.First.Item.ExtractTextColorOrItemColor(line.First.BaseItemInfoDescription));

		}

		this.flowLayoutPanelFriendlyNames.ResumeLayout();
		this.ResumeLayout();

		// Rasterize flowLayoutPanelFriendlyNames inside the tooltip border and cache it for fast redraw; the image is
		// shown at once, so the first display has the same border as the cached ones.
		var w = this.flowLayoutPanelFriendlyNames.Width;
		var h = this.flowLayoutPanelFriendlyNames.Height;
		using var panel = new Bitmap(w, h);
		this.flowLayoutPanelFriendlyNames.DrawToBitmap(panel, new Rectangle(0, 0, w, h));
		ToImage.Add(key, this.Framed(panel, this.flowLayoutPanelFriendlyNames.BackColor), null);
		if (ToImage.TryGetCopy(key, out var shown, out _))
			this.ShowCachedImage(shown);
	}

	private void AddRow(string friendlyName, Color color)
	{
		Control? row = MakeRow(this.UIService, this.FontService, friendlyName, color, 10F, FontStyle.Regular, BGColor: this.flowLayoutPanelFriendlyNames.BackColor);

		this.flowLayoutPanelFriendlyNames.Controls.Add(row);
	}

	private void BagButtonTooltip_Load(object? sender, EventArgs e)
	{
		// The tool tip may go multi-column, up to the size of the anchor's monitor.
		this.flowLayoutPanelFriendlyNames.MaximumSize = WorkingAreaOf(this.ButtonSack).Size;

		this.FillSackToolTip();

		this.PlaceUnder(this.ButtonSack);
	}


}