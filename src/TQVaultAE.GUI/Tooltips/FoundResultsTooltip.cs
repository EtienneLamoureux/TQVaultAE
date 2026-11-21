using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation;
using TQVaultAE.Application.Results;

namespace TQVaultAE.GUI.Tooltips;

/// <summary>The summary of the items a search found, under the result's control in the search dialog.</summary>
public partial class FoundResultsTooltip : FriendlyNamesTooltip
{
	/// <summary>Rendered search result summaries, up to 16 MB.</summary>
	private static readonly TooltipImageCache<(string ResultCollectionHash, float Scale), object?> ToImage = new(16L << 20);

	private static FoundResultsTooltip? _Current = null;
	public IEnumerable<SearchResult> ResultsToDisplay { get; private set; } = [];

#if DEBUG
	// For Design Mode
#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services and state.
	public FoundResultsTooltip() => InitializeComponent();
#pragma warning restore CS8618
#endif

	private FoundResultsTooltip(SearchDialogAdvanced instance, GuiServices services, Control anchorControl) : base(services, instance, anchorControl)
		=> InitializeComponent();

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

		lock (ToImage)
		{
			HideTooltip();
			_Current = new FoundResultsTooltip((SearchDialogAdvanced)anchorControl.FindForm()!, services, anchorControl)
			{
				ResultsToDisplay = results,
			};
			_Current.Show();
		}
		return _Current;
	}

	/// <summary>Summarizes the items found; drawn once, then shown from the cache.</summary>
	protected override void FillToolTip()
	{
		// Tooltip is grotesque at some point & you need to fine tune your search
		if (ResultsToDisplay.Count() > 250)
		{
			this.AddRow(Resources.SearchTooManyResultToDisplay, ItemStyle.Broken.Color());
			return;
		}

		var key = (MakeHash(this.ResultsToDisplay), UIService.Scale);
		if (ToImage.TryGetCopy(key, out var cached, out _))
		{
			this.ShowCachedImage(cached);
			return;
		}

		this.SuspendLayout();
		this.flowLayoutPanelFriendlyNames.SuspendLayout();

		if (!ResultsToDisplay.Any())
			this.AddRow(Resources.VaultGroupBoxEmpty, ItemStyle.Broken.Color());
		else
			this.AddSummary(ResultsToDisplay.Select(i => i.FriendlyNames).OfType<ToFriendlyNameResult>());

		this.flowLayoutPanelFriendlyNames.ResumeLayout();
		this.ResumeLayout();

		ToImage.Add(key, this.DrawLines(), null);
	}
}
