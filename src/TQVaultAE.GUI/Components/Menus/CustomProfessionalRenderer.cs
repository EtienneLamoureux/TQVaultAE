using TQVaultAE.GUI.Components.Character;
using TQVaultAE.GUI.Components.Rendering;

namespace TQVaultAE.GUI.Components.Menus;

/// <summary>
/// Class for rendering the context menu strip.
/// </summary>
internal sealed class CustomProfessionalRenderer : ToolStripProfessionalRenderer
{
	/// <summary>
	/// Gives a menu the TQVault look of the character and vault menus: the dark background, light grey entries, and the
	/// selected entry in black. The entries added later take the menu's colors.
	/// </summary>
	/// <param name="menu">The menu.</param>
	public static void Apply(ToolStrip menu)
	{
		menu.Renderer = new CustomProfessionalRenderer();
		menu.BackColor = Theme.Background;
		menu.ForeColor = Theme.MenuText;
	}

	/// <summary>
	/// Handler for rendering the contect meny strip.
	/// </summary>
	/// <param name="e">ToolStripItemTextRenderEventArgs data</param>
	protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
	{
		if (e.Item.Selected)
			e.TextColor = Color.Black;
		else
		{
			if (e.Item?.Name?.StartsWith(ComboBoxCharacter.TAGKEY, StringComparison.Ordinal) ?? false)
				e.TextColor = Color.Black;
			else if (e.Item is not null && MenuStyle.HasOwnTextColor(e.Item))
				e.TextColor = e.Item.ForeColor;
			else
				e.TextColor = Theme.MenuText;
		}

		base.OnRenderItemText(e);
	}
}
