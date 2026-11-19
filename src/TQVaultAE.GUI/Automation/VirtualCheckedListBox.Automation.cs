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
}
