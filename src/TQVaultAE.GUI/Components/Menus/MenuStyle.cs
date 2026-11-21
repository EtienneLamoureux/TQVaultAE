using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Menus;

/// <summary>
/// The look of the entries added in code to a menu built in code.
/// </summary>
internal static class MenuStyle
{
	/// <summary>The font of the menus: the game font at 9 points, grown by <paramref name="fontZoom"/>.</summary>
	public static Font MenuFont(IFontService fonts, float fontZoom) => fonts.GetFont(9.0F * fontZoom);

	/// <summary>
	/// Gives a menu the TQVault look of every menu: the renderer and colours of <see cref="CustomProfessionalRenderer"/>
	/// (the selected entry in black) and the menu font at the zoom.
	/// </summary>
	public static void Apply(ToolStrip menu, IFontService fonts, float fontZoom)
	{
		CustomProfessionalRenderer.Apply(menu);
		menu.Font = MenuFont(fonts, fontZoom);
	}

	/// <summary>
	/// Fills <paramref name="menu"/> with <paramref name="fill"/> each time it opens. A menu empty when asked to open gets
	/// its opening already cancelled: without undoing that, a menu filled only here opened on the second click.
	/// </summary>
	public static void FillOnOpening(this ContextMenuStrip menu, Action fill)
		=> menu.Opening += (_, e) =>
		{
			fill();
			e.Cancel = menu.Items.Count == 0;
		};

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

	/// <summary>The entries drawn in their own text color instead of the menu's.</summary>
	private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<ToolStripItem, object> OwnTextColor = [];

	/// <summary>
	/// Draws <paramref name="item"/> in <paramref name="color"/> while it is not selected (the selected entry stays black),
	/// for example the affixes of a mod in the mod color of the item tool tip.
	/// </summary>
	/// <returns><paramref name="item"/>, for use in an expression.</returns>
	public static T WithTextColor<T>(this T item, Color color)
		where T : ToolStripItem
	{
		item.ForeColor = color;
		OwnTextColor.AddOrUpdate(item, color);
		return item;
	}

	/// <summary>True when <paramref name="item"/> is drawn in its own text color (<see cref="WithTextColor"/>).</summary>
	public static bool HasOwnTextColor(ToolStripItem item) => OwnTextColor.TryGetValue(item, out _);

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
