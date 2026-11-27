using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>
/// Tab IV, the Buffs tab: every source of bonuses or maluses of the character by section (gear as text rows, the others
/// as icons), greyed when off; a click toggles one, the wheel scrolls the list. A scroll picked in the drop-down at the
/// top adds its buff to the Scroll section.
/// </summary>
internal sealed partial class CharacterStatsPanel
{
	/// <summary>The Buffs tab.</summary>
	public const int BuffsTab = 3;

	/// <summary>The list under the scroll choice, in game pixels: it scrolls between these rows.</summary>
	private const int BuffTop = 52;
	private const int BuffBottom = 386;
	private const int BuffLeft = 15;
	private const int BuffWidth = 228;

	/// <summary>Icons: 26 pixels, 8 per row every 28 pixels; text rows 16 pixels; a section title 20.</summary>
	private const int BuffIcon = 26;
	private const int BuffStep = 28;
	private const int BuffPerRow = 8;
	private const int BuffRowHeight = 16;
	private const int BuffTitleHeight = 20;
	private const int BuffSectionGap = 6;

	/// <summary>The scroll choice, at the top of the tab.</summary>
	private static readonly Rectangle ScrollBoxArea = new(90, 28, 153, 18);

	private static readonly Color BuffOffText = Color.FromArgb(110, 110, 105);

	private GameDropDown? _scrollBox;
	private Font? _scrollBoxFont;
	private bool _syncingScrollBox;
	private int _buffScroll;

	/// <summary>Raised by a click on a source of the Buffs tab: its key (<see cref="BuffEntryView.Id"/>).</summary>
	public event EventHandler<string>? BuffClicked;

	/// <summary>Raised when another scroll is picked: its record, null for none.</summary>
	public event EventHandler<RecordId?>? ScrollPicked;

	/// <summary>The places of the sections' titles and of their sources, in game pixels from the top of the list.</summary>
	private static List<(Rectangle Area, BuffSectionView Section, BuffEntryView? Entry)> BuffLayout(BuffTabView view)
	{
		var places = new List<(Rectangle, BuffSectionView, BuffEntryView?)>();
		int y = 0;
		foreach (var section in view.Sections)
		{
			places.Add((new Rectangle(BuffLeft, y, BuffWidth, 18), section, null));
			y += BuffTitleHeight;
			if (section.Group == BuffGroup.Gear)
			{
				foreach (var entry in section.Entries)
				{
					places.Add((new Rectangle(BuffLeft, y, BuffWidth, BuffRowHeight), section, entry));
					y += BuffRowHeight;
				}
			}
			else
			{
				for (int i = 0; i < section.Entries.Count; i++)
					places.Add((new Rectangle(BuffLeft + i % BuffPerRow * BuffStep, y + i / BuffPerRow * BuffStep, BuffIcon, BuffIcon), section, section.Entries[i]));
				y += (section.Entries.Count + BuffPerRow - 1) / BuffPerRow * BuffStep;
			}

			y += BuffSectionGap;
		}

		return places;
	}

	/// <summary>The height of the whole list, in game pixels.</summary>
	private static int BuffHeight(BuffTabView view)
		=> BuffLayout(view).Select(p => p.Area.Bottom).DefaultIfEmpty(0).Max();

	/// <summary>The most the list can scroll.</summary>
	private static int MaxBuffScroll(BuffTabView view) => Math.Max(0, BuffHeight(view) - (BuffBottom - BuffTop));

	/// <summary>The list's part of the panel, in game pixels.</summary>
	private static readonly Rectangle BuffArea = new(9, BuffTop, 240, BuffBottom - BuffTop);

	/// <summary>A place of the list on the panel, in game pixels, at the scroll position; null when out of sight.</summary>
	private Rectangle? OnPanel(Rectangle area)
	{
		var shown = new Rectangle(area.X, area.Y + BuffTop - this._buffScroll, area.Width, area.Height);
		return shown.IntersectsWith(BuffArea) ? shown : null;
	}

