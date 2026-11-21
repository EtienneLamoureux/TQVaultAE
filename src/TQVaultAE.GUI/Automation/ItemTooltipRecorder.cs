using System.Text.Json.Serialization;

namespace TQVaultAE.GUI.Automation;

/// <summary>A colored piece of a row: text and color as drawn.</summary>
internal sealed record TooltipDumpSegment(
	[property: JsonPropertyName("text")] string Text,
	[property: JsonPropertyName("color")] string Color,
	[property: JsonPropertyName("bold")] bool Bold);

/// <summary>
/// One row of an item tooltip as rendered.
/// <see cref="Kind"/>: <c>text</c>, <c>spacer</c>, <c>delimiter</c> (the gray line), <c>range</c> (text with a value on the right),
/// <c>icon</c> (a skill icon), <c>separator</c> (the line between two skills of a column),
/// <c>column</c> (what follows goes in a new panel on the right: <see cref="Text"/> says <c>relics</c> or <c>skills</c>).
/// </summary>
internal sealed record TooltipDumpRow(
	[property: JsonPropertyName("kind")] string Kind,
	[property: JsonPropertyName("text")] string Text,
	[property: JsonPropertyName("color"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Color = null,
	[property: JsonPropertyName("bold")] bool Bold = false,
	[property: JsonPropertyName("segments"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<TooltipDumpSegment>? Segments = null,
	[property: JsonPropertyName("range"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] TooltipDumpSegment? Range = null);

/// <summary>
/// Reads the rows of the item tooltip panels from their controls, in display order.
/// It only reads what the tooltip code created: it recognizes the rows made by <c>BaseTooltip.MakeRow</c>,
/// the range rows, and the skill blocks.
/// </summary>
internal sealed class ItemTooltipRecorder
{
	private readonly List<TooltipDumpRow> _rows = [];
	private readonly Queue<List<TooltipDumpRow>> _skillBlocks = new();

	internal IReadOnlyList<TooltipDumpRow> Rows => this._rows;

	/// <summary>Records the rows of a panel shown as is (the item panel, the relic column).</summary>
	internal void Panel(Control panel) => this.Read(panel, this._rows);

	/// <summary>What follows goes in a new panel on the right.</summary>
	internal void Column(string what) => this._rows.Add(new TooltipDumpRow("column", what));

	/// <summary>Records the rows of a skill block, placed later by <see cref="PlaceSkill"/>.</summary>
	internal void SkillBlock(Control panel)
	{
		var rows = new List<TooltipDumpRow>();
		this.Read(panel, rows);
		this._skillBlocks.Enqueue(rows);
	}

	/// <summary>Places the next skill block, at the top of a new column or under the previous skill.</summary>
	internal void PlaceSkill(bool newColumn)
	{
		this._rows.Add(newColumn ? new TooltipDumpRow("column", "skills") : new TooltipDumpRow("separator", string.Empty));
		if (this._skillBlocks.TryDequeue(out var rows))
			this._rows.AddRange(rows);
	}

	private void Read(Control container, List<TooltipDumpRow> rows)
	{
		foreach (Control control in container.Controls)
			this.ReadRow(control, rows);
	}

	private void ReadRow(Control control, List<TooltipDumpRow> rows)
	{
		switch (control)
		{
			case Label label when IsDelimiter(label):
				rows.Add(new TooltipDumpRow("delimiter", string.Empty));
				break;
			case Label label when IsSpacer(label):
				rows.Add(new TooltipDumpRow("spacer", string.Empty));
				break;
			case Label label:
				var segment = Segment(label);
				rows.Add(new TooltipDumpRow("text", segment.Text, segment.Color, segment.Bold));
				break;
			case PictureBox:
				rows.Add(new TooltipDumpRow("icon", string.Empty));
				break;
			case FlowLayoutPanel flow when IsMultiColorRow(flow):
				rows.Add(MultiColor("text", flow, null));
				break;
			case FlowLayoutPanel flow:
				// Skill header and title: their children are rows of their own.
				this.Read(flow, rows);
				break;
			case Panel range when range.Controls.Count == 2 && range.Controls[1] is Label right:
				var left = range.Controls[0];
				var rangeSegment = Segment(right);
				if (left is FlowLayoutPanel leftFlow && IsMultiColorRow(leftFlow))
					rows.Add(MultiColor("range", leftFlow, rangeSegment));
				else if (left is Label leftLabel)
				{
					var l = Segment(leftLabel);
					rows.Add(new TooltipDumpRow("range", l.Text, l.Color, l.Bold, Range: rangeSegment));
				}
				else
					rows.Add(new TooltipDumpRow("unknown:" + left.GetType().Name, left.Text, Range: rangeSegment));
				break;
			default:
				rows.Add(new TooltipDumpRow("unknown:" + control.GetType().Name, control.Text));
				break;
		}
	}

	/// <summary>The gray line of <c>TOOLTIPDELIM</c>.</summary>
	private static bool IsDelimiter(Label label)
		=> label.Text.Length == 0 && label.Height == 3 && label.BorderStyle == BorderStyle.FixedSingle
			&& (label.Anchor & AnchorStyles.Right) == AnchorStyles.Right;

	/// <summary>The blank line of <c>TOOLTIPSPACER</c>: a text label has no margin, the spacer keeps the default one.</summary>
	private static bool IsSpacer(Label label)
		=> label.Text == " " && label.Margin != Padding.Empty;

	/// <summary>A line with color tags in the middle: one label per colored segment, side by side.</summary>
	private static bool IsMultiColorRow(FlowLayoutPanel flow)
		=> flow.FlowDirection == FlowDirection.LeftToRight && flow.Controls.Count > 0
			&& flow.Controls.Cast<Control>().All(c => c is Label);

	private static TooltipDumpRow MultiColor(string kind, FlowLayoutPanel flow, TooltipDumpSegment? range)
	{
		var segments = flow.Controls.Cast<Label>().Select(Segment).ToList();
		return new TooltipDumpRow(kind, string.Concat(segments.Select(s => s.Text)), segments[0].Color, segments[0].Bold, segments, range);
	}

	private static TooltipDumpSegment Segment(Label label)
		=> new(label.Text, AutomationJson.Hex(label.ForeColor), label.Font.Bold);
}
