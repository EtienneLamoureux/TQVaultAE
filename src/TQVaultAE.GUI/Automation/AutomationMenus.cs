//-----------------------------------------------------------------------
// <copyright file="AutomationMenus.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.GUI.Components.Panels;

namespace TQVaultAE.GUI.Automation;

/// <summary>A context menu entry read by <see cref="SackPanel.AutomationContextMenu"/>.</summary>
internal sealed record AutomationMenuEntry(string Text, IReadOnlyList<AutomationMenuEntry>? Items, bool? Enabled = null, bool? Bold = null
	, bool? Checked = null, string? Hint = null);

/// <summary>Reads and clicks the context menus built by the panels.</summary>
internal static class AutomationMenus
{
	/// <summary>The entries, <c>-</c> for separators, with the entries of their sub menus.</summary>
	/// <remarks>A sub menu raises its opening event first, as when the user opens it: some fill their entries then.</remarks>
	internal static IReadOnlyList<AutomationMenuEntry> Describe(ToolStripItemCollection items)
		=> items.Cast<ToolStripItem>()
			.Where(i => i.Available)
			.Select(i => new AutomationMenuEntry(
				i is ToolStripSeparator ? "-" : i.Text ?? string.Empty
				, i is ToolStripMenuItem { HasDropDownItems: true } menu ? Describe(Opened(menu).DropDownItems) : null
				, i.Enabled ? null : false
				, i is ToolStripMenuItem { Font.Bold: true } ? true : null
				, i is ToolStripMenuItem { Checked: true } ? true : null
				, i is ToolStripMenuItem { ShowShortcutKeys: true, ShortcutKeyDisplayString: { Length: > 0 } hint } ? hint : null))
			.ToList();