	/// <summary>The sources in sight with the part of them in sight on the panel, in game pixels.</summary>
	private IEnumerable<(Rectangle Area, BuffEntryView Entry)> VisibleBuffs(BuffTabView view)
		=> BuffLayout(view).Where(p => p.Entry is not null)
			.Select(p => (Area: this.OnPanel(p.Area), Entry: p.Entry!))
			.Where(p => p.Area is not null)
			.Select(p => (Rectangle.Intersect(p.Area!.Value, BuffArea), p.Entry));

	/// <summary>The source under a point of the panel, or null.</summary>
	internal BuffEntryView? HitBuff(Point location)
		=> this._tab == BuffsTab && this.View?.Invoke() is { } view
			? this.VisibleBuffs(view.Buffs).Where(p => this.ScaledRect(p.Area).Contains(location)).Select(p => p.Entry).FirstOrDefault()
			: null;

	internal void RaiseBuffClick(string id) => this.BuffClicked?.Invoke(this, id);

	/// <summary>
	/// Makes the Buffs tab's clicks change <paramref name="selection"/>: a source toggles, a scroll is picked; then
	/// <paramref name="changed"/> redraws.
	/// </summary>
	/// <param name="builder">The builder of the panel's view, which knows the sources.</param>
	/// <param name="selection">The toggles of the character shown.</param>
	public void BindBuffs(Func<CharacterPanelViewBuilder> builder, Func<Application.Character.BuffSelection> selection, Action changed)
	{
		this.BuffClicked += (_, id) =>
		{
			if (builder().ToggleBuff(selection(), id))
				changed();
		};
		this.ScrollPicked += (_, scroll) =>
		{
			selection().PickScroll(scroll);
			changed();
		};
	}

	/// <summary>Scrolls the list by game pixels, within its height.</summary>
	internal void ScrollBuffs(int delta)
	{
		if (this.View?.Invoke() is not { } view)
			return;

		var scroll = Math.Clamp(this._buffScroll + delta, 0, MaxBuffScroll(view.Buffs));
		if (scroll == this._buffScroll)
			return;

		this._buffScroll = scroll;
		this.Invalidate();
	}

