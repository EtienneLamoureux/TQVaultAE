using System.Collections;
using TQVaultAE.GUI.Infrastructure;

namespace TQVaultAE.GUI.Components.Scaling;

/// <summary>
/// A control whose <c>ScaleControl</c> grows its font by the zoom factor (<see cref="ZoomScope.ScaleFont"/>): a zoom
/// gives it back its font of the <see cref="ZoomSnapshot"/> before scaling, so the font is grown once from it.
/// </summary>
internal interface IZoomedFont
{
	/// <summary>Gets or sets the font the zoom grows.</summary>
	Font ZoomedFont { get; set; }
}

/// <summary>
/// A control that sizes and places the controls it holds itself, from the scale: the <see cref="ZoomSnapshot"/> keeps
/// the control, none of the controls it holds.
/// </summary>
internal interface ILaysOutChildren
{
}

/// <summary>
/// A control that fits a part of itself to its content or to the room it is given, in code: the
/// <see cref="ZoomSnapshot"/> and each zoom start from its whole layout, and it fits again once the zoom is done.
/// </summary>
internal interface IZoomFit
{
	/// <summary>Gives back what the fit took, before the layout is kept or given back.</summary>
	void UndoFit();

	/// <summary>Fits again, once the window is scaled and laid out.</summary>
	void Fit();
}

/// <summary>
/// The window's layout at zoom 1, kept once built: bounds, margins, paddings, minimum and maximum sizes, the absolute
/// rows and columns of tables and the fonts the zoom grows, of every control of the window. A zoom gives them back
/// before the window is scaled once by the zoom from them: <see cref="Control.Scale(SizeF)"/> scales the values it is
/// given, rounded, so scaling from the sizes of the zoom before piled up the roundings of every step (margins, rows and
/// buttons drifted, fonts 12 -> 11.98 pt) and a reset did not give back the zoom 1 layout.
/// </summary>
/// <remarks>
/// After a move to a monitor of another DPI the values stay those of the DPI they were kept at: the scale applied after
/// a restore includes the ratio of the DPIs. A control made after the snapshot is kept at the next zoom, its values brought back to the snapshot's scale.
/// </remarks>
internal sealed class ZoomSnapshot
{
	private sealed record Entry(
		Control Control,
		Control? Parent,
		Rectangle Bounds,
		Padding Margin,
		Padding Padding,
		Size MinimumSize,
		Size MaximumSize,
		Font? Font,
		TableSizes? Table);

	/// <summary>
	/// The sizes of a table's rows and columns, and the laid out size of its last row and column: Windows Forms scales a
	/// last absolute row or column from that size (it may have been stretched to fill the table), not from its style.
	/// </summary>
	private sealed record TableSizes(float[] Columns, float[] Rows, int LastColumn, int LastColumnWidth, int LastRow, int LastRowHeight);

	private readonly Form root;
	private readonly Size clientSize;
	private readonly Padding rootPadding;
	private readonly List<Entry> entries = [];
	private readonly HashSet<Control> known = [];

	private ZoomSnapshot(Form root, float scale)
	{
		this.root = root;
		this.Scale = scale;
		this.clientSize = root.ClientSize;
		this.rootPadding = root.Padding;
	}

	/// <summary>Gets the scale (zoom times DPI scale) the values were kept at.</summary>
	public float Scale { get; }

	/// <summary>Keeps the layout of <paramref name="root"/> and its controls, at <paramref name="scale"/>.</summary>
	public static ZoomSnapshot Take(Form root, float scale)
	{
		var snapshot = new ZoomSnapshot(root, scale);
		var containers = Containers(root).ToList();
		foreach (var container in containers)
			container.SuspendLayout();

		UndoFits(root);
		snapshot.Keep(root.Controls);

		for (int i = containers.Count - 1; i >= 0; i--)
			containers[i].ResumeLayout(true);

		return snapshot;
	}

