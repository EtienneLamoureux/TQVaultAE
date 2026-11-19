using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;

namespace TQVaultAE.GUI.Tooltips;

public partial class FoundResultsTooltip : BaseTooltip
{
	/// <summary>Rendered search result summaries, up to 16 MB.</summary>
	private static readonly TooltipImageCache<(string ResultCollectionHash, float Scale), object?> ToImage = new(16L << 20);

	private static FoundResultsTooltip? _Current = null;
	public Control AnchorControl { get; private set; }
	public IEnumerable<SearchResult> ResultsToDisplay { get; private set; } = [];

#if DEBUG
	// For Design Mode
#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services and state.
	public FoundResultsTooltip() => InitializeComponent();
#pragma warning restore CS8618
#endif

	private FoundResultsTooltip(SearchDialogAdvanced instance, GuiServices services, Control anchorControl) : base(services)
	{
		InitializeComponent();

		this.AnchorControl = anchorControl;

		this.Owner = instance;

		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

		// Fill it outside of screen to avoid flickering
		this.Location = new Point(0, Screen.FromControl(this).WorkingArea.Height);
	}


	public static void InvalidateCache(params IEnumerable<SearchResult>[] resultlistCollection)
	{
		var hashlisttoremove = resultlistCollection.Where(s => s?.Any() ?? false).Select(l => MakeHash(l));

		ToImage.RemoveWhere(k => hashlisttoremove.Contains(k.ResultCollectionHash));
	}

	private static string MakeHash(IEnumerable<SearchResult> list)
	{
		var input = string.Join(string.Empty, list.Select(bi => bi.IdString).ToArray());
		return input.MakeMD5();
	}

	public static void HideTooltip()
	{
		lock (ToImage)
		{
			if (_Current is not null) _Current.Close();
			_Current = null;
		}
	}

	public static FoundResultsTooltip? ShowTooltip(GuiServices services, Control anchorControl, IEnumerable<SearchResult> results)
	{
		if (anchorControl is null) return null;
		if (!(results?.Any() ?? false)) return null;

		FoundResultsTooltip newTT;
		lock (ToImage)
		{
			HideTooltip();
			newTT = new FoundResultsTooltip(
				(SearchDialogAdvanced)anchorControl.FindForm()!
				, services
				, anchorControl
			)
			{
				ResultsToDisplay = results,
			};
			_Current = newTT;
			newTT.Show();
		}
		return _Current;
	}


	/// <summary>
	/// Init the tooltip content for a sack.  Summarizes the items within the sack
	/// </summary>
	/// <param name="button">button the mouse is over.  Corresponds to a sack</param>
	/// <returns>string to be displayed in the tooltip</returns>
	public void FillToolTip()
	{
		// Tooltip is grotesque at some point & you need to fine tune your search
		if (ResultsToDisplay.Count() > 250)
			AddRow(Resources.SearchTooManyResultToDisplay, ItemStyle.Broken.Color());
		else
		{
			var key = (MakeHash(this.ResultsToDisplay), UIService.Scale);

			// Redraw
			if (ToImage.TryGetCopy(key, out var cached, out _))
			{
				this.ShowCachedImage(cached);
				return;
			}

			this.SuspendLayout();

			this.flowLayoutPanelFriendlyNames.SuspendLayout();

			if (!ResultsToDisplay.Any())
				AddRow(Resources.VaultGroupBoxEmpty, ItemStyle.Broken.Color());
			else
			{

				var friendlylist = ItemSummaryText.SummarizeByName(ResultsToDisplay
					.Select(i => i.FriendlyNames)
					.OfType<ToFriendlyNameResult>());

				foreach (var line in friendlylist)
					AddRow(line.Text, line.First.Item.ExtractTextColorOrItemColor(line.First.BaseItemInfoDescription));

			}

			this.flowLayoutPanelFriendlyNames.ResumeLayout();
			this.ResumeLayout();

			// Rasterize flowLayoutPanelFriendlyNames and cache it for fast redraw
			var w = this.flowLayoutPanelFriendlyNames.Width;
			var h = this.flowLayoutPanelFriendlyNames.Height;
			var raster = new Bitmap(w, h);
			this.flowLayoutPanelFriendlyNames.DrawToBitmap(raster, new Rectangle(0, 0, w, h));
			ToImage.Add(key, raster, null);
		}
	}

	private void AddRow(string friendlyName, Color color)
	{
		Control? row = MakeRow(this.UIService, this.FontService, friendlyName, color, 10F, FontStyle.Regular, BGColor: this.flowLayoutPanelFriendlyNames.BackColor);
		this.flowLayoutPanelFriendlyNames.Controls.Add(row);
	}

	private void ItemTooltip_Load(object? sender, EventArgs e)
	{
		// The tool tip may go multi-column, up to the size of the anchor's monitor.
		this.flowLayoutPanelFriendlyNames.MaximumSize = WorkingAreaOf(this.AnchorControl).Size;

		this.FillToolTip();

		this.PlaceUnder(this.AnchorControl);
	}


}