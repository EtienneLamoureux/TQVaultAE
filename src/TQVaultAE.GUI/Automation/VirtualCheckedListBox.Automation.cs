using System.Diagnostics;
using System.Text;

namespace TQVaultAE.GUI.Components.Layout;

public partial class VirtualCheckedListBox
{
	/// <summary>Keyboard focus and scroll position, as drawn.</summary>
	internal object AutomationState()
	{
		int rows = this.RowsPerColumn;
		int firstVisible = this.FirstColumn * rows;
		int lastVisible = Math.Min(this._items.Count, (this.FirstColumn + this.VisibleColumns) * rows) - 1;
		return new
		{
			name = this.Name,
			count = this._items.Count,
			focused = this.Focused,
			focusedIndex = this._focusIndex,
			focusedText = this._focusIndex >= 0 ? this._items[this._focusIndex]?.ToString() : null,
			focusedChecked = this._focusIndex >= 0 && this._checked[this._focusIndex],
			focusRectangleDrawn = this._focusIndex >= 0 && this.Focused && this._focusIndex >= firstVisible && this._focusIndex <= lastVisible,
			rowsPerColumn = rows,
			visibleColumns = this.VisibleColumns,
			scrollBarVisible = this._scroll.Visible,
			firstColumn = this.FirstColumn,
			firstVisibleIndex = firstVisible,
			lastVisibleIndex = lastVisible,
			columnWidth = this.ColumnWidth,
			width = this.Width,
			textLeft = this.TextLeft,
		};
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
