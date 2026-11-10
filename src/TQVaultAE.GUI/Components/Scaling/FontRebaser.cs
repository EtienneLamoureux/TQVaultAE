using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Scaling;

/// <summary>
/// A control keeping fonts of its own besides <see cref="Control.Font"/> (a base font to fit, fonts of a tooltip or a menu
/// it shows itself): it moves them to the new base font, <see cref="IFontService.Rebase"/>.
/// </summary>
internal interface IFontRebasing
{
	/// <summary>
	/// Moves the control's own fonts to the base font in use. <see cref="FontRebaser"/> calls it before it rebases the
	/// control's <see cref="Control.Font"/> and children.
	/// </summary>
	void RebaseFonts(IFontService fontService);
}

/// <summary>
/// Moves the fonts of a window to the base font chosen in the settings, after <see cref="IFontService.Reload"/>: every
/// control keeps its size and style, so the zoom applied since the window was built stays.
/// </summary>
internal static class FontRebaser
{
	/// <summary>Rebases <paramref name="root"/>, its children and their context menus, parents first.</summary>
	public static void Apply(Control root, IFontService fontService)
	{
		ArgumentNullException.ThrowIfNull(root);
		ArgumentNullException.ThrowIfNull(fontService);

		root.SuspendLayout();
		try
		{
			Rebase(root, fontService);
		}
		finally
		{
			root.ResumeLayout(true);
		}
	}

	private static void Rebase(Control control, IFontService fontService)
	{
		if (control is IFontRebasing rebasing)
			rebasing.RebaseFonts(fontService);

		// A child inheriting its parent's font gets the new one from the parent: nothing to set.
		SetFont(control, fontService);

		if (control.ContextMenuStrip is not null)
			RebaseMenu(control.ContextMenuStrip, fontService);

		foreach (Control child in control.Controls)
			Rebase(child, fontService);
	}

	private static void SetFont(Control control, IFontService fontService)
	{
		var font = fontService.Rebase(control.Font);
		if (!ReferenceEquals(font, control.Font))
			control.Font = font;
	}

	/// <summary>Rebases a menu and its items, also the ones in sub menus.</summary>
	public static void RebaseMenu(ToolStrip menu, IFontService fontService)
	{
		ArgumentNullException.ThrowIfNull(menu);
		ArgumentNullException.ThrowIfNull(fontService);

		SetFont(menu, fontService);
		RebaseItems(menu.Items, fontService);
	}

	private static void RebaseItems(ToolStripItemCollection items, IFontService fontService)
	{
		foreach (ToolStripItem item in items)
		{
			var font = fontService.Rebase(item.Font);
			if (!ReferenceEquals(font, item.Font))
				item.Font = font;

			if (item is ToolStripDropDownItem dropDown && dropDown.HasDropDownItems)
				RebaseItems(dropDown.DropDownItems, fontService);
		}
	}
}
