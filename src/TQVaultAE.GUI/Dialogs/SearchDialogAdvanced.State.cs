using TQVaultAE.Application.DTOs;
using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Models.Search;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// The user's state of the dialog, kept when the main window makes a new dialog at another zoom.
/// </summary>
public partial class SearchDialogAdvanced
{
	/// <summary>State of the dialog this one replaces, restored once the categories are built.</summary>
	private SearchDialogState? _pendingState;

	/// <summary>
	/// The items of <see cref="_pendingState"/> are those the categories are built from: the build replaces the item
	/// database by a new one, so this is checked before it.
	/// </summary>
	private bool _pendingStateSameItems;

	/// <summary>Size the layout gave the dialog (zoom, fitted categories): another size is the user's.</summary>
	private Size _layoutSize;

	/// <summary>What the user had set when the dialog last closed; <see langword="null"/> before.</summary>
	public SearchDialogState? ClosedState { get; private set; }

	/// <summary>
	/// Gives the new dialog the <paramref name="state"/> of the one it replaces: it is restored once the categories are
	/// built, when the dialog opens.
	/// </summary>
	public void RestoreWhenBuilt(SearchDialogState? state)
		=> this._pendingState = state;

	/// <summary>The state is read while the dialog still shows: a hidden control reads as not visible.</summary>
	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		base.OnFormClosing(e);
		if (!e.Cancel)
			this.ClosedState = this.CaptureState();
	}

	/// <summary>Records the size the layout gave the dialog.</summary>
	private void KeepLayoutSize()
		=> this._layoutSize = this.Size;

	/// <summary>
	/// Before the categories are built from <paramref name="database"/>: whether the given state was made from these
	/// items, as the kept dialog checks before its rebuild.
	/// </summary>
	private void CheckPendingStateItems(System.Collections.Concurrent.ConcurrentBag<Application.Results.SearchResult> database)
		=> this._pendingStateSameItems = this._pendingState is { } state
			&& ReferenceEquals(database, state.Database) && database.Count == state.DatabaseCount;

	/// <summary>Restores the state given by <see cref="RestoreWhenBuilt"/>, once the categories are built.</summary>
	private void RestorePendingState()
	{
		if (this._pendingState is not { } state || !this.Visible || this._builtDatabase is null)
			return;

		this._pendingState = null;
		this.RestoreState(state);
	}

	private SearchDialogState? CaptureState()
	{
		// Closed before the categories were built, or before the given state could be restored: that one is still it.
		if (this._builtDatabase is null || this._pendingState is not null)
			return this._pendingState;

		float zoom = this.ScaledZoom;
		var scroll = this.flowLayoutPanelMainAutoScroll.AutoScrollPosition;
		var size = this.WindowState == FormWindowState.Normal ? this.Size : this.RestoreBounds.Size;

		return new SearchDialogState
		{
			Query = this.CurrentQuery(),
			Database = this._builtDatabase,
			DatabaseCount = this._builtCount,
			SearchText = this.scalingTextBoxSearchTerm.Text,
			QueryText = this.scalingComboBoxQueryList.Text,
			SelectedQueryName = (this.scalingComboBoxQueryList.SelectedItem as SearchQuery)?.QueryName,
			CategoryOrder = this.flowLayoutPanelMain.Controls.Cast<Control>().Select(c => c.Name).ToList(),
			CollapsedLists = this.CachedCheckedListBoxes.Where(lb => !lb.VisibleSelf).Select(lb => lb.Name).ToHashSet(),
			Lists = this.CachedCheckedListBoxes.ToDictionary(lb => lb.Name, lb => new SearchListPosition(lb.FocusedIndex, lb.FirstColumn)),
			// AutoScrollPosition reads negative.
			Scroll = new PointF(-scroll.X / zoom, -scroll.Y / zoom),
			UserSize = size != this._layoutSize ? new SizeF(size.Width / zoom, size.Height / zoom) : null,
			Maximized = this.WindowState == FormWindowState.Maximized,
		};
	}

	/// <summary>
	/// Sets the dialog as <paramref name="state"/> says. Its filters are restored like a saved query's; when the items
	/// changed since, only the settings are, as the kept dialog does on a rebuild (its filters referred to the old items).
	/// </summary>
	private void RestoreState(SearchDialogState state)
	{
		bool sameItems = this._pendingStateSameItems;
		var query = sameItems ? state.Query : state.Query.SettingsOnly();
		var filters = sameItems ? this.AttachFilters(query, this.LiveFilters()) : [];

		// The saved query box without loading its query: the filters may have been changed since it was chosen.
		if (sameItems)
		{
			this.scalingComboBoxQueryList.SelectedIndexChanged -= this.scalingComboBoxQueryList_SelectedIndexChanged;
			try
			{
				var selected = this.scalingComboBoxQueryList.Items.OfType<SearchQuery>()
					.FirstOrDefault(q => q.QueryName == state.SelectedQueryName);
				if (selected is not null)
					this.scalingComboBoxQueryList.SelectedItem = selected;
				else
					this.scalingComboBoxQueryList.Text = state.QueryText;
			}
			finally
			{
				this.scalingComboBoxQueryList.SelectedIndexChanged += this.scalingComboBoxQueryList_SelectedIndexChanged;
			}
		}

		// The category filter first and without the typing delay, so that the lists are filled with it.
		this.scalingTextBoxFilterCategories.TextChanged -= this.scalingTextBoxFilterCategories_TextChanged;
		try
		{
			this.scalingTextBoxFilterCategories.Text = query.Filter;
		}
		finally
		{
			this.scalingTextBoxFilterCategories.TextChanged += this.scalingTextBoxFilterCategories_TextChanged;
		}
		this.ReadFilterCategories();

		this.Make_SelectedFilters(query, filters);

		// Text typed too recently to be applied shows as typed: Apply applies it, as it would have.
		if (sameItems)
			this.TextBoxSearchTerm_UpdateText_Notrigger(state.SearchText);

		// Category order (hiding and showing categories moves them) and collapsed lists.
		this.flowLayoutPanelMain.SuspendLayout();
		try
		{
			for (int i = 0; i < state.CategoryOrder.Count; i++)
			{
				if (this.flowLayoutPanelMain.Controls[state.CategoryOrder[i]] is { } category)
					this.flowLayoutPanelMain.Controls.SetChildIndex(category, i);
			}
		}
		finally
		{
			this.flowLayoutPanelMain.ResumeLayout(true);
		}
		this.ShowCategoryLists(lb => !state.CollapsedLists.Contains(lb.Name));

		float zoom = this.ScaledZoom;
		if (state.Maximized)
			this.WindowState = FormWindowState.Maximized;
		else if (state.UserSize is { } userSize)
		{
			var screen = Screen.FromControl(this).WorkingArea;
			this.Size = new Size(
				Math.Min((int)Math.Round(userSize.Width * zoom), screen.Width),
				Math.Min((int)Math.Round(userSize.Height * zoom), screen.Height));
			this.LastFormSize = this.Size;
		}

		// Scroll positions, once the lists have their sizes.
		this.flowLayoutPanelMainAutoScroll.PerformLayout();
		foreach (var lb in this.CachedCheckedListBoxes)
		{
			if (!state.Lists.TryGetValue(lb.Name, out var position))
				continue;
			if (position.FocusedIndex >= 0)
				lb.FocusedIndex = position.FocusedIndex;
			lb.FirstColumn = position.FirstColumn;
		}
		this.flowLayoutPanelMainAutoScroll.AutoScrollPosition
			= new Point((int)Math.Round(state.Scroll.X * zoom), (int)Math.Round(state.Scroll.Y * zoom));
	}
}
