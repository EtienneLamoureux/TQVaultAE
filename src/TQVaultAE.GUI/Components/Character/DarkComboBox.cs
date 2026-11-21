using System.Drawing;
using System.Windows.Forms;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>
/// The drop-down lists drawn on the character panels' black: a dark grey box with white text, the open list with a grey
/// highlight. A drop-down list ignores its colours under the visual styles (a white box): it draws its items itself.
/// </summary>
internal static class DarkComboBox
{
	/// <summary>The box's background, the panels' dark grey.</summary>
	public static readonly Color Back = Color.FromArgb(24, 24, 24);

	/// <summary>Gives a drop-down list the panels' look.</summary>
	public static void Apply(ComboBox combo)
	{
		ArgumentNullException.ThrowIfNull(combo);
		combo.BackColor = Back;
		combo.ForeColor = Color.White;
		combo.FlatStyle = FlatStyle.Flat;
		combo.DrawMode = DrawMode.OwnerDrawFixed;
		combo.DrawItem += DrawItem;
	}

	/// <summary>
	/// A drop-down list as high as its row: its height follows its item height. The box is its item's height plus its
	/// border (3 pixels above and under); 2 pixels over the row keep the text whole.
	/// </summary>
	public static void FitHeight(ComboBox combo, int height)
	{
		ArgumentNullException.ThrowIfNull(combo);
		combo.ItemHeight = Math.Max(1, height - 4);
	}

	private static void DrawItem(object? sender, DrawItemEventArgs e)
	{
		if (sender is not ComboBox combo)
			return;

		// The list highlights the entry under the pointer; the box itself keeps the panel's background.
		var highlighted = (e.State & DrawItemState.Selected) != 0 && (e.State & DrawItemState.ComboBoxEdit) == 0;
		using (var back = new SolidBrush(highlighted ? Color.FromArgb(70, 70, 70) : combo.BackColor))
			e.Graphics.FillRectangle(back, e.Bounds);

		if (e.Index >= 0 && e.Index < combo.Items.Count)
		{
			TextRenderer.DrawText(e.Graphics, combo.GetItemText(combo.Items[e.Index]), e.Font ?? combo.Font, e.Bounds,
				combo.Enabled ? combo.ForeColor : Color.Gray,
				TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
		}
	}
}
