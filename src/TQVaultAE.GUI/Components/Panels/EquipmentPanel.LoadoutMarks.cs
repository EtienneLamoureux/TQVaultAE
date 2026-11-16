using System.Drawing.Drawing2D;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// The loadout marks: beside each equipment slot, a small game button tells how the worn piece compares with the same
/// slot of the loadout chosen in the loadout bar (<see cref="LoadoutComparison"/>): blue identical, red different, grey
/// when one side is empty. A click on a blue or red mark opens the item comparison of the two pieces.
/// </summary>
public partial class EquipmentPanel
{
	/// <summary>The game's difficulty buttons used as marks: normal (blue), legendary (red), and the unpressed one (grey).</summary>
	private const string SameMark = @"MENU\DIFFICULTYNORMALDOWN01.TEX";
	private const string DifferentMark = @"MENU\DIFFICULTYLEGENDARYDOWN01.TEX";
	private const string OneEmptyMark = @"MENU\DIFFICULTYNORMALUP01.TEX";

	// The marks' colours, for the first line of their tool tip.
	private static readonly Color SameColor = Color.FromArgb(120, 175, 255);
	private static readonly Color DifferentColor = Color.FromArgb(235, 95, 75);
	private static readonly Color OneEmptyColor = Color.FromArgb(185, 185, 185);
	private static readonly Color HintColor = Color.FromArgb(235, 235, 235);

	private Loadout? _comparedLoadout;

	/// <summary>Whether the panel set the hand cursor over a mark (and so puts the default one back).</summary>
	private bool _handOverMark;

	/// <summary>The mark whose tool tip is shown, if any.</summary>
	private LoadoutSlotMark? _tipMark;

	/// <summary>
	/// The game-style tool tip the marks show their state in (the statistics panel's, set by the stash panel); no tool tip
	/// without it.
	/// </summary>
	internal TQVaultAE.GUI.Tooltips.GameTooltip? MarkTooltip { get; set; }

	/// <summary>
	/// The loadout the worn equipment is compared with, the one chosen in the loadout bar; <see langword="null"/> for no
	/// marks (no loadout chosen, a read-only character, a loadouts file that could not be read).
	/// </summary>
	public Loadout? ComparedLoadout
	{
		get => this._comparedLoadout;
		set
		{
			if (ReferenceEquals(value, this._comparedLoadout))
				return;

			this._comparedLoadout = value;
			this.Invalidate();
		}
	}

	/// <summary>
	/// The marks of the equipment shown against <see cref="ComparedLoadout"/>, slot by slot; an item held from the
	/// equipment counts as out of its slot. Empty without a loadout to compare with.
	/// </summary>
	public IReadOnlyList<LoadoutSlotMark> LoadoutMarks
	{
		get
		{
			if (this.Sack is null || this._comparedLoadout is not { } loadout)
				return [];

			var worn = this.ShownSack.Select(item => item == this.DragInfo.Item ? null : item).ToList();
			return LoadoutComparison.Compare(worn, loadout.Pieces);
		}
	}

	/// <summary>Where a mark is drawn, in the panel's pixels: its place in cells times the cell size, so it follows zoom and DPI.</summary>
	public Rectangle MarkScreenBounds(LoadoutSlotMark mark)
	{
		var origin = this.CellTopLeft(Point.Empty);
		float unit = this.UIService.ItemUnitSize;
		return Rectangle.Round(new RectangleF(
			origin.X + (mark.Bounds.X * unit),
			origin.Y + (mark.Bounds.Y * unit),
			mark.Bounds.Width * unit,
			mark.Bounds.Height * unit));
	}

	/// <summary>The mark under a point of the panel, if any.</summary>
	private LoadoutSlotMark? MarkAt(Point location)
	{
		foreach (var mark in this.LoadoutMarks)
		{
			if (this.MarkScreenBounds(mark).Contains(location))
				return mark;
		}

		return null;
	}