	/// <summary>Raises the opening event of a sub menu (<c>DropDownOpening</c>) without showing it.</summary>
	private static ToolStripMenuItem Opened(ToolStripMenuItem menu)
	{
		typeof(ToolStripDropDownItem)
			.GetMethod("OnDropDownShow", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
			?.Invoke(menu, [EventArgs.Empty]);
		return menu;
	}

	/// <summary>
	/// Finds the entry of <paramref name="menu"/> by its path of texts (exact text first, else the first containing it)
	/// and clicks it once the calling tool has returned, through <paramref name="owner"/>'s message loop.
	/// </summary>
	/// <returns>The text of the entry to be clicked.</returns>
	internal static string ClickLater(Control owner, ToolStrip menu, IReadOnlyList<string> path)
		=> ClickLater(owner, Resolve(menu, path));

	/// <summary>
	/// Clicks the last entry of <paramref name="chain"/> (from <see cref="Resolve"/>) once the calling tool has returned,
	/// through <paramref name="owner"/>'s message loop, then closes the menu if the click left it open.
	/// </summary>
	/// <returns>The text of the entry to be clicked.</returns>
	internal static string ClickLater(Control owner, IReadOnlyList<ToolStripItem> chain)
	{
		var clicked = chain[^1];
		var menu = chain[0].Owner as ToolStripDropDown;
		owner.BeginInvoke(() =>
		{
			clicked.PerformClick();
			if (menu is { IsDisposed: false, Visible: true })
				menu.Close();
		});
		return clicked.Text ?? string.Empty;
	}

	/// <summary>
	/// Finds the entries along <paramref name="path"/>, a path of texts (exact text first, else the first containing it),
	/// among the entries a user sees (hidden ones are skipped); the sub menus raise their opening event on the way, as
	/// when the user opens them. The last one must be enabled and not open a sub menu.
	/// </summary>
	/// <returns>The entries, from the top level to the one to click.</returns>
	internal static IReadOnlyList<ToolStripItem> Resolve(ToolStrip menu, IReadOnlyList<string> path)
	{
		var chain = new List<ToolStripItem>();
		IEnumerable<ToolStripItem> items = menu.Items.Cast<ToolStripItem>().Where(i => i.Available).ToList();
		foreach (var text in path)
		{
			var entry = items.FirstOrDefault(i => string.Equals(i.Text, text, StringComparison.OrdinalIgnoreCase))
				?? items.FirstOrDefault(i => i.Text?.Contains(text, StringComparison.OrdinalIgnoreCase) == true)
				?? throw new AutomationRefusedException($"No entry '{text}'. Entries: {string.Join(" | ", items.Select(i => i.Text))}");
			if (chain.Count < path.Count - 1 && !entry.Enabled)
				throw new AutomationRefusedException($"'{entry.Text}' is disabled: a user could not open it.");
			chain.Add(entry);
			items = entry is ToolStripMenuItem { HasDropDownItems: true } sub
				? Opened(sub).DropDownItems.Cast<ToolStripItem>().Where(i => i.Available).ToList()
				: [];
		}

		return Clickable(chain);
	}

	/// <summary>
	/// Checks that a user could click the last entry of <paramref name="chain"/>: every entry shown and the ones above
	/// enabled, the last one enabled and not opening a sub menu.
	/// </summary>
	/// <returns><paramref name="chain"/>.</returns>
	internal static IReadOnlyList<ToolStripItem> Clickable(IReadOnlyList<ToolStripItem> chain)
	{
		if (chain.Count == 0)
			throw new AutomationRefusedException("Empty menu path.");
		if (chain.FirstOrDefault(i => !i.Available) is { } hidden)
			throw new AutomationRefusedException($"'{hidden.Text}' is hidden: a user could not click it.");
		var last = chain[^1];
		if (!last.Enabled)
			throw new AutomationRefusedException($"'{last.Text}' is disabled: a user could not click it.");
		if (last is ToolStripMenuItem { HasDropDownItems: true } parent)
			throw new AutomationRefusedException($"'{last.Text}' opens a sub menu: add one of its entries to the path ({string.Join(" | ", parent.DropDownItems.Cast<ToolStripItem>().Where(i => i.Available).Select(i => i.Text))}).");
		return chain;
	}

	/// <summary>
	/// Draws a menu and the sub menus opened along <paramref name="open"/>, as the user sees them: each drop down is
	/// shown, drawn, and the menu closed at the end. The sub menus are drawn to the right, level with their entry, which
	/// is drawn selected.
	/// </summary>
	internal static Bitmap Render(ContextMenuStrip menu, IReadOnlyList<string> open)
	{
		var parts = new List<(Bitmap Image, int Y)>();
		try
		{
			if (!menu.Visible)
				menu.Show(Point.Empty);
			parts.Add((AutomationImages.Render(menu), 0));

			ToolStripDropDown current = menu;
			int top = 0;
			foreach (var text in open)
			{
				var items = current.Items.Cast<ToolStripItem>().ToList();
				var entry = items.FirstOrDefault(i => string.Equals(i.Text, text, StringComparison.OrdinalIgnoreCase))
					?? items.FirstOrDefault(i => i.Text?.Contains(text, StringComparison.OrdinalIgnoreCase) == true);
				if (entry is not ToolStripMenuItem { HasDropDownItems: true } parent)
					throw new AutomationRefusedException($"No sub menu '{text}'. Entries: {string.Join(" | ", items.Select(i => i.Text))}");

				// The entry whose sub menu is open is drawn selected, as under the user's mouse.
				parent.Select();
				var (drawn, y) = parts[^1];
				parts[^1] = (AutomationImages.Render(current), y);
				drawn.Dispose();

				top += parent.Bounds.Y;
				parent.ShowDropDown();
				parts.Add((AutomationImages.Render(parent.DropDown), top));
				current = parent.DropDown;
			}

			int width = parts.Sum(p => p.Image.Width);
			int height = parts.Max(p => p.Y + p.Image.Height);
			var result = new Bitmap(width, height);
			using var g = Graphics.FromImage(result);
			g.Clear(Color.Transparent);
			int x = 0;
			foreach (var (image, y) in parts)
			{
				g.DrawImage(image, x, y);
				x += image.Width;
			}

			return result;
		}
		finally
		{
			menu.Close();
			foreach (var (image, _) in parts)
				image.Dispose();
		}
	}
}
