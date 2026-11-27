using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Buttons;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Tooltips;

/// <summary>The summary of a bag's items, under its button.</summary>
public partial class BagButtonTooltip : FriendlyNamesTooltip
{
	/// <summary>The tool tip shown, if any: one at a time.</summary>
	private static BagButtonTooltip? _Current;
	/// <summary>Rendered sack summaries, up to 16 MB.</summary>
	private static readonly TooltipImageCache<(SackCollection Sack, float Scale), object?> ToImage = new(16L << 20);

#if DEBUG
	// For Design Mode
#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services and state.
	public BagButtonTooltip() => InitializeComponent();
#pragma warning restore CS8618
#endif

	private BagButtonTooltip(GuiServices services, BagButtonBase button) : base(services, services.Forms.MainForm, button)
		=> InitializeComponent();

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
			_Current = new BagButtonTooltip(services, button);
			_Current.Show();
			return _Current;
		}
	}

	/// <summary>Summarizes the items of the bag; drawn once, then shown from the cache.</summary>
	protected override void FillToolTip()
	{
		if (((BagButtonBase)this.AnchorControl).Sack is not { } sack)
			return;

		var key = (sack, UIService.Scale);
		if (ToImage.TryGetCopy(key, out var cached, out _))
		{
			this.ShowCachedImage(cached);
			return;
		}

		this.SuspendLayout();
		this.flowLayoutPanelFriendlyNames.SuspendLayout();

		if (sack.IsEmpty)
			this.AddRow(Resources.VaultGroupBoxEmpty, ItemStyle.Broken.Color());
		else
			this.AddSummary(sack
				.Where(i => !RecordId.IsNullOrEmpty(i.BaseItemId))// skip empty items
				.Select(i => this.ItemText.GetFriendlyNames(i)));

		this.flowLayoutPanelFriendlyNames.ResumeLayout();
		this.ResumeLayout();

		// Cached inside the tool tip border and shown at once, so the first display has the same border as the cached ones.
		using var lines = this.DrawLines();
		ToImage.Add(key, this.Framed(lines, this.flowLayoutPanelFriendlyNames.BackColor), null);
		if (ToImage.TryGetCopy(key, out var shown, out _))
			this.ShowCachedImage(shown);
	}
}
