using System.Diagnostics;
using System.Text;

namespace TQVaultAE.GUI.Components.Layout;

/// <summary>Debug access to the check box lists: checks and text widths.</summary>
public partial class VirtualCheckedListBox
{
	/// <summary>Checks or unchecks an entry like a user (<see cref="ToggleItem"/>), so <see cref="SelectedValueChanged"/> follows; nothing when it already is.</summary>
	internal void AutomationSetChecked(int index, bool check)
	{
		if (this.GetItemChecked(index) != check)
			this.ToggleItem(index);
	}

	/// <summary>
	/// <see cref="GetMaxTextWidth"/> against measuring every text, with the time of each and the largest gap between a
	/// text's real width and its estimate (the margin must cover it).
	/// </summary>
	internal object AutomationTextWidths()
	{
		var watch = Stopwatch.StartNew();
		var (fast, measured) = this.MeasureMaxTextWidth();
		long fastMs = watch.ElapsedMilliseconds;

		var texts = this._items.Select(i => i?.ToString() ?? string.Empty).Distinct(StringComparer.Ordinal).ToList();
		using var g = this.CreateGraphics();
		int Measure(string text) => TextRenderer.MeasureText(g, text, this.Font, Size.Empty, MeasureFlags).Width;

		watch.Restart();
		var widths = texts.Select(t => (text: t, width: Measure(t))).ToList();
		long allMs = watch.ElapsedMilliseconds;
		var widest = widths.Count == 0 ? (text: string.Empty, width: 0) : widths.MaxBy(w => w.width);

		var runeWidths = new Dictionary<Rune, int>();
		int Estimate(string text) => text.EnumerateRunes().Sum(r =>
		{
			if (!runeWidths.TryGetValue(r, out int w))
				runeWidths[r] = w = Measure(r.ToString());
			return w;
		});
		var gaps = widths.Select(w => (w.text, gap: w.width - Estimate(w.text))).ToList();
		var worst = gaps.Count == 0 ? (text: string.Empty, gap: 0) : gaps.MaxBy(x => x.gap);

		return new
		{
			name = this.Name,
			texts = texts.Count,
			maxTextWidth = fast,
			measuredTexts = measured,
			fastMs,
			maxTextWidthAll = widest.width,
			widestText = widest.text,
			allMs,
			same = fast == widest.width,
			margin = this.EstimateMargin(widest.width),
			largestUnderestimate = worst.gap,
			largestUnderestimateText = worst.text,
			// What the former rule found: the widest of the 200 longest texts by character count.
			widestByLength200 = widths.OrderByDescending(w => w.text.Length).Take(200).Select(w => w.width).DefaultIfEmpty().Max(),
		};
	}
}
