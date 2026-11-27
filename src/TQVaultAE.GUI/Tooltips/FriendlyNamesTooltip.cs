using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Tooltips;

/// <summary>
/// A tool tip listing lines of text in a flow panel (<see cref="flowLayoutPanelFriendlyNames"/>), under an anchor: the
/// bag summaries, the search results and the search filters. It is made off screen, filled when it loads, then placed
/// under the anchor, as wide as the anchor's monitor allows (in columns).
/// </summary>
public class FriendlyNamesTooltip : BaseTooltip
{
	/// <summary>The lines, laid out top down then in columns. Made by the designer of each tool tip.</summary>
	protected FlowLayoutPanel flowLayoutPanelFriendlyNames = null!;

	/// <summary>The control the tool tip is shown under.</summary>
	internal Control AnchorControl { get; }

#if DEBUG
	// For Design Mode
#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services and state.
	public FriendlyNamesTooltip() { }
#pragma warning restore CS8618
#endif

	protected FriendlyNamesTooltip(GuiServices services, Form owner, Control anchor) : base(services)
	{
		this.Owner = owner;
		this.AnchorControl = anchor;

		this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

		// Filled out of view, so it does not flicker.
		this.Location = new Point(0, Screen.FromControl(this).WorkingArea.Height);
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		if (this.AnchorControl is null)
			return;

		// The tool tip may go multi-column, up to the size of the anchor's monitor.
		this.flowLayoutPanelFriendlyNames.MaximumSize = WorkingAreaOf(this.AnchorControl).Size;
		this.FillToolTip();
		this.PlaceUnder(this.AnchorControl);
	}

	/// <summary>Adds the lines, when the tool tip loads.</summary>
	protected virtual void FillToolTip()
	{
	}

	/// <summary>Adds a line (<see cref="BaseTooltip.MakeRow"/>), on the panel's background.</summary>
	protected void AddRow(string? text = TOOLTIPSPACER, Color? color = null, FontStyle style = FontStyle.Regular)
		=> this.flowLayoutPanelFriendlyNames.Controls.Add(
			MakeRow(this.UIService, this.FontService, text, color, 10F, style, BGColor: this.flowLayoutPanelFriendlyNames.BackColor));

	/// <summary>Adds one line per item name with its count, in the item's color (<see cref="ItemSummaryText"/>).</summary>
	protected void AddSummary(IEnumerable<ToFriendlyNameResult> names)
	{
		foreach (var line in ItemSummaryText.SummarizeByName(names))
			this.AddRow(line.Text, line.First.Item.ExtractTextColorOrItemColor(line.First.BaseItemInfoDescription));
	}

	/// <summary>A picture of the lines as drawn, to cache.</summary>
	protected Bitmap DrawLines()
	{
		var panel = this.flowLayoutPanelFriendlyNames;
		var picture = new Bitmap(panel.Width, panel.Height);
		panel.DrawToBitmap(picture, new Rectangle(0, 0, panel.Width, panel.Height));
		return picture;
	}
}