	/// <summary>
	/// Zooms the window from <paramref name="currentScale"/> to <paramref name="newScale"/>: its layout suspended, the
	/// window and its controls get their kept values back, then <paramref name="scaleWindow"/> scales the window once by
	/// the factor from <see cref="Scale"/> it is given, the tables' absolute rows and columns get the sizes that scaling
	/// gives from the kept layout, the layout resumes and the controls that fit themselves fit again. The controls made
	/// after the snapshot (made at <paramref name="currentScale"/>) are kept first, their values brought back to
	/// <see cref="Scale"/>: rounded once there, then zoomed from those values as the others.
	/// </summary>
	public void Zoom(float currentScale, float newScale, Action<float> scaleWindow)
	{
		this.entries.RemoveAll(entry => entry.Control.IsDisposed);
		this.known.RemoveWhere(control => control.IsDisposed);
		foreach (var control in this.MadeAfter(this.root.Controls).ToList())
			this.Keep(new[] { control }, this.Scale / currentScale);

		var containers = Containers(this.root).Where(c => c == this.root || this.known.Contains(c)).ToList();
		foreach (var container in containers)
			container.SuspendLayout();

		UndoFits(this.root);
		foreach (var entry in this.entries)
		{
			if (!entry.Control.IsDisposed && entry.Control.Parent == entry.Parent)
				Apply(entry);
		}

		if (this.root.Padding != this.rootPadding)
			this.root.Padding = this.rootPadding;

		float factor = newScale / this.Scale;
		scaleWindow(factor);

		foreach (var entry in this.entries)
		{
			if (entry.Table is { } sizes && entry.Control is TableLayoutPanel table && !table.IsDisposed)
				ScaleAbsoluteSizes(table, sizes, factor);
		}

		for (int i = containers.Count - 1; i >= 0; i--)
			containers[i].ResumeLayout(true);

		Fits(this.root);
	}

	/// <summary>Keeps <paramref name="controls"/> and the controls they hold, their values times <paramref name="factor"/>.</summary>
	private void Keep(IEnumerable controls, float factor = 1.0F)
	{
		int At(float value) => (int)Math.Round(value * factor);
		Size SizeAt(Size size) => new(At(size.Width), At(size.Height));

		foreach (Control control in controls)
		{
			Font? font = null;
			if (control is IZoomedFont zoomed)
			{
				var current = zoomed.ZoomedFont;
				font = factor == 1.0F
					? (Font)current.Clone()
					: new Font(current.FontFamily, current.Size * factor, current.Style, current.Unit, current.GdiCharSet, current.GdiVerticalFont);
			}

			this.entries.Add(new Entry(
				control,
				control.Parent,
				new Rectangle(At(control.Left), At(control.Top), At(control.Width), At(control.Height)),
				control.Margin.Scaled(factor),
				control.Padding.Scaled(factor),
				SizeAt(control.MinimumSize),
				SizeAt(control.MaximumSize),
				font,
				control is TableLayoutPanel table ? Sizes(table, factor) : null));
			this.known.Add(control);

			if (control is not ILaysOutChildren)
				this.Keep(control.Controls, factor);
		}
	}

	private static TableSizes Sizes(TableLayoutPanel table, float factor)
	{
		var widths = table.GetColumnWidths();
		var heights = table.GetRowHeights();
		return new TableSizes(
			[.. table.ColumnStyles.Cast<ColumnStyle>().Select(s => s.Width * factor)],
			[.. table.RowStyles.Cast<RowStyle>().Select(s => s.Height * factor)],
			widths.Length - 1,
			widths.Length > 0 ? (int)Math.Round(widths[^1] * factor) : 0,
			heights.Length - 1,
			heights.Length > 0 ? (int)Math.Round(heights[^1] * factor) : 0);
	}

	/// <summary>
	/// The absolute rows and columns of <paramref name="table"/> at the kept sizes times <paramref name="factor"/>, as
	/// Windows Forms' scaling sets them, but from the kept layout instead of the layout of the zoom before.
	/// </summary>
	private static void ScaleAbsoluteSizes(TableLayoutPanel table, TableSizes sizes, float factor)
	{
		if (table.ColumnStyles.Count == sizes.Columns.Length)
		{
			for (int i = 0; i < sizes.Columns.Length; i++)
			{
				var style = table.ColumnStyles[i];
				if (style.SizeType == SizeType.Absolute)
					style.Width = MathF.Round((i == sizes.LastColumn && sizes.LastColumnWidth > 0 ? sizes.LastColumnWidth : sizes.Columns[i]) * factor);
			}
		}

		if (table.RowStyles.Count == sizes.Rows.Length)
		{
			for (int i = 0; i < sizes.Rows.Length; i++)
			{
				var style = table.RowStyles[i];
				if (style.SizeType == SizeType.Absolute)
					style.Height = MathF.Round((i == sizes.LastRow && sizes.LastRowHeight > 0 ? sizes.LastRowHeight : sizes.Rows[i]) * factor);
			}
		}
	}

