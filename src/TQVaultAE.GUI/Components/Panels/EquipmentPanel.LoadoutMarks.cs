using System.Drawing.Drawing2D;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;

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

	private Loadout? _comparedLoadout;

	/// <summary>Whether the panel set the hand cursor over a mark (and so puts the default one back).</summary>
	private bool _handOverMark;

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
	/// <returns><see langword="true"/> when the comparison opens (after the caller returns: the dialog is modal).</returns>
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
		if (e.Button != MouseButtons.Left || this.DragInfo.IsActive || this.DragInfo.Compare.IsActive)
			return false;

		return this.MarkAt(e.Location) is { CanCompare: true } mark && this.ShowLoadoutComparison(mark.Slot);
	}

	/// <summary>The hand cursor over a mark that opens a comparison.</summary>
	private void LoadoutMarkMouseMove(object? sender, MouseEventArgs e)
	{
		if (this.DragInfo.Compare.IsActive)
			return;

		bool overMark = !this.DragInfo.IsActive && this.MarkAt(e.Location) is { CanCompare: true };
		if (overMark == this._handOverMark)
			return;

		this._handOverMark = overMark;
		this.Cursor = overMark ? Cursors.Hand : Cursors.Default;
	}
}
