namespace TQVaultAE.GUI.Components.Rendering;

/// <summary>
/// The look of the entries added in code to a menu built in code.
/// </summary>
internal static class MenuStyle
{
	/// <summary>
	/// Gives <paramref name="item"/> the background, font and text color of <paramref name="owner"/>, the menu it goes in.
	/// The font is set on the entry itself, not inherited: <see cref="Scaling.FontRebaser"/> walks the entries to rebase it.
	/// </summary>
	/// <returns><paramref name="item"/>, for use in an expression.</returns>
	public static T StyledLike<T>(this T item, ToolStrip owner)
		where T : ToolStripItem
	{
		item.BackColor = owner.BackColor;
		item.Font = owner.Font;
		item.ForeColor = owner.ForeColor;
		return item;
	}
}