	/// <summary>The window's client size at the snapshot, its size at zoom 1 before it grew to fit its content.</summary>
	public Size ClientSize => this.clientSize;

	private static void Apply(Entry entry)
	{
		var control = entry.Control;
		bool limits = control.MinimumSize != entry.MinimumSize || control.MaximumSize != entry.MaximumSize;
		if (limits)
		{
			control.MinimumSize = Size.Empty;
			control.MaximumSize = Size.Empty;
		}

		if (control.Margin != entry.Margin)
			control.Margin = entry.Margin;
		if (control.Padding != entry.Padding)
			control.Padding = entry.Padding;

		if (control is TableLayoutPanel table && entry.Table is { } sizes)
		{
			RestoreSizes(table.ColumnStyles.Cast<TableLayoutStyle>().ToList(), sizes.Columns);
			RestoreSizes(table.RowStyles.Cast<TableLayoutStyle>().ToList(), sizes.Rows);
		}

		if (entry.Font is { } font && control is IZoomedFont zoomed && !SameFont(zoomed.ZoomedFont, font))
			zoomed.ZoomedFont = new Font(font.FontFamily, font.Size, font.Style, font.Unit, font.GdiCharSet, font.GdiVerticalFont);

		// Auto-sized controls too: one that only grows (GrowOnly) kept the size of the largest zoom and squeezed its
		// neighbours; one that grows and shrinks takes the size of its content again at the next layout.
		if (control.Bounds != entry.Bounds)
			control.Bounds = entry.Bounds;

		if (limits)
		{
			control.MinimumSize = entry.MinimumSize;
			control.MaximumSize = entry.MaximumSize;
		}
	}

	/// <summary>The kept absolute sizes of a table's rows or columns, when it still has as many.</summary>
	private static void RestoreSizes(List<TableLayoutStyle> styles, float[] sizes)
	{
		if (styles.Count != sizes.Length)
			return;

		for (int i = 0; i < styles.Count; i++)
		{
			switch (styles[i])
			{
				case ColumnStyle column when column.Width != sizes[i]:
					column.Width = sizes[i];
					break;
				case RowStyle row when row.Height != sizes[i]:
					row.Height = sizes[i];
					break;
			}
		}
	}

	private static bool SameFont(Font a, Font b)
		=> a.Size == b.Size && a.Style == b.Style && a.Unit == b.Unit && a.FontFamily.Name == b.FontFamily.Name;

	/// <summary>The controls made after the snapshot, the outermost ones only.</summary>
	private IEnumerable<Control> MadeAfter(Control.ControlCollection controls)
	{
		foreach (Control control in controls)
		{
			if (!this.known.Contains(control))
				yield return control;
			else if (control is not ILaysOutChildren)
			{
				foreach (var inner in this.MadeAfter(control.Controls))
					yield return inner;
			}
		}
	}

	/// <summary>The control and the controls holding others under it, outer first, not inside an <see cref="ILaysOutChildren"/>.</summary>
	private static IEnumerable<Control> Containers(Control control)
	{
		if (control.Controls.Count == 0)
			yield break;

		yield return control;
		if (control is ILaysOutChildren)
			yield break;

		foreach (Control inner in control.Controls)
		{
			foreach (var container in Containers(inner))
				yield return container;
		}
	}

	private static void UndoFits(Control control)
	{
		foreach (Control inner in control.Controls)
		{
			if (inner is IZoomFit fit)
				fit.UndoFit();
			if (inner is not ILaysOutChildren)
				UndoFits(inner);
		}
	}

	private static void Fits(Control control)
	{
		foreach (Control inner in control.Controls)
		{
			if (inner is IZoomFit fit)
				fit.Fit();
			if (inner is not ILaysOutChildren)
				Fits(inner);
		}
	}
}
