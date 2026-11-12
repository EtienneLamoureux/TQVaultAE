#if DEBUG
namespace TQVaultAE.GUI.Dialogs;

using System.Drawing;
using System.Linq;
using System.Windows.Forms;

/// <summary>
/// Debug-only access to what the dialog shows, for the <c>item_properties_dump</c> automation tool.
/// </summary>
internal sealed partial class ItemProperties
{
	/// <summary>A row of a property grid as shown: its text and colors.</summary>
	internal sealed record AutomationRow(string Text, string Color, string SelectionColor);

	/// <summary>A property section as shown: its title (its group's caption), whether it is visible, and its rows.</summary>
	internal sealed record AutomationSection(string Label, bool Visible, AutomationRow[] Rows);

	/// <summary>What the dialog shows for one item.</summary>
	internal sealed record AutomationContent(string Name, string NameColor, AutomationSection[] Sections);

	/// <summary>
	/// Loads <paramref name="item"/> with the "filter extra info" box set to <paramref name="filterExtra"/>, as the
	/// dialog does when it opens or the box is clicked, and reads back what it shows. The dialog must be shown, so the
	/// visibility of its sections is read right.
	/// </summary>
	internal AutomationContent AutomationLoad(Domain.Entities.Item item, bool filterExtra)
	{
		this.Item = item;
		this.checkBoxFilterExtraInfo.CheckedChanged -= this.CheckBox1_CheckedChanged;
		this.checkBoxFilterExtraInfo.Checked = filterExtra;
		this.checkBoxFilterExtraInfo.CheckedChanged += this.CheckBox1_CheckedChanged;

		// A fresh dialog has empty grids; clear them so a hidden section does not keep the previous item's rows.
		foreach (var grid in new[] { this.dataGridViewBaseItemProperties, this.dataGridViewPrefixProperties, this.dataGridViewSuffixProperties })
			grid.Rows.Clear();

		this.LoadProperties();

		return new AutomationContent(
			this.labelItemName.Text,
			Hex(this.labelItemName.ForeColor),
			[
				Section(this.groupBoxBaseItemProperties, this.dataGridViewBaseItemProperties),
				Section(this.groupBoxPrefixProperties, this.dataGridViewPrefixProperties),
				Section(this.groupBoxSuffixProperties, this.dataGridViewSuffixProperties),
			]);
	}

	private static AutomationSection Section(GroupBox caption, DataGridView grid)
		=> new(
			caption.Text,
			caption.Visible && grid.Visible,
			grid.Rows.Cast<DataGridViewRow>()
				.Select(r => r.Cells[0])
				.Select(c => new AutomationRow(c.Value as string ?? string.Empty, Hex(c.Style.ForeColor), Hex(c.Style.SelectionForeColor)))
				.ToArray());

	private static string Hex(Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
}
#endif
