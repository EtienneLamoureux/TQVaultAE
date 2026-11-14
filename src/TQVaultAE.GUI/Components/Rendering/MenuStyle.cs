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

	/// <summary>
	/// An export sub menu for <paramref name="owner"/>, with its clipboard, file and PasteBin entries. Without a PasteBin
	/// API key the PasteBin entry is disabled, or left out with <paramref name="hidePasteBinWithoutKey"/>.
	/// </summary>
	public static ToolStripMenuItem ExportSubMenu(
		ToolStrip owner,
		string text,
		(string Text, EventHandler OnClick) clipboard,
		(string Text, EventHandler OnClick) file,
		(string Text, EventHandler OnClick) pasteBin,
		bool hasPasteBinKey,
		bool hidePasteBinWithoutKey = false)
	{
		var menu = new ToolStripMenuItem(text)
		{
			DisplayStyle = ToolStripItemDisplayStyle.Text,
		}.StyledLike(owner);

		menu.DropDownItems.Add(new ToolStripMenuItem(clipboard.Text, null, clipboard.OnClick).StyledLike(owner));
		menu.DropDownItems.Add(new ToolStripMenuItem(file.Text, null, file.OnClick).StyledLike(owner));
		if (hasPasteBinKey || !hidePasteBinWithoutKey)
		{
			menu.DropDownItems.Add(new ToolStripMenuItem(pasteBin.Text, null, pasteBin.OnClick)
			{
				Enabled = hasPasteBinKey,
			}.StyledLike(owner));
		}

		return menu;
	}
}