	/// <summary>
	/// Opens the item comparison of the worn piece of a slot (first) and the loadout's (second). Nothing happens when one
	/// side is empty: there is nothing to compare.
	/// </summary>
	/// <param name="slot">The equipment slot.</param>
	/// <returns><see langword="true"/> when the comparison opens (in a new window, after the caller returns).</returns>
	public bool ShowLoadoutComparison(int slot)
	{
		if (this._comparedLoadout is not { } loadout
			|| !this.LoadoutMarks.Any(mark => mark.Slot == slot && mark.CanCompare))
			return false;

		var worn = this.ShownSack.ToList()[slot];
		var piece = loadout.Pieces[slot]!;
		this.BeginInvoke(() => this.ShowComparison(worn, piece));
		return true;
	}

	/// <summary>Draws the marks over the panel, in the gaps beside the slots.</summary>
	private void PaintLoadoutMarks(Graphics graphics)
	{
		var marks = this.LoadoutMarks;
		if (marks.Count == 0)
			return;

		var state = graphics.Save();
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
		foreach (var mark in marks)
		{
			string resource = mark.Match switch
			{
				LoadoutSlotMatch.Same => SameMark,
				LoadoutSlotMatch.Different => DifferentMark,
				_ => OneEmptyMark,
			};

			// Shared by the UI service: never disposed here.
			var picture = this.UIService.LoadBitmap(RecordId.Create(resource));
			if (!ReferenceEquals(picture, this.UIService.DefaultBitmap))
				graphics.DrawImage(picture, this.MarkScreenBounds(mark));
		}

		graphics.Restore(state);
	}

	/// <summary>A left click on a blue or red mark opens the comparison; the other clicks go on as usual.</summary>
	private bool LoadoutMarkMouseDown(MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Left || this.DragInfo.IsActive || this.DragInfo.PairSelection.IsActive)
			return false;

		if (this.MarkAt(e.Location) is not { CanCompare: true } mark)
			return false;

		this.HideMarkTip();
		return this.ShowLoadoutComparison(mark.Slot);
	}

	/// <summary>The hand cursor over a mark that opens a comparison, and the tool tip of the mark under the pointer.</summary>
	private void LoadoutMarkMouseMove(object? sender, MouseEventArgs e)
	{
		if (this.DragInfo.PairSelection.IsActive)
			return;

		var mark = this.DragInfo.IsActive ? null : this.MarkAt(e.Location);
		this.ShowMarkTip(mark);

		bool overMark = mark is { CanCompare: true };
		if (overMark == this._handOverMark)
			return;

		this._handOverMark = overMark;
		this.Cursor = overMark ? Cursors.Hand : Cursors.Default;
	}

	/// <summary>
	/// Shows the tool tip of <paramref name="mark"/> (its state against the loadout, and "Click to compare" when a click
	/// compares), or hides it for none; kept as it is while the pointer stays on the same mark.
	/// </summary>
	private void ShowMarkTip(LoadoutSlotMark? mark)
	{
		if (mark == this._tipMark)
			return;

		this._tipMark = mark;
		if (this.MarkTooltip is not { } tooltip)
			return;

		if (mark is not { } shown || this._comparedLoadout is not { } loadout)
		{
			tooltip.Hide();
			return;
		}

		bool worn = this.ShownSack.Select(item => item == this.DragInfo.Item ? null : item).ElementAtOrDefault(shown.Slot) is not null;
		var (format, color) = shown.Match switch
		{
			LoadoutSlotMatch.Same => (Resources.LoadoutMarkSame, SameColor),
			LoadoutSlotMatch.Different => (Resources.LoadoutMarkDifferent, DifferentColor),
			_ when worn => (Resources.LoadoutMarkEmptyInLoadout, OneEmptyColor),
			_ => (Resources.LoadoutMarkNotWorn, OneEmptyColor),
		};

		var lines = new List<TQVaultAE.GUI.Tooltips.GameTooltipLine>
		{
			new(string.Format(System.Globalization.CultureInfo.CurrentCulture, format, loadout.Name), color, Title: true),
		};
		if (shown.CanCompare)
			lines.Add(new(Resources.LoadoutMarkClickToCompare, HintColor));

		tooltip.Show(this, lines);
	}

	/// <summary>The pointer left the panel, or a click acted on a mark: no tool tip.</summary>
	private void HideMarkTip()
	{
		if (this._tipMark is null)
			return;

		this._tipMark = null;
		this.MarkTooltip?.Hide();
	}
}