	/// <summary>
	/// The wheel scrolls the list a row per notch (a message can carry several notches); it is handled here, so the main
	/// window does not also scroll its vault list.
	/// </summary>
	protected override void OnMouseWheel(MouseEventArgs e)
	{
		base.OnMouseWheel(e);
		if (this._tab != BuffsTab || e.Delta == 0)
			return;

		var rows = e.Delta / 120 is var notches and not 0 ? notches : Math.Sign(e.Delta);
		this.ScrollBuffs(-rows * BuffStep);
		if (e is HandledMouseEventArgs handled)
			handled.Handled = true;

		// Another source lies under the cursor now: its tool tip, not the one of the source scrolled away.
		this.OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, e.X, e.Y, 0));
	}

	/// <summary>The scroll choice: a drop-down over the panel, shown on the Buffs tab only.</summary>
	private GameDropDown ScrollBox()
	{
		if (this._scrollBox is not null)
			return this._scrollBox;

		this._scrollBox = new GameDropDown(this._uiService, this._fontService) { Name = "BuffScrollComboBox", MaxDropDownItems = 16, Visible = false };
		// Only the user's choice: the selection the panel sets itself raises nothing.
		this._scrollBox.ItemPicked += (_, index) => this.RaiseScrollPicked(index);
		this.Controls.Add(this._scrollBox);
		return this._scrollBox;
	}

	/// <summary>Raises <see cref="ScrollPicked"/> for the scroll at <paramref name="index"/> of the view's choices.</summary>
	private void RaiseScrollPicked(int index)
	{
		if (!this._syncingScrollBox && this.View?.Invoke() is { } view && index >= 0 && index < view.Buffs.Scrolls.Count)
			this.ScrollPicked?.Invoke(this, view.Buffs.Scrolls[index].Id);
	}

	/// <summary>
	/// Shows the scroll choice on the Buffs tab with the scrolls of the view, at its place for the scale. Left alone while
	/// its list is open: setting its selection then would undo what the user is choosing.
	/// </summary>
	private void SyncScrollBox(CharacterPanelView? view)
	{
		if (this._tab != BuffsTab || view is null || view.Buffs.Scrolls.Count == 0)
		{
			if (this._scrollBox is not null)
				this._scrollBox.Visible = false;
			return;
		}

		var box = this.ScrollBox();
		if (box.DroppedDown)
			return;

		this._syncingScrollBox = true;
		try
		{
			box.SetItems([.. view.Buffs.Scrolls.Select(s => s.Name)], view.Buffs.SelectedScroll);

			// The panel's text font at the panel's scale; the box keeps its own copy (WinForms rescales a control's font).
			if (this.TextFont is { } font && (this._scrollBoxFont is null || Math.Abs(this._scrollBoxFont.Size - font.Size) > 0.01F || this._scrollBoxFont.FontFamily.Name != font.FontFamily.Name))
			{
				var old = this._scrollBoxFont;
				this._scrollBoxFont = new Font(font.FontFamily, font.Size, font.Style, font.Unit);
				box.TextFont = this._scrollBoxFont;
				old?.Dispose();
			}

			// The open list is as wide as its longest name: a menu sizes itself.
			box.Bounds = this.ScaledRect(ScrollBoxArea);
			box.BarScale = this.PanelScale;
			box.Visible = true;
		}
		finally
		{
			this._syncingScrollBox = false;
		}
	}

	private void PaintBuffs(Graphics g, CharacterPanelView view)
	{
		var buffs = view.Buffs;
		this.DrawText(g, buffs.ScrollTitle, this.TitleFont, Color.White, new Rectangle(15, 29, 75, 18), StringAlignment.Near);
		this.Bar(g, 48);

		this._buffScroll = Math.Clamp(this._buffScroll, 0, MaxBuffScroll(buffs));
		var state = g.Save();
		g.SetClip(this.ScaledRect(BuffArea));
		foreach (var (area, section, entry) in BuffLayout(buffs))
		{
			if (this.OnPanel(area) is not { } at)
				continue;

			if (entry is null)
				this.DrawText(g, section.Title, this.TitleFont, Color.White, at, StringAlignment.Near);
			else if (entry.Icon is null)
				this.DrawText(g, entry.Name, this.TextFont, entry.On ? Color.White : BuffOffText, at, StringAlignment.Near);
			else
				this.BuffIconAt(g, entry, at);
		}

		g.Restore(state);

		// A thin bar on the right shows where the list is when it is taller than the tab.
		var height = BuffHeight(buffs);
		var visible = BuffBottom - BuffTop;
		if (height > visible)
		{
			int thumb = Math.Max(12, visible * visible / height);
			int top = BuffTop + (visible - thumb) * this._buffScroll / Math.Max(1, height - visible);
			using var brush = new SolidBrush(Color.FromArgb(150, 200, 170, 90));
			g.FillRectangle(brush, this.ScaledRect(new Rectangle(244, top, 3, thumb)));
		}
	}

	/// <summary>An icon at its place, in colour when on, grey and dark when off.</summary>
	private void BuffIconAt(Graphics g, BuffEntryView entry, Rectangle at)
	{
		var icon = this.Picture(entry.Icon!);
		var target = this.ScaledRect(at);
		if (entry.On)
		{
			g.DrawImage(icon, target);
			return;
		}

		using var attributes = new ImageAttributes();
		attributes.SetColorMatrix(new ColorMatrix(
		[
			[0.12F, 0.12F, 0.12F, 0, 0],
			[0.23F, 0.23F, 0.23F, 0, 0],
			[0.05F, 0.05F, 0.05F, 0, 0],
			[0, 0, 0, 1, 0],
			[0, 0, 0, 0, 1],
		]));
		g.DrawImage(icon, target, 0, 0, icon.Width, icon.Height, GraphicsUnit.Pixel, attributes);
	}

	/// <summary>The rollovers of the Buffs tab: the sources in sight.</summary>
	private IEnumerable<(Rectangle Area, StatToolTip ToolTip)> BuffToolTipAreas(CharacterPanelView view)
		=> this.VisibleBuffs(view.Buffs).Select(p => (p.Area, p.Entry.ToolTip));
}
