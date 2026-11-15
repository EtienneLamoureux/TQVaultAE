using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// The compare selection (<see cref="Domain.Actions.ItemCompareSelection"/>, shared through <see cref="ItemDragInfo"/>):
/// the item menu's Compare entry starts it, the next left click on an item in any panel opens the comparison, Escape or
/// a right click cancels it. While it is on, every panel shows a hand cursor and the first item framed.
/// </summary>
public partial class SackPanel
{
	/// <summary>Color of the frame around the first item of the compare selection.</summary>
	private static readonly Color CompareFrameColor = TQColor.Aqua.Color();

	private bool _compareSubscribed;

	/// <summary>Follows the compare selection from now on: cursor and frame change when it starts or ends.</summary>
	private void SubscribeCompareSelection()
	{
		if (this._compareSubscribed)
			return;

		this.DragInfo.Compare.Changed += this.CompareSelectionChanged;
		this._compareSubscribed = true;
	}

	private void CompareSelectionChanged(object? sender, EventArgs e)
	{
		if (this.IsDisposed || this.Disposing)
			return;

		this.Cursor = this.DragInfo.Compare.IsActive ? Cursors.Hand : Cursors.Default;
		this.Invalidate();
	}

	/// <summary>Starts the compare selection with <paramref name="first"/>, from the item menu.</summary>
	private void StartCompareSelection(Item first)
	{
		this.DragInfo.Compare.Start(first);
		this.UIService.NotifyUser(Resources.SackPanelCompareSelectionStarted, TQColor.Aqua);
	}

	/// <summary>
	/// A mouse button pressed while the compare selection is on: a left click on an item opens the comparison (on the
	/// first item itself, it ends the selection), a right click cancels. The item is never taken.
	/// </summary>
	/// <returns><see langword="true"/> when the click belonged to the compare selection.</returns>
	private bool CompareSelectionMouseDown(MouseEventArgs e)
	{
		var selection = this.DragInfo.Compare;
		if (!selection.IsActive || this.DragInfo.IsActive)
			return false;

		if (e.Button == MouseButtons.Right)
			selection.Cancel();
		else if (e.Button == MouseButtons.Left && selection.Pick(this.FindItem(this.FindCell(e.Location))) is { } pair)
			// After the mouse handler: the new window takes the focus.
			this.BeginInvoke(() => this.ShowComparison(pair.First, pair.Second));

		return true;
	}

	/// <summary>
	/// Opens the item comparison of <paramref name="first"/> and <paramref name="second"/> in a new window, through the
	/// main content (<see cref="IItemComparisonHost"/>).
	/// </summary>
	private protected void ShowComparison(Item first, Item second)
		=> this.FindHost<IItemComparisonHost>()?.ShowComparison(first, second);

	/// <summary>The first parent of this panel that is a <typeparamref name="THost"/>: the main content.</summary>
	private protected THost? FindHost<THost>() where THost : class
	{
		for (var parent = this.Parent; parent is not null; parent = parent.Parent)
		{
			if (parent is THost host)
				return host;
		}

		return null;
	}

	/// <summary>Frames the first item of the compare selection when this panel shows it.</summary>
	private void PaintCompareSelection(Graphics graphics)
	{
		if (this.Sack is null || this.DragInfo.Compare.First is not { } first || !this.ShownSack.Any(i => ReferenceEquals(i, first)))
			return;

		var rect = this.GetItemScreenRectangle(first);
		if (rect.IsEmpty)
			return;

		float width = Math.Max(2F, 3F * this.UIService.Scale);
		using var pen = new Pen(CompareFrameColor, width) { Alignment = System.Drawing.Drawing2D.PenAlignment.Inset };
		graphics.DrawRectangle(pen, rect);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && this._compareSubscribed)
		{
			this.DragInfo.Compare.Changed -= this.CompareSelectionChanged;
			this._compareSubscribed = false;
		}

		if (disposing)
			this.numberFont.Dispose();

		base.Dispose(disposing);
	}
}
